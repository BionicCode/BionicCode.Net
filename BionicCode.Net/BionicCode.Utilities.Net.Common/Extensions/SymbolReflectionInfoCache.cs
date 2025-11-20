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
        private static readonly ConcurrentDictionary<ISymbolInfoDataCacheKey, SymbolInfoData> SymbolInfoDataCache = new ConcurrentDictionary<ISymbolInfoDataCacheKey, SymbolInfoData>();
        private const string MemberNotFoundArgumentExceptionMessage = "Unable to find the {0} named '{1}'{2}on the type '{3}'.";

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(Type type)
        {
            ISymbolInfoDataCacheKey cacheKey = new TypeDataCacheKey(type.TypeHandle);
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                symbolInfoData = new TypeData(type);
            }

            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(MethodInfo methodInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(methodInfo.DeclaringType.TypeHandle, methodInfo.Name, methodInfo.GetParameters());
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(methodInfo));
            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(constructorInfo.DeclaringType.TypeHandle, constructorInfo.Name, constructorInfo.GetParameters());
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                symbolInfoData = new ConstructorData(constructorInfo);
            }

            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(fieldInfo.DeclaringType.TypeHandle, fieldInfo.Name);
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                symbolInfoData = new FieldData(fieldInfo);
            }

            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(propertyInfo.DeclaringType.TypeHandle, propertyInfo.Name);
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                symbolInfoData = new PropertyData(propertyInfo);
            }

            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(EventInfo eventInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(eventInfo.DeclaringType.TypeHandle, eventInfo.Name);
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                symbolInfoData = new EventData(eventInfo);
            }

            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static void AddOrReplaceSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(parameterInfo.Member.DeclaringType.TypeHandle, parameterInfo.Name);
            if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
            {
                symbolInfoData = new ParameterData(parameterInfo);
            }

            SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
        }

        internal static TypeData GetOrCreateSymbolInfoDataCacheEntry(Type type)
        {
            ISymbolInfoDataCacheKey cacheKey = new TypeDataCacheKey(type.TypeHandle);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new TypeData(type));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (TypeData)symbolInfoData;
        }

        internal static MethodData GetOrCreateSymbolInfoDataCacheEntry(MethodInfo methodInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(methodInfo.DeclaringType.TypeHandle, methodInfo.Name, methodInfo.GetParameters());
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new MethodData(methodInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (MethodData)symbolInfoData;
        }

        internal static ConstructorData GetOrCreateSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(constructorInfo.DeclaringType.TypeHandle, constructorInfo.Name, constructorInfo.GetParameters());
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ConstructorData(constructorInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (ConstructorData)symbolInfoData;
        }

        internal static FieldData GetOrCreateSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(fieldInfo.DeclaringType.TypeHandle, fieldInfo.Name);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new FieldData(fieldInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (FieldData)symbolInfoData;
        }

        internal static PropertyData GetOrCreateSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(propertyInfo.DeclaringType.TypeHandle, propertyInfo.Name);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new PropertyData(propertyInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (PropertyData)symbolInfoData;
        }

        internal static EventData GetOrCreateSymbolInfoDataCacheEntry(EventInfo eventInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(eventInfo.DeclaringType.TypeHandle, eventInfo.Name);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new EventData(eventInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (EventData)symbolInfoData;
        }

        internal static ParameterData GetOrCreateSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
        {
            ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(parameterInfo.Member.DeclaringType.TypeHandle, parameterInfo.Name);
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey, key => new ParameterData(parameterInfo));

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            return (ParameterData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out EventData eventData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                  EventInfo eventInfo = declaringType.GetEvent(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
                  if (eventInfo is null)
                  {
                      if (declaringType.IsInterface)
                      {
                          Type[] implementedInterfaces = declaringType.GetInterfaces();
                          foreach (Type implementedInterface in implementedInterfaces)
                          {
                              eventInfo = declaringType.GetEvent(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
                              if (eventInfo != null)
                              {
                                  break;
                              }
                          }
                      }

                      if (eventInfo is null)
                      {
                          throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage, "event", cacheKey.MemberName, string.Empty, declaringType.ToFullDisplayName()), nameof(cacheKey));
                      }
                  }

                  return new EventData(eventInfo);
              });

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");

            eventData = (EventData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out MethodData methodData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  Type[] parameterTypes = cacheKey.ParameterList
              .Select(parameter => parameter.ParameterType)
              .ToArray();

                  Type[] genericTypeParameters = cacheKey.ParameterList
              .Where(parameter => parameter.IsGenericTypeParameter)
              .Select(parameter => parameter.ParameterType)
              .ToArray();

                  MethodInfo methodInfo = null;
                  var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);

#if NET9_0_OR_GREATER
          methodInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetMethod(cacheKey.MemberName, genericTypeParameters.Length, SymbolInfoData.AllMembersFlags, parameterTypes);
#else
                  List<MethodInfo> methodInfoCandidates = declaringType.GetMethods(SymbolInfoData.AllMembersFlags)
              .Where(method => method.Name.Equals(cacheKey.MemberName, StringComparison.Ordinal))
              .ToList();
                  if (methodInfoCandidates.Count > 1)
                  {
                      foreach (MethodInfo candidate in methodInfoCandidates.Where(method => methodInfo.GetParameters().Length == parameterTypes.Length))
                      {
                          ParameterInfo[] candidateParameters = candidate.GetParameters();
                          bool hasMismatch = false;
                          for (int parameterIndex = 0; parameterIndex < parameterTypes.Length; parameterIndex++)
                          {
                              MemberParameterInfo predicateParameterInfo = cacheKey.ParameterList[parameterIndex];
                              ParameterInfo candidateParameterInfo = candidateParameters[parameterIndex];
                              Type candidateParameterType = candidateParameterInfo.ParameterType;

                              if (predicateParameterInfo.IsGenericTypeParameter != candidateParameterType.IsGenericParameter
                          && predicateParameterInfo.ParameterType != candidateParameterInfo.ParameterType)
                              {
                                  hasMismatch = true;
                                  break;
                              }
                          }

                          if (!hasMismatch)
                          {
                              methodInfo = candidate;
                              break;
                          }
                      }
                  }
                  else
                  {
                      methodInfo = methodInfoCandidates.FirstOrDefault();
                  }
#endif

                  if (methodInfo is null)
                  {
                      throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SymbolReflectionInfoCache.MemberNotFoundArgumentExceptionMessage, "method", cacheKey.MemberName, " that matches the provided parameter list ", declaringType.ToFullDisplayName()), nameof(cacheKey));
                  }

                  if (methodInfo.ContainsGenericParameters || methodInfo.IsGenericMethodDefinition)
                  {
                      if (genericTypeParameters.Length != methodInfo.GetGenericArguments().Length)
                      {
                          throw new ArgumentException($"The number of provided generic declaringType arguments ({genericTypeParameters.Length}) does not match the generic declaringType parameter count found on method {methodInfo.ToSignatureShortName()}.", nameof(cacheKey));
                      }

                      methodInfo = methodInfo.MakeGenericMethod(genericTypeParameters);
                  }

                  return new MethodData(methodInfo);
              });

            // REMOVE::after testing
            Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData?.GetType()}");

            methodData = (MethodData)symbolInfoData;
        }

        internal static void GetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out FieldData fieldData)
        {
            SymbolInfoData symbolInfoData = SymbolReflectionInfoCache.SymbolInfoDataCache.GetOrAdd(cacheKey,
              key =>
              {
                  var declaringType = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle);
                  FieldInfo fieldInfo = declaringType.GetField(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
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
                  PropertyInfo propertyInfo = declaringType.GetProperty(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
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
                  ConstructorInfo constructorInfo = declaringType.GetConstructor(SymbolInfoData.AllMembersFlags, null, parameterTypes, Array.Empty<ParameterModifier>());
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

        internal static bool TryGetSymbolInfoDataCacheEntry<TEntry>(ISymbolInfoDataCacheKey key, out TEntry entry)
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
        internal static ITypeDataCacheKey CreateTypeSymbolCacheKey(RuntimeTypeHandle handle, params MemberParameterInfo[] parameterList)
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
        internal static IMemberDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params MemberParameterInfo[] parameterList)
          => new MemberDataCacheKey(declaringTypeHandle, memberName, parameterList);

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
