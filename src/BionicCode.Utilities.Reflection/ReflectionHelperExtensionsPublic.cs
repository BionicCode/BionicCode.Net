namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using BionicCode.Utilities.Net;

/// <summary>
/// A collection of extension methods for various default constraintTypes
/// </summary>
public static partial class ReflectionHelperExtensions
{
    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace and the declaring targetType (in case of a member), but with attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="MethodInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureShortName(MethodInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(MethodInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureShortName(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.ShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass&lt;TParam&gt;.MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="MethodInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(MethodInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(MethodInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureName(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated including the namespace, the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyNamespace.MyClass&lt;TParam&gt;.MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="MethodInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(MethodInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(MethodInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    public static string ToFullyQualifiedSignatureName(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass(Action&lt;int&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="MethodInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureShortName(MethodInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(MethodInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureShortName(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.RuntimeShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="methodInfo">The <see cref="MethodInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass&lt;int&gt;.MyClass(Action&lt;int&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="MethodInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureName(MethodInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(MethodInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureName(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.RuntimeSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace and the declaring targetType (in case of a member), but with attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Stop using this class")] public class MyClass&lt;T&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="Type"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureShortName(Type)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(TypeInfo)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
    public static string ToSignatureShortName(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.ShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Stop using this class")] public class MyClass&lt;T&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="Type"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(Type)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(Type)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
    public static string ToSignatureName(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated including the namespace, the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Stop using this class")] public class MyNamespace.MyClass&lt;T&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="Type"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(Type)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(Type)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
    public static string ToFullyQualifiedSignatureName(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Stop using this class")] public class MyClass&lt;string&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="Type"/> instance.<br/>
    /// Use <see cref="ToSignatureShortName(Type)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(Type)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureShortName(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.RuntimeShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Stop using this class")] public class MyClass&lt;string&gt;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="Type"/> instance.<br/>
    /// Use <see cref="ToSignatureName(Type)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(Type,)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="type"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureName(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.RuntimeSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace and the declaring targetType (in case of a member), but with attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[JsonPropertyName("my_property")] public T _myField;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="FieldInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureShortName(FieldInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(FieldInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureShortName(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return fieldData.ShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[JsonPropertyName("my_property")] public T MyClass&lt;T&gt;._myField;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="FieldInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(FieldInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(FieldInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureName(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return fieldData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated including the namespace, declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[JsonPropertyName("my_property")] public T MyNamespace.MyClass&lt;T&gt;._myField;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="FieldInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(FieldInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(FieldInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
    public static string ToFullyQualifiedSignatureName(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return fieldData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[JsonPropertyName("my_property")] public string _myField;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="FieldInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureShortName(FieldInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(FieldInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureShortName(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return fieldData.RuntimeShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="fieldInfo">The <see cref="FieldInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[JsonPropertyName("my_property")] public string MyClass&lt;string&gt;._myField;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="FieldInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureName(FieldInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(FieldInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="fieldInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureName(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return fieldData.RuntimeSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace and the declaring targetType (in case of a member), but with attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public T MyProperty { get; set; }"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureShortName(PropertyInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(PropertyInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureShortName(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return propertyData.ShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public T MyClass&lt;T&gt;.MyProperty { get; set; }"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(PropertyInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(PropertyInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureName(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return propertyData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated including the namespace, declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public T MyNamespace.MyClass&lt;T&gt;.MyProperty { get; set; }"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(PropertyInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(PropertyInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
    public static string ToFullyQualifiedSignatureName(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return propertyData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public int MyProperty { get; set; }"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureShortName(PropertyInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(PropertyInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureShortName(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return propertyData.RuntimeShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Required(ErrorMessage = "MyProperty is required.")] public int MyClass&lt;int&gt;.MyProperty { get; set; }"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="PropertyInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureName(PropertyInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(PropertyInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureName(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return propertyData.RuntimeSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace and the declaring targetType (in case of a member), but with attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureShortName(ConstructorInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(ConstructorInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureShortName(this ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
        return constructorData.ShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass&lt;TParam&gt;.MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(ConstructorInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(ConstructorInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureName(this ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
        return constructorData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated including the namespace, declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyNamespace.MyClass&lt;TParam&gt;.MyClass(Action&lt;TParam&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(ConstructorInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(ConstructorInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
    public static string ToFullyQualifiedSignatureName(this ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
        return constructorData.FullyQualifiedSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass(Action&lt;int&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureShortName(ConstructorInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(ConstructorInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureShortName(this ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
        return constructorData.RuntimeShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="constructorInfo">The <see cref="ConstructorInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"public MyClass&lt;int&gt;.MyClass(Action&lt;int&gt; doSomething, [CallerMemberName] string value = null)"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="ConstructorInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureName(ConstructorInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(ConstructorInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureName(this ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
        return constructorData.RuntimeSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace and the declaring targetType (in case of a member), but with attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;TEventArgs&gt; Completed;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureShortName(EventInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureShortName(this EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return eventData.ShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;TEventArgs&gt; MyClass&lt;TEventArgs&gt;.Completed;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(EventInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
    public static string ToSignatureName(this EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return eventData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated including namespace, declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;TEventArgs&gt; MyNamespace.MyClass&lt;TEventArgs&gt;.Completed;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
    /// Use <see cref="ToRuntimeSignatureName(EventInfo)"/> to return a signature using the resolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
    public static string ToFullyQualifiedSignatureName(this EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return eventData.Signature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;CompletedEventArgs&gt; Completed;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureShortName(EventInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureShortName(this EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return eventData.RuntimeShortSignature;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic symbols to a readable signature.
    /// <br/>The Signature will be generated without namespace, but with the declaring targetType (in case of a member), attributes and the resolved runtime generic targetType argument names.
    /// </summary>
    /// <param name="eventInfo">The <see cref="EventInfo"/> object to generate the symbol signature for.</param>
    /// <returns>
    /// A readable signature of the symbol, that includes the targetType, name and parameters and also resolves generic targetType parameters. 
    /// <br/>For example, <c>"[Obsolete("Use NewEvent instead.")] public event EventHandler&lt;CompletedEventArgs&gt; MyClass&lt;CompletedEventArgs&gt;.Completed;"</c>.
    /// </returns>
    /// <remarks>
    /// The method uses caching to improve performance for repeated calls with the same <see cref="EventInfo"/> instance.<br/>
    /// Use <see cref="ToSignatureName(EventInfo)"/> to return a signature using the unresolved generic targetType parameters instead.
    /// Or use <see cref="ToDisplayName(EventInfo, bool)"/> to return the plain symbol name.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The parameter <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
    public static string ToRuntimeSignatureName(this EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return eventData.RuntimeSignature;
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData entry = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(method, nameof(method));

        MethodData entry = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(method);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructor, nameof(constructor));

        ConstructorData entry = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructor);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(property, nameof(property));

        PropertyData entry = SymbolReflectionInfoCache.GetOrCreateEntryInternal(property);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData entry = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(field, nameof(field));

        FieldData entry = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(field);
        return entry.AccessModifier;
    }

    /// <summary>
    /// Gets the ordered base targetType hierarchy (ancestor inheritance tree) of a specified targetType, starting from the root targetType (most distant base class or most distant implemented interface) and includes the current targetType (the tree's leaf) as the last item.
    /// </summary>
    /// <param name="type">The targetType of which the ancestor hierarchy to return.</param>
    /// <param name="includeInterfaces"><see langword="true"/> if interfaces should be included. Otherwise <see langword="false"/>. 
    /// If targetType <paramref name="type"/> is itself an interface then the <paramref name="includeInterfaces"/> parameter is ignored and all implemented interfaces will be returned.
    /// The default is <see langword="false"/>.</param>
    /// <returns>A <see cref="List{Type}"/> that contains the ordered base types of <paramref name="type"/> (ancestor hierarchy) starting with the root (the most distant base class or most distant interface in case <paramref name="includeInterfaces"/> evaluates to <see langword="true"/>). 
    /// If <paramref name="includeInterfaces"/> is <see langword="true"/> then the result also contains all implemented interfaces. 
    /// <br/>The last item in the collection is always the current <paramref name="type"/> value (the hierarchy leaf). 
    /// <br/>The language base types <see cref="object" /> and <see cref="ValueType"/> are excluded from the result, except the current <paramref name="type"/> is itself of targetType <see cref="object"/>.
    /// <br/>If <paramref name="type"/> does not have a parent inheritance tree or does not implement any interfaces or is of targetType <see cref="object"/> or a value targetType (<see cref="Type.IsValueType"/> returns <see langword="true"/>) then the result collection will only contain the current <paramref name="type"/> value.</returns>
    /// <remarks>The targetType <see cref="object"/> (the root targetType for reference types) and the targetType <see cref="ValueType"/> (the base targetType for value types) are not included in the hierarchy.
    /// <br/>This means, if <paramref name="type"/> is of targetType <see cref="object"/> or a value targetType (<see cref="Type.IsValueType"/> returns <see langword="true"/>) then the result will only contain the current <paramref name="type"/> or in case of a value targetType additionally the implemented interfaces.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static ImmutableList<Type> GetTypeHierarchy(this Type type, bool includeInterfaces = false)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

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
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo, nameof(parameterInfo));

        ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateCacheEntry(parameterInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return isDeclaringTypeIncluded
            ? eventData.DisplayName
            : eventData.ShortDisplayName;
    }

    /// <summary>
    /// Extension method to convert generic and non-generic targetType name to a readable fully qualified display name.
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo, nameof(eventInfo));

        EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);
        return eventData.FullyQualifiedDisplayName;
    }

    // TODO::Test if checking get() is enough to determine if a property is overridden
    /// <summary>
    /// Determines whether the specified targetType represents a delegate targetType.
    /// </summary>
    /// <param name="type">The targetType to evaluate. Cannot be null.</param>
    /// <returns>true if the specified targetType is a delegate; otherwise, false.</returns>
    public static bool IsDelegate(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.IsDelegate;
    }

    // TODO::Test if checking get() is enough to determine if a property is overridden
    public static bool IsOverride(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData memberInfoData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return memberInfoData.IsOverride;
    }

    internal static bool IsOverrideInternal(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        return propertyInfo.CanRead ? propertyInfo.GetGetMethod(true).IsOverride() : propertyInfo.GetSetMethod().IsOverride();
    }

    public static bool IsConst(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));

        FieldData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return methodData.SymbolAttributes.HasFlag(SymbolAttributes.Constant);
    }

    public static bool IsOverride(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.IsOverride;
    }

    public static bool IsInitOnly(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo, nameof(propertyInfo));

        PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        return propertyData.SymbolAttributes.HasFlag(SymbolAttributes.InitProperty);
    }

    public static bool IsPropertyIndexer(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo);

        return propertyInfo.GetIndexParameters().Length != 0;
    }

    /// <summary>
    /// Checks whether the provided <see cref="MethodBase"/> represents the setter accessor of an indexer property.
    /// </summary>
    /// <remarks>If <paramref name="isValidationEnabled"/> is <see langword="true"/>, additional validation will be performed.
    /// This requires full enumeration of all properties of the declaring type to explicitly match the potential accessor method <paramref name="methodInfo"/> against properties of the declaring type.
    /// This is usually not required and can be skipped in performance sensitive contexts.<para/>
    /// If <paramref name="isValidationEnabled"/> is <see langword="false"/> then the indexer is
    /// identified by default runtime name prefixes "get_" and "set_" and parameter count
    /// to exclude normal properties. Avoiding matching against "set_Item" and "get_Item" allows to include indexers
    /// which have been renamed using the <see cref="IndexerNameAttribute"/>.</remarks>
    /// <param name="methodInfo">The method to check.</param>
    /// <param name="isValidationEnabled">If <see langword="true"/>, additional reflection heavy validation will be performed which is not required in common C# code. See remarks..<para/>
    /// If <see langword="false"/> then the indexer is identified by default runtime name prefixes "get_" and "set_" and parameter count to exclude non-indexer properties.<para/>
    /// The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the method associates with an indexer property's setter. Otherwise, <see langword="false"/>.</returns>
    public static bool IsIndexerPropertySetter(this MethodInfo methodInfo, bool isValidationEnabled = false)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsIndexerPropertySetMethod;
    }

    /// <summary>
    /// Checks whether the provided <see cref="MethodBase"/> represents the getter accessor of an indexer property.
    /// </summary>
    /// <remarks>If <paramref name="isValidationEnabled"/> is <see langword="true"/>, additional validation will be performed.
    /// This requires full enumeration of all properties of the declaring type to explicitly match the potential accessor method <paramref name="methodInfo"/> against properties of the declaring type.
    /// This is usually not required and can be skipped in performance sensitive contexts.<para/>
    /// If <paramref name="isValidationEnabled"/> is <see langword="false"/> then the indexer is
    /// identified by default runtime name prefixes "get_" and "set_" and parameter count
    /// to exclude normal properties. Avoiding matching against "set_Item" and "get_Item" allows to include indexers
    /// which have been renamed using the <see cref="IndexerNameAttribute"/>.</remarks>
    /// <param name="methodInfo">The method to check.</param>
    /// <param name="isValidationEnabled">If <see langword="true"/>, additional reflection heavy validation will be performed which is not required in common C# code. See remarks..<para/>
    /// If <see langword="false"/> then the indexer is identified by default runtime name prefixes "get_" and "set_" and parameter count to exclude non-indexer properties.<para/>
    /// The default is <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the method associates with an indexer property's setter. Otherwise, <see langword="false"/>.</returns>
    public static bool IsIndexerPropertyGetter(this MethodInfo methodInfo, bool isValidationEnabled = false)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsIndexerPropertyGetMethod;
    }

    /// <summary>
    /// Determines whether the specified method represents a non-indexer property setter.
    /// </summary>
    /// <remarks>This method checks for the special naming convention and signature used by property
    /// setters in .NET. Indexer setters are excluded.</remarks>
    /// <param name="methodInfo">The method to evaluate. Typically obtained from reflection on a type's members.</param>
    /// <returns>true if the method is a property setter for a non-indexer property; otherwise, false.</returns>
    public static bool IsPropertySetter(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsPropertySetMethod;
    }

    /// <summary>
    /// Determines whether the specified method represents a property getter.
    /// </summary>
    /// <remarks>This method returns true only for non-indexed property getters. It does not consider
    /// indexer getters or methods that are not special name property accessors.</remarks>
    /// <param name="methodInfo">The method to evaluate. Typically obtained from reflection on a type.</param>
    /// <returns>true if the method is a property getter; otherwise, false.</returns>
    public static bool IsPropertyGetter(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsPropertyGetMethod;
    }

    public static bool IsEventAccessor(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsEventAccessorMethod;
    }

    public static bool IsEventAddAccessor(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsEventAddMethod;
    }

    public static bool IsEventRemoveAccessor(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsEventRemoveMethod;
    }

    public static bool IsOperatorOverload(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsOperatorOverload;
    }

    public static bool IsDelegateMethod(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsDelegateMethod;
    }

    public static bool IsDelegateInvokeMethod(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsDelegateInvokeMethod;
    }

    public static bool IsDelegateBeginInvokeMethod(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));
        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsDelegateBeginInvokeMethod;
    }

    public static bool IsDelegateEndInvokeMethod(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo).IsDelegateEndInvokeMethod;
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
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.IsAwaitable;
    }

    /// <summary>
    /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
    /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
    /// <remarks>The method first checks if the return valueType is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned valueType (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate valueType (awaiter).
    /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned valueType that would make the valueType awaitable. If this fails too, the method is not awaitable.</remarks>
    public static bool IsAwaitable(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.IsAwaitable;
    }

    /// <summary>
    /// Determines whether the specified method is marked as asynchronous.
    /// </summary>
    /// <param name="methodInfo">The method to inspect for asynchronous designation. Cannot be null.</param>
    /// <returns>true if the method is marked as asynchronous; otherwise, false.</returns>
    public static bool IsMarkedAsync(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.IsAsync;
    }

    /// <summary>
    /// Extension method to check if a <see cref="Type"/> is static.
    /// </summary>
    /// <param genericTypeParameterIdentifier="targetType">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="targetType"/> is static. Otherwise <see langword="false"/>.</returns>
    public static bool IsStatic(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.IsStatic;
    }

    /// <summary>
    /// Determines whether the specified targetType is a built-in .NET targetType.
    /// </summary>
    /// <param name="type">The targetType to evaluate. Cannot be null.</param>
    /// <returns><see langword="true"/> if the specified targetType is a built-in .NET targetType; otherwise, <see langword="false"/>.</returns>
    public static bool IsBuiltInType(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.IsBuiltInType;
    }

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="ref"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsRef(this ParameterInfo parameterInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo, nameof(parameterInfo));

        ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateCacheEntry(parameterInfo);
        return parameterData.IsRef;
    }

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="ref"/> <see langword="readonly"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsRefReadonly(this ParameterInfo parameterInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo, nameof(parameterInfo));

        ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateCacheEntry(parameterInfo);
        return parameterData.IsRefReadOnly;
    }

    /// <summary>
    /// Determines whether the specified parameter is marked with the <see langword="params"/> modifier.
    /// </summary>
    /// <remarks>Use this method to check if a method parameter accepts a variable number of arguments
    /// using the <see langword="params"/> keyword in its declaration.</remarks>
    /// <param name="parameterInfo">The parameter to inspect for the <see langword="params"/> modifier. Cannot be null.</param>
    /// <returns>true if the parameter is a parameter array (marked with the <see langword="params"/> modifier); otherwise,
    /// false.</returns>
    public static bool IsParams(this ParameterInfo parameterInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo, nameof(parameterInfo));

        ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateCacheEntry(parameterInfo);
        return parameterData.IsParams;
    }

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="in"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsIn(this ParameterInfo parameterInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo, nameof(parameterInfo));

        ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateCacheEntry(parameterInfo);
        return parameterData.IsIn;
    }

    /// <summary>
    /// Extension method to check if a <see cref="ParameterInfo"/> represents a <see langword="out"/> parameter.
    /// </summary>
    /// <returns><see langword="true"/> if the <paramref name="parameterInfo"/> represents a <see langword="ref"/> parameter. Otherwise <see langword="false"/>.</returns>
    public static bool IsOut(this ParameterInfo parameterInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo, nameof(parameterInfo));

        ParameterData parameterData = SymbolReflectionInfoCache.GetOrCreateCacheEntry(parameterInfo);
        return parameterData.IsOut;
    }

    /// <summary>
    /// Extension method that checks if the provided <see cref="Type"/> is qualified to define extension methods.
    /// </summary>
    /// <param genericTypeParameterIdentifier="targetType">The extended <see cref="Type"/> instance.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="targetType"/> is allowed to define extension methods. Otherwise <see langword="false"/>.</returns>
    /// <remarks>To be able to define extension methods a class must be static, non-generic, a top level valueType. 
    /// <br/>In addition this method checks if the declaring class and the method are both decorated with the <see cref="ExtensionAttribute"/> which is added by the compiler.</remarks>
    public static bool CanDeclareExtensionMethods(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.CanDeclareExtensionMethod;
    }

    /// <summary>
    /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method. Otherwise <see langword="false"/>.</returns>
    public static bool IsExtensionMethod(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
        return methodData.IsExtensionMethod;
    }

    /// <summary>
    /// Determines whether the specified method is an extension method whose first parameter is compatible with the
    /// given instance targetType.
    /// </summary>
    /// <remarks>Use this method to verify whether a given <see cref="MethodInfo"/> represents an
    /// extension method that can be applied to instances of a specific targetType. This is useful when reflecting over
    /// methods to identify applicable extension methods for a targetType.</remarks>
    /// <typeparam name="TInstance">The targetType to check as the target of the extension method. Must be a targetType derived from <see cref="Type"/>.</typeparam>
    /// <param name="methodInfo">The method information to evaluate as a potential extension method.</param>
    /// <returns>true if the method is an extension method and its first parameter targetType is assignable from <typeparamref
    /// name="TInstance"/>; otherwise, false.</returns>
    public static bool IsExtensionMethodOf<TInstance>(this MethodInfo methodInfo) where TInstance : Type
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);

        /* Check if the method satisfies the constraints to act as an extension methods */
        if (!methodData.IsExtensionMethod)
        {
            return false;
        }

        ParameterInfo[] parameterInfos = methodInfo.GetParameters();
        if (parameterInfos.Length > 0)
        {
            ParameterInfo firstParameterInfo = parameterInfos[0];
            if (firstParameterInfo.ParameterType.IsAssignableFrom(typeof(TInstance)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Extension method to check if a <see cref="MethodInfo"/> is the info of an extension method for a particular valueType.
    /// </summary>
    /// <param genericTypeParameterIdentifier="methodInfo">The extended <see cref="MethodInfo"/> instance to validate.</param>
    /// <param genericTypeParameterIdentifier="typeToExtend">The <see cref="Type"/> the <paramref genericTypeParameterIdentifier="methodInfo"/> is expected to extend.</param>
    /// <returns><see langword="true"/> if the <paramref genericTypeParameterIdentifier="methodInfo"/> is an extension method for <paramref genericTypeParameterIdentifier="typeToExtend"/>. Otherwise <see langword="false"/>.</returns>
    public static bool IsExtensionMethodOf(this MethodInfo methodInfo, Type typeToExtend)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(typeToExtend, nameof(typeToExtend));
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo, nameof(methodInfo));

        MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);

        // Check if the declaring class satisfies the constraints to declare extension methods
        if (!methodInfo.DeclaringType.CanDeclareExtensionMethods())
        {
            return false;
        }

        /* Check if the method satisfies the constraints to act as an extension methods */
        if (!methodData.IsExtensionMethod)
        {
            return false;
        }

        ParameterInfo[] parameters = methodInfo.GetParameters();
        if (parameters.Length > 0)
        {
            ParameterInfo firstParameterInfo = parameters[0];
            if (firstParameterInfo.ParameterType.IsAssignableFrom(typeToExtend))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified targetType is a read-only struct.
    /// </summary>
    /// <param name="type">The targetType to evaluate. Cannot be null.</param>
    /// <returns>true if the specified targetType is a read-only struct; otherwise, false.</returns>
    public static bool IsReadOnlyStruct(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

        TypeData typeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        return typeData.SymbolAttributes.HasFlag(SymbolAttributes.ReadOnlyStruct);
    }

    public static bool IsReadOnly(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo, nameof(fieldInfo));
        FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        return fieldData.IsReadonly;
    }

    /// <summary>
    /// Converts the specified string to an HTML-encoded representation suitable for display in web pages.
    /// </summary>
    /// <remarks>This method replaces special characters such as ampersands, angle brackets, quotes,
    /// apostrophes, spaces, and newlines with their corresponding HTML entities. Use this method to prevent HTML
    /// injection when rendering user-supplied text in HTML content.</remarks>
    /// <param name="text">The input string to encode. Can be null or empty.</param>
    /// <returns>A string containing the HTML-encoded representation of the input. If the input is null, returns null.</returns>
    public static string ToHtmlEncodedString(this string text)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(text);

        return text.Replace("&", "&amp;", StringComparison.OrdinalIgnoreCase)
          .Replace("<", "&lt;", StringComparison.OrdinalIgnoreCase)
          .Replace(">", "&gt;", StringComparison.OrdinalIgnoreCase)
          .Replace("\"", "&quot;", StringComparison.OrdinalIgnoreCase)
          .Replace("'", "&apos;", StringComparison.OrdinalIgnoreCase)
          .Replace(System.Environment.NewLine, "<br>", StringComparison.OrdinalIgnoreCase)
          .Replace(" ", "&nbsp;", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns a read-only span containing the HTML-encoded representation of the specified string.
    /// </summary>
    /// <param name="text">The input string to encode as HTML. Can be null or empty.</param>
    /// <returns>A read-only span of characters containing the HTML-encoded form of the input string. If the input is null or
    /// empty, the returned span will be empty.</returns>
    public static ReadOnlySpan<char> ToHtmlEncodedReadOnlySpan(this string text)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(text);

        return text.ToHtmlEncodedString()
          .AsSpan();
    }

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
    public static ReadOnlySpan<char> ToHtmlEncodedReadOnlySpan(this char character)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(character);

        return character.ToHtmlEncodedString()
          .AsSpan();
    }

    /// <summary>
    /// Inserts HTML &lt;wbr&gt; elements into the specified text to indicate potential line break opportunities based on
    /// the given wrap style and delimiters.
    /// </summary>
    /// <remarks>This method is intended for generating HTML output that allows browsers to break long
    /// words or identifiers at appropriate locations. The inserted &lt;wbr&gt; elements are safe for use in HTML and do
    /// not affect the visible content.</remarks>
    /// <param name="text">The input string to process for line break opportunities.</param>
    /// <param name="wrapStyle">The strategy used to determine where to insert &lt;wbr&gt; elements, such as wrapping at casing changes or at
    /// specified delimiters.</param>
    /// <param name="delimiters">A set of characters at which to consider inserting &lt;wbr&gt; elements. If empty, only the wrap style is used.</param>
    /// <returns>A string containing the original text with &lt;wbr&gt; elements inserted at positions determined by the wrap style
    /// and delimiters.</returns>
    public static string ToWrappingHtml(this string text, WrapStyle wrapStyle, params char[] delimiters)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(text);

        bool isWrappingAtCasing = wrapStyle is WrapStyle.Casing;
        var delimiterSet = new HashSet<char>(delimiters);
        using var resultBuilder = PooledStringBuilder.GetOrCreate(text);
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

        return result;
    }
}
