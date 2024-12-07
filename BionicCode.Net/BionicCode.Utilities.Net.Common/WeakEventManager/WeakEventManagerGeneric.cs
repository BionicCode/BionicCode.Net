namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  public class WeakEventManager<TEventSource> : WeakEventManager
  {
    public string EventName { get; }

    private readonly ConditionalWeakTable<object, ClientHandlerInfoCollection> eventListenerHandlerMap;
    private ReaderWriterLockSlim ListenerReaderWriterLock { get; set; }
    private static object SyncLock { get; }

    private static readonly MethodData genericHandlerMethodData;
    private static readonly MethodData customHandlerMethodData;

    static WeakEventManager()
    {
      SyncLock = new object();

      MethodInfo methodInfo = typeof(WeakEventManager<TEventSource>).GetMethod(nameof(OnStronglyTypedEvent), BindingFlags.Instance | BindingFlags.NonPublic);
      genericHandlerMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

      methodInfo = typeof(WeakEventManager<TEventSource>).GetMethod(nameof(OnEventHandlerCustomDynamicSignature), BindingFlags.Instance | BindingFlags.NonPublic);
      customHandlerMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
    }

    internal WeakEventManager(string eventName, bool isCustomClientDelegate)
    {
      this.ListenerReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this.eventListenerHandlerMap = new ConditionalWeakTable<object, ClientHandlerInfoCollection>();

      var key = new EventInfoTableKey(eventName, typeof(TEventSource));
      if (!WeakEventManager.EventInfoTable.TryGetValue(key, out EventInfoTableEntry tableEntry))
      {
        // Use BindingFlags.FlattenHierarchy to also get base genericTypeDefinition static events via the subclass (including protected events of the hierarchy and private events of the current genericTypeDefinition)
        EventInfo eventInfo = typeof(TEventSource).GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
        EventData eventSourceEventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);

        TypeData eventHandlerTypeData = eventSourceEventData.EventHandlerTypeData;
        MethodData invocatorData = eventHandlerTypeData.DelegateInvokeMethodData;
        ParameterData[] eventHandlerParameters = invocatorData.Parameters;
        Delegate proxySourceEventHandler = null;

        if (!isCustomClientDelegate
          && invocatorData.Parameters.Length == 2)
        {
          try
          {
            Debug.WriteLine("OnStronglyTypedEvent attached to event source");
            MethodInfo proxySourceEventHandlerMethodInfo = WeakEventManager<TEventSource>.genericHandlerMethodData.MakeGenericMethodInfo(eventHandlerParameters[0].ParameterTypeData.GetType(), eventHandlerParameters[1].ParameterTypeData.GetType());
            proxySourceEventHandler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, proxySourceEventHandlerMethodInfo);
          }
          catch (ArgumentException e)
          {
            string exceptionMessage = string.Format(InternalDelegateSignatureMismatchExceptionMessage, eventSourceEventData.RuntimeShortSignature, GetType().GetMethod(nameof(OnStronglyTypedEvent), BindingFlags.NonPublic | BindingFlags.Instance).ToRuntimeSignatureShortName());

            throw new EventHandlerMismatchException(exceptionMessage, e);
          }
        }
        else
        {
          Debug.WriteLine("Dynamically generated source event handler attached to event source");
          proxySourceEventHandler = GenerateEventHandler(eventHandlerParameters);
        }

        tableEntry = new EventInfoTableEntry(eventSourceEventData, proxySourceEventHandler);
        _ = WeakEventManager.EventInfoTable.TryAdd(key, tableEntry);
      }

      this.EventSourceEventData = tableEntry.EventData;
      Debug.Assert(this.EventSourceEventData != null);

      this.ProxyEventHandler = tableEntry.EventHandler;
      Debug.Assert(this.ProxyEventHandler != null);

      this.EventName = eventName;
    }

    private Delegate GenerateEventHandler(ParameterData[] eventHandlerParameters)
    {
      Delegate eventSourceHandler;
      var expressionParameters = new List<ParameterExpression>();
      foreach (ParameterData parameter in eventHandlerParameters)
      {
        ParameterExpression expressionParameter = Expression.Parameter(parameter.ParameterTypeData.GetType(), parameter.Name);
        expressionParameters.Add(expressionParameter);
      }

      IEnumerable<UnaryExpression> castedExpressionParameters = expressionParameters.Select(parameter => Expression.TypeAs(parameter, typeof(object)));
      NewArrayExpression argsArray = Expression.NewArrayInit(typeof(object), castedExpressionParameters);
      ConstantExpression target = Expression.Constant(this);
      MethodCallExpression method = Expression.Call(target, WeakEventManager<TEventSource>.customHandlerMethodData.GetMethodInfo(), argsArray);
      eventSourceHandler = Expression.Lambda(method, expressionParameters).Compile();

      return eventSourceHandler;
    }

    private static bool TryGenerateAddEventHandlerInvocator(Type clientHandlerType, out Action<TEventSource, string, Delegate, SynchronizationContext> addHandlerInvocator)
    {
      addHandlerInvocator = null;

      ParameterExpression eventSource = Expression.Parameter(typeof(TEventSource), "eventSource");
      ParameterExpression eventName = Expression.Parameter(typeof(string), "eventName");
      ParameterExpression clientHandler = Expression.Parameter(typeof(Delegate), "clientHandler");
      ParameterExpression synchronizationContext = Expression.Parameter(typeof(SynchronizationContext), "synchronizationContext");

      MethodCallExpression method = null;
      if (clientHandlerType.IsGenericType)
      {
        Type[] genericTypeArguments = clientHandlerType.GetGenericArguments();
        Type genericTypeDefinition = clientHandlerType.GetGenericTypeDefinition();
        if (genericTypeDefinition == typeof(EventHandler<>))
        {
          Type argsType = genericTypeArguments[0];
          method = Expression.Call(typeof(WeakEventManager<TEventSource>), nameof(WeakEventManager<TEventSource>.AddGenericEventHandler), new Type[] { argsType }, eventSource, eventName, Expression.TypeAs(clientHandler, clientHandlerType), synchronizationContext);
        }
        else if (genericTypeDefinition == typeof(Action<,>))
        {
          Type senderType = genericTypeArguments[0];
          Type argsType = genericTypeArguments[1];
          method = Expression.Call(typeof(WeakEventManager<TEventSource>), nameof(WeakEventManager<TEventSource>.AddActionHandler), new Type[] { senderType, argsType }, eventSource, eventName, Expression.TypeAs(clientHandler, clientHandlerType), synchronizationContext);
        }
        else
        {
          return false;
        }
      }
      else
      {
        return false;
      }

      addHandlerInvocator = Expression.Lambda<Action<TEventSource, string, Delegate, SynchronizationContext>>(method, eventSource, eventName, clientHandler, synchronizationContext)
        .Compile();

      return true;
    }

    public static void AddEventHandler<TEventHandler>(TEventSource eventSource, string eventName, TEventHandler handler, bool executeOnCurrentSynchronizationContext = false) where TEventHandler : Delegate
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler<TEventHandler>(TEventSource eventSource, string eventName, TEventHandler handler, SynchronizationContext synchronizationContext) where TEventHandler : Delegate
    {
      lock (WeakEventManager<TEventSource>.SyncLock)
      {
        if (handler is EventHandler eventHandler)
        {
          AddEventHandler(eventSource, eventName, eventHandler, synchronizationContext);
          return;
        }

        Type eventHandlerType = handler.GetType();
        var key = new AddClientHandlerInvocatorTableKey(typeof(TEventSource), eventHandlerType);
        if (!WeakEventManager.AddClientHandlerInvocatorTable.TryGetValue(key, out AddClientHandlerInvocatorTableEntry addHandlerInvocatorInfo))
        {
          bool isSuccessful = TryGenerateAddEventHandlerInvocator(eventHandlerType, out Action<TEventSource, string, Delegate, SynchronizationContext> addHandlerInvocator);
          addHandlerInvocatorInfo = new AddClientHandlerInvocatorTableEntry(addHandlerInvocator, useAddCustomHandlerMethod: !isSuccessful, typeof(TEventSource));
          _ = WeakEventManager.AddClientHandlerInvocatorTable.TryAdd(key, addHandlerInvocatorInfo);
        }

        if (addHandlerInvocatorInfo.UseAddCustomHandlerMethod)
        {
          AddCustomHandler(eventSource, eventName, handler, synchronizationContext);
        }
        else
        {
          Action<TEventSource, string, Delegate, SynchronizationContext> addHandlerInvocator = addHandlerInvocatorInfo.GetAddHandlerInvocator<TEventSource>();
          addHandlerInvocator.Invoke(eventSource, eventName, handler, synchronizationContext);
        }
      }
    }

    //public static void AddCustomHandler<TEvent>(TEventSource eventSource, string eventName, TEvent handler, bool executeOnCurrentSynchronizationContext = false) where TEvent : Delegate
    //  => AddCustomEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    private static void AddCustomHandler(object eventSource, string eventName, Delegate handler, SynchronizationContext synchronizationContext)
    {
      Action<object, object, ClientHandlerInfo> eventHandlerInvocator = (sender, e, handlerInfo) =>
      {
        if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
        {
          //MethodInfo invokeMethod = clientHandler.GetType().GetMethod("Invoke");
          //_ = invokeMethod.Invoke(clientHandler.Target, new object[] { e });
          _ = clientHandler.DynamicInvoke(e);
        }
      };

      RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: true, (TEventSource)eventSource, eventName, synchronizationContext);
    }

    //public static void AddEventHandler<TEventArgs>(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
    //  => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    private static void AddGenericEventHandler<TEventArgs>(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, SynchronizationContext synchronizationContext)
    {
      Action<object, object, ClientHandlerInfo> eventHandlerInvocator =
        (sender, e, handlerInfo) =>
        {
          if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
          {
            var eventHandler = (EventHandler<TEventArgs>)clientHandler;
            eventHandler.Invoke(sender, (TEventArgs)e);
          }
        };

      RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: false, eventSource, eventName, synchronizationContext);
    }

    //public static void AddActionHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
    //  => AddActionHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    private static void AddActionHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, SynchronizationContext synchronizationContext)
    {
      Action<object, object, ClientHandlerInfo> eventHandlerInvocator =
        (sender, e, handlerInfo) =>
        {
          if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
          {
            var eventHandler = (Action<TSender, TEventArgs>)clientHandler;
            eventHandler.Invoke((TSender)sender, (TEventArgs)e);
          }
        };

      RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: false, eventSource, eventName, synchronizationContext);
    }

    //public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, bool executeOnCurrentSynchronizationContext = false)
    //  => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    private static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, SynchronizationContext synchronizationContext)
    {
      Action<object, object, ClientHandlerInfo> eventHandlerInvocator =
        (sender, e, handlerInfo) =>
        {
          if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
          {
            var eventHandler = (EventHandler)clientHandler;
            eventHandler.Invoke(sender, e as EventArgs);
          }
        };

      RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: false, eventSource, eventName, synchronizationContext);
    }

#if NET
    private static void ThrowIfInvalidArguments(TEventSource eventSource, string eventName, Delegate handler, [CallerArgumentExpression(nameof(eventSource))] string eventSourceArgumentName = null, [CallerArgumentExpression(nameof(eventName))] string eventNameArgumentName = null, [CallerArgumentExpression(nameof(handler))] string handlerArgumentName = null)
#else
    private static void ThrowIfInvalidArguments(TEventSource eventSource, string eventName, EventInfo eventInfo, Delegate handler, string eventSourceArgumentName = null, string eventNameArgumentName = null, string handlerArgumentName = null)
#endif
    {
      if (handler is null)
      {
        throw new ArgumentNullException(handlerArgumentName);
      }

      if (eventName is null)
      {
        throw new ArgumentNullException(eventNameArgumentName);
      }

      if (string.IsNullOrWhiteSpace(eventName))
      {
        throw new ArgumentException("The event name is invalid.", nameof(eventNameArgumentName));
      }
    }

    private static void ThrowIfInvalidHandler(EventInfo eventInfo, Delegate clientHandler)
    {
      MethodInfo eventDelegateInvokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
      ParameterInfo[] eventDelegateParameters = eventDelegateInvokeMethod.GetParameters();

      /* Validate the event */

      //if (eventDelegateMethodParameters.Length != 2)
      //{
      //  throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter count is {eventDelegateMethodParameters.Length} instead of 2"));
      //}

      //if (!(eventDelegateMethodParameters[0].ParameterType == typeof(TEventSource) 
      //  || eventDelegateMethodParameters[0].ParameterType == typeof(object)))
      //{
      //  throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter at index '0' is not of genericTypeDefinition {nameof(TEventSource)} or {typeof(object).FullName}"));
      //}

      //if (!typeof(EventArgs).IsAssignableFrom(eventDelegateMethodParameters[1].ParameterType))
      //{
      //  throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter at index '1' is not of genericTypeDefinition or derived from genericTypeDefinition {typeof(EventArgs).FullName}"));
      //}

      //if (eventDelegateMethodParameters[1].ParameterType != typeof(TEventArgs))
      //{
      //  throw new EventDelegateMismatchException(string.Format(EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage, nameof(TEventArgs), eventInfo.Name, typeof(TEventArgs), eventDelegateMethodParameters[1].ParameterType.FullName));
      //}

      MethodInfo eventHandlerMethod = clientHandler.Method;
      ParameterInfo[] clientHandlerParameters = eventHandlerMethod.GetParameters();

      /* Validate the event EventHandler */

      if (eventDelegateParameters.Length != clientHandlerParameters.Length)
      {
        throw new EventHandlerMismatchException(string.Format(WeakEventManager.HandlerDelegateSignatureMismatchExceptionMessage,
          eventInfo.EventHandlerType.ToSignatureName(),
          eventHandlerMethod.ToSignatureName(),
          $"Invalid parameter count."));
      }

      for (int parameterIndex = 0; parameterIndex < eventDelegateParameters.Length; parameterIndex++)
      {
        Type eventDelegateParameterType = eventDelegateParameters[parameterIndex].ParameterType;
        Type eventHandlerParameterType = clientHandlerParameters[parameterIndex].ParameterType;
        if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
        {
          throw new EventHandlerMismatchException(string.Format(WeakEventManager.HandlerDelegateSignatureMismatchExceptionMessage,
            eventInfo.EventHandlerType.ToSignatureName(),
            eventHandlerMethod.ToSignatureName(),
            $"Unable to cast parameter of genericTypeDefinition {eventDelegateParameterType.FullName} at parameter index {parameterIndex} of the event delegate to genericTypeDefinition {eventHandlerParameterType.FullName} at parameter index {parameterIndex} of the provided event handler."));
        }
      }
    }

    private static void RegisterClientHandler(Action<object, object, ClientHandlerInfo> clientHandlerAdapterInvocator, Delegate clientHandler, bool isCustomClientDelegate, object eventSource, string eventName, SynchronizationContext capturedSynchronizationContext)
    {
      // If the event is a static event, the eventSource is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      if (eventSource is null)
      {
        eventSource = DummyEventSourceForStaticEventHandlers.Instance;
      }

      WeakEventManager<TEventSource> weakEventManager;
      lock (ManagedWeakTable.TableLock)
      {
        weakEventManager = WeakEventManagerTable.GetOrCreateWeakEventManager<TEventSource>(eventSource, eventName, isCustomClientDelegate);
        ThrowIfInvalidHandler(weakEventManager.EventSourceEventData.GetEventInfo(), clientHandler);

        weakEventManager.RegisterHandler(clientHandlerAdapterInvocator, clientHandler, eventSource, capturedSynchronizationContext);
      }
    }

    //public static void RemoveEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
    //  => RemoveEventHandler(eventSource, eventName, (Delegate)handler);

    public static void RemoveEventHandler(TEventSource eventSource, string eventName, Delegate handler)
    {
      object adjustedEventSource = eventSource == null
        ? DummyEventSourceForStaticEventHandlers.Instance
        : (object)eventSource;

      lock (ManagedWeakTable.TableLock)
      {
        if (!WeakEventManagerTable.TryGetWeakEventManager(adjustedEventSource, eventName, out WeakEventManager<TEventSource> weakEventManager))
        {
#if DEBUG
          Debug.WriteLine("Unable to remove event handler because event source has expired or the event was never registered.");
#endif
          return;
        }

        weakEventManager.UnregisterHandler(handler, adjustedEventSource);
      }
    }

    private void RegisterHandler(Action<object, object, ClientHandlerInfo> clientHandlerAdapterInvocator, Delegate clientHandler, object eventSource, SynchronizationContext capturedSynchronizationContext)
    {
      try
      {
        this.ListenerReaderWriterLock.EnterWriteLock();

        // If the event handler is a static method, the delegate's target is NULL.
        // In this case, we need to provide a placeholder for the WeakTable entry.
        object eventListener = clientHandler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;
        if (!this.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
        {
          clientHandlerInfos = new ClientHandlerInfoCollection();
          this.eventListenerHandlerMap.Add(eventListener, clientHandlerInfos);
          WeakReference<object> eventListenerWeakReference = ManagedWeakTable.GetOrCreateWeakReference(eventListener);
          _ = this.EventListeners.Add(eventListenerWeakReference);
          StartListeningInternal(eventSource);
        }

        var clientHandlerInfo = new ClientHandlerInfo(clientHandler, clientHandlerAdapterInvocator, capturedSynchronizationContext);
        clientHandlerInfos.Add(clientHandlerInfo);

#if DEBUG
        Debug.WriteLine(">>> Add event handler");
        registeredEventHandlerCount++;
        Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");
#endif
      }
      finally
      {
        this.ListenerReaderWriterLock.ExitWriteLock();
      }
    }

    private void UnregisterHandler(Delegate handler, object eventSource)
    {
      try
      {
        this.ListenerReaderWriterLock.EnterWriteLock();

        object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;
        if (this.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
        {
          var delegateEqualityComparer = new DelegateSignatureEqualityComparer();
          foreach (ClientHandlerInfo handlerInfo in clientHandlerInfos.EnumerateSafe())
          {
            if (!handlerInfo.TryGetClientHandler(out Delegate eventHandler))
            {
              continue;
            }

            // Check if the delegate is a closure (created to capture the WeakEventManger's TEventSource and TEventArgs)
            if (eventHandler.Target != null
              && eventHandler.Target.GetType() != eventListener.GetType()
              && eventHandler.Target.GetType() != typeof(Delegate))
            {
              FieldInfo handlerField = eventHandler.Target.GetType().GetField("handler");
              if (handlerField is null)
              {
                continue;
              }

              object originalHandler = handlerField.GetValue(eventHandler.Target);
              if (!(originalHandler is Delegate invocatorDelegate))
              {
                continue;
              }

              eventHandler = invocatorDelegate;
            }

            if (delegateEqualityComparer.Equals(eventHandler, handler))
            {
              clientHandlerInfos.Remove(handlerInfo);
              handlerInfo.Dispose();
              Debug.WriteLine("<<< Removed event handler");

#if DEBUG
              unregisteredEventHandlerCount++;
              Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");
#endif

              break;
            }
          }

          if (clientHandlerInfos.Count == 0)
          {
            bool isListenerRemoved = this.eventListenerHandlerMap.Remove(eventListener)
              && (this.EventListeners.RemoveWhere(reference => reference.TryGetTarget(out object listener) && ReferenceEquals(listener, eventListener)) > 0);

            Debug.Assert(isListenerRemoved);
          }
        }

        if (!this.EventListeners.Any())
        {
          Debug.WriteLine("Empty handler list ==> call End Service from RemoveEventHandler() API");
          EndService(eventSource);
        }
      }
      finally
      {
        this.ListenerReaderWriterLock.ExitWriteLock();
      }
    }

    internal override void Purge()
    {
      if (this.IsPurged)
      {
        return;
      }

      bool hasLocalLockAcquired = false;
      if (!this.ListenerReaderWriterLock.IsWriteLockHeld)
      {
        this.ListenerReaderWriterLock.EnterWriteLock();
        hasLocalLockAcquired = true;
      }

      Debug.WriteLine($"WeakEventManager internal purge. Is listening: {this.IsListening}");
      Debug.WriteLine($"Stopping WeakEventManager and clearing {this.EventListeners.Count} event listener entries from {nameof(this.eventListenerHandlerMap)}");

      foreach (WeakReference<object> reference in this.EventListeners)
      {
        if (reference.TryGetTarget(out object evenListener))
        {
          if (this.eventListenerHandlerMap.TryGetValue(evenListener, out ClientHandlerInfoCollection clientHandlerInfos))
          {
            clientHandlerInfos.Clear();
          }

          _ = this.eventListenerHandlerMap.Remove(evenListener);
          ManagedWeakTable.RecycleWeakReference(reference);
        }
      }

      this.EventListeners.Clear();
      this.IsPurged = true;

      if (hasLocalLockAcquired)
      {
        this.ListenerReaderWriterLock.ExitWriteLock();
      }

      _ = TryDisposeLock();
    }

    public static void StopListening(TEventSource eventSource, string eventName)
    {
      if (WeakEventManagerTable.TryGetWeakEventManager(eventSource, eventName, out WeakEventManager<TEventSource> weakEventManager))
      {
        weakEventManager.StopListeningInternal(eventSource);
      }
    }

    public static void StartListening(TEventSource eventSource, string eventName)
    {
      if (WeakEventManagerTable.TryGetWeakEventManager(eventSource, eventName, out WeakEventManager<TEventSource> weakEventManager))
      {
        weakEventManager.StartListeningInternal(eventSource);
      }
    }

    private void OnStronglyTypedEvent<TSender, TEventArgs>(TSender sender, TEventArgs e)
    {
      if (this.IsPurged)
      {
        EndService(sender);
        return;
      }

      Debug.WriteLine($"Invoke deliver event handler");
      int eventCounter = 0;
      try
      {
        this.ListenerReaderWriterLock.EnterUpgradeableReadLock();

        foreach (WeakReference<object> eventListenerReference in this.EventListeners)
        {
          if (!eventListenerReference.TryGetTarget(out object eventListener))
          {
            continue;
          }

          if (this.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
          {
            foreach (ClientHandlerInfo handlerInfo in clientHandlerInfos.EnumerateSafe())
            {
              if (!handlerInfo.IsClientHandlerAlive)
              {
                try
                {
                  this.ListenerReaderWriterLock.EnterWriteLock();

                  Debug.WriteLine($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");
                  if (clientHandlerInfos.IsEmpty())
                  {
                    _ = this.eventListenerHandlerMap.Remove(eventListener);
                  }

                  continue;
                }
                finally
                {
                  this.ListenerReaderWriterLock.ExitWriteLock();
                }
              }

              Debug.WriteLine($"Invoke client handler. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");

              if (handlerInfo.ClientContext != null)
              {
                handlerInfo.ClientContext.Send(state => handlerInfo.ClientAdapterHandler.Invoke(sender, e, handlerInfo), null);
              }
              else
              {
                handlerInfo.ClientAdapterHandler.Invoke(sender, e, handlerInfo);
              }
            }
          }
        }

        bool hasListeners = this.EventListeners.Any();
        if (!hasListeners)
        {
          EndService(sender);
        }
      }
      finally
      {
        this.ListenerReaderWriterLock.ExitUpgradeableReadLock();
        _ = TryDisposeLock();
      }
    }

    private void OnEventHandlerCustomDynamicSignature(params object[] args)
    {
      var tableKey = new ManagedWeakTableKey(this.EventName, typeof(TEventSource));
      if (!(ManagedWeakTable.TryGetEntry(this.EventSourceId, tableKey, out ManagedWeakTableEntry entry)
        && entry.TryGetReferenceTarget(out object eventSource)))
      {
        return;
      }

      if (this.IsPurged)
      {
        EndService(eventSource);
      }

      Debug.WriteLine($"Invoke deliver event handler");
      int eventCounter = 0;
      try
      {
        this.ListenerReaderWriterLock.EnterUpgradeableReadLock();

        foreach (WeakReference<object> eventListenerReference in this.EventListeners)
        {
          if (!eventListenerReference.TryGetTarget(out object eventListener))
          {
            continue;
          }

          if (this.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
          {
            foreach (ClientHandlerInfo handlerInfo in clientHandlerInfos.EnumerateSafe())
            {
              if (!handlerInfo.IsClientHandlerAlive)
              {
                try
                {
                  this.ListenerReaderWriterLock.EnterWriteLock();

                  Debug.WriteLine($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown, because handler was dynamically generated as client handler does not follow C# conventions.");
                  if (clientHandlerInfos.IsEmpty())
                  {
                    _ = this.eventListenerHandlerMap.Remove(eventListener);
                  }

                  continue;
                }
                finally
                {
                  this.ListenerReaderWriterLock.ExitWriteLock();
                }
              }

              Debug.WriteLine($"Invoke client handler. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown, because handler was dynamically generated as client handler does not follow C# conventions.");

              if (handlerInfo.ClientContext != null)
              {
                handlerInfo.ClientContext.Send(state => handlerInfo.ClientAdapterHandler.Invoke(null, args, handlerInfo), null);
              }
              else
              {
                handlerInfo.ClientAdapterHandler.Invoke(null, args, handlerInfo);
              }
            }
          }
        }

        bool hasListeners = this.EventListeners.Any();
        if (!hasListeners)
        {
          EndService(eventSource);
        }
      }
      finally
      {
        this.ListenerReaderWriterLock.ExitUpgradeableReadLock();
        _ = TryDisposeLock();
      }
    }

    private void EndService(object eventSource)
    {
      Debug.WriteLine("End Service called");
      StopListeningInternal(eventSource);
      WeakEventManagerTable.RemoveWeakEventManager<TEventSource>(eventSource, this.EventName);
      if (!this.IsPurged)
      {
        Purge();
      }

      _ = TryDisposeLock();
    }

    private bool TryDisposeLock()
    {
      if (this.IsPurged
        && !this.ListenerReaderWriterLock.IsReadLockHeld && this.ListenerReaderWriterLock.WaitingReadCount == 0
        && !this.ListenerReaderWriterLock.IsWriteLockHeld && this.ListenerReaderWriterLock.WaitingWriteCount == 0)
      {
        this.ListenerReaderWriterLock?.Dispose();
        this.ListenerReaderWriterLock = null;

        return true;
      }

      return false;
    }

  }

  /// <summary>
  /// Event listener for static events (eventListenerHandlerMap that don't have a instance as target) used as key for storing static handlers in a table.
  /// </summary>
  internal class DummyEventListenerForStaticEventHandlers
  {
    public static readonly DummyEventListenerForStaticEventHandlers Instance = new DummyEventListenerForStaticEventHandlers();
  }

  /// <summary>
  /// Event listener for static events (eventListenerHandlerMap that don't have a instance as target) used as key for storing static handlers in a table.
  /// </summary>
  internal class DummyEventSourceForStaticEventHandlers
  {
    public static readonly DummyEventSourceForStaticEventHandlers Instance = new DummyEventSourceForStaticEventHandlers();
  }
}