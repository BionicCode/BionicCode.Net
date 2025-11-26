namespace BionicCode.Utilities.Net
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CSharp;

    /// <summary>
    /// A collection of extension methods for various default constraintTypes
    /// </summary>
    public static partial class HelperExtensionsCommon
    {
        private const string ParameterSeparator = ", ";
        private const char ExpressionTerminator = ';';

        private static readonly Type ValueTaskType = typeof(ValueTask);
        private static readonly Type ValueTaskGenericType = typeof(ValueTask<>);
        private static readonly Type TaskType = typeof(Task);
        private static readonly Type AsyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);
        private static readonly Type ExtensionAttributeType = typeof(ExtensionAttribute);
        private static readonly Type DelegateType = typeof(Delegate);
        private static readonly Type IsReadOnlyAttributeType = typeof(IsReadOnlyAttribute);

        /// <summary>
        /// The property genericTypeParameterIdentifier of an indexer property. This genericTypeParameterIdentifier is compiler generated and equals the typeName of the <see langword="static"/>field <see cref="System.Windows.Data.Binding.IndexerName" />.
        /// </summary>
        /// <typeName>The generated property genericTypeParameterIdentifier of an indexer is <c>Item</c>.</typeName>
        /// <remarks>This field exists to enable writing of cross-platform compatible reflection code without the requirement to import the PresentationFramework.dll.</remarks>
        public static readonly string IndexerName = "Item";

        private static readonly AccessModifierComparer AccessModifierComparer = new AccessModifierComparer();
        private static readonly CSharpCodeProvider CodeProvider = new CSharpCodeProvider();
        private static readonly FrozenSet<string> IgnorableParameterAttributes = new HashSet<string>
    {
      nameof(AsyncStateMachineAttribute),
      nameof(InAttribute),
      nameof(OutAttribute),
      nameof(DebuggerStepThroughAttribute),
      nameof(DebuggerBrowsableAttribute),
      nameof(DebuggerDisplayAttribute),
      nameof(DebuggerDisplayAttribute),
      nameof(DebuggerHiddenAttribute),
      nameof(DebuggerNonUserCodeAttribute),
      nameof(DebuggerStepperBoundaryAttribute),
      nameof(DebuggerTypeProxyAttribute),
      nameof(DebuggerVisualizerAttribute),
      //nameof(ProfileAttribute),
      //nameof(ProfilerMethodArgumentAttribute),
      //nameof(ProfilerPropertyArgumentAttribute),
      //nameof(ProfilerFactoryAttribute),
      nameof(IsReadOnlyAttribute),
    }.ToFrozenSet();

        /// <summary>
        /// Extension method to convert generic and non-generic method names to a readable full signature display name without the namespace.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol without the namespace, that includes the type, name, attributes, generic type constraints and parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c> and the method name <c>"Run"</c> becomes <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToRuntimeSignatureName(MethodInfo)"/>, this <see cref="ToSignatureName(MethodInfo)"/> method shows the defined generic type parameters (and not the runtime generic arguments)
        /// to construct the full runtime signature like <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));
            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.FullyQualifiedSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic method names to a readable runtime signature display name without the namespace and where all generic type parameters are replaced with their resolved runtime type arguments.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the return type, method name and parameters where all generic type parameters are replaced with their resolved runtime type arguments.
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the method name <c>"Run"</c> becomes <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(MethodInfo)"/>, this <see cref="ToRuntimeSignatureName(MethodInfo)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to return a readable full signature including the namespace.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, namespace, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"public void MyNamespace.MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : class"</c>.
        /// </returns>
        /// <remarks>
        /// Use <see cref="ToRuntimeSignatureName(Type)"/> to replace the generic type names with the runtime type arguments.
        /// <br/>Use <see cref="ToSignatureShortName(Type)"/> to return the signature without the namespace.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this Type type, bool withRuntimeGenericTypeArguments = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.FullyQualifiedSignature;
        }

        /// <summary>
        /// Extension method to return a readable full signature without the namespace. All generic type parameters are replaced with their resolved runtime type arguments.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the access modifiers, type identifier, type name and inheritance list, where all generic type parameters are replaced with their resolved runtime type arguments.
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the class name <c>"Task"</c> becomes <c>"public class Task&lt;int&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <br/>Use <see cref="ToRuntimeSignatureShortName(Type)"/> to return the the same runtime signature without the namespace.
        /// Use <see cref="ToSignatureName(Type)"/> and  <see cref="ToSignatureShortName(Type)"/> to return the signature with generic type parameter names.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c> and the field name <c>"currentTask"</c> becomes <c>"private static readonly Task&lt;TResult&gt; currentTask;"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToRuntimeSignatureName(FieldInfo)"/>, this <see cref="ToSignatureName(FieldInfo)"/> method shows the defined generic type parameters (and not the runtime generic arguments)
        /// to construct the full runtime signature like <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return fieldData.FullyQualifiedSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic field names to a readable runtime signature display name without the namespace and where all generic type parameters are replaced with their resolved runtime type arguments.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the access modifier, keywords, field type and field name and parameters where all generic type parameters are replaced with their resolved runtime type arguments.
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the field name <c>"currentTask"</c> becomes <c>"private static readonly Task&lt;int&gt; currentTask;"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(FieldInfo)"/>, this <see cref="ToRuntimeSignatureName(FieldInfo)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return fieldData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c> and the property name <c>"CurrentTask"</c> becomes <c>"public Task&lt;TResult&gt; CurrentTask { get; set; }"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToRuntimeSignatureName(PropertyInfo)"/>, this <see cref="ToSignatureName(PropertyInfo)"/> method shows the defined generic type parameters (and not the runtime generic arguments)
        /// to construct the full runtime signature like <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.FullyQualifiedSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic property names to a readable runtime signature display name without the namespace and where all generic type parameters are replaced with their resolved runtime type arguments.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the access modifier, property type, property name and accessors where all generic type parameters are replaced with their resolved runtime type arguments.
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the property name <c>"CurrentTask"</c> becomes <c>"public Task&lt;int&gt; CurrentTask { get; set; }"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(PropertyInfo)"/>, this <see cref="ToRuntimeSignatureName(PropertyInfo)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
        /// </summary>
        /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the constructor name <c>"Task"</c> becomes <c>"public Task(Func&lt;T&gt; func);"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToRuntimeSignatureName(ConstructorInfo)"/>, this <see cref="ToSignatureName(ConstructorInfo)"/> method shows the defined generic type parameters (and not the runtime generic arguments)
        /// to construct the full runtime signature like <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.FullyQualifiedSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the namespace.
        /// </summary>
        /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the constructor name <c>"Task"</c> becomes <c>"public Task(Func&lt;int&gt; func);"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(ConstructorInfo)"/>, this <see cref="ToRuntimeSignatureName(ConstructorInfo)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c> and the method name <c>"Run"</c> becomes <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToRuntimeSignatureName(MethodInfo)"/>, this <see cref="ToSignatureName(MethodInfo)"/> method shows the defined generic type parameters (and not the runtime generic arguments)
        /// to construct the full runtime signature like <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureShortName(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.ShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the method name <c>"Run"</c> becomes <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(MethodInfo)"/>, this <see cref="ToRuntimeSignatureName(MethodInfo)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureShortName(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to return a readable full signature without the namespace.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name, parameters and generic type parameters. 
        /// <br/>For example, <c>"public void MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : class"</c>.
        /// </returns>
        /// <remarks>
        /// Use <see cref="ToRuntimeSignatureName(Type)"/> and <see cref="ToRuntimeSignatureShortName(Type)"/> to replace the generic type names with the runtime type arguments.
        /// <br/>Use <see cref="ToSignatureShortName(Type)"/> to return the signature without the namespace.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
        public static string ToSignatureShortName(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.Signature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the class name <c>"Task"</c> becomes <c>"public class Task&lt;int&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(Type)"/>, this <see cref="ToRuntimeSignatureName(Type)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureShortName(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c> and the field name <c>"currentTask"</c> becomes <c>"private readonly Task&lt;TResult&gt; currentTask;"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToRuntimeSignatureName(FieldInfo)"/>, this <see cref="ToSignatureName(FieldInfo)"/> method shows the defined generic type parameters (and not the runtime generic arguments)
        /// to construct the full runtime signature like <c>"public Task&lt;TResult&gt; Task.Run&lt;TResult&gt;(Func&lt;Task&lt;TResult&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureShortName(this FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return fieldData.Signature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable full signature display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;int&gt;"</c> and the field name <c>"currentTask"</c> becomes <c>"private readonly Task&lt;int&gt; currentTask;"</c>.
        /// </returns>
        /// <remarks>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>Task.Run&lt;TResult&gt;</c> would return <c>"Task.Run`1"</c>, where the generic type parameters are replaced with a placeholder (e.g. `1). 
        /// <br/>Opposed to the counterpart <see cref="ToSignatureName(FieldInfo)"/>, this <see cref="ToRuntimeSignatureName(FieldInfo)"/> method replaces the generic type parameter placeholder with their resolved runtime arguments 
        /// to construct the full runtime signature like <c>"public Task&lt;int&gt; Task.Run&lt;int&gt;(Func&lt;Task&lt;int&gt;&gt; func);"</c>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureShortName(this FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return fieldData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace and the declaring type (in case of a member), but with attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public T MyProperty { get; set; }"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
        /// Use <see cref="ToRuntimeSignatureShortName(PropertyInfo)"/> to return a signature using the resolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureShortName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.ShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public T MyClass&lt;T&gt;.MyProperty { get; set; }"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
        /// Use <see cref="ToRuntimeSignatureName(PropertyInfo)"/> to return a signature using the resolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.Signature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public int MyProperty { get; set; }"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
        /// Use <see cref="ToSignatureShortName(PropertyInfo)"/> to return a signature using the unresolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureShortName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public int MyClass&lt;int&gt;.MyProperty { get; set; }"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
        /// Use <see cref="ToSignatureName(PropertyInfo)"/> to return a signature using the unresolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.RuntimeSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace and the declaring type (in case of a member), but with attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"public MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
        /// Use <see cref="ToRuntimeSignatureShortName(ConstructorInfo)"/> to return a signature using the resolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureShortName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.ShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"public MyClass&lt;TParam&gt;.MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
        /// Use <see cref="ToRuntimeSignatureName(ConstructorInfo)"/> to return a signature using the resolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.Signature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"public MyClass(Action&lt;int&gt; doSomething, [CallerMemberName] string value = null)"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
        /// Use <see cref="ToSignatureShortName(ConstructorInfo)"/> to return a signature using the unresolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureShortName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"public MyClass&lt;int&gt;.MyClass(Action&lt;int&gt; doSomething, [CallerMemberName] string value = null)"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
        /// Use <see cref="ToSignatureName(ConstructorInfo)"/> to return a signature using the unresolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.RuntimeSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace and the declaring type (in case of a member), but with attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;TEventArgs&gt; Completed;"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
        /// Use <see cref="ToRuntimeSignatureShortName(EventInfo)"/> to return a signature using the resolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureShortName(this EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return eventData.ShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;TEventArgs&gt; MyClass&lt;TEventArgs&gt;.Completed;"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
        /// Use <see cref="ToRuntimeSignatureName(EventInfo)"/> to return a signature using the resolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        public static string ToSignatureName(this EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return eventData.Signature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;CompletedEventArgs&gt; Completed;"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
        /// Use <see cref="ToSignatureShortName(EventInfo)"/> to return a signature using the unresolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureShortName(this EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return eventData.RuntimeShortSignature;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic symbols to a readable signature.
        /// <br/>The Signature will be generated without namespace, but with the declaring type (in case of a member), attributes and the resolved runtime generic type argument names.
        /// </summary>
        /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
        /// <returns>
        /// A readable signature of the symbol, that includes the type, name and parameters and also resolves generic type parameters. 
        /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;CompletedEventArgs&gt; MyClass&lt;CompletedEventArgs&gt;.Completed;"</c>.
        /// </returns>
        /// <remarks>
        /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
        /// Use <see cref="ToSignatureName(EventInfo)"/> to return a signature using the unresolved generic type parameters instead. Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
        public static string ToRuntimeSignatureName(this EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return eventData.RuntimeSignature;
        }

        #region REMOVE AFTER BENCHMARK COMPARISON!!!

        //    public static StringBuilder AppendSignatureName(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      SyntaxNode syntaxGraph = null;
        //      bool isTerminationRequested = false;
        //      if (memberInfo is MethodInfo methodInfo)
        //      {
        //        syntaxGraph = CreateMethodGraph(methodInfo, isFullyQualifiedName);
        //        isTerminationRequested = true;
        //      }

        //      if (syntaxGraph != null)
        //      {
        //        _ = nameBuilder.Append(syntaxGraph.ToString());
        //        if (isTerminationRequested)
        //        {
        //          _ = nameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
        //        }
        //      }

        //      return nameBuilder;
        //    }

        //    private static SyntaxNode CreateMethodGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
        //    {
        //      TypeSyntax returnType = SyntaxFactory.ParseTypeName(ToDisplayNameInternal(methodInfo.ReturnType, isFullyQualifiedName, isDeclaringTypeIncluded: false));
        //      string methodName = ToDisplayNameInternal(methodInfo, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        //      ParameterInfo[] parameters = methodInfo.GetParameters();
        //      foreach (ParameterInfo parameter in parameters)
        //      {
        //        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.EventName))
        //          .WithType(SyntaxFactory.IdentifierName(ToDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isDeclaringTypeIncluded: false)));

        //        if (parameter.IsRef())
        //        {
        //          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
        //        }
        //        else if (parameter.IsIn)
        //        {
        //          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
        //        }
        //        else if (parameter.IsOut)
        //        {
        //          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
        //        }
        //        //IList<CustomAttributeData> parameterAttributes = parameter.GetCustomAttributesData();
        //        //foreach(CustomAttributeData parameterAttribute in parameterAttributes)
        //        //{
        //        //  AttributeArgumentListSyntax argumentList = SyntaxFactory.AttributeArgumentList();

        //        //  IList<CustomAttributeTypedArgument> arguments = parameterAttribute.ConstructorArguments;
        //        //  foreach (CustomAttributeTypedArgument argument in arguments)
        //        //  {
        //        //    var argumentSyntax = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression)
        //        //  }
        //        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(ToDisplayNameInternal(attributeSyntax.EventName, isFullyQualifiedName, isDeclaringTypeIncluded: false)));
        //        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
        //        //}
        //        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
        //      }

        //      if (methodInfo.IsGenericMethod)
        //      {
        //        Type[] typeArguments = methodInfo.GetGenericArguments();
        //        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
        //        {
        //          Type typeArgument = typeArguments[typeArgumentIndex];
        //          //TypeParameterSyntax typeParameter = CreateMethodTypeParameter(typeArgument, isFullyQualifiedName);
        //          //methodGraph = methodGraph.AddTypeParameterListParameters(typeParameter);

        //          if (methodInfo.IsGenericMethodDefinition)
        //          {
        //            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
        //            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
        //            }

        //            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
        //            }

        //            Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();
        //            foreach (Type constraintType in constraintTypes)
        //            {
        //              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
        //              {
        //                continue;
        //              }

        //              string constraintName = ToDisplayNameInternal(constraintType, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
        //              constraints = constraints.Add(constraintSyntax);
        //            }

        //            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
        //            }

        //            string genericTypeParameterName = ToDisplayNameInternal(typeArgument, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
        //          }
        //        }
        //      }

        //      methodGraph = methodGraph.NormalizeWhitespace();
        //      return methodGraph;
        //    }

        //    private static SyntaxNode CreateDelegateGraph(MethodInfo methodInfo, bool isFullyQualifiedName)
        //    {
        //      TypeSyntax returnType = SyntaxFactory.ParseTypeName(ToDisplayNameInternal(methodInfo.ReturnType, isFullyQualifiedName, isDeclaringTypeIncluded: false));
        //      string methodName = ToDisplayNameInternal(methodInfo, isFullyQualifiedName, isDeclaringTypeIncluded: true);
        //      MethodDeclarationSyntax methodGraph = SyntaxFactory.MethodDeclaration(returnType, methodName)
        //        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

        //      ParameterInfo[] parameters = methodInfo.GetParameters();
        //      foreach (ParameterInfo parameter in parameters)
        //      {
        //        ParameterSyntax parameterSyntax = SyntaxFactory.Parameter(SyntaxFactory.Identifier(parameter.EventName))
        //          .WithType(SyntaxFactory.IdentifierName(ToDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isDeclaringTypeIncluded: false)));

        //        if (parameter.IsRef())
        //        {
        //          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
        //        }
        //        else if (parameter.IsIn)
        //        {
        //          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.InKeyword));
        //        }
        //        else if (parameter.IsOut)
        //        {
        //          parameterSyntax = parameterSyntax.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
        //        }
        //        //IList<CustomAttributeData> parameterAttributes = parameter.GetCustomAttributesData();
        //        //foreach(CustomAttributeData parameterAttribute in parameterAttributes)
        //        //{
        //        //  AttributeArgumentListSyntax argumentList = SyntaxFactory.AttributeArgumentList();

        //        //  IList<CustomAttributeTypedArgument> arguments = parameterAttribute.ConstructorArguments;
        //        //  foreach (CustomAttributeTypedArgument argument in arguments)
        //        //  {
        //        //    var argumentSyntax = SyntaxFactory.AttributeArgument(SyntaxFactory.ParseExpression)
        //        //  }
        //        //  AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.Identifier(ToDisplayNameInternal(attributeSyntax.EventName, isFullyQualifiedName, isDeclaringTypeIncluded: false)));
        //        //  parameterSyntax = parameterSyntax.AddAttributeLists(attributeSyntax);
        //        //}
        //        methodGraph = methodGraph.AddParameterListParameters(parameterSyntax);
        //      }

        //      if (methodInfo.IsGenericMethod)
        //      {
        //        Type[] typeArguments = methodInfo.GetGenericArguments();
        //        for (int typeArgumentIndex = 0; typeArgumentIndex < typeArguments.Length; typeArgumentIndex++)
        //        {
        //          Type typeArgument = typeArguments[typeArgumentIndex];
        //          TypeParameterSyntax typeParameter = CreateMethodTypeParameter(typeArgument, isFullyQualifiedName);
        //          methodGraph = methodGraph.AddTypeParameterListParameters(typeParameter);

        //          if (methodInfo.IsGenericMethodDefinition)
        //          {
        //            SeparatedSyntaxList<TypeParameterConstraintSyntax> constraints = SyntaxFactory.SeparatedList<TypeParameterConstraintSyntax>();
        //            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
        //            }

        //            if ((typeArgument.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
        //            }

        //            if (!typeArgument.IsValueType && (typeArgument.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
        //            {
        //              constraints = constraints.Add(SyntaxFactory.ConstructorConstraint());
        //            }

        //            Type[] constraintTypes = typeArgument.GetGenericParameterConstraints();
        //            foreach (Type constraintType in constraintTypes)
        //            {
        //              if (constraintType == typeof(object) || constraintType == typeof(ValueType))
        //              {
        //                continue;
        //              }

        //              string constraintName = ToDisplayNameInternal(constraintType, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //              TypeConstraintSyntax constraintSyntax = SyntaxFactory.TypeConstraint(SyntaxFactory.IdentifierName(constraintName));
        //              constraints = constraints.Add(constraintSyntax);
        //            }

        //            string genericTypeParameterName = ToDisplayNameInternal(typeArgument, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //            methodGraph = methodGraph.AddConstraintClauses(SyntaxFactory.TypeParameterConstraintClause(SyntaxFactory.IdentifierName(genericTypeParameterName), constraints));
        //          }
        //        }
        //      }

        //      methodGraph = methodGraph.NormalizeWhitespace(indentation: " ", elasticTrivia: true);
        //      return methodGraph;
        //    }

        //    private static TypeParameterSyntax CreateMethodTypeParameter(Type valueType, bool isFullyQualifiedName)
        //    {
        //      IEnumerable<Attribute> attributes = valueType.GetCustomAttributes();
        //      AttributeListSyntax attributeSyntaxList = SyntaxFactory.AttributeList();
        //      foreach (Attribute attribute in attributes)
        //      {
        //        string attributeName = ToDisplayNameInternal(attribute.GetType(), isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //        AttributeSyntax attributeSyntax = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));
        //        attributeSyntaxList = attributeSyntaxList.AddAttributes(attributeSyntax);
        //      }

        //      SyntaxKind variance = SyntaxKind.None;
        //      if (valueType.IsGenericParameter)
        //      {
        //        if ((valueType.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
        //        {
        //          variance = SyntaxKind.OutKeyword;
        //        }
        //        else if ((valueType.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
        //        {
        //          variance = SyntaxKind.InKeyword;
        //        }
        //      }

        //      string typeParameterName = ToDisplayNameInternal(valueType, isFullyQualifiedName, isDeclaringTypeIncluded: false);
        //      return SyntaxFactory.TypeParameter(new SyntaxList<AttributeListSyntax>() { attributeSyntaxList }, SyntaxFactory.Token(variance), SyntaxFactory.Identifier(typeParameterName));
        //    }

        //    private static string ToDisplayNameInternal(MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      string symbolName = new StringBuilder()
        //        .AppendDisplayNameInternal(memberInfo, isFullyQualifiedName, isDeclaringTypeIncluded)
        //        .ToString();

        //      return symbolName;
        //    }

        //    private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, Type valueType, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      var typeReference = new CodeTypeReference(valueType);
        //      ReadOnlySpan<char> typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).AsSpan();
        //      if (valueType.IsGenericType)
        //      {
        //        int startIndexOfGenericTypeParameters = typeName.IndexOf('<');
        //        typeName = typeName.Slice(0, startIndexOfGenericTypeParameters);
        //      }

        //      if (!isFullyQualifiedName)
        //      {
        //        int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
        //        if (startIndexOfUnqualifiedTypeName > 0)
        //        {
        //          typeName = typeName.Slice(startIndexOfUnqualifiedTypeName, typeName.Length - startIndexOfUnqualifiedTypeName);
        //        }
        //      }

        //      _ = nameBuilder.Append(typeName.ToArray());

        //      if (isDeclaringTypeIncluded)
        //      {
        //        return nameBuilder;
        //      }

        //      if (valueType.IsGenericType)
        //      {
        //        _ = nameBuilder.Append('<');

        //        Type[] typeArguments = valueType.GetGenericArguments();
        //        foreach (Type typeArgument in typeArguments)
        //        {
        //          _ = nameBuilder.AppendDisplayNameInternal(typeArgument, isFullyQualifiedName, isDeclaringTypeIncluded)
        //            .Append(ParameterSeparator);
        //        }

        //        _ = nameBuilder.Remove(nameBuilder.Length - ParameterSeparator.Length, ParameterSeparator.Length)
        //          .Append('>');
        //      }

        //      return nameBuilder;
        //    }

        //  private static StringBuilder AppendDisplayNameInternal(this StringBuilder nameBuilder, MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded)
        //    {
        //      if (memberInfo is Type valueType)
        //      {
        //        return nameBuilder.AppendDisplayNameInternal(valueType, isFullyQualifiedName, isDeclaringTypeIncluded);
        //      }

        //      if (isFullyQualifiedName)
        //      {
        //        _ = nameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isDeclaringTypeIncluded)
        //          .Append('.');
        //      }

        //      if (memberInfo.MemberType.HasFlag(MemberTypes.Constructor))
        //      {
        //        if (memberInfo.DeclaringType.IsGenericType)
        //        {
        //          int genericTypeArgumentPlaceholderIndex = memberInfo.DeclaringType.EventName.IndexOf('`');
        //          return nameBuilder.Append(memberInfo.DeclaringType.EventName, 0, genericTypeArgumentPlaceholderIndex);
        //        }
        //        else
        //        {
        //          return nameBuilder.Append(memberInfo.DeclaringType.EventName);
        //        }
        //      }
        //      else
        //      {
        //        return nameBuilder.Append(memberInfo.EventName);
        //      }
        //    }
        //    private static SymbolAttributes GetKind(this MemberInfo memberInfo)
        //    {
        //      var valueType = memberInfo as Type;
        //      var propertyInfo = memberInfo as PropertyInfo;
        //      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        //        ?? valueType?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
        //      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
        //      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
        //      var constructorInfo = memberInfo as ConstructorInfo;
        //      var fieldInfo = memberInfo as FieldInfo;
        //      var eventInfo = memberInfo as EventInfo;
        //      MethodInfo eventAddMethodInfo = eventInfo?.GetAddMethod(true);
        //      FieldInfo eventDeclaredFieldInfo = eventInfo?.DeclaringType.GetField(eventInfo.EventName, BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        //      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

        //      bool isDelegate = valueType?.IsDelegate() ?? false;
        //      if (isDelegate)
        //      {
        //        return SymbolAttributes.Delegate;
        //      }

        //      bool isClass = !isDelegate && (valueType?.IsClass ?? false);
        //      if (isClass)
        //      {
        //        SymbolAttributes classKind = SymbolAttributes.Class;
        //        if (valueType.IsAbstract)
        //        {
        //          classKind |= SymbolAttributes.Abstract;
        //        }

        //        if (valueType.IsSealed)
        //        {
        //          classKind |= SymbolAttributes.Final;
        //        }

        //        if (valueType.IsStatic())
        //        {
        //          classKind |= SymbolAttributes.Static;
        //        }

        //        return classKind;
        //      }

        //      bool isEnum = !isDelegate && (valueType?.IsEnum ?? false);
        //      if (isEnum)
        //      {
        //        return SymbolAttributes.Enum;
        //      }

        //      bool isStruct = !isDelegate && (valueType?.IsValueType ?? false);
        //      if (isStruct)
        //      {
        //        SymbolAttributes structKind = SymbolAttributes.Struct;

        //#if NETSTANDARD2_1_OR_GREATER || NET471_OR_GREATER || NET
        //        bool isReadOnlyStruct = isStruct && valueType.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null;
        //        if (isReadOnlyStruct)
        //        {
        //          structKind |= SymbolAttributes.Final;
        //        }
        //#endif
        //        return structKind;
        //      }

        //      bool isProperty = propertyInfo != null;
        //      if (isProperty)
        //      {
        //        bool isIndexerProperty = indexerPropertyIndexParameters.Length > 0;
        //        SymbolAttributes propertyKind = isIndexerProperty
        //          ? SymbolAttributes.IndexerProperty
        //          : SymbolAttributes.Property;

        //        MethodInfo getMethod = propertyInfo.GetGetMethod();
        //        if (!propertyInfo.CanWrite)
        //        {
        //          propertyKind |= SymbolAttributes.Final;
        //        }

        //        if (getMethod.IsAbstract)
        //        {
        //          propertyKind |= SymbolAttributes.Abstract;
        //        }

        //        if (getMethod.IsStatic)
        //        {
        //          propertyKind |= SymbolAttributes.Static;
        //        }

        //        if (getMethod.IsVirtual)
        //        {
        //          propertyKind |= SymbolAttributes.Virtual;
        //        }

        //        if (getMethod.IsOverride())
        //        {
        //          propertyKind |= SymbolAttributes.Override;
        //        }

        //        return propertyKind;
        //      }

        //      bool isMethod = !isDelegate && !isClass && memberInfo.MemberType.HasFlag(MemberTypes.Method);
        //      if (isMethod)
        //      {
        //        SymbolAttributes methodKind = SymbolAttributes.Method;
        //        if (methodInfo.IsFinal)
        //        {
        //          methodKind |= SymbolAttributes.Final;
        //        }

        //        if (methodInfo.IsAbstract)
        //        {
        //          methodKind |= SymbolAttributes.Abstract;
        //        }

        //        if (methodInfo.IsStatic)
        //        {
        //          methodKind |= SymbolAttributes.Static;
        //        }

        //        if (methodInfo.IsVirtual)
        //        {
        //          methodKind |= SymbolAttributes.Virtual;
        //        }

        //        if (methodInfo.IsOverride())
        //        {
        //          methodKind |= SymbolAttributes.Override;
        //        }

        //        return methodKind;
        //      }

        //      bool isEvent = eventInfo != null;
        //      if (isEvent)
        //      {
        //        SymbolAttributes eventKind = SymbolAttributes.Event;
        //        MethodInfo addHandlerMethod = eventInfo.GetAddMethod(true);
        //        if (addHandlerMethod.IsFinal)
        //        {
        //          eventKind |= SymbolAttributes.Final;
        //        }

        //        if (addHandlerMethod.IsAbstract)
        //        {
        //          eventKind |= SymbolAttributes.Abstract;
        //        }

        //        if (addHandlerMethod.IsStatic)
        //        {
        //          eventKind |= SymbolAttributes.Static;
        //        }

        //        if (addHandlerMethod.IsVirtual)
        //        {
        //          eventKind |= SymbolAttributes.Virtual;
        //        }

        //        if (addHandlerMethod.IsOverride())
        //        {
        //          eventKind |= SymbolAttributes.Override;
        //        }

        //        return eventKind;
        //      }

        //      bool isConstructor = constructorInfo != null;
        //      if (isConstructor)
        //      {
        //        SymbolAttributes constructorKind = SymbolAttributes.Constructor;

        //        if (constructorInfo.IsStatic)
        //        {
        //          constructorKind |= SymbolAttributes.Static;
        //        }

        //        return constructorKind;
        //      }

        //      bool isField = fieldInfo != null;
        //      if (isField)
        //      {
        //        SymbolAttributes fieldKind = SymbolAttributes.Event;
        //        if (fieldInfo.IsInitOnly)
        //        {
        //          fieldKind |= SymbolAttributes.Final;
        //        }

        //        if (fieldInfo.IsStatic)
        //        {
        //          fieldKind |= SymbolAttributes.Static;
        //        }

        //        return fieldKind;
        //      }

        //      bool isInterface = !isDelegate && !isClass && (valueType?.IsInterface ?? false);
        //      if (isInterface)
        //      {
        //        SymbolAttributes interfaceKind = SymbolAttributes.Interface;
        //        return interfaceKind;
        //      }

        //      return SymbolAttributes.Undefined;
        //    }
        //    internal static string ToSignatureNameInternal(this MemberInfo memberInfo, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        //    {
        //      var fieldInfo = memberInfo as FieldInfo;
        //      var eventInfo = memberInfo as EventInfo;
        //      var propertyInfo = memberInfo as PropertyInfo;
        //      AccessModifier GetAccessModifier()
        //      {
        //        switch (memberInfo)
        //        {
        //          case Type type:
        //            return type.IsPublic ? AccessModifier.Public
        //              : type.IsNestedPrivate ? AccessModifier.Private
        //              : type.IsNestedAssembly ? AccessModifier.Internal
        //              : type.IsNestedFamily ? AccessModifier.Protected
        //              : type.IsNestedPublic ? AccessModifier.Public
        //              : type.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
        //              : type.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
        //              : !type.IsVisible ? AccessModifier.Internal
        //              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        //          case MethodBase methodBaseInfo:
        //            return methodBaseInfo.IsPublic ? AccessModifier.Public
        //              : methodBaseInfo.IsPrivate ? AccessModifier.Private
        //              : methodBaseInfo.IsAssembly ? AccessModifier.Internal
        //              : methodBaseInfo.IsFamily ? AccessModifier.Protected
        //              : methodBaseInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        //              : methodBaseInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        //              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        //          case FieldInfo _:
        //            return fieldInfo.IsPublic ? AccessModifier.Public
        //              : fieldInfo.IsPrivate ? AccessModifier.Private
        //              : fieldInfo.IsAssembly ? AccessModifier.Internal
        //              : fieldInfo.IsFamily ? AccessModifier.Protected
        //              : fieldInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        //              : fieldInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        //              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        //          case EventInfo _:
        //            return eventInfo.GetAddMethod(true).GetAccessModifier();
        //          case PropertyInfo _:
        //            return propertyInfo.GetAccessors(true)
        //        .Select(accessor => accessor.GetAccessModifier())
        //        .Min();
        //          default:
        //            throw new NotSupportedException("The provided MemberInfo is not supported");
        //        }
        //      }
        //      // TODO::Create valueType specific overloads to eliminate valueType switching and use cached reflection data

        //      var valueType = memberInfo as Type;
        //      MethodInfo methodInfo = memberInfo as MethodInfo // MemberInfo is method
        //        ?? valueType?.GetMethod("Invoke"); // MemberInfo is potentially a delegate
        //      MethodInfo propertyGetMethodInfo = propertyInfo?.GetGetMethod(true);
        //      MethodInfo propertySetMethodInfo = propertyInfo?.GetSetMethod(true);
        //      var constructorInfo = memberInfo as ConstructorInfo;

        //      ParameterInfo[] indexerPropertyIndexParameters = propertyInfo?.GetIndexParameters() ?? Array.Empty<ParameterInfo>();

        //      SymbolAttributes memberAttributes = memberInfo.GetKind();
        //      StringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
        //      IEnumerable<CustomAttributeData> symbolAttributes = memberInfo.GetCustomAttributesData();
        //#if !NETSTANDARD2_0
        //      if (memberAttributes.HasFlag(SymbolAttributes.Final))
        //      {
        //        symbolAttributes = symbolAttributes.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
        //      }
        //#endif

        //      _ = signatureNameBuilder.AppendCustomAttributes(symbolAttributes, isAppendNewLineEnabled: true);

        //      AccessModifier accessModifier = GetAccessModifier();
        //      _ = signatureNameBuilder
        //        .Append(accessModifier.ToDisplayStringValue())
        //        .Append(' ');

        //      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate)
        //        && !memberAttributes.HasFlag(SymbolAttributes.Struct)
        //        && !memberAttributes.HasFlag(SymbolAttributes.Field)
        //        && memberAttributes.HasFlag(SymbolAttributes.Final))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("sealed")
        //          .Append(' ');
        //      }

        //      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate)
        //        && memberAttributes.HasFlag(SymbolAttributes.Static))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("static")
        //          .Append(' ');
        //      }

        //      bool isAbstract = memberAttributes.HasFlag(SymbolAttributes.Abstract);
        //      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate) && isAbstract)
        //      {
        //        _ = signatureNameBuilder
        //          .Append("abstract")
        //          .Append(' ');
        //      }

        //      if (!isAbstract
        //        && !memberAttributes.HasFlag(SymbolAttributes.Delegate)
        //        && !memberAttributes.HasFlag(SymbolAttributes.Class)
        //        && memberAttributes.HasFlag(SymbolAttributes.Virtual))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("virtual")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct)
        //        || memberAttributes.HasFlag(SymbolAttributes.ReadOnlyField))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("readonly")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Struct))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("struct")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Class))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("class")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Interface))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("interface")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Delegate))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("delegate")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Event))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("event")
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Enum))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("enum")
        //          .Append(' ');
        //      }

        //      if (!memberAttributes.HasFlag(SymbolAttributes.Delegate)
        //        && !memberAttributes.HasFlag(SymbolAttributes.Class)
        //        && memberAttributes.HasFlag(SymbolAttributes.Override))
        //      {
        //        _ = signatureNameBuilder
        //          .Append("override")
        //          .Append(' ');
        //      }

        //      // Set return valueType
        //      if (memberAttributes.HasFlag(SymbolAttributes.Method)
        //        || memberAttributes.HasFlag(SymbolAttributes.Property)
        //        || memberAttributes.HasFlag(SymbolAttributes.Field)
        //        || memberAttributes.HasFlag(SymbolAttributes.Delegate)
        //        || memberAttributes.HasFlag(SymbolAttributes.Event))
        //      {
        //        Type returnType = fieldInfo?.FieldType
        //          ?? methodInfo?.ReturnType
        //          ?? propertyGetMethodInfo?.ReturnType
        //          ?? eventInfo?.EventHandlerType;

        //        _ = signatureNameBuilder.AppendDisplayNameInternal(returnType, isFullyQualifiedName, isDeclaringTypeIncluded: false)
        //          .Append(' ');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Member) && (!isDeclaringTypeIncluded || isFullyQualifiedName))
        //      {
        //        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo.DeclaringType, isFullyQualifiedName, isDeclaringTypeIncluded: false)
        //          .Append('.');
        //      }

        //      // Member or valueType name
        //      if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
        //      {
        //        _ = signatureNameBuilder.Append("this");
        //      }
        //      else
        //      {
        //        _ = signatureNameBuilder.AppendDisplayNameInternal(memberInfo, isFullyQualifiedName: isFullyQualifiedName && memberAttributes.HasFlag(SymbolAttributes.Type), isDeclaringTypeIncluded: false);
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Constructor)
        //        || memberAttributes.HasFlag(SymbolAttributes.Method)
        //        || memberAttributes.HasFlag(SymbolAttributes.Delegate))
        //      {
        //        _ = signatureNameBuilder.Append('(');

        //        if (memberAttributes.HasFlag(SymbolAttributes.Method) && (methodInfo?.IsExtensionMethod() ?? false))
        //        {
        //          _ = signatureNameBuilder
        //            .Append("this")
        //            .Append(' ');
        //        }
        //      }
        //      else if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
        //      {
        //        _ = signatureNameBuilder.Append('[');
        //      }

        //      IEnumerable<ParameterInfo> parameters = methodInfo?.GetParameters()
        //        ?? constructorInfo?.GetParameters()
        //        ?? indexerPropertyIndexParameters
        //        ?? Enumerable.Empty<ParameterInfo>();

        //      if (parameters.Any())
        //      {
        //        foreach (ParameterInfo parameter in parameters)
        //        {
        //          bool isGenericTypeDefinition = false;
        //          if (memberAttributes.HasFlag(SymbolAttributes.GenericMethod))
        //          {
        //            isGenericTypeDefinition = methodInfo.IsGenericMethodDefinition;
        //          }
        //          else if (memberAttributes.HasFlag(SymbolAttributes.GenericType))
        //          {
        //            isGenericTypeDefinition = valueType.IsGenericTypeDefinition;
        //          }

        //          if (isGenericTypeDefinition)
        //          {
        //            IEnumerable<CustomAttributeData> attributes = parameter.GetCustomAttributesData();
        //            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
        //          }

        //          if (parameter.IsRef())
        //          {
        //            _ = signatureNameBuilder.Append("ref ");
        //          }
        //          else if (parameter.IsIn)
        //          {
        //            _ = signatureNameBuilder.Append("in ");
        //          }
        //          else if (parameter.IsOut)
        //          {
        //            _ = signatureNameBuilder.Append("out ");
        //          }

        //          _ = signatureNameBuilder
        //            .AppendDisplayNameInternal(parameter.ParameterType, isFullyQualifiedName, isDeclaringTypeIncluded: false)
        //            .Append(' ')
        //            .Append(parameter.EventName)
        //            .Append(HelperExtensionsCommon.ParameterSeparator);
        //        }

        //        // Remove trailing comma and whitespace
        //        _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Constructor)
        //        || memberAttributes.HasFlag(SymbolAttributes.Method)
        //        || memberAttributes.HasFlag(SymbolAttributes.Delegate))
        //      {
        //        _ = signatureNameBuilder.Append(')');
        //      }
        //      else if (memberAttributes.HasFlag(SymbolAttributes.IndexerProperty))
        //      {
        //        _ = signatureNameBuilder.Append(']');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Property))
        //      {
        //        _ = signatureNameBuilder
        //          .Append(' ')
        //          .Append('{')
        //          .Append(' ');

        //        if (propertyGetMethodInfo != null)
        //        {
        //          _ = signatureNameBuilder
        //            .Append("get")
        //            .Append(HelperExtensionsCommon.ExpressionTerminator)
        //            .Append(' ');
        //        }

        //        if (propertySetMethodInfo != null)
        //        {
        //          _ = signatureNameBuilder
        //            .Append("set")
        //            .Append(HelperExtensionsCommon.ExpressionTerminator)
        //            .Append(' ');
        //        }

        //        _ = signatureNameBuilder.Append('}');
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Class))
        //      {
        //        signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(valueType, isFullyQualifiedName);
        //      }

        //      if (memberAttributes.HasFlag(SymbolAttributes.Generic))
        //      {
        //        Type[] genericTypeParameterDefinitions = Type.EmptyTypes;
        //        if (memberAttributes.HasFlag(SymbolAttributes.GenericType) && valueType.IsGenericTypeDefinition)
        //        {
        //          genericTypeParameterDefinitions = valueType.GetGenericTypeDefinition().GetGenericArguments();
        //        }
        //        else if (memberAttributes.HasFlag(SymbolAttributes.GenericMethod) && methodInfo.IsGenericMethodDefinition)
        //        {
        //          genericTypeParameterDefinitions = methodInfo.GetGenericMethodDefinition().GetGenericArguments();
        //        }

        //        _ = signatureNameBuilder.AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isCompact);
        //      }

        //      if (!memberAttributes.HasFlag(SymbolAttributes.Class)
        //        && !memberAttributes.HasFlag(SymbolAttributes.Struct)
        //        && !memberAttributes.HasFlag(SymbolAttributes.Enum))
        //      {
        //        _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
        //      }

        //      string fullMemberName = signatureNameBuilder.ToString();
        //      StringBuilderFactory.Recycle(signatureNameBuilder);

        //      return fullMemberName;
        //    }
        //    private static StringBuilder AppendInheritanceSignature(this StringBuilder memberNameBuilder, Type typeData, bool isFullyQualified)
        //    {
        //      bool isDelegate = HelperExtensionsCommon.DelegateType.IsAssignableFrom(typeData);
        //      if (isDelegate)
        //      {
        //        return memberNameBuilder;
        //      }

        //      bool isSubclass = typeData.BaseType != typeof(object);
        //      Type[] interfaces = typeData.GetInterfaces();
        //      bool hasInterfaces = interfaces.Length > 0;
        //      if (isSubclass || hasInterfaces)
        //      {
        //        _ = memberNameBuilder.Append(" : ");
        //      }

        //      if (isSubclass)
        //      {
        //        _ = memberNameBuilder.Append(isFullyQualified ? typeData.BaseType.FullName : typeData.BaseType.EventName)
        //          .Append(HelperExtensionsCommon.ParameterSeparator);
        //      }

        //      foreach (Type interfaceData in interfaces)
        //      {
        //        _ = memberNameBuilder.Append(isFullyQualified ? interfaceData.FullName : interfaceData.EventName)
        //          .Append(HelperExtensionsCommon.ParameterSeparator);
        //      }

        //      if (isSubclass || hasInterfaces)
        //      {
        //        _ = memberNameBuilder.Remove(memberNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
        //      }

        //      return memberNameBuilder;
        //    }
        //    private static StringBuilder AppendGenericTypeConstraints(this StringBuilder constraintBuilder, Type[] genericTypeDefinitionsData, bool isFullyQualified, bool isCompact)
        //    {
        //      bool hasSingleNewLine = false;
        //      for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
        //      {
        //        Type genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
        //        Type[] constraints = genericTypeDefinitionData.GetGenericParameterConstraints();
        //        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
        //          && constraints.Length == 0)
        //        {
        //          continue;
        //        }

        //        if (isCompact)
        //        {
        //          if (!hasSingleNewLine)
        //          {
        //            _ = constraintBuilder.AppendLine()
        //            .Append(HelperExtensionsCommon.Indentation);
        //            hasSingleNewLine = true;
        //          }
        //          else
        //          {
        //            _ = constraintBuilder.Append(' ');
        //          }
        //        }
        //        else
        //        {
        //          _ = constraintBuilder.AppendLine()
        //            .Append(HelperExtensionsCommon.Indentation);
        //        }

        //        _ = constraintBuilder.Append("where")
        //          .Append(' ')
        //          .Append(genericTypeDefinitionData.EventName)
        //          .Append(" : ");

        //        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
        //        {
        //          _ = constraintBuilder.Append("class")
        //            .Append(HelperExtensionsCommon.ParameterSeparator);
        //        }

        //        if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
        //        {
        //          _ = constraintBuilder.Append("struct")
        //            .Append(HelperExtensionsCommon.ParameterSeparator);
        //        }

        //        foreach (Type constraint in constraints)
        //        {
        //          _ = constraintBuilder.AppendDisplayNameInternal(constraint, isFullyQualified, isDeclaringTypeIncluded: false)
        //            .Append(HelperExtensionsCommon.ParameterSeparator);
        //        }

        //        if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
        //        {
        //          _ = constraintBuilder.Append("new()")
        //            .Append(HelperExtensionsCommon.ParameterSeparator);
        //        }

        //        _ = constraintBuilder.Remove(constraintBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
        //      }

        //      return constraintBuilder;
        //    }
        #endregion REMOVE AFTER BENCHMARK COMPARISON!!!

        /// <summary>
        /// Generates a formatted signature string for the specified property, including modifiers, type, name,
        /// accessors, and optional custom attributes.
        /// </summary>
        /// <remarks>The generated signature reflects the property's access level,
        /// static/abstract/virtual/override modifiers, type (with generic arguments if applicable), indexer parameters
        /// (if any), and accessor visibility. When isCompact is true or isRuntimeSymbol is true, custom attributes are
        /// omitted for brevity or runtime compatibility. Use this method to display or analyze property signatures in
        /// code generation, documentation, or tooling scenarios.</remarks>
        /// <param name="propertyData">The property metadata to generate the signature for. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the property name; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature without custom attributes or extra formatting; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the property represents a runtime symbol and should be formatted accordingly; otherwise, false.</param>
        /// <returns>A string containing the complete signature of the property, including modifiers, type, name, accessors, and
        /// any applicable custom attributes.</returns>
        internal static string ToSignatureNameInternal(PropertyData propertyData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = propertyData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = propertyData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

            AccessModifier accessModifier = propertyData.AccessModifier;
            _ = signatureNameBuilder
              .Append(accessModifier.ToDisplayStringValue())
              .Append(' ');

            if (symbolAttributes.HasFlag(SymbolAttributes.Static))
            {
                _ = signatureNameBuilder
                  .Append("static")
                  .Append(' ');
            }

            bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
            if (!symbolAttributes.HasFlag(SymbolAttributes.Delegate) && isAbstract)
            {
                _ = signatureNameBuilder
                  .Append("abstract")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                _ = signatureNameBuilder
                  .Append("virtual")
                  .Append(' ');
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Override))
            {
                _ = signatureNameBuilder
                  .Append("override")
                  .Append(' ');
            }

            TypeData propertyTypeData = propertyData.PropertyTypeData;
            if (!isRuntimeSymbol && propertyTypeData.IsGenericType && !propertyTypeData.IsGenericTypeDefinition)
            {
                propertyTypeData = propertyTypeData.GenericTypeDefinitionData;
            }

            // Set return valueType
            _ = signatureNameBuilder.AppendDisplayNameInternal(propertyTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            // Member name
            if (symbolAttributes.HasFlag(SymbolAttributes.IndexerProperty))
            {
                _ = signatureNameBuilder.Append("this")
                  .Append('[');

                ParameterData[] parameters = propertyData.IndexerParameters;
                if (parameters.Any())
                {
                    foreach (ParameterData parameter in parameters)
                    {
                        IList<CustomAttributeData> attributes = parameter.AttributeData;
                        _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false)
                          .AppendDisplayNameInternal(parameter.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                          .Append(' ')
                          .Append(parameter.Name)
                          .Append(HelperExtensionsCommon.ParameterSeparator);
                    }

                    // Remove trailing comma and whitespace
                    _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
                      .Append(']');
                }
            }
            else
            {
                _ = signatureNameBuilder.AppendDisplayNameInternal(propertyData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            }

            _ = signatureNameBuilder
              .Append(' ')
              .Append('{')
              .Append(' ');

            if (propertyData.CanRead)
            {
                if (HelperExtensionsCommon.AccessModifierComparer.Compare(propertyData.GetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    _ = signatureNameBuilder
                      .Append(propertyData.GetAccessorAccessModifier.ToDisplayStringValue())
                      .Append(' ');
                }

                _ = signatureNameBuilder
                  .Append("get")
                  .Append(HelperExtensionsCommon.ExpressionTerminator)
                  .Append(' ');
            }

            if (propertyData.CanWrite)
            {
                if (HelperExtensionsCommon.AccessModifierComparer.Compare(propertyData.SetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    _ = signatureNameBuilder
                      .Append(propertyData.SetAccessorAccessModifier.ToDisplayStringValue())
                      .Append(' ');
                }

                if (propertyData.IsSetMethodReadOnly)
                {
                    _ = signatureNameBuilder
                      .Append("readonly")
                      .Append(' ');
                }

                if (propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty))
                {
                    _ = signatureNameBuilder
                      .Append("init")
                      .Append(HelperExtensionsCommon.ExpressionTerminator)
                      .Append(' ');
                }
                else
                {
                    _ = signatureNameBuilder
                    .Append("set")
                    .Append(HelperExtensionsCommon.ExpressionTerminator)
                    .Append(' ');
                }
            }

            _ = signatureNameBuilder.Append('}');

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the signature name string for the specified event, using the provided formatting and inclusion
        /// options.
        /// </summary>
        /// <param name="eventData">The event metadata to use when constructing the signature name. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the event signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature without custom attributes; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature is being generated for a runtime symbol, which affects formatting and attribute
        /// inclusion; otherwise, false.</param>
        /// <returns>A string representing the formatted signature name of the event, including modifiers, type, and name as
        /// specified by the input parameters.</returns>
        internal static string ToSignatureNameInternal(EventData eventData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = eventData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = eventData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

            AccessModifier accessModifier = eventData.AccessModifier;
            _ = signatureNameBuilder
              .Append(accessModifier.ToDisplayStringValue())
              .Append(' ');

            if (symbolAttributes.HasFlag(SymbolAttributes.Static))
            {
                _ = signatureNameBuilder
                  .Append("static")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
            {
                _ = signatureNameBuilder
                  .Append("abstract")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                _ = signatureNameBuilder
                  .Append("virtual")
                  .Append(' ');
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Override))
            {
                _ = signatureNameBuilder
                  .Append("override")
                  .Append(' ');
            }

            TypeData eventHandlerTypeData = eventData.EventHandlerTypeData;
            if (!isRuntimeSymbol && eventHandlerTypeData.IsGenericType && !eventHandlerTypeData.IsGenericTypeDefinition)
            {
                eventHandlerTypeData = eventHandlerTypeData.GenericTypeDefinitionData;
            }

            _ = signatureNameBuilder
              .Append("event")
              .Append(' ')
              .AppendDisplayNameInternal(eventHandlerTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            _ = signatureNameBuilder.AppendDisplayNameInternal(eventData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded)
              .Append(HelperExtensionsCommon.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the formatted signature name for a field, including modifiers, type, and name, based on the
        /// specified formatting options.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing field signature
        /// representations for display or analysis. The output format may vary depending on the combination of
        /// formatting flags provided.</remarks>
        /// <param name="fieldData">The metadata describing the field for which to generate the signature name.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified type name in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the field's signature; otherwise, false.</param>
        /// <param name="isCompact">true to use a compact format that omits custom attributes and some modifiers; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature is being generated for a runtime symbol, which may affect formatting; otherwise,
        /// false.</param>
        /// <returns>A string containing the formatted signature name of the field, including modifiers, type, and name,
        /// according to the specified options.</returns>
        internal static string ToSignatureNameInternal(FieldData fieldData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = fieldData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = fieldData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

            AccessModifier accessModifier = fieldData.AccessModifier;
            _ = signatureNameBuilder
              .Append(accessModifier.ToDisplayStringValue())
              .Append(' ');

            if (symbolAttributes.HasFlag(SymbolAttributes.ConstantField))
            {
                _ = signatureNameBuilder
                  .Append("const")
                  .Append(' ');
            }
            else
            {
                if (symbolAttributes.HasFlag(SymbolAttributes.Static))
                {
                    _ = signatureNameBuilder
                      .Append("static")
                      .Append(' ');
                }

                if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyField))
                {
                    _ = signatureNameBuilder
                      .Append("readonly")
                      .Append(' ');
                }

                if (fieldData.IsRef)
                {
                    _ = signatureNameBuilder
                      .Append("ref")
                      .Append(' ');
                }
            }

            TypeData fieldTypeData = fieldData.FieldTypeData;
            if (!isRuntimeSymbol && fieldTypeData.IsGenericType && !fieldTypeData.IsGenericTypeDefinition)
            {
                fieldTypeData = fieldTypeData.GenericTypeDefinitionData;
            }

            _ = signatureNameBuilder.AppendDisplayNameInternal(fieldTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            _ = signatureNameBuilder.AppendDisplayNameInternal(fieldData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded)
              .Append(HelperExtensionsCommon.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the signature name for a type, including modifiers, attributes, and type parameters, based on the
        /// specified formatting options.
        /// </summary>
        /// <remarks>When generating signatures for delegates, the return type and parameter list are
        /// included. For generic types, type parameters and constraints are appended unless compact formatting is
        /// requested. Attribute and inheritance information is omitted in compact or runtime symbol mode.</remarks>
        /// <param name="typeData">The type metadata used to construct the signature name.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature without attributes or inheritance information; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature is being generated for a runtime symbol; otherwise, false.</param>
        /// <returns>A string representing the formatted signature name of the specified type, including modifiers, attributes,
        /// and type parameters as determined by the input options.</returns>
        internal static string ToSignatureNameInternal(TypeData typeData, bool isFullyQualifiedName, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = typeData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
            if (!isRuntimeSymbol)
            {
                if (typeData.IsGenericType && !typeData.IsGenericTypeDefinition)
                {
                    typeData = typeData.GenericTypeDefinitionData;
                }

                if (!isCompact)
                {
                    IEnumerable<CustomAttributeData> customAttributesData = typeData.AttributeData;

                    if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                    {
                        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
                    }

                    _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
                }
            }

            AccessModifier accessModifier = typeData.AccessModifier;
            _ = signatureNameBuilder
              .Append(accessModifier.ToDisplayStringValue())
              .Append(' ');

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                _ = signatureNameBuilder
                  .Append("delegate")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Struct))
            {
                if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct))
                {
                    _ = signatureNameBuilder
                      .Append("readonly")
                      .Append(' ');
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.RefStruct))
                {
                    _ = signatureNameBuilder
                      .Append("ref")
                      .Append(' ');
                }

                _ = signatureNameBuilder
                  .Append("struct")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Class))
            {
                if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
                {
                    _ = signatureNameBuilder
                      .Append("abstract")
                      .Append(' ');
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Static))
                {
                    _ = signatureNameBuilder
                      .Append("static")
                      .Append(' ');
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    _ = signatureNameBuilder
                      .Append("sealed")
                      .Append(' ');
                }

                _ = signatureNameBuilder
                  .Append("class")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Interface))
            {
                _ = signatureNameBuilder
                  .Append("interface")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Enum))
            {
                _ = signatureNameBuilder
                  .Append("enum")
                  .Append(' ');
            }

            MethodData delegateInvocatorData = null;

            // Set return valueType
            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                delegateInvocatorData = typeData.DelegateInvokeMethodData;
                TypeData delegateReturnTypeData = delegateInvocatorData.ReturnTypeData;
                if (!isRuntimeSymbol && delegateReturnTypeData.IsGenericType && !delegateReturnTypeData.IsGenericTypeDefinition)
                {
                    delegateReturnTypeData = delegateReturnTypeData.GenericTypeDefinitionData;
                }

                _ = signatureNameBuilder.AppendDisplayNameInternal(delegateReturnTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                  .Append(' ');
            }

            // Type name
            _ = signatureNameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true);

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                _ = signatureNameBuilder.Append('(');

                ParameterData[] parameters = delegateInvocatorData.Parameters;
                if (parameters.Length > 0)
                {
                    foreach (ParameterData parameterData in parameters)
                    {
                        if (!isRuntimeSymbol)
                        {
                            IList<CustomAttributeData> attributes = parameterData.AttributeData;
                            _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
                        }

                        if (parameterData.IsRef)
                        {
                            _ = signatureNameBuilder.Append("ref ");
                        }
                        else if (parameterData.IsIn)
                        {
                            _ = signatureNameBuilder.Append("in ");
                        }
                        else if (parameterData.IsOut)
                        {
                            _ = signatureNameBuilder.Append("out ");
                        }

                        _ = signatureNameBuilder
                          .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                          .Append(' ')
                          .Append(parameterData.Name)
                          .Append(HelperExtensionsCommon.ParameterSeparator);
                    }

                    // Remove trailing comma and whitespace
                    _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
                      .Append(')');
                }
            }
            else if (!isCompact && symbolAttributes.HasFlag(SymbolAttributes.Class))
            {
                signatureNameBuilder = signatureNameBuilder.AppendInheritanceSignature(typeData, isFullyQualifiedName);
            }

            if (!isCompact && !isRuntimeSymbol)
            {
                TypeData[] genericTypeParameterDefinitions = typeData.GenericTypeArguments;
                if (genericTypeParameterDefinitions.Length > 0)
                {
                    _ = signatureNameBuilder
                      .Append(' ')
                      .AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isSingleLine: false, typeData.IndentationString);
                }
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);
            }

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates a formatted method signature string based on the specified method metadata and formatting options.
        /// </summary>
        /// <remarks>The generated signature reflects the specified formatting options and may include
        /// custom attributes, access modifiers, and generic type constraints depending on the provided parameters. This
        /// method does not validate the input metadata; callers should ensure that the provided MethodData is
        /// valid.</remarks>
        /// <param name="methodData">The metadata describing the method for which to generate the signature. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the method signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature without custom attributes or generic constraints; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true if the signature should be generated for a runtime symbol; otherwise, false.</param>
        /// <returns>A string representing the formatted method signature, including modifiers, return type, method name,
        /// parameters, and, if applicable, custom attributes and generic constraints.</returns>
        internal static string ToSignatureNameInternal(MethodData methodData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            Debug.WriteLine($"Generating method signature");

            SymbolComponentInfo symbolComponents = null;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();
            SymbolAttributes symbolAttributes = methodData.SymbolAttributes;

            if (!isRuntimeSymbol)
            {
                if (methodData.IsGenericMethod && !methodData.IsGenericMethodDefinition)
                {
                    methodData = methodData.GenericMethodDefinitionData;
                }

                if (!isCompact)
                {
                    IEnumerable<CustomAttributeData> customAttributesData = methodData.AttributeData;

                    if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                    {
                        customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
                    }

                    _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
                }
            }

            AccessModifier accessModifier = methodData.AccessModifier;
            _ = signatureNameBuilder
              .Append(accessModifier.ToDisplayStringValue())
              .Append(' ');

            if (methodData.IsStatic)
            {
                _ = signatureNameBuilder
                  .Append("static")
                  .Append(' ');
            }

            if (methodData.IsSealed)
            {
                _ = signatureNameBuilder
                  .Append("sealed")
                  .Append(' ');
                symbolComponents.AddModifier("sealed");
            }

            bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
            if (isAbstract)
            {
                _ = signatureNameBuilder
                  .Append("abstract")
                  .Append(' ');
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                _ = signatureNameBuilder
                  .Append("virtual")
                  .Append(' ');
            }

            if (methodData.IsOverride)
            {
                _ = signatureNameBuilder
                  .Append("override")
                  .Append(' ');
            }

            if (methodData.IsAsync)
            {
                _ = signatureNameBuilder
                  .Append("async")
                  .Append(' ');
            }

            if (methodData.IsReturnValueByRef)
            {
                _ = signatureNameBuilder
                  .Append("ref")
                  .Append(' ');
            }

            if (methodData.IsReturnValueReadOnly)
            {
                _ = signatureNameBuilder
                  .Append("readonly")
                  .Append(' ');
            }

            TypeData returnTypeData = methodData.ReturnTypeData;
            //if (!isRuntimeSymbol && returnTypeData.IsGenericType && !returnTypeData.IsGenericTypeDefinition)
            //{
            //    returnTypeData = returnTypeData.GenericTypeDefinitionData;
            //}

            _ = signatureNameBuilder.AppendDisplayNameInternal(returnTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
              .Append(' ');

            // Member name
            _ = signatureNameBuilder.AppendDisplayNameInternal(methodData, isFullyQualifiedName, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded)
              .Append('(');

            if (methodData.IsExtensionMethod)
            {
                _ = signatureNameBuilder
                  .Append("this")
                  .Append(' ');
            }

            ParameterData[] parameters = methodData.Parameters;
            if (parameters.Length > 0)
            {
                for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    ParameterData parameterData = parameters[parameterIndex];

                    if (!isRuntimeSymbol)
                    {
                        IList<CustomAttributeData> attributes = parameterData.AttributeData;
                        _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
                    }

                    if (parameterData.IsRef)
                    {
                        _ = signatureNameBuilder.Append("ref ");
                    }
                    else if (parameterData.IsIn)
                    {
                        _ = signatureNameBuilder.Append("in ");
                    }
                    else if (parameterData.IsOut)
                    {
                        _ = signatureNameBuilder.Append("out ");
                    }

                    _ = signatureNameBuilder
                      .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                      .Append(' ')
                      .Append(parameterData.Name)
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                // Remove trailing comma and whitespace
                _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
            }

            _ = signatureNameBuilder.Append(')');

            if (!isCompact && !isRuntimeSymbol)
            {
                TypeData[] genericTypeParameterDefinitions = methodData.GenericTypeArguments;
                if (genericTypeParameterDefinitions.Length > 0)
                {
                    _ = signatureNameBuilder
                      .AppendGenericTypeConstraints(genericTypeParameterDefinitions, isFullyQualifiedName, isSingleLine: false, methodData.IndentationString);
                }
            }

            _ = signatureNameBuilder.Append(HelperExtensionsCommon.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Generates the formatted signature name for a constructor based on the specified formatting options.
        /// </summary>
        /// <remarks>Custom attributes and certain modifiers are included or omitted in the signature
        /// based on the values of isCompact and isRuntimeSymbol. This method is intended for internal use when
        /// generating display names for constructors in various contexts.</remarks>
        /// <param name="constructorData">The metadata describing the constructor, including its parameters, attributes, and access modifiers.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified type name in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature without custom attributes; otherwise, false.</param>
        /// <param name="isRuntimeSymbol">true to format the signature for runtime symbol representation, omitting custom attributes and certain
        /// modifiers; otherwise, false.</param>
        /// <returns>A string containing the formatted constructor signature according to the specified options.</returns>
        internal static string ToSignatureNameInternal(ConstructorData constructorData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact, bool isRuntimeSymbol)
        {
            SymbolAttributes symbolAttributes = constructorData.SymbolAttributes;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (!(isRuntimeSymbol || isCompact))
            {
                IEnumerable<CustomAttributeData> customAttributesData = constructorData.AttributeData;

                if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                {
                    customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
                }

                _ = signatureNameBuilder.AppendCustomAttributes(customAttributesData, isAppendNewLineEnabled: true);
            }

            AccessModifier accessModifier = constructorData.AccessModifier;
            if (accessModifier is not AccessModifier.Undefined)
            {
                _ = signatureNameBuilder
                .Append(accessModifier.ToDisplayStringValue())
                .Append(' ');
            }

            if (constructorData.IsStatic)
            {
                _ = signatureNameBuilder
                  .Append("static")
                  .Append(' ');
            }

            // Member name
            _ = signatureNameBuilder.AppendDisplayNameInternal(constructorData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded)
              .Append('(');

            ParameterData[] parameters = constructorData.Parameters;
            if (parameters.Length > 0)
            {
                foreach (ParameterData parameterData in parameters)
                {
                    if (!isRuntimeSymbol)
                    {
                        IList<CustomAttributeData> attributes = parameterData.AttributeData;
                        _ = signatureNameBuilder.AppendCustomAttributes(attributes, isAppendNewLineEnabled: false);
                    }

                    if (parameterData.IsRef)
                    {
                        _ = signatureNameBuilder.Append("ref ");
                    }
                    else if (parameterData.IsIn)
                    {
                        _ = signatureNameBuilder.Append("in ");
                    }
                    else if (parameterData.IsOut)
                    {
                        _ = signatureNameBuilder.Append("out ");
                    }

                    _ = signatureNameBuilder
                      .AppendDisplayNameInternal(parameterData.ParameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                      .Append(' ')
                      .Append(parameterData.Name)
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                // Remove trailing comma and whitespace
                _ = signatureNameBuilder.Remove(signatureNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
            }

            _ = signatureNameBuilder
                  .Append(')')
                  .Append(HelperExtensionsCommon.ExpressionTerminator);

            string fullMemberName = signatureNameBuilder.ToString();
            StringBuilderFactory.Recycle(signatureNameBuilder);

            return fullMemberName;
        }

        /// <summary>
        /// Builds a SymbolComponentInfo representation of a method's signature, including modifiers, return type, name,
        /// parameters, and generic type information, based on the specified formatting options.
        /// </summary>
        /// <remarks>If isCompact is set to true, custom attributes and generic type constraints are
        /// omitted from the signature. The method supports both generic and non-generic methods, and can include or
        /// exclude the declaring type and fully qualified names as needed.</remarks>
        /// <param name="methodData">The metadata describing the method to be represented. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the method signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature without custom attributes and generic type constraints; otherwise,
        /// false.</param>
        /// <returns>A SymbolComponentInfo object containing the components of the method's signature as specified by the input
        /// parameters.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(MethodData methodData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            if (methodData.IsGenericMethod && !methodData.IsGenericMethodDefinition)
            {
                methodData = methodData.GenericMethodDefinitionData;
            }

            SymbolAttributes symbolAttributes = methodData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = methodData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = methodData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (methodData.IsStatic)
            {
                symbolComponents.AddModifier("static");
            }

            if (methodData.IsSealed)
            {
                symbolComponents.AddModifier("sealed");
            }

            bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
            if (isAbstract)
            {
                symbolComponents.AddModifier("abstract");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                symbolComponents.AddModifier("virtual");
            }

            if (methodData.IsOverride)
            {
                symbolComponents.AddModifier("override");
            }

            if (methodData.IsAsync)
            {
                symbolComponents.AddModifier("async");
            }

            if (methodData.IsReturnValueByRef)
            {
                symbolComponents.AddModifier("ref");
            }

            if (methodData.IsReturnValueReadOnly)
            {
                symbolComponents.AddModifier("readonly");
            }

            symbolComponents.ReturnType = methodData.ReturnTypeData.CompactSymbolComponentInfo;

            // Member name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(methodData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            symbolComponents.IsExtensionMethodParameter = methodData.IsExtensionMethod;

            ParameterData[] parameters = methodData.Parameters;
            if (parameters.Length > 0)
            {
                for (int parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
                {
                    ParameterData parameterData = parameters[parameterIndex];
                    SymbolComponentInfo parameterInfo = parameterData.SymbolComponentInfo;
                    symbolComponents.AddParameter(parameterInfo);
                }
            }

            if (methodData.IsGenericMethod)
            {
                IEnumerable<SymbolComponentInfo> genericTypeParameterComponents = methodData.GenericTypeArguments.Select(typeParameterData =>
                {
                    SymbolComponentInfo info = typeParameterData.SymbolComponentInfo;
                    info.IsParameter = true;
                    return info;
                });
                symbolComponents.AddGenericTypeParameterRange(genericTypeParameterComponents);

                if (!isCompact)
                {
                    AddGenericTypeConstraints(symbolComponents, methodData.GenericTypeArguments, isFullyQualifiedName);
                }
            }

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of the specified type,
        /// including modifiers, attributes, and generic parameters as appropriate.
        /// </summary>
        /// <remarks>When isCompact is false, the returned signature includes access modifiers, custom
        /// attributes, and inheritance or generic constraints where applicable. For delegate types, the signature
        /// includes parameter and return type information. The method does not validate the input typeData; callers
        /// should ensure it represents a valid type.</remarks>
        /// <param name="typeData">The type metadata to convert into signature components. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false to use simple names.</param>
        /// <param name="isCompact">true to generate a compact signature with minimal modifiers and attributes; otherwise, false to include full
        /// details.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components for the specified type, including
        /// modifiers, attributes, name, generic parameters, and, for delegates, parameter and return type information.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(TypeData typeData, bool isFullyQualifiedName, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: typeData.IsBuiltInType)
            {
                IsSymbol = true,
            };

            if (typeData.IsGenericType && !typeData.IsGenericTypeDefinition)
            {
                typeData = typeData.GenericTypeDefinitionData;
            }

            SymbolAttributes symbolAttributes = typeData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = typeData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);

                AccessModifier accessModifier = typeData.AccessModifier;
                symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

                if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
                {
                    symbolComponents.AddModifier("delegate");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Struct))
                {
                    if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct))
                    {
                        symbolComponents.AddModifier("readonly");
                    }
                    else if (symbolAttributes.HasFlag(SymbolAttributes.RefStruct))
                    {
                        symbolComponents.AddModifier("ref");
                    }

                    symbolComponents.AddModifier("struct");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Class))
                {
                    if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
                    {
                        symbolComponents.AddModifier("abstract");
                    }
                    else if (symbolAttributes.HasFlag(SymbolAttributes.Static))
                    {
                        symbolComponents.AddModifier("static");
                    }
                    else if (symbolAttributes.HasFlag(SymbolAttributes.Final))
                    {
                        symbolComponents.AddModifier("sealed");
                    }

                    symbolComponents.AddModifier("class");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Interface))
                {
                    symbolComponents.AddModifier("interface");
                }
                else if (symbolAttributes.HasFlag(SymbolAttributes.Enum))
                {
                    symbolComponents.AddModifier("enum");
                }
            }

            MethodData delegateInvocatorData = null;
            TypeData delegateReturnTypeData = null;

            // Set return valueType
            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                delegateReturnTypeData = typeData.DelegateInvokeMethodData.ReturnTypeData;
                symbolComponents.ReturnType = delegateReturnTypeData.CompactSymbolComponentInfo;
            }

            // Type name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isGenericTypeParameterIncluded: false);

            if (symbolAttributes.HasFlag(SymbolAttributes.Delegate))
            {
                ParameterData[] parameters = delegateInvocatorData.Parameters;
                if (parameters.Length > 0)
                {
                    foreach (ParameterData parameterData in parameters)
                    {
                        SymbolComponentInfo parameterInfo = parameterData.SymbolComponentInfo;
                        symbolComponents.AddParameter(parameterInfo);
                    }
                }
            }
            else if (!isCompact && symbolAttributes.HasFlag(SymbolAttributes.Class))
            {
                AddInheritanceSignature(symbolComponents, typeData, isFullyQualifiedName);
            }

            if (typeData.IsGenericType)
            {
                TypeData[] genericTypeArguments = typeData.GenericTypeArguments;
                IEnumerable<SymbolComponentInfo> genericTypeParameterComponents = genericTypeArguments.Select(typeParameterData => typeParameterData.SymbolComponentInfo);
                symbolComponents.AddGenericTypeParameterRange(genericTypeParameterComponents);

                if (!isCompact)
                {
                    AddGenericTypeConstraints(symbolComponents, genericTypeArguments, isFullyQualifiedName);
                }
            }

            symbolComponents.HasExpressionTerminator = symbolAttributes.HasFlag(SymbolAttributes.Delegate);
            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of a parameter, including its
        /// type, modifiers, and custom attributes, according to the specified formatting options.
        /// </summary>
        /// <remarks>Custom attributes are included in the signature unless isCompact is set to true. The
        /// method applies parameter modifiers such as ref, in, or out as appropriate. If the parameter type is a
        /// constructed generic type, its generic type definition is used for display purposes.</remarks>
        /// <param name="parameterData">The parameter metadata to convert into signature components. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to use fully qualified type names in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature representation that omits custom attributes; otherwise, false.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components for the specified parameter, formatted
        /// according to the provided options.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(ParameterData parameterData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            TypeData parameterTypeData = parameterData.ParameterTypeData;
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: parameterTypeData.IsBuiltInType)
            {
                IsSymbol = false,
                IsParameter = true,
                HasExpressionTerminator = false,
                HasInlineAttributes = true,
            };

            if (parameterTypeData.IsGenericType && !parameterTypeData.IsGenericTypeDefinition)
            {
                parameterTypeData = parameterTypeData.GenericTypeDefinitionData;
            }

            SymbolAttributes symbolAttributes = parameterData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = parameterData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);
            }

            if (parameterData.IsRef)
            {
                symbolComponents.AddModifier("ref");
            }
            else if (parameterData.IsIn)
            {
                symbolComponents.AddModifier("in");
            }
            else if (parameterData.IsOut)
            {
                symbolComponents.AddModifier("out");
            }

            // Type name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(parameterTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true);
            _ = symbolComponents.ValueNameBuilder.AppendDisplayNameInternal(parameterData);

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of the specified field,
        /// including modifiers, attributes, and type information.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing field signatures for
        /// display or analysis. The output reflects the specified formatting options and may omit certain attributes or
        /// components based on the provided parameters.</remarks>
        /// <param name="fieldData">The field metadata to extract signature components from. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified name of the field; otherwise, false to use the simple name.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the field's signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature that omits custom attributes; otherwise, false to include all relevant
        /// attributes.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components of the field, such as modifiers, type, and
        /// name.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(FieldData fieldData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            SymbolAttributes symbolAttributes = fieldData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = fieldData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType).ToHashSet();
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = fieldData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (symbolAttributes.HasFlag(SymbolAttributes.ConstantField))
            {
                symbolComponents.AddModifier("const");
            }
            else
            {
                if (symbolAttributes.HasFlag(SymbolAttributes.Static))
                {
                    symbolComponents.AddModifier("static");
                }

                if (symbolAttributes.HasFlag(SymbolAttributes.ReadOnlyField))
                {
                    symbolComponents.AddModifier("readonly");
                }

                if (fieldData.IsRef)
                {
                    symbolComponents.AddModifier("ref");
                }
            }

            symbolComponents.ReturnType = fieldData.FieldTypeData.CompactSymbolComponentInfo;

            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(fieldData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of the specified event,
        /// including modifiers, attributes, and event type information.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing event signatures for
        /// display or analysis. The output reflects the specified formatting options and may exclude certain attributes
        /// or modifiers based on the provided parameters.</remarks>
        /// <param name="eventData">The event metadata to extract signature components from. Must not be null.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified name of the event in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the event's signature; otherwise, false.</param>
        /// <param name="isCompact">true to produce a compact signature that omits custom attributes; otherwise, false.</param>
        /// <returns>A SymbolComponentInfo object containing the signature components of the event, including modifiers, event
        /// type, and name.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(EventData eventData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            SymbolAttributes symbolAttributes = eventData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = eventData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = eventData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (symbolAttributes.HasFlag(SymbolAttributes.Static))
            {
                symbolComponents.AddModifier("static");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Abstract))
            {
                symbolComponents.AddModifier("abstract");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                symbolComponents.AddModifier("virtual");
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Override))
            {
                symbolComponents.AddModifier("override");
            }

            symbolComponents.AddModifier("event");

            symbolComponents.ReturnType = eventData.EventHandlerTypeData.CompactSymbolComponentInfo;

            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(eventData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            return symbolComponents;
        }

        /// <summary>
        /// Builds a SymbolComponentInfo representation of a property signature based on the specified property metadata
        /// and formatting options.
        /// </summary>
        /// <remarks>This method is intended for internal use when constructing property signatures for
        /// display or analysis purposes. The output reflects the specified formatting options and may differ depending
        /// on the property type (e.g., indexer vs. regular property) and the presence of custom attributes or
        /// modifiers.</remarks>
        /// <param name="propertyData">The metadata describing the property, including its type, access modifiers, attributes, and accessor
        /// information. Cannot be null.</param>
        /// <param name="isFullyQualifiedName">true to include the property's fully qualified name in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the property's signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature that omits custom attributes; otherwise, false.</param>
        /// <returns>A SymbolComponentInfo object containing the components of the property's signature, including modifiers,
        /// return type, name, parameters (for indexers), and accessor information.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(PropertyData propertyData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = false,
            };

            SymbolAttributes symbolAttributes = propertyData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = propertyData.AttributeData;

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != typeof(IsReadOnlyAttribute));
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = propertyData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (symbolAttributes.HasFlag(SymbolAttributes.Static))
            {
                symbolComponents.AddModifier("static");
            }

            bool isAbstract = symbolAttributes.HasFlag(SymbolAttributes.Abstract);
            if (!symbolAttributes.HasFlag(SymbolAttributes.Delegate) && isAbstract)
            {
                symbolComponents.AddModifier("abstract");
            }
            else if (symbolAttributes.HasFlag(SymbolAttributes.Virtual))
            {
                symbolComponents.AddModifier("virtual");
            }

            if (symbolAttributes.HasFlag(SymbolAttributes.Override))
            {
                symbolComponents.AddModifier("override");
            }

            // Set return valueType
            symbolComponents.ReturnType = propertyData.PropertyTypeData.CompactSymbolComponentInfo;

            if (symbolAttributes.HasFlag(SymbolAttributes.IndexerProperty))
            {
                _ = symbolComponents.IsIndexer = true;

                ParameterData[] parameters = propertyData.IndexerParameters;
                if (parameters.Any())
                {
                    foreach (ParameterData parameter in parameters)
                    {
                        SymbolComponentInfo parameterInfo = parameter.SymbolComponentInfo;
                        symbolComponents.AddParameter(parameterInfo);
                    }
                }
            }
            else
            {
                _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(propertyData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);
            }

            if (propertyData.CanRead)
            {
                var propertyGet = new SymbolComponentInfo("get", isKeyword: true);
                if (HelperExtensionsCommon.AccessModifierComparer.Compare(propertyData.GetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    propertyGet.AddModifier(propertyData.GetAccessorAccessModifier.ToDisplayStringValue());
                }

                symbolComponents.PropertyGet = propertyGet;
            }

            if (propertyData.CanWrite)
            {
                bool isInitProperty = propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty);
                string accessor = isInitProperty
                  ? "init"
                  : "set";
                var propertySet = new SymbolComponentInfo(accessor, isKeyword: true);
                if (HelperExtensionsCommon.AccessModifierComparer.Compare(propertyData.SetAccessorAccessModifier, propertyData.AccessModifier) < 0)
                {
                    propertySet.AddModifier(propertyData.SetAccessorAccessModifier.ToDisplayStringValue());
                }

                if (propertyData.IsSetMethodReadOnly)
                {
                    propertySet.AddModifier("readonly");
                }

                symbolComponents.PropertySet = propertySet;
            }

            return symbolComponents;
        }

        /// <summary>
        /// Creates a SymbolComponentInfo instance representing the signature components of a constructor, based on the
        /// specified formatting and inclusion options.
        /// </summary>
        /// <remarks>When isCompact is set to true, custom attributes are excluded from the signature. The
        /// isFullyQualifiedName and isDeclaringTypeIncluded parameters control the level of detail included in the
        /// constructor's name within the signature.</remarks>
        /// <param name="constructorData">The metadata describing the constructor, including its attributes, access modifier, parameters, and other
        /// relevant information.</param>
        /// <param name="isFullyQualifiedName">true to include the fully qualified name of the constructor in the signature; otherwise, false.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the constructor's signature; otherwise, false.</param>
        /// <param name="isCompact">true to generate a compact signature that omits custom attributes; otherwise, false to include custom
        /// attributes in the signature.</param>
        /// <returns>A SymbolComponentInfo object containing the formatted signature components of the specified constructor.</returns>
        internal static SymbolComponentInfo ToSignatureComponentsInternal(ConstructorData constructorData, bool isFullyQualifiedName, bool isDeclaringTypeIncluded, bool isCompact)
        {
            SymbolComponentInfo symbolComponents = new SymbolComponentInfo(isKeyword: false)
            {
                IsSymbol = true,
                HasExpressionTerminator = true,
            };

            SymbolAttributes symbolAttributes = constructorData.SymbolAttributes;
            IEnumerable<CustomAttributeData> customAttributesData = constructorData.AttributeData;
            PooledStringBuilder signatureNameBuilder = StringBuilderFactory.GetOrCreate();

            if (symbolAttributes.HasFlag(SymbolAttributes.Final))
            {
                customAttributesData = customAttributesData.Where(attributeData => attributeData.AttributeType != HelperExtensionsCommon.IsReadOnlyAttributeType);
            }

            if (!isCompact)
            {
                AddCustomAttributes(symbolComponents, customAttributesData);
            }

            AccessModifier accessModifier = constructorData.AccessModifier;
            symbolComponents.AddModifier(accessModifier.ToDisplayStringValue());

            if (constructorData.IsStatic)
            {
                symbolComponents.AddModifier("static");
            }

            // Member name
            _ = symbolComponents.NameBuilder.AppendDisplayNameInternal(constructorData, isFullyQualifiedName, isGenericTypeParameterIncluded: false, isDeclaringTypeIncluded);

            ParameterData[] parameters = constructorData.Parameters;
            if (parameters.Length > 0)
            {
                foreach (ParameterData parameterData in parameters)
                {
                    SymbolComponentInfo parameterInfo = parameterData.SymbolComponentInfo;
                    symbolComponents.AddParameter(parameterInfo);
                }
            }

            return symbolComponents;
        }

        private static PooledStringBuilder AppendCustomAttributes(this PooledStringBuilder nameBuilder, IEnumerable<CustomAttributeData> attributes, bool isAppendNewLineEnabled)
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

                    _ = nameBuilder.Append(constructorPositionalArgument.Value.ToArgumentDisplayValue())
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
                {
                    hasAttributeArguments = true;

                    _ = nameBuilder.Append(constructorNamedArgument.MemberName)
                      .Append(" = ")
                      .Append(constructorNamedArgument.TypedValue.Value.ToArgumentDisplayValue())
                      .Append(HelperExtensionsCommon.ParameterSeparator);
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
                    _ = nameBuilder.Append(' ');
                }
            }

            return nameBuilder;
        }

        private static void AddCustomAttributes(SymbolComponentInfo symbolComponentInfo, IEnumerable<CustomAttributeData> attributes)
        {
            foreach (CustomAttributeData attribute in attributes)
            {
                if (HelperExtensionsCommon.IgnorableParameterAttributes.Contains(attribute.AttributeType.Name))
                {
                    continue;
                }

                var customAttributeInfo = new SymbolComponentInfo(attribute.AttributeType.Name);
                symbolComponentInfo.AddCustomAttribute(customAttributeInfo);

                foreach (CustomAttributeTypedArgument constructorPositionalArgument in attribute.ConstructorArguments)
                {
                    string customAttributeConstructorArg = constructorPositionalArgument.Value.ToArgumentDisplayValue();
                    customAttributeInfo.AddCustomAttributeConstructorArg(customAttributeConstructorArg);
                }

                foreach (CustomAttributeNamedArgument constructorNamedArgument in attribute.NamedArguments)
                {
                    string propertyName = constructorNamedArgument.MemberName;
                    string propertyValue = constructorNamedArgument.TypedValue.Value.ToArgumentDisplayValue();
                    customAttributeInfo.AddCustomAttributeNamedArg((propertyName, propertyValue));
                }
            }
        }

        private static string ToArgumentDisplayValue(this object value)
        {
            switch (value)
            {
                case string stringValue:
                    return $"\"{stringValue}\"";
                case char charValue:
                    return $"'{charValue}'";
                case double doubleValue:
                    return string.Format(CultureInfo.InvariantCulture, "{0}", doubleValue);
                case Enum enumValue:
                    return $"{value.GetType().ToDisplayName()}.{enumValue.ToString()}";
                case Type type:
                    return $"typeof({type.ToDisplayName()})";
                case IEnumerable enumerableValue:
                    return $"new[] {{ {string.Join(", ", enumerableValue.OfType<object>().Select(val => val.ToArgumentDisplayValue()))} }}";
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        /// <param genericTypeParameterIdentifier="valueType"></param>
        /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="valueType"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="valueType"/>.</exception>
        /// <exception cref="NotSupportedException">The valueType provided by the <paramref genericTypeParameterIdentifier="valueType"/> is not supported.</exception>
        /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
        public static AccessModifier GetAccessModifier(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return entry.AccessModifier;
        }

        /// <summary>
        /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        /// <param genericTypeParameterIdentifier="valueType"></param>
        /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="valueType"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="valueType"/>.</exception>
        /// <exception cref="NotSupportedException">The valueType provided by the <paramref genericTypeParameterIdentifier="valueType"/> is not supported.</exception>
        /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
        public static AccessModifier GetAccessModifier(this MethodInfo method)
        {
            ArgumentNullExceptionEx.ThrowIfNull(method, nameof(method));

            MethodData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(method);
            return entry.AccessModifier;
        }

        /// <summary>
        /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        /// <param genericTypeParameterIdentifier="valueType"></param>
        /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="valueType"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="valueType"/>.</exception>
        /// <exception cref="NotSupportedException">The valueType provided by the <paramref genericTypeParameterIdentifier="valueType"/> is not supported.</exception>
        /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
        public static AccessModifier GetAccessModifier(this ConstructorInfo constructor)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructor, nameof(constructor));

            ConstructorData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructor);
            return entry.AccessModifier;
        }

        /// <summary>
        /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        /// <param genericTypeParameterIdentifier="valueType"></param>
        /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="valueType"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="valueType"/>.</exception>
        /// <exception cref="NotSupportedException">The valueType provided by the <paramref genericTypeParameterIdentifier="valueType"/> is not supported.</exception>
        /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
        public static AccessModifier GetAccessModifier(this PropertyInfo property)
        {
            ArgumentNullExceptionEx.ThrowIfNull(property, nameof(property));

            PropertyData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(property);
            return entry.AccessModifier;
        }

        /// <summary>
        /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        /// <param genericTypeParameterIdentifier="valueType"></param>
        /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="valueType"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="valueType"/>.</exception>
        /// <exception cref="NotSupportedException">The valueType provided by the <paramref genericTypeParameterIdentifier="valueType"/> is not supported.</exception>
        /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
        public static AccessModifier GetAccessModifier(this EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return entry.AccessModifier;
        }

        /// <summary>
        /// Gets the access modifier for <see cref="MemberInfo"/> symbolAttributes like <see cref="Type"/>, <see cref="MethodInfo"/>, <see cref="ConstructorInfo"/>, <see cref="PropertyInfo"/>, <see cref="EventInfo"/> or <see cref="FieldInfo"/>.
        /// </summary>
        /// <param genericTypeParameterIdentifier="valueType"></param>
        /// <returns>The <see cref="AccessModifier"/> for the current <paramref genericTypeParameterIdentifier="valueType"/>.</returns>
        /// <exception cref="InvalidOperationException">Unable to identify the accessibility of the <paramref genericTypeParameterIdentifier="valueType"/>.</exception>
        /// <exception cref="NotSupportedException">The valueType provided by the <paramref genericTypeParameterIdentifier="valueType"/> is not supported.</exception>
        /// <remarks>For a <see cref="PropertyInfo"/> the property accessors with the least restriction provides the access modifier for the property. This is a compiler rule.</remarks>
        public static AccessModifier GetAccessModifier(this FieldInfo field)
        {
            ArgumentNullExceptionEx.ThrowIfNull(field, nameof(field));

            FieldData entry = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(field);
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
            ConstructorInfo constructorInfo = constructorData.GetConstructorInfo();
            return constructorInfo.IsPublic ? AccessModifier.Public
              : constructorInfo.IsPrivate ? AccessModifier.Private
              : constructorInfo.IsAssembly ? AccessModifier.Internal
              : constructorInfo.IsFamily ? AccessModifier.Protected
              : constructorInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
              : constructorInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
              : constructorInfo.IsStatic ? AccessModifier.Undefined
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
        /// Gets the ordered base type hierarchy (ancestor inheritance tree) of a specified type, starting from the root type (most distant base class or most distant implemented interface) and includes the current type (the tree's leaf) as the last item.
        /// </summary>
        /// <param name="type">The type of which the ancestor hierarchy to return.</param>
        /// <param name="includeInterfaces"><see langword="true"/> if interfaces should be included. Otherwise <see langword="false"/>. 
        /// If type <paramref name="type"/> is itself an interface then the <paramref name="includeInterfaces"/> parameter is ignored and all implemented interfaces will be returned.
        /// The default is <see langword="false"/>.</param>
        /// <returns>A <see cref="List{Type}"/> that contains the ordered base types of <paramref name="type"/> (ancestor hierarchy) starting with the root (the most distant base class or most distant interface in case <paramref name="includeInterfaces"/> evaluates to <see langword="true"/>). 
        /// If <paramref name="includeInterfaces"/> is <see langword="true"/> then the result also contains all implemented interfaces. 
        /// <br/>The last item in the collection is always the current <paramref name="type"/> value (the hierarchy leaf). 
        /// <br/>The language base types <see cref="object" /> and <see cref="ValueType"/> are excluded from the result, except the current <paramref name="type"/> is itself of type <see cref="object"/>.
        /// <br/>If <paramref name="type"/> does not have a parent inheritance tree or does not implement any interfaces or is of type <see cref="object"/> or a value type (<see cref="Type.IsValueType"/> returns <see langword="true"/>) then the result collection will only contain the current <paramref name="type"/> value.</returns>
        /// <remarks>The type <see cref="object"/> (the root type for reference types) and the type <see cref="ValueType"/> (the base type for value types) are not included in the hierarchy.
        /// <br/>This means, if <paramref name="type"/> is of type <see cref="object"/> or a value type (<see cref="Type.IsValueType"/> returns <see langword="true"/>) then the result will only contain the current <paramref name="type"/> or in case of a value type additionally the implemented interfaces.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
        public static ImmutableList<Type> GetTypeHierarchy(this Type type, bool includeInterfaces = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            if (type == typeof(object) || type.IsValueType)
            {
                return ImmutableList.Create<Type>();
            }

            if (type.IsInterface)
            {
                return includeInterfaces
                  ? ImmutableList.Create(type.GetInterfaces()).Add(type)
                  : ImmutableList.Create<Type>();
            }

            Type subType = type;
            var subTypes = new Stack<Type>();
            if (type == typeof(object))
            {
                subTypes.Push(type);
            }
            else
            {
                while (subType.BaseType != null
                  && subType.BaseType != typeof(object))
                {
                    subType = subType.BaseType;
                    subTypes.Push(subType);
                }
            }

            if (includeInterfaces)
            {
                subTypes.AddRange(type.GetInterfaces());
            }

            return subTypes.ToImmutableList();
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.DisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this MethodInfo methodInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return isDeclaringTypeIncluded
                ? methodData.DisplayName
                : methodData.ShortDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this ConstructorInfo constructorInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return isDeclaringTypeIncluded
                ? constructorData.DisplayName
                : constructorData.ShortDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this PropertyInfo propertyInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return isDeclaringTypeIncluded
                ? propertyData.DisplayName
                : propertyData.ShortDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this FieldInfo fieldInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return isDeclaringTypeIncluded
                ? fieldData.DisplayName
                : fieldData.ShortDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
            return parameterData.DisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToDisplayName(this EventInfo eventInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return isDeclaringTypeIncluded
                ? eventData.DisplayName
                : eventData.ShortDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic type name to a readable fully qualified display name.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToFullDisplayName(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.FullyQualifiedDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToFullDisplayName(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.FullyQualifiedDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToFullDisplayName(this ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            return constructorData.FullyQualifiedDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbol Namespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToFullDisplayName(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.FullyQualifiedDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToFullDisplayName(this FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return fieldData.FullyQualifiedDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic member names to a readable display genericTypeParameterIdentifier without the symbolNamespace.
        /// </summary>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        public static string ToFullDisplayName(this EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            return eventData.FullyQualifiedDisplayName;
        }

        /// <summary>
        /// Extension method to convert generic and non-generic valueType names to a readable display genericTypeParameterIdentifier including the symbolNamespace.
        /// </summary>
        /// <param genericTypeParameterIdentifier="memberInfo">The <see cref="Type"/> to extend.</param>
        /// <returns>
        /// A readable genericTypeParameterIdentifier of valueType members, especially generic members. For example, <c>"Task.Run`1"</c> becomes <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
        /// </returns>
        /// <remarks>
        /// <para>Because <see cref="Type"/> derives from <see cref="MemberInfo"/> this extension method also works on <see cref="Type"/>.</para>
        /// Usually <see cref="MemberInfo.Name"/> for generic members like <c>"Task.Run&lt;TResult&gt;"</c> would return <c>"Task.Run`1"</c>. 
        /// <br/>This helper unwraps the generic valueType parameters to construct the full valueType genericTypeParameterIdentifier like <c>"System.Threading.Tasks.Task.Run&lt;TResult&gt;"</c>.
        /// </remarks>
        internal static string ToDisplayNameInternal(SymbolInfoData symbolInfoData, bool isFullyQualifiedName, bool isGenericTypeParameterIncluded, bool isDeclaringTypeIncluded)
        {
            PooledStringBuilder nameBuilder = StringBuilderFactory.GetOrCreate();

            switch (symbolInfoData)
            {
                case ParameterData parameterData:
                    _ = nameBuilder.AppendDisplayNameInternal(parameterData);
                    break;
                case TypeData typeData:
                    _ = nameBuilder.AppendDisplayNameInternal(typeData, isFullyQualifiedName, isGenericTypeParameterIncluded);
                    break;
                case MemberInfoData memberInfoData:
                    _ = nameBuilder.AppendDisplayNameInternal(memberInfoData, isFullyQualifiedName, isGenericTypeParameterIncluded, isDeclaringTypeIncluded);
                    break;
                default:
                    throw new NotImplementedException();
            }

            string symbolName = nameBuilder.ToString();
            nameBuilder.Recycle();

            return symbolName;
        }

        /// <summary>
        /// Appends a human-readable display name for the specified type to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>The display name includes type information in a format suitable for display in user
        /// interfaces or logs. If the type is a generic type and isGenericTypeParameterIncluded is true, the generic
        /// type parameters are included in the display name.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the display name of the type will be appended. Cannot be null.</param>
        /// <param name="type">The type whose display name is to be appended. Cannot be null.</param>
        /// <param name="isGenericTypeParameterIncluded">true to include generic type parameter names in the display name; otherwise, false. The default is true.</param>
        /// <returns>The StringBuilder instance with the appended display name.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, Type type, bool isGenericTypeParameterIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), typeData, isFullyQualifiedName: false, isGenericTypeParameterIncluded);
            return nameBuilder;
        }

        /// <summary>
        /// Appends a display-friendly name for the specified method to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>This method is useful for generating human-readable representations of method
        /// signatures, such as for logging or diagnostic purposes. The format of the display name may vary depending on
        /// the method's characteristics and the value of isDeclaringTypeIncluded.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the display name of the method will be appended. Cannot be null.</param>
        /// <param name="methodInfo">The MethodInfo representing the method whose display name is to be appended. Cannot be null.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the display name; otherwise, false. The default is false.</param>
        /// <returns>The StringBuilder instance with the method's display name appended.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, MethodInfo methodInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), methodData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        /// <summary>
        /// Appends a display-friendly name for the specified event to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>This method does not clear or reset the contents of the StringBuilder. It appends the
        /// event's display name to the existing content. The format of the display name may include the declaring type
        /// if isDeclaringTypeIncluded is set to true.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the event's display name will be appended. Cannot be null.</param>
        /// <param name="eventInfo">The EventInfo representing the event whose display name is to be appended. Cannot be null.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the display name; otherwise, false. The default is false.</param>
        /// <returns>The same StringBuilder instance provided in nameBuilder, with the event's display name appended.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, EventInfo eventInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), eventData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        /// <summary>
        /// Appends the display name of the specified constructor to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>The display name includes the constructor's signature and, optionally, the declaring
        /// type if specified. This method does not clear or reset the StringBuilder; it appends to its existing
        /// content.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the constructor's display name will be appended. Cannot be null.</param>
        /// <param name="constructorInfo">The ConstructorInfo representing the constructor whose display name is to be appended. Cannot be null.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the display name; otherwise, false. The default is false.</param>
        /// <returns>The StringBuilder instance with the constructor's display name appended.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, ConstructorInfo constructorInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), constructorData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        /// <summary>
        /// Appends a display-friendly name for the specified property to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>This method is useful for generating human-readable representations of property
        /// names, such as for logging or UI display. The format of the display name may vary depending on whether the
        /// declaring type is included.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the property's display name will be appended. Cannot be null.</param>
        /// <param name="propertyInfo">The PropertyInfo representing the property whose display name is to be appended. Cannot be null.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the display name; otherwise, false. The default is false.</param>
        /// <returns>The same StringBuilder instance provided in nameBuilder, with the property's display name appended.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, PropertyInfo propertyInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), propertyData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        /// <summary>
        /// Appends a display-friendly name for the specified parameter to the provided StringBuilder instance.
        /// </summary>
        /// <param name="nameBuilder">The StringBuilder to which the display name will be appended. Cannot be null.</param>
        /// <param name="parameterInfo">The ParameterInfo representing the parameter whose display name is to be appended. Cannot be null.</param>
        /// <returns>The same StringBuilder instance with the display name of the parameter appended.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), parameterData);
            return nameBuilder;
        }

        /// <summary>
        /// Appends the display name of the specified field to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>The display name includes the field's name and, optionally, its declaring type if
        /// isDeclaringTypeIncluded is set to true. This method does not clear or reset the StringBuilder; it appends to
        /// its existing content.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the display name will be appended. Cannot be null.</param>
        /// <param name="fieldInfo">The FieldInfo representing the field whose display name is to be appended. Cannot be null.</param>
        /// <param name="isDeclaringTypeIncluded">true to include the declaring type in the display name; otherwise, false. The default is false.</param>
        /// <returns>The same StringBuilder instance with the field's display name appended.</returns>
        public static StringBuilder AppendDisplayName(this StringBuilder nameBuilder, FieldInfo fieldInfo, bool isDeclaringTypeIncluded = false)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), fieldData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        /// <summary>
        /// Appends the fully qualified display name of the specified type to the provided StringBuilder instance.
        /// </summary>
        /// <remarks>This method appends the namespace and type name, including generic type parameters if
        /// specified, to the end of the provided StringBuilder. The method does not clear or modify the existing
        /// contents of the StringBuilder except to append the type's display name.</remarks>
        /// <param name="nameBuilder">The StringBuilder to which the fully qualified display name will be appended. Cannot be null.</param>
        /// <param name="type">The type whose fully qualified display name is to be appended. Cannot be null.</param>
        /// <param name="isGenericTypeParameterIncluded">true to include generic type parameter names in the display name; otherwise, false. The default is true.</param>
        /// <returns>The StringBuilder instance with the fully qualified display name of the specified type appended.</returns>
        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, Type type, bool isGenericTypeParameterIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), typeData, isFullyQualifiedName: true, isGenericTypeParameterIncluded);
            return nameBuilder;
        }

        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, MethodInfo methodInfo, bool isDeclaringTypeIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), methodData, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, EventInfo eventInfo, bool isDeclaringTypeIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));

            EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), eventData, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, ConstructorInfo constructorInfo, bool isDeclaringTypeIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), constructorData, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, PropertyInfo propertyInfo, bool isDeclaringTypeIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), propertyData, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), parameterData);
            return nameBuilder;
        }

        public static StringBuilder AppendFullDisplayName(this StringBuilder nameBuilder, FieldInfo fieldInfo, bool isDeclaringTypeIncluded = true)
        {
            ArgumentNullExceptionEx.ThrowIfNull(nameBuilder, nameof(nameBuilder));
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            _ = AppendDisplayNameInternal(PooledStringBuilder.Create(nameBuilder), fieldData, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded);
            return nameBuilder;
        }

        private static PooledStringBuilder AppendDisplayNameInternal(this PooledStringBuilder nameBuilder, TypeData typeData, bool isFullyQualifiedName, bool isGenericTypeParameterIncluded)
        {
            Type type = typeData.GetType();
            if (typeData.IsByRef)
            {
                typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type.GetElementType());
                type = typeData.GetType();
            }

            var typeReference = new CodeTypeReference(type);
            ReadOnlySpan<char> typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference).AsSpan();

            if (typeData.IsGenericType)
            {
                int startIndexOfGenericTypeParameters = typeName.IndexOf('<');
                typeName = typeName.Slice(0, startIndexOfGenericTypeParameters);
            }

            if (!isFullyQualifiedName)
            {
                int startIndexOfUnqualifiedTypeName = typeName.LastIndexOf('.') + 1;
                if (startIndexOfUnqualifiedTypeName > 0)
                {
                    typeName = typeName.Slice(startIndexOfUnqualifiedTypeName);
                }
            }

            _ = nameBuilder.Append(typeName.ToArray());

            if (isGenericTypeParameterIncluded && typeData.IsGenericType)
            {
                _ = nameBuilder.AppendGenericTypeArguments(typeData, isFullyQualifiedName);
            }

            return nameBuilder;
        }

        private static PooledStringBuilder AppendDisplayNameInternal(this PooledStringBuilder nameBuilder, ParameterData parameterData)
          => nameBuilder.Append(parameterData.Name);

        private static PooledStringBuilder AppendDisplayNameInternal(this PooledStringBuilder nameBuilder, MemberInfoData memberInfoData, bool isFullyQualifiedName, bool isGenericTypeParameterIncluded, bool isDeclaringTypeIncluded)
        {
            if (isFullyQualifiedName || isDeclaringTypeIncluded)
            {
                _ = nameBuilder.AppendDisplayNameInternal(memberInfoData.DeclaringTypeData, isFullyQualifiedName, isGenericTypeParameterIncluded: true)
                  .Append('.');
            }

            if (memberInfoData.SymbolAttributes.HasFlag(SymbolAttributes.Constructor))
            {
                _ = nameBuilder.AppendDisplayNameInternal(memberInfoData.DeclaringTypeData, isFullyQualifiedName: false, isGenericTypeParameterIncluded: false);
            }
            else if (memberInfoData is PropertyData propertyData)
            {
                _ = nameBuilder.Append(propertyData.Name);

                if (propertyData.IsIndexer)
                {
                    _ = nameBuilder.Append('[');

                    foreach (ParameterData indexerParameter in propertyData.IndexerParameters)
                    {
                        _ = nameBuilder.Append(indexerParameter.ParameterTypeData.ShortDisplayName);
                    }

                    _ = nameBuilder.Append(']');
                }
            }
            else
            {
                _ = nameBuilder.Append(memberInfoData.Name);

                if (isGenericTypeParameterIncluded
                  && memberInfoData.SymbolAttributes.HasFlag(SymbolAttributes.GenericMethod)
                  && memberInfoData is MethodData methodData)
                {
                    _ = nameBuilder.AppendGenericTypeArguments(methodData, isFullyQualifiedName);
                }
            }

            return nameBuilder;
        }

        private static PooledStringBuilder AppendGenericTypeArguments(this PooledStringBuilder nameBuilder, MethodData methodData, bool isFullyQualified)
        {
            if (!methodData.IsGenericMethod)
            {
                return nameBuilder;
            }

            // Could be an open generic valueType. Therefore we need to obtain all definitions.
            TypeData[] genericTypeArguments = methodData.GenericTypeArguments;
            TypeData[] genericTypeParameterDefinitions = methodData.IsGenericMethodDefinition
              ? methodData.GenericTypeArguments
              : Array.Empty<TypeData>();

            AppendGenericParameters(nameBuilder, isFullyQualified, genericTypeParameterDefinitions, genericTypeArguments);
            return nameBuilder;
        }

        private static PooledStringBuilder AppendGenericTypeArguments(this PooledStringBuilder nameBuilder, TypeData typeData, bool isFullyQualified)
        {
            if (!typeData.IsGenericType)
            {
                return nameBuilder;
            }

            // Could be an open generic valueType. Therefore we need to obtain all definitions.
            TypeData[] genericTypeArguments = typeData.GenericTypeArguments;
            TypeData[] genericTypeParameterDefinitions = typeData.IsGenericTypeDefinition
              ? typeData.GenericTypeArguments
              : Array.Empty<TypeData>();

            AppendGenericParameters(nameBuilder, isFullyQualified, genericTypeParameterDefinitions, genericTypeArguments);
            return nameBuilder;
        }

        private static void AppendGenericParameters(PooledStringBuilder nameBuilder, bool isFullyQualified, TypeData[] genericTypeParameterDefinitions, TypeData[] genericTypeArguments)
        {
            _ = nameBuilder.Append('<');
            for (int typeArgumentIndex = 0; typeArgumentIndex < genericTypeArguments.Length; typeArgumentIndex++)
            {
                TypeData genericParameterTypeData = genericTypeArguments[typeArgumentIndex];
                if (genericTypeParameterDefinitions.Length > 0)
                {
                    TypeData genericTypeParameterDefinitionData = genericTypeParameterDefinitions[typeArgumentIndex];
                    if ((genericTypeParameterDefinitionData.GenericParameterAttributes & GenericParameterAttributes.Covariant) != 0)
                    {
                        _ = nameBuilder.Append("out")
                          .Append(' ');
                    }
                    else if ((genericTypeParameterDefinitionData.GenericParameterAttributes & GenericParameterAttributes.Contravariant) != 0)
                    {
                        _ = nameBuilder.Append("in")
                          .Append(' ');
                    }
                }

                _ = nameBuilder.AppendDisplayNameInternal(genericParameterTypeData, isFullyQualified, isGenericTypeParameterIncluded: true)
                  .Append(HelperExtensionsCommon.ParameterSeparator);
            }

            // Remove trailing comma and whitespace
            _ = nameBuilder.Remove(nameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length)
              .Append('>');
        }

        private static PooledStringBuilder AppendGenericTypeConstraints(this PooledStringBuilder constraintBuilder, TypeData[] genericTypeDefinitionsData, bool isFullyQualified, bool isSingleLine, ReadOnlySpan<char> lineIndentation)
        {
            bool hasSingleNewLine = false;
            for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
            {
                TypeData genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
                TypeData[] constraints = genericTypeDefinitionData.GenericParameterConstraintsData;
                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
                  && constraints.Length == 0)
                {
                    continue;
                }

                if (isSingleLine)
                {
                    if (!hasSingleNewLine)
                    {
                        _ = constraintBuilder.AppendLine()
                        .Append(lineIndentation);
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
                      .Append(lineIndentation);
                }

                _ = constraintBuilder.Append("where")
                  .Append(' ')
                  .Append(genericTypeDefinitionData.Name)
                  .Append(" : ");

                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    _ = constraintBuilder.Append("class")
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    _ = constraintBuilder.Append("struct")
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                foreach (TypeData constraintData in constraints)
                {
                    _ = constraintBuilder.AppendDisplayNameInternal(constraintData, isFullyQualified, isGenericTypeParameterIncluded: true)
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    _ = constraintBuilder.Append("new()")
                      .Append(HelperExtensionsCommon.ParameterSeparator);
                }

                _ = constraintBuilder.Remove(constraintBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
            }

            return constraintBuilder;
        }

        private static void AddGenericTypeConstraints(SymbolComponentInfo symbolComponents, TypeData[] genericTypeDefinitionsData, bool isFullyQualified)
        {
            for (int genericTypeArgumentIndex = 0; genericTypeArgumentIndex < genericTypeDefinitionsData.Length; genericTypeArgumentIndex++)
            {
                TypeData genericTypeDefinitionData = genericTypeDefinitionsData[genericTypeArgumentIndex];
                var constraintComponents = new SymbolComponentInfo(genericTypeDefinitionData.Name);
                TypeData[] constraints = genericTypeDefinitionData.GenericParameterConstraintsData;
                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask) == GenericParameterAttributes.None
                  && constraints.Length == 0)
                {
                    continue;
                }

                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    var constraint = new SymbolComponentInfo("class", isKeyword: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                if ((genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    var constraint = new SymbolComponentInfo("struct", isKeyword: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                foreach (TypeData constraintData in constraints)
                {
                    var constraint = new SymbolComponentInfo(isKeyword: constraintData.IsBuiltInType);
                    _ = constraint.NameBuilder.AppendDisplayNameInternal(constraintData, isFullyQualified, isGenericTypeParameterIncluded: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                if (!genericTypeDefinitionData.IsValueType && (genericTypeDefinitionData.GenericParameterAttributes & GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    var constraint = new SymbolComponentInfo("new()", isKeyword: true);
                    constraintComponents.AddGenericTypeConstraint(constraint);
                }

                symbolComponents.AddGenericTypeConstraint(constraintComponents);
            }
        }

        private static PooledStringBuilder AppendInheritanceSignature(this PooledStringBuilder memberNameBuilder, TypeData typeData, bool isFullyQualified)
        {
            if (typeData.IsDelegate)
            {
                return memberNameBuilder;
            }

            bool isSubclass = typeData.IsSubclass;
            TypeData[] interfaces = typeData.InterfacesData;
            bool hasInterfaces = interfaces.Length > 0;
            if (isSubclass || hasInterfaces)
            {
                _ = memberNameBuilder.Append(" : ");
            }

            if (isSubclass)
            {
                _ = memberNameBuilder.Append(isFullyQualified ? typeData.BaseTypeData.GetType().FullName : typeData.BaseTypeData.Name)
                  .Append(HelperExtensionsCommon.ParameterSeparator);
            }

            foreach (TypeData interfaceData in interfaces)
            {
                _ = memberNameBuilder.Append(isFullyQualified ? interfaceData.GetType().FullName : interfaceData.Name)
                  .Append(HelperExtensionsCommon.ParameterSeparator);
            }

            if (isSubclass || hasInterfaces)
            {
                _ = memberNameBuilder.Remove(memberNameBuilder.Length - HelperExtensionsCommon.ParameterSeparator.Length, HelperExtensionsCommon.ParameterSeparator.Length);
            }

            return memberNameBuilder;
        }

        private static void AddInheritanceSignature(SymbolComponentInfo symbolComponents, TypeData typeData, bool isFullyQualified)
        {
            if (typeData.IsDelegate)
            {
                return;
            }

            bool isSubclass = typeData.IsSubclass;
            TypeData[] interfaces = typeData.InterfacesData;
            if (isSubclass)
            {
                var inheritedTypeComponent = new SymbolComponentInfo(isKeyword: typeData.BaseTypeData.IsBuiltInType);
                _ = inheritedTypeComponent.NameBuilder.Append(isFullyQualified ? typeData.BaseTypeData.FullyQualifiedDisplayName : typeData.BaseTypeData.Name);
                symbolComponents.AddInheritedType(inheritedTypeComponent);
            }

            foreach (TypeData interfaceData in interfaces)
            {
                var inheritedTypeComponent = new SymbolComponentInfo(isKeyword: false);
                _ = inheritedTypeComponent.NameBuilder.Append(isFullyQualified ? interfaceData.FullyQualifiedDisplayName : interfaceData.Name);
                symbolComponents.AddInheritedType(inheritedTypeComponent);
            }
        }

        // TODO::Test if checking get() is enough to determine if a property is overridden
        public static bool IsDelegate(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.SymbolAttributes.HasFlag(SymbolAttributes.Delegate);
        }

        internal static bool IsDelegateInternal(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            return HelperExtensionsCommon.DelegateType.IsAssignableFrom(type);
        }

        // TODO::Test if checking get() is enough to determine if a property is overridden
        public static bool IsOverride(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData memberInfoData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return memberInfoData.IsOverride;
        }

        internal static bool IsOverrideInternal(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            return propertyInfo.CanRead ? propertyInfo.GetGetMethod(true).IsOverride() : propertyInfo.GetSetMethod().IsOverride();
        }

        public static bool IsConst(this FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            FieldData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
            return methodData.SymbolAttributes.HasFlag(SymbolAttributes.Constant);
        }

        internal static bool IsConstInternal(FieldData fieldData)
          => fieldData.GetFieldInfo().IsLiteral;

        public static bool IsOverride(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.IsOverride;
        }

#if NET
        public static bool IsInitOnly(this PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
            return propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty);
        }

        internal static bool IsInitOnlyInternal(PropertyData propertyData)
        {
            if (propertyData.CanWrite)
            {
                Type[] requiredModifiers = propertyData.SetMethodData.GetMethodInfo().ReturnParameter.GetRequiredCustomModifiers();
                if (requiredModifiers.Length > 0)
                {
                    return requiredModifiers.FirstOrDefault(type => type == typeof(IsExternalInit)) != default;
                }
            }

            return false;
        }
#endif

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
        /// <remarks>The method first checks if the return valueType is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned valueType (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate valueType (awaiter).
        /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned valueType that would make the valueType awaitable. If this fails too, the method is not awaitable.</remarks>
        public static bool IsAwaitable(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.IsAwaitable;
        }

        /// <summary>
        /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
        /// </summary>
        /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
        /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
        /// <remarks>The method first checks if the return valueType is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned valueType (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate valueType (awaiter).
        /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned valueType that would make the valueType awaitable. If this fails too, the method is not awaitable.</remarks>
        internal static bool IsAwaitableInternal(MethodData methodData)
          => IsAwaitableInternal(methodData.ReturnTypeData);

        /// <summary>
        /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
        /// </summary>
        /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
        /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
        /// <remarks>The method first checks if the return valueType is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned valueType (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate valueType (awaiter).
        /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned valueType that would make the valueType awaitable. If this fails too, the method is not awaitable.</remarks>
        public static bool IsAwaitable(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.IsAwaitable;
        }

        /// <summary>
        /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
        /// </summary>
        /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
        /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
        /// <remarks>The method first checks if the return valueType is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned valueType (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate valueType (awaiter).
        /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned valueType that would make the valueType awaitable. If this fails too, the method is not awaitable.</remarks>
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

            // The return valueType of the method is not directly returning an awaitable valueType.
            // So, search for an extension method named "GetAwaiter" for the return valueType of the currently validated method that effectively converts the valueType into an awaitable object.
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

        internal static bool IsAwaitableTask(Type type)
          => HelperExtensionsCommon.TaskType.IsAssignableFrom(type)
            || HelperExtensionsCommon.TaskType.IsAssignableFrom(type.BaseType);

        internal static bool IsAwaitableValueTask(Type type)
          => HelperExtensionsCommon.ValueTaskType == type
            || (type.IsGenericType && HelperExtensionsCommon.ValueTaskGenericType == type.GetGenericTypeDefinition());

        public static bool IsMarkedAsync(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.IsAsync;
        }

        internal static bool IsMarkedAsyncInternal(MethodData methodData)
          => methodData.GetMethodInfo().GetCustomAttribute(HelperExtensionsCommon.AsyncStateMachineAttributeType) != null;

        /// <summary>
        /// Extension method to check if a <see cref="Type"/> is static.
        /// </summary>
        /// <param genericTypeParameterIdentifier="type">The extended <see cref="Type"/> instance.</param>
        /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="type"/> is static. Otherwise <see langword="false"/>.</returns>
        public static bool IsStatic(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.IsStatic;
        }

        internal static bool IsStaticInternal(TypeData typeData)
          => typeData.IsAbstract && typeData.IsSealed;

        public static bool IsBuiltInType(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.IsBuiltInType;
        }

        internal static bool IsBuiltInTypeInternal(TypeData typeData)
        {
            var typeReference = new CodeTypeReference(typeData.GetType());
            string typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference);
            int typeNameStartIndex = typeName.LastIndexOf('.') + 1;
            if (typeNameStartIndex > 0)
            {
                typeName = typeName.Substring(typeNameStartIndex);
            }

            return !HelperExtensionsCommon.CodeProvider.IsValidIdentifier(typeName);
        }

        /// <summary>
        /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="ref"/> parameter.
        /// </summary>
        /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
        public static bool IsRef(this ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            ParameterData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);
            return typeData.IsRef;
        }

        internal static bool IsRefInternal(ParameterData parameterData)
          => parameterData.IsByRef && !parameterData.IsOut && !parameterData.IsIn;

        /// <summary>
        /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
        /// </summary>
        /// <param genericTypeParameterIdentifier="type">The extended <see cref="Type"/> instance.</param>
        /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="type"/> is allowed to define extension methods. Otherwise <see langword="false"/>.</returns>
        /// <remarks>To be able to define extension methods a class must be static, non-generic, a top level valueType. 
        /// <br/>In addition this method checks if the declaring class and the method are both decorated with the <see cref="ExtensionAttribute"/> which is added by the compiler.</remarks>
        public static bool CanDeclareExtensionMethods(this Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
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

        internal static bool CanDeclareExtensionMethodsInternalUncached(Type type)
        {
            bool isStatic = type.IsAbstract && type.IsSealed;
            if (!isStatic || type.IsNested || type.IsGenericType)
            {
                return false;
            }

            Attribute typeExtensionAttribute = type.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
            return typeExtensionAttribute != null;
        }

        /// <summary>
        /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method.
        /// </summary>
        /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
        /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method. Otherwise <see langword="false"/>.</returns>
        public static bool IsExtensionMethod(this MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
            return methodData.IsExtensionMethod;
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

        internal static bool IsExtensionMethodInternalUncached(MethodInfo methodInfo)
        {
            // Check if the declaring class satisfies the constraints to declare extension methods
            Type declaringType = methodInfo.DeclaringType;
            if (!declaringType.CanDeclareExtensionMethods())
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

            // Must have at least the 'this' parameter
            ParameterInfo[] parameterInfoData = methodInfo.GetParameters();
            if (parameterInfoData.Length < 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method for a particular valueType.
        /// </summary>
        /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
        /// <param genericTypeParameterIdentifier="typeToExtend">The <see cref="Type"/> the <paramref genericTypeParameterIdentifier="methodInfo"/> is expected to extend.</param>
        /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method for <paramref genericTypeParameterIdentifier="typeToExtend"/>. Otherwise <see langword="false"/>.</returns>
        public static bool IsExtensionMethodOf(this MethodInfo methodInfo, Type typeToExtend)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));
            ArgumentNullExceptionEx.ThrowIfNull(typeToExtend, nameof(typeToExtend));

            // Check if the declaring class satisfies the constraints to declare extension methods
            if (!methodInfo.DeclaringType.CanDeclareExtensionMethods())
            {
                return false;
            }

            /* Check if the method satisfies the constraints to act as an extension methods */

            if (!IsExtensionMethodInternalUncached(methodInfo))
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

        /// <summary>
        /// Determines whether the specified type is a read-only struct.
        /// </summary>
        /// <param name="type">The type to evaluate. Cannot be null.</param>
        /// <returns>true if the specified type is a read-only struct; otherwise, false.</returns>
        public static bool IsReadOnlyStruct(Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            TypeData typeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
            return typeData.SymbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct);
        }

        internal static bool IsReadOnlyStructInternal(Type type)
          => type.IsValueType && type.GetCustomAttribute(HelperExtensionsCommon.IsReadOnlyAttributeType) != null;

        //public static object GetAwaiter(this object obj)
        //{
        //  MethodInfo getAwaiterMethodInfo = obj.GetType().GetMethod(nameof(Task.GetAwaiter));
        //  if (getAwaiterMethodInfo != null)
        //  {
        //    return (Task)getAwaiterMethodInfo.Invoke(obj, null);
        //  }

        //  // The return valueType of the method is not directly returning an awaitable valueType.
        //  // So search for an extension method named "GetAwaiter" for the return valueType that effectively converts the valueType into an awaitable object.
        //  foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        //  {
        //    foreach (TypeInfo type in assembly.GetExportedTypes())
        //    {
        //      if (!type.IsSealed || type.IsGenericType)
        //      {
        //        continue;
        //      }

        //      getAwaiterMethodInfo = type.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public, null, new[] { obj.GetType() }, null);
        //      if (getAwaiterMethodInfo != null)
        //      {
        //        return (Task)getAwaiterMethodInfo.Invoke(obj, null);
        //      }

        //      //foreach (MethodInfo extensionMethodCandidate in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
        //      //{
        //      //  if (!extensionMethodCandidate.EventName.Equals(nameof(Task.GetAwaiter), StringComparison.Ordinal))
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

        /// <summary>
        /// Determines the symbol attributes for the specified type represented by the given TypeData instance.
        /// </summary>
        /// <remarks>The returned SymbolAttributes value may include multiple flags combined using a
        /// bitwise OR to represent all applicable characteristics of the type. This method does not perform validation
        /// on the input; callers should ensure that typeData is valid and represents a supported type.</remarks>
        /// <param name="typeData">The TypeData instance representing the type for which to retrieve symbol attributes. Cannot be null.</param>
        /// <returns>A SymbolAttributes value that describes the kind and characteristics of the specified type, such as whether
        /// it is a class, struct, interface, enum, delegate, generic, static, abstract, or final. Returns
        /// SymbolAttributes.Undefined if the type does not match any recognized category.</returns>
        internal static SymbolAttributes GetAttributesInternal(TypeData typeData)
        {
            Type type = typeData.GetType();
            if (IsDelegateInternal(type))
            {
                SymbolAttributes delegateAttributes = SymbolAttributes.Delegate;
                if (typeData.IsGenericType)
                {
                    delegateAttributes |= SymbolAttributes.Generic;
                }

                return delegateAttributes;
            }

            if (type.IsClass)
            {
                SymbolAttributes classAttributes = SymbolAttributes.Class;
                if (type.IsAbstract)
                {
                    classAttributes |= SymbolAttributes.Abstract;
                }

                if (typeData.IsSealed)
                {
                    classAttributes |= SymbolAttributes.Final;
                }

                if (typeData.IsStatic)
                {
                    classAttributes |= SymbolAttributes.Static;
                }

                if (typeData.IsGenericType)
                {
                    classAttributes |= SymbolAttributes.Generic;
                }

                return classAttributes;
            }

            if (type.IsInterface)
            {
                SymbolAttributes interfaceAttributes = SymbolAttributes.Interface;
                return interfaceAttributes;
            }

            if (type.IsEnum)
            {
                SymbolAttributes enumAttributes = SymbolAttributes.Enum;
                return enumAttributes;
            }

            if (type.IsValueType)
            {
                SymbolAttributes structAttributes = SymbolAttributes.Struct;

                if (typeData.IsGenericType)
                {
                    structAttributes |= SymbolAttributes.Generic;
                }

                if (typeData.IsByRefLike)
                {
                    structAttributes |= SymbolAttributes.ByReference;
                }

                bool isReadOnlyStruct = IsReadOnlyStructInternal(type);
                if (isReadOnlyStruct)
                {
                    structAttributes |= SymbolAttributes.Final;
                }
                return structAttributes;
            }

            return SymbolAttributes.Undefined;
        }

        /// <summary>
        /// Determines the set of symbol attributes for the specified property based on its metadata and accessor
        /// methods.
        /// </summary>
        /// <param name="propertyData">The metadata describing the property for which to retrieve symbol attributes. Cannot be null.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that represent the characteristics of the property, such as
        /// whether it is static, abstract, virtual, an indexer, or has other modifiers.</returns>
        internal static SymbolAttributes GetAttributesInternal(PropertyData propertyData)
        {
            SymbolAttributes propertyAttributes = propertyData.IsIndexer
              ? SymbolAttributes.IndexerProperty
              : SymbolAttributes.Property;

            MethodData accessorData = propertyData.GetMethodData ?? propertyData.SetMethodData;
            MethodInfo accessorMethodInfo = accessorData.GetMethodInfo();
            if (!propertyData.CanWrite)
            {
                propertyAttributes |= SymbolAttributes.Final;
            }

            if (IsInitOnlyInternal(propertyData))
            {
                propertyAttributes |= SymbolAttributes.Init;
            }

            if (accessorMethodInfo.IsAbstract)
            {
                propertyAttributes |= SymbolAttributes.Abstract;
            }

            if (propertyData.IsStatic)
            {
                propertyAttributes |= SymbolAttributes.Static;
            }

            if (accessorMethodInfo.IsVirtual)
            {
                propertyAttributes |= SymbolAttributes.Virtual;
            }

            if (propertyData.IsOverride)
            {
                propertyAttributes |= SymbolAttributes.Override;
            }

            return propertyAttributes;
        }

        /// <summary>
        /// Determines the set of symbol attributes for the specified field based on its metadata and characteristics.
        /// </summary>
        /// <remarks>The returned attributes reflect the field's characteristics, including whether it is
        /// static, constant, read-only, or by-reference. This method is intended for internal use when mapping field
        /// metadata to symbol attributes.</remarks>
        /// <param name="fieldData">The field metadata used to evaluate and construct the corresponding symbol attributes.</param>
        /// <returns>A bitwise combination of <see cref="SymbolAttributes"/> values that represent the attributes of the field,
        /// such as static, constant, or by-reference.</returns>
        internal static SymbolAttributes GetAttributesInternal(FieldData fieldData)
        {
            FieldInfo fieldInfo = fieldData.GetFieldInfo();
            SymbolAttributes fieldAttributes = SymbolAttributes.Field;
            if (fieldInfo.IsInitOnly)
            {
                fieldAttributes |= SymbolAttributes.Final;
            }

            if (fieldData.IsRef)
            {
                fieldAttributes |= SymbolAttributes.ByReference;
            }

            if (fieldData.IsStatic)
            {
                fieldAttributes |= SymbolAttributes.Static;
            }

            if (IsConstInternal(fieldData))
            {
                fieldAttributes |= SymbolAttributes.Constant;
            }

            return fieldAttributes;
        }

        /// <summary>
        /// Determines the set of symbol attributes for a parameter based on its metadata.
        /// </summary>
        /// <param name="parameterData">The metadata describing the parameter, including its direction and optionality.</param>
        /// <returns>A bitwise combination of SymbolAttributes flags that represent the parameter's characteristics, such as In,
        /// Out, Ref, and Optional.</returns>
        internal static SymbolAttributes GetAttributesInternal(ParameterData parameterData)
        {
            SymbolAttributes parameterAttributes = SymbolAttributes.Parameter;
            if (parameterData.IsIn)
            {
                parameterAttributes |= SymbolAttributes.InParameter;
            }

            if (parameterData.IsRef)
            {
                parameterAttributes |= SymbolAttributes.RefParameter;
            }

            if (parameterData.IsOut)
            {
                parameterAttributes |= SymbolAttributes.OutParameter;
            }

            if (parameterData.IsOptional)
            {
                parameterAttributes |= SymbolAttributes.OptionalParameter;
            }

            return parameterAttributes;
        }

        /// <summary>
        /// Determines the set of symbol attributes for the specified method based on its metadata and characteristics.
        /// </summary>
        /// <param name="methodData">The method metadata used to evaluate and determine the applicable symbol attributes.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that describe the method's characteristics, such as whether
        /// it is static, abstract, virtual, final, override, or generic.</returns>
        internal static SymbolAttributes GetAttributesInternal(MethodData methodData)
        {
            MethodInfo methodInfo = methodData.GetMethodInfo();
            SymbolAttributes methodAttributes = SymbolAttributes.Method;
            if (methodInfo.IsFinal)
            {
                methodAttributes |= SymbolAttributes.Final;
            }

            if (methodInfo.IsAbstract)
            {
                methodAttributes |= SymbolAttributes.Abstract;
            }

            if (methodData.IsStatic)
            {
                methodAttributes |= SymbolAttributes.Static;
            }

            if (methodInfo.IsVirtual)
            {
                methodAttributes |= SymbolAttributes.Virtual;
            }

            if (methodData.IsOverride)
            {
                methodAttributes |= SymbolAttributes.Override;
            }

            if (methodData.IsGenericMethod)
            {
                methodAttributes |= SymbolAttributes.Generic;
            }

            return methodAttributes;
        }

        /// <summary>
        /// Determines the symbol attributes for a constructor based on the specified constructor data.
        /// </summary>
        /// <param name="constructorData">The data describing the constructor, including whether it is static.</param>
        /// <returns>A combination of symbol attributes representing the constructor's characteristics. Includes the static
        /// attribute if the constructor is static.</returns>
        internal static SymbolAttributes GetAttributesInternal(ConstructorData constructorData)
        {
            SymbolAttributes constructorAttributes = SymbolAttributes.Constructor;

            if (constructorData.IsStatic)
            {
                constructorAttributes |= SymbolAttributes.Static;
            }

            return constructorAttributes;
        }

        /// <summary>
        /// Determines the set of symbol attributes for the specified event based on its add method characteristics.
        /// </summary>
        /// <param name="eventData">The event metadata used to evaluate and derive the corresponding symbol attributes.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that represent the attributes of the event, such as Final,
        /// Abstract, Static, Virtual, or Override.</returns>
        internal static SymbolAttributes GetAttributesInternal(EventData eventData)
        {
            MethodData eventAddMethodData = eventData.AddMethodData;
            SymbolAttributes eventAttributes = SymbolAttributes.Event;
            MethodInfo addHandlerMethod = eventAddMethodData.GetMethodInfo();
            if (addHandlerMethod.IsFinal)
            {
                eventAttributes |= SymbolAttributes.Final;
            }

            if (addHandlerMethod.IsAbstract)
            {
                eventAttributes |= SymbolAttributes.Abstract;
            }

            if (eventAddMethodData.IsStatic)
            {
                eventAttributes |= SymbolAttributes.Static;
            }

            if (addHandlerMethod.IsVirtual)
            {
                eventAttributes |= SymbolAttributes.Virtual;
            }

            if (eventAddMethodData.IsOverride)
            {
                eventAttributes |= SymbolAttributes.Override;
            }

            return eventAttributes;
        }

        public static dynamic Cast(this object obj, Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(obj, nameof(obj));
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            return typeof(HelperExtensionsCommon).GetMethod(nameof(HelperExtensionsCommon.Cast), BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(object) }, null).GetGenericMethodDefinition().MakeGenericMethod(type).Invoke(obj, null);
        }

        private static T Cast<T>(this object obj) => (T)obj;

        /// <summary>
        /// Converts the specified string to an HTML-encoded representation suitable for display in web pages.
        /// </summary>
        /// <remarks>This method replaces special characters such as ampersands, angle brackets, quotes,
        /// apostrophes, spaces, and newlines with their corresponding HTML entities. Use this method to prevent HTML
        /// injection when rendering user-supplied text in HTML content.</remarks>
        /// <param name="text">The input string to encode. Can be null or empty.</param>
        /// <returns>A string containing the HTML-encoded representation of the input. If the input is null, returns null.</returns>
        internal static string ToHtmlEncodedString(this string text)
          => text.Replace("&", "&amp;", StringComparison.OrdinalIgnoreCase)
          .Replace("<", "&lt;", StringComparison.OrdinalIgnoreCase)
          .Replace(">", "&gt;", StringComparison.OrdinalIgnoreCase)
          .Replace("\"", "&quot;", StringComparison.OrdinalIgnoreCase)
          .Replace("'", "&apos;", StringComparison.OrdinalIgnoreCase)
          .Replace(System.Environment.NewLine, "<br>", StringComparison.OrdinalIgnoreCase)
          .Replace(" ", "&nbsp;", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns a read-only span containing the HTML-encoded representation of the specified string.
        /// </summary>
        /// <param name="text">The input string to encode as HTML. Can be null or empty.</param>
        /// <returns>A read-only span of characters containing the HTML-encoded form of the input string. If the input is null or
        /// empty, the returned span will be empty.</returns>
        internal static ReadOnlySpan<char> ToHtmlEncodedReadOnlySpan(this string text)
          => text.ToHtmlEncodedString()
          .AsSpan();

        /// <summary>
        /// Converts the specified character to its corresponding HTML-encoded string representation.
        /// </summary>
        /// <remarks>Use this method to safely represent individual characters in HTML output, ensuring
        /// that special characters are properly encoded to prevent HTML parsing issues or security vulnerabilities such
        /// as cross-site scripting (XSS).</remarks>
        /// <param name="character">The character to encode as an HTML entity or special HTML string.</param>
        /// <returns>A string containing the HTML-encoded representation of the character. Returns a named HTML entity for
        /// special characters such as '&', '<', '>', '"', '\'', and space; returns "<br>" for a line feed character
        /// ('\n'); returns an empty string for a carriage return character ('\r'); otherwise, returns the character as
        /// a string.</returns>
        internal static string ToHtmlEncodedString(this char character)
        {
            if (character == '&')
            {
                return "&amp;";
            }
            else if (character == '<')
            {
                return "&lt;";
            }
            else if (character == '>')
            {
                return "&gt;";
            }
            else if (character == ' ')
            {
                return "&nbsp;";
            }
            else if (character == '"')
            {
                return "&quot;";
            }
            else if (character == '\'')
            {
                return "&apos;";
            }
            else if (character == System.Environment.NewLine[1]) // \n
            {
                return "<br>";
            }
            else if (character == System.Environment.NewLine[0]) // \r
            {
                return string.Empty;
            }
            else
            {
                return character.ToString();
            }
        }

        /// <summary>
        /// Returns a read-only span containing the HTML-encoded representation of the specified character.
        /// </summary>
        /// <param name="character">The character to encode as HTML.</param>
        /// <returns>A read-only span of characters containing the HTML-encoded form of the input character. If the character
        /// does not require encoding, the span contains the original character.</returns>
        internal static ReadOnlySpan<char> ToHtmlEncodedReadOnlySpan(this char character)
          => character.ToHtmlEncodedString()
          .AsSpan();

        /// <summary>
        /// Inserts HTML <wbr> elements into the specified text to indicate potential line break opportunities based on
        /// the given wrap style and delimiters.
        /// </summary>
        /// <remarks>This method is intended for generating HTML output that allows browsers to break long
        /// words or identifiers at appropriate locations. The inserted <wbr> elements are safe for use in HTML and do
        /// not affect the visible content.</remarks>
        /// <param name="text">The input string to process for line break opportunities.</param>
        /// <param name="wrapStyle">The strategy used to determine where to insert <wbr> elements, such as wrapping at casing changes or at
        /// specified delimiters.</param>
        /// <param name="delimiters">A set of characters at which to consider inserting <wbr> elements. If empty, only the wrap style is used.</param>
        /// <returns>A string containing the original text with <wbr> elements inserted at positions determined by the wrap style
        /// and delimiters.</returns>
        internal static string ToWrappingHtml(this string text, WrapStyle wrapStyle, params char[] delimiters)
        {
            bool isWrappingAtCasing = wrapStyle is WrapStyle.Casing;
            var delimiterSet = new HashSet<char>(delimiters);
            PooledStringBuilder resultBuilder = StringBuilderFactory.GetOrCreateWith(text);
            for (int characterIndex = text.Length - 1; characterIndex >= 0; characterIndex--)
            {
                bool hasNextCharacter = resultBuilder.Length > characterIndex + 1;
                char textCharacter = resultBuilder[characterIndex];
                char nextTextCharacter = hasNextCharacter
                  ? resultBuilder[characterIndex + 1]
                  : default;
                if ((delimiterSet.Contains(textCharacter)
                  && ((textCharacter == '&' && hasNextCharacter && nextTextCharacter != 'n') || !hasNextCharacter))
                  || (isWrappingAtCasing && char.IsUpper(textCharacter)))
                {
                    _ = resultBuilder.Insert(characterIndex, "<wbr>");
                }
            }

            string result = resultBuilder.ToString();
            resultBuilder.Recycle();

            return result;
        }

        /// <summary>
        /// Appends an HTML-formatted representation of the specified symbol component, including attributes, modifiers,
        /// type, name, parameters, and constraints, to the provided string builder.
        /// </summary>
        /// <remarks>The generated HTML includes semantic CSS classes for syntax highlighting and is
        /// intended for use in documentation or code display scenarios. The method does not encode user-provided
        /// values; callers should ensure that all symbol component data is safe for HTML output.</remarks>
        /// <param name="signatureBuilder">The string builder to which the HTML-formatted symbol signature will be appended.</param>
        /// <param name="symbolComponentInfo">The symbol component information describing the structure and metadata to render as inline HTML.</param>
        /// <returns>The same <see cref="PooledStringBuilder"/> instance with the appended HTML-formatted symbol signature.</returns>
        internal static PooledStringBuilder AppendInlineHtml(this PooledStringBuilder signatureBuilder, SymbolComponentInfo symbolComponentInfo)
        {
            if (symbolComponentInfo.CustomAttributes.Any())
            {
                foreach (SymbolComponentInfo attribute in symbolComponentInfo.CustomAttributes)
                {
                    _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">[</span>")
                      .Append($"<span class=\"syntax-type\">")
                      .Append(attribute.Name);

                    bool hasConstructorArgs = symbolComponentInfo.CustomAttributeConstructorArgs.Any();
                    bool hasNamedArgs = symbolComponentInfo.CustomAttributeNamedArgs.Any();
                    bool hasArguments = hasConstructorArgs || hasNamedArgs;
                    if (hasArguments)
                    {
                        _ = signatureBuilder.Append('(');
                    }

                    if (hasConstructorArgs)
                    {
                        _ = signatureBuilder.AppendJoin(", ", symbolComponentInfo.CustomAttributeConstructorArgs);
                    }

                    if (hasNamedArgs)
                    {
                        _ = signatureBuilder.Append(' ');
                        foreach ((string PropertyName, string PropertyValue) in symbolComponentInfo.CustomAttributeNamedArgs)
                        {
                            _ = signatureBuilder.Append(PropertyName)
                              .Append(" = ")
                              .Append(PropertyValue)
                              .Append(", ");
                        }

                        _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2);
                    }

                    if (hasArguments)
                    {
                        _ = signatureBuilder.Append($"<span class=\"syntax-type\">")
                          .Append(')');
                    }

                    _ = signatureBuilder.Append("</span>")
                      .Append($"<span class=\"syntax-delimiter\">]</span>");

                    if (symbolComponentInfo.HasInlineAttributes)
                    {
                        _ = signatureBuilder.Append(' ');
                    }
                    else
                    {
                        _ = signatureBuilder.Append("<br>");
                    }
                }
            }

            if (!symbolComponentInfo.IsParameter && symbolComponentInfo.Modifiers.Any())
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">");
                foreach (string modifier in symbolComponentInfo.Modifiers)
                {
                    _ = signatureBuilder.Append(modifier)
                      .Append(' ');
                }

                _ = signatureBuilder.Append($"</span>");
            }

            if (symbolComponentInfo.ReturnType != null)
            {
                _ = signatureBuilder.AppendInlineHtml(symbolComponentInfo.ReturnType)
                    .Append(' ');
            }

            if (symbolComponentInfo.Name.Length > 0)
            {
                if (symbolComponentInfo.IsKeyword)
                {
                    _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">");
                }
                else if (symbolComponentInfo.IsSymbol && !symbolComponentInfo.IsParameter)
                {
                    _ = signatureBuilder.Append($"<span class=\"syntax-symbol\">");
                }
                else
                {
                    _ = signatureBuilder.Append($"<span class=\"syntax-type\">");
                }

                _ = signatureBuilder.Append(symbolComponentInfo.Name)
                  .Append("</span>");
            }

            if (symbolComponentInfo.ValueName.Length > 0)
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-value\">")
                  .Append(' ')
                  .Append(symbolComponentInfo.ValueName)
                  .Append("</span>");
            }

            if (symbolComponentInfo.IsIndexer)
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">")
                  .Append("this").
                  Append("</span>");
            }

            if (symbolComponentInfo.GenericTypeParameters.Any())
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">")
                  .Append('<'.ToHtmlEncodedReadOnlySpan())
                  .Append("</span>")
                  .Append($"<span class=\"syntax-type\">");

                foreach (SymbolComponentInfo typeParameter in symbolComponentInfo.GenericTypeParameters)
                {
                    _ = signatureBuilder.AppendInlineHtml(typeParameter)
                      .Append(',')
                      .Append(' ');
                }

                _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2)
                  .Append("</span>")
                  .Append($"<span class=\"syntax-delimiter\">")
                  .Append('>'.ToHtmlEncodedReadOnlySpan())
                  .Append("</span>");
            }

            if (symbolComponentInfo.Parameters.Any())
            {
                _ = signatureBuilder.Append($"<span class=\"syntax-delimiter\">");
                if (symbolComponentInfo.IsIndexer)
                {
                    _ = signatureBuilder.Append('['.ToHtmlEncodedReadOnlySpan());
                }
                else
                {
                    _ = signatureBuilder.Append('('.ToHtmlEncodedReadOnlySpan());
                }

                _ = signatureBuilder.Append("</span>")
                  .Append($"<span class=\"syntax-type\">");

                foreach (SymbolComponentInfo parameter in symbolComponentInfo.Parameters)
                {
                    if (parameter.IsExtensionMethodParameter)
                    {
                        _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">")
                          .Append("this")
                          .Append("</span>")
                          .Append(' ');
                    }

                    _ = signatureBuilder.AppendInlineHtml(parameter)
                      .Append(',')
                      .Append(' ');
                }

                _ = signatureBuilder.Remove(signatureBuilder.Length - 2, 2)
                  .Append($"<span class=\"syntax-delimiter\">");

                if (symbolComponentInfo.IsIndexer)
                {
                    _ = signatureBuilder.Append(']'.ToHtmlEncodedReadOnlySpan())
                    .Append("</span>");
                }
                else
                {
                    _ = signatureBuilder.Append(')'.ToHtmlEncodedReadOnlySpan())
                    .Append("</span>");
                }
            }

            if (symbolComponentInfo.PropertyGet != null)
            {
                _ = signatureBuilder.Append(' ')
                  .Append('{')
                  .Append(' ')
                  .AppendInlineHtml(symbolComponentInfo.PropertyGet)
                  .Append(';')
                  .Append(' ')
                  .Append('}');
            }

            if (symbolComponentInfo.PropertySet != null)
            {
                _ = signatureBuilder.Append(' ')
                  .Append('{')
                  .Append(' ')
                  .AppendInlineHtml(symbolComponentInfo.PropertySet)
                  .Append(';')
                  .Append(' ')
                  .Append('}');
            }

            if (symbolComponentInfo.GenericTypeConstraints.Any())
            {
                foreach (SymbolComponentInfo constraintInfo in symbolComponentInfo.GenericTypeConstraints)
                {
                    _ = signatureBuilder.Append(System.Environment.NewLine.ToHtmlEncodedReadOnlySpan())
                      .Append(symbolComponentInfo.IndentationString.ToHtmlEncodedReadOnlySpan())
                      .Append($"<span class=\"syntax-keyword\">")
                      .Append("where")
                      .Append(' ')
                      .Append("</span>")
                      .Append($"<span class=\"syntax-type\">")
                      .Append(constraintInfo.Name)
                      .Append(' ')
                      .Append("</span>")
                      .Append($"<span class=\"syntax-delimiter\">")
                      .Append(':')
                      .Append(' ')
                      .Append("</span>");

                    foreach (SymbolComponentInfo constraint in constraintInfo.GenericTypeConstraints)
                    {
                        if (constraint.IsKeyword)
                        {
                            _ = signatureBuilder.Append($"<span class=\"syntax-keyword\">")
                              .Append(constraint.Name)
                              .Append(',')
                              .Append(' ')
                              .Append("</span>");
                        }
                        else
                        {
                            _ = signatureBuilder.Append($"<span class=\"syntax-type\">")
                              .Append(constraint.Name)
                              .Append(',')
                              .Append(' ')
                              .Append("</span>");
                        }

                        _ = signatureBuilder.Remove(signatureBuilder.Length - ", </span>".Length, ", </span>".Length);
                    }
                }
            }

            if (symbolComponentInfo.HasExpressionTerminator)
            {
                _ = signatureBuilder
                    .Append($"<span class=\"syntax-delimiter\">")
                    .Append(';')
                    .Append("</span>");
            }

            return signatureBuilder;
        }

        /// <summary>
        /// Determines whether the specified delegate is compatible with the signature of the given event.
        /// </summary>
        /// <remarks>This method checks whether the delegate can be used as an event handler for the
        /// specified event by comparing the parameter types of the delegate's method and the event's handler type.
        /// Parameter types must match in number and be assignable according to .NET type compatibility rules.</remarks>
        /// <param name="clientHandler">The delegate to test for compatibility with the event's handler signature.</param>
        /// <param name="eventInfo">The event whose handler signature is used for compatibility comparison. Cannot be null.</param>
        /// <returns>true if the delegate's method parameters are assignable to the event handler's parameters; otherwise, false.</returns>
        public static bool IsAssignable(this Delegate clientHandler, EventInfo eventInfo)
        {
            MethodInfo eventDelegateInvokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
            ParameterInfo[] eventDelegateParameters = eventDelegateInvokeMethod.GetParameters();

            MethodInfo eventHandlerMethod = clientHandler.Method;
            ParameterInfo[] clientHandlerParameters = eventHandlerMethod.GetParameters();

            /* Validate the event EventHandler */

            if (eventDelegateParameters.Length != clientHandlerParameters.Length)
            {
                return false;
            }

            for (int parameterIndex = 0; parameterIndex < eventDelegateParameters.Length; parameterIndex++)
            {
                Type eventDelegateParameterType = eventDelegateParameters[parameterIndex].ParameterType;
                Type eventHandlerParameterType = clientHandlerParameters[parameterIndex].ParameterType;
                if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
