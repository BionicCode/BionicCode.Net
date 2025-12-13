namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out EventData eventData)
        {
            bool isKeyNormalized = TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                cacheKey = normalizedCacheKey;
            }

            // At this point an anonymous key will always fail and therfore trigger actual normalization and creation of the well-known key.
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                eventData = CreateEventData(cacheKey);
                if (!isKeyNormalized)
                {
                    // REMOVE::after testing
                    Debug.WriteLine($"Normalized key for {eventData.GetType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventData.GetEventInfo());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, eventData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {eventData.GetType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            eventData = (EventData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out MethodData methodData)
        {
            bool isKeyNormalized = TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                cacheKey = normalizedCacheKey;
            }

            // At this point an anonymous key will always fail and therfore trigger actual normalization and creation of the well-known key.
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                methodData = CreateMethodData(cacheKey);
                if (!isKeyNormalized)
                {
                    // REMOVE::after testing
                    Debug.WriteLine($"Normalized key for {methodData.GetType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForMethod(methodData.GetMethodInfo());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, methodData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {methodData.GetType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            methodData = (MethodData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out FieldData fieldData)
        {
            bool isKeyNormalized = TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                cacheKey = normalizedCacheKey;
            }

            // At this point an anonymous key will always fail and therfore trigger actual normalization and creation of the well-known key.
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                fieldData = CreateFieldData(cacheKey);
                if (!isKeyNormalized)
                {
                    // REMOVE::after testing
                    Debug.WriteLine($"Normalized key for {fieldData.GetType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForField(fieldData.GetFieldInfo());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, fieldData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {fieldData.GetType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            fieldData = (FieldData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out PropertyData propertyData)
        {
            bool isKeyNormalized = TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                cacheKey = normalizedCacheKey;
            }

            // At this point an anonymous key will always fail and therfore trigger actual normalization and creation of the well-known key.
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                propertyData = CreatePropertyData(cacheKey);
                if (!isKeyNormalized)
                {
                    // REMOVE::after testing
                    Debug.WriteLine($"Normalized key for {propertyData.GetType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForProperty(propertyData.GetPropertyInfo());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, propertyData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {propertyData.GetType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            propertyData = (PropertyData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out ParameterData parameterData)
        {
            SymbolInfoDataCacheKey normalizedCacheKey = SymbolReflectionInfoCache.NormalizeKey(cacheKey);
            _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);
            parameterData = (ParameterData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out ConstructorData constructorData)
        {
            bool isKeyNormalized = TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                cacheKey = normalizedCacheKey;
            }

            // At this point an anonymous key will always fail and therfore trigger actual normalization and creation of the well-known key.
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                constructorData = CreateConstructorData(cacheKey);
                if (!isKeyNormalized)
                {
                    // REMOVE::after testing
                    Debug.WriteLine($"Normalized key for {constructorData.GetType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructorData.GetConstructorInfo());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, constructorData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {constructorData.GetType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            constructorData = (ConstructorData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out TypeData typeData)
        {
            bool isKeyNormalized = TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                cacheKey = normalizedCacheKey;
            }

            // At this point an anonymous key will always fail and therfore trigger actual normalization and creation of the well-known key.
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                typeData = CreateTypeData(cacheKey);
                if (!isKeyNormalized)
                {
                    // REMOVE::after testing
                    Debug.WriteLine($"Normalized key for {typeData.UnwrapType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForType(typeData.UnwrapType());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, typeData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {typeData.UnwrapType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            typeData = (TypeData)symbolInfoData;
        }

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
                    ParameterData parameterData = get;
                default:
                    throw new NotSupportedException($"The symbol kind '{cacheKey.SymbolKind}' is not supported for normalization.");
            }

            _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
            _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(normalizedCacheKey, symbolInfoData);

            return normalizedCacheKey;
        }

        private static TypeData CreateTypeData(SymbolInfoDataCacheKey cacheKey)
        {
            Type type = Type.GetTypeFromHandle(cacheKey.SymbolTypeHandle);
            return new TypeData(type);
        }

        private static PropertyData CreatePropertyData(SymbolInfoDataCacheKey cacheKey)
        {
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
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "property",
                    cacheKey.SymbolName,
                    string.Empty,
                    declaringType.ToFullDisplayName()),
                    nameof(cacheKey));
            }

            return new PropertyData(propertyInfo);
        }

        private static ConstructorData CreateConstructorData(SymbolInfoDataCacheKey cacheKey)
        {
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
                throw new ArgumentException(string.Format(
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "constructor",
                    cacheKey.SymbolName,
                    " that matches the provided parameter list ",
                    declaringType.ToFullDisplayName()),
                    nameof(cacheKey));
            }

            return new ConstructorData(constructorInfo);
        }

        private static FieldData CreateFieldData(SymbolInfoDataCacheKey cacheKey)
        {
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
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "field",
                    cacheKey.SymbolName,
                    string.Empty,
                    declaringType.ToFullDisplayName()), nameof(cacheKey));
            }

            return new FieldData(fieldInfo);
        }

        private static MethodData CreateMethodData(SymbolInfoDataCacheKey cacheKey)
        {
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
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "method",
                    cacheKey.SymbolName,
                    " that matches the provided parameter list ",
                    declaringType.ToFullDisplayName()),
                    nameof(cacheKey));
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
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture,
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "event",
                    cacheKey.SymbolName,
                    string.Empty,
                    declaringType.ToFullDisplayName()),
                    nameof(cacheKey));
            }

            return new EventData(eventInfo);
        }

        private static EventData CreateParameterData(SymbolInfoDataCacheKey cacheKey)
        {
            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            MethodBase methodBase;

            // REVIEW::If-statement order matters here. Order ifrom most specific to least specific i.e. best lookup performance to worst performance.
            if (cacheKey.MethodHandle != default)
            {
                methodBase = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle);
            }
            else if (declaringType.IsDelegate())
            {
                methodBase = declaringType.GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
            }
            else if (!string.IsNullOrWhiteSpace(cacheKey.ParameterMemberName))
            {
                MethodInfo[] methoCandidates = declaringType.GetMethods(HelperExtensionsCommon.AllMembersFullHierarchyFlags);

                // TODO::Find method based on parameter name and ParameterKind and position. When checking for parameter name also consider indexer where the name is 'Item'.

                    .FirstOrDefault(methodInfo => methodInfo.Name.Equals(cacheKey.ParameterMemberName, StringComparison.Ordinal) && );
                if (methodBase is null)
                {
                    PropertyInfo propertyInfo = declaringType.GetProperty(cacheKey.ParameterMemberName, HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    if (propertyInfo is not null)
                    {
                        methodBase = propertyInfo.GetMethod ?? propertyInfo.SetMethod;
                    }
                }
                if (methodBase is null)
                {
                    methodBase = declaringType.GetConstructor(cacheKey.ParameterMemberName);
                }
            }
            else
            {
                declaringType.GetMethod(cacheKey.SymbolName);
                Type[] parameterTypes = cacheKey.MethodParameterInfos
                    .Select(methodParameterInfo => Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle))
                    .ToArray();
                if (cacheKey.SymbolKind == SymbolKind.Constructor)
                {
                    methodBase = declaringType.GetConstructor(
                        HelperExtensionsCommon.AllMembersFullHierarchyFlags,
                        binder: null,
                        types: parameterTypes,
                        modifiers: null);
                }
                else
                {
                    methodBase = declaringType.GetMethod(
                        cacheKey.SymbolName,
                        cacheKey.GenericTypeParameterCount,
                        HelperExtensionsCommon.AllMembersFullHierarchyFlags,
                        binder: null,
                        types: parameterTypes,
                        modifiers: null);
                }
            }
            if (methodBase is null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "method or constructor",
                    cacheKey.SymbolName,
                    " that matches the provided parameter list ",
                    declaringType.ToFullDisplayName()),
                    nameof(cacheKey));
            }
            ParameterInfo parameterInfo = methodBase.GetParameters().FirstOrDefault(p => p.Name == cacheKey.SymbolName);
            if (parameterInfo is null)
            {
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage,
                    "parameter",
                    cacheKey.SymbolName,
                    string.Empty,
                    methodBase.ToFullDisplayName()),
                    nameof(cacheKey));
            }
            return new EventData(parameterInfo);
        }
    }
}
