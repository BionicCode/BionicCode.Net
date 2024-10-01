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
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeTypeHandle>(type.Name, type.Namespace, default, type.TypeHandle, Type.EmptyTypes);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new TypeData(type);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static void AddOrReplaceSymbolInfoDataCacheEntry(MethodInfo constructorInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeMethodHandle>(constructorInfo.Name, constructorInfo.DeclaringType.Namespace, constructorInfo.DeclaringType.TypeHandle, constructorInfo.MethodHandle, constructorInfo.GetParameters());
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new MethodData(constructorInfo);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static void AddOrReplaceSymbolInfoDataCacheEntry(ConstructorInfo constructorInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeMethodHandle>(constructorInfo.Name, constructorInfo.DeclaringType.Namespace, constructorInfo.DeclaringType.TypeHandle, constructorInfo.MethodHandle, constructorInfo.GetParameters());
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new ConstructorData(constructorInfo);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static void AddOrReplaceSymbolInfoDataCacheEntry(FieldInfo fieldInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeFieldHandle>(fieldInfo.Name, fieldInfo.DeclaringType.Namespace, fieldInfo.DeclaringType.TypeHandle, fieldInfo.FieldHandle, fieldInfo.FieldType);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new FieldData(fieldInfo);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static void AddOrReplaceSymbolInfoDataCacheEntry(PropertyInfo propertyInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey(propertyInfo.Name, propertyInfo.DeclaringType.Namespace, propertyInfo.DeclaringType.TypeHandle, propertyInfo.PropertyType);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new PropertyData(propertyInfo);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static void AddOrReplaceSymbolInfoDataCacheEntry(EventInfo eventInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey(eventInfo.Name, eventInfo.DeclaringType.Namespace, eventInfo.DeclaringType.TypeHandle, eventInfo.EventHandlerType);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new EventData(eventInfo);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static void AddOrReplaceSymbolInfoDataCacheEntry(ParameterInfo parameterInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey(parameterInfo.Name, parameterInfo.Member.DeclaringType.Namespace, parameterInfo.Member.DeclaringType.TypeHandle, parameterInfo.ParameterType);
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new ParameterData(parameterInfo);
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache[cacheKey] = symbolInfoData;
    }

    internal static TypeData GetOrCreateSymbolInfoDataCacheEntry(Type type)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeTypeHandle>(type.Name, type.Namespace, default, type.TypeHandle, Type.EmptyTypes);
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

    internal static MethodData GetOrCreateSymbolInfoDataCacheEntry(MethodInfo constructorInfo)
    {
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeMethodHandle>(constructorInfo.Name, constructorInfo.DeclaringType.Namespace, constructorInfo.DeclaringType.TypeHandle, constructorInfo.MethodHandle, constructorInfo.GetParameters());
      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        symbolInfoData = new MethodData(constructorInfo);
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
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeMethodHandle>(constructorInfo.Name, constructorInfo.DeclaringType.Namespace, constructorInfo.DeclaringType.TypeHandle, constructorInfo.MethodHandle, constructorInfo.GetParameters());
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
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey<RuntimeFieldHandle>(fieldInfo.Name, fieldInfo.DeclaringType.Namespace, fieldInfo.DeclaringType.TypeHandle, fieldInfo.FieldHandle, fieldInfo.FieldType);
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
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey(propertyInfo.Name, propertyInfo.DeclaringType.Namespace, propertyInfo.DeclaringType.TypeHandle, propertyInfo.PropertyType);
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
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey(eventInfo.Name, eventInfo.DeclaringType.Namespace, eventInfo.DeclaringType.TypeHandle, eventInfo.EventHandlerType);
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
      ISymbolInfoDataCacheKey cacheKey = new SymbolInfoDataCacheKey(parameterInfo.Name, parameterInfo.Member.DeclaringType.Namespace, parameterInfo.Member.DeclaringType.TypeHandle, parameterInfo.ParameterType);
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

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(ISymbolInfoDataCacheKey cacheKey, out EventData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        EventInfo eventInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetEvent(cacheKey.Name, SymbolInfoData.AllMembersFlags);
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

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(ISymbolInfoDataCacheKey cacheKey, out MethodData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        MethodInfo methodInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetMethod(cacheKey.Name, SymbolInfoData.AllMembersFlags);
        if (methodInfo is null)
        {
          return false;
        }

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

      eventData = (MethodData)symbolInfoData;

      return true;
    }

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(ISymbolInfoDataCacheKey cacheKey, out FieldData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        FieldInfo fieldInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetField(cacheKey.Name, SymbolInfoData.AllMembersFlags);
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

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(ISymbolInfoDataCacheKey cacheKey, out PropertyData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        PropertyInfo propertyInfo = Type.GetTypeFromHandle(cacheKey.DeclaringTypeHandle).GetProperty(cacheKey.Name, SymbolInfoData.AllMembersFlags);
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

    internal static bool TryGetOrCreateSymbolInfoDataCacheEntry(ISymbolInfoDataCacheKey cacheKey, out ConstructorData eventData)
    {
      eventData = null;

      if (!SymbolReflectionInfoCache.SymbolInfoDataCache.TryGetValue(cacheKey, out SymbolInfoData symbolInfoData))
      {
        Type[] parameterTypes = cacheKey.Arguments.Cast<ParameterInfo>()
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

    // TODO::Test performance with 'in' parameter to pass the struct (key) by reference
    private static SymbolInfoData CreateMemberInfoDataCacheEntry(object symbolInfo, ISymbolInfoDataCacheKey key)
    {
      SymbolInfoData entry;
      switch (symbolInfo)
      {
        case Type type:
          entry = new TypeData(type);
          break;
        case MethodInfo method:
          entry = new MethodData(method);
          break;
        case ConstructorInfo constructor:
          entry = new ConstructorData(constructor);
          break;
        case FieldInfo field:
          entry = new FieldData(field);
          break;
        case PropertyInfo property:
          entry = new PropertyData(property);
          break;
        case EventInfo eventInfo:
          entry = new EventData(eventInfo);
          break;
        case ParameterInfo parameter:
          entry = new ParameterData(parameter);
          break;
        default:
          throw new NotImplementedException();
      }

      SymbolReflectionInfoCache.SymbolInfoDataCache.Add(key, entry);
      return entry;
    }

    internal static ISymbolInfoDataCacheKey CreateMemberSymbolCacheKey<THandle>(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle, THandle symbolHandle) where THandle : struct
    {
      ISymbolInfoDataCacheKey key = new SymbolInfoDataCacheKey<THandle>(symbolName, symbolNamespace, default, symbolHandle, Type.EmptyTypes);
      return key;
    }

    internal static ISymbolInfoDataCacheKey CreateMemberSymbolCacheKey(string symbolName, string symbolNamespace, RuntimeTypeHandle declaringTypeHandle)
    {
      ISymbolInfoDataCacheKey key = new SymbolInfoDataCacheKey(symbolName, symbolNamespace, default, Type.EmptyTypes);
      return key;
    }
  }
}