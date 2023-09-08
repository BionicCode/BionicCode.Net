﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.CodeDom;
  using System.Collections;
  using System.Collections.Generic;
  using System.Data.Common;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Net.PeerToPeer;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Text;
  using System.Threading.Tasks;
  using Microsoft.CSharp;
  using Microsoft.VisualBasic;
  using static System.Net.Mime.MediaTypeNames;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {
    /// <summary>
    /// The property name of an indexer property. This name is compiler generated and equals the value of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
    /// </summary>
    /// <value>The generated property name of an indexer is <c>Item</c>.</value>
    /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
    public static readonly string IndexerName = "Item";

    private static CSharpCodeProvider CodeProvider { get; } = new CSharpCodeProvider();

    /// <summary>
    /// Converts a <see cref="Predicate{T}"/> to a <see cref="Func{T, TResult}"/>.
    /// </summary>
    /// <typeparam name="TParam">The parameter type for the predicate.</typeparam>
    /// <param name="predicate">The predicate to convert.</param>
    /// <returns>A <c>Func<typeparamref name="TParam"/>, bool></c> that returns the result of <paramref name="predicate"/>.</returns>
    public static Func<TParam, bool> ToFunc<TParam>(this Predicate<TParam> predicate) => param => predicate.Invoke(param);

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
    public static string ToDisplaySignatureName(this PropertyInfo propertyInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)propertyInfo).ToDisplaySignatureName(isQualifyMemberEnabled);

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
    public static string ToDisplaySignatureName(this MethodInfo methodInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)methodInfo).ToDisplaySignatureName(isQualifyMemberEnabled);

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
    public static string ToDisplaySignatureName(this Type typeInfo)
      => ((MemberInfo)typeInfo).ToDisplaySignatureName(false);

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
    public static string ToDisplaySignatureName(this ConstructorInfo constructorInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)constructorInfo).ToDisplaySignatureName(isQualifyMemberEnabled);

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
    public static string ToDisplaySignatureName(this FieldInfo fieldInfo, bool isQualifyMemberEnabled = false)
      => ((MemberInfo)fieldInfo).ToDisplaySignatureName(isQualifyMemberEnabled);

    internal static string ToDisplaySignatureName(this MemberInfo memberInfo, bool isQualifyMemberEnabled = false)
    {
      Type type = memberInfo as Type;
      PropertyInfo propertyInfo = memberInfo as PropertyInfo;
      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        ?? type?.GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic); // MemberInfo is delegate
      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
      ConstructorInfo constructorInfo = memberInfo as ConstructorInfo;
      FieldInfo fieldInfo = memberInfo as FieldInfo;
      EventInfo eventInfo = memberInfo as EventInfo;
      MethodInfo eventAddMethodInfo = eventInfo?.GetAddMethod(true);
      FieldInfo eventDeclaredFieldInfo = eventInfo?.DeclaringType.GetField(eventInfo.Name, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

      bool isProperty = memberInfo.MemberType.HasFlag(MemberTypes.Property);
      bool isMethod = memberInfo.MemberType.HasFlag(MemberTypes.Method);
      bool isEvent = memberInfo.MemberType.HasFlag(MemberTypes.Event);
      bool isConstructor = memberInfo.MemberType.HasFlag(MemberTypes.Constructor);
      bool isField = memberInfo.MemberType.HasFlag(MemberTypes.Field);
      bool isClass = type?.IsClass ?? false;
      bool isInterface = type?.IsInterface ?? false;
      bool isDelegate = type?.IsDelegate() ?? false;
      bool isIndexerProperty = indexerPropertyIndexParameters.Length > 0;
      bool propertyHasGet = propertyGetMethodInfo != null;
      bool propertyHasSet = propertySetMethodInfo != null;

      var fullMemberNameBuilder = new StringBuilder();

      AccessModifier accessModifier = memberInfo.GetAccessModifier();
      _ = fullMemberNameBuilder
        .Append(accessModifier.ToDisplayStringValue())
        .Append(' ');

      if ((methodInfo?.IsFinal ?? false)
        || (constructorInfo?.IsFinal ?? false)
        || (propertyGetMethodInfo?.IsFinal ?? false)
        || (propertySetMethodInfo?.IsFinal ?? false))
      {
        _ = fullMemberNameBuilder
          .Append("sealed")
          .Append(' ');
      }

      if ((methodInfo?.IsStatic ?? false)
        || (constructorInfo?.IsStatic ?? false)
        || (propertyGetMethodInfo?.IsStatic ?? false)
        || (propertySetMethodInfo?.IsStatic ?? false)
        || (fieldInfo?.IsStatic ?? false)
        || (eventAddMethodInfo?.IsStatic ?? false))
      {
        _ = fullMemberNameBuilder
          .Append("static")
          .Append(' ');
      }


      bool isAbstract = false;
      if ((methodInfo?.IsAbstract ?? false)
        || (constructorInfo?.IsAbstract ?? false)
        || (propertyGetMethodInfo?.IsAbstract ?? false)
        || (propertySetMethodInfo?.IsAbstract ?? false)
        || (eventAddMethodInfo?.IsAbstract ?? false))
      {
        isAbstract = true;
        _ = fullMemberNameBuilder
          .Append("abstract")
          .Append(' ');
      }

      if (!isAbstract
        && ((methodInfo?.IsVirtual ?? false)
        || (constructorInfo?.IsVirtual ?? false)
        || (propertyGetMethodInfo?.IsVirtual ?? false)
        || (propertySetMethodInfo?.IsVirtual ?? false)
        || (eventAddMethodInfo?.IsVirtual ?? false)))
      {
        _ = fullMemberNameBuilder
          .Append("virtual")
          .Append(' ');
      }

      if (isClass)
      {
        _ = fullMemberNameBuilder
          .Append("class")
          .Append(' ');
      }

      if (isInterface)
      {
        _ = fullMemberNameBuilder
          .Append("interface")
          .Append(' ');
      }

      if (isDelegate)
      {
        _ = fullMemberNameBuilder
          .Append("delegate")
          .Append(' ');
      }

      if (isEvent)
      {
        _ = fullMemberNameBuilder
          .Append("event")
          .Append(' ');
      }

      if ((methodInfo?.IsOverride() ?? false)
        || (propertyGetMethodInfo?.IsOverride() ?? false)
        || (propertySetMethodInfo?.IsOverride() ?? false)
        || (eventAddMethodInfo?.IsOverride() ?? false))
      {
        _ = fullMemberNameBuilder
          .Append("override")
          .Append(' ');
      }

      // Set return type
      if (isMethod
        || isProperty
        || isField
        || isDelegate
        || isEvent)
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

      if (isIndexerProperty)
      {
        _ = fullMemberNameBuilder
          .Append("this")
          .Append('[');
      }
      else if (isConstructor)
      {
        _ = fullMemberNameBuilder.Append(constructorInfo.ToDisplayName());
      }
      else
      {
        // Set actual qualified member name
        if (isQualifyMemberEnabled && memberInfo.DeclaringType != null)
        {
          _ = fullMemberNameBuilder.Append(memberInfo.DeclaringType.ToDisplayName())
            .Append('.');
        }

        _ = fullMemberNameBuilder.Append(memberInfo.ToDisplayName());
      }

      if (isConstructor || isMethod)
      {
        _ = fullMemberNameBuilder.Append('(');

        if (isMethod && (methodInfo?.IsExtensionMethod() ?? false))
        {
          _ = fullMemberNameBuilder
            .Append("this")
            .Append(' ');
        }
      }
      else if (isProperty && !isIndexerProperty)
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
      }

      IEnumerable<ParameterInfo> paramters = methodInfo?.GetParameters()
        ?? constructorInfo?.GetParameters()
        ?? indexerPropertyIndexParameters
        ?? Enumerable.Empty<ParameterInfo>();

      if (paramters.Any())
      {
        foreach (ParameterInfo parameter in paramters)
        {
          var typeReference = new CodeTypeReference(parameter.ParameterType);
          _ = fullMemberNameBuilder
            .Append(HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference))
            .Append(' ')
            .Append(parameter.Name)
            .Append(',')
            .Append(' ');
        }

        // Remove trailing comma and whitespace
        _ = fullMemberNameBuilder.Remove(fullMemberNameBuilder.Length - 2, 2);
      }

      if (isIndexerProperty)
      {
        _ = fullMemberNameBuilder.Append(']');
      }
      else if (isConstructor || isMethod)
      {
        _ = fullMemberNameBuilder.Append(')')
          .Append(';');
      }
      else if (isProperty)
      {
        _ = fullMemberNameBuilder.Append('}');
      }
      else if (isEvent || isField)
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
    /// <remarks>For a <see cref="PropertyInfo"/> the property accessor with the least restriction provides the access modifier for the property. This is acompiler rule.</remarks>
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
            : throw new InvalidOperationException("Unable to identify the accessibility of the Type.");
        case MethodBase methodBaseInfo:
          return methodBaseInfo.IsPublic ? AccessModifier.Public
            : methodBaseInfo.IsPrivate ? AccessModifier.Private
            : methodBaseInfo.IsAssembly ? AccessModifier.Internal
            : methodBaseInfo.IsFamily ? AccessModifier.Protected
            : methodBaseInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
            : methodBaseInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
            : throw new InvalidOperationException("Unable to identify the accessibility of the Type.");
        case FieldInfo fieldInfo:
          return fieldInfo.IsPublic ? AccessModifier.Public
            : fieldInfo.IsPrivate ? AccessModifier.Private
            : fieldInfo.IsAssembly ? AccessModifier.Internal
            : fieldInfo.IsFamily ? AccessModifier.Protected
            : fieldInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
            : fieldInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
            : throw new InvalidOperationException("Unable to identify the accessibility of the Type.");
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
      // Property accessor with the least restriction provides the access modifier for the property.
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
    /// <para>Because <see cref="Type"/> dervies from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type name like <c>"Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToDisplayName(this MemberInfo memberInfo)
    {
      string memberName = memberInfo.Name;

      // Those member types can't be generic
      if (memberInfo.MemberType.HasFlag(MemberTypes.Field)
        || memberInfo.MemberType.HasFlag(MemberTypes.Property)
        || memberInfo.MemberType.HasFlag(MemberTypes.Event))
      {
        return memberName;
      }

      Type[] genericTypeArguments = Array.Empty<Type>();
      if (memberInfo is Type type)
      {
        if (!type.IsGenericType)
        {
          return memberName;
        }

        genericTypeArguments = type.GetGenericArguments();
      }
      else if (memberInfo is MethodInfo methodInfo)
      {
        if (!methodInfo.IsGenericMethod)
        {
          return memberName;
        }

        genericTypeArguments = methodInfo.GetGenericArguments();
      }
      else if (memberInfo is ConstructorInfo constructorInfo)
      {
        memberName = memberInfo.DeclaringType.Name;
        if (!constructorInfo.DeclaringType.IsGenericType)
        {
          return memberName;
        }
      }

      memberName = memberName
        .Split(new[] { '`' }, StringSplitOptions.RemoveEmptyEntries)
        .First()
        .Trim();

      return FinishTypeNameConstruction(memberName, genericTypeArguments);
    }

    /// <summary>
    /// Extension method to convert generic and non-generic type names to a readable display name including the namespace.
    /// </summary>
    /// <param name="memberInfo">The <see cref="Type"/> to extend.</param>
    /// <returns>
    /// A readable name of type members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// <para>Because <see cref="Type"/> dervies from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
    /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
    /// <br/>This helper unwraps the generic type parameters to construct the full type name like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
    /// </remarks>
    public static string ToFullDisplayName(this MemberInfo memberInfo)
    {
      string fullMemberName = memberInfo is Type typeInfo
        ? $"{typeInfo.Namespace}.{typeInfo.ToDisplayName()}"
        : $"{memberInfo.DeclaringType.Namespace}.{memberInfo.DeclaringType.ToDisplayName()}.{memberInfo.Name}";

      if (memberInfo.MemberType.HasFlag(MemberTypes.Field)
        || memberInfo.MemberType.HasFlag(MemberTypes.Property)
        || memberInfo.MemberType.HasFlag(MemberTypes.Event))
      {
        return fullMemberName;
      }

      Type[] genericTypeArguments = Array.Empty<Type>();
      if (memberInfo is Type type)
      {
        fullMemberName = type.FullName;
        if (!type.IsGenericType)
        {
          return fullMemberName;
        }

        genericTypeArguments = type.GetGenericArguments();
      }
      else if (memberInfo is MethodInfo methodInfo)
      {
        if (!methodInfo.IsGenericMethod)
        {
          return fullMemberName;
        }

        genericTypeArguments = methodInfo.GetGenericArguments();
      }
      else if (memberInfo is ConstructorInfo constructorInfo)
      {
        fullMemberName = $"{constructorInfo.DeclaringType.Namespace}.{constructorInfo.DeclaringType.ToDisplayName()}.{constructorInfo.DeclaringType.Name}";
        if (!constructorInfo.DeclaringType.IsGenericType)
        {
          return fullMemberName;
        }
      }

      fullMemberName = fullMemberName
        .Split(new[] { '`' }, StringSplitOptions.RemoveEmptyEntries)
        .First()
        .Trim();

      return FinishTypeNameConstruction(fullMemberName, genericTypeArguments);
    }

    private static string FinishTypeNameConstruction(string typeName, Type[] genericTypeArguments)
    {
      if (!genericTypeArguments.Any())
      {
        return typeName;
      }

      StringBuilder nameBuilder = new StringBuilder(typeName)
        .Append('<');

      foreach (Type genericParameterType in genericTypeArguments)
      {
        var typeReference = new CodeTypeReference(genericParameterType);
        _ = nameBuilder.Append(HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference))
          .Append(',')
          .Append(' ');
      }

      // Remove trailing comma and whitespace
      _ = nameBuilder.Remove(nameBuilder.Length - 2, 2)
        .Append('>');

      return nameBuilder.ToString();
    }

    private static Lazy<Type> DelegateType { get; } = new Lazy<Type>(() => typeof(Delegate));
    public static bool IsDelegate(this Type typeInfo)
      => HelperExtensionsCommon.DelegateType.Value.IsAssignableFrom(typeInfo);

    // TODO::Test if checking get() is enough to determine if a property is overriden
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
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asnychronous/awaitable method.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return type is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned type (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate type (awaiter).
    /// <br/>If that fails too, it checks whther there exists any extension method named "GetAwaiter" for the returned type that would make the type awaitable. If this fails too, the method is not awaitable.</remarks>
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
    /// <param name="typeInfo">The extended <see cref="Type"/> istance.</param>
    /// <returns><see langword="true"/> if the <paramref name="typeInfo"/> is static. Otherwise <see langword="false"/>.</returns>
    public static bool IsStatic(this Type typeInfo)
      => typeInfo.IsAbstract && typeInfo.IsSealed;

    /// <summary>
    /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
    /// </summary>
    /// <param name="typeInfo">The extended <see cref="Type"/> istance.</param>
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
    /// <param name="methodInfo">The extended <see cref="MethodInfo"/> istance to validate.</param>
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
    /// <param name="methodInfo">The extended <see cref="MethodInfo"/> istance to validate.</param>
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

    public static dynamic Cast(this object obj, Type type)
        => typeof(HelperExtensionsCommon).GetMethod(nameof(HelperExtensionsCommon.Cast), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object) }, null).GetGenericMethodDefinition().MakeGenericMethod(type).Invoke(obj, null);

    private static T Cast<T>(this object obj) => (T)obj;

#if !NET7_0_OR_GREATER
    public static double TotalMicroseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E6, 1);
    public static double TotalNanoseconds(this TimeSpan duration) => System.Math.Round(duration.Ticks / (double)Stopwatch.Frequency * 1E9, 0);
#endif
  }
}
