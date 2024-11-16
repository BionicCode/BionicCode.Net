namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  class ExampleArgs : EventArgs
  { }
  class Example
  {
    public event Action<string, int, DateTime> SomeEvent;
    public event EventHandler<ExampleArgs> SomeOtherEvent;
  }
  /// <inheritdoc />
  public class WeakEventAggregator : IEventAggregator
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public WeakEventAggregator()
    {
      this.EventHandlerTable = new ConcurrentDictionary<string, List<Delegate>>();
      this.EventHandlerSynchronizationContextTable = new ConcurrentDictionary<Delegate, SynchronizationContext>();
      //this.EventPublisherTable = new ConditionalWeakTable<object, List<(EventInfo EventInfo, Delegate Handler)>>();
    }

    private void OnEventHandlerGeneric<TSender, TEventArgs>(TSender sender, TEventArgs e)
    { }

    private static void OnEventHandlerCustomDynamicSignature(params object[] args)
    {
      // TODO::Invoke client handler using reflection
    }

    #region Implementation of IEventAggregator

    /// <inheritdoc />
    public void RegisterObservable<TEventSource>(object eventSource, params string[] eventNames)
      => RegisterObservable(eventSource, (IEnumerable<string>)eventNames);

    /// <inheritdoc />
    public void RegisterObservable<TEventSource>(TEventSource eventSource, IEnumerable<string> eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      Type eventSourceType = eventSource.GetType();
      foreach (string eventName in eventNames)
      {
        var key = new EventInfoTableKey<TEventSource>(eventName);
        if (!(WeakEventAggregator.SourceEventInfoTable.TryGetValue(key, out object entry)
          && entry is EventInfoTableEntry<TEventSource> eventInfoTableEntry))
        {
          EventInfo eventInfo = eventSourceType.GetEvent(
            eventName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)
              ?? throw new ArgumentException($"The event {eventName} was not found on the event source {eventSource.GetType().FullName} or on its declaring base eventHandlerGenericTypeDefinition.");
          eventInfoTableEntry = new EventInfoTableEntry<TEventSource>(eventInfo);
          _ = WeakEventAggregator.SourceEventInfoTable.TryAdd(key, eventInfoTableEntry);
        }

        eventInfoTableEntry.RegistrationService.AddSourceInstance(eventSource);
      }
    }

    /// <inheritdoc />
    public bool TryRemoveObservable(Type eventSourceType, bool removeEventObservers = false, bool includeInstanceEvents = false)
      => TryRemoveObservableInternal(null, eventSourceType, null, removeEventObservers, includeInstanceEvents, includeStaticEvents: true);

    /// <inheritdoc />
    public bool TryRemoveObservable(Type eventSourceType, bool removeEventObservers = false, bool includeInstanceEvents = false, params string[] eventNames)
      => TryRemoveObservableInternal(null, eventSourceType, eventNames, removeEventObservers, includeInstanceEvents, includeStaticEvents: true);

    /// <inheritdoc />
    public bool TryRemoveObservable(Type eventSourceType, IEnumerable<string> eventNames, bool removeEventObservers = false, bool includeInstanceEvents = false)
      => TryRemoveObservableInternal(null, eventSourceType, eventNames, removeEventObservers, includeInstanceEvents, includeStaticEvents: true);

    /// <inheritdoc />
    public bool TryRemoveObservable<TEventSource>(bool removeEventObservers = false, bool includeInstanceEvents = false)
      => TryRemoveObservableInternal(null, typeof(TEventSource), null, removeEventObservers, includeInstanceEvents, includeStaticEvents: true);

    /// <inheritdoc />
    public bool TryRemoveObservable<TEventSource>(bool removeEventObservers = false, bool includeInstanceEvents = false, params string[] eventNames)
      => TryRemoveObservableInternal(null, typeof(TEventSource), eventNames, removeEventObservers, includeInstanceEvents, includeStaticEvents: true);

    /// <inheritdoc />
    public bool TryRemoveObservable<TEventSource>(IEnumerable<string> eventNames, bool removeEventObservers = false, bool includeInstanceEvents = false)
      => TryRemoveObservableInternal(null, typeof(TEventSource), eventNames, removeEventObservers, includeInstanceEvents, includeStaticEvents: true);

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, bool removeAllEventObservers = false, bool includeStaticEvents = false, params string[] eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      return TryRemoveObservableInternal(eventSource, eventSource.GetType(), eventNames, removeAllEventObservers, includeInstanceEvents: true, includeStaticEvents);
    }

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, IEnumerable<string> eventNames, bool removeAllEventObservers = false, bool includeStaticEvents = false)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      return TryRemoveObservableInternal(eventSource, eventSource.GetType(), eventNames, removeAllEventObservers, includeInstanceEvents: true, includeStaticEvents);
    }

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, bool removeAllObserversOfEvents = false, bool includeStaticEvents = false)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      return TryRemoveObservableInternal(eventSource, eventSource.GetType(), null, removeAllObserversOfEvents, includeInstanceEvents: true, includeStaticEvents);
    }

    private void RemoveObservableInternal<TEventSource>(TEventSource eventSource, Type eventSourceType, IEnumerable<string> eventNames, bool removeAllEventObservers, bool includeInstanceEvents, bool includeStaticEvents)
    {
      if (eventNames is null)
      {
        eventNames = eventSourceType.GetEvents()
          .Select(eventInfo => eventInfo.Name);
      }

      foreach (string eventName in eventNames)
      {
        var key = new EventInfoTableKey<TEventSource>(eventName);
        if (!(WeakEventAggregator.SourceEventInfoTable.TryGetValue(key, out object entry)
          && entry is EventInfoTableEntry<TEventSource> eventInfoTableEntry))
        {
          continue;
        }

        eventInfoTableEntry.RegistrationService.RemoveSourceInstance(eventSource);
      }
    }

    //private void RemoveHandlerInternal<TEventSource>(string eventName, Type eventHandlerType, Delegate handler, object eventSourceInstance)
    //{
    //  var key = new EventInfoTableKey<TEventSource>(eventName);
    //  if (!(WeakEventAggregator.SourceEventInfoTable.TryGetValue(key, out object entry) 
    //    && entry is EventInfoTableEntry<TEventSource> eventInfoTableEntry))
    //  {
    //    return;
    //  }

    //  eventInfoTableEntry.RemoveRegistration
    //    if (eventHandlerType == typeof(EventHandler)
    //    || eventHandlerType == typeof(EventHandler<EventArgs>)
    //    || eventHandlerType == typeof(Action<object, EventArgs>))
    //  {
    //    WeakEventManager<object, EventArgs>.RemoveEventHandler(eventSourceInstance, eventName, handler);
    //  }
    //  else if (eventHandlerType == typeof(EventHandler<object>)
    //    || eventHandlerType == typeof(Action<object, object>))
    //  {
    //    WeakEventManager<object, object>.RemoveEventHandler(eventSourceInstance, eventName, handler);
    //  }
    //  else
    //  {
    //    MethodInfo closedHandlerMethod = null;
    //    Type eventArgsType = null;
    //    Type eventHandlerSourceType = null;
    //    Type eventHandlerGenericTypeDefinition = eventHandlerType.GetGenericTypeDefinition();
    //    if ((eventHandlerGenericTypeDefinition == typeof(EventHandler<>)))
    //    {
    //      Type[] typeArguments = eventHandlerType.GetGenericArguments();
    //      closedHandlerMethod = GetType().GetMethod(nameof(OnEventHandlerGeneric)).MakeGenericMethod(typeArguments);
    //      eventHandlerSourceType = typeof(object);
    //      eventArgsType = typeArguments[0];
    //    }
    //    else
    //    {
    //      MethodInfo invocator = eventHandlerType.GetMethod("Invoke");
    //      ParameterInfo[] eventHandlerParameters = invocator.GetParameters();
    //      if (eventHandlerParameters.Length == 2)
    //      {
    //        Type[] parameterTypes = eventHandlerParameters.Select(parameter => parameter.ParameterType).ToArray();
    //        eventHandlerSourceType = parameterTypes[0];
    //        eventArgsType = parameterTypes[1];
    //      }
    //      else
    //      {
    //        eventHandlerSourceType = eventHandlerParameters[0].ParameterType;
    //        eventArgsType = typeof(object[]);
    //      }
    //    }

    //    Type closedWeakEventManagerType = typeof(WeakEventManager<>).MakeGenericType(eventHandlerSourceType);
    //    MethodInfo weakEventManagerAddHandlerMethodInfo = closedWeakEventManagerType.GetMethod("RemoveEventHandler");
    //    _ = weakEventManagerAddHandlerMethodInfo.Invoke(null, new object[] { eventSourceInstance, eventName, handler });
    //  }
    //}

    /// <inheritdoc />
    public void RegisterObserver<TEventSource>(string eventName, Delegate eventHandler)
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      RegisterObserverInternal<TEventSource>(eventName, eventHandler, false, null);
    }

    /// <inheritdoc />
    public void RegisterObserver<TEventSource>(string eventName, Delegate eventHandler, bool executeOnCurrentSynchronizationContext)
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));

      RegisterObserverInternal<TEventSource>(eventName, eventHandler, executeOnCurrentSynchronizationContext, null);
    }

    /// <inheritdoc />
    public void RegisterObserver<TEventSource>(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext)
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
      ArgumentNullExceptionEx.ThrowIfNull(eventHandler, nameof(eventHandler));
      ArgumentNullExceptionEx.ThrowIfNull(synchronizationContext, nameof(synchronizationContext));

      RegisterObserverInternal<TEventSource>(eventName, eventHandler, false, synchronizationContext);
    }

    public void RegisterObserverInternal<TEventSource>(string eventName, Delegate eventHandler, bool executeOnCurrentSynchronizationContext, SynchronizationContext synchronizationContext)
    {
      Type eventSourceType = typeof(TEventSource);
      var key = new EventInfoTableKey<TEventSource>(eventName);
      if (!(WeakEventAggregator.SourceEventInfoTable.TryGetValue(key, out object entry) && entry is EventInfoTableEntry<TEventSource> eventInfoTableEntry))
      {
        EventInfo eventInfo = eventSourceType.GetEvent(
          eventName,
          BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)
        ?? throw new ArgumentException($"The event {eventName} was not found on the event source {eventSourceType.FullName} or on its declaring base eventHandlerGenericTypeDefinition.");
        eventInfoTableEntry = new EventInfoTableEntry<TEventSource>(eventInfo);
        _ = WeakEventAggregator.SourceEventInfoTable.TryAdd(key, eventInfo);
      }

      ThrowIfEventHandlerInvalid(eventInfoTableEntry, eventHandler);

      Type eventHandlerType = eventHandler.GetType();
      IClientEventHandlerRegistrar<TEventSource> clientEventHandlerRegistrar;
      bool isGenericEventHandler = eventHandlerType.IsGenericType;
      Type eventHandlerTypeDefinition = isGenericEventHandler ? eventHandlerType.GetGenericTypeDefinition() : null;
      if (eventHandlerType == typeof(EventHandler))
      {
        clientEventHandlerRegistrar = new EventHandlerRegistrar<TEventSource>((EventHandler)eventHandler, eventName, synchronizationContext);
      }
      else if (isGenericEventHandler && eventHandlerTypeDefinition == typeof(EventHandler<>))
      {
        Type eventArgsType = eventHandlerType.GetGenericArguments()[0];
        clientEventHandlerRegistrar = (IClientEventHandlerRegistrar<TEventSource>)typeof(EventHandlerGenericRegistrar<,>).MakeGenericType(typeof(TEventSource), eventArgsType)
          .GetConstructor(new Type[] { eventHandlerType, typeof(string) })
          .Invoke(new object[] { eventHandler, eventName, synchronizationContext });
      }
      else if (isGenericEventHandler && eventHandlerTypeDefinition == typeof(Action<,>))
      {
        Type eventSenderType = eventHandlerType.GetGenericArguments()[0];
        Type eventArgsType = eventHandlerType.GetGenericArguments()[1];
        clientEventHandlerRegistrar = (IClientEventHandlerRegistrar<TEventSource>)typeof(ActionRegistrar<,,>).MakeGenericType(typeof(TEventSource), eventSenderType, eventArgsType)
          .GetConstructor(new Type[] { eventHandlerType, typeof(string) })
          .Invoke(new object[] { eventHandler, eventName, synchronizationContext });
      }
      else
      {
        clientEventHandlerRegistrar = new AnonymousDelegateRegistrar<TEventSource>(eventHandler, eventName);
        //Type eventArgsType = null;
        //Type eventHandlerSourceType = typeof(TEventSource);
        //Type eventHandlerGenericTypeDefinition = eventHandlerType.GetGenericTypeDefinition();
        //if (eventHandlerGenericTypeDefinition == typeof(EventHandler<>))
        //{
        //  Type[] typeArguments = eventHandlerType.GetGenericArguments();
        //  eventArgsType = typeArguments[0];
        //}
        //else
        //{
        //  MethodInfo invocator = eventHandlerType.GetMethod("Invoke");
        //  ParameterInfo[] eventHandlerParameters = invocator.GetParameters();
        //  eventArgsType = typeof(object[]);
        //}

        //Type closedWeakEventManagerType = typeof(WeakEventManager<,>).MakeGenericType(eventHandlerSourceType, eventArgsType);
        //MethodInfo weakEventManagerAddHandlerMethodInfo = closedWeakEventManagerType.GetMethod("AddEventHandler");
        //if (synchronizationContext != null)
        //{
        //  subscribeDelegate = eventSource => _ = weakEventManagerAddHandlerMethodInfo.Invoke(null, new object[] { eventSource, eventName, eventHandler, synchronizationContext });
        //}
        //else
        //{
        //  subscribeDelegate = eventSource => _ = weakEventManagerAddHandlerMethodInfo.Invoke(null, new object[] { eventSource, eventName, eventHandler, executeOnCurrentSynchronizationContext });
        //}

        //MethodInfo weakEventManagerRemoveHandlerMethodInfo = closedWeakEventManagerType.GetMethod("RemoveEventHandler");
        //unsubscribeDelegate = eventSource => _ = weakEventManagerRemoveHandlerMethodInfo.Invoke(null, new object[] { eventSource, eventName, eventHandler });
      }

      eventInfoTableEntry.RegistrationService.RegisterHandler(clientEventHandlerRegistrar);
    }

    private void ThrowIfEventHandlerInvalid<TEventSource>(EventInfoTableEntry<TEventSource> entry, Delegate eventHandler)
    {
      ParameterInfo[] invocatorParameters = entry.InvocatorMethod.GetParameters();
      ParameterInfo[] eventHandlerParameters = eventHandler.GetType().GetMethod("Invoke")?.GetParameters();
      if (eventHandlerParameters != null)
      {
        if (invocatorParameters.Length != eventHandlerParameters.Length)
        {
          throw new EventHandlerMismatchException($"Wrong event handler signature. The parameter count of the registered event handler does not match the event delegate {entry.EventInfo.EventHandlerType.FullName}.");
        }

        for (int index = 0; index < invocatorParameters.Length; index++)
        {
          Type invocatorParameterType = invocatorParameters[index].ParameterType;
          Type eventHandlerParameterType = eventHandlerParameters[index].ParameterType;
          if (!eventHandlerParameterType.IsAssignableFrom(invocatorParameterType))
          {
            throw new EventHandlerMismatchException($"Wrong event handler signature. The parameter eventHandlerTypeDefinition at index {index} of the registered event handler does not match the event delegate {entry.EventInfo.EventHandlerType.FullName}. Found eventHandlerTypeDefinition {eventHandlerParameterType.FullName}. Expected eventHandlerTypeDefinition {invocatorParameterType.FullName}.");
          }
        }
      }
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return isMarshalEventToCurrentThreadEnabled
         ? TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, SynchronizationContext.Current)
         : TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(string eventName, Delegate eventHandler, SynchronizationContext synchronizationContext)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, synchronizationContext);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return isMarshalEventToCurrentThreadEnabled
         ? TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, SynchronizationContext.Current)
         : TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, synchronizationContext);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return isMarshalEventToCurrentThreadEnabled
         ? TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, SynchronizationContext.Current)
         : TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver(Delegate eventHandler, SynchronizationContext synchronizationContext)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, synchronizationContext);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return isMarshalEventToCurrentThreadEnabled
         ? TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, SynchronizationContext.Current)
         : TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, synchronizationContext);
    }

    /// <inheritdoc />
    public void RemoveObserver<TEventSource>(string eventName, Delegate eventHandler)
    {
      var key = new EventInfoTableKey(eventName, typeof(TEventSource));
      if (!WeakEventAggregator.SourceEventInfoTable.TryGetValue(key, out EventInfoTableEntry entry))
      {
        return;
      }


      string fullyQualifiedEventIdOfSpecificSource =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfSpecificSource, out _);
    }

    /// <inheritdoc />
    public bool TryRemoveObserver(string eventName, Type eventSourceType, Delegate eventHandler)
    {
      string fullyQualifiedEventIdOfSpecificSource =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfSpecificSource, out _);
    }

    /// <inheritdoc />
    public bool TryRemoveObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler) 
      => TryRemoveObserver(eventName, eventSourceType, (Delegate)eventHandler);

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver(string eventName, Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());

      string fullyQualifiedEventIdOfGlobalSource =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfGlobalSource, out _);
    }

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver<TEventArgs>(string eventName, EventHandler<TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());

      string fullyQualifiedEventIdOfGlobalSource =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
      return this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfGlobalSource, out _);
    }

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver(Delegate eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType(eventHandler.GetType());
      return TryRemoveGlobalObserverInternal(normalizedEventHandlerType);
    }

    /// <inheritdoc />
    public bool TryRemoveGlobalObserver<TEventArgs>(EventHandler<TEventArgs> eventHandler)
    {
      Type normalizedEventHandlerType = NormalizeEventHandlerType<TEventArgs>(eventHandler.GetType());
      return TryRemoveGlobalObserverInternal(normalizedEventHandlerType);
    }

    /// <inheritdoc />
    public bool TryRemoveAllObservers(string eventName, Type eventSourceType)
    {
      string fullyQualifiedEventIdOfSpecificSource =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      if (this.EventHandlerTable.TryRemove(fullyQualifiedEventIdOfSpecificSource,
        out List<Delegate> removedDelegates))
      {
        foreach (Delegate eventHandler in removedDelegates)
        {
          _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
        }

        return true;
      }

      return false;
    }

    /// <inheritdoc />
    public bool TryRemoveAllObservers(Type eventSourceType)
    {
      bool result = false;
      string fullyQualifiedEventIdOfSpecificSourcePrefix =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, string.Empty);
      for (int index = this.EventHandlerTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<string, List<Delegate>> handlersEntry = this.EventHandlerTable.ElementAt(index);
        if (handlersEntry.Key.StartsWith(fullyQualifiedEventIdOfSpecificSourcePrefix, StringComparison.Ordinal))
        {
          result |= this.EventHandlerTable.TryRemove(handlersEntry.Key, out _);
          foreach (Delegate eventHandler in handlersEntry.Value)
          {
            _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
          }
        }
      }

      return result;
    }

    /// <inheritdoc />
    public bool TryRemoveAllObservers(string eventName)
    {
      bool result = false;
      string fullyQualifiedEventIdSuffix = $".{eventName}";
      for (int index = this.EventHandlerTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<string, List<Delegate>> handlersEntry = this.EventHandlerTable.ElementAt(index);
        if (handlersEntry.Key.EndsWith(fullyQualifiedEventIdSuffix, StringComparison.Ordinal))
        {
          result |= this.EventHandlerTable.TryRemove(handlersEntry.Key, out _);
          foreach (Delegate eventHandler in handlersEntry.Value)
          {
            _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
          }
        }
      }

      return result;
    }

#endregion Implementation of IEventAggregator

    private bool TryRegisterObserverInternal(
      Delegate eventHandler,
      string fullyQualifiedEventName)
    {
      if (this.EventHandlerTable.TryGetValue(fullyQualifiedEventName, out List<Delegate> handlers))
      {
        handlers.Add(eventHandler);
        return true;
      }

      return this.EventHandlerTable.TryAdd(fullyQualifiedEventName, new List<Delegate>() { eventHandler });
    }

    private bool TryRegisterObserverInternal(
      Delegate eventHandler,
      string fullyQualifiedEventName,
      SynchronizationContext synchronizationContext)
    {
      _ = this.EventHandlerSynchronizationContextTable.TryAdd(eventHandler, synchronizationContext);
      if (this.EventHandlerTable.TryGetValue(fullyQualifiedEventName, out List<Delegate> handlers))
      {
        handlers.Add(eventHandler);
        return true;
      }

      return this.EventHandlerTable.TryAdd(fullyQualifiedEventName, new List<Delegate>() { eventHandler });
    }

    private bool TryRemoveGlobalObserverInternal(Type normalizedEventHandlerType)
    {
      bool result = false;
      string fullyQualifiedEventIdOfGlobalSourcePrefix =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      for (int index = this.EventHandlerTable.Count - 1; index >= 0; index--)
      {
        KeyValuePair<string, List<Delegate>> handlersEntry = this.EventHandlerTable.ElementAt(index);
        if (handlersEntry.Key.StartsWith(fullyQualifiedEventIdOfGlobalSourcePrefix, StringComparison.Ordinal))
        {
          result |= this.EventHandlerTable.TryRemove(handlersEntry.Key, out _);
          foreach (Delegate eventHandler in handlersEntry.Value)
          {
            _ = this.EventHandlerSynchronizationContextTable.TryRemove(eventHandler, out _);
          }
        }
      }

      return result;
    }

    private void DelegateHandleEvent((Type EventHandlerType, ICollection<string> EventIds) eventInfo, object sender, object args)
    {
      IEnumerable<Delegate> handlers = eventInfo.EventIds
        .SelectMany(
          eventId => this.EventHandlerTable.TryGetValue(eventId, out List<Delegate> delegates)
            ? delegates
            : new List<Delegate>());

      foreach (Delegate handler in handlers)
      {
        try
        {
          if (this.EventHandlerSynchronizationContextTable.TryGetValue(handler, out SynchronizationContext synchronizationContext))
          {
            synchronizationContext.Post(new SendOrPostCallback(param => handler.DynamicInvoke(sender, args)), null);
          }
          else
          {
            _ = handler.DynamicInvoke(sender, args);
          }
        }
        catch (ArgumentException e)
        {
          MethodInfo delegateInvokeMethodInfo = handler.GetType().GetMethod("Invoke");
          string handlerSignatureParameterList = delegateInvokeMethodInfo?
            .GetParameters()
            .Select(parameterInfo => parameterInfo.ParameterType.FullName)
            .Aggregate((result, current) => result += ", " + current).TrimEnd(',', ' ');

          throw new WrongEventHandlerSignatureException(
            $"The found callback signature does not match the registered delegate{System.Environment.NewLine}'{FormatTypeName(handler.GetType())}'. {System.Environment.NewLine}{System.Environment.NewLine}Expected: '{delegateInvokeMethodInfo.ReturnType.Name} {handler.Method.Name}({handlerSignatureParameterList})'.{System.Environment.NewLine}Actual: '{handler.Method}'.", e);
        }
      }
    }

    private string FormatTypeName(Type typeToFormat, bool isFullyQualified = true)
    {
      if (!typeToFormat.IsGenericType)
      {
        return isFullyQualified ? typeToFormat.FullName : typeToFormat.Name;
      }

      Type[] genericArguments = typeToFormat.GetGenericArguments();
      string originalTypeName = isFullyQualified ? typeToFormat.FullName : typeToFormat.Name;
      return originalTypeName.Substring(0, originalTypeName.IndexOf("`", StringComparison.OrdinalIgnoreCase)) + "<" + String.Join(", ", genericArguments.Select(type => FormatTypeName(type))) + ">";
    }

    private bool TryCreateEventIdOfInterfaceType(object eventSource, string eventName, out string eventId)
    {
      eventId = string.Empty;

      Type eventSourceInterfaceType = eventSource.GetType().GetInterfaces().FirstOrDefault(
        interfaceType => interfaceType.GetEvent(
          eventName,
          BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance |
          BindingFlags.Static) != null);

      if (eventSourceInterfaceType != null)
      {
        string fullyQualifiedInterfaceEventIdOfSpecificEvent =
          CreateFullyQualifiedEventIdOfSpecificSource(eventSourceInterfaceType, eventName);
        eventId = fullyQualifiedInterfaceEventIdOfSpecificEvent;
      }

      return !string.IsNullOrWhiteSpace(eventId);
    }

    private List<string> CreateEventIdsOfConcreteType(
      object eventSource,
      Type normalizedEventHandlerType,
      string eventName)
    {
      string fullyQualifiedEventIdOfGlobalEvent =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, eventName);
      string fullyQualifiedEventIdOfUnknownGlobalEvent =
        CreateFullyQualifiedEventIdOfGlobalSource(normalizedEventHandlerType, string.Empty);
      string fullyQualifiedImplementationEventIdOfSpecificEvent =
        CreateFullyQualifiedEventIdOfSpecificSource(eventSource.GetType(), eventName);

      var eventIds = new List<string>
      {
        fullyQualifiedImplementationEventIdOfSpecificEvent, fullyQualifiedEventIdOfGlobalEvent,
        fullyQualifiedEventIdOfUnknownGlobalEvent
      };
      return eventIds;
    }

    private Type NormalizeEventHandlerType(Type eventHandlerType) => eventHandlerType == typeof(EventHandler) || eventHandlerType == typeof(Action<object, EventArgs>)
      ? typeof(EventHandler<EventArgs>)
      : eventHandlerType;

    private Type NormalizeEventHandlerType<TEventArgs>(Type eventHandlerType) => eventHandlerType == typeof(EventHandler) || eventHandlerType == typeof(Action<object, EventArgs>)
      ? typeof(EventHandler<EventArgs>)
      : eventHandlerType == typeof(EventHandler<TEventArgs>)
        ? typeof(EventHandler<TEventArgs>)
        : eventHandlerType;

    private string CreateFullyQualifiedEventIdOfGlobalSource(Type eventHandlerType, string eventName) => eventHandlerType.FullName.ToLowerInvariant() + "." + eventName;

    private string CreateFullyQualifiedEventIdOfSpecificSource(Type eventSource, string eventName) => eventSource.AssemblyQualifiedName.ToLowerInvariant() + "." + eventSource.FullName.ToLowerInvariant() + "." + eventName;

    private static ConcurrentDictionary<object, object> SourceEventInfoTable { get; } = new ConcurrentDictionary<EventInfoTableKey, EventInfoTableEntry>();
    private ConcurrentDictionary<string, List<Delegate>> EventHandlerTable { get; }
    private ConcurrentDictionary<Delegate, SynchronizationContext> EventHandlerSynchronizationContextTable { get; }
    //private ConditionalWeakTable<object, List<(EventInfo EventInfo, Delegate Handler)>> EventPublisherTable { get; }
  }
}