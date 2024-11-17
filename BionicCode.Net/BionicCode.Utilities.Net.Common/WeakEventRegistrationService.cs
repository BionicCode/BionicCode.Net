namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
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
    private readonly Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>> listenerRegistrars;
    private static readonly Dictionary<Type, Dictionary<string, EventInfo>> typeEventInfosMap = new Dictionary<Type, Dictionary<string, EventInfo>>();
    private static readonly Dictionary<Type, ImmutableHashSet<Type>> typeHierarchies = new Dictionary<Type, ImmutableHashSet<Type>>();
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly object syncLock;

    public bool HasPublishers => this.eventSourceInstances.Count > 0;

    public WeakEventRegistrationService()
    {
      this.eventSourceInstances = new WeakCollection<object>();
      this.listenerRegistrars = new Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>>();
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

        Debug.Assert(eventInfo.DeclaringType == eventSourceType);

        AddSourceInstanceInternal(eventSource, eventInfo);
      }
    }

    public void AddSourceInstanceInternal(object eventSource, EventInfo eventInfo)
    {
      Type eventSourceType = eventSource.GetType();
      Debug.Assert(eventInfo.DeclaringType == eventSourceType);

      if (this.eventSourceInstances.Contains(eventSource))
      {
        throw new ArgumentException("The event source instance was already added.");
      }

      if (!WeakEventRegistrationService.typeEventInfosMap.TryGetValue(eventSourceType, out Dictionary<string, EventInfo> eventInfoMap))
      {
        eventInfoMap = new Dictionary<string, EventInfo>();
        typeEventInfosMap.Add(eventSourceType, eventInfoMap);
      }

      if (!eventInfoMap.TryGetValue(eventInfo.Name, out _))
      {
        eventInfoMap.Add(eventInfo.Name, eventInfo);
      }

      this.eventSourceInstances.Add(eventSource);
      RegisterListenersFor(eventSource, eventInfo.Name);
    }

    public void RemoveSourceInstance(object eventSource)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(eventSource))
        {
          return;
        }

        Type eventSourceType = eventSource.GetType();
        if (!WeakEventRegistrationService.typeEventInfosMap.TryGetValue(eventSourceType, out Dictionary<string, EventInfo> eventInfoMap))
        {
          return;
        }

        foreach (KeyValuePair<string, EventInfo> eventInfoMapEntry in eventInfoMap)
        {
          RemoveSourceInstanceInternal(eventSource, eventInfoMapEntry.Value.Name);
        }
      }
    }

    public void RemoveSourceInstance(object eventSource, string eventName)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(eventSource))
        {
          return;
        }

        Type eventSourceType = eventSource.GetType();
        if (!WeakEventRegistrationService.typeEventInfosMap.TryGetValue(eventSourceType, out Dictionary<string, EventInfo> eventInfoMap))
        {
          return;
        }

        RemoveSourceInstanceInternal(eventSource, eventName);
      }
    }

    private void RemoveSourceInstanceInternal(object eventSource, string eventName)
    {
      _ = this.eventSourceInstances.Remove(eventSource);
      UnregisterListenersFor(eventSource, eventName);
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

        foreach (IClientEventHandlerRegistrar registrar in listenersForEventName)
        {
          if (registrar.TryGetClientHandler(out Delegate storedDelegate) && Delegate.Equals(storedDelegate, clientHandler))
          {
            UnregisterClientHandlersFor(registrar);
          }
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

    private ImmutableHashSet<Type> EnsureSupportedObservableTypes(object eventSource)
    {
      Type eventSourceType = eventSource.GetType();
      if (!WeakEventRegistrationService.typeHierarchies.TryGetValue(eventSourceType, out ImmutableHashSet<Type> supportedObservableTypes))
      {
        supportedObservableTypes = TypeHierarchyProvider.GetTypeHierarchy(eventSourceType, includeCurrentType: true);
        WeakEventRegistrationService.typeHierarchies.Add(eventSourceType, supportedObservableTypes);
      }

      return supportedObservableTypes;
    }

    private void UnregisterListenersFor<TEventSource>(TEventSource eventSource, string eventName)
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
                registrar.UnregisterDelegate(eventSource);
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

        if (!(WeakEventRegistrationService.typeEventInfosMap.TryGetValue(eventSource.GetType(), out Dictionary<string, EventInfo> eventInfoMap) 
          && eventInfoMap.ContainsKey(registrar.EventName)))
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

        if (!(WeakEventRegistrationService.typeEventInfosMap.TryGetValue(eventSource.GetType(), out Dictionary<string, EventInfo> eventInfoMap)
          && eventInfoMap.ContainsKey(registrar.EventName)))
        {
          continue;
        }

        registrar.UnregisterDelegate(eventSource);
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

        typeHierarchyMap.Add(type, typeHierarchy);
      }

      return typeHierarchy;
    }

    private static readonly Dictionary<Type, ImmutableHashSet<Type>> typeHierarchyMap = new Dictionary<Type, ImmutableHashSet<Type>>();
  }
}