namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    internal static class SymbolReflectionInfoCache
    {
        private static readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData> SymbolInfoDataCache = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData>();
        private static readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoDataCacheKey> AnonymousSymbolDataCacheKeyMap = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoDataCacheKey>();
        private static readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoDataCacheKey> IndexerParameterSymbolDataCacheKeyMap = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoDataCacheKey>();
        private const string MemberNotFoundArgumentExceptionMessage = "Unable to find the {0} named '{1}'{2}on the type '{3}'.";
        private const string InvalidDeclaringTypeHandleFoundInKeyExceptionMessage = $"The key's property '{nameof(SymbolInfoDataCacheKey)}.{nameof(SymbolInfoDataCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type.";
        private const string DeclaringTypeHandleInKeyIsDefaultExceptionMessage = $"The value 'default' is not a valid value for the key's '{nameof(SymbolInfoDataCacheKey)}.{nameof(SymbolInfoDataCacheKey.DeclaringTypeHandle)}' property. The property must reference a valid declaring type handle.";

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
            => GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

        /// <summary>
        /// Converts the specified <see cref="ConstructorInfo"/> to a <see cref="ConstructorData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="constructorInfo">The constructor to convert to a <see cref="ConstructorData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="ConstructorData"/> instance that describes the specified constructor. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="ConstructorData"/> instances for the same constructor.</remarks>
        public static ConstructorData ToConstructorData(this ConstructorInfo constructorInfo)
            => GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);

        /// <summary>
        /// Converts the specified <see cref="FieldInfo"/> to a <see cref="FieldData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="fieldInfo">The field to convert to a <see cref="FieldData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="FieldData"/> instance that describes the specified field. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="FieldData"/> instances for the same field.</remarks>
        public static FieldData ToFieldData(this FieldInfo fieldInfo)
            => GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);

        /// <summary>
        /// Converts the specified <see cref="PropertyInfo"/> to a <see cref="PropertyData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="propertyInfo">The property to convert to a <see cref="PropertyData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="PropertyData"/> instance that describes the specified property. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="PropertyData"/> instances for the same property.</remarks>
        public static PropertyData ToPropertyData(this PropertyInfo propertyInfo)
            => GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);

        /// <summary>
        /// Converts the specified <see cref="EventInfo"/> to a <see cref="EventData"/> instance representing its metadata and
        /// characteristics.
        /// </summary>
        /// <param name="eventInfo">The event to convert to a <see cref="EventData"/> instance. Cannot be <see langword="null"/>.</param>
        /// <returns>A <see cref="EventData"/> instance that describes the specified event. The returned instance is cached for future use if it is not already present in the cache, in which case the cached instance will be returned.</returns>
        /// <remarks>The method utilizes a caching mechanism to optimize performance by avoiding redundant creation of <see cref="EventData"/> instances for the same event.</remarks>
        public static EventData ToEventData(this EventInfo eventInfo)
            => GetOrCreateSymbolInfoDataCacheEntry(eventInfo);

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
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForType(type);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new TypeData(type, cacheKey));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {type.GetType()}");

            return (TypeData)symbolInfoData;
        }

        public static MethodData GetOrCreateSymbolInfoDataCacheEntry(MethodInfo methodInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForMethod(methodInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(methodInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {methodInfo.GetType()}");

            return (MethodData)symbolInfoData;
        }

        public static ConstructorData GetOrCreateSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructorInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ConstructorData(constructorInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {constructorInfo.GetType()}");

            return (ConstructorData)symbolInfoData;
        }

        public static FieldData GetOrCreateSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForField(fieldInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new FieldData(fieldInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {fieldInfo.GetType()}");

            return (FieldData)symbolInfoData;
        }

        public static PropertyData GetOrCreateSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForProperty(propertyInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new PropertyData(propertyInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {propertyInfo.GetType()}");

            return (PropertyData)symbolInfoData;
        }

        public static EventData GetOrCreateSymbolInfoDataCacheEntry(EventInfo eventInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new EventData(eventInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {eventInfo.GetType()}");

            return (EventData)symbolInfoData;
        }

        public static ParameterData GetOrCreateSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
        {
            MemberInfo member = parameterInfo.Member;
            SymbolInfoDataCacheKey cacheKey;
            // If the 'ParameterInfo.Member' property returns a 'PropertyInfo' then the current parameter 'parameterInfo'
            // was obtained via PropertyInfo.GetIndexParameters method call. As a result, the parameter's association to the property's accessors is ambiguous.
            // We need to normalize it to remove association ambiguity by explicitly associating it with a property's accessor method.
            // We basically replace the current 'GetIndexerParameters()' based 'propertyInfo' argument with a PropertyInfo from an accessor method.
            if (member is PropertyInfo propertyInfo)
            {
                PropertyData propertyData = propertyInfo.ToPropertyData();
                SymbolInfoDataCacheKey normalizedParameterDataCacheKey = ConvertIndexerPropertyToAccessorAssociatedParameterCacheKey(propertyData, parameterInfo.Position);

                cacheKey = normalizedParameterDataCacheKey;
            }
            else
            {
                cacheKey = SymbolInfoDataCacheKey.CreateForParameter(parameterInfo);
            }

            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ParameterData(parameterInfo, key));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {parameterInfo.GetType()}");

            return (ParameterData)symbolInfoData;
        }

        internal static SymbolInfoDataCacheKey ConvertIndexerPropertyToAccessorAssociatedParameterCacheKey(PropertyData propertyData, int parameterIndex)
        {
            SymbolInfoDataCacheKey normalizedParameterDataCacheKey = SymbolReflectionInfoCache.IndexerParameterSymbolDataCacheKeyMap.GetOrAdd(propertyData.CacheKey,
                key =>
                {
                    // By convention, the getter takes precedence over the setter
                    MethodData accessorMethod = propertyData.CanRead
                            ? propertyData.PropertyGetMethodData
                            : propertyData.PropertySetMethodData;
                    ParameterData parameterData = accessorMethod.Parameters[parameterIndex];
                    return parameterData.CacheKey;
                });

            return normalizedParameterDataCacheKey;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <returns>The event data associated with the cache entry identified by the specified key.</returns>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        public static EventData GetOrCreateEventDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberEvent],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating an event symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            EventData eventData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<EventData>(ref cacheKey)
                : (EventData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateEventData);

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
        public static MethodData GetOrCreateMethodDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberMethod],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a method symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            MethodData methodData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<MethodData>(ref cacheKey)
                : (MethodData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateMethodData);

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
        public static FieldData GetOrCreateFieldDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberField],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a field symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            FieldData fieldData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<FieldData>(ref cacheKey)
                : (FieldData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateFieldData);

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
        public static PropertyData GetOrCreatePropertyDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberProperty],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a property symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            PropertyData propertyData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<PropertyData>(ref cacheKey)
                : (PropertyData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreatePropertyData);

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
        public static ParameterData GetOrCreateParameterDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.Parameter],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            ParameterData parameterData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<ParameterData>(ref cacheKey)
                : (ParameterData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateParameterData);

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
        public static ConstructorData GetOrCreateConstructorDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberConstructor],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            ConstructorData constructorData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<ConstructorData>(ref cacheKey)
                : (ConstructorData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateConstructorData);

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
        public static TypeData GetOrCreateTypeDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.Type],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a type symbol.");

            // Optimization: Avoid normalization for non-anonymous symbols
            TypeData typeData = cacheKey.IsAnonymousSymbolKey
                ? GetOrCreateNormalizedSymbolInfoDataCacheEntry<TypeData>(ref cacheKey)
                : (TypeData)SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateTypeData); // Optimization: Avoid normalization for non-anonymous symbols

            return typeData;
        }

        public static bool TryGetSymbolInfoDataCacheEntry<TEntry>(SymbolInfoDataCacheKey key, out TEntry? entry)
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
        /// <param name="cacheKey">The cache key representing the symbol to normalize. If the key refers to an anonymous symbol, it will be
        /// mapped to a canonical equivalent.</param>
        /// <returns>A normalized cache key that uniquely identifies the symbol. If the input key is already canonical, the same
        /// key is returned.</returns>
        /// <exception cref="NotSupportedException">Thrown if the symbol kind of <paramref name="cacheKey"/> is not supported for normalization.</exception>
        /// <exception cref="InvalidReflectionCacheKeyException">Thrown if key's integrity is invalid as it contains information that makes symbol lookup impossible.</exception>
        public static SymbolInfoDataCacheKey NormalizeKey(SymbolInfoDataCacheKey cacheKey)
        {
            if (!cacheKey.IsAnonymousSymbolKey)
            {
                return cacheKey;
            }

            if (SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryGetValue(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey))
            {
                return normalizedCacheKey;
            }

            switch (cacheKey.SymbolKind)
            {
                case SymbolKind.MemberEvent:
                    EventData eventData = CreateEventData(cacheKey);
                    normalizedCacheKey = eventData.CacheKey;
                    break;
                case SymbolKind.MemberMethod:
                    MethodData methodData = CreateMethodData(cacheKey);
                    normalizedCacheKey = methodData.CacheKey;
                    break;
                case SymbolKind.MemberConstructor:
                    ConstructorData constructorData = CreateConstructorData(cacheKey);
                    normalizedCacheKey = constructorData.CacheKey;
                    break;
                case SymbolKind.MemberField:
                    FieldData fieldData = CreateFieldData(cacheKey);
                    normalizedCacheKey = fieldData.CacheKey;
                    break;
                case SymbolKind.MemberProperty:
                    PropertyData propertyData = CreatePropertyData(cacheKey);
                    normalizedCacheKey = propertyData.CacheKey;
                    break;
                case SymbolKind.Type:
                    TypeData typeData = CreateTypeData(cacheKey);
                    normalizedCacheKey = typeData.CacheKey;
                    break;
                case SymbolKind.Parameter:
                    ParameterData parameterData = CreateParameterData(cacheKey);
                    normalizedCacheKey = parameterData.CacheKey;
                    break;
                default:
                    throw new NotSupportedException($"The symbol kind '{cacheKey.SymbolKind}' is not supported for normalization.");
            }

            _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);

            return normalizedCacheKey;
        }

        private static TypeData CreateTypeData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.Type],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a type symbol.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                cacheKey.SymbolTypeHandle,
                nameof(cacheKey.SymbolTypeHandle),
                $"The value 'default' is not a valid value for the key's '{nameof(SymbolInfoDataCacheKey)}.{nameof(SymbolInfoDataCacheKey.SymbolTypeHandle)}' property. The property must reference a valid type handle.");

            Type? type = Type.GetTypeFromHandle(cacheKey.SymbolTypeHandle);
            if (type is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolInfoDataCacheKey)}.{nameof(SymbolInfoDataCacheKey.SymbolTypeHandle)}' does not contain a valid handle for the type.");
            }

            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(type);
        }

        private static PropertyData CreatePropertyData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberProperty],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a property symbol.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                cacheKey.DeclaringTypeHandle,
                nameof(cacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException(SymbolReflectionInfoCache.InvalidDeclaringTypeHandleFoundInKeyExceptionMessage);
            }

            Type[] indexerParameters = Type.EmptyTypes;
            if (cacheKey.ParameterList.HasItems)
            {
                indexerParameters = cacheKey.ParameterList
                .Select(parameterData => parameterData.ParameterTypeData.UnwrapType())
                .ToArray();
            }
            else if (cacheKey.MethodParameterInfoList.HasItems)
            {
                indexerParameters = cacheKey.MethodParameterInfoList
                .Select(methodParameterInfo => Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle))
                .Where(type => type is not null)
                .ToArray()!;
            }

            // Find the property by name and parameter types (for indexers)
            PropertyInfo? propertyInfo = declaringType?.GetProperty(
                cacheKey.SymbolName,
                HelperExtensionsCommon.AllMembersFullHierarchyFlags,
                null,
                null,
                indexerParameters,
                null);

            if (propertyInfo is null)
            {
                throw new InvalidReflectionCacheKeyException();
            }

            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
        }

        private static ConstructorData CreateConstructorData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberConstructor],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                cacheKey.DeclaringTypeHandle,
                nameof(cacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);

            ConstructorInfo? constructorInfo = null;
            if (cacheKey.MethodHandle != default)
            {
                constructorInfo = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle) as ConstructorInfo;
            }
            else if (declaringType is not null)
            {
                Type[] parameterTypes = cacheKey.ParameterList
                    .Select(parameter => parameter.ParameterTypeData.UnwrapType())
                    .ToArray();
                constructorInfo = declaringType.GetConstructor(
                    HelperExtensionsCommon.AllMembersFullHierarchyFlags,
                    binder: null,
                    types: parameterTypes,
                    modifiers: null);
            }

            if (constructorInfo is null)
            {
                throw declaringType is null
                    ? new InvalidReflectionCacheKeyException(SymbolReflectionInfoCache.InvalidDeclaringTypeHandleFoundInKeyExceptionMessage)
                    : new InvalidReflectionCacheKeyException();
            }

            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
        }

        private static FieldData CreateFieldData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind
                , [SymbolKind.MemberField],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a field symbol.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                cacheKey.DeclaringTypeHandle,
                nameof(cacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);

            FieldInfo? fieldInfo = null;
            if (cacheKey.FieldHandle != default)
            {
                fieldInfo = FieldInfo.GetFieldFromHandle(cacheKey.FieldHandle);
            }
            else if (declaringType is not null)
            {
                fieldInfo = declaringType.GetField(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            }

            if (fieldInfo is null)
            {
                throw declaringType is null
                    ? new InvalidReflectionCacheKeyException(SymbolReflectionInfoCache.InvalidDeclaringTypeHandleFoundInKeyExceptionMessage)
                    : new InvalidReflectionCacheKeyException();
            }

            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo);
        }

        private static MethodData CreateMethodData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.MemberMethod],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a method symbol.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                cacheKey.DeclaringTypeHandle,
                nameof(cacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);

            MethodInfo? methodInfo = null;
            if (cacheKey.MethodHandle != default)
            {
                methodInfo = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle) as MethodInfo;
            }
            else if (declaringType is not null
                && ReferenceEquals(cacheKey.ParameterList, ParameterList.Empty)
                && ReferenceEquals(cacheKey.MethodParameterInfoList, MethodParameterInfoList.Empty)
                && cacheKey.GenericTypeParameterCount == 0)
            {
                methodInfo = declaringType.GetMethod(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            }
            else if (declaringType is not null)
            {
                Type[] parameterTypes = Type.EmptyTypes;
                if (cacheKey.ParameterList.HasItems)
                {
                    parameterTypes = cacheKey.ParameterList
                        .Where(parameterData => parameterData.DeclaringTypeHandle.Equals(cacheKey.DeclaringTypeHandle))
                        .Select(parameterData => parameterData.ParameterTypeData.UnwrapType())
                        .ToArray();
                }
                else if (cacheKey.MethodParameterInfoList.HasItems)
                {
                    parameterTypes = cacheKey.MethodParameterInfoList
                        .Where(methodParameterInfo => methodParameterInfo.DeclaringTypeHandle.Equals(cacheKey.DeclaringTypeHandle))
                        .Select(methodParameterInfo => Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle))
                        .Where(type => type is not null)
                        .ToArray()!;
                }

                methodInfo = declaringType.GetMethod(
                    cacheKey.SymbolName,
                    cacheKey.GenericTypeParameterCount,
                    HelperExtensionsCommon.AllMembersFullHierarchyFlags,
                    binder: null,
                    types: parameterTypes,
                    modifiers: null);
            }

            if (methodInfo is null)
            {
                throw declaringType is null
                    ? new InvalidReflectionCacheKeyException(SymbolReflectionInfoCache.InvalidDeclaringTypeHandleFoundInKeyExceptionMessage)
                    : new InvalidReflectionCacheKeyException();
            }

            MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

            if (methodData.ContainsGenericParameters || methodData.IsGenericMethodDefinition)
            {
                TypeList genericTypeParameters = methodData.GenericMethodParameters;
                methodData = methodData.MakeGenericMethodData(genericTypeParameters);
            }

            return methodData;
        }

        private static EventData CreateEventData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(cacheKey.SymbolKind,
                [SymbolKind.MemberEvent],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating an event symbol.");

            ArgumentNullExceptionAdvanced.ThrowIfDefault(
                cacheKey.DeclaringTypeHandle,
                nameof(cacheKey.DeclaringTypeHandle),
                SymbolReflectionInfoCache.DeclaringTypeHandleInKeyIsDefaultExceptionMessage);

            Type? declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            if (declaringType is null)
            {
                throw new InvalidReflectionCacheKeyException($"The key's property '{nameof(SymbolInfoDataCacheKey)}.{nameof(SymbolInfoDataCacheKey.DeclaringTypeHandle)}' does not contain a valid handle for the declaring type handle.");
            }

            EventInfo? eventInfo = declaringType.GetEvent(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            if (eventInfo is null && declaringType.IsInterface)
            {
                Type[] implementedInterfaces = declaringType.GetInterfaces();
                foreach (Type implementedInterface in implementedInterfaces)
                {
                    eventInfo = implementedInterface.GetEvent(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    if (eventInfo is not null)
                    {
                        break;
                    }
                }
            }

            if (eventInfo is null)
            {
                throw new InvalidReflectionCacheKeyException();
            }

            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
        }

        private static ParameterData CreateParameterData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                cacheKey.SymbolKind,
                [SymbolKind.Parameter],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

            CacheKeyParameterDescriptor parameterDescriptor = cacheKey.CacheKeyParameterDescriptor;
            CacheKeyParameterMemberDescriptor declaringMemberDescriptor = cacheKey.CacheKeyParameterMemberDescriptor;

            Type? declaringType = Type.GetTypeFromHandle(declaringMemberDescriptor.DeclaringTypeHandle);
            TypeData? declaringTypeData = null;
            if (declaringType is not null)
            {
                declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);
            }

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
                    ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
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
                    MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
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

            // Before enumerating the members of the declaring type, check for special types that have known parameter members
            // like delegates and their invoker methods.
            if (declaringTypeData is not null && declaringTypeData.IsDelegate)
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

            if (declaringTypeData is null)
            {
                throw new InvalidReflectionCacheKeyException(SymbolReflectionInfoCache.InvalidDeclaringTypeHandleFoundInKeyExceptionMessage);
            }

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
                else if (declaringMemberDescriptor.HasPropertyTypeHandle)
                {
                    IEnumerable<PropertyData> propertyCandidates = declaringTypeData.EnumerateProperties()
                        .Where(propertyData => propertyData.PropertyTypeData.Handle.Equals(declaringMemberDescriptor.PropertyTypeHandle));

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

            throw declaringType is null
                ? new InvalidReflectionCacheKeyException(SymbolReflectionInfoCache.InvalidDeclaringTypeHandleFoundInKeyExceptionMessage)
                : new InvalidReflectionCacheKeyException();
        }

        private static bool TryFindParameterInProperty(CacheKeyParameterDescriptor parameterDescriptor, CacheKeyParameterMemberDescriptor declaringMemberDescriptor, PropertyData propertyCandidate, out ParameterData? parameterDataCandidate, out InvalidReflectionCacheKeyException? exception)
        {
            parameterDataCandidate = null;
            exception = null;

            bool isPropertyAccessorSetterRequested = declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberNormalPropertySet
                or ParameterizedSymbolKind.MemberIndexerPropertySet;
            bool isIndexerPropertyGetterRequested = declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberIndexerPropertyGet;

            if (isPropertyAccessorSetterRequested)
            {
                if (declaringMemberDescriptor.ParameterizedMemberKind is ParameterizedSymbolKind.MemberIndexerPropertySet
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
            else if (isIndexerPropertyGetterRequested)
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
            else // Accessor is undefined - need to search all accessor methods
            {
                MethodData propertyAccessorCandidate;

                // Simplest case: property is not an indexer. In this case only the set accessor is parameterized.
                if (!propertyCandidate.IsIndexer)
                {
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
                else
                {   // Property is an indexer - both accessors are parameterized.
                    // However, getter and setter share the same parameter list - except the "value" parameter for the setter.
                    // If the searched parameter is not the setter's "value" parameter the parameter association is ambiguous.
                    // In this case the getter takes precedence over the setter.
                    // Hence, we start with the getter to allow it to produce a match first.
                    if (propertyCandidate.CanRead)
                    {
                        propertyAccessorCandidate = propertyCandidate.PropertyGetMethodData;
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
                    else if (propertyCandidate.CanWrite)
                    {
                        propertyAccessorCandidate = propertyCandidate.PropertyGetMethodData;
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

                    return false;
                }
            }
        }

        private static bool TryFindParameterCandidateInMethods(CacheKeyParameterDescriptor parameterDescriptor, CacheKeyParameterMemberDescriptor declaringMemberDescriptor, IEnumerable<MethodData> methodCandidates, out ParameterData? parameterDataCandidate)
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

        private static bool TryFindParameterCandidate(CacheKeyParameterDescriptor parameterDescriptor, ParameterList parameters, out ParameterData? parameterDataCandidate, out InvalidReflectionCacheKeyException? exception)
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

        private static TSymbolInfoData GetOrCreateNormalizedSymbolInfoDataCacheEntry<TSymbolInfoData>(ref SymbolInfoDataCacheKey cacheKey) where TSymbolInfoData : SymbolInfoData
        {
            TSymbolInfoData result;
            SymbolInfoDataCacheKey normalizedCacheKey = SymbolReflectionInfoCache.NormalizeKey(cacheKey);
            _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(normalizedCacheKey, out SymbolInfoData? symbolInfoData);
            result = (TSymbolInfoData)symbolInfoData!;
            cacheKey = normalizedCacheKey;

            return result;
        }
    }
}
