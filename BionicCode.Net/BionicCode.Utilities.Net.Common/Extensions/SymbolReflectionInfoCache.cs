namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Loader;
    using Microsoft.CodeAnalysis;

    internal static class SymbolReflectionInfoCache
    {
        private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKey, SymbolInfoData> SymbolInfoDataCache = new ConcurrentDictionary<SymbolReflectionInfoCacheKey, SymbolInfoData>();
        private static readonly ConcurrentDictionary<SymbolReflectionInfoCacheKey, SymbolReflectionInfoCacheKey> AnonymousSymbolDataCacheKeyMap = new ConcurrentDictionary<SymbolReflectionInfoCacheKey, SymbolReflectionInfoCacheKey>();
        private static readonly ConcurrentDictionary<AmbiguousIndexerPropertyKey, SymbolReflectionInfoCacheKey> IndexerParameterSymbolDataCacheKeyMap = new ConcurrentDictionary<AmbiguousIndexerPropertyKey, SymbolReflectionInfoCacheKey>();
        private const string MemberNotFoundArgumentExceptionMessage = "Unable to find the {0} named '{1}'{2}on the type '{3}'.";
        private const string InvalidDeclaringTypeHandleFoundInKeyExceptionMessage = $"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type.";
        private const string DeclaringTypeHandleInKeyIsDefaultExceptionMessage = $"The value 'default' is not a valid value for the key's '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' property. The property must reference a valid declaring type handle.";

        #region Extension Methods

        /// <summary>
        /// Converts the specified <see cref="Type"/> to a <see cref="TypeData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="type">The type to convert to a <see cref="TypeData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="TypeData"/> instance that describes the specified type. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="TypeData"/> instances for the same type.</remarks>
        public static TypeData ToTypeData(this Type type)
            => GetOrCreateSymbolInfoDataCacheEntry(type);

        /// <summary>
        /// Converts the specified <see cref="MethodInfo"/> to a <see cref="MethodData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="methodInfo">The method to convert to a <see cref="MethodData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="MethodData"/> instance that describes the specified method. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="MethodData"/> instances for the same method.</remarks>
        public static MethodData ToMethodData(this MethodInfo methodInfo)
            => GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);

        /// <summary>
        /// Converts the specified <see cref="ConstructorInfo"/> to a <see cref="ConstructorData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="constructorInfo">The constructor to convert to a <see cref="ConstructorData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="ConstructorData"/> instance that describes the specified constructor. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="ConstructorData"/> instances for the same constructor.</remarks>
        public static ConstructorData ToConstructorData(this ConstructorInfo constructorInfo)
            => GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);

        /// <summary>
        /// Converts the specified <see cref="FieldInfo"/> to a <see cref="FieldData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="fieldInfo">The field to convert to a <see cref="FieldData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="FieldData"/> instance that describes the specified field. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="FieldData"/> instances for the same field.</remarks>
        public static FieldData ToFieldData(this FieldInfo fieldInfo)
            => GetOrCreateSymbolReflectionInfoCacheEntry(fieldInfo);

        /// <summary>
        /// Converts the specified <see cref="PropertyInfo"/> to a <see cref="PropertyData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="propertyInfo">The property to convert to a <see cref="PropertyData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="PropertyData"/> instance that describes the specified property. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="PropertyData"/> instances for the same property.</remarks>
        public static PropertyData ToPropertyData(this PropertyInfo propertyInfo)
            => GetOrCreateSymbolReflectionInfoCacheEntry(propertyInfo);

        /// <summary>
        /// Converts the specified <see cref="EventInfo"/> to a <see cref="EventData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="eventInfo">The event to convert to a <see cref="EventData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="EventData"/> instance that describes the specified event. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="EventData"/> instances for the same event.</remarks>
        public static EventData ToEventData(this EventInfo eventInfo)
            => GetOrCreateSymbolReflectionInfoCacheEntry(eventInfo);

        /// <summary>
        /// Converts the specified <see cref="ParameterInfo"/> to a <see cref="ParameterData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="parameterInfo">The parameter to convert to a <see cref="ParameterData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="ParameterData"/> instance that describes the specified parameter. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="ParameterData"/> instances for the same parameter.</remarks>
        public static ParameterData ToParameterData(this ParameterInfo parameterInfo)
            => GetOrCreateSymbolInfoDataCacheEntry(parameterInfo);

        #endregion Extension Methods

        public static TypeData GetOrCreateSymbolInfoDataCacheEntry(Type type)
        {
            SymbolReflectionInfoCacheKey cacheKey = SymbolReflectionInfoCacheKey.CreateForType(type);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new TypeData(type, cacheKey));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {type.GetType()}");

            return (TypeData)symbolInfoData;
        }

        public static MethodData GetOrCreateSymbolReflectionInfoCacheEntry(MethodInfo methodInfo, bool isExplicitInterfaceImplementation)
        {
            SymbolReflectionInfoCacheKey cacheKey = SymbolReflectionInfoCacheKey.CreateForMethod(methodInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(methodInfo, key, isExplicitInterfaceImplementation));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {methodInfo.GetType()}");

            return (MethodData)symbolInfoData;
        }

        public static ConstructorData GetOrCreateSymbolReflectionInfoCacheEntry(ConstructorInfo constructorInfo)
        {
            SymbolReflectionInfoCacheKey cacheKey = SymbolReflectionInfoCacheKey.CreateForConstructor(constructorInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ConstructorData(constructorInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {constructorInfo.GetType()}");

            return (ConstructorData)symbolInfoData;
        }

        public static FieldData GetOrCreateSymbolReflectionInfoCacheEntry(FieldInfo fieldInfo)
        {
            SymbolReflectionInfoCacheKey cacheKey = SymbolReflectionInfoCacheKey.CreateForField(fieldInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new FieldData(fieldInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {fieldInfo.GetType()}");

            return (FieldData)symbolInfoData;
        }

        public static PropertyData GetOrCreateSymbolReflectionInfoCacheEntry(PropertyInfo propertyInfo, bool isExplicitInterfaceImplementation = false)
        {
            // This call also validates that the 'PropertyInfo.DeclaringType' returns an interface type when 'isExplicitInterfaceImplementation' is true.
            SymbolReflectionInfoCacheKey cacheKey = SymbolReflectionInfoCacheKey.CreateForProperty(propertyInfo, isExplicitInterfaceImplementation);

            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key =>
            {

                return new PropertyData(propertyInfo, key, isExplicitInterfaceImplementation);
            });

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {propertyInfo.GetType()}");

            return (PropertyData)symbolInfoData;
        }

        public static EventData GetOrCreateSymbolReflectionInfoCacheEntry(EventInfo eventInfo)
        {
            SymbolReflectionInfoCacheKey cacheKey = SymbolReflectionInfoCacheKey.CreateForEvent(eventInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new EventData(eventInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {eventInfo.GetType()}");

            return (EventData)symbolInfoData;
        }

        public static ParameterData GetOrCreateSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
        {
            MemberInfo member = parameterInfo.Member;
            SymbolReflectionInfoCacheKey cacheKey;
            // If the 'ParameterInfo.Member' property returns a 'PropertyInfo' then the current parameter 'parameterInfo'
            // was obtained via PropertyInfo.GetIndexParameters method call. As a result, the parameter's association to the property's accessors is ambiguous.
            // We need to normalize it to remove association ambiguity by explicitly associating it with a property's accessor method.
            // We basically replace the current 'GetIndexerParameters()' based 'propertyInfo' argument with a PropertyInfo from an accessor method.
            if (member is PropertyInfo propertyInfo)
            {
                ParameterData normalizedParameterData = ConvertAmbiguousIndexerPropertyParameterToAccessorAssociatedParameter(parameterInfo);

                return normalizedParameterData;
            }
            else
            {
                cacheKey = SymbolReflectionInfoCacheKey.CreateForParameter(parameterInfo);
            }

            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ParameterData(parameterInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {parameterInfo.GetType()}");

            return (ParameterData)symbolInfoData;
        }

        internal static ParameterData ConvertAmbiguousIndexerPropertyParameterToAccessorAssociatedParameter(ParameterInfo parameterInfo)
        {
            ArgumentExceptionAdvanced.ThrowIfFalse(
                parameterInfo.Member is PropertyInfo,
                nameof(parameterInfo),
                $"The provided argument '{nameof(parameterInfo)}' is not ambiguous. The provided parameter info must be ambiguous in that it was obtained via '{typeof(PropertyInfo).ToFullyQualifiedSignatureName()}.{nameof(PropertyInfo.GetIndexParameters)}()' belong to a property to be considered ambiguous.");

            var propertyInfo = parameterInfo.Member as PropertyInfo;
            PropertyData propertyData = GetOrCreateSymbolReflectionInfoCacheEntry(propertyInfo!);

            // By convention, the getter takes precedence over the setter
            MethodData accessorData = propertyData.CanRead
                ? propertyData.PropertyGetMethodData
                : propertyData.PropertySetMethodData;

            SymbolReflectionInfoCacheKey accessorMethodCacheKey = accessorData.CacheKey;
            AmbiguousIndexerPropertyKey ambiguousKey = new AmbiguousIndexerPropertyKey(parameterInfo.Position, accessorMethodCacheKey);
            SymbolReflectionInfoCacheKey normalizedParameterDataCacheKey = SymbolReflectionInfoCache.IndexerParameterSymbolDataCacheKeyMap.GetOrAdd(ambiguousKey,
                key =>
                {
                    var accessorData = (MethodData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(key.AccessorMethodCacheKey, CreateMethodData);
                    ParameterList accessorParameters = accessorData.Parameters;
                    ParameterData parameterData = accessorParameters[key.ParameterIndex];
                    return parameterData.CacheKey;
                });

            return (ParameterData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(normalizedParameterDataCacheKey, CreateParameterData);
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The event data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static EventData GetOrCreateEventDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberEvent],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating an event symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            EventData eventData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<EventData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is EventData existingEventData
                    ? existingEventData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing event data found for the provided non-anonymous cache key.");

            return eventData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The method data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static MethodData GetOrCreateMethodDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberMethod],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a method symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            MethodData methodData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<MethodData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is MethodData existingMethodData
                    ? existingMethodData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing method data found for the provided non-anonymous cache key.");

            return methodData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The field data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static FieldData GetOrCreateFieldDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberField],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a field symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            FieldData fieldData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<FieldData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is FieldData existingFieldData
                    ? existingFieldData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing field data found for the provided non-anonymous cache key.");

            return fieldData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The property data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static PropertyData GetOrCreatePropertyDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberProperty],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a property symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            PropertyData propertyData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<PropertyData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is PropertyData existingPropertyData
                    ? existingPropertyData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing property data found for the provided non-anonymous cache key.");

            return propertyData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The parameter data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static ParameterData GetOrCreateParameterDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.Parameter],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            ParameterData parameterData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<ParameterData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is ParameterData existingParameterData
                    ? existingParameterData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing parameter data found for the provided non-anonymous cache key.");

            return parameterData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The constructor data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static ConstructorData GetOrCreateConstructorDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberConstructor],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            ConstructorData constructorData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<ConstructorData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is ConstructorData existingConstructorData
                    ? existingConstructorData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing constructor data found for the provided non-anonymous cache key.");

            return constructorData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The type data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static TypeData GetOrCreateTypeDataCacheEntry(ref SymbolReflectionInfoCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.Type],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a type symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            TypeData typeData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<TypeData>(ref cacheKey)
                : SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData? existingEntry) && existingEntry is TypeData existingTypeData
                    ? existingTypeData
                    : throw new ArgumentExceptionAdvanced($"Invalid argument '{nameof(cacheKey)}'. No existing type data found for the provided non-anonymous cache key.");

            return typeData;
        }

        public static bool TryGetSymbolInfoDataCacheEntry<TEntry>(SymbolReflectionInfoCacheKey key, out TEntry? entry)
          where TEntry : SymbolInfoData
        {
            entry = null;

            if (SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(key, out SymbolInfoData? symbolInfoData))
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
        public static SymbolReflectionInfoCacheKey NormalizeKey(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            if (!anonymousCacheKey.IsAnonymousSymbolKey)
            {
                return anonymousCacheKey;
            }

            if (SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryGetValue(anonymousCacheKey, out SymbolReflectionInfoCacheKey normalizedCacheKey))
            {
                return normalizedCacheKey;
            }

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

            _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(anonymousCacheKey, normalizedCacheKey);

            return normalizedCacheKey;
        }

        private static TypeData CreateTypeData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                anonymousCacheKey.SymbolKind,
                [SymbolKind.Type],
                nameof(anonymousCacheKey),
                 $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a type symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                anonymousCacheKey.SymbolTypeHandle,
                nameof(anonymousCacheKey.SymbolTypeHandle),
                $"The value 'default' is not a valid value for the key's '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.SymbolTypeHandle)}' property. The property must reference a valid type handle.");

            Type? type = Type.GetTypeFromHandle(anonymousCacheKey.SymbolTypeHandle);
            if (type is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.SymbolTypeHandle)}' does not contain a valid handle for the type.");
            }

            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
        }

        private static PropertyData CreatePropertyData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                anonymousCacheKey.SymbolKind,
                [SymbolKind.MemberProperty],
                nameof(anonymousCacheKey),
                 $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a property symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                anonymousCacheKey.DeclaringTypeHandle,
                nameof(anonymousCacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);

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

        private static ConstructorData CreateConstructorData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                anonymousCacheKey.SymbolKind,
                [SymbolKind.MemberConstructor],
                nameof(anonymousCacheKey),
                 $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

            ConstructorInfo? constructorInfo = null;
            if (anonymousCacheKey.MethodHandle != default)
            {
                constructorInfo = MethodBase.GetMethodFromHandle(anonymousCacheKey.MethodHandle) as ConstructorInfo;
                if (constructorInfo is null)
                {
                    throw new InvalidReflectionCacheKeyException("Constructor info could not be retrieved from the provided method handle.");
                }

                return SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(constructorInfo);
            }

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                anonymousCacheKey.DeclaringTypeHandle,
                nameof(anonymousCacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);

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

        private static FieldData CreateFieldData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                anonymousCacheKey.SymbolKind
                , [SymbolKind.MemberField],
                nameof(anonymousCacheKey),
                $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a field symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

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

            Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);

            return declaringTypeData.TryGetFieldByName(anonymousCacheKey.SymbolName, out FieldData? fieldData)
                ? fieldData!
                : throw new InvalidReflectionCacheKeyException();
        }

        private static MethodData CreateMethodData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                anonymousCacheKey.SymbolKind,
                [SymbolKind.MemberMethod],
                nameof(anonymousCacheKey),
                $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a method symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

            MethodInfo? methodInfo = null;
            if (anonymousCacheKey.MethodHandle != default)
            {
                methodInfo = MethodBase.GetMethodFromHandle(anonymousCacheKey.MethodHandle) as MethodInfo;
                if (methodInfo is null)
                {
                    throw new InvalidReflectionCacheKeyException("Method info could not be retrieved from the provided method handle.");
                }

                MethodData method = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
                return method;
            }

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                anonymousCacheKey.DeclaringTypeHandle,
                nameof(anonymousCacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);

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

        private static EventData CreateEventData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(anonymousCacheKey.SymbolKind,
                [SymbolKind.MemberEvent],
                nameof(anonymousCacheKey),
                $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating an event symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                anonymousCacheKey.DeclaringTypeHandle,
                nameof(anonymousCacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(anonymousCacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);
            EventData eventData = declaringTypeData.TryGetEventByName(anonymousCacheKey.SymbolName, out EventData? existingEventData)
                ? existingEventData!
                : throw new InvalidReflectionCacheKeyException();

            return eventData;
        }

        private static ParameterData CreateParameterData(SymbolReflectionInfoCacheKey anonymousCacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                anonymousCacheKey.SymbolKind,
                [SymbolKind.Parameter],
                nameof(anonymousCacheKey),
                $"The symbol kind '{anonymousCacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

            ArgumentExceptionAdvanced.ThrowIfFalse(
                anonymousCacheKey.IsAnonymousSymbolKey,
                nameof(anonymousCacheKey),
                "The provided cache key is not anonymous. This method only supports creating parameter data for anonymous parameter keys.");

            WellKnownParameterDescriptor parameterDescriptor = anonymousCacheKey.ParameterDescriptor;
            ParameterMemberDescriptor declaringMemberDescriptor = anonymousCacheKey.ParameterMemberDescriptor;

            ParameterData? parameterDataCandidate = null;

            // REVIEW::If-statement order matters here. Order from most specific to least specific i.e. best lookup performance to worst performance.
            // Fastest path: member is explicitly identified by method handle...
            if (declaringMemberDescriptor.MemberHandle != default)
            {
                MethodBase? methodBase = MethodBase.GetMethodFromHandle(declaringMemberDescriptor.MemberHandle);
                if (methodBase is null)
                {
                    throw new InvalidReflectionCacheKeyException();
                }

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
                    MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry(methodInfo);
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

            Type? declaringType = Type.GetTypeFromHandle(declaringMemberDescriptor.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolReflectionInfoCacheKey)}.{nameof(SymbolReflectionInfoCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);

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

        private static TSymbolInfoData GetOrCreateNormalizedSymbolInfoDataCacheEntry<TSymbolInfoData>(ref SymbolReflectionInfoCacheKey cacheKey) where TSymbolInfoData : SymbolInfoData
        {
            TSymbolInfoData result;
            SymbolReflectionInfoCacheKey normalizedCacheKey = SymbolReflectionInfoCache.NormalizeKey(cacheKey);
            _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(normalizedCacheKey, out SymbolInfoData? symbolInfoData);
            result = (TSymbolInfoData)symbolInfoData!;
            cacheKey = normalizedCacheKey;

            return result;
        }

        #region AmbiguousIndexerPropertyKey

        private readonly struct AmbiguousIndexerPropertyKey : IEquatable<AmbiguousIndexerPropertyKey>
        {
            public AmbiguousIndexerPropertyKey(int parameterIndex, SymbolReflectionInfoCacheKey accessorMethodCacheKey)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(parameterIndex);
                ArgumentNullExceptionAdvanced.ThrowIfDefault(accessorMethodCacheKey);

                this.ParameterIndex = parameterIndex;
                this.AccessorMethodCacheKey = accessorMethodCacheKey;
            }

            public int ParameterIndex { get; init; }
            public SymbolReflectionInfoCacheKey AccessorMethodCacheKey { get; init; }

            public bool Equals(AmbiguousIndexerPropertyKey other)
                => this.ParameterIndex == other.ParameterIndex
                && this.AccessorMethodCacheKey.Equals(other.AccessorMethodCacheKey);

            public override int GetHashCode()
                => HashCode.Combine(this.ParameterIndex, this.AccessorMethodCacheKey);

            public override bool Equals([NotNullWhen(true)] object? obj)
                => obj is AmbiguousIndexerPropertyKey other && Equals(other);

            public static bool operator ==(AmbiguousIndexerPropertyKey left, AmbiguousIndexerPropertyKey right)
                => left.Equals(right);

            public static bool operator !=(AmbiguousIndexerPropertyKey left, AmbiguousIndexerPropertyKey right)
                => !(left == right);
        }

        #endregion

        #region AssemblyId

        internal readonly struct AssemblyId : IEquatable<AssemblyId>
        {
            public static readonly AssemblyId Empty = new AssemblyId(Guid.Empty, 0);

            // Keep it small: Guid (16 bytes) + int (4 bytes) = 20 bytes.
            public readonly Guid ModuleVersionId;
            public readonly int InstanceHash;

            public AssemblyId(Guid moduleVersionId, int instanceHash)
            {
                this.ModuleVersionId = moduleVersionId;
                this.InstanceHash = instanceHash;
            }

            public static AssemblyId FromAssembly(Assembly asm)
            {
                ArgumentNullException.ThrowIfNull(asm);

                // ModuleVersionId is available on the manifest module (typical single-module assemblies)
                Guid mvid = asm.ManifestModule.ModuleVersionId;

                // RuntimeHelpers.GetHashCode uses object identity and is stable for the lifetime of the object.
                int instanceHash = RuntimeHelpers.GetHashCode(asm);

                // Optionally strengthen uniqueness by mixing in the load-context identity:
                var alc = AssemblyLoadContext.GetLoadContext(asm);
                if (alc != null)
                {
                    // mix in ALC identity (also object-identity based)
                    unchecked
                    {
                        instanceHash = (instanceHash * 31) + RuntimeHelpers.GetHashCode(alc);
                    }
                }

                return new AssemblyId(mvid, instanceHash);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hasCode = new HashCode();
                    // fold GUID into int then combine
                    int g1 = this.ModuleVersionId.GetHashCode();
                    hasCode.Add(g1);
                    hasCode.Add(this.InstanceHash);

                    return hasCode.ToHashCode();
                }
            }

            public override bool Equals(object obj)
                => obj is AssemblyId other && Equals(other);

            public bool Equals(AssemblyId other)
                => this.ModuleVersionId.Equals(other.ModuleVersionId)
                    && this.InstanceHash == other.InstanceHash;

            public static bool operator ==(AssemblyId a, AssemblyId b) => a.Equals(b);
            public static bool operator !=(AssemblyId a, AssemblyId b) => !a.Equals(b);

            // Optional: compress to a 64-bit value for smaller memory
            public ulong ToUInt64()
            {
                // cheap non-cryptographic fold of GUID bytes + instanceHash -> 64-bit
                Span<byte> buf = stackalloc byte[20]; // 16 + 4
                _ = this.ModuleVersionId.TryWriteBytes(buf);
                buf[16] = (byte)this.InstanceHash;
                buf[17] = (byte)(this.InstanceHash >> 8);
                buf[18] = (byte)(this.InstanceHash >> 16);
                buf[19] = (byte)(this.InstanceHash >> 24);

                // simple FNV or xxhash-style fold; here a basic fold to 64-bit
                ulong h = 1469598103934665603ul;
                foreach (byte b in buf)
                {
                    h = (h ^ b) * 1099511628211ul;
                }

                return h;
            }
        }

        #endregion
    }
}
