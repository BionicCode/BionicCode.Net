namespace BionicCode.Utilities.Net.Reflection;

using BionicCode.Utilities.Net;
using BionicCode.Utilities.Net.Reflection.Exceptions;

internal sealed class PropertyDataView : MemberDataView, IPropertyDataView
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
    /// <br/>The value is <see langword="true"/> if the property is read-only (which is when <see cref="CanRead"/> returns <see langword="true"/> 
    /// and <see cref="CanWrite"/> returns <see langword="false"/>); otherwise, <see langword="false"/>.</param>
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
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a get method.</exception>
    /// <value><see cref="IMethodDataView"/> representing the get method of the property".</value>
    public IMethodDataView PropertyGetMethod => GetPropertyDataOrThrow() is PropertyData propertyData && propertyData.CanRead
        ? propertyData.PropertyGetMethodData.View
        : throw new InvalidOperationException($"The property '{GetPropertyDataOrThrow().FullyQualifiedSignature}' does not have a getter.");

    /// <summary>
    /// Attempts to retrieve the get method of the property.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="getMethodData"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertyGetMethod"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="getMethodData">When this method returns successfully, contains the get method of the property.
    /// <br/>The value is <see langword="true"/> if the property is sealed.
    /// <br/>If this method succeeds (which is when the return value is <see langword="true"/>) then <paramref name="getMethodData"/> is guaranteed to never be <see langword="null".</param>
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
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a set method.</exception>
    /// <value><see cref="IMethodDataView"/> representing the set method of the property.</value>
    public IMethodDataView PropertySetMethod => GetPropertyDataOrThrow() is PropertyData propertyData && propertyData.CanWrite
        ? propertyData.PropertySetMethodData.View
        : throw new InvalidOperationException($"The property '{GetPropertyDataOrThrow().FullyQualifiedSignature}' does not have a setter.");

    /// <summary>
    /// Attempts to retrieve the set method of the property.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="setMethodData"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertySetMethod"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="setMethodData">When this method returns successfully, contains the set method of the property.
    /// <br/>If this method succeeds (which is when the return value is <see langword="true"/>) then <paramref name="setMethodData"/> is guaranteed to never be <see langword="null".</param>
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

    /// <summary>
    /// Returns <see cref="IParameterListView"/> representing the property's get method parameters, if property has a getter and the property is an indexer.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetPropertyGetMethodParameters(out IParameterListView?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a set method.</exception>
    /// <value><see cref="IParameterListView"/> representing the parameters of the property's get method. Will be empty for indexer getters.</value>
    public IParameterListView PropertyGetMethodParameters => GetPropertyDataOrThrow() is PropertyData propertyData && propertyData.CanRead
        ? propertyData.PropertyGetMethodParameters.View
        : throw new InvalidOperationException($"The property '{GetPropertyDataOrThrow().FullyQualifiedSignature}' does not have a getter.");

    /// <summary>
    /// Attempts to retrieve the property's get method parameters (for indexer properties).
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="getMethodParameterListView"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertyGetMethodParameters"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="getMethodParameterListView">When this method returns successfully, contains the property's get method parameters. The collection will be empty for indexer getters.
    /// <br/>If this method succeeds (which is when the return value is <see langword="true"/>) then <paramref name="getMethodParameterListView"/> is guaranteed to never be <see langword="null".</param>
    /// <returns>Returns <see langword="true"/> if the property has a parameterized getter (indexer property) and the data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetPropertyGetMethodParameters(out IParameterListView? getMethodParameterListView)
    {
        if (TryGetPropertyData(out PropertyData? propertyData) && propertyData!.CanRead)
        {
            getMethodParameterListView = propertyData.PropertyGetMethodParameters.View;
            return true;
        }
        else
        {
            getMethodParameterListView = default;
            return false;
        }
    }

    /// <summary>
    /// Returns <see cref="IParameterListView"/> representing the property's set method parameters, if property has a setter.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetPropertySetMethodParameters(out IParameterListView?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a set method.</exception>
    /// <value><see cref="IParameterListView"/> representing the parameters of the property's set method.</value>
    public IParameterListView PropertySetMethodParameters => GetPropertyDataOrThrow() is PropertyData propertyData && propertyData.CanWrite
        ? propertyData.PropertySetMethodParameters.View
        : throw new InvalidOperationException($"The property '{GetPropertyDataOrThrow().FullyQualifiedSignature}' does not have a setter.");

    /// <summary>
    /// Attempts to retrieve the property's set method parameters.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="setMethodParameterListView"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertySetMethodParameters"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="setMethodParameterListView">When this method returns successfully, contains the property's set method parameters.
    /// <br/>If this method succeeds (which is when the return value is <see langword="true"/>) then <paramref name="setMethodParameterListView"/> is guaranteed to never be <see langword="null".</param>
    /// <returns>Returns <see langword="true"/> if the the data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetPropertySetMethodParameters(out IParameterListView? setMethodParameterListView)
    {
        if (TryGetPropertyData(out PropertyData? propertyData) && propertyData!.CanWrite)
        {
            setMethodParameterListView = propertyData.PropertySetMethodParameters.View;
            return true;
        }
        else
        {
            setMethodParameterListView = default;
            return false;
        }
    }

    /// <summary>
    /// Returns <see cref="ITypeDataView"/> representing the property's type.
    /// </summary>
    /// <remarks>
    /// This property throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetPropertyTypeData(out ITypeDataView?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    /// <value><see cref="ITypeDataView"/> representing the property's type.</value>
    public ITypeDataView PropertyType => GetPropertyDataOrThrow().PropertyTypeData.View;

    /// <summary>
    /// Attempts to retrieve the property's type data.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded, 
    /// the <paramref name="propertyTypeDataView"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use the property <see cref="PropertyType"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="propertyTypeDataView">When this method returns successfully, contains the property's type data. 
    /// <br/>If this method succeeds (which is when the return value is <see langword="true"/>) then <paramref name="propertyTypeDataView"/> is guaranteed to never be <see langword="null".</param>
    /// <returns>Returns <see langword="true"/> if the the data is still reachable in the environment and was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetPropertyTypeData(out ITypeDataView? propertyTypeDataView)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyTypeDataView = propertyData!.PropertyTypeData.View;
            return true;
        }
        else
        {
            propertyTypeDataView = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the value of the indexer property for the specified target and index parameters.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIndexerValue(object?, object?[], out object?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="indexerPropertyParameters">The indexer arguments used to resolve the property value.</param>
    /// <returns>The current value returned by the indexer property.</returns>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public object? GetIndexerValue(object? target, params object?[] indexerPropertyParameters) => GetPropertyDataOrThrow().GetIndexerValue(target, indexerPropertyParameters);

    /// <summary>
    /// Attempts to retrieve the value of the indexer property for the specified target and index parameters.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="propertyValue"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use <see cref="GetIndexerValue(object?, object?[])"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="indexerPropertyParameters">The indexer arguments used to resolve the property value.</param>
    /// <param name="propertyValue">When this method returns successfully, contains the current value returned by the indexer property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIndexerValue(object? target, out object? propertyValue, params object?[] indexerPropertyParameters)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyValue = propertyData!.GetIndexerValue(target, indexerPropertyParameters);
            return true;
        }
        else
        {
            propertyValue = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the value of the indexer property for the specified target and strongly typed index parameters.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIndexerValue{TTarget, TValue, TIndex}(TTarget, TIndex[], out TValue)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <typeparam name="TTarget">The type of the target instance.</typeparam>
    /// <typeparam name="TValue">The expected return type of the property value.</typeparam>
    /// <typeparam name="TIndex">The type of the indexer arguments.</typeparam>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="indexerPropertyParameters">The indexer arguments used to resolve the property value.</param>
    /// <returns>The current value returned by the indexer property.</returns>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indexerPropertyParameters) => GetPropertyDataOrThrow().GetIndexerValue<TTarget, TValue, TIndex>(target, indexerPropertyParameters);

    /// <summary>
    /// Attempts to retrieve the value of the indexer property for the specified target and strongly typed index parameters.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="propertyValue"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use <see cref="GetIndexerValue{TTarget, TValue, TIndex}(TTarget, TIndex[])"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <typeparam name="TTarget">The type of the target instance.</typeparam>
    /// <typeparam name="TValue">The expected return type of the property value.</typeparam>
    /// <typeparam name="TIndex">The type of the indexer arguments.</typeparam>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="indexerPropertyParameters">The indexer arguments used to resolve the property value.</param>
    /// <param name="propertyValue">When this method returns successfully, contains the current value returned by the indexer property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIndexerValue<TTarget, TValue, TIndex>(TTarget target, out TValue propertyValue, params TIndex[] indexerPropertyParameters)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyValue = propertyData!.GetIndexerValue<TTarget, TValue, TIndex>(target, indexerPropertyParameters);
            return true;
        }
        else
        {
            propertyValue = default!;
            return false;
        }
    }

    /// <summary>
    /// Returns the value of the indexer property for the specified target and object index parameters.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetIndexerValue{TTarget, TValue}(TTarget, object[], out TValue)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <typeparam name="TTarget">The type of the target instance.</typeparam>
    /// <typeparam name="TValue">The expected return type of the property value.</typeparam>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="indexerPropertyParameters">The indexer arguments used to resolve the property value.</param>
    /// <returns>The current value returned by the indexer property.</returns>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object?[] indexerPropertyParameters) => GetPropertyDataOrThrow().GetIndexerValue<TTarget, TValue>(target, indexerPropertyParameters);

    /// <summary>
    /// Attempts to retrieve the value of the indexer property for the specified target and object index parameters.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="propertyValue"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use <see cref="GetIndexerValue{TTarget, TValue}(TTarget, object[])"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <typeparam name="TTarget">The type of the target instance.</typeparam>
    /// <typeparam name="TValue">The expected return type of the property value.</typeparam>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="indexerPropertyParameters">The indexer arguments used to resolve the property value.</param>
    /// <param name="propertyValue">When this method returns successfully, contains the current value returned by the indexer property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetIndexerValue<TTarget, TValue>(TTarget target, out TValue propertyValue, params object?[] indexerPropertyParameters)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyValue = propertyData!.GetIndexerValue<TTarget, TValue>(target, indexerPropertyParameters);
            return true;
        }
        else
        {
            propertyValue = default!;
            return false;
        }
    }

    /// <summary>
    /// Returns the value of the property for the specified target.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetValue(object?, out object?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <param name="target">The instance that owns the property.</param>
    /// <returns>The current value of the property.</returns>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public object? GetValue(object? target) => GetPropertyDataOrThrow().GetValue(target);

    /// <summary>
    /// Attempts to retrieve the value of the property for the specified target.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="propertyValue"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use <see cref="GetValue(object?)"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="target">The instance that owns the property.</param>
    /// <param name="propertyValue">When this method returns successfully, contains the current value of the property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue(object? target, out object? propertyValue)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyValue = propertyData!.GetValue(target);
            return true;
        }
        else
        {
            propertyValue = default;
            return false;
        }
    }

    /// <summary>
    /// Returns the value of the property for the specified target.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TryGetValue{TTarget, TValue}(TTarget, out TValue)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <typeparam name="TTarget">The type of the target instance.</typeparam>
    /// <typeparam name="TValue">The expected return type of the property value.</typeparam>
    /// <param name="target">The instance that owns the property.</param>
    /// <returns>The current value of the property.</returns>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public TValue GetValue<TTarget, TValue>(TTarget target) => GetPropertyDataOrThrow().GetValue<TTarget, TValue>(target);

    /// <summary>
    /// Attempts to retrieve the value of the property for the specified target.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the <paramref name="propertyValue"/> parameter is set to its default value and the method returns <see langword="false"/>.
    /// <para/>Use <see cref="GetValue{TTarget, TValue}(TTarget)"/> to get the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <typeparam name="TTarget">The type of the target instance.</typeparam>
    /// <typeparam name="TValue">The expected return type of the property value.</typeparam>
    /// <param name="target">The instance that owns the property.</param>
    /// <param name="propertyValue">When this method returns successfully, contains the current value of the property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved; otherwise, <see langword="false"/>.</returns>
    public bool TryGetValue<TTarget, TValue>(TTarget target, out TValue propertyValue)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyValue = propertyData!.GetValue<TTarget, TValue>(target);
            return true;
        }
        else
        {
            propertyValue = default!;
            return false;
        }
    }

    /// <summary>
    /// Sets the value of the indexer property for the specified target and index parameters.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TrySetIndexerValue(object?, object?, object?[]?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="value">The value to assign to the indexer property.</param>
    /// <param name="indexerPropertyIndex">The indexer arguments that identify the indexer property slot to update.</param>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public void SetIndexerValue(object? target, object? value, params object?[] indexerPropertyIndex) => GetPropertyDataOrThrow().SetIndexerValue(target, value, indexerPropertyIndex);

    /// <summary>
    /// Attempts to set the value of the indexer property for the specified target and index parameters.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the method returns <see langword="false"/>.
    /// <para/>Use <see cref="SetIndexerValue(object?, object?, object?[])"/> to set the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="target">The instance that owns the indexer property.</param>
    /// <param name="value">The value to assign to the indexer property.</param>
    /// <param name="indexerPropertyIndex">The indexer arguments that identify the indexer property slot to update.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved and the value was set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetIndexerValue(object? target, object? value, params object?[] indexerPropertyIndex)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyData!.SetIndexerValue(target, value, indexerPropertyIndex);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the value of the property on a target value type instance.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TrySetStructValue{TTarget, TValue}(ref TTarget, TValue, object[]?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <typeparam name="TTarget">The type of the target value type.</typeparam>
    /// <typeparam name="TValue">The type of the value to assign.</typeparam>
    /// <param name="target">The value type instance that owns the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="indexerPropertyIndex">The optional indexer arguments if the property is an indexer.</param>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value) where TTarget : struct => GetPropertyDataOrThrow().SetStructValue(ref target, value);

    /// <summary>
    /// Attempts to set the value of the property on a target value type instance.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the method returns <see langword="false"/>.
    /// <para/>Use <see cref="SetStructValue{TTarget, TValue}(ref TTarget, TValue, object?[]?)"/> to set the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <typeparam name="TTarget">The type of the target value type.</typeparam>
    /// <typeparam name="TValue">The type of the value to assign.</typeparam>
    /// <param name="target">The value type instance that owns the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="indexerPropertyIndex">The optional indexer arguments if the property is an indexer.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved and the value was set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetStructValue<TTarget, TValue>(ref TTarget target, TValue value) where TTarget : struct
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyData!.SetStructValue(ref target, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the value of the property for the specified target.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TrySetValue(object?, object?)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <param name="target">The instance that owns the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public void SetValue(object? target, object? value) => GetPropertyDataOrThrow().SetValue(target, value);

    /// <summary>
    /// Attempts to set the value of the property for the specified target.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the method returns <see langword="false"/>.
    /// <para/>Use <see cref="SetValue(object?, object?)"/> to set the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <param name="target">The instance that owns the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved and the value was set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetValue(object? target, object? value)
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyData!.SetValue(target, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Sets the value of the property for the specified target.
    /// </summary>
    /// <remarks>
    /// This method throws a <see cref="ReflectionCacheEntryAlcNotAvailableException"/> if the underlying cache entry is not available,
    /// which can happen if the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> associated with the cache entry has been unloaded.
    /// <para/>To avoid the exception, use <see cref="TrySetValue{TTarget, TValue}(TTarget, TValue)"/> which returns a boolean indicating success or failure instead of throwing.
    /// </remarks>
    /// <typeparam name="TTarget">The type of the target reference type.</typeparam>
    /// <typeparam name="TValue">The type of the value to assign.</typeparam>
    /// <param name="target">The instance that owns the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <exception cref="ReflectionCacheEntryAlcNotAvailableException">Thrown if the underlying cache entry is not available due to <see cref="System.Runtime.Loader.AssemblyLoadContext"/> unload.</exception>
    public void SetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class => GetPropertyDataOrThrow().SetValue(target, value);

    /// <summary>
    /// Attempts to set the value of the property for the specified target.
    /// </summary>
    /// <remarks>If the property data is not found in the cache due to the <see cref="System.Runtime.Loader.AssemblyLoadContext"/> being unloaded,
    /// the method returns <see langword="false"/>.
    /// <para/>Use <see cref="SetValue{TTarget, TValue}(TTarget, TValue)"/> to set the value directly, which throws an exception if the cache entry is not available.</remarks>
    /// <typeparam name="TTarget">The type of the target reference type.</typeparam>
    /// <typeparam name="TValue">The type of the value to assign.</typeparam>
    /// <param name="target">The instance that owns the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <returns><see langword="true"/> if the property data was successfully retrieved and the value was set; otherwise, <see langword="false"/>.</returns>
    public bool TrySetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class
    {
        if (TryGetPropertyData(out PropertyData? propertyData))
        {
            propertyData!.SetValue(target, value);
            return true;
        }
        else
        {
            return false;
        }
    }

    public TValue GetIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TIndex1 index1, TIndex2 index2) => throw new NotImplementedException();
    public TValue GetIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3) => throw new NotImplementedException();
    public void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct => throw new NotImplementedException();
    public bool TrySetStructValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct => throw new NotImplementedException();
    public void SetStructIndexerValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indexerPropertyIndex) where TTarget : struct => throw new NotImplementedException();
    public void Set3DIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) => throw new NotImplementedException();
    public void Set2DIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TValue value, TIndex1 index1, TIndex2 index2) => throw new NotImplementedException();
    public void SetIndexerValue<TTarget, TValue, TIndex1>(TTarget target, TValue value, TIndex1 index1) => throw new NotImplementedException();
    public void SetStruct3DIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : struct => throw new NotImplementedException();
    public void SetStruct2DIndexerValue<TTarget, TValue, TIndex1, TIndex2>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : struct => throw new NotImplementedException();
    public void SetStructIndexerValue<TTarget, TValue, TIndex1>(ref TTarget target, TValue value, TIndex1 index1) where TTarget : struct => throw new NotImplementedException();
}