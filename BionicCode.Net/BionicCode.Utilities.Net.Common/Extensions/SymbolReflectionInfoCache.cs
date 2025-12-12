namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using Microsoft.CodeAnalysis;

    internal static class SymbolReflectionInfoCache
    {
        private static readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData> SymbolInfoDataCache = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData>();
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
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateEventData);

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
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage, "event", cacheKey.SymbolName, string.Empty, declaringType.ToFullDisplayName()), nameof(cacheKey));
            }

            return new EventData(eventInfo);
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(SymbolInfoDataCacheKey cacheKey, out MethodData methodData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, CreateMethodData);

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData?.GetType()}");

            methodData = (MethodData)symbolInfoData;
        }

        private static MethodData CreateMethodData(SymbolInfoDataCacheKey cacheKey)
        {
            Type[] parameterTypes = cacheKey.ParameterList
                .Select(parameter => parameter.ParameterTypeData.GetType())
                .ToArray();

            Type[] genericTypeParameters = cacheKey.ParameterList
                .Where(parameter => parameter.IsGenericTypeParamater)
                .Select(parameter => parameter.ParameterTypeData.GetType())
                .ToArray();

            Type declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
            MethodInfo methodInfo = declaringType.GetMethod(
                cacheKey.SymbolName,
                genericTypeParameters.Length,
                HelperExtensionsCommon.AllMembersFlags,
                binder: null,
                types: parameterTypes,
                modifiers: null);

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
                methodInfo = methodInfo.MakeGenericMethod(genericTypeParameters);
            }

            return new MethodData(methodInfo);
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out FieldData fieldData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                  FieldInfo fieldInfo = declaringType.GetField(cacheKey.MemberName, HelperExtensionsCommon.AllMembersFlags);
                  if (fieldInfo is null)
                  {
                      throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage, "field", cacheKey.MemberName, string.Empty, declaringType.ToFullDisplayName()), nameof(cacheKey));
                  }

                  return new FieldData(fieldInfo);
              });

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            fieldData = (FieldData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out PropertyData propertyData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                  PropertyInfo propertyInfo = declaringType.GetProperty(cacheKey.MemberName, HelperExtensionsCommon.AllMembersFlags);
                  if (propertyInfo is null)
                  {
                      throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage, "property", cacheKey.MemberName, string.Empty, declaringType.ToFullDisplayName()), nameof(cacheKey));
                  }

                  return new PropertyData(propertyInfo);
              });

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            propertyData = (PropertyData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out ConstructorData constructorData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  Type[] parameterTypes = cacheKey.ParameterList?.Cast<ParameterInfo>()
              .Select(parameterInfo => parameterInfo.ParameterType)
              .ToArray();

                  var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                  ConstructorInfo constructorInfo = declaringType.GetConstructor(HelperExtensionsCommon.AllMembersFlags, null, parameterTypes, Array.Empty<ParameterModifier>());
                  if (constructorInfo is null)
                  {
                      throw new ArgumentException(string.Format(SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage, "constructor", cacheKey.MemberName, " that matches the provided parameter list ", declaringType.ToFullDisplayName()), nameof(cacheKey));
                  }

                  return new ConstructorData(constructorInfo);
              });

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            constructorData = (ConstructorData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(ITypeDataCacheKey cacheKey, out TypeData typeData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  var type = Type.GetTypeFromHandle(cacheKey.TypeHandle);
                  return new TypeData(type);
              });

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
        /// Do not use with delegates as this overload does not allow to provide the related parameter list in order to eliminate any ambiguities.
        /// </summary>
        internal static ITypeDataCacheKey CreateTypeSymbolCacheKey(RuntimeTypeHandle handle)
          => new TypeDataCacheKey(handle);

        /// <summary>
        /// Use with delegates and provide the parameter list of the delegate.
        /// </summary>
        /// <param name="handle">Handle of the delegate declaringType.</param>
        /// <param name="parameterList">The parameter list of the delegate.</param>
        /// <returns>A valid key that can be used to query the cache.</returns>
        internal static ITypeDataCacheKey CreateTypeSymbolCacheKey(RuntimeTypeHandle handle, params MethodParameterInfo[] parameterList)
          => new TypeDataCacheKey(handle, parameterList);

        /// <summary>
        /// Use with delegates and provide the parameter list of the delegate.
        /// </summary>
        /// <param name="handle">Handle of the delegate declaringType.</param>
        /// <param name="parameterList">The parameter list of the delegate.</param>
        /// <returns>A valid key that can be used to query the cache.</returns>
        internal static ITypeDataCacheKey CreateTypeSymbolCacheKey(RuntimeTypeHandle handle, params ParameterInfo[] parameterList)
          => new TypeDataCacheKey(handle, parameterList);

        /// <summary>
        /// Do not use with methods and constructors as this overload does not allow to provide the related parameter list in order to eliminate any ambiguities.
        /// </summary>
        internal static IMemberDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName)
          => new MemberDataCacheKey(declaringTypeHandle, memberName);

        /// <summary>
        /// Use with methods and constructors and provide the related parameter list.
        /// </summary>
        /// <param name="declaringTypeHandle">Handle of the method or constructor.</param>
        /// <param name="memberName">The name of the member that should be looked up.</param>
        /// <param name="parameterList">The parameter list of the method or constructor.</param>
        /// <returns>A valid key that can be used to query the cache.</returns>
        internal static SymbolInfoDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, ParameterList parameterList)
          => new SymbolInfoDataCacheKey(declaringTypeHandle, memberName, parameterList);

        /// <summary>
        /// Use with methods and constructors and provide the related parameter list.
        /// </summary>
        /// <param name="declaringTypeHandle">Handle of the method or constructor.</param>
        /// <param name="memberName">The name of the member that should be looked up.</param>
        /// <param name="parameterList">The parameter list of the method or constructor.</param>
        /// <returns>A valid key that can be used to query the cache.</returns>
        internal static IMemberDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params ParameterInfo[] parameterList)
          => new MemberDataCacheKey(declaringTypeHandle, memberName, parameterList);
    }
}
