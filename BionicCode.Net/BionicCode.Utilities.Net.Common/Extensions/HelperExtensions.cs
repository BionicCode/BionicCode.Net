﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using System.CodeDom;
  using Microsoft.CSharp;
  using System.IO;
  using System.CodeDom.Compiler;
  using Microsoft.CodeAnalysis.CSharp;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Microsoft.CodeAnalysis;
  using System.Runtime.InteropServices;
  using Microsoft.CodeAnalysis.Operations;
  using System.Xml.Linq;
  using System.Reflection.Metadata;
  using System.Globalization;
  using Microsoft.Extensions.Caching.Memory;
  using Microsoft.Extensions.Logging;

  /// <summary>
  /// A collection of extension methods for various default constraintTypes
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    private const string ParameterSeparator = ", ";
    private const char ExpressionTerminator = ';';
    private const string Indentation = "  ";

    private static readonly Type ValueTaskType = typeof(ValueTask);
    private static readonly Type TaskType = typeof(Task); 
    private static readonly Type AsyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);
    private static readonly Type ExtensionAttributeType = typeof(ExtensionAttribute); 
    private static readonly Type DelegateType = typeof(Delegate);
#if !NETSTANDARD2_0
    private static readonly Type IsReadOnlyAttributeType = typeof(IsReadOnlyAttribute);
#endif

    /// <summary>
    /// The property genericTypeParameterIdentifier of an indexer property. This genericTypeParameterIdentifier is compiler generated and equals the typeName of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
    /// </summary>
    /// <typeName>The generated property genericTypeParameterIdentifier of an indexer is <c>Item</c>.</typeName>
    /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
    public static readonly string IndexerName = "Item";

    private static readonly CSharpCodeProvider CodeProvider = new CSharpCodeProvider();
    private static readonly Dictionary<SymbolInfoDataCacheKey, SymbolInfoData> MemberInfoDataCache = new Dictionary<SymbolInfoDataCacheKey, SymbolInfoData>();
    private static readonly HashSet<string> IgnorableParameterAttributes = new HashSet<string>
    {
      nameof(InAttribute),
      nameof(OutAttribute),
#if !NETSTANDARD2_0
      nameof(IsReadOnlyAttribute),
#endif
    };

    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam genericTypeParameterIdentifier="TParam">The parameter type for the predicate.</typeparam>
    /// <param genericTypeParameterIdentifier="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref genericTypeParameterIdentifier="TParam"/>, bool></c> that returns the result of <paramref genericTypeParameterIdentifier="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => predicate.Invoke;

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    ///// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureName(this PropertyInfo propertyInfo, bool isFullyQualifiedName = false)
    //  => propertyInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    ///// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureShortName(this PropertyInfo propertyInfo, bool isFullyQualifiedName = false)
    //  => propertyInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: true);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureName(this MethodInfo methodInfo, bool isFullyQualifiedName = false)
    //  => methodInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureShortName(this MethodInfo methodInfo, bool isFullyQualifiedName = false)
    //  => methodInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: true);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="typeInfo">The <see cref="Type"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureName(this Type typeInfo, bool isFullyQualifiedName = false)
    //  => typeInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="constructorInfo">The <see cref="ConstructorInfo"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureName(this ConstructorInfo constructorInfo, bool isFullyQualifiedName = false)
    //  => constructorInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="constructorInfo">The <see cref="ConstructorInfo"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureShortName(this ConstructorInfo constructorInfo, bool isFullyQualifiedName = false)
    //  => constructorInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: true);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="fieldInfo">The <see cref="FieldInfo"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureName(this FieldInfo fieldInfo, bool isFullyQualifiedName = false)
    //  => fieldInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false);

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    ///// </summary>
    ///// <param genericTypeParameterIdentifier="fieldInfo">The <see cref="FieldInfo"/> to extend.</param>
    ///// <returns>
    ///// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    ///// </returns>
    ///// <remarks>
    ///// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    ///// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    ///// </remarks>
    //public static string ToSignatureShortName(this FieldInfo fieldInfo, bool isFullyQualifiedName = false)
    //  => fieldInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: true);

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this MemberInfo memberInfo, bool isFullyQualifiedName = false, bool isCompact = false)
      => memberInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false, isCompact);

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param genericTypeParameterIdentifier="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature genericTypeParameterIdentifier like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureShortName(this MemberInfo memberInfo, bool isFullyQualifiedName = false, bool isCompact = false)
      => memberInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: true, isCompact);

    internal static string ToSignatureNameInternal(this MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName, bool isCompact)
    {
      var type = memberInfo as Type;
      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        ?? type?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
      var propertyInfo = memberInfo as PropertyInfo;
      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
      var constructorInfo = memberInfo as ConstructorInfo;
      var fieldInfo = memberInfo as FieldInfo;
      var eventInfo = memberInfo as EventInfo;

      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

      SymbolKinds memberKind = GetKind(memberInfo);

      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

      IEnumerable<CustomAttributeData> symbolAttributes = memberInfo.GetCustomAttributesData();

#if !NETSTANDARD2_0
      if (memberKind.HasFlag(SymbolKinds.Final))
      {
        symbolAttributes = symbolAttributes.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
      }
#endif

      _ = signatureNameBuilder.AppendCustomAttributes(symbolAttributes, isAppendNewLineEnabled: true);

      AccessModifier accessModifier = memberInfo.GetAccessModifier();
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (memberKind.HasFlag(SymbolKinds.Method) && methodInfo.IsAwaitable())
        // TODO::insert 'async' method modifier


      if (!memberKind.HasFlag(SymbolKinds.Delegate)
        && !memberKind.HasFlag(SymbolKinds.Struct)
        && !memberKind.HasFlag(SymbolKinds.Field)
        && memberKind.HasFlag(SymbolKinds.Final))
      {
        _ = signatureNameBuilder
          .Append("sealed")
          .Append(' ');
      }

      if (!memberKind.HasFlag(SymbolKinds.Delegate)
        && memberKind.HasFlag(SymbolKinds.Static))
      {
        _ = signatureNameBuilder
          .Append("static")
          .Append(' ');
      }

      bool isAbstract = memberKind.HasFlag(SymbolKinds.Abstract);
      if (!memberKind.HasFlag(SymbolKinds.Delegate) && isAbstract)
      {
        _ = signatureNameBuilder
          .Append("abstract")
          .Append(' ');
      }

      if (!isAbstract
        && !memberKind.HasFlag(SymbolKinds.Delegate)
        && !memberKind.HasFlag(SymbolKinds.Class)
        && memberKind.HasFlag(SymbolKinds.Virtual))
      {
        _ = signatureNameBuilder
          .Append("virtual")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.ReadOnlyStruct) 
        || memberKind.HasFlag(SymbolKinds.ReadOnlyField))
      {
        _ = signatureNameBuilder
          .Append("readonly")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Struct))
      {
        _ = signatureNameBuilder
          .Append("struct")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Class))
      {
        _ = signatureNameBuilder
          .Append("class")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Interface))
      {
        _ = signatureNameBuilder
          .Append("interface")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Delegate))
      {
        _ = signatureNameBuilder
          .Append("delegate")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Event))
      {
        _ = signatureNameBuilder
          .Append("event")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Enum))
      {
        _ = signatureNameBuilder
          .Append("enum")
          .Append(' ');
      }

      if (!memberKind.HasFlag(SymbolKinds.Delegate)
        && !memberKind.HasFlag(SymbolKinds.Class)
        && memberKind.HasFlag(SymbolKinds.Override))
      {
        _ = signatureNameBuilder
          .Append("override")
          .Append(' ');
      }

      // Set return type
      if (memberKind.HasFlag(SymbolKinds.Method)
        || memberKind.HasFlag(SymbolKinds.Property)
        || memberKind.HasFlag(SymbolKinds.Field)
        || memberKind.HasFlag(SymbolKinds.Delegate)
        || memberKind.HasFlag(SymbolKinds.Event))
      {
        Type returnType = fieldInfo?.FieldType
          ?? methodInfo?.ReturnType
          ?? propertyGetMethodInfo?.ReturnType
          ?? eventInfo?.EventHandlerType;

        _ = signatureNameBuilder.AppendDisplayNameInternal(returnType, isFullyQualifiedName, isShortName: false)
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKinds.Member) && (!isShortName || isFullyQualifiedName))
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isShortName: false)
          .Append('.');
      }

      // Member or type name
      if (memberKind.HasFlag(SymbolKinds.IndexerProperty))
      {
        _ = signatureNameBuilder.Append("this");
      }
      else
      {
        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo, isFullyQualifiedName: isFullyQualifiedName && memberKind.HasFlag(SymbolKinds.Type), isShortName: false);
      }

      if (memberKind.HasFlag(SymbolKinds.Constructor) 
        || memberKind.HasFlag(SymbolKinds.Method) 
        || memberKind.HasFlag(SymbolKinds.Delegate))
      {
        _ = signatureNameBuilder.Append('(');

        if (memberKind.HasFlag(SymbolKinds.Method) && (methodInfo?.IsExtensionMethod() ?? false))
        {
          _ = signatureNameBuilder
            .Append("this")
            .Append(' ');
        }
      }
      else if (memberKind.HasFlag(SymbolKinds.IndexerProperty))
      {
        _ = signatureNameBuilder.Append('[');
      }

      IEnumerable<ParameterInfo> parameters = methodInfo?.GetParameters()
        ?? constructorInfo?.GetParameters()
        ?? indexerPropertyIndexParameters
        ?? Enumerable.Empty<ParameterInfo>();

      if (parameters.Any())
      {
        foreach (ParameterInfo parameter in parameters)
        {
          bool isGenericTypeDefinition = false;
          if (memberKind.HasFlag(SymbolKinds.GenericMethod))
          {
            isGenericTypeDefinition = methodInfo.IsGenericMethodDefinition;
          }
          else if (memberKind.HasFlag(SymbolKinds.GenericType))
          {
            isGenericTypeDefinition = type.IsGenericTypeDefinition;
          }

          if (isGenericTypeDefinition)
          {
            IEnumerable<CustomAttributeData> attributes = parameter.GetCustomAttributesData();
            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
          }

          if (parameter.IsRef())
          {
            _ = signatureNameBuilder.Append("ref ");
          }
          else if (parameter.IsIn)
          {
            _ = signatureNameBuilder.Append("in ");
          }
          else if (parameter.IsOut)
          {
            _ = signatureNameBuilder.Append("out ");
          }

          _ = signatureNameBuilder
            .AppendDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isShortName: false)
            .Append(' ')
            .Append(parameter.Name)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        // Remove trailing comma and whitespace
        _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
      }

      if (memberKind.HasFlag(SymbolKinds.Constructor)
        || memberKind.HasFlag(SymbolKinds.Method)
        || memberKind.HasFlag(SymbolKinds.Delegate))
      {
        _ = signatureNameBuilder.Append(')');
      }
      else if (memberKind.HasFlag(SymbolKinds.IndexerProperty))
      {
        _ = signatureNameBuilder.Append(']');
      }

      if (memberKind.HasFlag(SymbolKinds.Property))
      {
        _ = signatureNameBuilder
          .Append(' ')
          .Append('{')
          .Append(' ');

        if (propertyGetMethodInfo != null)
        {
          _ = signatureNameBuilder
            .Append("get")
            .Append(HelperExtensionsCommon.ExpressionTerminator)
            .Append(' ');
        }

        if (propertySetMethodInfo != null)
        {
          _ = signatureNameBuilder
            .Append("set")
            .Append(HelperExtensionsCommon.ExpressionTerminator)
            .Append(' ');
        }

        _ = signatureNameBuilder.Append('}');
      }

      if (memberKind.HasFlag(SymbolKinds.Class))
      {
        signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(type, isFullyQualifiedName);
      }

      if (memberKind.HasFlag(SymbolKinds.Generic))
      {
        Type[] genericTypeParameterDefinitions = Type.EmptyTypes;
        if (memberKind.HasFlag(SymbolKinds.GenericType) && type.IsGenericTypeDefinition)
        {
          genericTypeParameterDefinitions = type.GetGenericTypeDefinition().GetGenericArguments();
        }
        else if (memberKind.HasFlag(SymbolKinds.GenericMethod) && methodInfo.IsGenericMethodDefinition)
        {
          genericTypeParameterDefinitions = methodInfo.GetGenericMethodDefinition().GetGenericArguments();
        }

        _ = signatureNameBuilder.AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isCompact);
      }

      if (!memberKind.HasFlag(SymbolKinds.Class) 
        && !memberKind.HasFlag(SymbolKinds.Struct)
        && !memberKind.HasFlag(SymbolKinds.Enum))
      {
        _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
      }

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    private static StringBuilder AppendCustomAttributes(this StringBuilder nameBuilder, IEnumerable<CustomAttributeData> attributes, bool isAppendNewLineEnabled)
    {
      foreach (CustomAttributeData attribute in attributes)
      {
        bool hasAttributeArguments = false;

        if (HelperExtensionsCommon.IgnorableParameterAttributes.Contains(attribute.AttributeType.Name))
        {
          continue;
        }

        _ = nameBuilder.Append('[')
          .Append(attribute.AttributeType.Name)
          .Append('(');
        foreach (CustomAttributeTypedArgument constructorPositionalArgument in attribute.ConstructorArguments)
        {
          hasAttributeArguments = true;

          if (constructorPositionalArgument.ArgumentType == typeof(string))
          {
            _ = nameBuilder.Append('"')
              .Append((string)constructorPositionalArgument.Value)
              .Append('"')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else if (constructorPositionalArgument.ArgumentType == typeof(char))
          {
            _ = nameBuilder.Append('\'')
              .Append((char)constructorPositionalArgument.Value)
              .Append('\'')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else
          {
            _ = nameBuilder.AppendPrimitiveType(constructorPositionalArgument.Value)
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
        }

        foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
        {
          hasAttributeArguments = true;

          _ = nameBuilder.Append(constructorNamedArgument.MemberName)
            .Append(" = ");

          if (constructorNamedArgument.TypedValue.ArgumentType == typeof(string))
          {
            _ = nameBuilder.Append('"')
              .Append((string)constructorNamedArgument.TypedValue.Value)
              .Append('"')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else if (constructorNamedArgument.TypedValue.ArgumentType == typeof(char))
          {
            _ = nameBuilder.Append('\'')
              .Append((char)constructorNamedArgument.TypedValue.Value)
              .Append('\'')
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
          else
          {
            _ = nameBuilder.AppendPrimitiveType(constructorNamedArgument.TypedValue.Value)
              .Append(HelperExtensionsCommon.ParameterSeparator);
          }
        }

        if (!hasAttributeArguments)
        {
          // Remove trailing opening parenthesis
          _ = nameBuilder.Remove(nameBuilder.Length - 1, 1)
            .Append(']');
        }
        else
        {
          // Remove trailing comma and whitespace
          _ = nameBuilder.Remove(nameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
            .Append(')')
            .Append(']');
        }

        if (isAppendNewLineEnabled)
        {
          _ = nameBuilder.AppendLine();
        }
        else
        {
          // Remove trailing comma and whitespace
          _ = nameBuilder.Append(' ');
        }
      }

      return nameBuilder;
    }

    private static StringBuilder AppendPrimitiveType(this StringBuilder stringBuilder, object value)
    {
      if (!value.GetType().IsPrimitive)
      {
        throw new ArgumentException("Only primitive typeArguments allowed)");
      }

      switch (value)
      {
        case int intValue:
          return stringBuilder.Append(intValue);
        case double doubleValue:
          return stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}", doubleValue);
        default:
          throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this Type type)
    {
      TypeData entry = GetMemberInfoDataCacheEntry<TypeData>(type);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this MethodInfo method)
    {
      MethodData entry = GetMemberInfoDataCacheEntry<MethodData>(method);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this ConstructorInfo constructor)
    {
      ConstructorData entry = GetMemberInfoDataCacheEntry<ConstructorData>(constructor);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this PropertyInfo property)
    {
      PropertyData entry = GetMemberInfoDataCacheEntry<PropertyData>(property);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this EventInfo eventInfo)
    {
      EventData entry = GetMemberInfoDataCacheEntry<EventData>(eventInfo);
      return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this FieldInfo field)
    {
      FieldData entry = GetMemberInfoDataCacheEntry<FieldData>(field);
      return entry.AccessModifier;
    }

    internal static AccessModifier GetAccessModifierInternal(PropertyData propertyData) 
      => GetPropertyAccessModifier(propertyData.GetMethodData, propertyData.SetMethodData).PropertyModifier;

    internal static AccessModifier GetAccessModifierInternal(EventData eventData) 
      => GetAccessModifierInternal(eventData.AddMethodData);

    internal static AccessModifier GetAccessModifierInternal(FieldData fieldData)
    {
      FieldInfo fieldInfo = fieldData.GetFieldInfo();
      return fieldInfo.IsPublic ? AccessModifier.Public
        : fieldInfo.IsPrivate ? AccessModifier.Private
        : fieldInfo.IsAssembly ? AccessModifier.Internal
        : fieldInfo.IsFamily ? AccessModifier.Protected
        : fieldInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : fieldInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static AccessModifier GetAccessModifierInternal(MethodData methodData)
    {
      MethodInfo methodInfo = methodData.GetMethodInfo();
      return methodInfo.IsPublic ? AccessModifier.Public
        : methodInfo.IsPrivate ? AccessModifier.Private
        : methodInfo.IsAssembly ? AccessModifier.Internal
        : methodInfo.IsFamily ? AccessModifier.Protected
        : methodInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : methodInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static AccessModifier GetAccessModifierInternal(ConstructorData constructorData)
    {
      ConstructorInfo methodInfo = constructorData.GetConstructorInfo();
      return methodInfo.IsPublic ? AccessModifier.Public
        : methodInfo.IsPrivate ? AccessModifier.Private
        : methodInfo.IsAssembly ? AccessModifier.Internal
        : methodInfo.IsFamily ? AccessModifier.Protected
        : methodInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : methodInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static AccessModifier GetAccessModifierInternal(TypeData typeData)
    {
      Type typeInfo = typeData.GetType();
      return typeInfo.IsPublic ? AccessModifier.Public
        : typeInfo.IsNestedPrivate ? AccessModifier.Private
        : typeInfo.IsNestedAssembly ? AccessModifier.Internal
        : typeInfo.IsNestedFamily ? AccessModifier.Protected
        : typeInfo.IsNestedPublic ? AccessModifier.Public
        : typeInfo.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
        : typeInfo.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
        : !typeInfo.IsVisible ? AccessModifier.Internal
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
    }

    internal static (AccessModifier PropertyModifier, AccessModifier GetMethodModifier, AccessModifier SetMethodModifier) GetPropertyAccessModifier(MethodData getMethodData, MethodData setMethodData)
    {
      AccessModifier getMethodModifier = getMethodData.AccessModifier;
      AccessModifier setMethodModifier = setMethodData.AccessModifier;

      // Property accessors with the least restriction provides the access modifier for the property.
      AccessModifier propertyAccessModifier = (AccessModifier)System.Math.Min((int)getMethodModifier, (int)setMethodModifier);

      return (propertyAccessModifier, getMethodModifier, setMethodModifier);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this MemberInfo memberInfo, bool isShortName = false)
      => ToDisplayNameInternal(memberInfo, isFullyQualifiedName: false, isShortName);
    //{
    //  bool isMemberIndexerProperty = type is PropertyInfo propertyInfo && (propertyInfo.GetIndexParameters()?.Length ?? -1) > 0;
    //  StringBuilder memberNameBuilder = new StringBuilder(isMemberIndexerProperty
    //    ? $"{type.Name}[]"
    //    : type.Name);

    //  // Those member constraintTypes can't be generic
    //  if (type.MemberType.HasFlag(MemberTypes.Field)
    //    || type.MemberType.HasFlag(MemberTypes.Property)
    //    || type.MemberType.HasFlag(MemberTypes.Event))
    //  {
    //    return memberNameBuilder.ToString();
    //  }

    //  int indexOfGenericTypeArgumentStart = type.Name.IndexOf('`');
    //  Type[] genericTypeArguments = Array.Empty<Type>();
    //  if (type is Type type)
    //  {
    //    if (!type.IsGenericType)
    //    {
    //      memberNameBuilder = BuildInheritanceSignature(memberNameBuilder, type, isFullyQualified: false);

    //      return memberNameBuilder.ToString();
    //    }

    //    genericTypeArguments = type.GetGenericArguments();
    //  }
    //  else if (type is MethodInfo methodInfo)
    //  {
    //    if (!methodInfo.IsGenericMethod)
    //    {
    //      return memberNameBuilder.ToString();
    //    }

    //    genericTypeArguments = methodInfo.GetGenericArguments();
    //  }
    //  else if (type is ConstructorInfo constructorInfo)
    //  {
    //    _ = memberNameBuilder.Clear()
    //      .Append(type.DeclaringType.Name);
    //    if (!constructorInfo.DeclaringType.IsGenericType)
    //    {
    //      return memberNameBuilder.ToString();
    //    }

    //    indexOfGenericTypeArgumentStart = memberNameBuilder.ToString().IndexOf('`');
    //  }

    //  _ = memberNameBuilder.Remove(indexOfGenericTypeArgumentStart, memberNameBuilder.Length - indexOfGenericTypeArgumentStart);
    //  memberNameBuilder = FinishTypeNameConstruction(memberNameBuilder, genericTypeArguments);
    //  if (type is Type superclass && superclass.BaseType != null)
    //  {
    //    memberNameBuilder = BuildInheritanceSignature(memberNameBuilder, superclass, isFullyQualified: false);
    //  }

    //  return memberNameBuilder.ToString();
    //}

    /// <summary>
    /// Extension method to convert generic and non-generic type names to a readable display genericTypeParameterIdentifier including the symbolNamespace.
    /// </summary>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this MemberInfo memberInfo, bool isShortName = false)
      => ToDisplayNameInternal(memberInfo, isFullyQualifiedName: true, isShortName);
    //{
    //  StringBuilder fullMemberNameBuilder = new StringBuilder(type is Type typeInfo
    //    ? $"{typeInfo.Namespace}.{typeInfo.ToDisplayName()}"
    //    : $"{type.DeclaringType.Namespace}.{type.DeclaringType.ToDisplayName()}.{type.Name}");

    //  switch (type)
    //  {
    //    case Type type:
    //  }

    //  if (type.MemberType.HasFlag(MemberTypes.Field)
    //    || type.MemberType.HasFlag(MemberTypes.Property)
    //    || type.MemberType.HasFlag(MemberTypes.Event))
    //  {
    //    return fullMemberNameBuilder.ToString();
    //  }

    //  int indexOfGenericTypeArgumentStart = fullMemberNameBuilder.ToString().IndexOf('`');
    //  Type[] genericTypeArguments = Array.Empty<Type>();
    //  if (type is Type type)
    //  {
    //    _ = fullMemberNameBuilder.Clear()
    //      .Append(type.FullName);
    //    if (!type.IsGenericType)
    //    {
    //      fullMemberNameBuilder = BuildInheritanceSignature(fullMemberNameBuilder, type, isFullyQualified: true);

    //      return fullMemberNameBuilder.ToString();
    //    }

    //    indexOfGenericTypeArgumentStart = type.FullName.IndexOf('`');
    //    genericTypeArguments = type.GetGenericArguments();
    //  }
    //  else if (type is MethodInfo methodInfo)
    //  {
    //    if (!methodInfo.IsGenericMethod)
    //    {
    //      return fullMemberNameBuilder.ToString();
    //    }

    //    genericTypeArguments = methodInfo.GetGenericArguments();
    //  }
    //  else if (type is ConstructorInfo constructorInfo)
    //  {
    //    _ = fullMemberNameBuilder.Clear()
    //      .Append($"{constructorInfo.DeclaringType.Namespace}.{constructorInfo.DeclaringType.ToDisplayName()}.{constructorInfo.DeclaringType.Name}");
    //    if (!constructorInfo.DeclaringType.IsGenericType)
    //    {
    //      return fullMemberNameBuilder.ToString();
    //    }

    //    indexOfGenericTypeArgumentStart = fullMemberNameBuilder.ToString().IndexOf('`');
    //  }

    //  _ = fullMemberNameBuilder.Remove(indexOfGenericTypeArgumentStart, fullMemberNameBuilder.Length - indexOfGenericTypeArgumentStart);
    //  fullMemberNameBuilder = FinishTypeNameConstruction(fullMemberNameBuilder, genericTypeArguments);
    //  if (type is Type superclass && superclass.BaseType != null)
    //  {
    //    fullMemberNameBuilder = BuildInheritanceSignature(fullMemberNameBuilder, superclass, isFullyQualified: true);
    //  }

    //  return fullMemberNameBuilder.ToString();
    //}

    /// <summary>
    /// Extension method to convert generic and non-generic type names to a readable display genericTypeParameterIdentifier including the symbolNamespace.
    /// </summary>
    /// <param genericTypeParameterIdentifier="memberInfo">The <see cref="Type"/> to extend.</param>
    /// <returns>
    /// A readable genericTypeParameterIdentifier of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type genericTypeParameterIdentifier like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    private static string ToDisplayNameInternal(MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
    {
      StringBuilder nameBuilder = StringBuilderFactory.GetOrCreate()
        .AppendDisplayNameInternal(memberInfo, isFullyQualifiedName, isShortName);
      string symbolName = nameBuilder.ToString();
      StringBuilderFactory.Recycle(nameBuilder);

      return symbolName;
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isShortName = false)
      => AppendDisplayNameInternal(nameBuilder, memberInfo, isFullyQualifiedName: true, isShortName);

    public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isShortName = false)
      => AppendDisplayNameInternal(nameBuilder, memberInfo, isFullyQualifiedName: true, isShortName);

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, Type type, bool isFullyQualifiedName, bool isShortName)
    {
      if (type.IsByRef)
      {
        type = type.GetElementType();
      }

      var typeReference = new CodeTypeReference(type);
      ReadOnlySpan<char> typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).AsSpan();

      if (type.IsGenericType)
      {
        int startIndexOfGenericTypeParameters = typeName.IndexOf('<');
        typeName = typeName.Slice(0, startIndexOfGenericTypeParameters);
      }

      if (!isFullyQualifiedName)
      {
        int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
        if (startIndexOfUnqualifiedTypeName > 0)
        {
          typeName = typeName.Slice(startIndexOfUnqualifiedTypeName, typeName.Length - startIndexOfUnqualifiedTypeName);
        }
      }

      _ = nameBuilder.Append(typeName.ToArray());

      if (isShortName)
      {
        return nameBuilder;
      }

      if (type.IsGenericType)
      {
        _ = nameBuilder.AppendGenericTypeArguments(type, isFullyQualifiedName);
      }

      return nameBuilder;
    }

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
    {
      if (memberInfo is Type type)
      {
        return nameBuilder.AppendDisplayNameInternal(type, isFullyQualifiedName, isShortName);
      }

      if (isFullyQualifiedName)
      {
        _ = nameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isShortName)
          .Append('.');
      }

      if (memberInfo.MemberType.HasFlag(MemberTypes.Constructor))
      {
        return nameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName: false, isShortName: true);
      }
      else
      {
        _ = nameBuilder.Append(memberInfo.Name);

        if (!isShortName)
        {
          _ = nameBuilder.AppendGenericTypeArguments(memberInfo, isFullyQualifiedName);
        }
      }

      return nameBuilder;
    }

    public static StringBuilder AppendSignatureName(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
    {
      SyntaxNode syntaxGraph = null;
      bool isTerminationRequested = false;
      if (memberInfo is MethodInfo methodInfo)
      {
        syntaxGraph = CreateMethodGraph(methodInfo, isFullyQualifiedName);
        isTerminationRequested = true;
      }

      if (syntaxGraph != null)
      {
        _ = nameBuilder.Append(syntaxGraph.ToString());
        if (isTerminationRequested)
        {
          _ = nameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
        }
      }

      return nameBuilder;
    }

    private static SyntaxNode CreateMethodGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
    {
      TypeSyntax returnType = SyntaxFactory.ParseTypeName(ToDisplayNameInternal(methodInfo.ReturnType, isFullyQualifiedName, isShortName: false));
      string methodName = ToDisplayNameInternal(methodInfo, isFullyQualifiedName, isShortName: false);
      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

      ParameterInfo[] parameters = methodInfo.GetParameters();
      foreach (ParameterInfo parameter in parameters)
      {
        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
          .WithType(SyntaxFactory.IdentifierName(ToDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isShortName: false)));

        if (parameter.IsRef())
        {
          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
        }
        else if (parameter.IsIn)
        {
          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
        }
        else if (parameter.IsOut)
        {
          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
        }
        //IList<CustomAttributeData> parameterAttributes = parameter.GetCustomAttributesData();
        //foreach(CustomAttributeData parameterAttribute in parameterAttributes)
        //{
        //  AttributeArgumentListSyntax argumentList = SyntaxFactory.AttributeArgumentList();

        //  IList<CustomAttributeTypedArgument> arguments = parameterAttribute.ConstructorArguments;
        //  foreach (CustomAttributeTypedArgument argument in arguments)
        //  {
        //    var argumentSyntax = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression)
        //  }
        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(ToDisplayNameInternal(attributeSyntax.Name, isFullyQualifiedName, isShortName: false)));
        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
        //}
        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
      }

      if (methodInfo.IsGenericMethod)
      {
        Type[] typeArguments = methodInfo.GetGenericArguments();
        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
        {
          Type typeArgument = typeArguments[typeArgumentIndex];
          //TypeParameterSyntax typeParameter = CreateMethodTypeParameter(typeArgument, isFullyQualifiedName);
          //methodGraph = methodGraph.AddTypeParameterListParameters(typeParameter);

          if (methodInfo.IsGenericMethodDefinition)
          {
            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
            }

            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }

            Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();
            foreach (Type constraintType in constraintTypes)
            {
              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
              {
                continue;
              }

              string constraintName = ToDisplayNameInternal(constraintType, isFullyQualifiedName, isShortName: false);
              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
              constraints = constraints.Add(constraintSyntax);
            }

            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
            }

            string genericTypeParameterName = ToDisplayNameInternal(typeArgument, isFullyQualifiedName, isShortName: false);
            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
          }
        }
      }

      methodGraph = methodGraph.NormalizeWhitespace();
      return methodGraph;
    }

    private static SyntaxNode CreateDelegateGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
    {
      TypeSyntax returnType = SyntaxFactory.ParseTypeName(ToDisplayNameInternal(methodInfo.ReturnType, isFullyQualifiedName, isShortName: false));
      string methodName = ToDisplayNameInternal(methodInfo, isFullyQualifiedName, isShortName: true);
      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

      ParameterInfo[] parameters = methodInfo.GetParameters();
      foreach (ParameterInfo parameter in parameters)
      {
        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.Name))
          .WithType(SyntaxFactory.IdentifierName(ToDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isShortName: false)));

        if (parameter.IsRef())
        {
          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
        }
        else if (parameter.IsIn)
        {
          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
        }
        else if (parameter.IsOut)
        {
          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
        }
        //IList<CustomAttributeData> parameterAttributes = parameter.GetCustomAttributesData();
        //foreach(CustomAttributeData parameterAttribute in parameterAttributes)
        //{
        //  AttributeArgumentListSyntax argumentList = SyntaxFactory.AttributeArgumentList();

        //  IList<CustomAttributeTypedArgument> arguments = parameterAttribute.ConstructorArguments;
        //  foreach (CustomAttributeTypedArgument argument in arguments)
        //  {
        //    var argumentSyntax = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression)
        //  }
        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(ToDisplayNameInternal(attributeSyntax.Name, isFullyQualifiedName, isShortName: false)));
        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
        //}
        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
      }

      if (methodInfo.IsGenericMethod)
      {
        Type[] typeArguments = methodInfo.GetGenericArguments();
        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
        {
          Type typeArgument = typeArguments[typeArgumentIndex];
          TypeParameterSyntax typeParameter = CreateMethodTypeParameter(typeArgument, isFullyQualifiedName);
          methodGraph = methodGraph.AddTypeParameterListParameters(typeParameter);

          if (methodInfo.IsGenericMethodDefinition)
          {
            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
            {
              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
            }

            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
            {
              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }

            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
            {
              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
            }

            Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();
            foreach (Type constraintType in constraintTypes)
            {
              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
              {
                continue;
              }

              string constraintName = ToDisplayNameInternal(constraintType, isFullyQualifiedName, isShortName: false);
              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
              constraints = constraints.Add(constraintSyntax);
            }

            string genericTypeParameterName = ToDisplayNameInternal(typeArgument, isFullyQualifiedName, isShortName: false);
            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
          }
        }
      }

      methodGraph = methodGraph.NormalizeWhitespace(indentation: " ", elasticTrivia: true);
      return methodGraph;
    }

    private static TypeParameterSyntax CreateMethodTypeParameter(Type type, bool isFullyQualifiedName)
    {
      IEnumerable<Attribute> attributes = type.GetCustomAttributes();
      AttributeListSyntax attributeSyntaxList = SyntaxFactory.AttributeList();
      foreach (Attribute attribute in attributes)
      {
        string attributeName = ToDisplayNameInternal(attribute.GetType(), isFullyQualifiedName, isShortName: false);
        AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));
        attributeSyntaxList = attributeSyntaxList.AddAttributes(attributeSyntax);
      }

      SyntaxKind variance = SyntaxKind.None;
      if (type.IsGenericParameter)
      {
        if ((type.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
        {
          variance = SyntaxKind.OutKeyword;
        }
        else if ((type.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
        {
          variance = SyntaxKind.InKeyword;
        }
      }

      string typeParameterName = ToDisplayNameInternal(type, isFullyQualifiedName, isShortName: false);
      return SyntaxFactory.TypeParameter(new SyntaxList<AttributeListSyntax>() { attributeSyntaxList }, SyntaxFactory.Token(variance), SyntaxFactory.Identifier(typeParameterName));
    }

    private static StringBuilder AppendGenericTypeArguments(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualified)
    {
      // Could be an open generic type. Therefore we need to obtain all definitions.
      Type[] genericTypeParameterDefinitions = Type.EmptyTypes;
      Type[] genericTypeArguments;
      if (memberInfo is MethodInfo methodInfo && methodInfo.IsGenericMethod)
      {
        genericTypeArguments = methodInfo.GetGenericArguments();
        if (methodInfo.IsGenericMethodDefinition)
        {
          genericTypeParameterDefinitions = methodInfo.GetGenericArguments();
        }
      }
      else if (memberInfo is Type type && type.IsGenericType)
      {
        genericTypeArguments = type.GetGenericArguments();
        if (type.IsGenericTypeDefinition)
        {
          genericTypeParameterDefinitions = type.GetGenericArguments(); 
        }
      }
      else
      {
        return nameBuilder;
      }

      _ = nameBuilder.Append('<');
      for (int typeArgumentIndex = 0; typeArgumentIndex < genericTypeArguments.Length; typeArgumentIndex++)
      {
        Type genericParameterType = genericTypeArguments[typeArgumentIndex];
        if (genericTypeParameterDefinitions.Length > 0)
        {
          Type genericTypeParameterDefinition = genericTypeParameterDefinitions[typeArgumentIndex];
          if ((genericTypeParameterDefinition.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
          {
            _ = nameBuilder.Append("out")
              .Append(' ');
          }
          else if ((genericTypeParameterDefinition.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
          {
            _ = nameBuilder.Append("in")
              .Append(' ');
          }
        }

        _ = nameBuilder.AppendDisplayNameInternal(genericParameterType, isFullyQualified, isShortName: false)
          .Append(HelperExtensionsCommon.ParameterSeparator);
      }

      // Remove trailing comma and whitespace
      _ = nameBuilder.Remove(nameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
        .Append('>');

      return nameBuilder;
    }

    private static StringBuilder AppendGenericTypeConstraints(this StringBuilder constraintBuilder, Type[] genericTypeDefinitions, bool isFullyQualified, bool isCompact)
    {
      bool hasSingleNewLine = false;
      for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitions.Length; genericTypeArgumentIndex++)
      {
        Type genericTypeDefinition = genericTypeDefinitions[genericTypeArgumentIndex];
        Type[] constraints = genericTypeDefinition.GetGenericParameterConstraints();
        if ((genericTypeDefinition.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
          && constraints.Length == 0)
        {
          continue;
        }

        if (isCompact)
        {
          if (!hasSingleNewLine)
          {
            _ = constraintBuilder.AppendLine()
            .Append(HelperExtensionsCommon.Indentation);
            hasSingleNewLine = true;
          }
          else
          {
            _ = constraintBuilder.Append(' ');
          }
        }
        else
        {
          _ = constraintBuilder.AppendLine()
            .Append(HelperExtensionsCommon.Indentation);
        }

        _ = constraintBuilder.Append("where")
          .Append(' ')
          .Append(genericTypeDefinition.Name)
          .Append(" : ");

        if ((genericTypeDefinition.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
        {
          _ = constraintBuilder.Append("class")
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        if ((genericTypeDefinition.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
        {
          _ = constraintBuilder.Append("struct")
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        foreach (Type constraint in constraints)
        {
          if (constraint == typeof(object) || constraint == typeof(ValueType))
          {
            continue;
          }

          _ = constraintBuilder.AppendDisplayNameInternal(constraint, isFullyQualified, isShortName: false)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        if (!genericTypeDefinition.IsValueType && (genericTypeDefinition.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
        {
          _ = constraintBuilder.Append("new()")
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        _ = constraintBuilder.Remove(constraintBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
      }

      return constraintBuilder;
    }

    private static StringBuilder AppendInheritanceSignature(this StringBuilder memberNameBuilder, Type type, bool isFullyQualified)
    {
      if (!type.IsDelegate())
      {
        bool isSubclass = type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType);
        Type[] interfaces = type.GetInterfaces();
        bool hasInterfaces = interfaces.Length > 0;
        if (isSubclass || hasInterfaces)
        {
          _ = memberNameBuilder.Append(" : ");
        }

        if (isSubclass)
        {
          _ = memberNameBuilder.Append(isFullyQualified ? type.BaseType.FullName : type.BaseType.Name)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        foreach (Type interfaceInfo in interfaces)
        {
          _ = memberNameBuilder.Append(isFullyQualified ? interfaceInfo.FullName : interfaceInfo.Name)
            .Append(HelperExtensionsCommon.ParameterSeparator);
        }

        if (isSubclass || hasInterfaces)
        {
          _ = memberNameBuilder.Remove(memberNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
        }
      }

      return memberNameBuilder;
    }

    public static StringBuilder AppendStringBuilder(this StringBuilder stringBuilder, StringBuilder value)
    {
#if NETSTANDARD2_0 || NETFRAMEWORK
      char[] tempArray = new char[value.Length];
      value.CopyTo(0, tempArray, 0, value.Length);
      return stringBuilder.Append(tempArray);
#else
      return stringBuilder.Append(value);
#endif
    }

    public static StringBuilder AppendReadOnlySpan(this StringBuilder stringBuilder, ReadOnlySpan<char> value)
    {
#if NETSTANDARD2_0 || NETFRAMEWORK
      return stringBuilder.Append(value.ToArray());
#else
      return stringBuilder.Append(value);
#endif
    }

    // TODO::Test if checking get() is enough to determine if a property is overridden
    public static bool IsDelegate(this Type typeInfo)
    {
      TypeData typeData = GetMemberInfoDataCacheEntry<TypeData>(typeInfo);
      return typeData.IsDelegate;
    }

    internal static bool IsDelegateInternal(this Type typeInfo)
      => HelperExtensionsCommon.DelegateType.IsAssignableFrom(typeInfo);

    // TODO::Test if checking get() is enough to determine if a property is overridden
    public static bool IsOverride(this PropertyInfo propertyInfo)
    {
      SymbolInfoDataCacheKey cacheKey = CreateMemberInfoDataCacheKey(propertyInfo);
      MemberInfoData memberInfoData = GetMemberInfoDataCacheEntry<MemberInfoData>(cacheKey, propertyInfo);
      return memberInfoData.IsOverride;
    }

    internal static bool IsOverrideInternal(this PropertyInfo propertyInfo)
      => propertyInfo.CanRead ? propertyInfo.GetGetMethod(true).IsOverride() : propertyInfo.GetSetMethod().IsOverride();

    public static bool IsOverride(this MethodInfo methodInfo)
    {
      MethodData methodData = GetMemberInfoDataCacheEntry<MethodData>(methodInfo);
      return methodData.IsOverride;
    }

    internal static bool IsOverrideInternal(MethodData methodData)
    {
      MethodInfo methodInfo = methodData.GetMethodInfo();
      return !methodInfo.Equals(methodInfo.GetBaseDefinition());
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    public static bool IsAwaitable(this MethodInfo methodInfo)
    {
      MethodData methodData = GetMemberInfoDataCacheEntry<MethodData>(methodInfo);
      return methodData.IsAwaitable;
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    internal static bool IsAwaitableInternal(MethodData methodData)
      => IsAwaitableInternal(methodData.ReturnTypeData);

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    public static bool IsAwaitable(this Type type)
    {
      TypeData typeData = GetMemberInfoDataCacheEntry<TypeData>(type);
      return typeData.IsAwaitable;
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    internal static bool IsAwaitableInternal(TypeData typeData)
    {
      Type type = typeData.GetType();
      if (IsAwaitableTask(type) || IsAwaitableValueTask(type))
      {
        return true;
      }

      if (type.GetMethod(nameof(Task.GetAwaiter)) != null)
      {
        return true;
      }

      // The return type of the method is not directly returning an awaitable type.
      // So, search for an extension method named "GetAwaiter" for the return type of the currently validated method that effectively converts the type into an awaitable object.
      // By compiler convention the "GetAwaiter" method must return an awaiter object that implements the INotifyComplete interface
      foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
      {
        foreach (System.Reflection.TypeInfo typeInfo in assembly.GetExportedTypes())
        {
          if (!typeInfo.CanDeclareExtensionMethods())
          {
            continue;
          }

          MethodInfo extensionMethodInfo = typeInfo.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { type }, null);
          if (extensionMethodInfo == null
            || !extensionMethodInfo.IsExtensionMethodOf(type))
          {
            return false;
          }

          if (extensionMethodInfo.ReturnType.GetProperty("IsCompleted") != null
            && extensionMethodInfo.ReturnType.GetInterface(nameof(INotifyCompletion)) != null
            && extensionMethodInfo.ReturnType.GetMethod("GetResult") is MethodInfo getResultMethodInfo
            && getResultMethodInfo.GetParameters().Length == 0)
          {
            return true;
          }
        }
      }

      return false;
    }

    private static bool IsAwaitableTask(Type type)
      => HelperExtensionsCommon.TaskType.IsAssignableFrom(type)
        || HelperExtensionsCommon.TaskType.IsAssignableFrom(type.BaseType);

    private static bool IsAwaitableValueTask(Type type)
      => HelperExtensionsCommon.ValueTaskType.IsAssignableFrom(type)
        || HelperExtensionsCommon.ValueTaskType.IsAssignableFrom(type.BaseType);

    public static bool IsMarkedAsync(this MethodInfo methodInfo)
    {
      MethodData methodData = GetMemberInfoDataCacheEntry<MethodData>(methodInfo);
      return methodData.IsAsync;
    }

    internal static bool IsMarkedAsyncInternal(MethodData methodData)
      => methodData.GetMethodInfo().GetCustomAttribute(HelperExtensionsCommon.AsyncStateMachineAttributeType) != null;

    /// <summary>
    /// Extension method to check if a <see cref="Type"/> is static.
    /// </summary>
    /// <param genericTypeParameterIdentifier="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="typeInfo"/> is static. Otherwise <see langword="false"/>.</returns>
    public static bool IsStatic(this Type typeInfo)
    {
      TypeData typeData = GetMemberInfoDataCacheEntry<TypeData>(typeInfo);
      return typeData.IsStatic;
    }

    internal static bool IsStaticInternal(TypeData typeData) 
      => typeData.IsAbstract && typeData.IsSealed;

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="ref"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsRef(this ParameterInfo parameterInfo)
    {
      ParameterData typeData = GetMemberInfoDataCacheEntry<ParameterData>(parameterInfo);
      return typeData.IsRef;
    }

    internal static bool IsRefInternal(ParameterData parameterData)
      => parameterData.IsByRef && !parameterData.ParameterInfo.IsOut && !parameterData.ParameterInfo.IsIn;

    /// <summary>
    /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
    /// </summary>
    /// <param genericTypeParameterIdentifier="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="typeInfo"/> is allowed to define extension methods. Otherwise <see langword="false"/>.</returns>
    /// <remarks>To be able to define extension methods a class must be static, non-generic, a top level type. 
    /// <br/>In addition this method checks if the declaring class and the method are both decorated with the <see cref="ExtensionAttribute"/> which is added by the compiler.</remarks>
    public static bool CanDeclareExtensionMethods(this Type typeInfo)
    {
      TypeData typeData = GetMemberInfoDataCacheEntry<TypeData>(typeInfo);
      return typeData.CanDeclareExtensionMethod;
    }

    internal static bool CanDeclareExtensionMethodsInternal(TypeData typeData)
    {
      Type typeInfo = typeData.GetType();
      if (!typeData.IsStatic || typeInfo.IsNested || typeInfo.IsGenericType)
      {
        return false;
      }

      Attribute typeExtensionAttribute = typeInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      return typeExtensionAttribute != null;
    }

    /// <summary>
    /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method. Otherwise <see langword="false"/>.</returns>
    public static bool IsExtensionMethod(this MethodInfo methodInfo)
    {
      // Check if the declaring class satisfies the constraints to declare extension methods
      if (!methodInfo.DeclaringType.CanDeclareExtensionMethods())
      {
        return false;
      }

      /* Check if the method satisfies the constraints to act as an extension methods */

      if (!methodInfo.IsStatic)
      {
        return false;
      }

      Attribute methodExtensionAttribute = methodInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      if (methodExtensionAttribute == null)
      {
        return false;
      }

      ParameterInfo[] parameterInfos = methodInfo.GetParameters();
      if (parameterInfos.Length < 1)
      {
        return false;
      }

      return true;
    }
    internal static bool IsExtensionMethodInternal(MethodData methodData)
    {
      // Check if the declaring class satisfies the constraints to declare extension methods
      TypeData declaringTypeData = methodData.DeclaringTypeData;
      if (!declaringTypeData.CanDeclareExtensionMethod)
      {
        return false;
      }

      /* Check if the method satisfies the constraints to act as an extension methods */

      if (!methodData.IsStatic)
      {
        return false;
      }

      MethodInfo methodInfo = methodData.GetMethodInfo(); 
      Attribute methodExtensionAttribute = methodInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
      if (methodExtensionAttribute == null)
      {
        return false;
      }

      // Must have at least the 'this' parameter
      ParameterData[] parameterInfoData = methodData.Parameters;
      if (parameterInfoData.Length < 1)
      {
        return false;
      }

      return true;
    }

    /// <summary>
    /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method for a particular type.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <param genericTypeParameterIdentifier="typeToExtend">The <see cref="Type"/> the <paramref genericTypeParameterIdentifier="methodInfo"/> is expected to extend.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method for <paramref genericTypeParameterIdentifier="typeToExtend"/>. Otherwise <see langword="false"/>.</returns>
    public static bool IsExtensionMethodOf(this MethodInfo methodInfo, Type typeToExtend)
    {
      // Check if the declaring class satisfies the constraints to declare extension methods
      if (!methodInfo.DeclaringType.CanDeclareExtensionMethods())
      {
        return false;
      }

      /* Check if the method satisfies the constraints to act as an extension methods */

      if (!methodInfo.IsExtensionMethodInternal())
      {
        return false;
      }

      ParameterInfo[] parameterInfos = methodInfo.GetParameters();
      if (parameterInfos.Length > 0)
      {
        ParameterInfo firstParameterInfo = parameterInfos[0];
        if (firstParameterInfo.ParameterType.IsAssignableFrom(typeToExtend))
        {
          return true;
        }
      }

      return false;
    }

#if !NETSTANDARD2_0
    public static bool IsReadOnlyStruct(Type type)
    {
      TypeData typeData = GetMemberInfoDataCacheEntry<TypeData>(type);
      return typeData.IsReadOnlyStruct;
    }

    internal static bool IsReadOnlyStructInternal(Type type)
      => type.IsValueType && type.GetCustomAttribute(HelperExtensionsCommon.IsReadOnlyAttributeType) != null;
#endif

    //public static object GetAwaiter(this object obj)
    //{
    //  MethodInfo getAwaiterMethodInfo = obj.GetType().GetMethod(nameof(Task.GetAwaiter));
    //  if (getAwaiterMethodInfo != null)
    //  {
    //    return (Task)getAwaiterMethodInfo.Invoke(obj, null);
    //  }

    //  // The return type of the method is not directly returning an awaitable type.
    //  // So search for an extension method named "GetAwaiter" for the return type that effectively converts the type into an awaitable object.
    //  foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
    //  {
    //    foreach (TypeInfo typeInfo in assembly.GetExportedTypes())
    //    {
    //      if (!typeInfo.IsSealed || typeInfo.IsGenericType)
    //      {
    //        continue;
    //      }

    //      getAwaiterMethodInfo = typeInfo.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public, null, new[] { obj.GetType() }, null);
    //      if (getAwaiterMethodInfo != null)
    //      {
    //        return (Task)getAwaiterMethodInfo.Invoke(obj, null);
    //      }

    //      //foreach (MethodInfo extensionMethodCandidate in typeInfo.GetMethods(BindingFlags.Static | BindingFlags.Public))
    //      //{
    //      //  if (!extensionMethodCandidate.Name.Equals(nameof(Task.GetAwaiter), StringComparison.Ordinal))
    //      //  {
    //      //    continue;
    //      //  }

    //      //  ParameterInfo[] parameterInfos = extensionMethodCandidate.GetParameters();
    //      //  if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType == getAwaiterMethodInfo.ReturnType)
    //      //  {
    //      //    return true;
    //      //  }
    //      //}
    //    }
    //  }

    //  return null;
    //}

    //internal static SymbolKinds GetKind(this MemberInfo memberInfo)
    //{
    //  SymbolInfoData cacheEntry = GetMemberInfoDataCacheEntry<SymbolInfoData>(memberInfo);
    //  return cacheEntry.Kind;
    //}

    internal static SymbolKinds GetKindInternal(TypeData typeData)
    {
      Type type =  typeData.GetType();
      if (typeData.IsDelegate)
      {
        SymbolKinds delegateKind = SymbolKinds.Delegate;
        if (type.IsGenericType)
        {
          delegateKind |= SymbolKinds.Generic;
        }

        return delegateKind;
      }

      if (type.IsClass)
      {
        SymbolKinds classKind = SymbolKinds.Class;
        if (type.IsAbstract)
        {
          classKind |= SymbolKinds.Abstract;
        }

        if (type.IsSealed)
        {
          classKind |= SymbolKinds.Final;
        }

        if (typeData.IsStatic)
        {
          classKind |= SymbolKinds.Static;
        }

        if (type.IsGenericType)
        {
          classKind |= SymbolKinds.Generic;
        }

        return classKind;
      }

      if (type.IsInterface)
      {
        SymbolKinds interfaceKind = SymbolKinds.Interface;
        return interfaceKind;
      }

      if (type.IsEnum)
      {
        SymbolKinds enumKind = SymbolKinds.Enum;
        return enumKind;
      }

      if (type.IsValueType)
      {
        SymbolKinds structKind = SymbolKinds.Struct;

        if (type.IsGenericType)
        {
          structKind |= SymbolKinds.Generic;
        }

#if !NETSTANDARD2_0
        bool isReadOnlyStruct = typeData.IsReadOnlyStruct;
        if (isReadOnlyStruct)
        {
          structKind |= SymbolKinds.Final;
        }
#endif
        return structKind;
      }

      return SymbolKinds.Undefined;
    }

    internal static SymbolKinds GetKindInternal(PropertyData propertyData)
    {
      PropertyInfo propertyInfo = propertyData.PropertyInfo;
      SymbolKinds propertyKind = propertyData.IsIndexer
        ? SymbolKinds.IndexerProperty
        : SymbolKinds.Property;

      MethodData accessorData = propertyData.GetMethodData ?? propertyData.SetMethodData;
      MethodInfo accessorMethodInfo = accessorData.GetMethodInfo();
      if (!propertyInfo.CanWrite)
      {
        propertyKind |= SymbolKinds.Final;
      }

      if (accessorMethodInfo.IsAbstract)
      {
        propertyKind |= SymbolKinds.Abstract;
      }

      if (accessorMethodInfo.IsStatic)
      {
        propertyKind |= SymbolKinds.Static;
      }

      if (accessorMethodInfo.IsVirtual)
      {
        propertyKind |= SymbolKinds.Virtual;
      }

      if (propertyData.IsOverride)
      {
        propertyKind |= SymbolKinds.Override;
      }

      return propertyKind;
    }

    internal static SymbolKinds GetKindInternal(FieldData fieldData)
    {
      FieldInfo fieldInfo = fieldData.GetFieldInfo();
      SymbolKinds fieldKind = SymbolKinds.Field;
      if (fieldInfo.IsInitOnly)
      {
        fieldKind |= SymbolKinds.Final;
      }

      if (fieldInfo.IsStatic)
      {
        fieldKind |= SymbolKinds.Static;
      }

      return fieldKind;
    }

    internal static SymbolKinds GetKindInternal(MethodData methodData)
    {
      MethodInfo methodInfo = methodData.GetMethodInfo();
      SymbolKinds methodKind = SymbolKinds.Method;
      if (methodInfo.IsFinal)
      {
        methodKind |= SymbolKinds.Final;
      }

      if (methodInfo.IsAbstract)
      {
        methodKind |= SymbolKinds.Abstract;
      }

      if (methodInfo.IsStatic)
      {
        methodKind |= SymbolKinds.Static;
      }

      if (methodInfo.IsVirtual)
      {
        methodKind |= SymbolKinds.Virtual;
      }

      if (methodData.IsOverride)
      {
        methodKind |= SymbolKinds.Override;
      }

      if (methodInfo.IsGenericMethod)
      {
        methodKind |= SymbolKinds.Generic;
      }

      return methodKind;
    }

    internal static SymbolKinds GetKindInternal(ConstructorData constructorData)
    {
      ConstructorInfo constructorInfo = constructorData.GetConstructorInfo();
      SymbolKinds constructorKind = SymbolKinds.Constructor;

      if (constructorInfo.IsStatic)
      {
        constructorKind |= SymbolKinds.Static;
      }

      return constructorKind;
    }

    internal static SymbolKinds GetKindInternal(EventData eventData)
    {
      MethodData eventAddMethodInfo = eventData.AddMethod;
      SymbolKinds eventKind = SymbolKinds.Event;
      MethodInfo addHandlerMethod = eventAddMethodInfo.GetMethodInfo();
      if (addHandlerMethod.IsFinal)
      {
        eventKind |= SymbolKinds.Final;
      }

      if (addHandlerMethod.IsAbstract)
      {
        eventKind |= SymbolKinds.Abstract;
      }

      if (addHandlerMethod.IsStatic)
      {
        eventKind |= SymbolKinds.Static;
      }

      if (addHandlerMethod.IsVirtual)
      {
        eventKind |= SymbolKinds.Virtual;
      }

      if (addHandlerMethod.IsOverride())
      {
        eventKind |= SymbolKinds.Override;
      }

      return eventKind;
    }


    internal static TEntry GetMemberInfoDataCacheEntry<TEntry>(object symbolInfo) where TEntry : SymbolInfoData
    {
      SymbolInfoDataCacheKey cacheKey = CreateMemberInfoDataCacheKey(symbolInfo);
      if (!HelperExtensionsCommon.MemberInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = CreateMemberInfoDataCacheEntry(symbolInfo, cacheKey);
      }

      return (TEntry)symbolInfoData;
    }

    private static SymbolInfoData CreateMemberInfoDataCacheEntry(object symbolInfo, SymbolInfoDataCacheKey key)
    {
      MemberInfoData entry;
      switch (symbolInfo)
      {
        case Type type:
          entry = new TypeData()
          {
            Handle = type.TypeHandle,
            Name = type.Name,
            DeclaringTypeHandle = type.TypeHandle,
          };
          break;
        case MethodInfo method:
          entry = new MethodData()
          {
            Handle = method.MethodHandle,
            Name = method.Name,
            DeclaringTypeHandle = method.DeclaringType.TypeHandle,
          };
          break;
        case FieldInfo field:
          entry = new FieldData()
          {
            Handle = field.FieldHandle,
            Name = field.Name,
            DeclaringTypeHandle = field.DeclaringType.TypeHandle,
          };
          break;
        case PropertyInfo property:
          entry = new MethodData()
          {
            Name = property.Name,
            DeclaringTypeHandle = property.DeclaringType.TypeHandle,
          };
          break;
      }
      HelperExtensionsCommon.MemberInfoDataCache.Add(key, entry);
      return entry;
    }

    private static SymbolInfoDataCacheKey CreateMemberInfoDataCacheKey(object symbolInfo) => throw new NotImplementedException();

    public static dynamic Cast(this object obj, Type type)
        => typeof(HelperExtensionsCommon).GetMethod(nameof(HelperExtensionsCommon.Cast), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object) }, null).GetGenericMethodDefinition().MakeGenericMethod(type).Invoke(obj, null);

    private static T Cast<T>(this object obj) => (T)obj;

#if !NET7_0_OR_GREATER
    public static double TotalMicroseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E6, 1);
    public static double TotalNanoseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E9, 0);
#endif
  }

  internal abstract class SymbolInfoData
  {
    protected SymbolInfoData(string name, char[] symbolNamespace)
    {
      this.Name = name;
      this.Namespace = symbolNamespace;
    }

    public abstract bool IsStatic { get; }
    public abstract HashSet<CustomAttributeData> AttributeData { get; }
    public abstract SymbolKinds Kind { get; }
    public string Name { get; }
    public char[] Namespace { get; }
    public abstract AccessModifier AccessModifier { get; }
    public abstract char[] FullyQualifiedSignature { get; }
    public abstract char[] Signature { get; }
  }

  internal abstract class MemberInfoData : SymbolInfoData
  {
    private HashSet<CustomAttributeData> attributeData;

    protected MemberInfoData(MemberInfo memberInfo) : base (memberInfo.Name, memberInfo.DeclaringType.Namespace.ToCharArray())
    {
      this.DeclaringTypeHandle = memberInfo.DeclaringType.TypeHandle;
    }

    public Type GetDeclaringType()
      => Type.GetTypeFromHandle(this.DeclaringTypeHandle);

    protected abstract MemberInfo GetMemberInfo();

    public abstract bool IsOverride { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }

    public override HashSet<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new HashSet<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData()));
  }

  internal sealed class TypeData : SymbolInfoData
  {
    private HashSet<CustomAttributeData> attributeData;
    private SymbolKinds kind;
    private AccessModifier accessModifier;
    private bool? canDeclareExtensionMethod;
    private bool? isAwaitable;
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private bool? isStatic;

    public TypeData(Type type) : base(type.Name, type.Namespace.ToCharArray())
    {
      this.Handle = type.TypeHandle;
    }

    new public Type GetType()
      => Type.GetTypeFromHandle(this.Handle);

    public RuntimeTypeHandle Handle { get; }
    public bool IsAwaitable 
      => (bool)(this.isAwaitable ?? (this.isAwaitable = HelperExtensionsCommon.IsAwaitableInternal(this)));

    public bool CanDeclareExtensionMethod 
      => (bool)(this.canDeclareExtensionMethod ?? (this.canDeclareExtensionMethod = HelperExtensionsCommon.CanDeclareExtensionMethodsInternal(this)));

    public override HashSet<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new HashSet<CustomAttributeData>(GetType().GetCustomAttributesData()));

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined 
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public override char[] Signature 
      => this.signature ?? (this.signature = GetType().ToSignatureShortName().ToCharArray());

    public override char[] FullyQualifiedSignature 
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = GetType().ToSignatureShortName(isFullyQualifiedName: true).ToCharArray());

    public override bool IsStatic 
      => (bool)(this.isStatic ?? (this.isStatic = HelperExtensionsCommon.IsStaticInternal(this)));

    public override SymbolKinds Kind => this.kind is SymbolKinds.Undefined 
      ? (this.kind = HelperExtensionsCommon.GetKindInternal(this)) 
      : this.kind;

    public bool IsAbstract { get; internal set; }
    public bool IsSealed { get; internal set; }
  }

  internal sealed class MethodData : MemberInfoData
  {
    private SymbolKinds kind;
    private AccessModifier accessModifier;
    private bool? isAwaitable;
    private bool? isAsync;
    private bool? isExtensionMethod;
    private ParameterData[] parameters;
    private TypeData[] genericTypeArguments;
    private bool? isOverride;
    private bool? isStatic;
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private TypeData declaringTypeData;
    private MethodData returnTypeData;

    public MethodData(MethodInfo methodInfo) : base(methodInfo)
    {
      this.Handle = methodInfo.MethodHandle;
    }

    public MethodInfo GetMethodInfo()
      => (MethodInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle);

    protected override MemberInfo GetMemberInfo() 
      => GetMethodInfo();

    public RuntimeMethodHandle Handle { get; }
    public ParameterData[] Parameters 
      => this.parameters ?? (this.parameters = GetMethodInfo().GetParameters().Select(parameterInfo => new ParameterData(parameterInfo)).ToArray());

    public TypeData[] GenericTypeArguments
    {
      get
      {
        if (this.genericTypeArguments == null)
        {
          Type[] typeArguments = GetMethodInfo().GetGenericArguments();
          this.genericTypeArguments = typeArguments.Select(type => new TypeData(type)).ToArray();
        }

        return this.genericTypeArguments;
      }
    }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public bool IsExtensionMethod 
      => (bool)(this.isExtensionMethod ?? (this.isExtensionMethod = HelperExtensionsCommon.IsExtensionMethodInternal(this)));

    public bool IsAsync 
      => (bool)(this.isAsync ?? (this.isAsync = HelperExtensionsCommon.IsMarkedAsyncInternal(this)));

    public bool IsAwaitable 
      => (bool)(this.isAwaitable ?? (this.isAwaitable = HelperExtensionsCommon.IsAwaitableInternal(this)));

    public override bool IsOverride 
      => (bool)(this.isOverride ?? (this.isOverride = HelperExtensionsCommon.IsOverrideInternal(this)));

    public override bool IsStatic 
      => (bool)(this.isStatic ?? (this.isStatic = GetMethodInfo().IsStatic));

    public override SymbolKinds Kind => this.kind is SymbolKinds.Undefined
      ? (this.kind = HelperExtensionsCommon.GetKindInternal(this))
      : this.kind;

    public override char[] Signature => this.signature ?? (this.signature = GetType().ToSignatureShortName().ToCharArray());

    public override char[] FullyQualifiedSignature => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = GetType().ToSignatureShortName(isFullyQualifiedName: true).ToCharArray());

    public TypeData DeclaringTypeData 
      => this.declaringTypeData ?? (this.declaringTypeData = HelperExtensionsCommon.GetMemberInfoDataCacheEntry<TypeData>(GetDeclaringType()));

    public MethodData ReturnTypeData 
      => this.returnTypeData ?? (this.returnTypeData = HelperExtensionsCommon.GetMemberInfoDataCacheEntry<MethodData>(GetMethodInfo().ReturnType));
  }

  internal sealed class EventData : MemberInfoData
  {
    private readonly EventInfo eventInfo;
    private AccessModifier accessModifier;
    private bool? isStatic;

    public EventData(EventInfo eventInfo) : base(eventInfo)
    {
      this.eventInfo = eventInfo;
    }

    public EventInfo GetEventInfo()
      => this.eventInfo;

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(GetEventInfo()))
      : this.accessModifier;

    public MethodData AddMethodData { get; }
    public MethodData RemoveMethodData { get; }

    public override bool IsStatic => isStatic;
    public override SymbolKinds Kind { get; }

    public override char[] FullyQualifiedSignature { get; }
    public override char[] Signature { get; }
  }

  internal sealed class ConstructorData : MemberInfoData
  {
    private AccessModifier accessModifier;
    private IEnumerable<ParameterData> parameters;
    private IEnumerable<CustomAttributeData> attributeData;

    public ConstructorData(ConstructorInfo constructorInfo)
    {
      this.Handle = constructorInfo.MethodHandle;
      this.DeclaringTypeHandle = constructorInfo.DeclaringType.TypeHandle;
    }

    public ConstructorInfo GetConstructorInfo()
      => (ConstructorInfo)MethodInfo.GetMethodFromHandle(this.Handle);

    public Type GetDeclaringType()
      => (Type)Type.GetTypeFromHandle(this.DeclaringTypeHandle);

    public RuntimeMethodHandle Handle { get; set; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }
    public IEnumerable<ParameterData> Parameters => this.parameters ?? (this.parameters = MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle).GetParameters().Select(parameterInfo => new ParameterData(parameterInfo)));
    
    public override IEnumerable<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle).GetCustomAttributesData());

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(MethodInfo.GetMethodFromHandle(this.Handle)))
      : this.accessModifier;
  }

  internal sealed class ParameterData : MemberInfoData
  {
    private IEnumerable<CustomAttributeData> attributeData;

    public ParameterData(ParameterInfo parameterInfo)
    {
      this.IsRef = parameterInfo.IsRef();
      this.DeclaringTypeHandle = parameterInfo.Member.DeclaringType.TypeHandle;
      this.ParameterInfo = parameterInfo;
    }

    public Type GetDeclaringType()
      => (Type)Type.GetTypeFromHandle(this.DeclaringTypeHandle);

    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }
    public bool IsRef { get; }
    public ParameterInfo ParameterInfo { get; }
    public override IEnumerable<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = this.ParameterInfo.GetCustomAttributesData());

    public bool IsByRef { get; internal set; }
  }

  internal sealed class PropertyData : MemberInfoData
  {
    private AccessModifier? propertyAccessModifier;
    private AccessModifier? setAccessorAccessModifier;
    private AccessModifier? getAccessorAccessModifier;
    private ParameterData[] indexerParameters;
    private IEnumerable<CustomAttributeData> attributeData;
    private readonly TypeData propertyTypeData;

    public PropertyData(PropertyInfo propertyInfo)
    {
      this.DeclaringTypeHandle = propertyInfo.DeclaringType.TypeHandle;
      this.CanRead = propertyInfo.CanRead;
      this.CanWrite = propertyInfo.CanWrite;
      this.PropertyInfo = propertyInfo;
    }

    public Type GetDeclaringType()
      => (Type)Type.GetTypeFromHandle(this.DeclaringTypeHandle);

    private void GetAccessors()
    {
      (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = HelperExtensionsCommon.GetPropertyAccessModifier(this.PropertyInfo);
      this.propertyAccessModifier = propertyModifier;
      this.setAccessorAccessModifier = setMethodModifier;
      this.getAccessorAccessModifier = getMethodModifier;
    }

    public bool IsIndexer => this.IndexerParameters.Length > 0;
    public ParameterData[] IndexerParameters => this.indexerParameters ?? (this.indexerParameters = this.PropertyInfo.GetIndexParameters().Select(parameterInfo => new ParameterData(parameterInfo)).ToArray());

    public override AccessModifier AccessModifier
    {
      get
      {
        if (this.propertyAccessModifier is null)
        {
          GetAccessors();
        }

        return this.propertyAccessModifier.Value;
      }
    }

    public AccessModifier SetAccessorAccessModifier
    {
      get
      {
        if (this.setAccessorAccessModifier is null)
        {
          GetAccessors();
        }
        
        return this.setAccessorAccessModifier.Value;
      }
    }

    public AccessModifier GetAccessorAccessModifier
    {
      get
      {
        if (this.getAccessorAccessModifier is null)
        {
          GetAccessors();
        }

        return this.getAccessorAccessModifier.Value;
      }
    }

    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }
    public TypeData PropertyTypeData => propertyTypeData; 
    public PropertyInfo PropertyInfo { get; }
    public bool IsRef { get; }
    public bool CanWrite { get; }
    public bool CanRead { get; }
    public MethodData GetMethodData { get; }
    public MethodData SetMethodData { get; }
    public override IEnumerable<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = this.PropertyInfo.GetCustomAttributesData());
  }

  internal sealed class FieldData : MemberInfoData
  {
    private IEnumerable<CustomAttributeData> attributeData;

    public FieldInfo GetFieldInfo()
      => (FieldInfo)FieldInfo.GetFieldFromHandle(this.Handle);

    public RuntimeFieldHandle Handle { get; set; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }
    public override IEnumerable<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = FieldInfo.GetFieldFromHandle(this.Handle).GetCustomAttributesData());
  }

  internal readonly struct SymbolInfoDataCacheKey : IEquatable<SymbolInfoDataCacheKey>
  {
    public SymbolInfoDataCacheKey(string name, Type[] arguments, RuntimeTypeHandle declaringTypeHandle)
    {
      this.Name = name;
      this.Arguments = arguments;
      this.DeclaringTypeHandle = declaringTypeHandle;
    }

    public string Name { get; }
    public Type[] Arguments { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }

    public bool Equals(SymbolInfoDataCacheKey other) => other.Name.Equals(this.Name, StringComparison.Ordinal) 
      && other.Arguments.SequenceEqual(this.Arguments) 
      && other.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle);

    public override bool Equals(object obj) => obj is SymbolInfoDataCacheKey key && Equals(key);

    public override int GetHashCode()
    {
      int hashCode = 1248511333;
      hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Name);
      hashCode = hashCode * -1521134295 + EqualityComparer<Type[]>.Default.GetHashCode(this.Arguments);
      hashCode = hashCode * -1521134295 + this.DeclaringTypeHandle.GetHashCode();
      return hashCode;
    }

    public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
    public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
  }
}