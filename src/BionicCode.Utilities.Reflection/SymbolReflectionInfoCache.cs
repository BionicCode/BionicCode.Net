namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BionicCode.Utilities.Net.Reflection.Exceptions;
using Microsoft.CodeAnalysis;

internal static class SymbolReflectionInfoCache
{
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, SymbolInfoData> s_symbolInfoDataCache = new();
    private static readonly ConcurrentDictionary<SymbolInfoData, SymbolReflectionInfoCacheKeyInternal> s_reverseSymbolInfoDataCache = new();
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKey, SymbolReflectionInfoCacheKeyInternal> s_publicCacheKeyMap = new();
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, SymbolReflectionInfoCacheKey> s_reversePublicCacheKeyMap = new();
    //private static readonly ConcurrentDictionary<RuntimeMethodHandle, SymbolReflectionInfoCacheKeyInternal> s_wellKnownMethodAndConstructorCacheKeyTable = new();
    //private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, RuntimeMethodHandle> s_reverseWellKnownMethodAndConstructorCacheKeyTable = new();
    //private static readonly ConcurrentDictionary<RuntimeTypeHandle, SymbolReflectionInfoCacheKeyInternal> s_wellKnownTypeCacheKeyTable = new();
    //private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, RuntimeTypeHandle> s_reverseWellKnownTypeCacheKeyTable = new();
    private static readonly ConcurrentDictionary<object, SymbolReflectionInfoCacheKeyInternal> s_wellKnownSymbolInfoCacheKeyTable = new();
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, object> s_reverseWellKnownSymbolInfoCacheKeyTable = new();
    private static readonly ConcurrentDictionary<AssemblyLoadContextMonitor.AssemblyLoadContextInfo, ConcurrentHashSet<SymbolReflectionInfoCacheKeyInternal>> s_assemblyLoadContextIdToCacheKeyMap = new();
    private static readonly ConcurrentDictionary<AnonymousSymbolDescriptorContainer, SymbolReflectionInfoCacheKeyInternal> s_normalizedAnonymousCacheKeyMap = new();
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, AnonymousSymbolDescriptorContainer> s_reverseNormalizedAnonymousCacheKeyMap = new();
    private static readonly ConcurrentDictionary<AmbiguousIndexerPropertyKey, SymbolReflectionInfoCacheKeyInternal> s_indexerParameterSymbolDataCacheKeyMap = new();
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, AmbiguousIndexerPropertyKey> s_reverseIndexerParameterSymbolDataCacheKeyMap = new();
    private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKeyInternal, IEventDataView> s_eventDataViewTable = new();
    private const string DeclaringTypeHandleInKeyIsDefaultExceptionMessage = $"The value 'default' is not a valid value for the key's '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)}' property. The property must reference a valid declaring type handle.";

    static SymbolReflectionInfoCache() => AssemblyLoadContextMonitor.AssemblyLoadContextUnloading += OnAssemblyLoadContextUnloading;

    private static void OnAssemblyLoadContextUnloading(object? sender, AssemblyLoadContextMonitor.AssemblyLoadContextUnloadingEventArgs e)
    {
        AssemblyLoadContextMonitor.AssemblyLoadContextInfo assemblyLoadContextInfo = e.AssemblyLoadContextInfo;
        lock (assemblyLoadContextInfo.SyncLock)
        {
            if (s_assemblyLoadContextIdToCacheKeyMap.TryRemove(assemblyLoadContextInfo, out ConcurrentHashSet<SymbolReflectionInfoCacheKeyInternal>? cacheKeysOfAssemblyLoadContext))
            {
                foreach (SymbolReflectionInfoCacheKeyInternal cacheKey in cacheKeysOfAssemblyLoadContext)
                {
                    if (s_symbolInfoDataCache.TryRemove(cacheKey, out SymbolInfoData? symbolInfoData))
                    {
                        _ = s_reverseSymbolInfoDataCache.TryRemove(symbolInfoData, out _);
                    }

                    ClearEventDataViewTable(cacheKey);
                    ClearAnonymousCacheKeyMap(cacheKey);
                    //ClearWellKnownMethodAndConstructorCacheKeyTable(cacheKey);
                    ClearIndexerParameterSymbolDataCacheKeyMap(cacheKey);
                    //ClearWellKnownTypeCacheKeyTable(cacheKey);
                    ClearWellKnownSymbolCacheKeyTable(cacheKey);
                    ClearPublicCacheKeyMap(cacheKey);
                }

                cacheKeysOfAssemblyLoadContext.Clear();
            }
        }
    }

    private static void ClearPublicCacheKeyMap(SymbolReflectionInfoCacheKeyInternal cacheKey)
    {
        if (s_reversePublicCacheKeyMap.TryRemove(cacheKey, out SymbolReflectionInfoCacheKey publicCacheKey))
        {
            _ = SymbolReflectionInfoCache.s_publicCacheKeyMap.TryRemove(publicCacheKey, out _);
        }
    }

    private static void ClearEventDataViewTable(SymbolReflectionInfoCacheKeyInternal cacheKey) => _ = SymbolReflectionInfoCache.s_eventDataViewTable.TryRemove(cacheKey, out _);

    private static void ClearAnonymousCacheKeyMap(SymbolReflectionInfoCacheKeyInternal cacheKey)
    {
        if (SymbolReflectionInfoCache.s_reverseNormalizedAnonymousCacheKeyMap.TryRemove(cacheKey, out AnonymousSymbolDescriptorContainer anonymousSymbolReflectionInfoCacheKey))
        {
            _ = SymbolReflectionInfoCache.s_normalizedAnonymousCacheKeyMap.TryRemove(anonymousSymbolReflectionInfoCacheKey, out _);
        }
    }

    //private static void ClearWellKnownMethodAndConstructorCacheKeyTable(SymbolReflectionInfoCacheKeyInternal cacheKey)
    //{
    //    if (SymbolReflectionInfoCache.s_reverseWellKnownMethodAndConstructorCacheKeyTable.TryRemove(cacheKey, out RuntimeMethodHandle methodHandle))
    //    {
    //        _ = SymbolReflectionInfoCache.s_wellKnownMethodAndConstructorCacheKeyTable.TryRemove(methodHandle, out _);
    //    }
    //}

    //private static void ClearWellKnownTypeCacheKeyTable(SymbolReflectionInfoCacheKeyInternal cacheKey)
    //{
    //    if (SymbolReflectionInfoCache.s_reverseWellKnownTypeCacheKeyTable.TryRemove(cacheKey, out RuntimeTypeHandle typeHandle))
    //    {
    //        _ = SymbolReflectionInfoCache.s_wellKnownTypeCacheKeyTable.TryRemove(typeHandle, out _);
    //    }
    //}

    private static void ClearWellKnownSymbolCacheKeyTable(SymbolReflectionInfoCacheKeyInternal cacheKey)
    {
        if (SymbolReflectionInfoCache.s_reverseWellKnownSymbolInfoCacheKeyTable.TryRemove(cacheKey, out object? symbolInfo))
        {
            _ = SymbolReflectionInfoCache.s_wellKnownSymbolInfoCacheKeyTable.TryRemove(symbolInfo, out _);
        }
    }

    private static void ClearIndexerParameterSymbolDataCacheKeyMap(SymbolReflectionInfoCacheKeyInternal cacheKey)
    {
        if (SymbolReflectionInfoCache.s_reverseIndexerParameterSymbolDataCacheKeyMap.TryRemove(cacheKey, out AmbiguousIndexerPropertyKey ambiguousKey))
        {
            _ = SymbolReflectionInfoCache.s_indexerParameterSymbolDataCacheKeyMap.TryRemove(ambiguousKey, out _);
        }
    }

    #region Extension Methods
    /// <summary>
    /// Converts the specified <see cref="Type"/> to a <see cref="TypeData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="type">The type to convert to a <see cref="TypeData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="TypeData"/> instance that describes the specified type. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="TypeData"/> instances for the same type.</remarks>
    public static ITypeDataView ToTypeDataView(this Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type);

        return GetOrCreateEntryInternal(type).View;
    }

    /// <summary>
    /// Converts the specified <see cref="MethodInfo"/> to a <see cref="MethodData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="methodInfo">The method to convert to a <see cref="MethodData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="MethodData"/> instance that describes the specified method. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="MethodData"/> instances for the same method.</remarks>
    public static IMethodDataView ToMethodDataView(this MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        return GetOrCreateEntryInternal(methodInfo).View;
    }

    /// <summary>
    /// Converts the specified <see cref="ConstructorInfo"/> to a <see cref="ConstructorData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="constructorInfo">The constructor to convert to a <see cref="ConstructorData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="ConstructorData"/> instance that describes the specified constructor. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="ConstructorData"/> instances for the same constructor.</remarks>
    public static IConstructorDataView ToConstructorDataView(this ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo);

        return GetOrCreateEntryInternal(constructorInfo).View;
    }

    /// <summary>
    /// Converts the specified <see cref="FieldInfo"/> to a <see cref="FieldData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="fieldInfo">The field to convert to a <see cref="FieldData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="FieldData"/> instance that describes the specified field. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="FieldData"/> instances for the same field.</remarks>
    public static IFieldDataView ToFieldDataView(this FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo);

        return GetOrCreateEntryInternal(fieldInfo).View;
    }

    /// <summary>
    /// Converts the specified <see cref="PropertyInfo"/> to a <see cref="PropertyData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="propertyInfo">The property to convert to a <see cref="PropertyData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="PropertyData"/> instance that describes the specified property. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="PropertyData"/> instances for the same property.</remarks>
    public static IPropertyDataView ToPropertyDataView(this PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo);

        return GetOrCreateEntryInternal(propertyInfo).View;
    }

    /// <summary>
    /// Converts the specified <see cref="EventInfo"/> to a <see cref="EventData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="eventInfo">The event to convert to a <see cref="EventData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="EventData"/> instance that describes the specified event. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="EventData"/> instances for the same event.</remarks>
    public static IEventDataView ToEventDataView(this EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo);

        return GetOrCreateEntryInternal(eventInfo).View;
    }

    /// <summary>
    /// Converts the specified <see cref="ParameterInfo"/> to a <see cref="ParameterData"/> instance representing its metadata and
    /// characteristics.
    /// </summary>
    /// <param name="parameterInfo">The parameter to convert to a <see cref="ParameterData"/> instance. Cannot be <see langword="null"/>.</param>
    /// <returns>A <see cref="ParameterData"/> instance that describes the specified parameter. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
    /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="ParameterData"/> instances for the same parameter.</remarks>
    public static IParameterDataView ToParameterDataView(this ParameterInfo parameterInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo);

        return GetOrCreateEntryInternal(parameterInfo).View;
    }
    #endregion Extension Methods

    #region Property Cache API
    public static IPropertyDataView GetOrCreateCacheEntry(PropertyInfo propertyInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo);

        return GetOrCreateEntryInternal(propertyInfo).View;
    }

    private static PropertyData GetOrCreateEntryInternal(PropertyInfo propertyInfo)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(propertyInfo, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is PropertyData existingPropertyData)
            {
                return existingPropertyData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the property '{cacheKey.PropertyDescriptor.PropertyName}', but no corresponding symbol info existingTypeData entry exists.");
            }
        }

        Type? declaringType = propertyInfo.DeclaringType;
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType, nameof(propertyInfo), $"The '{nameof(PropertyInfo.DeclaringType)}' property of the provided '{nameof(propertyInfo)}' is null. Property must have a declaring type.");

        var descriptor = new WellKnownPropertyDescriptor(propertyInfo);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForProperty(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[propertyInfo] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = propertyInfo;
        MonitorAssemblyLoadContextOfType(cacheKey, declaringType);
        var propertyData = (PropertyData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new PropertyData(descriptor));
        s_reverseSymbolInfoDataCache[propertyData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForProperty(propertyData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {propertyInfo.GetType()}");

        return propertyData;
    }

    /// <summary>
    /// Attempts to retrieve the cached property data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the property data. If the
    /// cache key is invalid or does not correspond to a property, the method returns <see langword="false"/> and sets
    /// <paramref name="propertyDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the property's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.MemberProperty"/>.</param>
    /// <param name="propertyDataView">When this method returns <see langword="true"/>, contains the property data view of type <see cref="IPropertyDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the property cache entry is found and the property data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.MemberProperty"/>.</exception>
    public static bool TryGetPropertyCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out IPropertyDataView? propertyDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberProperty],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a property symbol.");

        propertyDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing property data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetPropertyDataCacheEntryInternal(internalCacheKey, out PropertyData propertyData))
        {
            propertyDataView = propertyData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetPropertyDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out PropertyData propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.MemberProperty],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating a property symbol.");

        return TryGetPropertyDataCacheEntryInternal(internalCacheKey, out propertyData);
    }

    private static bool TryGetPropertyDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out PropertyData? propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberProperty],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a property symbol.");

        propertyData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing property data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetPropertyDataCacheEntryInternal(internalCacheKey, out propertyData);
    }

    private static bool TryGetPropertyDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out PropertyData propertyData)
    {
        propertyData = null!;
        if (s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is PropertyData existingPropertyData)
        {
            propertyData = existingPropertyData;

            return true;
        }

        // No existing property data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Property Cache API

    #region Event Cache API
    public static IEventDataView GetOrCreateCacheEntry(EventInfo eventInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo);

        return GetOrCreateEntryInternal(eventInfo).View;
    }

    private static EventData GetOrCreateEntryInternal(EventInfo eventInfo)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(eventInfo, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is EventData existingEventData)
            {
                return existingEventData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the event '{cacheKey.EventDescriptor.EventName}', but no corresponding symbol info existingTypeData entry exists.");
            }
        }

        Type? declaringType = eventInfo.DeclaringType;
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType, nameof(eventInfo), $"The '{nameof(EventInfo.DeclaringType)}' property of the provided '{nameof(eventInfo)}' is null. Event must have a declaring type.");

        var descriptor = new WellKnownEventDescriptor(eventInfo);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForEvent(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[eventInfo] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = eventInfo;
        MonitorAssemblyLoadContextOfType(cacheKey, declaringType);
        var eventData = (EventData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new EventData(descriptor));
        s_reverseSymbolInfoDataCache[eventData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForEvent(eventData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {eventInfo.GetType()}");

        return eventData;
    }

    /// <summary>
    /// Attempts to retrieve the cached event data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the event data. If the
    /// cache key is invalid or does not correspond to an event, the method returns <see langword="false"/> and sets
    /// <paramref name="eventDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the event's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.MemberEvent"/>.</param>
    /// <param name="eventDataView">When this method returns <see langword="true"/>, contains the event data view of type <see cref="IEventDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the event cache entry is found and the event data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.MemberEvent"/>.</exception>
    public static bool TryGetEventCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out IEventDataView? eventDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberEvent],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating an event symbol.");

        eventDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing event data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetEventDataCacheEntryInternal(internalCacheKey, out EventData eventData))
        {
            eventDataView = eventData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetEventDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out EventData eventData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.MemberEvent],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating an event symbol.");

        return TryGetEventDataCacheEntryInternal(internalCacheKey, out eventData);
    }

    private static bool TryGetEventDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out EventData? eventData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberEvent],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating an event symbol.");

        eventData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing event data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetEventDataCacheEntryInternal(internalCacheKey, out eventData);
    }

    private static bool TryGetEventDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out EventData eventData)
    {
        eventData = null!;
        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is EventData existingEventData)
        {
            eventData = existingEventData;

            return true;
        }

        // No existing event data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Event Cache API

    #region Field Cache API
    public static IFieldDataView GetOrCreateCacheEntry(FieldInfo fieldInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo);

        return GetOrCreateEntryInternal(fieldInfo).View;
    }

    private static FieldData GetOrCreateEntryInternal(FieldInfo fieldInfo)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(fieldInfo, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is FieldData existingFieldData)
            {
                return existingFieldData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the field '{cacheKey.FieldDescriptor.FieldName}', but no corresponding symbol info existingTypeData entry exists.");
            }
        }

        Type? declaringType = fieldInfo.DeclaringType;
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType, nameof(fieldInfo), $"The '{nameof(FieldInfo.DeclaringType)}' property of the provided '{nameof(fieldInfo)}' is null. Field must have a declaring type.");

        var descriptor = new WellKnownFieldDescriptor(fieldInfo);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForField(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[fieldInfo] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = fieldInfo;
        MonitorAssemblyLoadContextOfType(cacheKey, declaringType);
        var fieldData = (FieldData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new FieldData(descriptor));
        s_reverseSymbolInfoDataCache[fieldData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForField(fieldData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {fieldInfo.GetType()}");

        return fieldData;
    }

    /// <summary>
    /// Attempts to retrieve the cached field data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the field data. If the
    /// cache key is invalid or does not correspond to a field, the method returns <see langword="false"/> and sets
    /// <paramref name="fieldDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the field's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.MemberField"/>.</param>
    /// <param name="fieldDataView">When this method returns <see langword="true"/>, contains the field data view of type <see cref="IFieldDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the field cache entry is found and the field data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.MemberField"/>.</exception>
    public static bool TryGetFieldCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out IFieldDataView? fieldDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberField],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a field symbol.");

        fieldDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing field data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetFieldDataCacheEntryInternal(internalCacheKey, out FieldData fieldData))
        {
            fieldDataView = fieldData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetFieldDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out FieldData fieldData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.MemberField],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating a field symbol.");

        return TryGetFieldDataCacheEntryInternal(internalCacheKey, out fieldData);
    }

    private static bool TryGetFieldDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out FieldData? fieldData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberField],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a field symbol.");

        fieldData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing field data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetFieldDataCacheEntryInternal(internalCacheKey, out fieldData);
    }

    private static bool TryGetFieldDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out FieldData fieldData)
    {
        fieldData = null!;
        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is FieldData existingFieldData)
        {
            fieldData = existingFieldData;

            return true;
        }

        // No existing field data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Field Cache API

    #region Method Cache API
    public static IMethodDataView GetOrCreateCacheEntry(MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        return GetOrCreateEntryInternal(methodInfo).View;
    }

    private static MethodData GetOrCreateEntryInternal(MethodInfo methodInfo)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(methodInfo, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is MethodData existingMethodData)
            {
                return existingMethodData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the method '{cacheKey.MethodDescriptor.MethodName}', but no corresponding symbol info existingTypeData entry exists.");
            }
        }

        Type? declaringType = methodInfo.DeclaringType;
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType, nameof(methodInfo), $"The '{nameof(MethodInfo.DeclaringType)}' property of the provided '{nameof(methodInfo)}' is null. Method must have a declaring type.");

        var descriptor = new WellKnownMethodDescriptor(methodInfo);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForMethod(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[methodInfo] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = methodInfo;
        MonitorAssemblyLoadContextOfType(cacheKey, declaringType);
        var methodData = (MethodData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(descriptor));
        s_reverseSymbolInfoDataCache[methodData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForMethod(methodData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {methodInfo.GetType()}");

        return methodData;
    }

    /// <summary>
    /// Attempts to retrieve the cached method data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the method data. If the
    /// cache key is invalid or does not correspond to a method, the method returns <see langword="false"/> and sets
    /// <paramref name="methodDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the method's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.MemberMethod"/>.</param>
    /// <param name="methodDataView">When this method returns <see langword="true"/>, contains the method data view of type <see cref="IMethodDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the method cache entry is found and the method data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.MemberMethod"/>.</exception>
    public static bool TryGetMethodCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out IMethodDataView? methodDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberMethod],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a method symbol.");

        methodDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing method data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetMethodDataCacheEntryInternal(internalCacheKey, out MethodData methodData))
        {
            methodDataView = methodData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetMethodDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out MethodData methodData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.MemberMethod],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating a method symbol.");

        return TryGetMethodDataCacheEntryInternal(internalCacheKey, out methodData);
    }

    private static bool TryGetMethodDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out MethodData? methodData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberMethod],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a method symbol.");

        methodData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing method data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetMethodDataCacheEntryInternal(internalCacheKey, out methodData);
    }

    private static bool TryGetMethodDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out MethodData methodData)
    {
        methodData = null!;
        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is MethodData existingMethodData)
        {
            methodData = existingMethodData;

            return true;
        }

        // No existing method data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Method Cache API

    #region Constructor Cache API
    public static IConstructorDataView GetOrCreateCacheEntry(ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo);

        return GetOrCreateEntryInternal(constructorInfo).View;
    }

    private static ConstructorData GetOrCreateEntryInternal(ConstructorInfo constructorInfo)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(constructorInfo, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is ConstructorData existingConstructorData)
            {
                return existingConstructorData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the constructor, but no corresponding symbol info existingTypeData entry exists.");
            }
        }

        Type? declaringType = constructorInfo.DeclaringType;
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType, nameof(constructorInfo), $"The '{nameof(ConstructorInfo.DeclaringType)}' property of the provided '{nameof(constructorInfo)}' is null. Constructor must have a declaring type.");

        var descriptor = new WellKnownConstructorDescriptor(constructorInfo);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForConstructor(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[constructorInfo] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = constructorInfo;
        MonitorAssemblyLoadContextOfType(cacheKey, declaringType);
        var constructorData = (ConstructorData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new ConstructorData(descriptor));
        s_reverseSymbolInfoDataCache[constructorData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForConstructor(constructorData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {constructorInfo.GetType()}");

        return constructorData;
    }

    /// <summary>
    /// Attempts to retrieve the cached constructor data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the constructor data. If the
    /// cache key is invalid or does not correspond to a constructor, the method returns <see langword="false"/> and sets
    /// <paramref name="constructorDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the constructor's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.MemberConstructor"/>.</param>
    /// <param name="constructorDataView">When this method returns <see langword="true"/>, contains the constructor data view of type <see cref="IConstructorDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the constructor cache entry is found and the constructor data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.MemberConstructor"/>.</exception>
    public static bool TryGetConstructorCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out IConstructorDataView? constructorDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberConstructor],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

        constructorDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing constructor data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetConstructorDataCacheEntryInternal(internalCacheKey, out ConstructorData constructorData))
        {
            constructorDataView = constructorData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetConstructorDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out ConstructorData constructorData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.MemberConstructor],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

        return TryGetConstructorDataCacheEntryInternal(internalCacheKey, out constructorData);
    }

    private static bool TryGetConstructorDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out ConstructorData? constructorData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.MemberConstructor],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

        constructorData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing constructor data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetConstructorDataCacheEntryInternal(internalCacheKey, out constructorData);
    }

    private static bool TryGetConstructorDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out ConstructorData fieldData)
    {
        fieldData = null!;
        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is ConstructorData existingFieldData)
        {
            fieldData = existingFieldData;

            return true;
        }

        // No existing field data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Constructor Cache API

    #region Type Cache API

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to register a type while the assembly load context has already been unloaded.</exception>
    public static ITypeDataView GetOrCreateCacheEntry(Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type);

        return GetOrCreateEntryInternal(type).View;
    }

    private static TypeData GetOrCreateEntryInternal(Type type)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(type, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is TypeData existingTypeData)
            {
                return existingTypeData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the type '{cacheKey.TypeDescriptor.TypeName}', but no corresponding symbol info existingTypeData entry exists.");
            }
        }

        var descriptor = new WellKnownTypeDescriptor(type);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForType(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[type] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = type;
        MonitorAssemblyLoadContextOfType(cacheKey, type);
        var typeData = (TypeData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new TypeData(descriptor));
        s_reverseSymbolInfoDataCache[typeData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForType(typeData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {type.GetType()}");

        return typeData;
    }

    /// <summary>
    /// Attempts to retrieve the cached type data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the type data. If the
    /// cache key is invalid or does not correspond to a type, the method returns <see langword="false"/> and sets
    /// <paramref name="typeDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the type's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.Type"/>.</param>
    /// <param name="typeDataView">When this method returns <see langword="true"/>, contains the type data view of type <see cref="ITypeDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the type cache entry is found and the type data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.Type"/>.</exception>
    public static bool TryGetTypeCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out ITypeDataView? typeDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.Type],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a type symbol.");

        typeDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing type data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetTypeDataCacheEntryInternal(internalCacheKey, out TypeData typeData))
        {
            typeDataView = typeData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetTypeDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out TypeData typeData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.Type],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating a type symbol.");

        return TryGetTypeDataCacheEntryInternal(internalCacheKey, out typeData);
    }

    private static bool TryGetTypeDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out TypeData? typeData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.Type],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a type symbol.");

        typeData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing type data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetTypeDataCacheEntryInternal(internalCacheKey, out typeData);
    }

    private static bool TryGetTypeDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out TypeData typeData)
    {
        typeData = null!;
        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is TypeData existingTypeData)
        {
            typeData = existingTypeData;

            return true;
        }

        // No existing type data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Type Cache API

    #region Parameter Cache API
    public static IParameterDataView GetOrCreateCacheEntry(ParameterInfo parameter)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(parameter);

        return GetOrCreateEntryInternal(parameter).View;
    }

    private static ParameterData GetOrCreateEntryInternal(ParameterInfo parameter)
    {
        if (s_wellKnownSymbolInfoCacheKeyTable.TryGetValue(parameter, out SymbolReflectionInfoCacheKeyInternal cacheKey))
        {
            // If we found a key in the cache, we can use it to omit key generation costs
            if (s_symbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? symbolInfoData)
                && symbolInfoData is ParameterData existingParameterData)
            {
                return existingParameterData;
            }
            else
            {
                throw new InvalidOperationException($"Inconsistent state detected in the symbol reflection info cache. A cache key was found for the parameter '{cacheKey.TypeDescriptor.TypeName}', but no corresponding symbol info existingParameterData entry exists.");
            }
        }

        if (parameter.Member is PropertyInfo)
        {
            // Handle the case where the parameter is obtained from an indexer property via PropertyInfo.GetIndexParameters()
            // Such parameters are considered "ambiguous" since they don't have a unique identity on their own and are shared between the getter and setter of the indexer property.
            // To provide a consistent caching experience, we will normalize such parameters to be associated with either the getter or setter method (by convention, we choose the getter if it exists) of the indexer property.
            return ConvertAmbiguousIndexerPropertyParameterToAccessorAssociatedParameter(parameter);
        }

        var descriptor = new WellKnownParameterDescriptor(parameter);
        cacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForParameter(descriptor);
        s_wellKnownSymbolInfoCacheKeyTable[parameter] = cacheKey;
        s_reverseWellKnownSymbolInfoCacheKeyTable[cacheKey] = parameter;

        Type declaringType = parameter.Member.DeclaringType ?? throw new InvalidOperationException($"The declaring type of the member '{parameter.Member.Name}' for the parameter '{parameter.Name}' cannot be null.");
        MonitorAssemblyLoadContextOfType(cacheKey, declaringType);
        var parameterData = (ParameterData)s_symbolInfoDataCache.GetOrAdd(cacheKey, key => new ParameterData(descriptor));
        s_reverseSymbolInfoDataCache[parameterData] = cacheKey;
        var publicCacheKey = SymbolReflectionInfoCacheKey.CreateForParameter(parameterData);
        s_publicCacheKeyMap[publicCacheKey] = cacheKey;
        s_reversePublicCacheKeyMap[cacheKey] = publicCacheKey;

        // REMOVE::after testing
        Debug.WriteLine($"Found SymbolInfoData entry for {parameter.GetType()}");

        return parameterData;
    }

    private static ParameterData ConvertAmbiguousIndexerPropertyParameterToAccessorAssociatedParameter(ParameterInfo parameterInfo)
    {
        ArgumentExceptionAdvanced.ThrowIfFalse(
            parameterInfo.Member is PropertyInfo,
            nameof(parameterInfo),
            $"The provided argument '{nameof(parameterInfo)}' is not ambiguous. The provided parameter info must be ambiguous in that it was obtained via '{typeof(PropertyInfo).ToFullyQualifiedSignatureName()}.{nameof(PropertyInfo.GetIndexParameters)}()' belong to a property to be considered ambiguous.");

        var propertyInfo = parameterInfo.Member as PropertyInfo;
        PropertyData propertyData = GetOrCreateEntryInternal(propertyInfo!);

        // Since we came here from PropertyInfo.GetIndexParameters() call, the parameter can't be the 'value' parameter of the setter.
        // By convention, the getter takes precedence over the setter
        MethodData accessorData = propertyData.CanRead
            ? propertyData.PropertyGetMethodData
            : propertyData.PropertySetMethodData;

        ParameterData accessorParameter = accessorData.Parameters[parameterInfo.Position];

        return accessorParameter;
    }

    /// <summary>
    /// Attempts to retrieve the cached parameter data view associated with the specified cache key.
    /// </summary>
    /// <remarks>This method validates the cache key before attempting to retrieve the parameter data. If the
    /// cache key is invalid or does not correspond to a parameter, the method returns <see langword="false"/> and sets
    /// <paramref name="parameterDataView"/> to <see langword="null"/>.
    /// <para/>The <paramref name="cacheKey"/> is considered invalid if the referenced cache entry was GC collected due to its associated assembly had been unloaded.
    /// <para/>The cache is designed to not return any real cache entry. Instead interaction is represented by a view.
    /// <br/>This is to prevent potential memory leaks in scenarios where the caller holds on to the returned cache entry for an extended period of time (e.g. via static variables) while the underlying cache entry may need to be collected (e.g. in ALC unloading scenarios).
    /// </remarks>
    /// <param name="cacheKey">The <see cref="SymbolReflectionInfoCacheKey"/> key that identifies the parameter's cache entry. 
    /// <br/>Must not be <see langword="default"/> and must have a symbol kind of <see cref="SymbolKind.Parameter"/>.</param>
    /// <param name="parameterDataView">When this method returns <see langword="true"/>, contains the parameter data view of type <see cref="IParameterDataView"/> associated with the specified
    /// cache key; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
    /// <returns><see langword="true"/> if the parameter cache entry is found and the parameter data view is retrieved; otherwise,
    /// <see langword="false"/> when the underlying cache entry was collected or the cache key is invalid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cacheKey"/> is <see langword="default"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="cacheKey"/> does not have a symbol kind of <see cref="SymbolKind.Parameter"/>.</exception>
    public static bool TryGetParameterCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out IParameterDataView? parameterDataView)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.Parameter],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

        parameterDataView = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing parameter data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        if (TryGetParameterDataCacheEntryInternal(internalCacheKey, out ParameterData parameterData))
        {
            parameterDataView = parameterData.View;
            return true;
        }

        return false;
    }

    private static bool TryGetParameterDataCacheEntry(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out ParameterData parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(internalCacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            internalCacheKey.SymbolKind,
            [SymbolKind.Parameter],
            nameof(internalCacheKey),
             $"The symbol kind '{internalCacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

        return TryGetParameterDataCacheEntryInternal(internalCacheKey, out parameterData);
    }

    private static bool TryGetParameterDataCacheEntry(SymbolReflectionInfoCacheKey cacheKey, out ParameterData? parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKey);
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            cacheKey.SymbolKind,
            [SymbolKind.Parameter],
            nameof(cacheKey),
             $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

        parameterData = null;
        if (!s_publicCacheKeyMap.TryGetValue(cacheKey, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
        {
            // No existing parameter data found for the provided public cache key.
            // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
            // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.
            return false;
        }

        return TryGetParameterDataCacheEntryInternal(internalCacheKey, out parameterData);
    }

    private static bool TryGetParameterDataCacheEntryInternal(SymbolReflectionInfoCacheKeyInternal internalCacheKey, out ParameterData parameterData)
    {
        parameterData = null!;
        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(internalCacheKey, out SymbolInfoData? existingEntry) && existingEntry is ParameterData existingParameterData)
        {
            parameterData = existingParameterData;

            return true;
        }

        // No existing parameter data found for the provided public cache key.
        // Since keys can't be created via constructor by the client, the only reason for an invalid public key is
        // that the cached entry had been evicted e.g. in the course of an assembly load context unloading event.

        return false;
    }
    #endregion Parameter Cache API

    public static bool TryGetSymbolInfoDataCacheEntry<TEntry>(SymbolReflectionInfoCacheKeyInternal key, out TEntry? entry)
      where TEntry : SymbolInfoData
    {
        entry = null;

        if (SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(key, out SymbolInfoData? symbolInfoData))
        {
            entry = (TEntry)symbolInfoData;
        }

        return entry != null;
    }

    /// <summary>
    /// Normalizes a cache key for a symbol, returning a canonical key for anonymous symbols.
    /// </summary>
    /// <remarks>Normalization ensures that anonymous symbols are consistently mapped to a canonical
    /// cache key, enabling reliable symbol identification and lookup. For non-anonymous symbols, the original key
    /// is returned unchanged.
    /// <para>Normalization also ensures that the related <see cref="SymbolInfoData"/> object is created and cached as its instance is required to derive the well-known key.</para></remarks>
    /// <param name="anonymousCacheKey">The cache key representing the symbol to normalize. If the key refers to an anonymous symbol, it will be
    /// mapped to a canonical equivalent.</param>
    /// <returns>A normalized cache key that uniquely identifies the symbol. If the input key is already canonical, the same
    /// key is returned.</returns>
    /// <exception cref="NotSupportedException">Thrown if the symbol kind of <paramref name="anonymousCacheKey"/> is not supported for normalization.</exception>
    /// <exception cref="InvalidReflectionCacheKeyException">Thrown if key's integrity is invalid as it contains information that makes symbol lookup impossible.</exception>
    public static SymbolReflectionInfoCacheKeyInternal NormalizeKey(AnonymousSymbolDescriptorContainer anonymousCacheKey)
    {
        SymbolReflectionInfoCacheKeyInternal normalizedCacheKey = SymbolReflectionInfoCache.s_normalizedAnonymousCacheKeyMap.GetOrAdd(anonymousCacheKey, anonymousCacheKey =>
        {
            SymbolReflectionInfoCacheKeyInternal normalizedCacheKey;
            switch (anonymousCacheKey.SymbolKind)
            {
                case SymbolKind.MemberEvent:
                    EventData eventData = CreateEventData(anonymousCacheKey);
                    normalizedCacheKey = eventData.CacheKey;
                    break;
                case SymbolKind.MemberMethod:
                    MethodData methodData = CreateMethodData(anonymousCacheKey);
                    normalizedCacheKey = methodData.CacheKey;
                    break;
                case SymbolKind.MemberConstructor:
                    ConstructorData constructorData = CreateConstructorData(anonymousCacheKey);
                    normalizedCacheKey = constructorData.CacheKey;
                    break;
                case SymbolKind.MemberField:
                    FieldData fieldData = CreateFieldData(anonymousCacheKey);
                    normalizedCacheKey = fieldData.CacheKey;
                    break;
                case SymbolKind.MemberProperty:
                    PropertyData propertyData = CreatePropertyData(anonymousCacheKey);
                    normalizedCacheKey = propertyData.CacheKey;
                    break;
                case SymbolKind.Type:
                    TypeData typeData = CreateTypeData(anonymousCacheKey);
                    normalizedCacheKey = typeData.CacheKey;
                    break;
                case SymbolKind.Parameter:
                    ParameterData parameterData = CreateParameterData(anonymousCacheKey);
                    normalizedCacheKey = parameterData.CacheKey;
                    break;
                default:
                    throw new NotSupportedException($"The symbol kind '{anonymousCacheKey.SymbolKind}' is not supported for normalization.");
            }

            _ = SymbolReflectionInfoCache.s_normalizedAnonymousCacheKeyMap.TryAdd(anonymousCacheKey, normalizedCacheKey);
            _ = SymbolReflectionInfoCache.s_reverseNormalizedAnonymousCacheKeyMap.TryAdd(normalizedCacheKey, anonymousCacheKey);

            return normalizedCacheKey;
        });

        return normalizedCacheKey;
    }

    private static TypeData CreateTypeData(SymbolReflectionInfoCacheKeyInternal anonymousCacheKey)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            anonymousCacheKey.SymbolKind,
            [SymbolKind.Type],
            nameof(anonymousCacheKey),
             $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a type symbol.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            anonymousCacheKey.IsAnonymousSymbolKey,
            nameof(anonymousCacheKey),
            "The provided cache key is not anonymous. This method only supports creating parameter existingTypeData for anonymous parameter keys.");

        ArgumentNullExceptionAdvanced.ThrowIfDefault(
            anonymousCacheKey.SymbolTypeHandle,
            nameof(anonymousCacheKey.SymbolTypeHandle),
            $"The value 'default' is not a valid value for the key's '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.SymbolTypeHandle)}' property. The property must reference a valid type handle.");

        Type? type = Type.GetTypeFromHandle(anonymousCacheKey.SymbolTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.SymbolTypeHandle)}' does not contain a valid handle for the type.");

        return SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
    }

    private static PropertyData CreatePropertyData(SymbolReflectionInfoCacheKeyInternal anonymousCacheKey)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            anonymousCacheKey.SymbolKind,
            [SymbolKind.MemberProperty],
            nameof(anonymousCacheKey),
             $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a property symbol.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            anonymousCacheKey.IsAnonymousSymbolKey,
            nameof(anonymousCacheKey),
            "The provided cache key is not anonymous. This method only supports creating parameter existingTypeData for anonymous parameter keys.");

        ArgumentNullExceptionAdvanced.ThrowIfDefault(
            anonymousCacheKey.DeclaringTypeHandle,
            nameof(anonymousCacheKey.DeclaringTypeHandle),
            SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

        Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");

        TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(declaringType);

        // Find the property by name and parameter types (for indexers)
        if (!string.IsNullOrWhiteSpace(anonymousCacheKey.SymbolName)
            && declaringTypeData.TryGetPropertyByName(anonymousCacheKey.SymbolName, out PropertyData? propertyData))
        {
            return propertyData!;
        }

        if (anonymousCacheKey.ParameterList.HasItems
            && declaringTypeData.TryGetIndexerPropertyByParameterList(anonymousCacheKey.ParameterList, anonymousCacheKey.IndexerPropertyAccessor, out propertyData))
        {
            return propertyData!;
        }
        else if (anonymousCacheKey.MethodParameterInfoList.HasItems
            && declaringTypeData.TryGetIndexerPropertyByParameterList(anonymousCacheKey.MethodParameterInfoList, anonymousCacheKey.IndexerPropertyAccessor, out propertyData))
        {
            return propertyData!;
        }

        throw new InvalidReflectionCacheKeyException();
    }

    private static ConstructorData CreateConstructorData(SymbolReflectionInfoCacheKeyInternal anonymousCacheKey)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            anonymousCacheKey.SymbolKind,
            [SymbolKind.MemberConstructor],
            nameof(anonymousCacheKey),
             $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            anonymousCacheKey.IsAnonymousSymbolKey,
            nameof(anonymousCacheKey),
            "The provided cache key is not anonymous. This method only supports creating parameter existingTypeData for anonymous parameter keys.");

        ConstructorInfo? constructorInfo = null;
        if (anonymousCacheKey.MethodHandle != default)
        {
            constructorInfo = MethodBase.GetMethodFromHandle(anonymousCacheKey.MethodHandle) as ConstructorInfo ?? throw new InvalidReflectionCacheKeyException("Constructor info could not be retrieved from the provided method handle.");

            return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
        }

        ArgumentNullExceptionAdvanced.ThrowIfDefault(
            anonymousCacheKey.DeclaringTypeHandle,
            nameof(anonymousCacheKey.DeclaringTypeHandle),
            SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

        Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");

        TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(declaringType);

        if (anonymousCacheKey.ParameterList.HasItems
            && declaringTypeData.TryGetConstructorByParameterList(anonymousCacheKey.ParameterList, out ConstructorData? constructorData))
        {
            return constructorData!;
        }
        else if (anonymousCacheKey.MethodParameterInfoList.HasItems
            && declaringTypeData.TryGetConstructorByParameterList(anonymousCacheKey.MethodParameterInfoList, out constructorData))
        {
            return constructorData!;
        }

        throw new InvalidReflectionCacheKeyException();
    }

    private static FieldData CreateFieldData(SymbolReflectionInfoCacheKeyInternal anonymousCacheKey)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            anonymousCacheKey.SymbolKind
            , [SymbolKind.MemberField],
            nameof(anonymousCacheKey),
            $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a field symbol.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            anonymousCacheKey.IsAnonymousSymbolKey,
            nameof(anonymousCacheKey),
            "The provided cache key is not anonymous. This method only supports creating parameter existingTypeData for anonymous parameter keys.");

        FieldInfo? fieldInfo;
        if (anonymousCacheKey.FieldHandle != default)
        {
            fieldInfo = FieldInfo.GetFieldFromHandle(anonymousCacheKey.FieldHandle);
            return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);
        }

        ArgumentNullExceptionAdvanced.ThrowIfDefault(
            anonymousCacheKey.DeclaringTypeHandle,
            nameof(anonymousCacheKey.DeclaringTypeHandle),
            SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

        Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");

        TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(declaringType);

        return declaringTypeData.TryGetFieldByName(anonymousCacheKey.SymbolName, out FieldData? fieldData)
            ? fieldData!
            : throw new InvalidReflectionCacheKeyException();
    }

    private static MethodData CreateMethodData(SymbolReflectionInfoCacheKeyInternal anonymousCacheKey)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            anonymousCacheKey.SymbolKind,
            [SymbolKind.MemberMethod],
            nameof(anonymousCacheKey),
            $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a method symbol.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            anonymousCacheKey.IsAnonymousSymbolKey,
            nameof(anonymousCacheKey),
            "The provided cache key is not anonymous. This method only supports creating parameter existingTypeData for anonymous parameter keys.");

        MethodInfo? methodInfo = null;
        if (anonymousCacheKey.MethodHandle != default)
        {
            methodInfo = MethodBase.GetMethodFromHandle(anonymousCacheKey.MethodHandle) as MethodInfo ?? throw new InvalidReflectionCacheKeyException("Method info could not be retrieved from the provided method handle.");

            MethodData method = SymbolReflectionInfoCache.GetOrCreateEntryInternal(methodInfo);
            return method;
        }

        ArgumentNullExceptionAdvanced.ThrowIfDefault(
            anonymousCacheKey.DeclaringTypeHandle,
            nameof(anonymousCacheKey.DeclaringTypeHandle),
            SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

        Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");

        TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(declaringType);

        if (anonymousCacheKey.ParameterList.HasItems
            && declaringTypeData.TryGetMethod(anonymousCacheKey.SymbolName, anonymousCacheKey.GenericParameterList, anonymousCacheKey.ParameterList, out MethodData? methodData))
        {
            return methodData!;
        }
        else if (anonymousCacheKey.MethodParameterInfoList.HasItems
            && declaringTypeData.TryGetMethod(anonymousCacheKey.SymbolName, anonymousCacheKey.GenericParameterList, anonymousCacheKey.MethodParameterInfoList, out methodData))
        {
            return methodData!;
        }

        throw new InvalidReflectionCacheKeyException();
    }

    private static EventData CreateEventData(AnonymousEventDescriptor eventDescriptor)
    {
        RuntimeTypeHandle declaringTypeHandle = eventDescriptor.ImplementingTypeHandle;
        Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The property '{nameof(AnonymousEventDescriptor)}.{nameof(AnonymousEventDescriptor.ImplementingTypeHandle)}' does not return a valid handle for the declaring type.");

        TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(declaringType);
        EventData eventData = eventDescriptor.IsExplicitInterfaceImplementation
            ? declaringTypeData.TryGetExplicitEvent(eventDescriptor.DeclaringInterfaceTypeHandle, eventDescriptor.EventName, out EventData? explicitMemberEventData)
                ? explicitMemberEventData!
                : throw new InvalidReflectionCacheKeyException()
            : declaringTypeData.TryGetEventByName(eventDescriptor.EventName, out EventData? memberEventData)
                ? memberEventData!
                : throw new InvalidReflectionCacheKeyException();
        return eventData;
    }

    private static ParameterData CreateParameterData(SymbolReflectionInfoCacheKeyInternal anonymousCacheKey)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
            anonymousCacheKey.SymbolKind,
            [SymbolKind.Parameter],
            nameof(anonymousCacheKey),
            $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            anonymousCacheKey.IsAnonymousSymbolKey,
            nameof(anonymousCacheKey),
            "The provided cache key is not anonymous. This method only supports creating parameter existingTypeData for anonymous parameter keys.");

        WellKnownParameterDescriptor parameterDescriptor = anonymousCacheKey.ParameterDescriptor;
        ParameterMemberDescriptor declaringMemberDescriptor = anonymousCacheKey.ParameterMemberDescriptor;

        ParameterData? parameterDataCandidate = null;

        // REVIEW::If-statement order matters here. Order from most specific to least specific i.e. best lookup performance to worst performance.
        // Fastest path: member is explicitly identified by method handle...
        if (declaringMemberDescriptor.MemberHandle != default)
        {
            MethodBase? methodBase = MethodBase.GetMethodFromHandle(declaringMemberDescriptor.MemberHandle) ?? throw new InvalidReflectionCacheKeyException();

            if (methodBase is ConstructorInfo constructorInfo)
            {
                ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
                if (TryFindParameterCandidate(parameterDescriptor, constructorData.Parameters, out parameterDataCandidate, out InvalidReflectionCacheKeyException? exception))
                {
                    return parameterDataCandidate!;
                }
                else
                {
                    throw exception ?? new InvalidReflectionCacheKeyException();
                }
            }
            else
            {
                var methodInfo = (MethodInfo)methodBase;
                MethodData methodData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(methodInfo);
                if (TryFindParameterCandidate(parameterDescriptor, methodData.Parameters, out parameterDataCandidate, out InvalidReflectionCacheKeyException? exception))
                {
                    return parameterDataCandidate!;
                }
                else
                {
                    throw exception ?? new InvalidReflectionCacheKeyException();
                }
            }
        }

        ArgumentNullExceptionAdvanced.ThrowIfDefault(
            anonymousCacheKey.DeclaringTypeHandle,
            nameof(anonymousCacheKey.DeclaringTypeHandle),
            SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

        Type? declaringType = Type.GetTypeFromHandle(declaringMemberDescriptor.DeclaringTypeHandle) ?? throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKeyInternal)}.{nameof(SymbolReflectionInfoCacheKeyInternal.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");

        TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(declaringType);

        // Before enumerating the members of the declaring type, check for special types that have known parameter members
        // like delegates and their invoker methods.
        if (declaringTypeData.IsDelegate)
        {
            MethodData methodData = declaringTypeData.DelegateInvokeMethodData;
            if (TryFindParameterCandidate(parameterDescriptor, methodData.Parameters, out parameterDataCandidate, out InvalidReflectionCacheKeyException? exception))
            {
                return parameterDataCandidate!;
            }
            else
            {
                throw exception ?? new InvalidReflectionCacheKeyException();
            }
        }

        /* No fast lookup possible: need to search amongst all parameterizable members of the provided declaring type.
         * However, if caller has provided enough information about the member that declares the parameter, we still can optimize the search.
         * To ensure correctness, we always need to enumerate all candidates to be able to detect ambiguities.
         * Since ambiguities are not allowed we usually throw to signal that the provided key is too weak.
         * 
         * The worst case is when ParameterizedMemberKind is Undefined.
         * In this case we need to enumerate all parameterizable members - even when a match was found: Multiple matches need to be detected.
         */

        bool isDeclaringMemberAmbiguityExpected = !declaringMemberDescriptor.HasParameterizedMemberKind;
        bool isCandidateAmbiguous = false;

        if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberMethod or ParameterizedSymbolKind.Undefined)
        {
            IEnumerable<MethodData> methodCandidates;

            // Try to limit the candidate set if possible. Method name is the best filter we have.
            // Still it probably yields multiple candidates due to potential method overloads.
            if (declaringMemberDescriptor.HasDeclaringMemberName)
            {
                methodCandidates = declaringTypeData.TryGetMethodByName(declaringMemberDescriptor.DeclaringMemberName, out MethodList? methods) && methods is not null
                    ? methods
                    : throw new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided parameter member name '{declaringMemberDescriptor.DeclaringMemberName}' does not map to any method on the provided declaring type '{declaringTypeData.FullyQualifiedSignature}'.");
            }
            else
            {
                methodCandidates = declaringTypeData.EnumerateMethods();
            }

            if (TryFindParameterCandidateInMethods(parameterDescriptor, declaringMemberDescriptor, methodCandidates, out parameterDataCandidate))
            {
                // Early out if we have a definitive candidate and no declaring member ambiguity is expected.
                // Otherwise we need to continue searching other member kinds to be able to detect ambiguities.
                if (!isDeclaringMemberAmbiguityExpected)
                {
                    return parameterDataCandidate ?? throw new InvalidReflectionCacheKeyException("No valid parameter candidate found.");
                }
            }
        }

        if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberConstructor or ParameterizedSymbolKind.Undefined)
        {
            IEnumerable<ConstructorData> constructorCandidates = declaringTypeData.EnumerateConstructors();

            // We always enumerate the full list of candidates to be able to detect and flag ambiguities.     
            int constructorCandidateCount = 0;
            foreach (ConstructorData constructorData in constructorCandidates)
            {
                if (declaringMemberDescriptor.MemberParameterCount != constructorData.Parameters.Count)
                {
                    continue;
                }

                // Ok, current method is a candidate. Now try to find the parameter amongst its parameters.
                if (TryFindParameterCandidate(parameterDescriptor, constructorData.Parameters, out parameterDataCandidate, out _))
                {
                    /* Method match found */

                    constructorCandidateCount++;

                    // Only a single method match is allowed. Otherwise the key and parameter association is ambiguous.
                    isCandidateAmbiguous = constructorCandidateCount > 1;

                    ThrowIfParameterCandidateIsAmbiguous(isCandidateAmbiguous);
                }
            }

            // Early out if we have a definitive candidate and no declaring member ambiguity is expected.
            if (!isDeclaringMemberAmbiguityExpected)
            {
                return parameterDataCandidate ?? throw new InvalidReflectionCacheKeyException("No valid parameter candidate found.");
            }
        }

        if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberNormalPropertySet
            or ParameterizedSymbolKind.MemberIndexerPropertyGet
            or ParameterizedSymbolKind.MemberIndexerPropertySet
            or ParameterizedSymbolKind.MemberIndexerPropertyGetOrSet
            or ParameterizedSymbolKind.Undefined)
        {

            // Try to limit the candidate set if possible. Property name is the best filter for properties we have
            // Otherwise property type will potentially yield multiple property candidates.
            if (declaringMemberDescriptor.HasDeclaringMemberName)
            {
                if (!declaringTypeData.TryGetPropertyByName(declaringMemberDescriptor.DeclaringMemberName, out PropertyData? propertyCandidate) || propertyCandidate is null)
                {
                    throw new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided parameter member name '{declaringMemberDescriptor.DeclaringMemberName}' does not map to any property on the provided declaring type '{declaringTypeData.FullyQualifiedSignature}'.");
                }

                if (TryFindParameterInProperty(parameterDescriptor, declaringMemberDescriptor, propertyCandidate, out parameterDataCandidate, out InvalidReflectionCacheKeyException exception))
                {
                    return parameterDataCandidate!;
                }
                else
                {
                    throw exception ?? throw new InvalidReflectionCacheKeyException();
                }
            }
            else if (declaringMemberDescriptor.HasMemberTypeHandle)
            {
                IEnumerable<PropertyData> propertyCandidates = declaringTypeData.EnumerateProperties()
                    .Where(propertyData => propertyData.PropertyTypeData.Handle.Equals(declaringMemberDescriptor.MemberTypeHandle));

                // We always enumerate the full list of candidates to be able to detect and flag ambiguities.    
                foreach (PropertyData propertyCandidate in propertyCandidates)
                {
                    if (TryFindParameterInProperty(parameterDescriptor, declaringMemberDescriptor, propertyCandidate, out parameterDataCandidate, out _))
                    {
                        return parameterDataCandidate!;
                    }
                }

                throw new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided property types don't map to a valid property declared on the provided declaring type '{declaringTypeData.FullyQualifiedSignature}'.");
            }
            else
            {
                IEnumerable<PropertyData> propertyCandidates = declaringTypeData.EnumerateProperties();

                // We always enumerate the full list of candidates to be able to detect and flag ambiguities.    
                foreach (PropertyData propertyCandidate in propertyCandidates)
                {
                    if (TryFindParameterInProperty(parameterDescriptor, declaringMemberDescriptor, propertyCandidate, out parameterDataCandidate, out _))
                    {
                        return parameterDataCandidate!;
                    }
                }

                throw new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided property types don't map to a valid property declared on the provided declaring type '{declaringTypeData.FullyQualifiedSignature}'.");
            }
        }

        throw new InvalidReflectionCacheKeyException();
    }

    private static bool TryFindParameterInProperty(WellKnownParameterDescriptor parameterDescriptor, ParameterMemberDescriptor declaringMemberDescriptor, PropertyData propertyCandidate, out ParameterData? parameterDataCandidate, out InvalidReflectionCacheKeyException? exception)
    {
        parameterDataCandidate = null;
        exception = null;

        bool isPropertyAccessorSetterRequested = declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberNormalPropertySet
            or ParameterizedSymbolKind.MemberIndexerPropertySet
            or ParameterizedSymbolKind.MemberIndexerPropertyGetOrSet;
        bool isIndexerPropertyGetterRequested = declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberIndexerPropertyGet
            or ParameterizedSymbolKind.MemberIndexerPropertyGetOrSet;

        // In case 'ParameterizedMemberKind' is 'MemberIndexerPropertyGetOrSet' give the getter precedence over the setter.
        if (isIndexerPropertyGetterRequested)
        {
            if (!propertyCandidate.IsIndexer)
            {
                exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property specifies '{declaringMemberDescriptor.ParameterizedMemberKind}' for an indexer getter but the property is not an indexer.");

                return false;
            }

            if (propertyCandidate.CanRead)
            {
                MethodData propertyAccessorCandidate = propertyCandidate.PropertyGetMethodData;
                if (TryFindParameterCandidateInMethods(parameterDescriptor, declaringMemberDescriptor, [propertyAccessorCandidate], out parameterDataCandidate))
                {
                    return true;
                }
                else
                {
                    exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' does not specify an accessor that matches the provided parameter information.");

                    return false;
                }
            }
            else
            {
                exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property specifies '{declaringMemberDescriptor.ParameterizedMemberKind}' for a getter but the property is write-only.");

                return false;
            }
        }
        else if (isPropertyAccessorSetterRequested)
        {
            if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberIndexerPropertySet or ParameterizedSymbolKind.MemberIndexerPropertyGetOrSet
                && !propertyCandidate.IsIndexer)
            {
                exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' specifies '{declaringMemberDescriptor.ParameterizedMemberKind}' for an indexer setter but the property is not an indexer.");

                return false;
            }

            if (propertyCandidate.CanWrite)
            {
                MethodData propertyAccessorCandidate = propertyCandidate.PropertySetMethodData;
                if (TryFindParameterCandidateInMethods(parameterDescriptor, declaringMemberDescriptor, [propertyAccessorCandidate], out parameterDataCandidate))
                {
                    return true;
                }
                else
                {
                    exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' does not specify an accessor that matches the provided parameter information.");

                    return false;
                }
            }
            else
            {
                exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' specifies '{declaringMemberDescriptor.ParameterizedMemberKind}' for a setter but the property is read-only.");

                return false;
            }
        }
        else // Accessor is undefined - need to search all accessor methods
        {
            MethodData propertyAccessorCandidate;

            // Simplest case: property is not an indexer. In this case only the set accessor is parameterized.
            if (!propertyCandidate.IsIndexer)
            {
                if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberIndexerPropertyGet)
                {
                    exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' specifies '{declaringMemberDescriptor.ParameterizedMemberKind}' for an indexer getter but the property is not an indexer.");
                    return false;
                }

                if (propertyCandidate.CanWrite)
                {
                    propertyAccessorCandidate = propertyCandidate.PropertySetMethodData;
                    if (TryFindParameterCandidateInMethods(parameterDescriptor, declaringMemberDescriptor, [propertyAccessorCandidate], out parameterDataCandidate))
                    {
                        return true;
                    }
                    else
                    {
                        exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' does not specify an accessor that matches the provided parameter information.");

                        return false;
                    }
                }
                else
                {
                    exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' does not specify the accessor kind but the property is read-only and has no parameterized accessor.");

                    return false;
                }
            }
            else // Property is an indexer
            {
                int propertyAccessorCount = 0;

                // Property is an indexer - both accessors are parameterized.
                // However, getter and setter share the same parameter list - except the "value" parameter for the setter.
                // If the searched parameter is not the setter's "value" parameter the parameter association is ambiguous.
                // We should not support this case and instead ask the caller to provide more specific key information.
                if (propertyCandidate.CanRead)
                {
                    propertyAccessorCandidate = propertyCandidate.PropertyGetMethodData;
                    if (TryFindParameterCandidateInMethods(parameterDescriptor, declaringMemberDescriptor, [propertyAccessorCandidate], out parameterDataCandidate))
                    {
                        propertyAccessorCount++;
                    }
                }

                // Early out: if we already have found a matching accessor and the requested kind is 'GetOrSet' we can return immediately
                // since we define a precedence of getter over setter..
                if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberIndexerPropertyGetOrSet
                    && propertyAccessorCount == 1)
                {
                    return true;
                }

                if (propertyCandidate.CanWrite)
                {
                    propertyAccessorCandidate = propertyCandidate.PropertyGetMethodData;
                    if (TryFindParameterCandidateInMethods(parameterDescriptor, declaringMemberDescriptor, [propertyAccessorCandidate], out parameterDataCandidate))
                    {
                        propertyAccessorCount++;
                    }
                }

                bool isPropertyAccessorAmbiguous = propertyAccessorCount > 1;
                if (isPropertyAccessorAmbiguous)
                {
                    exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided declaring member information for the property '{propertyCandidate.FullyQualifiedSignature}' specifies multiple accessors that match the provided parameter information. Please be more specific which accessor should be preferred.");

                    return false;
                }

                return propertyAccessorCount == 1;
            }
        }
    }

    private static bool TryFindParameterCandidateInMethods(WellKnownParameterDescriptor parameterDescriptor, ParameterMemberDescriptor declaringMemberDescriptor, IEnumerable<MethodData> methodCandidates, out ParameterData? parameterDataCandidate)
    {
        parameterDataCandidate = null;

        // We always enumerate the full list of candidates to be able to detect and flag ambiguities.
        bool isCandidateAmbiguous;
        int methodCandidateCount = 0;
        foreach (MethodData methodData in methodCandidates)
        {
            if (declaringMemberDescriptor.HasMemberGenericMethodParameters
                && !declaringMemberDescriptor.MemberGenericMethodParameters.SequenceEqual(methodData.GenericMethodParameters))
            {
                continue;
            }

            if (declaringMemberDescriptor.HasMemberParameterCount
                && declaringMemberDescriptor.MemberParameterCount != methodData.Parameters.Count)
            {
                continue;
            }

            // Ok, current method is a candidate. Now try to find the parameter amongst its parameters.
            if (TryFindParameterCandidate(parameterDescriptor, methodData.Parameters, out parameterDataCandidate, out _))
            {
                /* Method match found */

                methodCandidateCount++;

                // Only a single method match is allowed. Otherwise the key and parameter association is ambiguous.
                isCandidateAmbiguous = methodCandidateCount > 1;

                ThrowIfParameterCandidateIsAmbiguous(isCandidateAmbiguous);
            }
        }

        return parameterDataCandidate is not null;
    }

    private static bool TryFindParameterCandidate(WellKnownParameterDescriptor parameterDescriptor, ParameterList parameters, out ParameterData? parameterDataCandidate, out InvalidReflectionCacheKeyException? exception)
    {
        parameterDataCandidate = null;
        exception = null;

        bool isParameterAmbiguityExpected = !(parameterDescriptor.HasParameterKind && (parameterDescriptor.HasParameterTypeHandle || parameterDescriptor.HasParameterName));
        bool isCandidateAmbiguous;

        // Filter ordered from fastest to slowest path to optimize lookup performance.
        IEnumerable<ParameterData> parameterCandidates = null;
        if (parameterDescriptor.HasParameterName)
        {
            if (parameters.TryGetParameterByName(parameterDescriptor.ParameterName, out ParameterData? parameterCandidate) && parameterCandidate is not null)
            {
                parameterCandidates = new[] { parameterCandidate };

                return true;
            }
            else
            {
                // No parameter with matching name found in this parameter set
                exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided parameter name '{parameterDescriptor.ParameterName}' does not match any parameter.");
                return false;
            }
        }
        else if (parameterDescriptor.HasParameterPosition)
        {
            if (parameterDescriptor.ParameterPosition >= parameters.Count)
            {
                exception = new InvalidReflectionCacheKeyException($"The key for the parameter is invalid. The provided parameter position is out of range for the delegate invoke method. Expected position: '0-{parameters.Count - 1}'; Found: '{parameterDescriptor.ParameterPosition}'");
                return false;
            }

            parameterDataCandidate = parameters[parameterDescriptor.ParameterPosition];

            return true;
        }
        else if (parameterDescriptor.HasParameterTypeHandle)
        {
            parameterCandidates = parameters
                .Where(parameterData => parameterData.ParameterTypeData.Handle.Equals(parameterDescriptor.ParameterTypeHandle));
        }
        else
        {
            parameterCandidates = parameters;
        }

        int parameterCandidateCount = 0;
        foreach (ParameterData parameterCandidate in parameterCandidates)
        {
            // Parameter modifier does not match
            if (parameterDescriptor.HasParameterKind
                && parameterCandidate.ParameterKind != parameterDescriptor.ParameterKind)
            {
                continue;
            }

            /* Parameter match found */

            parameterDataCandidate = parameterCandidate;
            parameterCandidateCount++;
            isCandidateAmbiguous = parameterCandidateCount > 1;

            ThrowIfParameterCandidateIsAmbiguous(isCandidateAmbiguous);

            // No early out required.
            // The parameter name is the only unambiguous identifier for a parameter within a method.
            // Therefore, in an unambiguous scenario the 'parameterCandidates' list will only contain one item
            // and the loop will "early out" naturally.
        }

        return parameterDataCandidate is not null;
    }

    private static void ThrowIfParameterCandidateIsAmbiguous(bool isCandidateAmbiguous)
    {
        if (isCandidateAmbiguous)
        {
            throw new AmbiguousMatchException("Multiple symbols were found that match the provided constraints. To eliminate ambiguity, please provide both member name for the method, constructor or indexer property that defines the parameter and the ParameterKind when creating the cache key. For parameters you can also provide a RuntimeMethodHandle from the member that defines the parameter.");
        }
    }

    private static EventData GetOrCreateNormalizedEventDataCacheEntry(AnonymousEventDescriptor eventDescriptor, out SymbolReflectionInfoCacheKeyInternal normalizedCacheKey)
    {
        EventData eventData = CreateEventData(eventDescriptor);
        normalizedCacheKey = eventData.CacheKey;

        _ = SymbolReflectionInfoCache.s_symbolInfoDataCache.TryGetValue(normalizedCacheKey, out _);

        return eventData;
    }

    private static void MonitorAssemblyLoadContextOfType(SymbolReflectionInfoCacheKeyInternal cacheKey, Type type)
    {
        if (!AssemblyLoadContextMonitor.TryStartMonitoringAssembly(type.Assembly, out AssemblyLoadContextMonitor.AssemblyLoadContextInfo assemblyLoadContextInfo))
        {
            throw new InvalidOperationException($"Failed to monitor assembly load context for type '{type.ToFullyQualifiedSignatureName()}' in assembly '{type.Assembly.FullName}'.");
        }

        lock (assemblyLoadContextInfo.SyncLock)
        {
            if (assemblyLoadContextInfo.IsUnloading)
            {
                throw new InvalidOperationException($"The assembly load context for type '{type.ToFullyQualifiedSignatureName()}' in assembly '{type.Assembly.FullName}' is already unloaded.");
            }

            ConcurrentHashSet<SymbolReflectionInfoCacheKeyInternal> cacheKeysOfSameAssembly = SymbolReflectionInfoCache.s_assemblyLoadContextIdToCacheKeyMap.GetOrAdd(assemblyLoadContextInfo, _ => []);
            _ = cacheKeysOfSameAssembly.TryAdd(cacheKey);
        }
    }

    #region AmbiguousIndexerPropertyKey

    private readonly struct AmbiguousIndexerPropertyKey : IEquatable<AmbiguousIndexerPropertyKey>
    {
        public AmbiguousIndexerPropertyKey(int parameterIndex, SymbolReflectionInfoCacheKeyInternal accessorMethodCacheKey)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
            ArgumentNullExceptionAdvanced.ThrowIfDefault(accessorMethodCacheKey);

            ParameterIndex = parameterIndex;
            AccessorMethodCacheKey = accessorMethodCacheKey;
        }

        public int ParameterIndex { get; init; }
        public SymbolReflectionInfoCacheKeyInternal AccessorMethodCacheKey { get; init; }

        public bool Equals(AmbiguousIndexerPropertyKey other)
            => ParameterIndex == other.ParameterIndex
            && AccessorMethodCacheKey.Equals(other.AccessorMethodCacheKey);

        public override int GetHashCode()
            => HashCode.Combine(ParameterIndex, AccessorMethodCacheKey);

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is AmbiguousIndexerPropertyKey other && Equals(other);

        public static bool operator ==(AmbiguousIndexerPropertyKey left, AmbiguousIndexerPropertyKey right)
            => left.Equals(right);

        public static bool operator !=(AmbiguousIndexerPropertyKey left, AmbiguousIndexerPropertyKey right)
            => !(left == right);
    }

    #endregion

    #region SymbolDataViewBase
    internal abstract class SymbolDataViewBase
    {
        private protected SymbolDataViewBase(SymbolReflectionInfoCacheKey cacheKey) => CacheKey = cacheKey;

        public SymbolReflectionInfoCacheKey CacheKey { get; }

        // central helper for "get or throw"
        private protected PropertyData GetPropertyDataOrThrow()
            => TryGetPropertyDataCacheEntry(CacheKey, out PropertyData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException();

        private protected bool TryGetPropertyData(out PropertyData? propertyData)
            => TryGetPropertyDataCacheEntry(CacheKey, out propertyData);

        // central helper for "get or throw"
        private protected TypeData GetTypeDataOrThrow()
            => TryGetTypeDataCacheEntry(CacheKey, out TypeData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException();

        private protected bool TryGetTypeData(out TypeData? typeData)
            => TryGetTypeDataCacheEntry(CacheKey, out typeData);

        // central helper for "get or throw"
        private protected EventData GetEventDataOrThrow()
            => TryGetEventDataCacheEntry(CacheKey, out EventData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException();

        private protected bool TryGetEventData(out EventData? eventData)
            => TryGetEventDataCacheEntry(CacheKey, out eventData);

        // central helper for "get or throw"
        private protected ConstructorData GetConstructorDataOrThrow()
            => TryGetConstructorDataCacheEntry(CacheKey, out ConstructorData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException();

        private protected bool TryGetConstructorData(out ConstructorData? constructorData)
            => TryGetConstructorDataCacheEntry(CacheKey, out constructorData);

        // central helper for "get or throw"
        private protected FieldData GetFieldDataOrThrow()
            => TryGetFieldDataCacheEntry(CacheKey, out FieldData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException();

        private protected bool TryGetFieldData(out FieldData? fieldData)
            => TryGetFieldDataCacheEntry(CacheKey, out fieldData);

        // central helper for "get or throw"
        private protected ParameterData GetParameterDataOrThrow()
            => TryGetParameterDataCacheEntry(CacheKey, out ParameterData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException();

        private protected bool TryGetParameterData(out ParameterData? parameterData)
            => TryGetParameterDataCacheEntry(CacheKey, out parameterData);

        // central helper for "get or throw"
        private protected SymbolInfoData GetSymbolInfoDataOrThrow() => CacheKey.SymbolKind switch
        {
            SymbolKind.MemberProperty => TryGetPropertyDataCacheEntry(CacheKey, out PropertyData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            SymbolKind.Type => TryGetTypeDataCacheEntry(CacheKey, out TypeData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            SymbolKind.MemberEvent => TryGetEventDataCacheEntry(CacheKey, out EventData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            SymbolKind.MemberMethod => TryGetMethodDataCacheEntry(CacheKey, out MethodData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            SymbolKind.MemberConstructor => TryGetConstructorDataCacheEntry(CacheKey, out ConstructorData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            SymbolKind.MemberField => TryGetFieldDataCacheEntry(CacheKey, out FieldData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            SymbolKind.Parameter => TryGetParameterDataCacheEntry(CacheKey, out ParameterData? data)
                ? data!
                : throw new ReflectionCacheEntryAlcNotAvailableException(),
            _ => throw new NotSupportedException($"The specified symbol kind '{CacheKey.SymbolKind}' is not supported.")
        };

        private protected bool TryGetSymbolInfoData(out SymbolInfoData? symbolInfoData) => CacheKey.SymbolKind switch
        {
            SymbolKind.MemberProperty => (symbolInfoData = TryGetPropertyDataCacheEntry(CacheKey, out PropertyData? data) ? data! : null) is not null,
            SymbolKind.Type => (symbolInfoData = TryGetTypeDataCacheEntry(CacheKey, out TypeData? data) ? data! : null) is not null,
            SymbolKind.MemberEvent => (symbolInfoData = TryGetEventDataCacheEntry(CacheKey, out EventData? data) ? data! : null) is not null,
            SymbolKind.MemberMethod => (symbolInfoData = TryGetMethodDataCacheEntry(CacheKey, out MethodData? data) ? data! : null) is not null,
            SymbolKind.MemberConstructor => (symbolInfoData = TryGetConstructorDataCacheEntry(CacheKey, out ConstructorData? data) ? data! : null) is not null,
            SymbolKind.MemberField => (symbolInfoData = TryGetFieldDataCacheEntry(CacheKey, out FieldData? data) ? data! : null) is not null,
            SymbolKind.Parameter => (symbolInfoData = TryGetParameterDataCacheEntry(CacheKey, out ParameterData? data) ? data! : null) is not null,
            _ => throw new NotSupportedException($"The specified symbol kind '{CacheKey.SymbolKind}' is not supported.")
        };
    }
    #endregion SymbolDataViewBase

    #region SymbolInfoDataCacheProvider
    internal abstract class SymbolInfoDataCacheProvider
    {
        protected SymbolReflectionInfoCacheKey GetPublicCacheKey()
        {
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo<SymbolInfoData>(this);
            if (!s_reverseSymbolInfoDataCache.TryGetValue((SymbolInfoData)this, out SymbolReflectionInfoCacheKeyInternal internalCacheKey))
            {
                return s_reversePublicCacheKeyMap[internalCacheKey];
            }

            throw new InvalidOperationException("The cache key for the symbol info data instance could not be found. This indicates a problem with the cache key management in the SymbolReflectionInfoCache and should be reported.");
        }

        protected static PropertyData GetOrCreateCacheEntry(PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(propertyInfo);
            return (PropertyData)SymbolReflectionInfoCache.GetOrCreateEntryInternal(propertyInfo);
        }

        protected static TypeData GetOrCreateCacheEntry(Type type)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(type);
            return SymbolReflectionInfoCache.GetOrCreateEntryInternal(type);
        }

        protected static MethodData GetOrCreateCacheEntry(MethodInfo methodInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);
            return SymbolReflectionInfoCache.GetOrCreateEntryInternal(methodInfo);
        }

        protected static ConstructorData GetOrCreateCacheEntry(ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo);
            return SymbolReflectionInfoCache.GetOrCreateEntryInternal(constructorInfo);
        }

        protected static FieldData GetOrCreateCacheEntry(FieldInfo fieldInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(fieldInfo);
            return SymbolReflectionInfoCache.GetOrCreateEntryInternal(fieldInfo);
        }

        protected static EventData GetOrCreateCacheEntry(EventInfo eventInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(eventInfo);
            return SymbolReflectionInfoCache.GetOrCreateEntryInternal(eventInfo);
        }

        protected static ParameterData GetOrCreateCacheEntry(ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(parameterInfo);
            return SymbolReflectionInfoCache.GetOrCreateEntryInternal(parameterInfo);
        }

        //protected static TSymbolInfoData GetOrCreateCacheEntry<TSymbolInfoData>(SymbolReflectionInfoCacheKeyInternal cacheKeyInternal)
        //    where TSymbolInfoData : SymbolInfoData
        //{
        //    ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKeyInternal);
        //    return typeof(TSymbolInfoData) switch
        //    {
        //        Type type when type == typeof(PropertyData) => SymbolReflectionInfoCache.TryGetPropertyDataCacheEntryInternal(cacheKeyInternal, out PropertyData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        Type type when type == typeof(TypeData) => SymbolReflectionInfoCache.TryGetTypeDataCacheEntryInternal(cacheKeyInternal, out TypeData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        Type type when type == typeof(EventData) => SymbolReflectionInfoCache.TryGetEventDataCacheEntryInternal(cacheKeyInternal, out EventData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        Type type when type == typeof(MethodData) => SymbolReflectionInfoCache.TryGetMethodDataCacheEntryInternal(cacheKeyInternal, out MethodData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        Type type when type == typeof(ConstructorData) => SymbolReflectionInfoCache.TryGetConstructorDataCacheEntryInternal(cacheKeyInternal, out ConstructorData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        Type type when type == typeof(FieldData) => SymbolReflectionInfoCache.TryGetFieldDataCacheEntryInternal(cacheKeyInternal, out FieldData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        Type type when type == typeof(ParameterData) => SymbolReflectionInfoCache.TryGetParameterDataCacheEntryInternal(cacheKeyInternal, out ParameterData? data)
        //            && data is TSymbolInfoData typedData
        //                ? typedData
        //                : throw new ReflectionCacheEntryAlcNotAvailableException(),
        //        _ => throw new NotSupportedException($"The specified symbol info data type '{typeof(TSymbolInfoData).FullName}' is not supported.")
        //    };
        //}
    }
    #endregion SymbolInfoDataCacheProvider

    #region SymbolReflectionInfoCacheKeyInternal
    /// <summary>
    /// Represents a unique cache key for a well-known symbol, such as a type, method, property, event, field, constructor, or
    /// parameter, used in reflection-based symbol lookup and caching scenarios.
    /// </summary>
    /// <remarks>A <see cref="SymbolReflectionInfoCacheKeyInternal"/> encapsulates identifying information for a symbol, supporting
    /// both well-known symbols (with runtime metadata) and anonymous symbols (identified by signature). This struct is
    /// used to efficiently cache and retrieve reflection information for various symbol kinds, including support for
    /// explicit interface implementations and anonymous members. Instances are typically created using the provided
    /// static factory methods, which enforce correct construction for each symbol kind. <see cref="SymbolReflectionInfoCacheKeyInternal"/> is
    /// immutable and can be used as a key in hash-based collections.
    /// </remarks>
    private readonly struct SymbolReflectionInfoCacheKeyInternal : IEquatable<SymbolReflectionInfoCacheKeyInternal>
    {
        /// <summary>
        /// Gets the name of the symbol represented by this instance.
        /// </summary>
        /// <value>The name of the symbol, such as the method name, property name, event name, field name, or type name.</value>
        public readonly string SymbolName { get; }

        /// <summary>
        /// Gets the kind of symbol represented by this instance.
        /// </summary>
        /// <value>The kind of symbol, such as type, method, property, event, field, constructor, or parameter.</value>
        public readonly SymbolKind SymbolKind { get; }

        private readonly WellKnownParameterDescriptor _parameterDescriptor;
        public WellKnownParameterDescriptor ParameterDescriptor => SymbolKind is SymbolKind.Parameter
            ? _parameterDescriptor
            : ThrowInvalidPropertyContextException<WellKnownParameterDescriptor>([SymbolKind.Parameter]);

        private readonly WellKnownPropertyDescriptor _propertyDescriptor;
        public WellKnownPropertyDescriptor PropertyDescriptor => SymbolKind is SymbolKind.MemberProperty
            ? _propertyDescriptor
            : ThrowInvalidPropertyContextException<WellKnownPropertyDescriptor>([SymbolKind.MemberProperty]);

        private readonly WellKnownMethodDescriptor _methodDescriptor;
        public WellKnownMethodDescriptor MethodDescriptor => SymbolKind is SymbolKind.MemberMethod
            ? _methodDescriptor
            : ThrowInvalidPropertyContextException<WellKnownMethodDescriptor>([SymbolKind.MemberMethod]);

        private readonly WellKnownConstructorDescriptor _constructorDescriptor;
        public WellKnownConstructorDescriptor ConstructorDescriptor => SymbolKind is SymbolKind.MemberConstructor
            ? _constructorDescriptor
            : ThrowInvalidPropertyContextException<WellKnownConstructorDescriptor>([SymbolKind.MemberConstructor]);

        private readonly WellKnownTypeDescriptor _typeDescriptor;
        public WellKnownTypeDescriptor TypeDescriptor => SymbolKind is SymbolKind.Type
            ? _typeDescriptor
            : ThrowInvalidPropertyContextException<WellKnownTypeDescriptor>([SymbolKind.Type]);

        private readonly WellKnownFieldDescriptor _fieldDescriptor;
        public WellKnownFieldDescriptor FieldDescriptor => SymbolKind is SymbolKind.MemberField
            ? _fieldDescriptor
            : ThrowInvalidPropertyContextException<WellKnownFieldDescriptor>([SymbolKind.MemberField]);

        private readonly WellKnownEventDescriptor _eventDescriptor;
        public WellKnownEventDescriptor EventDescriptor => SymbolKind is SymbolKind.MemberEvent
            ? _eventDescriptor
            : ThrowInvalidPropertyContextException<WellKnownEventDescriptor>([SymbolKind.MemberEvent]);

        private readonly int _hashCode;

        private SymbolReflectionInfoCacheKeyInternal(
            string name,
            SymbolKind symbolKind,
            WellKnownTypeDescriptor typeDescriptor,
            WellKnownParameterDescriptor parameterDescriptor,
            WellKnownMethodDescriptor methodDescriptor,
            WellKnownConstructorDescriptor constructorDescriptor,
            WellKnownPropertyDescriptor propertyDescriptor,
            WellKnownFieldDescriptor fieldDescriptor,
            WellKnownEventDescriptor eventDescriptor)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));

            SymbolKind = symbolKind;
            _typeDescriptor = typeDescriptor;
            _parameterDescriptor = parameterDescriptor;
            _methodDescriptor = methodDescriptor;
            _constructorDescriptor = constructorDescriptor;
            _propertyDescriptor = propertyDescriptor;
            _fieldDescriptor = fieldDescriptor;
            _eventDescriptor = eventDescriptor;
            SymbolName = name;

            _hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Creates a cache key for an event symbol.
        /// </summary>
        /// <param name="eventDescriptor">The event descriptor.</param>
        /// <returns>The unique cache key for the event symbol.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventDescriptor"/> is <see langword="null"/>.</exception>
        internal static SymbolReflectionInfoCacheKeyInternal CreateForEvent(WellKnownEventDescriptor eventDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(eventDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                eventDescriptor.EventInfo.Name,
                SymbolKind.MemberEvent,
                default,
                default,
                default,
                default,
                default,
                default,
                default);
        }

        /// <summary>
        /// Creates a cache key for a well-known property symbol.
        /// </summary>
        /// <param name="propertyDescriptor">The <see cref="Net.WellKnownPropertyDescriptor"/> that describes a well-known property (which is where the <see cref="PropertyInfo"/> is available) or an anonymous property (which when only signature information is available).</param>
        /// <remarks>This method creates a unique cache key for well-known or anonymous property symbols, including indexer properties.
        /// <para/>For maximum performance and zero ambiguity, always prefer to create property cache keys using well-known <see cref="PropertyInfo"/> instances via the <see cref="Net.WellKnownPropertyDescriptor"/>.</remarks>
        /// <returns>The unique cache key for the well-known property symbol.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="propertyDescriptor"/> or its declaring type is <see langword="default"/>.</item>
        /// </list>
        /// </exception>
        public static SymbolReflectionInfoCacheKeyInternal CreateForProperty(WellKnownPropertyDescriptor propertyDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                propertyDescriptor.PropertyInfo.Name,
                SymbolKind.MemberProperty,
                default,
                default,
                default,
                default,
                propertyDescriptor,
                default,
                default);
        }

        /// <summary>
        /// Creates a cache key for a well-known method symbol.
        /// </summary>
        /// <param name="methodDescriptor">The <see cref="Net.WellKnownMethodDescriptor"/> that describes a well-known method (which is where the <see cref="MethodInfo"/> is available).</param>
        /// <remarks>This method creates a unique cache key for well-known or anonymous method symbols, including regular methods, property accessors, and event accessors.
        /// <para/>For maximum performance and zero ambiguity, always prefer to create method cache keys using well-known <see cref="MethodInfo"/> instances via the <see cref="Net.WellKnownMethodDescriptor"/>.</remarks>
        /// <returns>The unique cache key for the well-known method symbol.</returns>
        internal static SymbolReflectionInfoCacheKeyInternal CreateForMethod(WellKnownMethodDescriptor methodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                methodDescriptor.MethodName,
                SymbolKind.MemberMethod,
                default,
                default,
                methodDescriptor,
                default,
                default,
                default,
                default);
        }

        internal static SymbolReflectionInfoCacheKeyInternal CreateForType(WellKnownTypeDescriptor typeDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(typeDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                typeDescriptor.TypeName,
                SymbolKind.Type,
                typeDescriptor,
                default,
                default,
                default,
                default,
                default,
                default);
        }

        internal static SymbolReflectionInfoCacheKeyInternal CreateForField(WellKnownFieldDescriptor fieldDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(fieldDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                fieldDescriptor.FieldName,
                SymbolKind.MemberField,
                default,
                default,
                default,
                default,
                default,
                fieldDescriptor,
                default);
        }

        /// <summary>
        /// Creates a new cache key for a well-known constructor using the specified <see cref="WellKnownMethodDescriptor"/>.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.
        ///<para/>For maximum performance and zero ambiguity, always prefer to create constructor cache keys using well-known <see cref="ConstructorInfo"/> instances via the <see cref="Net.WellKnownMethodDescriptor"/>.</remarks>
        /// <param name="constructorDescriptor"></param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKeyInternal"/> representing the specified well-known constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorDescriptor"/> is <see langword="default"/>.</exception>
        internal static SymbolReflectionInfoCacheKeyInternal CreateForConstructor(WellKnownConstructorDescriptor constructorDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(constructorDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                string.Empty,
                SymbolKind.MemberConstructor,
                default,
                default,
                default,
                constructorDescriptor,
                default,
                default,
                default);
        }

        internal static SymbolReflectionInfoCacheKeyInternal CreateForParameter(WellKnownParameterDescriptor parameterDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterDescriptor);

            return new SymbolReflectionInfoCacheKeyInternal(
                parameterDescriptor.ParameterName,
                SymbolKind.Parameter,
                default,
                parameterDescriptor,
                default,
                default,
                default,
                default,
                default);
        }

        public override int GetHashCode()
            => _hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                if (_hashCode != 0)
                {
                    return _hashCode;
                }

                var hashCode = new HashCode();
                hashCode.Add(SymbolName);
                hashCode.Add(SymbolKind);
                hashCode.Add(_parameterDescriptor);
                hashCode.Add(_methodDescriptor);
                hashCode.Add(_constructorDescriptor);
                hashCode.Add(_propertyDescriptor);
                hashCode.Add(_fieldDescriptor);
                hashCode.Add(_eventDescriptor);
                hashCode.Add(_typeDescriptor);

                return hashCode.ToHashCode();
            }
        }

        public override bool Equals(object obj) => obj is SymbolReflectionInfoCacheKeyInternal other && Equals(other);

        public bool Equals(SymbolReflectionInfoCacheKeyInternal other) => SymbolName == other.SymbolName
            && SymbolKind == other.SymbolKind
            && _parameterDescriptor == other._parameterDescriptor
            && _methodDescriptor == other._methodDescriptor
            && _constructorDescriptor == other._constructorDescriptor
            && _propertyDescriptor == other._propertyDescriptor
            && _fieldDescriptor == other._fieldDescriptor
            && _eventDescriptor == other._eventDescriptor
            && _typeDescriptor == other._typeDescriptor;

        public static bool operator ==(SymbolReflectionInfoCacheKeyInternal left, SymbolReflectionInfoCacheKeyInternal right) => left.Equals(right);
        public static bool operator !=(SymbolReflectionInfoCacheKeyInternal left, SymbolReflectionInfoCacheKeyInternal right) => !(left == right);

        [DoesNotReturn]
        private TResult ThrowInvalidPropertyContextException<TResult>(ReadOnlySpan<SymbolKind> allowedSymbolKinds, [CallerMemberName] string? propertyName = null)
        {
            ArgumentExceptionAdvanced.ThrowIfTrue(allowedSymbolKinds.IsEmpty, nameof(allowedSymbolKinds), "At least one allowed symbol kind must be provided.");

            string allowedKinds = allowedSymbolKinds.JoinToString(kind => $"{typeof(SymbolKind).FullName}.{kind}", ", ");
            return allowedSymbolKinds.Length > 1
                ? throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns any of the following values: {allowedKinds}.")
                : throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns the value '{allowedKinds[0]}'.");
        }
    }
    #endregion SymbolReflectionInfoCacheKeyInternal
}

