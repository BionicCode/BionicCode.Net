namespace BionicCode.Utilities.Net
{
  using System;
  using System.CodeDom;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using Microsoft.CSharp;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    private const string ParameterSeparator = ", ";
    /// <summary>
    /// The property name of an indexer property. This name is compiler generated and equals the typeName of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
    /// </summary>
    /// <typeName>The generated property name of an indexer is <c>Item</c>.</typeName>
    /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
    public static readonly string IndexerName = "Item";

    private static CSharpCodeProvider CodeProvider { get; } = new CSharpCodeProvider();

    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam name="TParam">The parameter type for the predicate.</typeparam>
    /// <param name="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref name="TParam"/>, bool></c> that returns the result of <paramref name="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => predicate.Invoke;

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display name without the namespace.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to extend.</param>
    /// <param name="isPropertyGet"><see langword="true"/> when the get() of the property should be used or <see langword="false"/> to use the set() method..</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature name like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this PropertyInfo propertyInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)propertyInfo).ToSignatureName(isQualifyMemberEnabled);

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display name without the namespace.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature name like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this MethodInfo methodInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)methodInfo).ToSignatureName(isQualifyMemberEnabled);

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display name without the namespace.
    /// </summary>
    /// <param name="typeInfo">The <see cref="Type"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature name like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this Type typeInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)typeInfo).ToSignatureName(false);

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display name without the namespace.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature name like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this ConstructorInfo constructorInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)constructorInfo).ToSignatureName(isQualifyMemberEnabled);

    /// <summary>
    /// Extension method to convert generic and non-generic member names to a readable full signature display name without the namespace.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full signature name like <c>"public static Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Action action);"</c>.
    /// </remarks>
    public static string ToSignatureName(this FieldInfo fieldInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)fieldInfo).ToSignatureName(isQualifyMemberEnabled);

    internal static string ToSignatureName(this MemberInfo memberInfo, bool isFullyQualifySymbolEnabled = false)
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

      SymbolKind memberKind = GetKind(memberInfo);

      var fullMemberNameBuilder = new StringBuilder();

      AccessModifier accessModifier = memberInfo.GetAccessModifier();
      _ = fullMemberNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if (!memberKind.HasFlag(SymbolKind.Delegate)
        && memberKind.HasFlag(SymbolKind.Final))
      {
        _ = fullMemberNameBuilder
          .Append("sealed")
          .Append(' ');
      }

      if (!memberKind.HasFlag(SymbolKind.Delegate)
        && memberKind.HasFlag(SymbolKind.Static))
      {
        _ = fullMemberNameBuilder
          .Append("static")
          .Append(' ');
      }

      bool isAbstract = memberKind.HasFlag(SymbolKind.Abstract);
      if (!memberKind.HasFlag(SymbolKind.Delegate) && isAbstract)
      {
        _ = fullMemberNameBuilder
          .Append("abstract")
          .Append(' ');
      }

      if (!isAbstract
        && !memberKind.HasFlag(SymbolKind.Delegate)
        && !memberKind.HasFlag(SymbolKind.Class)
        && memberKind.HasFlag(SymbolKind.Virtual))
      {
        _ = fullMemberNameBuilder
          .Append("virtual")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.ReadOnlyStruct) || memberKind.HasFlag(SymbolKind.ReadOnlyField))
      {
        _ = fullMemberNameBuilder
          .Append("readonly")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.Struct))
      {
        _ = fullMemberNameBuilder
          .Append("struct")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.Class))
      {
        _ = fullMemberNameBuilder
          .Append("class")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.Interface))
      {
        _ = fullMemberNameBuilder
          .Append("interface")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.Delegate))
      {
        _ = fullMemberNameBuilder
          .Append("delegate")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.Event))
      {
        _ = fullMemberNameBuilder
          .Append("event")
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.Enum))
      {
        _ = fullMemberNameBuilder
          .Append("enum")
          .Append(' ');
      }

      if (!memberKind.HasFlag(SymbolKind.Delegate)
        && !memberKind.HasFlag(SymbolKind.Class)
        && memberKind.HasFlag(SymbolKind.Override))
      {
        _ = fullMemberNameBuilder
          .Append("override")
          .Append(' ');
      }

      // Set return type
      if (memberKind.HasFlag(SymbolKind.Method)
        || memberKind.HasFlag(SymbolKind.Property)
        || memberKind.HasFlag(SymbolKind.Field)
        || memberKind.HasFlag(SymbolKind.Delegate)
        || memberKind.HasFlag(SymbolKind.Event))
      {
        Type returnType = fieldInfo?.FieldType
          ?? methodInfo?.ReturnType
          ?? propertyGetMethodInfo?.ReturnType
          ?? eventDeclaredFieldInfo?.FieldType;

        var typeReference = new CodeTypeReference(returnType);
        string returnTypeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();

        _ = fullMemberNameBuilder
          .Append(returnTypeName)
          .Append(' ');
      }

      if (memberKind.HasFlag(SymbolKind.IndexerProperty))
      {
        _ = fullMemberNameBuilder
          .Append("this")
          .Append('[');
      }
      else if (memberKind.HasFlag(SymbolKind.Constructor))
      {
        _ = fullMemberNameBuilder.Append(constructorInfo.ToDisplayName());
      }
      else
      {
        // Set actual qualified type name
        if (isFullyQualifySymbolEnabled)
        {
          //// Prefix with class name in case type is a member
          //if (memberInfo.DeclaringType != null)
          //{
          //  //_ = fullMemberNameBuilder.Append(memberInfo.DeclaringType.ToDisplayName())
          //  _ = fullMemberNameBuilder.Append(memberInfo.ToFullDisplayName())
          //    .Append('.');
          //}

          _ = fullMemberNameBuilder.Append(memberInfo.ToFullDisplayName());
        }
        else
        {
          _ = fullMemberNameBuilder.Append(memberInfo.ToDisplayName());
        }
      }

      if (memberKind.HasFlag(SymbolKind.Constructor) || memberKind.HasFlag(SymbolKind.Method) || memberKind.HasFlag(SymbolKind.Delegate))
      {
        _ = fullMemberNameBuilder.Append('(');

        if (memberKind.HasFlag(SymbolKind.Method) && (methodInfo?.IsExtensionMethod() ?? false))
        {
          _ = fullMemberNameBuilder
            .Append("this")
            .Append(' ');
        }
      }

      IEnumerable<ParameterInfo> parameters = methodInfo?.GetParameters()
        ?? constructorInfo?.GetParameters()
        ?? indexerPropertyIndexParameters
        ?? Enumerable.Empty<ParameterInfo>();

      if (parameters.Any())
      {
        foreach (ParameterInfo parameter in parameters)
        {
          var typeReference = new CodeTypeReference(parameter.ParameterType);
          string typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference);
          if (!isFullyQualifySymbolEnabled)
          {
            int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
            if (startIndexOfUnqualifiedTypeName > 0)
            {
              typeName = typeName.Substring(startIndexOfUnqualifiedTypeName, typeName.Length - startIndexOfUnqualifiedTypeName);
            }
          }

          _ = fullMemberNameBuilder
            .Append(typeName)
            .Append(' ')
            .Append(parameter.Name)
            .Append(ParameterSeparator);
        }

        // Remove trailing comma and whitespace
        _ = fullMemberNameBuilder.Remove(fullMemberNameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length);
      }

      if (memberKind.HasFlag(SymbolKind.IndexerProperty))
      {
        _ = fullMemberNameBuilder.Append(']');
      }

      if (memberKind.HasFlag(SymbolKind.Property))
      {
        _ = fullMemberNameBuilder
          .Append(' ')
          .Append('{')
          .Append(' ');
        if (propertyGetMethodInfo != null)
        {
          _ = fullMemberNameBuilder
            .Append("get")
            .Append(';')
            .Append(' ');
        }

        if (propertySetMethodInfo != null)
        {
          _ = fullMemberNameBuilder
            .Append("set")
            .Append(';')
            .Append(' ');
        }

        _ = fullMemberNameBuilder.Append('}');
      }
      else if (memberKind.HasFlag(SymbolKind.Constructor) || memberKind.HasFlag(SymbolKind.Method) || memberKind.HasFlag(SymbolKind.Delegate))
      {
        _ = fullMemberNameBuilder.Append(')')
          .Append(';');
      }
      else if (memberKind.HasFlag(SymbolKind.Event) || memberKind.HasFlag(SymbolKind.Field))
      {
        _ = fullMemberNameBuilder.Append(';');
      }

      string fullMemberName = fullMemberNameBuilder.ToString();

      return fullMemberName;
    }

    /// <summary>
    /// Gets the access modifier for <see cref="MemberInfo"/> objects like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
    /// </summary>
    /// <param name="memberInfo"></param>
    /// <returns>The <see cref="AccessModifier"/> for the current <paramref name="memberInfo"/>.</returns>
    /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref name="memberInfo"/>.</exception>
    /// <exception cref="NotSupportedException">The type provided by the <paramref name="memberInfo"/> is not supported.</exception>
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
    /// Extension method to convert generic and non-generic member names to a readable display name without the namespace.
    /// </summary>
    /// <param name="memberInfo">The <see cref="MemberInfo"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type name like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this MemberInfo memberInfo)
      => ToDisplayNameInternal(memberInfo, isFullyQualifiedName: false);
    //{
    //  bool isMemberIndexerProperty = memberInfo is PropertyInfo propertyInfo && (propertyInfo.GetIndexParameters()?.Length ?? -1) > 0;
    //  StringBuilder memberNameBuilder = new StringBuilder(isMemberIndexerProperty
    //    ? $"{memberInfo.Name}[]"
    //    : memberInfo.Name);

    //  // Those member types can't be generic
    //  if (memberInfo.MemberType.HasFlag(MemberTypes.Field)
    //    || memberInfo.MemberType.HasFlag(MemberTypes.Property)
    //    || memberInfo.MemberType.HasFlag(MemberTypes.Event))
    //  {
    //    return memberNameBuilder.ToString();
    //  }

    //  int indexOfGenericTypeArgumentStart = memberInfo.Name.IndexOf('`');
    //  Type[] genericTypeArguments = Array.Empty<Type>();
    //  if (memberInfo is Type type)
    //  {
    //    if (!type.IsGenericType)
    //    {
    //      memberNameBuilder = BuildInheritanceSignature(memberNameBuilder, type, isFullyQualified: false);

    //      return memberNameBuilder.ToString();
    //    }

    //    genericTypeArguments = type.GetGenericArguments();
    //  }
    //  else if (memberInfo is MethodInfo methodInfo)
    //  {
    //    if (!methodInfo.IsGenericMethod)
    //    {
    //      return memberNameBuilder.ToString();
    //    }

    //    genericTypeArguments = methodInfo.GetGenericArguments();
    //  }
    //  else if (memberInfo is ConstructorInfo constructorInfo)
    //  {
    //    _ = memberNameBuilder.Clear()
    //      .Append(memberInfo.DeclaringType.Name);
    //    if (!constructorInfo.DeclaringType.IsGenericType)
    //    {
    //      return memberNameBuilder.ToString();
    //    }

    //    indexOfGenericTypeArgumentStart = memberNameBuilder.ToString().IndexOf('`');
    //  }

    //  _ = memberNameBuilder.Remove(indexOfGenericTypeArgumentStart, memberNameBuilder.Length - indexOfGenericTypeArgumentStart);
    //  memberNameBuilder = FinishTypeNameConstruction(memberNameBuilder, genericTypeArguments);
    //  if (memberInfo is Type superclass && superclass.BaseType != null)
    //  {
    //    memberNameBuilder = BuildInheritanceSignature(memberNameBuilder, superclass, isFullyQualified: false);
    //  }

    //  return memberNameBuilder.ToString();
    //}

    /// <summary>
    /// Extension method to convert generic and non-generic type names to a readable display name including the namespace.
    /// </summary>
    /// <param name="memberInfo">The <see cref="Type"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type name like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this MemberInfo memberInfo)
      => ToDisplayNameInternal(memberInfo, isFullyQualifiedName: true);
    //{
    //  StringBuilder fullMemberNameBuilder = new StringBuilder(memberInfo is Type typeInfo
    //    ? $"{typeInfo.Namespace}.{typeInfo.ToDisplayName()}"
    //    : $"{memberInfo.DeclaringType.Namespace}.{memberInfo.DeclaringType.ToDisplayName()}.{memberInfo.Name}");

    //  switch (memberInfo)
    //  {
    //    case Type type:
    //  }

    //  if (memberInfo.MemberType.HasFlag(MemberTypes.Field)
    //    || memberInfo.MemberType.HasFlag(MemberTypes.Property)
    //    || memberInfo.MemberType.HasFlag(MemberTypes.Event))
    //  {
    //    return fullMemberNameBuilder.ToString();
    //  }

    //  int indexOfGenericTypeArgumentStart = fullMemberNameBuilder.ToString().IndexOf('`');
    //  Type[] genericTypeArguments = Array.Empty<Type>();
    //  if (memberInfo is Type type)
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
    //  else if (memberInfo is MethodInfo methodInfo)
    //  {
    //    if (!methodInfo.IsGenericMethod)
    //    {
    //      return fullMemberNameBuilder.ToString();
    //    }

    //    genericTypeArguments = methodInfo.GetGenericArguments();
    //  }
    //  else if (memberInfo is ConstructorInfo constructorInfo)
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
    //  if (memberInfo is Type superclass && superclass.BaseType != null)
    //  {
    //    fullMemberNameBuilder = BuildInheritanceSignature(fullMemberNameBuilder, superclass, isFullyQualified: true);
    //  }

    //  return fullMemberNameBuilder.ToString();
    //}

    /// <summary>
    /// Extension method to convert generic and non-generic type names to a readable display name including the namespace.
    /// </summary>
    /// <param name="memberInfo">The <see cref="Type"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type name like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    private static string ToDisplayNameInternal(MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
    {
      StringBuilder fullMemberNameBuilder = new StringBuilder();
      if (memberInfo is Type typeInfo)
      {
        AppendDisplayNameInternal(fullMemberNameBuilder, typeInfo, isFullyQualifiedName, isShortName);
      }
      else
      {
        _ = fullMemberNameBuilder.Append(memberInfo.DeclaringType.Namespace)
          .AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isShortName: false);
        if (memberInfo is ConstructorInfo)
        {          
          _ = fullMemberNameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isShortName: true)
        }
        else
        {
          fullMemberNameBuilder.Append(memberInfo.Name);
        }
      }

      string symbolName;
      
      // Assembly full name manually for improved efficiency (to avoid string operations to cut-off the generic type part)
      switch (memberInfo)
      {
        // For example: System.Threading.Task
        case Type type:
          symbolName = isFullyQualifiedName 
            ? $"{type.Namespace}.{type.Name}" 
            : type.Name;
          break;

        // For example: System.Threading.Task.Run
        default:
         symbolName = isFullyQualifiedName 
            ? $"{memberInfo.DeclaringType.Namespace}.{ToDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName)}.{memberInfo.Name}" 
            : memberInfo.Name;
          break;
      }

      _ = fullMemberNameBuilder.Append(symbolName);

      if (memberInfo.MemberType.HasFlag(MemberTypes.Field)
        || memberInfo.MemberType.HasFlag(MemberTypes.Property)
        || memberInfo.MemberType.HasFlag(MemberTypes.Event)
        || memberInfo.MemberType.HasFlag(MemberTypes.Constructor))
      {
        return fullMemberNameBuilder.ToString();
      }

      Type[] genericTypeArguments = Array.Empty<Type>();
      if (memberInfo is Type typeDeclaration)
      {
        if (!typeDeclaration.IsGenericType)
        {
          if (!typeDeclaration.IsEnum)
          {
            fullMemberNameBuilder = BuildInheritanceSignature(fullMemberNameBuilder, typeDeclaration, isFullyQualifiedName);
          }

          return fullMemberNameBuilder.ToString();
        }

        genericTypeArguments = typeDeclaration.GetGenericArguments();
      }
      else if (memberInfo is MethodInfo methodInfo)
      {
        if (!methodInfo.IsGenericMethod)
        {
          return fullMemberNameBuilder.ToString();
        }

        genericTypeArguments = methodInfo.GetGenericArguments();
      }
      //else if (memberInfo is ConstructorInfo constructorInfo)
      //{
      //  //_ = fullMemberNameBuilder.Clear()
      //  //  .Append($"{constructorInfo.DeclaringType.Namespace}.{constructorInfo.DeclaringType.ToDisplayName()}.{constructorInfo.DeclaringType.Name}");
      //  if (!constructorInfo.DeclaringType.IsGenericType)
      //  {
      //    return fullMemberNameBuilder.ToString();
      //  }
      //}

      fullMemberNameBuilder = FinishTypeNameConstruction(fullMemberNameBuilder, genericTypeArguments);
      if (memberInfo is Type superclass && superclass.BaseType != null)
      {
        fullMemberNameBuilder = BuildInheritanceSignature(fullMemberNameBuilder, superclass, isFullyQualified: true);
      }

      return fullMemberNameBuilder.ToString();
    }

    public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, Type type, bool isFullyQualifiedName = false)
      => AppendDisplayNameInternal(nameBuilder, type, isFullyQualifiedName, isShortName: false);

    public static StringBuilder AppendShortDisplayName(this StringBuilder nameBuilder, Type type, bool isFullyQualifiedName = false)
      => AppendDisplayNameInternal(nameBuilder, type, isFullyQualifiedName, isShortName: true);

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, Type type, bool isFullyQualifiedName, bool isShortName)
    {
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
        _ = nameBuilder.Append('<');

        Type[] typeArguments = type.GetGenericArguments();
        foreach (Type typeArgument in typeArguments)
        {
          _ = nameBuilder.AppendDisplayName(typeArgument, isFullyQualifiedName)
            .Append(ParameterSeparator);
        }

        _ = nameBuilder.Remove(nameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length)
          .Append('>');
      }

      return nameBuilder;
    }

    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isShortName)
    {
      if (memberInfo is MethodInfo methodInfo)
      {
        var method = new CodeMemberMethod();
        if (methodInfo.IsGenericMethod)
        {
          int genericParameterPlaceholderIndex = methodInfo.Name.IndexOf('`');
          method.Name = methodInfo.Name.Substring(0, genericParameterPlaceholderIndex);

          var typeArguments = methodInfo.GetGenericArguments();
          foreach (Type typeArgument in typeArguments)
          {
            CodeTypeParameter typeParameter = new CodeTypeParameter(typeArgument);
            method.TypeParameters.Add(typeParameter);
          }
        }
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
        _ = nameBuilder.Append('<');

        Type[] typeArguments = type.GetGenericArguments();
        foreach (Type typeArgument in typeArguments)
        {
          _ = nameBuilder.AppendDisplayName(typeArgument, isFullyQualifiedName)
            .Append(ParameterSeparator);
        }

        _ = nameBuilder.Remove(nameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length)
          .Append('>');
      }

      return nameBuilder;
    }

    private static CodeTypeParameter CreateCodeTypeParameter(Type typeParameter)
    {
      if (typeParameter.IsGenericParameter)
      {
        var codeTypeParameter = new CodeTypeParameter().
}
      else if (typeParameter.IsGenericType)
      {
        var codeTypeReference = new CodeTypeReference(typeParameter.ToDisplayName());

        Type[] typeArguments = typeParameter.GetGenericArguments();
        foreach (Type typeArgument in typeArguments)
        {
          CodeTypeReference typeParameter = CreateCodeTypeReference(typeArgument);
          codeTypeReference.TypeArguments.Add(typeParameter);
        }

      }
    }

    private static CodeTypeReference CreateCodeTypeReference(Type typeParameter)
    {
       var typeReference = new CodeTypeReference(typeParameter.ToDisplayName());
      if (typeParameter.IsGenericType)
      {
        Type[] typeArguments = typeParameter.GetGenericArguments();
        foreach (Type typeArgument in typeArguments)
        {
          CodeTypeReference nestedTypeParameter = CreateCodeTypeReference(typeArgument);
          typeReference.TypeArguments.Add(nestedTypeParameter);
        }
      }

      return typeReference;
    }

    private static StringBuilder FinishTypeNameConstruction(StringBuilder nameBuilder, Type[] genericTypeArguments)
    {
      if (!genericTypeArguments.Any())
      {
        return nameBuilder;
      }

      _ = nameBuilder.Append('<');
      foreach (Type genericParameterType in genericTypeArguments)
      {
        var typeReference = new CodeTypeReference(genericParameterType);
        _ = nameBuilder.Append(HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference))
          .Append(ParameterSeparator);
      }

      // Remove trailing comma and whitespace
      _ = nameBuilder.Remove(nameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length)
        .Append('>');

      return nameBuilder;
    }

    private static StringBuilder BuildInheritanceSignature(StringBuilder memberNameBuilder, Type type, bool isFullyQualified)
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
            .Append(ParameterSeparator);
        }

        foreach (Type interfaceInfo in interfaces)
        {
          _ = memberNameBuilder.Append(isFullyQualified ? interfaceInfo.FullName : interfaceInfo.Name)
            .Append(ParameterSeparator);
        }

        if (isSubclass || hasInterfaces)
        {
          _ = memberNameBuilder.Remove(memberNameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length);
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
    /// <param name="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
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
        foreach (TypeInfo typeInfo in assembly.GetExportedTypes())
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
    /// <param name="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref name="typeInfo"/> is static. Otherwise <see langword="false"/>.</returns>
    public static bool IsStatic(this Type typeInfo)
      => typeInfo.IsAbstract && typeInfo.IsSealed;

    /// <summary>
    /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
    /// </summary>
    /// <param name="typeInfo">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref name="typeInfo"/> is allowed to define extension methods. Otherwise <see langword="false"/>.</returns>
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
    /// <param name="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the <paramref name="methodInfo"/> is an extension method. Otherwise <see langword="false"/>.</returns>
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
    /// <param name="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <param name="typeToExtend">The <see cref="Type"/> the <paramref name="methodInfo"/> is expected to extend.</param>
    /// <returns><see langword="true"/> if the <paramref name="methodInfo"/> is an extension method for <paramref name="typeToExtend"/>. Otherwise <see langword="false"/>.</returns>
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

    private static SymbolKind GetKind(this MemberInfo memberInfo)
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
        return SymbolKind.Delegate;
      }

      bool isClass = !isDelegate && (type?.IsClass ?? false);
      if (isClass)
      {
        SymbolKind classKind = SymbolKind.Class;
        if (type.IsAbstract)
        {
          classKind |= SymbolKind.Abstract;
        }

        if (type.IsSealed)
        {
          classKind |= SymbolKind.Final;
        }

        if (type.IsStatic())
        {
          classKind |= SymbolKind.Static;
        }

        return classKind;
      }

      bool isEnum = !isDelegate && (type?.IsEnum ?? false);
      if (isEnum)
      {
        return SymbolKind.Enum;
      }

      bool isStruct = !isDelegate && (type?.IsValueType ?? false);
      if (isStruct)
      {
        SymbolKind structKind = SymbolKind.Struct;

#if NETSTANDARD2_1_OR_GREATER || NET471_OR_GREATER || NET
        bool isReadOnlyStruct  = isStruct && type.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null;
        if (isReadOnlyStruct)
        {
          structKind |= SymbolKind.Final;
        }
#endif
        return structKind;
      }

      bool isProperty = propertyInfo != null;
      if (isProperty)
      {
        bool isIndexerProperty = indexerPropertyIndexParameters.Length > 0;
        SymbolKind propertyKind = isIndexerProperty 
          ? SymbolKind.IndexerProperty 
          : SymbolKind.Property;

        MethodInfo getMethod = propertyInfo.GetGetMethod();
        if (!propertyInfo.CanWrite)
        {
          propertyKind |= SymbolKind.Final;
        }

        if (getMethod.IsAbstract)
        {
          propertyKind |= SymbolKind.Abstract;
        }

        if (getMethod.IsStatic)
        {
          propertyKind |= SymbolKind.Static;
        }

        if (getMethod.IsVirtual)
        {
          propertyKind |= SymbolKind.Virtual;
        }

        if (getMethod.IsOverride())
        {
          propertyKind |= SymbolKind.Override;
        }

        return propertyKind;
      }

      bool isMethod = !isDelegate && !isClass && memberInfo.MemberType.HasFlag(MemberTypes.Method);
      if (isMethod)
      {
        SymbolKind methodKind = SymbolKind.Method;
        if (methodInfo.IsFinal)
        {
          methodKind |= SymbolKind.Final;
        }

        if (methodInfo.IsAbstract)
        {
          methodKind |= SymbolKind.Abstract;
        }

        if (methodInfo.IsStatic)
        {
          methodKind |= SymbolKind.Static;
        }

        if (methodInfo.IsVirtual)
        {
          methodKind |= SymbolKind.Virtual;
        }

        if (methodInfo.IsOverride())
        {
          methodKind |= SymbolKind.Override;
        }

        return methodKind;
      }

      bool isEvent = eventInfo != null;
      if (isEvent)
      {
        SymbolKind eventKind = SymbolKind.Event;
        MethodInfo addHandlerMethod = eventInfo.GetAddMethod(true);
        if (addHandlerMethod.IsFinal)
        {
          eventKind |= SymbolKind.Final;
        }

        if (addHandlerMethod.IsAbstract)
        {
          eventKind |= SymbolKind.Abstract;
        }

        if (addHandlerMethod.IsStatic)
        {
          eventKind |= SymbolKind.Static;
        }

        if (addHandlerMethod.IsVirtual)
        {
          eventKind |= SymbolKind.Virtual;
        }

        if (addHandlerMethod.IsOverride())
        {
          eventKind |= SymbolKind.Override;
        }

        return eventKind;
      }

      bool isConstructor = constructorInfo != null;
      if (isConstructor)
      {
        SymbolKind constructorKind = SymbolKind.Constructor;

        if (constructorInfo.IsStatic)
        {
          constructorKind |= SymbolKind.Static;
        }

        return constructorKind;
      }

      bool isField = fieldInfo != null;
      if (isField)
      {
        SymbolKind fieldKind = SymbolKind.Event;
        if (fieldInfo.IsInitOnly)
        {
          fieldKind |= SymbolKind.Final;
        }

        if (fieldInfo.IsStatic)
        {
          fieldKind |= SymbolKind.Static;
        }

        return fieldKind;
      }

      bool isInterface = !isDelegate && !isClass && (type?.IsInterface ?? false);
      if (isInterface)
      {
        SymbolKind interfaceKind = SymbolKind.Interface;
        return interfaceKind;
      }

      return SymbolKind.Undefined;
    }

    public static dynamic Cast(this object obj, Type type)
        => typeof(HelperExtensionsCommon).GetMethod(nameof(HelperExtensionsCommon.Cast), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object) }, null).GetGenericMethodDefinition().MakeGenericMethod(type).Invoke(obj, null);

    private static T Cast<T>(this object obj) => (T)obj;

#if !NET7_0_OR_GREATER
    public static double TotalMicroseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E6, 1);
    public static double TotalNanoseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E9, 0);
#endif
  }

  [Flags]
  public enum SymbolKind
  {
    Undefined = 0,
    Final = 1,
    Virtual = 2,
    Abstract = 4,
    Static = 8,
    Class = 16,
    SealedClass = Final | Class,
    AbstractClass = Abstract | Class,
    StaticClass = Static | Class,
    Interface = 32,
    Delegate = 64,
    Struct = 128,
    ReadOnlyStruct = Final | Struct,
    Enum = 256,
    Method = 512,
    VirtualMethod = Virtual | Method,
    AbstractMethod = Abstract | Method,
    StaticMethod = Static | Method,
    SealedOverrideMethod = Final | Override | Method,
    OverrideMethod = Override | Method,
    Property = 1024,
    ReadOnlyProperty = Final | Property,
    AbstractProperty = Abstract | Property,
    AbstractReadOnlyProperty = Abstract | Final | Property,
    StaticProperty = Static | Property,
    StaticReadOnlyProperty = Static | Final | Property,
    VirtualProperty = Virtual | Property,
    VirtualReadOnlyProperty = Virtual| Final | Property,
    OverrideProperty = Override | Property,
    OverrideReaOnlyProperty = Override | Final | Property,
    Field = 2048,
    ReadOnlyField = Final | Field,
    StaticField = Static | Property,
    StaticReadOnlyField = Static | Final | Field,
    Event = 4096,
    VirtualEvent = Virtual | Event,
    AbstractEvent = Abstract | Event,
    OverrideEvent = Override | Event,
    Constructor = 8192,
    StaticConstructor = Static | Constructor,
    IndexerProperty = 16384 | Property,
    ReadOnlyIndexerProperty = Final | IndexerProperty,
    OverrideReadOnlyIndexerProperty = Override | Final | IndexerProperty,
    OverrideIndexerProperty = Override| IndexerProperty,
    StaticIndexerProperty = Static | IndexerProperty,
    StaticReadOnlyIndexerProperty = Static | Final | IndexerProperty,
    AbstractIndexerProperty = Abstract | IndexerProperty,
    AbstractReadOnlyIndexerProperty = Abstract | Final | IndexerProperty,
    Override = 32768,
  }
}