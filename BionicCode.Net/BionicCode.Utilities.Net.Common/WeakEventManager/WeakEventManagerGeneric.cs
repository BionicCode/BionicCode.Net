namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Reflection.Metadata;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Threading;

  public class WeakEventManager<TEventSource> : WeakEventManager
  {
    private readonly ConditionalWeakTable<object, ClientHandlerInfoCollection> eventListenerHandlerMap;
    private ReaderWriterLockSlim ListenerReaderWriterLock { get; }
    private string EventName { get; }

    private static readonly MethodData genericHandlerMethodData;
    private static readonly MethodData addHandlerEventHandlerMethodData;
    private static readonly MethodData addHandlerEventHandlerGenericMethodData;
    private static readonly MethodData addHandlerActionMethodData;

    static WeakEventManager()
    {
      MethodInfo methodInfo = typeof(WeakEventManager<TEventSource>).GetMethod(nameof(OnStronglyTypedEvent), BindingFlags.Instance | BindingFlags.NonPublic);
      genericHandlerMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

      Type[] methodParameters = new Type[] { typeof(TEventSource), typeof(string), typeof(Action<,>), typeof(SynchronizationContext) };
      methodInfo = typeof(WeakEventManager<>).GetMethod("AddEventHandler", methodParameters);
      addHandlerActionMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

      methodParameters = new Type[] { typeof(TEventSource), typeof(string), typeof(EventHandler<>), typeof(SynchronizationContext) };
      methodInfo = typeof(WeakEventManager<>).GetMethod("AddEventHandler", methodParameters);
      addHandlerEventHandlerGenericMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);

      methodParameters = new Type[] { typeof(TEventSource), typeof(string), typeof(EventHandler), typeof(SynchronizationContext) };
      methodInfo = typeof(WeakEventManager<>).GetMethod("AddEventHandler", methodParameters);
      addHandlerEventHandlerMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
    }

    internal WeakEventManager(string eventName)
    {
      // Use BindingFlags.FlattenHierarchy to also get base genericTypeDefinition static events via the subclass (including protected events of the hierarchy and private events of the current genericTypeDefinition)
      this.EventSourceEventInfo = typeof(TEventSource).GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      if (this.EventSourceEventInfo is null)
      {
        throw new ArgumentException($"The specified event '{eventName}' on event source genericTypeDefinition '{typeof(TEventSource).FullName}' could not be found. Please check the provided event name, event source genericTypeDefinition.");
      }

      Type eventHandlerType = this.EventSourceEventInfo.EventHandlerType;
      TypeData eventHandlerTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventHandlerType);
      if (!WeakEventManager<TEventSource>.ProxyEventHandlerPool.TryGetValue(eventHandlerTypeData, out MethodData handlerMethodData))
      {
        MethodData invocatorData = eventHandlerTypeData.DelegateInvokeMethodData;
        ParameterData[] eventHandlerParameters = invocatorData.Parameters;
        MethodInfo handlerMethodInfo = WeakEventManager<TEventSource>.genericHandlerMethodData.GetMethodInfo().MakeGenericMethod(eventHandlerParameters[0].ParameterTypeData.GetType(), eventHandlerParameters[1].ParameterTypeData.GetType());

        handlerMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(handlerMethodInfo);
        _ = WeakEventManager<TEventSource>.ProxyEventHandlerPool.TryAdd(eventHandlerTypeData, handlerMethodData);
      }

      if (handlerMethodData.Parameters.Length == 2)
      {
        try
        {
          Debug.WriteLine("OnStronglyTypedEvent attached to event source");
          this.ProxyEventHandler = Delegate.CreateDelegate(eventHandlerType, this, handlerMethodData.GetMethodInfo());
        }
        catch (ArgumentException e)
        {
          string exceptionMessage = string.Format(InternalDelegateSignatureMismatchExceptionMessage, eventHandlerType.ToRuntimeSignatureShortName(), GetType().GetMethod(nameof(OnStronglyTypedEvent), BindingFlags.NonPublic | BindingFlags.Instance).ToRuntimeSignatureShortName());

          throw new EventHandlerMismatchException(exceptionMessage, e);
        }
      }
      else
      {
        Debug.WriteLine("Dynamically generated source event handler attached to event source");
        ParameterInfo[] eventHandlerParameters = handlerMethodData.Parameters
          .Select(parameterData => parameterData.ParameterInfo)
          .ToArray();
        this.ProxyEventHandler = GenerateEventHandler(eventHandlerParameters);
      }

      this.EventName = eventName;
      this.ListenerReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this.eventListenerHandlerMap = new ConditionalWeakTable<object, ClientHandlerInfoCollection>();
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
      //NewArrayExpression argsArray = Expression.NewArrayInit(typeof(object), castedExpressionParameters);
      MethodCallExpression method = Expression.Call(GetType().GetMethod(nameof(OnEventHandlerCustomDynamicSignature)), castedExpressionParameters);
      eventSourceHandler = Expression.Lambda(method, expressionParameters).Compile();

      return eventSourceHandler;
    }

    private Action<TEventSource, string, Delegate, SynchronizationContext> GenerateAddEventHandlerInvocator(Type clientHandlerType)
    {
      MethodInfo addHandlerInvocator = null;
      (Type Type, string Name)[] parameters = null;

      if (clientHandlerType.IsGenericType)
      {
        Type[] genericTypeArguments = clientHandlerType.GetGenericArguments();
        Type genericTypeDefinition = clientHandlerType.GetGenericTypeDefinition();
        if (genericTypeDefinition == typeof(EventHandler<>))
        {
          Type argsType = genericTypeArguments[0];
          addHandlerInvocator = addHandlerEventHandlerGenericMethodData.GetMethodInfo().MakeGenericMethod(argsType);
          parameters = new (Type, string)[] { (typeof(object), "senderType"), (argsType, "eventArgsType" };
        }
        else if (genericTypeDefinition == typeof(Action<,>))
        {
          Type senderType = genericTypeArguments[0];
          Type argsType = genericTypeArguments[1];
          addHandlerInvocator = addHandlerActionMethodData.GetMethodInfo().MakeGenericMethod(senderType, argsType);
          parameters = new (Type, string)[] { (senderType, "senderType"), (argsType, "eventArgsType" };
        }
      }

      var expressionParameters = new List<ParameterExpression>()
      {
        Expression.Parameter(typeof(TEventSource), "eventSource"),
        Expression.Parameter(typeof(TEventSource), "eventName"),
        Expression.Parameter(clientHandlerType, "clientHandler"),
        Expression.Parameter(typeof(SynchronizationContext), "synchronizationContext"),
      };
      
      MethodCallExpression method = Expression.Call(addHandlerInvocator, expressionParameters);
      Action<TEventSource, string, Delegate, SynchronizationContext> eventSourceHandler = Expression.Lambda<Action<TEventSource, string, Delegate, SynchronizationContext>>(method, expressionParameters).Compile();

      return eventSourceHandler;
    }

    public static void AddCustomEventHandler<TEvent>(TEventSource eventSource, string eventName, TEvent handler, bool executeOnCurrentSynchronizationContext = false) where TEvent : Delegate
      => AddCustomEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddCustomEventHandler<TEvent>(TEventSource eventSource, string eventName, TEvent handler, SynchronizationContext synchronizationContext) where TEvent : Delegate
    {
      Type eventhandlerType = handler.GetType();
      if (handler is EventHandler eventHandler)
      {
        AddEventHandler(eventSource, eventName, eventHandler, synchronizationContext);
        return;
      }
      else if (eventhandlerType.IsGenericType)
      {
        Type[] genericTypeArguments = eventhandlerType.GetGenericArguments();
        Type genericTypeDefinition = eventhandlerType.GetGenericTypeDefinition();

        if (genericTypeDefinition == typeof(EventHandler<>))
        {
          Type argsType = genericTypeArguments[0];
        _ = addHandlerEventHandlerGenericMethodData.GetMethodInfo().MakeGenericMethod(argsType)
            .Invoke(null, new object[] { eventSource, eventName, handler, synchronizationContext });
        }
        else if (genericTypeDefinition == typeof(Action<,>))
        {
          Type senderType = genericTypeArguments[0];
          Type argsType = genericTypeArguments[1];
          _ = addHandlerActionMethodData.GetMethodInfo().MakeGenericMethod(senderType, argsType)
            .Invoke(null, new object[] { eventSource, eventName, handler, synchronizationContext });
        }
        else
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

          RegisterClientHandler(eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
        }
      }
      else if (eventhandlerType.IsGenericType && eventhandlerType.GetGenericTypeDefinition() == typeof(Action<,>))
      {
        Type senderType = eventhandlerType.GetGenericArguments()[0];
        Type argsType = eventhandlerType.GetGenericArguments()[1];
        Type[] methodParameters = new Type[] { typeof(TEventSource), typeof(string), typeof(Action<,>), typeof(SynchronizationContext) };
        _ = typeof(WeakEventManager<>).GetMethod("AddEventHandler", methodParameters)
          .MakeGenericMethod(senderType, argsType)
          .Invoke(null, new object[] { eventSource, eventName, eventhandlerType, synchronizationContext });
      }
      else
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

        RegisterClientHandler(eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
      }
    }

    public static void AddEventHandler<TEventArgs>(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler<TEventArgs>(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, SynchronizationContext synchronizationContext)
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

      RegisterClientHandler(eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
    }

    public static void AddActionHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
      => AddActionHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddActionHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, SynchronizationContext synchronizationContext)
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

      RegisterClientHandler(eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, SynchronizationContext synchronizationContext)
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

      RegisterClientHandler(eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
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
        throw new EventHandlerMismatchException(string.Format(HandlerDelegateSignatureMismatchExceptionMessage, 
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
          throw new EventHandlerMismatchException(string.Format(HandlerDelegateSignatureMismatchExceptionMessage,
            eventInfo.EventHandlerType.ToSignatureName(),
            eventHandlerMethod.ToSignatureName(),
            $"Unable to cast parameter of genericTypeDefinition {eventDelegateParameterType.FullName} at parameter index {parameterIndex} of the event delegate to genericTypeDefinition {eventHandlerParameterType.FullName} at parameter index {parameterIndex} of the provided event handler."));
        }
      }
    }

    private static void RegisterClientHandler(Action<object, object, ClientHandlerInfo> clientHandlerAdapterInvocator, Delegate clientHandler, TEventSource eventSource, string eventName, SynchronizationContext capturedSynchronizationContext)
    {
      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = clientHandler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;
      WeakEventManager<TEventSource> weakEventManager = WeakEventManagerTable.GetOrCreateWeakEventManager<TEventSource>(eventSource, eventName);
      ThrowIfInvalidHandler(weakEventManager.EventSourceEventInfo, clientHandler);
      
      if (weakEventManager.IsPurged)
      {
        return;
      }

      weakEventManager.ListenerReaderWriterLock.EnterWriteLock();
      if (!weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
      {
        clientHandlerInfos = new ClientHandlerInfoCollection();
        weakEventManager.eventListenerHandlerMap.Add(eventListener, clientHandlerInfos);
        WeakReference<object> eventListenerWeakReference = ManagedWeakTable.GetOrCreateWeakReference(eventListener);
        _ = weakEventManager.EventListeners.Add(eventListenerWeakReference);
        weakEventManager.StartListeningInternal(eventSource);
      }

      var clientHandlerInfo = new ClientHandlerInfo(clientHandler, clientHandlerAdapterInvocator, eventSource, capturedSynchronizationContext);
      clientHandlerInfos.Add(clientHandlerInfo);

#if DEBUG
      Debug.WriteLine(">>> Add event handler");
      registeredEventHandlerCount++;
      Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");
#endif
      weakEventManager.ListenerReaderWriterLock.ExitWriteLock();
    }

    //public static void RemoveEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
    //  => RemoveEventHandler(eventSource, eventName, (Delegate)handler);

    public static void RemoveEventHandler(TEventSource eventSource, string eventName, Delegate handler)
    {
      object adjustedEventSource = eventSource == null
        ? DummyEventSourceForStaticEventHandlers.Instance
        : (object)eventSource;

      if (!WeakEventManagerTable.TryGetWeakEventManager(adjustedEventSource, eventName, out WeakEventManager<TEventSource> weakEventManager))
      {
#if DEBUG
        unregisteredEventHandlerCount++;
        Debug.WriteLine("Unable to remove event handler because event source has expired");
#endif
        return;
      }

      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;
      if (weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
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
          bool isListenerRemoved = weakEventManager.eventListenerHandlerMap.Remove(eventListener)
            && (weakEventManager.EventListeners.RemoveWhere(reference => reference.TryGetTarget(out object listener) && ReferenceEquals(listener, eventListener)) > 0);

          Debug.Assert(isListenerRemoved);
        }
      }

      if (!weakEventManager.EventListeners.Any())
      {
        Debug.WriteLine("Empty handler list ==> call End Service from RemoveEventHandler() API");
        weakEventManager.EndService(adjustedEventSource);
      }
    }

    internal override void Purge()
    {
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
      this.ListenerReaderWriterLock.Dispose();
      this.IsPurged = true;
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
        return;
      }

      Debug.WriteLine($"Invoke deliver event handler");
      int eventCounter = 0;
      this.ListenerReaderWriterLock.EnterReadLock();
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
              Debug.WriteLine($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");
              if (clientHandlerInfos.IsEmpty())
              {
                _ = this.eventListenerHandlerMap.Remove(eventListener);
              }

              continue;
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

      this.ListenerReaderWriterLock.ExitReadLock();
    }

    //private void OnEvent(object sender, TEventArgs e)
    //{
    //  if (this.IsPurged)
    //  {
    //    return;
    //  }

    //  Debug.WriteLine($"Invoke deliver event handler");
    //  int eventCounter = 0;
    //  this.ListenerReaderWriterLock.EnterReadLock();
    //  foreach (WeakReference<object> eventListenerReference in this.EventListeners)
    //  {
    //    if (!eventListenerReference.TryGetTarget(out object eventListener))
    //    {
    //      continue;
    //    }

    //    if (this.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<ClientHandlerInfo> clientHandlerInfos))
    //    {
    //      foreach (ClientHandlerInfo handlerInfo in clientHandlerInfos)
    //      {
    //        Debug.WriteLine($"Invoke client ({eventListener.GetType().FullName}) event handler #{eventCounter++}. Event source: {sender?.GetType().FullName ?? "STATIC"}");

    //        if (handlerInfo.ClientContext != null)
    //        {
    //          handlerInfo.ClientContext.Send(state => handlerInfo.ClientHandler.Invoke((TEventSource)sender, e), null);
    //        }
    //        else
    //        {
    //          handlerInfo.ClientHandler.Invoke((TEventSource)sender, e);
    //        }
    //      }
    //    }
    //  }

    //  bool hasListeners = this.EventListeners.Any();
    //  if (!hasListeners)
    //  {
    //    object adjustedEventSource = sender ?? DummyEventSourceForStaticEventHandlers.Instance;
    //    EndService(adjustedEventSource);
    //  }

    //  this.ListenerReaderWriterLock.ExitReadLock();
    //}

    //private void InvokeClientHandler<TSender, TEventArgs>(Delegate clientHandler,  TSender sender, TEventArgs e)
    //{
    //  if (clientHandler is EventHandler defaultEventHandler)
    //  {
    //    defaultEventHandler.Invoke(sender, e as EventArgs);
    //  }
    //  else if (clientHandler is EventHandler<TEventArgs> genericDefaultEventHandler)
    //  {
    //    genericDefaultEventHandler.Invoke(sender, e);
    //  }
    //  else if (clientHandler is Action<TSender, TEventArgs> actionDelegate)
    //  {
    //    actionDelegate.Invoke(sender, e);
    //  }
    //  else
    //  {
    //    _ = clientHandler.DynamicInvoke(e);
    //  }
    //}

    //private void InvokeUnconventionalClientHandler(Delegate clientHandler, params object[] e)
    //{
    //  _ = clientHandler.DynamicInvoke(e);
    //}

    private void OnEventHandlerCustomDynamicSignature(params object[] args)
    {
      if (this.IsPurged)
      {
        return;
      }

      Debug.WriteLine($"Invoke deliver event handler");
      int eventCounter = 0;
      this.ListenerReaderWriterLock.EnterReadLock();
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
              Debug.WriteLine($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown, because handler was dynamically generated as client handler does not follow C# conventions.");
              if (clientHandlerInfos.IsEmpty())
              {
                _ = this.eventListenerHandlerMap.Remove(eventListener);
              }

              continue;
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
        EndService(null);
      }

      this.ListenerReaderWriterLock.ExitReadLock();
    }

    private void EndService(object eventSource)
    {
      Debug.WriteLine("End Service called");
      StopListeningInternal(eventSource);
      WeakEventManagerTable.RemoveWeakEventManager(eventSource, this.EventName);
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