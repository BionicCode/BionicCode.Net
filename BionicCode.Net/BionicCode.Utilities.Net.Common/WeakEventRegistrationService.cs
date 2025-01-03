namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.InteropServices;
  using BionicCode.Utilities.Net;
  using Microsoft.Win32;

  internal class WeakEventRegistrationService
  {
    private static readonly Dictionary<Type, ImmutableHashSet<Type>> typeHierarchies = new Dictionary<Type, ImmutableHashSet<Type>>();
    private static readonly object staticReadWriteSyncLock = new object();
    private readonly Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>> listenerRegistrars;
    private readonly Dictionary<Type, HashSet<string>> typeToRegisteredEventsMap;
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly object syncLock;

    public bool HasPublishers => this.eventSourceInstances.Count > 0;

    public WeakEventRegistrationService()
    {
      this.eventSourceInstances = new WeakCollection<object>();
      this.listenerRegistrars = new Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>>();
      this.typeToRegisteredEventsMap = new Dictionary<Type, HashSet<string>>();
      this.syncLock = new object();
    }

    public void AddSourceInstance(object eventSource)
    {
      lock (this.syncLock)
      {
        Type eventSourceType = eventSource.GetType();
        IEnumerable<EventInfo> eventInfos = eventSourceType.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        foreach (EventInfo eventInfo in eventInfos)
        {
          AddSourceInstanceInternal(eventSource, eventInfo);
        }
      }
    }

    public void AddSourceInstance(object eventSource, string eventName)
    {
      lock (this.syncLock)
      {
        Debug.Assert(!string.IsNullOrWhiteSpace(eventName));

        Type eventSourceType = eventSource.GetType();
        EventInfo eventInfo = eventSourceType.GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        if (eventInfo is null)
        {
          throw new ArgumentException($"The event {eventInfo.Name} was not found on the event source {eventSourceType.FullName} or on its declaring base eventHandlerGenericTypeDefinition.");
        }

        Debug.Assert(eventInfo.ReflectedType == eventSourceType);

        AddSourceInstanceInternal(eventSource, eventInfo);
      }
    }

    public void AddSourceInstanceInternal(object eventSource, EventInfo eventInfo)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(eventSource))
        {
          this.eventSourceInstances.Add(eventSource);
        }

        Type eventSourceType = eventSource.GetType();
        Debug.Assert(eventInfo.ReflectedType == eventSourceType);

        if (!this.typeToRegisteredEventsMap.TryGetValue(eventSourceType, out HashSet<string> registeredEventsLookupTable))
        {
          registeredEventsLookupTable = new HashSet<string>();
          this.typeToRegisteredEventsMap.Add(eventSourceType, registeredEventsLookupTable);
        }

        _ = registeredEventsLookupTable.Add(eventInfo.Name);
      }

      RegisterListenersFor(eventSource, eventInfo.Name);
    }

    public void RemoveSourceInstance(object eventSource, bool removeListeners)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(eventSource))
        {
          return;
        }

        Type eventSourceType = eventSource.GetType();
        if (!this.typeToRegisteredEventsMap.TryGetValue(eventSourceType, out HashSet<string> registeredEventsLookupTable))
        {
          return;
        }

        foreach (string eventName in registeredEventsLookupTable)
        {
          RemoveSourceInstanceInternal(eventSource, eventName, removeListeners);
        }

        registeredEventsLookupTable.Clear();
      }
    }

    public void RemoveSourceInstance(object eventSource, string eventName, bool removeListeners)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(eventSource))
        {
          return;
        }

        Type eventSourceType = eventSource.GetType();
        if (!(this.typeToRegisteredEventsMap.TryGetValue(eventSourceType, out HashSet<string> registeredEventsLookupTable)
          && registeredEventsLookupTable.Contains(eventName)))
        {
          return;
        }

        RemoveSourceInstanceInternal(eventSource, eventName, removeListeners);
        _ = registeredEventsLookupTable.Remove(eventName);
      }
    }

    private void RemoveSourceInstanceInternal(object eventSource, string eventName, bool removeListeners)
    {
      _ = this.eventSourceInstances.Remove(eventSource);
      UnregisterListenersFor(eventSource, eventName, removeListeners);
    }

    public void RegisterHandler(IClientEventHandlerRegistrar registrar)
    {
      lock (this.syncLock)
      {
        if (!this.listenerRegistrars.TryGetValue(registrar.EventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
        {
          listenersForAllEventsOfSourceType = new Dictionary<string, List<IClientEventHandlerRegistrar>>();
          this.listenerRegistrars.Add(registrar.EventSourceType, listenersForAllEventsOfSourceType);
        }

        if (!listenersForAllEventsOfSourceType.TryGetValue(registrar.EventName, out List<IClientEventHandlerRegistrar> listenersForEventName))
        {
          listenersForEventName = new List<IClientEventHandlerRegistrar>();
          listenersForAllEventsOfSourceType.Add(registrar.EventName, listenersForEventName);
        }

        listenersForEventName.Add(registrar);
        RegisterClientHandlersFor(registrar);
      }
    }

    public void UnregisterHandler<TObservedEventSource>(string eventName, Delegate clientHandler)
    {
      lock (this.syncLock)
      {
        Type eventSourceType = typeof(TObservedEventSource);
        if (!(this.listenerRegistrars.TryGetValue(eventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType)
          && listenersForAllEventsOfSourceType.TryGetValue(eventName, out List<IClientEventHandlerRegistrar> listenersForEventName)))
        {
          return;
        }

        for (int index = listenersForEventName.Count - 1; index >= 0; index--)
        {
          IClientEventHandlerRegistrar registrar = listenersForEventName[index];
          if (registrar.TryGetClientHandler(out Delegate storedDelegate) && Delegate.Equals(storedDelegate, clientHandler))
          {
            UnregisterClientHandlersFor(registrar);
            listenersForEventName.RemoveAt(index);
          }
        }
      }
    }

    public void UnregisterAllHandlers<TObservedEventSource>()
    {
      lock (this.syncLock)
      {
        Type eventSourceType = typeof(TObservedEventSource);
        if (!(this.listenerRegistrars.TryGetValue(eventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType)))
        {
          return;
        }

        foreach (KeyValuePair<string, List<IClientEventHandlerRegistrar>> listenersForEventNameEntry in listenersForAllEventsOfSourceType)
        {
          for (int index = listenersForEventNameEntry.Value.Count - 1; index >= 0; index--)
          {
            IClientEventHandlerRegistrar registrar = listenersForEventNameEntry.Value[index];
            UnregisterClientHandlersFor(registrar);
            listenersForEventNameEntry.Value.RemoveAt(index);
          }
        }
      }
    }

    public void UnregisterAllHandlersFromEvent<TObservedEventSource>(string eventName)
    {
      lock (this.syncLock)
      {
        Type eventSourceType = typeof(TObservedEventSource);
        if (!(this.listenerRegistrars.TryGetValue(eventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType)
          && listenersForAllEventsOfSourceType.TryGetValue(eventName, out List<IClientEventHandlerRegistrar> listenersForEventName)))
        {
          return;
        }

        for (int index = listenersForEventName.Count - 1; index >= 0; index--)
        {
          IClientEventHandlerRegistrar registrar = listenersForEventName[index];
          UnregisterClientHandlersFor(registrar);
          listenersForEventName.RemoveAt(index);
        }
      }
    }

    private void RegisterListenersFor(object eventSource, string eventName)
    {
      ImmutableHashSet<Type> supportedObservableTypes = EnsureSupportedObservableTypes(eventSource);

      foreach (Type supportedListenerType in supportedObservableTypes)
      {
        if (this.listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
        {
          if (listenersForAllEventsOfSourceType.TryGetValue(eventName, out List<IClientEventHandlerRegistrar> listenersForEventName))
          {
            foreach (IClientEventHandlerRegistrar registrar in listenersForEventName)
            {
              if (registrar.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase))
              {
                registrar.RegisterDelegate(eventSource);
              }
            }
          }
        }
      }
    }

    private void UnregisterListenersFor<TEventSource>(TEventSource eventSource, string eventName, bool removeListeners)
    {
      ImmutableHashSet<Type> supportedObservableTypes = EnsureSupportedObservableTypes(eventSource);
      foreach (Type supportedListenerType in supportedObservableTypes)
      {
        if (this.listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
        {
          if (listenersForAllEventsOfSourceType.TryGetValue(eventName, out List<IClientEventHandlerRegistrar> listenersForEventName))
          {
            for (int index = listenersForEventName.Count - 1; index >= 0; index--)
            {
              IClientEventHandlerRegistrar registrar = listenersForEventName[index];
              if (registrar.EventName.Equals(eventName, StringComparison.OrdinalIgnoreCase))
              {
                registrar.UnregisterDelegate(eventSource);

                if (removeListeners)
                {
                  listenersForEventName.RemoveAt(index);
                }
              }
            }
          }
        }
      }
    }

    private void RegisterClientHandlersFor(IClientEventHandlerRegistrar registrar)
    {
      foreach (object eventSource in this.eventSourceInstances)
      {
        ImmutableHashSet<Type> supportedObservableTypes = EnsureSupportedObservableTypes(eventSource);
        if (!supportedObservableTypes.Contains(registrar.EventSourceType))
        {
          continue;
        }

        if (!(this.typeToRegisteredEventsMap.TryGetValue(eventSource.GetType(), out HashSet<string> registeredEventsLookupTable) 
          && registeredEventsLookupTable.Contains(registrar.EventName)))
        {
          continue;
        }

        registrar.RegisterDelegate(eventSource);
      }
    }

    private void UnregisterClientHandlersFor(IClientEventHandlerRegistrar registrar)
    {
      foreach (object eventSource in this.eventSourceInstances)
      {
        ImmutableHashSet<Type> supportedObservableTypes = EnsureSupportedObservableTypes(eventSource);
        if (!supportedObservableTypes.Contains(registrar.EventSourceType))
        {
          continue;
        }

        if (!(this.typeToRegisteredEventsMap.TryGetValue(eventSource.GetType(), out HashSet<string> registeredEventsLookupTable)
          && registeredEventsLookupTable.Contains(registrar.EventName)))
        {
          continue;
        }

        registrar.UnregisterDelegate(eventSource);
      }
    }

    private static ImmutableHashSet<Type> EnsureSupportedObservableTypes(object eventSource)
    {
      Type eventSourceType = eventSource.GetType();
      
      lock (WeakEventRegistrationService.staticReadWriteSyncLock)
      {
        if (!WeakEventRegistrationService.typeHierarchies.TryGetValue(eventSourceType, out ImmutableHashSet<Type> supportedObservableTypes))
        {
          supportedObservableTypes = TypeHierarchyProvider.GetTypeHierarchy(eventSourceType, includeCurrentType: true);
          WeakEventRegistrationService.typeHierarchies.Add(eventSourceType, supportedObservableTypes);
        }

        return supportedObservableTypes;
      }
    }
  }

  internal static class TypeHierarchyProvider
  {
    public static ImmutableHashSet<Type> GetTypeHierarchy(Type type, bool includeCurrentType)
    {
      if (!typeHierarchyMap.TryGetValue(type, out ImmutableHashSet<Type> typeHierarchy))
      {
        typeHierarchy = type.GetTypeHierarchy(includeInterfaces: true).ToImmutableHashSet();
        if (includeCurrentType)
        {
          typeHierarchy = typeHierarchy.Add(type);
        }

        _ = typeHierarchyMap.TryAdd(type, typeHierarchy);
      }

      return typeHierarchy;
    }

    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<Type>> typeHierarchyMap = new ConcurrentDictionary<Type, ImmutableHashSet<Type>>();
  }
}