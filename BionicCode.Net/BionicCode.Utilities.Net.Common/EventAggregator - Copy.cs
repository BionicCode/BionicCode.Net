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

  class ExanpleArgs : EventArgs
  { }
  class Example
  {
    public event Action<string, int, DateTime> SomeEvent;
    public event EventHandler<ExanpleArgs> SomeOtherEvent;
  }
  /// <inheritdoc />
  public class EventAggregatorNew<TSource> : IEventAggregator
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public EventAggregatorNew()
    {
      this.EventHandlerTable = new ConcurrentDictionary<string, List<Delegate>>();
      this.EventHandlerSynchronizationContextTable = new ConcurrentDictionary<Delegate, SynchronizationContext>();
      //this.EventPublisherTable = new ConditionalWeakTable<object, List<(EventInfo EventInfo, Delegate Handler)>>();
    }
    
    private void OnEventHandler(object sender, EventArgs e)
    { }

    private void OnEventHandlerGeneric<TEventArgs>(object sender, TEventArgs e)
    { }

    private void OnEventHandlerCustom<TSender, TEventArgs>(TSender sender, TEventArgs e)
    { }

    private static void OnEventHandlerCustomUnspecified(params object[] args)
    { }

    #region Implementation of IEventAggregator

    /// <inheritdoc />
    public bool TryRegisterObservable(TSource eventSource, params string[] eventNames)
      => TryRegisterObservable(eventSource, (IEnumerable<string>)eventNames);

    /// <inheritdoc />
    public bool TryRegisterObservable(TSource eventSource, IEnumerable<string> eventNames)
    {
      ArgumentNullExceptionEx.ThrowIfNull(eventSource, nameof(eventSource));
      ArgumentNullExceptionEx.ThrowIfNull(eventNames, nameof(eventNames));

      Type eventSourceType = eventSource.GetType();
      Delegate eventSourceHandler = null;
      foreach (string eventName in eventNames.Distinct())
      {
        var key = new EventHandlerTableKey(eventName, eventSourceType);
        if (EventAggregatorNew<TSource>.SourceEventInfoTable.TryGetValue(key, out EventInfoTableEntry entry))
        {
          if (entry.EventSourceInstances.Contains(eventSource))
          {
            continue;
          }
          else
          {
            entry.EventSourceInstances.Add(eventSource);
            entry.SourceEventInfo.AddEventHandler(eventSource, entry.GeneratedHandler);

            continue;
          }
        }

        entry = new EventInfoTableEntry(eventSource);
        _ = EventAggregatorNew<TSource>.SourceEventInfoTable.TryAdd(key, entry);

        EventInfo eventInfo = eventSource.GetType()
          .GetEvent(
            eventName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy)
          ?? throw new ArgumentException($"The event {eventName} was not found on the event source {eventSource.GetType().FullName} or on its declaring base type.");

        Type eventHandlerType = eventInfo.EventHandlerType;
        if (eventHandlerType == typeof(EventHandler))
        {
          eventSourceHandler = new EventHandler(OnEventHandler);
        }
        else if (eventHandlerType == typeof(EventHandler<EventArgs>))
        {
          eventSourceHandler = new EventHandler<EventArgs>(OnEventHandler);
        }
        else if (eventHandlerType == typeof(Action<object, EventArgs>))
        {
          eventSourceHandler = new Action<object, EventArgs>(OnEventHandler);
        }
        else
        {
          MethodInfo closedHandlerMethod = null;
          if (eventHandlerType.IsGenericType)
          {
            Type[] typeArguments = eventHandlerType.GetGenericArguments();
            if (eventHandlerType.GetGenericTypeDefinition() == typeof(EventHandler<>))
            {
              if (typeof(EventArgs).IsAssignableFrom(typeArguments[0]))
              {
                closedHandlerMethod = GetType().GetMethod(nameof(OnEventHandler));
              }
              else
              {
                closedHandlerMethod = GetType().GetMethod(nameof(OnEventHandlerGeneric)).MakeGenericMethod(typeArguments);
              }
            }
            else if (typeArguments.Length == 2)
            {
              closedHandlerMethod = GetType().GetMethod(nameof(OnEventHandlerCustom)).MakeGenericMethod(typeArguments);
            }
          }
          else
          {
            MethodInfo invocator = eventHandlerType.GetMethod("Invoke");
            ParameterInfo[] eventHandlerParameters = invocator.GetParameters();
            if (eventHandlerParameters.Length == 2)
            {
              Type[] parameterTypes = eventHandlerParameters.Select(parameter => parameter.ParameterType).ToArray();
              closedHandlerMethod = GetType().GetMethod(nameof(OnEventHandlerCustom)).MakeGenericMethod(parameterTypes);
            }
            else
            {
              eventSourceHandler = GenerateEventHandler(eventHandlerParameters);
            }
          }

          if (closedHandlerMethod != null)
          {
            eventSourceHandler = Delegate.CreateDelegate(
              eventHandlerType,
              this,
              closedHandlerMethod);
          }
        }

        WeakEventManager<
        entry.SourceEventInfo.AddEventHandler(eventSource, eventSourceHandler);
      }

      return true;
    }

    private Delegate GenerateEventHandler(ParameterInfo[] eventHandlerParameters)
    {
      Delegate eventSourceHandler;
      var expressionParameters = new List<ParameterExpression>();
      foreach (ParameterInfo parameter in eventHandlerParameters)
      {
        ParameterExpression expressionParameter = Expression.Parameter(parameter.ParameterType, parameter.Name);
        expressionParameters.Add(expressionParameter);
      }

      IEnumerable<UnaryExpression> castedExpressionParameters = expressionParameters.Select(parameter => Expression.TypeAs(parameter, typeof(object)));
      NewArrayExpression argsArray = Expression.NewArrayInit(typeof(object), castedExpressionParameters);
      MethodCallExpression method = Expression.Call(GetType().GetMethod(nameof(OnEventHandlerCustomUnspecified)), argsArray);
      eventSourceHandler = Expression.Lambda(method, expressionParameters).Compile();
      return eventSourceHandler;
    }

#if NET || NETSTANDARD2_1_OR_GREATER || NETCOREAPP
    /// <inheritdoc />
    public bool TryRemoveObservable(Type eventSourceType, bool removeEventObservers = false, params string[] eventNames)
      => TryRemoveObservable(eventSourceType, eventNames, removeEventObservers);

    /// <inheritdoc />
    public bool TryRemoveObservable(Type eventSourceType, IEnumerable<string> eventNames, bool removeEventObservers = false)
    {
      bool hasRemovedObservable = false;
      foreach (string eventName in eventNames)
      {
        var key = new EventHandlerTableKey(eventName, eventSourceType);
        if (!EventAggregatorNew.GeneratedEventHandlerTable.TryRemove(key, out IList<EventInfoTableEntry> entriesPerInstance) || entriesPerInstance.IsEmpty())
        {
          continue;
        }

        foreach (EventInfoTableEntry instanceEntry in entriesPerInstance)
        {
          if (!instanceEntry.EventSourceInstances.TryGetTarget(out object instance))
          {
            continue;
          }

          instanceEntry.SourceEventInfo.RemoveEventHandler(instance, instanceEntry.GeneratedHandler);
          hasRemovedObservable = true;

          if (removeEventObservers)
          {
            _ = TryRemoveAllObservers(eventName, instance.GetType());
          }
        }
      }

      return hasRemovedObservable;
    }

#endif

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, bool removeEventObservers = false, params string[] eventNames)
      => TryRemoveObservable(eventSource, eventNames, removeEventObservers);

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, IEnumerable<string> eventNames, bool removeEventObservers = false)
    {
      bool hasRemovedObservable = false;
      Type eventSourceType = eventSource.GetType();
      foreach (string eventName in eventNames)
      {
        var key = new EventHandlerTableKey(eventName, eventSourceType);
        if (!EventAggregatorNew<TSource>.SourceEventInfoTable.TryGetValue(key, out EventInfoTableEntry entry))
        {
          continue;
        }

        if (entry.EventSourceInstances.Remove(eventSource))
        {
          entry.SourceEventInfo.RemoveEventHandler(eventSource, entry.GeneratedHandler);
          hasRemovedObservable = true;
        }

        if (removeEventObservers)
        {
          _ = TryRemoveAllObservers(eventName, eventSource.GetType());
        }
      }

      return hasRemovedObservable;
    }

    /// <inheritdoc />
    public bool TryRemoveObservable(object eventSource, bool removeObserversOfEvents = false)
    {
      bool hasRemovedObservable = false;

      foreach (KeyValuePair<EventHandlerTableKey, EventInfoTableEntry> entry in EventAggregatorNew<TSource>.SourceEventInfoTable)
      {
        if (entry.Value.EventSourceInstances.Contains(eventSource))
        {
          entry.Value.SourceEventInfo.RemoveEventHandler(eventSource, entry.Value.GeneratedHandler);
          hasRemovedObservable = true;
        }
      }

      if (removeObserversOfEvents)
      {
        _ = TryRemoveAllObservers(eventSource.GetType());
      }

      return hasRemovedObservable;
    }

    /// <inheritdoc />
    public bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler)
    {
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler, bool isMarshalEventToCurrentThreadEnabled)
    {
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      return isMarshalEventToCurrentThreadEnabled
        ? TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, SynchronizationContext.Current)
        : TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName);
    }

    /// <inheritdoc />
    public bool TryRegisterObserver(string eventName, Type eventSourceType, Delegate eventHandler, SynchronizationContext synchronizationContext)
    {
      string fullyQualifiedEventName = CreateFullyQualifiedEventIdOfSpecificSource(eventSourceType, eventName);
      return TryRegisterObserverInternal(eventHandler, fullyQualifiedEventName, synchronizationContext);
    }

    /// <inheritdoc />
    public bool TryRegisterObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler)
      => TryRegisterObserver(eventName, eventSourceType, (Delegate)eventHandler);

    /// <inheritdoc />
    public bool TryRegisterObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler, bool isMarshalEventToCurrentThreadEnabled)
      => TryRegisterObserver(eventName, eventSourceType, (Delegate)eventHandler, isMarshalEventToCurrentThreadEnabled);

    /// <inheritdoc />
    public bool TryRegisterObserver<TEventArgs>(string eventName, Type eventSourceType, EventHandler<TEventArgs> eventHandler, SynchronizationContext synchronizationContext)
      => TryRegisterObserver(eventName, eventSourceType, (Delegate)eventHandler, synchronizationContext);

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

    private static ConcurrentDictionary<EventHandlerTableKey, EventInfoTableEntry> SourceEventInfoTable { get; } = new ConcurrentDictionary<EventHandlerTableKey, EventInfoTableEntry>();
    private ConcurrentDictionary<string, List<Delegate>> EventHandlerTable { get; }
    private ConcurrentDictionary<Delegate, SynchronizationContext> EventHandlerSynchronizationContextTable { get; }
    //private ConditionalWeakTable<object, List<(EventInfo EventInfo, Delegate Handler)>> EventPublisherTable { get; }
  }
}