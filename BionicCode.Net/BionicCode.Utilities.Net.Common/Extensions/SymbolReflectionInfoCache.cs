namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
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
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (TypeData)symbolInfoData;
        }

        internal static MethodData GetOrCreateSymbolInfoDataCacheEntry(MethodInfo methodInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForMethod(methodInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(methodInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (MethodData)symbolInfoData;
        }

        internal static ConstructorData GetOrCreateSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructorInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ConstructorData(constructorInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (ConstructorData)symbolInfoData;
        }

        internal static FieldData GetOrCreateSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForField(fieldInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new FieldData(fieldInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (FieldData)symbolInfoData;
        }

        internal static PropertyData GetOrCreateSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForProperty(propertyInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new PropertyData(propertyInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (PropertyData)symbolInfoData;
        }

        internal static EventData GetOrCreateSymbolInfoDataCacheEntry(EventInfo eventInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new EventData(eventInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (EventData)symbolInfoData;
        }

        internal static ParameterData GetOrCreateSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
        {
            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForParameter(parameterInfo);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ParameterData(parameterInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

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

        private static EventData CreateEventData(SymbolInfoDataCacheKey cacheKey)
        {
            var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            EventInfo eventInfo = declaringType.GetEvent(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFlags);
            if (eventInfo is null && declaringType.IsInterface)
            {
                Type[] implementedInterfaces = declaringType.GetInterfaces();
                foreach (Type implementedInterface in implementedInterfaces)
                {
                    eventInfo = implementedInterface.GetEvent(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFlags);
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

        private static MethodData CreateMethodData(SymbolInfoDataCacheKey cacheKey)
        {
            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            ImmutableList<ParameterData> genericTypeParameters = cacheKey.ParameterList.GenericTypeParameters;

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
                    .Select(parameterData => parameterData.ParameterTypeData.GetType())
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
                    genericTypeParameters.Count,
                    HelperExtensionsCommon.AllMembersFlags,
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
                Type[] typeArguments = genericTypeParameters
                    .Select(parameterData => parameterData.ParameterTypeData.GetType())
                    .ToArray();
                methodInfo = methodInfo.MakeGenericMethod(typeArguments);
            }

            return new MethodData(methodInfo);
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

        private static FieldData CreateFieldData(SymbolInfoDataCacheKey cacheKey)
        {
            FieldInfo fieldInfo;
            if (cacheKey.FieldHandle != default)
            {
                fieldInfo = FieldInfo.GetFieldFromHandle(cacheKey.FieldHandle);
            }
            else
            {
                var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                fieldInfo = declaringType.GetField(cacheKey.SymbolName, HelperExtensionsCommon.AllMembersFlags);
            }

            if (fieldInfo is null)
            {
                var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
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

        private static PropertyData CreatePropertyData(SymbolInfoDataCacheKey cacheKey)
        {
            var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            Type[] indexerParameters = Type.EmptyTypes;
            if (cacheKey.ParameterList.HasItems)
            {
                indexerParameters = cacheKey.ParameterList
                .Select(parameterData => parameterData.ParameterTypeData.GetType())
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
                HelperExtensionsCommon.AllMembersFlags,
                null,
                null,
                cacheKey.ParameterList.Select(parameterData => parameterData.ParameterTypeData.GetType()).ToArray(),
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

        private static ConstructorData CreateConstructorData(SymbolInfoDataCacheKey cacheKey)
        {
            ConstructorInfo constructorInfo;
            if (cacheKey.MethodHandle != default)
            {
                constructorInfo = MethodBase.GetMethodFromHandle(cacheKey.MethodHandle) as ConstructorInfo;
            }
            else
            {
                var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                Type[] parameterTypes = cacheKey.ParameterList
                    .Select(parameter => parameter.ParameterTypeData.GetType())
                    .ToArray();
                constructorInfo = declaringType.GetConstructor(
                    HelperExtensionsCommon.AllMembersFlags,
                    binder: null,
                    types: parameterTypes,
                    modifiers: null);
            }

            if (constructorInfo is null)
            {
                var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
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
                    Debug.WriteLine($"Normalized key for {typeData.GetType()}");

                    normalizedCacheKey = SymbolInfoDataCacheKey.CreateForType(typeData.GetType());
                    _ = SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryAdd(cacheKey, normalizedCacheKey);
                    cacheKey = normalizedCacheKey;
                }

                _ = SymbolReflectionInfoCache.SymbolInfoDataCache.TryAdd(cacheKey, typeData);

                // REMOVE::after testing
                Debug.WriteLine($"Created SymbolInfoData entry for {typeData.GetType()}");

                return;
            }

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            typeData = (TypeData)symbolInfoData;
        }

        private static TypeData CreateTypeData(SymbolInfoDataCacheKey cacheKey)
        {
            Type type = Type.GetTypeFromHandle(cacheKey.SymbolTypeHandle);
            return new TypeData(type);
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

        public static bool TryGetNormalizedKey(SymbolInfoDataCacheKey cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey)
        {
            if (cacheKey.IsAnonymousSymbolKey)
            {
                return SymbolReflectionInfoCache.AnonymousSymbolDataCacheKeyMap.TryGetValue(cacheKey, out normalizedCacheKey);
            }

            normalizedCacheKey = cacheKey;
            return true;
        }
    }
}
