namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    internal static class SymbolReflectionInfoCache
    {
        private static readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData> SymbolInfoDataCache = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData>();
        private static readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoDataCacheKey> AnonymousSymbolDataCacheKeyMap = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoDataCacheKey>();
        private const string MemberNotFoundArgumentExceptionMessage = "Unable to find the {0} named '{1}'{2}on the type '{3}'.";

        internal static TypeData GetOrCreateSymbolInfoDataCacheEntry(Type type)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForType(type);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new TypeData(type));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {type.GetType()}");

            return (TypeData)symbolInfoData;
        }

        internal static MethodData GetOrCreateSymbolInfoDataCacheEntry(MethodInfo methodInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForMethod(methodInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(methodInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {methodInfo.GetType()}");

            return (MethodData)symbolInfoData;
        }

        internal static ConstructorData GetOrCreateSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructorInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ConstructorData(constructorInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {constructorInfo.GetType()}");

            return (ConstructorData)symbolInfoData;
        }

        internal static FieldData GetOrCreateSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForField(fieldInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new FieldData(fieldInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {fieldInfo.GetType()}");

            return (FieldData)symbolInfoData;
        }

        internal static PropertyData GetOrCreateSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForProperty(propertyInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new PropertyData(propertyInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {propertyInfo.GetType()}");

            return (PropertyData)symbolInfoData;
        }

        internal static EventData GetOrCreateSymbolInfoDataCacheEntry(EventInfo eventInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new EventData(eventInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {eventInfo.GetType()}");

            return (EventData)symbolInfoData;
        }

        internal static ParameterData GetOrCreateSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForParameter(parameterInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ParameterData(parameterInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {parameterInfo.GetType()}");

            return (ParameterData)symbolInfoData;
        }

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="eventData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey, out EventData eventData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<EventData>(ref cacheKey, out eventData);

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="methodData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey, out MethodData methodData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<MethodData>(ref cacheKey, out methodData);

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="fieldData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey, out FieldData fieldData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<FieldData>(ref cacheKey, out fieldData);

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="propertyData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey, out PropertyData propertyData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<PropertyData>(ref cacheKey, out propertyData);

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="parameterData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out ParameterData parameterData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<ParameterData>(ref cacheKey, out parameterData);

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="constructorData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey, out ConstructorData constructorData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<ConstructorData>(ref cacheKey, out constructorData);

        /// <summary>
        /// Gets the existing symbol information data cache entry associated with the specified key, or creates a new
        /// entry if one does not exist.
        /// </summary>
        /// <param name="cacheKey">A reference to the key used to identify the symbol information data cache entry. The key may be updated to
        /// its normalized form.</param>
        /// <param name="typeData">When this method returns, contains the event data associated with the cache entry identified by the
        /// specified key.</param>
        /// <remarks>If the provided <paramref name="cacheKey"/> refers to an anonymous symbol, it will be normalized to a canonical key.</remarks>
        internal static void GetOrCreateSymbolInfoDataCacheEntry(ref SymbolInfoDataCacheKey cacheKey, out TypeData typeData)
            => GetOrCreateNormalizedSymbolInfoDataCacheEntry<TypeData>(ref cacheKey, out typeData);

        internal static bool TryGetSymbolInfoDataCacheEntry<TEntry>(SymbolInfoDataCacheKey key, out TEntry entry)
          where TEntry : SymbolInfoData
        {
            entry = null;

            if (SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(key, out SymbolInfoData symbolInfoData))
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

            SymbolInfoData symbolInfoData;
            switch (cacheKey.SymbolKind)
            {
                case SymbolKind.MemberEvent:
                    EventData eventData = CreateEventData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventData.GetEventInfo());
                    symbolInfoData = eventData;
                    break;
                case SymbolKind.MemberMethod:
                    MethodData methodData = CreateMethodData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForMethod(methodData.GetMethodInfo());
                    symbolInfoData = methodData;
                    break;
                case SymbolKind.Constructor:
                    ConstructorData constructorData = CreateConstructorData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructorData.GetConstructorInfo());
                    symbolInfoData = constructorData;
                    break;
                case SymbolKind.MemberField:
                    FieldData fieldData = CreateFieldData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForField(fieldData.GetFieldInfo());
                    symbolInfoData = fieldData;
                    break;
                case SymbolKind.MemberProperty:
                    PropertyData propertyData = CreatePropertyData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForProperty(propertyData.GetPropertyInfo());
                    symbolInfoData = propertyData;
                    break;
                case SymbolKind.Type:
                    TypeData typeData = CreateTypeData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForType(typeData.UnwrapType());
                    symbolInfoData = typeData;
                    break;
                case SymbolKind.MemberParameter:
                    ParameterData parameterData = CreateParameterData(cacheKey);
                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForParameter(parameterData.GetParameterInfo());
                    symbolInfoData = parameterData;
                    break;
                default:
                    throw new NotSupportedException($"The symbol kind '{cacheKey.SymbolKind}' is not supported for normalization.");
            }

            _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
            _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(normalizedCacheKey, symbolInfoData);

            return normalizedCacheKey;
        }

        private static TypeData CreateTypeData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(
                cacheKey.SymbolKind,
                [SymbolKind.Type],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a type symbol.");

            Type type = Type.GetTypeFromHandle(cacheKey.SymbolTypeHandle);

            if (type is null)
            {
                throw new InvalidReflectionCacheKeyException();
            }

            return new TypeData(type);
        }

        private static PropertyData CreatePropertyData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(
                cacheKey.SymbolKind,
                [SymbolKind.MemberProperty],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a property symbol.");

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            Type[] indexerParameters = Type.EmptyTypes;
            if (cacheKey.ParameterList.HasItems)
            {
                indexerParameters = cacheKey.ParameterList
                .Select(parameterData => parameterData.ParameterTypeData.UnwrapType())
                .ToArray();
            }
            else if (cacheKey.MethodParameterInfos.HasItems)
            {
                indexerParameters = cacheKey.MethodParameterInfos
                .Select(methodParameterInfo => Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle))
                .ToArray();
            }

            // Find the property by name and parameter types (for indexers)
            PropertyInfo propertyInfo = declaringType.GetProperty(
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

            return new PropertyData(propertyInfo);
        }

        private static ConstructorData CreateConstructorData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(
                cacheKey.SymbolKind,
                [SymbolKind.Constructor],
                nameof(cacheKey),
                 $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a constructor symbol.");

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            ConstructorInfo constructorInfo;
            if (cacheKey.MethodHandle != default)
            {
                constructorInfo = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle) as ConstructorInfo;
            }
            else
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
                throw new InvalidReflectionCacheKeyException();
            }

            return new ConstructorData(constructorInfo);
        }

        private static FieldData CreateFieldData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(
                cacheKey.SymbolKind
                , [SymbolKind.MemberField],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a field symbol.");

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            FieldInfo fieldInfo;
            if (cacheKey.FieldHandle != default)
            {
                fieldInfo = FieldInfo.GetFieldFromHandle(cacheKey.FieldHandle);
            }
            else
            {
                fieldInfo = declaringType.GetField(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            }

            if (fieldInfo is null)
            {
                throw new InvalidReflectionCacheKeyException();
            }

            return new FieldData(fieldInfo);
        }

        private static MethodData CreateMethodData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(
                cacheKey.SymbolKind,
                [SymbolKind.MemberMethod],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a method symbol.");

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            MethodInfo methodInfo;
            if (cacheKey.MethodHandle != default)
            {
                methodInfo = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle) as MethodInfo;
            }
            else
            {
                Type[] parameterTypes = Type.EmptyTypes;
                if (cacheKey.ParameterList.HasItems)
                {
                    parameterTypes = cacheKey.ParameterList
                    .Select(parameterData => parameterData.ParameterTypeData.UnwrapType())
                    .ToArray();
                }
                else if (cacheKey.MethodParameterInfos.HasItems)
                {
                    parameterTypes = cacheKey.MethodParameterInfos
                    .Select(methodParameterInfo => Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle))
                    .ToArray();
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
                throw new InvalidReflectionCacheKeyException();
            }

            if (methodInfo.ContainsGenericParameters || methodInfo.IsGenericMethodDefinition)
            {
                IEnumerable<Type> genericTypeParameters = cacheKey.ParameterList.HasItems
                    ? cacheKey.ParameterList.GenericTypeParameters
                    .Select(parameterData => parameterData.ParameterTypeData.UnwrapType())
                    : cacheKey.MethodParameterInfos.GenericTypeParameters
                    .Select(methodParameterInfo => Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle));
                methodInfo = methodInfo.MakeGenericMethod(genericTypeParameters.ToArray());
            }

            return new MethodData(methodInfo);
        }

        private static EventData CreateEventData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(cacheKey.SymbolKind,
                [SymbolKind.MemberEvent],
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating an event symbol.");

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            EventInfo eventInfo = declaringType.GetEvent(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
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

            return new EventData(eventInfo);
        }

        private static ParameterData CreateParameterData(SymbolInfoDataCacheKey cacheKey)
        {
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual<SymbolKind>(
                cacheKey.SymbolKind,
                new SymbolKind[] { SymbolKind.MemberParameter },
                nameof(cacheKey),
                $"The symbol kind '{cacheKey.SymbolKind}' is not valid for creating a parameter symbol.");

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            SymbolInfoDataCacheKey declaringTypeKey = SymbolInfoDataCacheKey.CreateForType(declaringType);
            TypeData declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType);
            ParameterData parameterDataCandidate = null;

            // REVIEW::If-statement order matters here. Order ifrom most specific to least specific i.e. best lookup performance to worst performance.
            if (cacheKey.MethodHandle != default)
            {
                MethodBase methodBase = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle);
                if (methodBase is ConstructorInfo constructorInfo)
                {
                    ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
                    parameterDataCandidate = constructorData.Parameters.FirstOrDefault(
                        parameterData => parameterData.Name.Equals(cacheKey.SymbolName, StringComparison.Ordinal)
                        && parameterData.Position == cacheKey.ParameterPosition);
                }
                else
                {
                    var methodInfo = (MethodInfo)methodBase;
                    MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
                    parameterDataCandidate = methodData.Parameters.FirstOrDefault(
                        parameterData => parameterData.Name.Equals(cacheKey.SymbolName, StringComparison.Ordinal)
                        && parameterData.Position == cacheKey.ParameterPosition);
                }

                return parameterDataCandidate;
            }
            else if (declaringTypeData.IsDelegate)
            {
                MethodData methodData = declaringTypeData.DelegateInvokeMethodData;
                parameterDataCandidate = methodData.Parameters.FirstOrDefault(
                    parameterData => parameterData.Name.Equals(cacheKey.SymbolName, StringComparison.Ordinal)
                    && parameterData.Position == cacheKey.ParameterPosition);

                return parameterDataCandidate;
            }
            else
            {
                bool isParameterMethodNameDefined = !string.IsNullOrWhiteSpace(cacheKey.ParameterMemberName);
                bool isParameterKindDefined = cacheKey.ParameterKind is not ParameterKind.Undefined;
                bool isParameterizedSymbolKindDefined = cacheKey.ParameterizedSymbolKind is not ParameterizedSymbolKind.Undefined;
                bool isAmbiguityExpected = !(isParameterMethodNameDefined && isParameterKindDefined && isParameterizedSymbolKindDefined);
                bool isCandidateAmbiguous = false;
                int discoveredMethodCandidateCount = 0;

                if (cacheKey.ParameterizedSymbolKind is ParameterizedSymbolKind.MemberMethod or ParameterizedSymbolKind.Undefined)
                {
                    foreach (MethodData methodData in declaringTypeData.EnumerateMethods())
                    {
                        if (isParameterMethodNameDefined
                            && !methodData.Name.Equals(cacheKey.ParameterMemberName, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (methodData.Parameters.FirstOrDefault(parameterData => parameterData.Name == cacheKey.SymbolName) is not ParameterData parameterCandidate
                            || parameterCandidate.Position != cacheKey.ParameterPosition)
                        {
                            continue;
                        }

                        if (isParameterKindDefined && cacheKey.ParameterKind != parameterCandidate.ParameterKind)
                        {
                            continue;
                        }

                        parameterDataCandidate = parameterCandidate;
                        discoveredMethodCandidateCount++;
                        isCandidateAmbiguous = isAmbiguityExpected && discoveredMethodCandidateCount > 1;

                        ThrowIfParameterCandidateIsAmbiguous(isCandidateAmbiguous);
                        if (!isAmbiguityExpected && discoveredMethodCandidateCount == 1)
                        {
                            break;
                        }
                    }

                    // Early out if we have a definitive candidate and no ambiguity is expected.
                    // Otherwise we need to continue searching to detect ambiguities.
                    if (!isAmbiguityExpected && parameterDataCandidate is not null)
                    {
                        return parameterDataCandidate;
                    }
                }

                if (cacheKey.ParameterizedSymbolKind is ParameterizedSymbolKind.Constructor or ParameterizedSymbolKind.Undefined)
                {
                    foreach (ConstructorData constructorData in declaringTypeData.EnumerateConstructors())
                    {
                        if (isParameterMethodNameDefined
                            && !constructorData.Name.Equals(cacheKey.ParameterMemberName, StringComparison.Ordinal))
                        {
                            continue;
                        }

                        if (constructorData.Parameters.FirstOrDefault(parameterData => parameterData.Name == cacheKey.SymbolName) is not ParameterData parameterCandidate
                            || parameterCandidate.Position != cacheKey.ParameterPosition)
                        {
                            continue;
                        }

                        if (isParameterKindDefined && cacheKey.ParameterKind != parameterCandidate.ParameterKind)
                        {
                            continue;
                        }

                        parameterDataCandidate = parameterCandidate;
                        discoveredMethodCandidateCount++;
                        isCandidateAmbiguous = isAmbiguityExpected && discoveredMethodCandidateCount > 1;

                        ThrowIfParameterCandidateIsAmbiguous(isCandidateAmbiguous);
                        if (!isAmbiguityExpected && discoveredMethodCandidateCount == 1)
                        {
                            break;
                        }
                    }

                    // Early out if we have a definitive candidate and no ambiguity is expected.
                    // Otherwise we need to continue searching to detect ambiguities.
                    if (!isAmbiguityExpected && parameterDataCandidate is not null)
                    {
                        return parameterDataCandidate;
                    }
                }

                if (cacheKey.ParameterizedSymbolKind is ParameterizedSymbolKind.MemberIndexerProperty or ParameterizedSymbolKind.Undefined)
                {
                    foreach (PropertyData propertyData in declaringTypeData.EnumerateProperties())
                    {
                        if (!propertyData.IsIndexer)
                        {
                            continue;
                        }

                        MethodData propertyAccessorData = propertyData.GetMethodData ?? propertyData.SetMethodData;
                        if (propertyAccessorData.Parameters.FirstOrDefault(parameterData => parameterData.Name.Equals(cacheKey.SymbolName, StringComparison.Ordinal)) is not ParameterData parameterCandidate
                            || parameterCandidate.Position != cacheKey.ParameterPosition)
                        {
                            continue;
                        }

                        if (isParameterKindDefined && cacheKey.ParameterKind != parameterCandidate.ParameterKind)
                        {
                            continue;
                        }

                        parameterDataCandidate = parameterCandidate;
                        discoveredMethodCandidateCount++;
                        isCandidateAmbiguous = isAmbiguityExpected && discoveredMethodCandidateCount > 1;

                        ThrowIfParameterCandidateIsAmbiguous(isCandidateAmbiguous);
                        if (!isAmbiguityExpected && discoveredMethodCandidateCount == 1)
                        {
                            break;
                        }
                    }

                    // Early out if we have a definitive candidate and no ambiguity is expected.
                    // Otherwise we need to continue searching to detect ambiguities.
                    if (!isAmbiguityExpected && parameterDataCandidate is not null)
                    {
                        return parameterDataCandidate;
                    }
                }
            }

            throw new InvalidReflectionCacheKeyException();
        }

        private static void ThrowIfParameterCandidateIsAmbiguous(bool isCandidateAmbiguous)
        {
            if (isCandidateAmbiguous)
            {
                throw new AmbiguousMatchException("Multiple symbols were found that match the provided constraints. To eliminate ambiguity, please provide both member name for the method, constructor or indexer property that defines the parameter and the ParameterKind when creating the cache key. For parameters you can also provide a RuntimeMethodHandle from the member that defines the parameter.");
            }
        }

        private static void GetOrCreateNormalizedSymbolInfoDataCacheEntry<TSymbolInfoData>(ref SymbolInfoDataCacheKey cacheKey, out TSymbolInfoData result) where TSymbolInfoData : SymbolInfoData
        {
            SymbolInfoDataCacheKey normalizedCacheKey = SymbolReflectionInfoCache.NormalizeKey(cacheKey);
            _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);
            result = (TSymbolInfoData)symbolInfoData;
            cacheKey = normalizedCacheKey;
        }
    }
}
