namespace BionicCode.Utilities.Net.Reflection;

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BionicCode.Utilities.Net;
using BionicCode.Utilities.Net.Reflection.Exceptions;

[SuppressMessage(
    "Design",
    "CA1065:Do not raise exceptions in unexpected locations",
    Justification = "PropertyDataView is a live cache-backed view. After ALC unload, the underlying symbol is unavailable and the only correct behavior is to throw.")]
internal class PropertyDataView : SymbolInfoDataView, IPropertyDataView
{
    internal PropertyDataView(SymbolReflectionInfoCacheKey cacheKey) : base(cacheKey)
    { }

    /// <summary>
    /// Returns whether the property has a get accessor i.e. can be read from.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetCanRead(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the property has a get accessor and can be read from; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool CanRead => GetPropertyDataOrThrow().CanRead;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the associated property has a get accessor i.e. can be read from.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="canRead"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="CanRead"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="canRead">When this method returns successfully, contains a value indicating whether the property can be read from.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetCanRead(out bool canRead)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            canRead = propertyData!.CanRead;
            return true;
        }
        else
        {
            canRead = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the property has a set accessor i.e. can be written to. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetCanWrite(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the property has a set accessor and can be written to; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool CanWrite => GetPropertyDataOrThrow().CanWrite;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the associated property can be written to.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="canWrite"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="CanWrite"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="canWrite">When this method returns successfully, contains a value indicating whether the property can be written to.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetCanWrite(out bool canWrite)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            canWrite = propertyData!.CanWrite;
            return true;
        }
        else
        {
            canWrite = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the access modifier of the property's getter. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetGetAccessorAccessModifier(out AccessModifier)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value>The <see cref="AccessModifier"/> of the property's getter if it exists; otherwise, returns a default value.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public AccessModifier GetAccessorAccessModifier => GetPropertyDataOrThrow().GetAccessorAccessModifier;

    /// <summary>
    /// Attempts to retrieve the <see cref="AccessModifier"/> of the property's getter.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="getMethodAccessModifier"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="GetAccessorAccessModifier"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="getMethodAccessModifier">When this method returns successfully, contains the <see cref="AccessModifier"/> of the property's getter.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetGetAccessorAccessModifier(out AccessModifier getMethodAccessModifier)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            getMethodAccessModifier = propertyData!.GetAccessorAccessModifier;
            return true;
        }
        else
        {
            getMethodAccessModifier = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the access modifier of the property's setter. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetSetAccessorAccessModifier(out AccessModifier)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value>The <see cref="AccessModifier"/> of the property's setter if it exists; otherwise, returns a default value.</value>
    public AccessModifier SetAccessorAccessModifier => GetPropertyDataOrThrow().SetAccessorAccessModifier;

    /// <summary>
    /// Attempts to retrieve the <see cref="AccessModifier"/> of the property's setter.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="getMethodAccessModifier"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="SetAccessorAccessModifier"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="getMethodAccessModifier">When this method returns successfully, contains the <see cref="AccessModifier"/> of the property's setter.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetSetAccessorAccessModifier(out AccessModifier getMethodAccessModifier)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            getMethodAccessModifier = propertyData!.SetAccessorAccessModifier;
            return true;
        }
        else
        {
            getMethodAccessModifier = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the property is an indexer. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsIndexer(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the property is an indexer; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsIndexer => GetPropertyDataOrThrow().IsIndexer;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the associated property is an indexer.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isIndexer"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsIndexer"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isIndexer">When this method returns successfully, contains a value indicating whether the property is an indexer.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsIndexer(out bool isIndexer)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            isIndexer = propertyData!.IsIndexer;
            return true;
        }
        else
        {
            isIndexer = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the property is an init-only property. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsInit(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <value><see langword="true"/> if the property is an init-only property; otherwise, <see langword="false"/>.</value>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsInit => GetPropertyDataOrThrow().IsInit;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the property is an init-only property.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isInit"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsInit"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isInit">When this method returns successfully, contains a value indicating whether the property is an init-only property.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsInit(out bool isInit)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            isInit = propertyData!.IsInit;
            return true;
        }
        else
        {
            isInit = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the property is an override. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsOverride(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public bool IsOverride => GetPropertyDataOrThrow().IsOverride;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the property is an override.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="isOverride"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsOverride"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isOverride">When this method returns successfully, contains a value indicating whether the property is an override.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsOverride(out bool isOverride)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            isOverride = propertyData!.IsOverride;
            return true;
        }
        else
        {
            isOverride = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the property is read-only. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsReadOnly(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value><see langword="true"/> if the property is read-only (which is when <see cref="CanRead"/> returns <see langword="true"/> and <see cref="CanWrite"/> returns <see langword="false"/>); otherwise, <see langword="false"/>.</value>
    public bool IsReadOnly => GetPropertyDataOrThrow().IsReadOnly;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the associated property can be read from.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="isReadOnly"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsReadOnly"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isReadOnly">When this method returns successfully, contains a value indicating whether the property is read-only.
    /// <br/>The value is <see langword="true"/> if the property is read-only (which is when <see cref="CanRead"/> returns <see langword="true"/> and <see cref="CanWrite"/> returns <see langword="false"/>); otherwise, <see langword="false"/>.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsReadOnly(out bool isReadOnly)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            isReadOnly = propertyData!.CanRead;
            return true;
        }
        else
        {
            isReadOnly = default;
            return false;
        }
    }

    /// <summary>
    /// Returns whether the property is sealed. 
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIsSealed(out bool)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value><see langword="true"/> if the property is sealed; otherwise, <see langword="false"/>.</value>
    public bool IsSealed => GetPropertyDataOrThrow().IsSealed;

    /// <summary>
    /// Attempts to retrieve a value indicating whether the associated property is sealed.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="isSealed"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="IsSealed"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="isSealed">When this method returns successfully, contains a value indicating whether the property is sealed.
    /// <br/>The value is <see langword="true"/> if the property is sealed.</param>
    /// <returns>Returns <see langword="true"/> if the property data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIsSealed(out bool isSealed)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            isSealed = propertyData!.IsSealed;
            return true;
        }
        else
        {
            isSealed = default;
            return false;
        }
    }

    /// <summary>
    /// Returns <see cref="IMethodDataView"/> representing the get method of the property, if available.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetPropertyGetMethodData(out IMethodDataView?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <exception cref="NotSupportedException">Thrown if the property does not have a get method.</exception>
    /// <value><see cref="IMethodDataView"/> representing the get method of the property".</value>
    public IMethodDataView PropertyGetMethodData => GetPropertyDataOrThrow().PropertyGetMethodData.View;

    /// <summary>
    /// Attempts to retrieve the get method of the property.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="getMethodData"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertyGetMethodData"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="getMethodData">When this method returns successfully, contains the get method of the property.
    /// <br/>The value is <see langword="true"/> if the property is sealed.</param>
    /// <returns>Returns <see langword="true"/> if the property has a getter and the data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetPropertyGetMethodData(out IMethodDataView? getMethodData)
    {
        if (TryGetPropertyData(out PropertyData? propertyData) && propertyData!.CanRead)
        {
            getMethodData = propertyData.PropertyGetMethodData.View;
            return true;
        }
        else
        {
            getMethodData = default;
            return false;
        }
    }

    /// <summary>
    /// Returns <see cref="IMethodDataView"/> representing the set method of the property, if available.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetPropertySetMethodData(out IMethodDataView?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <exception cref="NotSupportedException">Thrown if the property does not have a set method.</exception>
    /// <value><see cref="IMethodDataView"/> representing the set method of the property.</value>
    public IMethodDataView PropertySetMethodData => GetPropertyDataOrThrow().PropertySetMethodData.View;

    /// <summary>
    /// Attempts to retrieve the set method of the property.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="setMethodData"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertySetMethodData"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="setMethodData">When this method returns successfully, contains the set method of the property.
    /// <returns>Returns <see langword="true"/> if the property has a setter and the data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetPropertySetMethodData(out IMethodDataView? setMethodData)
    {
        if (TryGetPropertyData(out PropertyData? propertyData) && propertyData!.CanWrite)
        {
            setMethodData = propertyData.PropertySetMethodData.View;
            return true;
        }
        else
        {
            setMethodData = default;
            return false;
        }
    }
    public IParameterListView PropertyGetMethodParameters { get; }
    public IParameterListView PropertySetMethodParameters { get; }
    public ITypeDataView? PropertyTypeData { get; }
    public AccessModifier AccessModifier { get; }
    public BindingFlags BindingFlagsVisibilityMask { get; }
    public ITypeDataView? DeclaringTypData { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public RuntimeTypeHandle ImplementingTypeHandle { get; }
    public ITypeDataView? ImplementingTypData { get; }
    public bool IsAssembly { get; }
    public bool IsExplicitInterfaceImplementation { get; }
    public bool IsFamily { get; }
    public bool IsFamilyAndAssembly { get; }
    public bool IsFamilyOrAssembly { get; }
    public bool IsPrivate { get; }
    public bool IsPublic { get; }
    public bool IsStatic { get; }

    public object? GetIndexerValue(object? target, object?[] indexerPropertyParameters) => throw new NotImplementedException();
    public TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indexerPropertyParameters) => throw new NotImplementedException();
    public TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object[] indexerPropertyParameters) => throw new NotImplementedException();
    public object? GetValue(object? target) => throw new NotImplementedException();
    public TValue GetValue<TTarget, TValue>(TTarget target) => throw new NotImplementedException();
    public void SetIndexerValue(object? target, object? value, object?[]? indexerPropertyIndex = null) => throw new NotImplementedException();
    public void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, object[]? indexerPropertyIndex = null) where TTarget : struct => throw new NotImplementedException();
    public void SetValue(object? target, object? value) => throw new NotImplementedException();
    public void SetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class => throw new NotImplementedException();
}