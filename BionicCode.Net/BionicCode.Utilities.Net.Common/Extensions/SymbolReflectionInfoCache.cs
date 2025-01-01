namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Management;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using Microsoft.CodeAnalysis;

  internal static class SymbolReflectionInfoCache
  {
    private static readonly Dictionary<ISymbolInfoDataCacheKey, SymbolInfoData> SymbolInfoDataCache = new Dictionary<ISymbolInfoDataCacheKey, SymbolInfoData>();

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
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new MethodData(methodInfo);
      }

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
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new TypeData(type)
        {
          IsParameterType = true
        };
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (TypeData)symbolInfoData;
    }

    internal static MethodData GetOrCreateSymbolInfoDataCacheEntry(MethodInfo methodInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(methodInfo.DeclaringType.TypeHandle, methodInfo.Name, methodInfo.GetParameters());
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new MethodData(methodInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (MethodData)symbolInfoData;
    }

    internal static ConstructorData GetOrCreateSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(constructorInfo.DeclaringType.TypeHandle, constructorInfo.Name, constructorInfo.GetParameters());
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new ConstructorData(constructorInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (ConstructorData)symbolInfoData;
    }

    internal static FieldData GetOrCreateSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(fieldInfo.DeclaringType.TypeHandle, fieldInfo.Name);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new FieldData(fieldInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (FieldData)symbolInfoData;
    }

    internal static PropertyData GetOrCreateSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(propertyInfo.DeclaringType.TypeHandle, propertyInfo.Name);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new PropertyData(propertyInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (PropertyData)symbolInfoData;
    }

    internal static EventData GetOrCreateSymbolInfoDataCacheEntry(EventInfo eventInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(eventInfo.DeclaringType.TypeHandle, eventInfo.Name);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new EventData(eventInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (EventData)symbolInfoData;
    }

    internal static ParameterData GetOrCreateSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new MemberDataCacheKey(parameterInfo.Member.DeclaringType.TypeHandle, parameterInfo.Name);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new ParameterData(parameterInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      return (ParameterData)symbolInfoData;
    }

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out EventData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        EventInfo eventInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetEvent(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
        if (eventInfo is null)
        {
          return false;
        }

        symbolInfoData = new EventData(eventInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      eventData = (EventData)symbolInfoData;

      return true;
    }

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out MethodData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
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
          return false;
        }

        if (methodInfo.ContainsGenericParameters || methodInfo.IsGenericMethodDefinition)        
        {
          if (genericTypeParameters.Length != methodInfo.GetGenericArguments().Length) 
          {
            throw new InvalidOperationException($"The number of provided generic tpe arguments ({genericTypeParameters.Length}) does not match the generic type parameter count found on method {methodInfo.ToSignatureShortName()}.");
          }

          methodInfo = methodInfo.MakeGenericMethod(genericTypeParameters);
        }

        symbolInfoData = new MethodData(methodInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

#if DEBUG
      // TODO::Remove after testing
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      eventData = (MethodData)symbolInfoData;

      return true;
    }

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out FieldData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        FieldInfo fieldInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetField(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
        if (fieldInfo is null)
        {
          return false;
        }

        symbolInfoData = new FieldData(fieldInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      eventData = (FieldData)symbolInfoData;

      return true;
    }

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out PropertyData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        PropertyInfo propertyInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetProperty(cacheKey.MemberName, SymbolInfoData.AllMembersFlags);
        if (propertyInfo is null)
        {
          return false;
        }

        symbolInfoData = new PropertyData(propertyInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      eventData = (PropertyData)symbolInfoData;

      return true;
    }

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(IMemberDataCacheKey cacheKey, out ConstructorData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        Type[] parameterTypes = cacheKey.ParameterList.Cast<ParameterInfo>()
          .Select(parameterInfo => parameterInfo.ParameterType)
          .ToArray();
        ConstructorInfo constructorInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetConstructor(SymbolInfoData.AllMembersFlags, null, parameterTypes, Array.Empty<ParameterModifier>());
        if (constructorInfo is null)
        {
          return false;
        }

        symbolInfoData = new ConstructorData(constructorInfo);
        SymbolReflectionInfoCache.SymbolInfoDataCache.Add(cacheKey, symbolInfoData);
      }

      // TODO::Remove after testing
#if DEBUG
      else
      {
        Debug.WriteLine($"Found SymbolInfoData entry for {symbolInfoData.GetType()}");
      }
#endif

      eventData = (ConstructorData)symbolInfoData;

      return true;
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

    internal static ISymbolInfoDataCacheKey CreateTypeSymbolCacheKey(RuntimeTypeHandle handle, params MemberParameterInfo[] parameterList) 
      => new TypeDataCacheKey(handle, parameterList);

    internal static ISymbolInfoDataCacheKey CreateTypeSymbolCacheKey(RuntimeTypeHandle handle, params ParameterInfo[] parameterList)
      => new TypeDataCacheKey(handle, parameterList);

    internal static IMemberDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName) 
      => new MemberDataCacheKey(declaringTypeHandle, memberName);

    internal static IMemberDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params MemberParameterInfo[] parameterList)
      => new MemberDataCacheKey(declaringTypeHandle, memberName, parameterList);

    internal static IMemberDataCacheKey CreateMemberSymbolCacheKey(RuntimeTypeHandle declaringTypeHandle, string memberName, params ParameterInfo[] parameterList)
      => new MemberDataCacheKey(declaringTypeHandle, memberName, parameterList);
  }
}