namespace BionicCode.Utilities.Net
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

  /// <summary>
  /// A collection of extension methods for various default constraintTypes
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    private const string ParameterSeparator = ", ";
    private const char ExpressionTerminator = ';';
    /// <summary>
    /// The property genericTypeParameterIdentifier of an indexer property. This genericTypeParameterIdentifier is compiler generated and equals the typeName of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
    /// </summary>
    /// <typeName>The generated property genericTypeParameterIdentifier of an indexer is <c>Item</c>.</typeName>
    /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
    public static readonly string IndexerName = "Item";

    private static CSharpCodeProvider CodeProvider { get; } = new CSharpCodeProvider();

    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam genericTypeParameterIdentifier="TParam">The parameter type for the predicate.</typeparam>
    /// <param genericTypeParameterIdentifier="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref genericTypeParameterIdentifier="TParam"/>, bool></c> that returns the result of <paramref genericTypeParameterIdentifier="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => predicate.Invoke;

    ///// <summary>
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    ///// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
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
    public static string ToSignatureName(this MemberInfo memberInfo, bool isFullyQualifiedName = false)
      => memberInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: false);

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
    public static string ToSignatureShortName(this MemberInfo memberInfo, bool isFullyQualifiedName = false)
      => memberInfo.ToSignatureNameInternal(isFullyQualifiedName, isShortName: true);

    internal static string ToSignatureNameInternal(this MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
    {
      var type = memberInfo as Type;
      var propertyInfo = memberInfo as PropertyInfo;
      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        ?? type?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
      var constructorInfo = memberInfo as ConstructorInfo;
      var fieldInfo = memberInfo as FieldInfo;
      var eventInfo = memberInfo as EventInfo;
      //FieldInfo eventDeclaredFieldInfo = eventInfo?.DeclaringType.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

      SymbolKinds memberKind = GetKind(memberInfo);

      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

      IList<CustomAttributeData> symbolAttributes = memberInfo.GetCustomAttributesData();
      if (symbolAttributes.Count > 0)
      {        
        _ = signatureNameBuilder.AppendCustomAttributes(symbolAttributes)
          .AppendLine();
      }

      AccessModifier accessModifier = memberInfo.GetAccessModifier();
      _ = signatureNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

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
          bool isGenericTypeDefinition;
          if (memberKind.HasFlag(SymbolKinds.Method))
          {
            isGenericTypeDefinition = methodInfo.IsGenericMethodDefinition;
          }
          else
          {
            isGenericTypeDefinition = type.IsGenericTypeDefinition;
          }

          if (isGenericTypeDefinition)
          {
            IList<CustomAttributeData> attributes = parameter.GetCustomAttributesData();
            _ = signatureNameBuilder.AppendCustomAttributes(attributes);
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
        if (memberKind.HasFlag(SymbolKinds.GenericType) && type.ContainsGenericParameters)
        {
          genericTypeParameterDefinitions = type.GetGenericTypeDefinition().GetGenericArguments();
        }
        else if (memberKind.HasFlag(SymbolKinds.GenericMethod) && methodInfo.ContainsGenericParameters)
        {
          genericTypeParameterDefinitions = methodInfo.GetGenericMethodDefinition().GetGenericArguments();
        }

        for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeParameterDefinitions.Length; genericTypeArgumentIndex++)
        {
          Type genericTypeParameterDefinition = genericTypeParameterDefinitions[genericTypeArgumentIndex];
          _ = signatureNameBuilder.AppendGenericTypeConstraints(genericTypeParameterDefinition, isFullyQualifiedName);
        }
      }

      if (!memberKind.HasFlag(SymbolKinds.Class) && !memberKind.HasFlag(SymbolKinds.Struct))
      {
        _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
      }

      string fullMemberName = signatureNameBuilder.ToString();
      StringBuilderFactory.Recycle(signatureNameBuilder);

      return fullMemberName;
    }

    private static StringBuilder AppendCustomAttributes(this StringBuilder nameBuilder, IList<CustomAttributeData> attributes)
    {
      if (attributes.Count > 0)
      {
        foreach (CustomAttributeData attribute in attributes)
        {
          _ = nameBuilder.Append('[')
            .Append(attribute.AttributeType.Name)
            .Append('(');
          foreach (CustomAttributeTypedArgument constructorPositionalArgument in attribute.ConstructorArguments)
          {
            if (constructorPositionalArgument.ArgumentType == typeof(string))
            {
              _ = nameBuilder.Append('"')
                .Append(constructorPositionalArgument.Value)
                .Append('"')
                .Append(HelperExtensionsCommon.ParameterSeparator);
            }
            else if (constructorPositionalArgument.ArgumentType == typeof(char))
            {
              _ = nameBuilder.Append('\'')
                .Append(constructorPositionalArgument.Value)
                .Append('\'')
                .Append(HelperExtensionsCommon.ParameterSeparator);
            }
            else
            {
              _ = nameBuilder.Append(constructorPositionalArgument.Value)
                .Append(HelperExtensionsCommon.ParameterSeparator);
            }
          }

          foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
          {
            _ = nameBuilder.Append(constructorNamedArgument.MemberName)
              .Append(" = ");

            if (constructorNamedArgument.TypedValue.ArgumentType == typeof(string))
            {
              _ = nameBuilder.Append('"')
                .Append(constructorNamedArgument.TypedValue.Value)
                .Append('"')
                .Append(HelperExtensionsCommon.ParameterSeparator);
            }
            else if (constructorNamedArgument.TypedValue.ArgumentType == typeof(char))
            {
              _ = nameBuilder.Append('\'')
                .Append(constructorNamedArgument.TypedValue.Value)
                .Append('\'')
                .Append(HelperExtensionsCommon.ParameterSeparator);
            }
            else
            {
              _ = nameBuilder.Append(constructorNamedArgument.TypedValue.Value)
                .Append(HelperExtensionsCommon.ParameterSeparator);
            }
          }
        }

        // Remove trailing comma and whitespace
        _ = nameBuilder.Remove(nameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);

        _ = nameBuilder.Append(')')
          .Append(']')
          .Append(' ');
      }

      return nameBuilder;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> attributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param genericTypeParameterIdentifier="type"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="type"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="type"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref genericTypeParameterIdentifier="type"/> is not supported.</exception>
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
    public static AccessModifier GetAccessModifier(this MemberInfo memberInfo)
    {
      switch (memberInfo)
      {
        case Type typeInfo:
          return typeInfo.IsPublic ? AccessModifier.Public
            : typeInfo.IsNestedPrivate ? AccessModifier.Private
            : typeInfo.IsNestedAssembly ? AccessModifier.Internal
            : typeInfo.IsNestedFamily ? AccessModifier.Protected
            : typeInfo.IsNestedPublic ? AccessModifier.Public
            : typeInfo.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
            : typeInfo.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
            : !typeInfo.IsVisible ? AccessModifier.Internal
            : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        case MethodBase methodBaseInfo:
          return methodBaseInfo.IsPublic ? AccessModifier.Public
            : methodBaseInfo.IsPrivate ? AccessModifier.Private
            : methodBaseInfo.IsAssembly ? AccessModifier.Internal
            : methodBaseInfo.IsFamily ? AccessModifier.Protected
            : methodBaseInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
            : methodBaseInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
            : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        case FieldInfo fieldInfo:
          return fieldInfo.IsPublic ? AccessModifier.Public
            : fieldInfo.IsPrivate ? AccessModifier.Private
            : fieldInfo.IsAssembly ? AccessModifier.Internal
            : fieldInfo.IsFamily ? AccessModifier.Protected
            : fieldInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
            : fieldInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
            : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        case EventInfo eventInfo:
          return eventInfo.GetAddMethod(true).GetAccessModifier();
        case PropertyInfo propertyInfo:
          return GetPropertyAccessModifier(propertyInfo);
        default:
          throw new NotSupportedException("The provided MemberInfo is not supported");
      }
    }

    private static AccessModifier GetPropertyAccessModifier(PropertyInfo propertyInfo)
    {
      // Property accessors with the least restriction provides the access modifier for the property.
      AccessModifier propertyAccessModifier = propertyInfo.GetAccessors(true)
        .Select(accessor => accessor.GetAccessModifier())
        .Min();

      return propertyAccessModifier;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the namespace.
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
    /// Extension method to convert generic and non-generic type names to a readable display genericTypeParameterIdentifier including the namespace.
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
    /// Extension method to convert generic and non-generic type names to a readable display genericTypeParameterIdentifier including the namespace.
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

    private static StringBuilder AppendGenericTypeConstraints(this StringBuilder constraintBuilder, Type genericTypeDefinition, bool isFullyQualified)
    {
      Type[] constraints = genericTypeDefinition.GetGenericParameterConstraints();
      if ((genericTypeDefinition.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None && constraints.Length == 0)
      {
        return constraintBuilder;
      }

      _ = constraintBuilder.AppendLine()
        .Append(' ')
        .Append(' ')
        .Append(' ')
        .Append(' ')
        .Append("where")
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

      return constraintBuilder.Remove(constraintBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
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

    private static Lazy<Type> DelegateType { get; } = new Lazy<Type>(() => typeof(Delegate));
    public static bool IsDelegate(this Type typeInfo)
      => HelperExtensionsCommon.DelegateType.Value.IsAssignableFrom(typeInfo);

    // TODO::Test if checking get() is enough to determine if a property is overridden
    public static bool IsOverride(this PropertyInfo methodInfo)
      => methodInfo.GetGetMethod(true).IsOverride();

    public static bool IsOverride(this MethodInfo methodInfo)
      => !methodInfo.Equals(methodInfo.GetBaseDefinition());

    private static Lazy<Type> TaskType { get; } = new Lazy<Type>(() => typeof(Task));
    public static bool IsAwaitableTask(this MethodInfo methodInfo)
      => HelperExtensionsCommon.TaskType.Value.IsAssignableFrom(methodInfo.ReturnType)
        || HelperExtensionsCommon.TaskType.Value.IsAssignableFrom(methodInfo.ReturnType.BaseType);

    private static Lazy<Type> ValueTaskType { get; } = new Lazy<Type>(() => typeof(ValueTask));
    public static bool IsAwaitableValueTask(this MethodInfo methodInfo)
      => HelperExtensionsCommon.ValueTaskType.Value.IsAssignableFrom(methodInfo.ReturnType)
        || HelperExtensionsCommon.ValueTaskType.Value.IsAssignableFrom(methodInfo.ReturnType.BaseType);

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
    public static bool IsAwaitable(this MethodInfo methodInfo)
    {
      if (methodInfo.IsAwaitableTask() || methodInfo.IsAwaitableValueTask())
      {
        return true;
      }

      if (methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null)
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

          MethodInfo extensionMethodInfo = typeInfo.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { methodInfo.ReturnType }, null);
          if (extensionMethodInfo == null
            || !extensionMethodInfo.IsExtensionMethodOf(methodInfo.ReturnType))
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

    /// <summary>
    /// Extension method to check if a <see cref="Type"/> is static.
    /// </summary>
    /// <param genericTypeParameterIdentifier="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="typeInfo"/> is static. Otherwise <see langword="false"/>.</returns>
    public static bool IsStatic(this Type typeInfo)
      => typeInfo.IsAbstract && typeInfo.IsSealed;

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="ref"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsRef(this ParameterInfo parameterInfo)
      => parameterInfo.ParameterType.IsByRef && !parameterInfo.IsOut;

    /// <summary>
    /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
    /// </summary>
    /// <param genericTypeParameterIdentifier="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="typeInfo"/> is allowed to define extension methods. Otherwise <see langword="false"/>.</returns>
    /// <remarks>To be able to define extension methods a class must be static, non-generic, a top level type. 
    /// <br/>In addition this method checks if the declaring class and the method are both decorated with the <see cref="ExtensionAttribute"/> which is added by the compiler.</remarks>
    public static bool CanDeclareExtensionMethods(this Type typeInfo)
    {
      if (!typeInfo.IsStatic() || typeInfo.IsNested || typeInfo.IsGenericType)
      {
        return false;
      }

      ExtensionAttribute typeExtensionAttribute = typeInfo.GetCustomAttribute<ExtensionAttribute>(false);
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

      ExtensionAttribute methodExtensionAttribute = methodInfo.GetCustomAttribute<ExtensionAttribute>(false);
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

      if (!methodInfo.IsExtensionMethod())
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
    private static bool IsReadOnlyStruct(Type type)
      => type.IsValueType && type.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null;
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

    internal static SymbolKinds GetKind(this MemberInfo memberInfo)
    {
      var type = memberInfo as Type;
      var propertyInfo = memberInfo as PropertyInfo;
      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        ?? type?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
      var constructorInfo = memberInfo as ConstructorInfo;
      var fieldInfo = memberInfo as FieldInfo;
      var eventInfo = memberInfo as EventInfo;
      MethodInfo eventAddMethodInfo = eventInfo?.GetAddMethod(true);
      FieldInfo eventDeclaredFieldInfo = eventInfo?.DeclaringType.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

      bool isDelegate = type?.IsDelegate() ?? false;
      if (isDelegate)
      {
        SymbolKinds delegateKind = SymbolKinds.Delegate;
        if (type.IsGenericType)
        {
          delegateKind |= SymbolKinds.Generic;
        }

        return delegateKind;
      }

      bool isClass = !isDelegate && (type?.IsClass ?? false);
      if (isClass)
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

        if (type.IsStatic())
        {
          classKind |= SymbolKinds.Static;
        }

        if (type.IsGenericType)
        {
          classKind |= SymbolKinds.Generic;
        }

        return classKind;
      }

      bool isEnum = !isDelegate && (type?.IsEnum ?? false);
      if (isEnum)
      {
        return SymbolKinds.Enum;
      }

      bool isStruct = !isDelegate && (type?.IsValueType ?? false);
      if (isStruct)
      {
        SymbolKinds structKind = SymbolKinds.Struct;

        if (type.IsGenericType)
        {
          structKind |= SymbolKinds.Generic;
        }

#if !NETSTANDARD2_0
        bool isReadOnlyStruct = isStruct && IsReadOnlyStruct(type);
        if (isReadOnlyStruct)
        {
          structKind |= SymbolKinds.Final;
        }
#endif
        return structKind;
      }

      bool isProperty = propertyInfo != null;
      if (isProperty)
      {
        bool isIndexerProperty = indexerPropertyIndexParameters.Length > 0;
        SymbolKinds propertyKind = isIndexerProperty 
          ? SymbolKinds.IndexerProperty 
          : SymbolKinds.Property;

        MethodInfo getMethod = propertyInfo.GetGetMethod();
        if (!propertyInfo.CanWrite)
        {
          propertyKind |= SymbolKinds.Final;
        }

        if (getMethod.IsAbstract)
        {
          propertyKind |= SymbolKinds.Abstract;
        }

        if (getMethod.IsStatic)
        {
          propertyKind |= SymbolKinds.Static;
        }

        if (getMethod.IsVirtual)
        {
          propertyKind |= SymbolKinds.Virtual;
        }

        if (getMethod.IsOverride())
        {
          propertyKind |= SymbolKinds.Override;
        }

        return propertyKind;
      }

      bool isMethod = !isDelegate && !isClass && memberInfo.MemberType.HasFlag(MemberTypes.Method);
      if (isMethod)
      {
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

        if (methodInfo.IsOverride())
        {
          methodKind |= SymbolKinds.Override;
        }

        if (methodInfo.IsGenericMethod)
        {
          methodKind |= SymbolKinds.Generic;
        }

        return methodKind;
      }

      bool isEvent = eventInfo != null;
      if (isEvent)
      {
        SymbolKinds eventKind = SymbolKinds.Event;
        MethodInfo addHandlerMethod = eventInfo.GetAddMethod(true);
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

      bool isConstructor = constructorInfo != null;
      if (isConstructor)
      {
        SymbolKinds constructorKind = SymbolKinds.Constructor;

        if (constructorInfo.IsStatic)
        {
          constructorKind |= SymbolKinds.Static;
        }

        return constructorKind;
      }

      bool isField = fieldInfo != null;
      if (isField)
      {
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

      bool isInterface = !isDelegate && !isClass && (type?.IsInterface ?? false);
      if (isInterface)
      {
        SymbolKinds interfaceKind = SymbolKinds.Interface;
        return interfaceKind;
      }

      return SymbolKinds.Undefined;
    }

    public static dynamic Cast(this object obj, Type type)
        => typeof(HelperExtensionsCommon).GetMethod(nameof(HelperExtensionsCommon.Cast), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object) }, null).GetGenericMethodDefinition().MakeGenericMethod(type).Invoke(obj, null);

    private static T Cast<T>(this object obj) => (T)obj;

#if !NET7_0_OR_GREATER
    public static double TotalMicroseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E6, 1);
    public static double TotalNanoseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E9, 0);
#endif
  }
}