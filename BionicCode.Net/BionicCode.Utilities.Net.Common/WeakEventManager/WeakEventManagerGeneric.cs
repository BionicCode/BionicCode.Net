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
    internal class ClientHandlerInfoCollection : IEnumerable<ClientHandlerInfo>
    {
      public int Count => this.items.Count;
      private readonly List<ClientHandlerInfo> items;

      public ClientHandlerInfoCollection()
      {
        this.items = new List<ClientHandlerInfo>();
      }

      public IEnumerable<ClientHandlerInfo> EnumerateSafe()
      {
        var itemsCopy = this.items.ToList();
        for (int index = itemsCopy.Count - 1; index >= 0; index--)
        {
          ClientHandlerInfo item = itemsCopy[index];
          if (item.IsClientHandlerAlive)
          {
            yield return item;
          }
        }
      }

      public void Add(ClientHandlerInfo clientHandlerInfo)
      {
        StartListeningToItem(clientHandlerInfo);
        this.items.Add(clientHandlerInfo);
      }

      public void Remove(ClientHandlerInfo clientHandlerInfo) 
        => clientHandlerInfo.Dispose();

      public void Clear()
      {
        for (int index = this.items.Count - 1; index >= 0; index--)
        {
          ClientHandlerInfo item = this.items[index];
          StopListeningToItem(item);
          item.Dispose();
          this.items.RemoveAt(index);
        }
      }

      private void OnItemDisposed(object sender, EventArgs e)
      {
        var item = (ClientHandlerInfo)sender;
        StopListeningToItem(item);
        _ = this.items.Remove(item);
      }

      private void StartListeningToItem(ClientHandlerInfo clientHandlerInfo) 
        => clientHandlerInfo.Disposed += OnItemDisposed;

      private void StopListeningToItem(ClientHandlerInfo clientHandlerInfo)
        => clientHandlerInfo.Disposed -= OnItemDisposed;

      IEnumerator<ClientHandlerInfo> IEnumerable<ClientHandlerInfo>.GetEnumerator()
      {
        foreach (ClientHandlerInfo item in EnumerateSafe())
        {
          yield return item;
        }
      }

      IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<ClientHandlerInfo>)this).GetEnumerator();
    }

    internal class ClientHandlerInfo : IDisposable
    {
      public ClientHandlerInfo(Delegate clientHandler, Action<object, object, ClientHandlerInfo> clientAdapterHandler, TEventSource eventSource, SynchronizationContext clientContext)
      {
        this.ClientHandler = WeakReferencePool.GetOrCreate(clientHandler);
        this.EventSource = WeakReferencePool.GetOrCreate(eventSource);
        this.ClientAdapterHandler = clientAdapterHandler;
        this.ClientContext = clientContext;
      }

      private void OnDisposed()
        => this.Disposed?.Invoke(this, EventArgs.Empty);

      public void Clear()
        => Dispose();

      public bool TryGetClientHandler(out Delegate handler)
      {
        handler = null;
        if (this.IsDisposed || this.ClientHandler is null)
        {
          return false;
        }

        if (this.ClientHandler.TryGetTarget(out object target) && target is Delegate clientHandler)
        {
          handler = clientHandler;
        }
        else
        {
          Dispose();
        }

        return handler != null;
      }

      public bool TryGetEventSource(out TEventSource eventSource)
      {
        eventSource = default;
        if (this.IsDisposed || this.EventSource is null)
        {
          return false;
        }

        if (this.EventSource.TryGetTarget(out object target) && target is TEventSource livingEventSource)
        {
          eventSource = livingEventSource;
        }
        else
        {
          Dispose();
        }

        return eventSource != null;
      }

      public event EventHandler Disposed;
      public bool IsDisposed { get; private set; }
      private WeakReference<object> ClientHandler { get; set; }
      public Action<object, object, ClientHandlerInfo> ClientAdapterHandler { get; }
      private WeakReference<object> EventSource { get; set; }
      public SynchronizationContext ClientContext { get; }
      public bool IsClientHandlerAlive => !this.IsDisposed && TryGetClientHandler(out _) && TryGetEventSource(out _);

      protected virtual void Dispose(bool disposing)
      {
        if (!this.IsDisposed)
        {
          if (disposing)
          {
            WeakReferencePool.Add(this.ClientHandler);
            this.ClientHandler = null;
            WeakReferencePool.Add(this.EventSource);
            this.EventSource = null;
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          this.IsDisposed = true;
          OnDisposed();
        }
      }

      // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~ClientHandlerInfo()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose()
      {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
      }
    }

    private const string EventDelegateNotSupportedExceptionMessage = "The event delegate must follow the common design guidelines for .NET CLR events that is: two parameters, typed and ordered as follows: delegate(sender, e) where parameter 'sender' is either of type {0} or {1} and where parameter 'e' is of type {2} or {3}, where {3} must be a subclass of {2}. For example: {4}. Events that deviate from this common event guidelines are currently not supported. The found event delegate signature '{5}' violates these guidelines, because {6}.";
    private const string HandlerDelegateSignatureMismatchExceptionMessage = "Event handler delegate signature mismatch. Expected signature as required from event source: '{0}'. Found signature on provided event handler: '{1}'. Because: {2}";
    private const string InternalDelegateSignatureMismatchExceptionMessage = "Internal exception: Event handler delegate signature mismatch. Expected signature as required from event source: '{0}'. Found signature on provided event handler: '{1}'.";
    private const string EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage = "Event delegate signature mismatch. The provided generic type argument '{0}' does not match the type found on the specified event '{1}'. The provided generic type argument '{0}' is '{2}'. But the type found on the event delegate is '{3}'.";
    private readonly ConditionalWeakTable<object, ClientHandlerInfoCollection> eventListenerHandlerMap;
    private ReaderWriterLockSlim ListenerReaderWriterLock { get; }
    private string EventName { get; }

    private static readonly MethodInfo genericHandlerMethod = typeof(WeakEventManager<TEventSource>).GetMethod(nameof(OnStronglyTypedEvent), BindingFlags.Instance | BindingFlags.NonPublic);

    internal WeakEventManager(string eventName)
    {
      // Use BindingFlags.FlattenHierarchy to also get base type static events via the subclass (including protected events of the hierarchy and private events of the current type)
      this.EventSourceEventInfo = typeof(TEventSource).GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      if (this.EventSourceEventInfo is null)
      {
        throw new ArgumentException($"The specified event '{eventName}' on event source type '{typeof(TEventSource).FullName}' could not be found. Please check the provided event name, event source type.");
      }

      Type eventHandlerType = this.EventSourceEventInfo.EventHandlerType;
      TypeData eventHandlerTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventHandlerType);
      if (!WeakEventManager<TEventSource>.ProxyEventHandlerPool.TryGetValue(eventHandlerTypeData, out MethodData handlerMethodData))
      {
        MethodData invocatorData = eventHandlerTypeData.DelegateInvokeMethodData;
        ParameterData[] eventHandlerParameters = invocatorData.Parameters;
        MethodInfo handlerMethodInfo = WeakEventManager<TEventSource>.genericHandlerMethod.MakeGenericMethod(eventHandlerParameters[0].ParameterTypeData.GetType(), eventHandlerParameters[1].ParameterTypeData.GetType());

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

    public static void AddEventHandler(TEventSource eventSource, string eventName, Delegate handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler(TEventSource eventSource, string eventName, Delegate handler, SynchronizationContext synchronizationContext)
    {
      Type eventhandlerType = handler.GetType();
      if (handler is EventHandler eventHandler)
      {
        AddEventHandler(eventSource, eventName, eventHandler, synchronizationContext);
        return;
      }
      else if (eventhandlerType.IsGenericType && eventhandlerType.GetGenericTypeDefinition() == typeof(EventHandler<>))
      {
        Type argsType = eventhandlerType.GetGenericArguments()[0];
        Type[] methodParameters = new Type[] { typeof(TEventSource), typeof(string), typeof(EventHandler<>), typeof(SynchronizationContext) };
        _ = typeof(WeakEventManager<>).GetMethod("AddEventHandler", methodParameters)
          .MakeGenericMethod(argsType)
          .Invoke(null, new object[] { eventSource, eventName, eventhandlerType, synchronizationContext });
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

    public static void AddEventHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, SynchronizationContext synchronizationContext)
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
      //  throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter at index '0' is not of type {nameof(TEventSource)} or {typeof(object).FullName}"));
      //}

      //if (!typeof(EventArgs).IsAssignableFrom(eventDelegateMethodParameters[1].ParameterType))
      //{
      //  throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter at index '1' is not of type or derived from type {typeof(EventArgs).FullName}"));
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
            $"Unable to cast parameter of type {eventDelegateParameterType.FullName} at parameter index {parameterIndex} of the event delegate to type {eventHandlerParameterType.FullName} at parameter index {parameterIndex} of the provided event handler."));
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