namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Generic;
using System.Reflection;
using BionicCode.Utilities.Net.Reflection.Exceptions;

internal class SymbolInfoDataView : SymbolReflectionInfoCache.SymbolDataViewBase, ISymbolInfoDataView
{
    internal SymbolInfoDataView(SymbolReflectionInfoCacheKey cacheKey) : base(cacheKey)
    {
    }

    /// <summary>
    /// Returns the name of the assembly that declares the symbol. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetAssemblyName(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The name of the assembly that declares the symbol.</value>
    public string AssemblyName => GetSymbolInfoDataOrThrow().AssemblyName;

    /// <summary>
    /// Attempts to retrieve the name of the assembly that declares the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="assemblyName"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="AssemblyName"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="assemblyName">When this method returns successfully, contains the name of the assembly that declares the symbol.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetAssemblyName(out string? assemblyName)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            assemblyName = symbolInfoData!.AssemblyName;
            return true;
        }
        else
        {
            assemblyName = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the namespace of the symbol. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetNamespace(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The namespace of the symbol.</value>
    public string Namespace => GetSymbolInfoDataOrThrow().Namespace;

    /// <summary>
    /// Attempts to retrieve the namespace of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="Namespace"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the namespace of the symbol.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetNamespace(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.Namespace;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the display name of the symbol. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetDisplayName(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The display name of the symbol.
    /// <br/>For example, a method name: <c>"MyClass.DoSomething&lt;T&gt;"</c>.</value>
    public string DisplayName => GetSymbolInfoDataOrThrow().DisplayName;
    /// <summary>
    /// Attempts to retrieve the display name of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="DisplayName"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the display name of the symbol.
    /// <br/>For example, a method name: <c>"MyClass.DoSomething&lt;T&gt;"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetDisplayName(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.DisplayName;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the fully qualified display name of the symbol. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetFullyQualifiedDisplayName(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The fully qualified display name of the symbol.
    /// <br/>For example, a method name: <c>"MyNamespace.MyClass.DoSomething&lt;T&gt;"</c></value>
    public string FullyQualifiedDisplayName => GetSymbolInfoDataOrThrow().FullyQualifiedDisplayName;
    /// <summary>
    /// Attempts to retrieve the fully qualified display name of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="FullyQualifiedDisplayName"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the fully qualified display name of the symbol.
    /// <br/>For example, a method name: <c>"MyNamespace.MyClass.DoSomething&lt;T&gt;"</c></param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFullyQualifiedDisplayName(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.FullyQualifiedDisplayName;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the fully qualified runtime signature of the symbol. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetFullyQualifiedRuntimeSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The fully qualified runtime signature of the symbol.
    /// <br/>For example: <c>"internal void MyNamespace.MyClass.DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    public string FullyQualifiedRuntimeSignature => GetSymbolInfoDataOrThrow().FullyQualifiedRuntimeSignature;
    /// <summary>
    /// Attempts to retrieve the fully qualified runtime signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="FullyQualifiedRuntimeSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the fully qualified runtime signature of the symbol.
    /// <br/>For example: <c>"internal void MyNamespace.MyClass.DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFullyQualifiedRuntimeSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.FullyQualifiedRuntimeSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the fully qualified signature of the symbol. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetFullyQualifiedSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The fully qualified signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyNamespace.MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    public string FullyQualifiedSignature => GetSymbolInfoDataOrThrow().FullyQualifiedSignature;
    /// <summary>
    /// Attempts to retrieve the fully qualified signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="FullyQualifiedSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the fully qualified signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyNamespace.MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFullyQualifiedSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.FullyQualifiedSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the indentation string to use for formatting.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIndentationString(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The indentation string to use for formatting.</value>
    public string IndentationString => GetSymbolInfoDataOrThrow().IndentationString;
    /// <summary>
    /// Attempts to retrieve the indentation string to use for formatting.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IndentationString"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the indentation string to use for formatting.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIndentationString(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.IndentationString;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the name of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetName(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The name of the symbol.</value>
    public string Name => GetSymbolInfoDataOrThrow().Name;
    /// <summary>
    /// Attempts to retrieve the name of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="Name"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the name of the symbol.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetName(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.Name;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the runtime short compact signature of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetRuntimeShortCompactSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The runtime short compact signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;int&gt;(int firstValue, string value = null)"</c>.</value>
    public string RuntimeShortCompactSignature => GetSymbolInfoDataOrThrow().RuntimeShortCompactSignature;
    /// <summary>
    /// Attempts to retrieve the runtime short compact signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="RuntimeShortCompactSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the runtime short compact signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;int&gt;(int firstValue, string value = null)"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetRuntimeShortCompactSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.RuntimeShortCompactSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the runtime short signature of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetRuntimeShortSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The runtime short signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    public string RuntimeShortSignature => GetSymbolInfoDataOrThrow().RuntimeShortSignature;
    /// <summary>
    /// Attempts to retrieve the runtime short signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="RuntimeShortSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the runtime short signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetRuntimeShortSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.RuntimeShortSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the runtime signature of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetRuntimeSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The runtime signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyClass.DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    public string RuntimeSignature => GetSymbolInfoDataOrThrow().RuntimeSignature;
    /// <summary>
    /// Attempts to retrieve the runtime signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="RuntimeSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the runtime signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyClass.DoSomething&lt;int&gt;(int firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetRuntimeSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.RuntimeSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the short compact signature of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetShortCompactSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The short compact signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;T&gt;(T firstValue, string value = null)"</c>.</value>
    public string ShortCompactSignature => GetSymbolInfoDataOrThrow().ShortCompactSignature;
    /// <summary>
    /// Attempts to retrieve the short compact signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="ShortCompactSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the short compact signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;T&gt;(T firstValue, string value = null)"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetShortCompactSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.ShortCompactSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the short display name of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetShortDisplayName(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The short display name of the symbol.
    /// <br/>For example, a method name: <c>"DoSomething;T&gt;"</c>.</value>
    public string ShortDisplayName => GetSymbolInfoDataOrThrow().ShortDisplayName;
    /// <summary>
    /// Attempts to retrieve the short display name of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="ShortDisplayName"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the short display name of the symbol.
    /// <br/>For example, a method name: <c>"DoSomething;T&gt;"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetShortDisplayName(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.ShortDisplayName;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the short signature of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetShortSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The short signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    public string ShortSignature => GetSymbolInfoDataOrThrow().ShortSignature;
    /// <summary>
    /// Attempts to retrieve the short signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="ShortSignature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the short signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetShortSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.ShortSignature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the signature of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetSignature(out string?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</value>
    public string Signature => GetSymbolInfoDataOrThrow().Signature;
    /// <summary>
    /// Attempts to retrieve the signature of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="Signature"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the signature of the symbol.
    /// <br/>For example, a method signature: <c>"internal void MyClass.DoSomething&lt;T&gt;(T firstValue, [CallerMemberName] string value = null) where T : struct"</c>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSignature(out string? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.Signature;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the number of indentation white spaces.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetFormattingIndentation(out int)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The number of indentation white spaces.</value>
    public int FormattingIndentation
    {
        get => GetSymbolInfoDataOrThrow().FormattingIndentation;
        set => GetSymbolInfoDataOrThrow().FormattingIndentation = value;
    }
    /// <summary>
    /// Attempts to retrieve the number of indentation white spaces.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="FormattingIndentation"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the number of indentation white spaces.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetFormattingIndentation(out int value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.FormattingIndentation;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns a list of <see cref="CustomAttributeData"/>.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetCustomAttributeData(out IList<CustomAttributeData>)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The list of <see cref="CustomAttributeData"/>.</value>
    public IList<CustomAttributeData> AttributeData => GetSymbolInfoDataOrThrow().AttributeData;
    /// <summary>
    /// Attempts to retrieve the list of <see cref="CustomAttributeData"/>.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="AttributeData"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the list of <see cref="CustomAttributeData"/>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetCustomAttributeData(out IList<CustomAttributeData>? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.AttributeData;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns a <see cref="SymbolAttributes"/> that describes the attributes of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetSymbolAttributesSymbolAttributes(out SymbolAttributes>)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The <see cref="SymbolAttributes"/> which describes the attributes of the symbol.</value>
    public SymbolAttributes SymbolAttributes => GetSymbolInfoDataOrThrow().SymbolAttributes;
    /// <summary>
    /// Attempts to retrieve the <see cref="SymbolAttributes"/> that describes the attributes of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="SymbolAttributes"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the <see cref="SymbolAttributes"/> which describes the attributes of the symbol.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSymbolAttributes(out SymbolAttributes value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.SymbolAttributes;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns a <see cref="SymbolComponentInfo"/> that contains the signature components of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetSymbolComponentInfo(out SymbolComponentInfo)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The <see cref="SymbolComponentInfo"/> which contains the signature components of the symbol.</value>
    public SymbolComponentInfo SymbolComponentInfo => GetSymbolInfoDataOrThrow().SymbolComponentInfo;
    /// <summary>
    /// Attempts to retrieve the <see cref="SymbolComponentInfo"/> that contains the signature components of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="SymbolComponentInfo"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the <see cref="SymbolComponentInfo"/> which contains the signature components of the symbol.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSymbolComponentInfo(out SymbolComponentInfo? value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.SymbolComponentInfo;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Returns a <see cref="SymbolKind"/> that identifies the kind of the symbol.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetSymbolKind(out SymbolKind)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The <see cref="SymbolKind"/> which identifies the kind of the symbol.</value>
    public SymbolKind SymbolKind => GetSymbolInfoDataOrThrow().SymbolKind;
    /// <summary>
    /// Attempts to retrieve the <see cref="SymbolKind"/> that identifies the kind of the symbol.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="value"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="SymbolKind"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="value">When this method returns successfully, contains the <see cref="SymbolKind"/> which identifies the kind of the symbol.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSymbolKind(out SymbolKind value)
    {
        if (TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData))
        {
            value = symbolInfoData!.SymbolKind;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}