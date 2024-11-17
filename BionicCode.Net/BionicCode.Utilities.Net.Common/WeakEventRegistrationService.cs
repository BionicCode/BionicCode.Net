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
  using System.Diagnostics.Tracing;
  using System.Linq;
  using BionicCode.Utilities.Net;
  using Microsoft.Win32;

  internal class WeakEventRegistrationService
  {
    private readonly Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>> listenerRegistrars;
    private Dictionary<Type, ImmutableHashSet<Type>> typeHierarchies;
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly Lazy<ImmutableHashSet<Type>> typeHierarchyFactory;
    private readonly object syncLock;

    public bool HasPublishers => this.eventSourceInstances.Count > 0;

    public WeakEventRegistrationService()
    {
      this.eventSourceInstances = new WeakCollection<object>();
      this.listenerRegistrars = new Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>>();
      this.typeHierarchies = new Dictionary<Type, ImmutableHashSet<Type>>();
      this.syncLock = new object();
    }

    public void AddSourceInstance(object eventSource, string eventName)
    {
      lock (this.syncLock)
      {
        if (this.eventSourceInstances.Contains(eventSource))
        {
          throw new ArgumentException("The event source instance was already added.");
        }

        this.eventSourceInstances.Add(eventSource);
        RegisterListenersFor(eventSource, eventName);
      }
    }

    public void RemoveSourceInstance(object eventSource)
    {
      lock (this.syncLock)
      {
        if (!this.eventSourceInstances.Contains(eventSource))
        {
          return;
        }

        _ = this.eventSourceInstances.Remove(eventSource);
        UnregisterListenersFor(eventSource);
      }
    }

    public void RemoveAllSourceInstances<TEventSource>()
    {
      lock (this.syncLock)
      {
        foreach (object instance in this.eventSourceInstances)
        {
          _ = this.eventSourceInstances.Remove(instance);
          foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
          {
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }

    public void RegisterHandler(IClientEventHandlerRegistrar registrar)
    {
      lock (this.syncLock)
      {
        WeakEventRegistrationService.listenerRegistrars.Add(registrar);
        foreach (object instance in this.eventSourceInstances)
        {
          registrar.RegisterDelegate((TEventSource)instance);
        }
      }
    }

    public void UnregisterHandler<TEventSource>(Delegate clientHandler)
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          if (registrar.ContainsDelegate(clientHandler))
          {
            _ = WeakEventRegistrationService.listenerRegistrars.Remove(registrar);
            foreach (object instance in this.eventSourceInstances)
            {
              registrar.UnregisterDelegate((TEventSource)instance);
            }
          }
        }
      }
    }

    public void UnregisterAllHandlers()
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          _ = WeakEventRegistrationService.listenerRegistrars.Remove(registrar);
          foreach (object instance in this.eventSourceInstances)
          {
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }

    public void ClearAll()
    {
      lock (this.syncLock)
      {
        foreach (IClientEventHandlerRegistrar registrar in WeakEventRegistrationService.listenerRegistrars)
        {
          _ = WeakEventRegistrationService.listenerRegistrars.Remove(registrar);
          foreach (object instance in this.eventSourceInstances)
          {
            _ = this.eventSourceInstances.Remove(instance);
            registrar.UnregisterDelegate((TEventSource)instance);
          }
        }
      }
    }

    private void RegisterListenersFor(object eventSource, string eventName)
    {
      Type eventSourceType = eventSource.GetType();
      if (!this.typeHierarchies.TryGetValue(eventSourceType, out ImmutableHashSet<Type> supportedObservableTypes))
      {
        supportedObservableTypes = TypeHierarchyProvider.GetTypeHierarchy(eventSource.GetType(), includeCurrentType: true);
        this.typeHierarchies.Add(eventSourceType, supportedObservableTypes);
      }

      foreach (Type supportedListenerType in supportedObservableTypes)
      {
        if (listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listeners))
        {
          if (listeners.TryGetValue(eventName, out List<IClientEventHandlerRegistrar> listenersForEventName))
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

    private void RegisterGlobalListenersFor<TEventSource>(TEventSource eventSource)
    {
      if (this.typeHierarchies is null)
      {
        this.typeHierarchies = TypeHierarchyProvider.GetTypeHierarchy(eventSource.GetType(), includeCurrentType: true);
      }

      foreach (Type supportedListenerType in this.typeHierarchies)
      {
        if (listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listeners))
        {
          foreach (List<IClientEventHandlerRegistrar> registrars in listeners.Values)
          {
            foreach (IClientEventHandlerRegistrar registrar in registrars)
            {
              registrar.RegisterDelegate(eventSource);
            }
          }
        }
      }
    }

    private void UnregisterListenersFor<TEventSource>(TEventSource eventSource, string eventName)
    {
      Type eventSourceType = eventSource.GetType();
      if (!this.typeHierarchies.TryGetValue(eventSourceType, out ImmutableHashSet<Type> supportedObservableTypes))
      {
        supportedObservableTypes = TypeHierarchyProvider.GetTypeHierarchy(eventSource.GetType(), includeCurrentType: true);
        this.typeHierarchies.Add(eventSourceType, supportedObservableTypes);
      }

      foreach (Type supportedListenerType in supportedObservableTypes)
      {
        if (listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listeners))
        {
          if (listeners.TryGetValue(eventName, out List<IClientEventHandlerRegistrar> listenersForEventName))
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