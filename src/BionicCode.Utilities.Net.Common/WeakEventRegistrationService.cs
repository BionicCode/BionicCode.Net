namespace BionicCode.Utilities.Net;

#region Info
// //  
// BionicUtilities.Net.Standard
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

internal class WeakEventRegistrationService
{
    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<Type>> typeHierarchies = new ConcurrentDictionary<Type, ImmutableHashSet<Type>>();
    private readonly Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>> listenerRegistrars;
    private readonly Dictionary<Type, HashSet<string>> typeToRegisteredEventsMap;
    private readonly WeakCollection<object> eventSourceInstances;
    private readonly object syncLock;

    public bool HasPublishers => eventSourceInstances.Count > 0;

    public WeakEventRegistrationService()
    {
        eventSourceInstances = new WeakCollection<object>();
        listenerRegistrars = new Dictionary<Type, Dictionary<string, List<IClientEventHandlerRegistrar>>>();
        typeToRegisteredEventsMap = new Dictionary<Type, HashSet<string>>();
        syncLock = new object();
    }

    public void AddSourceInstance(object eventSource)
    {
        lock (syncLock)
        {
            Type eventSourceType = eventSource.GetType();
            TypeData eventSourceTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventSourceType);
            IEnumerable<EventData> events = eventSourceTypeData.EnumerateEvents();
            foreach (EventData eventData in events)
            {
                Debug.Assert(eventData.GetEventInfo().ReflectedType == eventSourceType);
                AddSourceInstanceInternal(eventSource, eventSourceType, eventData.Name);
            }
        }
    }

    public void AddSourceInstance(object eventSource, string eventName)
    {
        lock (syncLock)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(eventName));

            Type eventSourceType = eventSource.GetType();
            TypeData eventSourceTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventSourceType);
            if (!eventSourceTypeData.TryGetEventByName(eventName, out EventData? eventData))
            {
                throw new ArgumentException($"The event '{eventName}' was not found on the event source {eventSourceTypeData.FullyQualifiedDisplayName} or on its declaring base eventHandlerGenericTypeDefinition.");
            }

            Debug.Assert(eventData!.GetEventInfo().ReflectedType == eventSourceType);

            AddSourceInstanceInternal(eventSource, eventSourceType, eventData.Name);
        }
    }

    private void AddSourceInstanceInternal(object eventSource, Type eventSourceType, string eventName)
    {
        if (!eventSourceInstances.Contains(eventSource))
        {
            eventSourceInstances.Add(eventSource);
        }

        if (!typeToRegisteredEventsMap.TryGetValue(eventSourceType, out HashSet<string> registeredEventsLookupTable))
        {
            registeredEventsLookupTable = new HashSet<string>();
            typeToRegisteredEventsMap.Add(eventSourceType, registeredEventsLookupTable);
        }

        _ = registeredEventsLookupTable.Add(eventName);

        RegisterListenersFor(eventSource, eventName);
    }

    public void RemoveSourceInstance(object eventSource, bool removeListeners)
    {
        lock (syncLock)
        {
            if (!eventSourceInstances.Contains(eventSource))
            {
                return;
            }

            Type eventSourceType = eventSource.GetType();
            if (!typeToRegisteredEventsMap.TryGetValue(eventSourceType, out HashSet<string> registeredEventsLookupTable))
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
        lock (syncLock)
        {
            if (!eventSourceInstances.Contains(eventSource))
            {
                return;
            }

            Type eventSourceType = eventSource.GetType();
            if (!(typeToRegisteredEventsMap.TryGetValue(eventSourceType, out HashSet<string> registeredEventsLookupTable)
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
        _ = eventSourceInstances.Remove(eventSource);
        UnregisterListenersFor(eventSource, eventName, removeListeners);
    }

    public void RegisterHandler(IClientEventHandlerRegistrar registrar)
    {
        lock (syncLock)
        {
            if (!listenerRegistrars.TryGetValue(registrar.EventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
            {
                listenersForAllEventsOfSourceType = new Dictionary<string, List<IClientEventHandlerRegistrar>>();
                listenerRegistrars.Add(registrar.EventSourceType, listenersForAllEventsOfSourceType);
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
        lock (syncLock)
        {
            Type eventSourceType = typeof(TObservedEventSource);
            if (!(listenerRegistrars.TryGetValue(eventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType)
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
        lock (syncLock)
        {
            Type eventSourceType = typeof(TObservedEventSource);
            if (!listenerRegistrars.TryGetValue(eventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
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
        lock (syncLock)
        {
            Type eventSourceType = typeof(TObservedEventSource);
            if (!(listenerRegistrars.TryGetValue(eventSourceType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType)
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
            if (listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
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
            if (listenerRegistrars.TryGetValue(supportedListenerType, out Dictionary<string, List<IClientEventHandlerRegistrar>> listenersForAllEventsOfSourceType))
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
        foreach (object eventSource in eventSourceInstances)
        {
            ImmutableHashSet<Type> supportedObservableTypes = EnsureSupportedObservableTypes(eventSource);
            if (!supportedObservableTypes.Contains(registrar.EventSourceType))
            {
                continue;
            }

            if (!(typeToRegisteredEventsMap.TryGetValue(eventSource.GetType(), out HashSet<string> registeredEventsLookupTable)
              && registeredEventsLookupTable.Contains(registrar.EventName)))
            {
                continue;
            }

            registrar.RegisterDelegate(eventSource);
        }
    }

    private void UnregisterClientHandlersFor(IClientEventHandlerRegistrar registrar)
    {
        foreach (object eventSource in eventSourceInstances)
        {
            ImmutableHashSet<Type> supportedObservableTypes = EnsureSupportedObservableTypes(eventSource);
            if (!supportedObservableTypes.Contains(registrar.EventSourceType))
            {
                continue;
            }

            if (!(typeToRegisteredEventsMap.TryGetValue(eventSource.GetType(), out HashSet<string> registeredEventsLookupTable)
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

        ImmutableHashSet<Type> supportedObservableTypes = WeakEventRegistrationService.typeHierarchies.GetOrAdd(eventSourceType,
          key =>
          {
              supportedObservableTypes = TypeHierarchyProvider.GetTypeHierarchy(eventSourceType, includeCurrentType: true);
              return supportedObservableTypes;
          });

        return supportedObservableTypes;
    }
}

internal static class TypeHierarchyProvider
{
    public static ImmutableHashSet<Type> GetTypeHierarchy(Type type, bool includeCurrentType)
    {
        ImmutableHashSet<Type> typeHierarchy = typeHierarchyMap.GetOrAdd(type,
          key =>
        {
            typeHierarchy = type.GetTypeHierarchy(includeInterfaces: true).ToImmutableHashSet();
            if (includeCurrentType)
            {
                typeHierarchy = typeHierarchy.Add(type);
            }

            return typeHierarchy;
        });

        return typeHierarchy;
    }

    private static readonly ConcurrentDictionary<Type, ImmutableHashSet<Type>> typeHierarchyMap = new ConcurrentDictionary<Type, ImmutableHashSet<Type>>();
}
