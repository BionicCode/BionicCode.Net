namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  public class WeakEventManager<TEventSource, TEventArgs> : WeakEventManager
  {
    internal class ClientHandlerInfo : IDisposable
    {
      public bool IsDisposed { get; private set; }

      public ClientHandlerInfo(Delegate clientHandler, TEventSource eventSource, SynchronizationContext clientContext)
      {
        WeakReference<object> clientHandlerWeakReference = WeakReferencePool.GetOrCreate(clientHandler);
        this.ClientHandler = clientHandlerWeakReference;
        WeakReference<object> eventSourceWeakReference = WeakReferencePool.GetOrCreate(eventSource);
        this.EventSource = eventSourceWeakReference;
        this.ClientContext = clientContext;
      }

      public void Clear()
      {
        WeakReferencePool.Add(this.ClientHandler);
        this.ClientHandler = null;
        WeakReferencePool.Add(this.EventSource);
        this.EventSource = null;
      }

      public bool TryGetClientHandler(out Delegate handler)
      {
        handler = null;
        if (this.ClientHandler is null)
        {
          return false;
        }

        if (this.ClientHandler.TryGetTarget(out object target) && target is Delegate clientHandler)
        {
          handler = clientHandler;
        }
        else
        {
          WeakReferencePool.Add(this.ClientHandler);
          this.ClientHandler = null;
        }

        return handler != null;
      }

      public bool TryGetEventSource(out TEventSource eventSource)
      {
        eventSource = default;
        if (this.EventSource is null)
        {
          return false;
        }

        if (this.EventSource.TryGetTarget(out object target) && target is TEventSource livingEventSource)
        {
          eventSource = livingEventSource;
        }
        else
        {
          WeakReferencePool.Add(this.EventSource);
          this.EventSource = null;
        }

        return eventSource != null;
      }

      private WeakReference<object> ClientHandler { get; set; }
      private WeakReference<object> EventSource { get; set; }
      public SynchronizationContext ClientContext { get; }

      protected virtual void Dispose(bool disposing)
      {
        if (!this.IsDisposed)
        {
          if (disposing)
          {
            Clear();
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          this.IsDisposed = true;
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
    private readonly ConditionalWeakTable<object, HashSet<ClientHandlerInfo>> eventListenerHandlerMap;
    private ReaderWriterLockSlim ListenerReaderWriterLock { get; }
    private string EventName { get; }

    internal WeakEventManager(string eventName, EventInfo eventInfo)
    {
      // Use BindingFlags.FlattenHierarchy to also get base type static events via the subclass (but only public)
      this.EventSourceEventInfo = eventInfo;

      Type eventHandlerType = this.EventSourceEventInfo.EventHandlerType;
      MethodInfo invocator = eventHandlerType.GetMethod("Invoke");
      ParameterInfo[] eventHandlerParameters = invocator.GetParameters();
      if (eventHandlerParameters.Length == 2)
      {
        try
        {
          Debug.WriteLine("OnStronglyTypedEvent attached to event source");
          this.ProxyEventHandler = Delegate.CreateDelegate(eventHandlerType, this, nameof(OnStronglyTypedEvent));
        }
        catch (ArgumentException e)
        {
          string exceptionMessage = string.Format(InternalDelegateSignatureMismatchExceptionMessage, eventHandlerType.ToSignatureName(), GetType().GetMethod(nameof(OnStronglyTypedEvent), BindingFlags.NonPublic | BindingFlags.Instance).ToSignatureName());

          throw new EventHandlerMismatchException(exceptionMessage, e);
        }
      }
      else
      {
        Debug.WriteLine("Dynamically generated source event handler attached to event source");
        this.ProxyEventHandler = GenerateEventHandler(eventHandlerParameters);
      }

      this.EventName = eventName;
      this.ListenerReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this.eventListenerHandlerMap = new ConditionalWeakTable<object, HashSet<ClientHandlerInfo>>();
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
      MethodCallExpression method = Expression.Call(GetType().GetMethod(nameof(OnEventHandlerCustomDynamicSignature)), argsArray);
      eventSourceHandler = Expression.Lambda(method, expressionParameters).Compile();

      return eventSourceHandler;
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, Delegate handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler(TEventSource eventSource, string eventName, Delegate handler, SynchronizationContext synchronizationContext)
    {
      if (handler is EventHandler eventHandler)
      {
        AddEventHandler(eventSource, eventName, eventHandler, synchronizationContext);
        return;
      }

      MethodInfo invokeMethod = handler.GetType().GetMethod("Invoke");
      Action<TEventSource, TEventArgs> eventHandlerInvocator;
      if (invokeMethod != null)
      {
        eventHandlerInvocator = (sender, e) => _ = invokeMethod.Invoke(handler, new object[] { sender, e });
      }
      else
      {
        eventHandlerInvocator = (sender, e) => _ = handler.DynamicInvoke(sender, e);
      }

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, SynchronizationContext synchronizationContext)
    {
      Action<TEventSource, TEventArgs> eventHandlerInvocator = (sender, e) => handler.Invoke(sender, e);

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    public static void AddEventHandler(TEventSource eventSource, string eventName, Action<TEventSource, TEventArgs> handler, SynchronizationContext synchronizationContext)
    {
      Action<TEventSource, TEventArgs> eventHandlerInvocator = handler;

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, Action<TEventSource, TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
      => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

    private static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, SynchronizationContext synchronizationContext)
    {
      Action<TEventSource, TEventArgs> eventHandlerInvocator = (sender, e) => handler.Invoke(sender, e as EventArgs);

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName, synchronizationContext);
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

    private static void ThrowIfInvalidHandler(EventInfo eventInfo, Delegate handler)
    {
      MethodInfo eventDelegateInvokeMethod = eventInfo.EventHandlerType.GetMethod("Invoke");
      ParameterInfo[] eventDelegateMethodParameters = eventDelegateInvokeMethod.GetParameters();

      /* Validate the event */

      if (eventDelegateMethodParameters.Length != 2)
      {
        throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter count is {eventDelegateMethodParameters.Length} instead of 2"));
      }

      if (!(eventDelegateMethodParameters[0].ParameterType == typeof(TEventSource) 
        || eventDelegateMethodParameters[0].ParameterType == typeof(object)))
      {
        throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter at index '0' is not of type {nameof(TEventSource)} or {typeof(object).FullName}"));
      }

      //if (!typeof(EventArgs).IsAssignableFrom(eventDelegateMethodParameters[1].ParameterType))
      //{
      //  throw new EventDelegateNotSupportedException(string.Format(EventDelegateNotSupportedExceptionMessage, nameof(TEventSource), typeof(object).FullName, typeof(EventArgs).FullName, nameof(TEventArgs), typeof(EventHandler).ToSignatureName(), eventInfo.EventHandlerType.ToSignatureName(), $"the parameter at index '1' is not of type or derived from type {typeof(EventArgs).FullName}"));
      //}

      if (eventDelegateMethodParameters[1].ParameterType != typeof(TEventArgs))
      {
        throw new EventDelegateMismatchException(string.Format(EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage, nameof(TEventArgs), eventInfo.Name, typeof(TEventArgs), eventDelegateMethodParameters[1].ParameterType.FullName));
      }

      MethodInfo eventHandlerMethod = handler.Method;
      ParameterInfo[] eventHandlerMethodParameters = eventHandlerMethod.GetParameters();

      /* Validate the event EventHandler */

      if (eventDelegateMethodParameters.Length != eventHandlerMethodParameters.Length)
      {
        throw new EventHandlerMismatchException(string.Format(HandlerDelegateSignatureMismatchExceptionMessage, 
          eventInfo.EventHandlerType.ToSignatureName(),
          eventHandlerMethod.ToSignatureName(),
          $"Invalid parameter count."));
      }

      for (int parameterIndex = 0; parameterIndex < eventDelegateMethodParameters.Length; parameterIndex++)
      {
        Type eventDelegateParameterType = eventDelegateMethodParameters[parameterIndex].ParameterType;
        Type eventHandlerParameterType = eventHandlerMethodParameters[parameterIndex].ParameterType;
        if (!eventHandlerParameterType.IsAssignableFrom(eventDelegateParameterType))
        {
          throw new EventHandlerMismatchException(string.Format(HandlerDelegateSignatureMismatchExceptionMessage,
            eventInfo.EventHandlerType.ToSignatureName(),
            eventHandlerMethod.ToSignatureName(),
            $"Unable to cast parameter of type {eventDelegateParameterType.FullName} at parameter index {parameterIndex} of the event delegate to type {eventHandlerParameterType.FullName} at parameter index {parameterIndex} of the provided event handler."));
        }
      }
    }

    private static void RegisterClientHandler(object eventListener, Action<TEventSource, TEventArgs> eventHandlerInvocator, Delegate originalHandler, TEventSource eventSource, string eventName, SynchronizationContext capturedSynchronizationContext)
    {
      // Use BindingFlags.FlattenHierarchy to also get base type static events via the subclass (but only public)
      EventInfo eventInfo = typeof(TEventSource).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      if (eventInfo is null)
      {
        throw new ArgumentException($"The specified event '{eventName}' on event source type '{typeof(TEventSource).FullName}' could not be found. Please check the provided event name, event source type.");
      }

      ThrowIfInvalidHandler(eventInfo, originalHandler);

      WeakEventManager<TEventSource, TEventArgs> weakEventManager;
      weakEventManager = WeakEventManagerTable.GetOrCreateWeakEventManager<TEventSource, TEventArgs>(eventSource, eventName, eventInfo);
      
      if (weakEventManager.IsPurged)
      {
        return;
      }

      weakEventManager.ListenerReaderWriterLock.EnterWriteLock();
      if (!weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<ClientHandlerInfo> clientHandlerInfos))
      {
        clientHandlerInfos = new HashSet<ClientHandlerInfo>();
        weakEventManager.eventListenerHandlerMap.Add(eventListener, clientHandlerInfos);
        WeakReference<object> eventListenerWeakReference = ManagedWeakTable.GetOrCreateWeakReference(eventListener);
        _ = weakEventManager.EventListeners.Add(eventListenerWeakReference);
        weakEventManager.StartListeningInternal(eventSource);
      }

      var clientHandlerInfo = new ClientHandlerInfo(eventHandlerInvocator, eventSource, capturedSynchronizationContext);
      _ = clientHandlerInfos.Add(clientHandlerInfo);
      Debug.WriteLine(">>> Add event handler");
      registeredEventHandlerCount++;
      Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");
      weakEventManager.ListenerReaderWriterLock.ExitWriteLock();
    }

    public static void RemoveEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
      => RemoveEventHandler(eventSource, eventName, (Delegate)handler);

    public static void RemoveEventHandler(TEventSource eventSource, string eventName, Delegate handler)
    {
      if (!WeakEventManagerTable.TryGetWeakEventManager(eventSource, eventName, out WeakEventManager<TEventSource, TEventArgs> weakEventManager))
      {
        unregisteredEventHandlerCount++;
        Debug.WriteLine("Unable to remove event handler because event source has expired");
        return;
      }

      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;
      if (weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<ClientHandlerInfo> clientHandlerInfos))
      {
        List<ClientHandlerInfo> handlerInfos = clientHandlerInfos.ToList();
        var delegateEqualityComparer = new DelegateSignatureEqualityComparer();
        foreach (ClientHandlerInfo handlerInfo in handlerInfos)
        {
          if (!handlerInfo.TryGetClientHandler(out Delegate eventHandler))
          {
            _ = clientHandlerInfos.Remove(handlerInfo);
            handlerInfo.Dispose();
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
            bool isRemoved = clientHandlerInfos.Remove(handlerInfo);
            handlerInfo.Dispose();
            Debug.Assert(isRemoved);
            Debug.WriteLine("<<< Removed event handler");
            unregisteredEventHandlerCount++;
            Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");

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
        weakEventManager.EndService(eventSource);
      }
    }

    internal override void Purge()
    {
      Debug.WriteLine($"WeakEventManager internal purge. Is listening: {this.IsListening}");
      Debug.WriteLine($"Stopping WeakEventManager and clearing {this.EventListeners.Count} event listener entries from {nameof(this.eventListenerHandlerMap)}");
#if NETSTANDARD2_1_OR_GREATER || NET
      this.eventListenerHandlerMap.Clear();
#else
      foreach (WeakReference<object> reference in this.EventListeners)
      {
        if (reference.TryGetTarget(out object evenListener))
        {
          _ = this.eventListenerHandlerMap.Remove(evenListener);
          ManagedWeakTable.RecycleWeakReference(reference);
        }
      }
#endif

      this.EventListeners.Clear();
      this.ListenerReaderWriterLock.Dispose();
      this.IsPurged = true;
    }

    public void StopListening(TEventSource eventSource)
      => StopListeningInternal(eventSource);

    public void StartListening(TEventSource eventSource)
      => StartListeningInternal(eventSource);

    private void OnStronglyTypedEvent<TSender>(TSender sender, TEventArgs e)
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

        if (this.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<ClientHandlerInfo> clientHandlerInfos))
        {
          foreach (ClientHandlerInfo handlerInfo in clientHandlerInfos)
          {
            if (!handlerInfo.TryGetClientHandler(out Delegate clientHandler))
            {
              Debug.WriteLine($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");
              continue;
            }

            Debug.WriteLine($"Invoke client handler. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");
            
            if (handlerInfo.ClientContext != null)
            {
              handlerInfo.ClientContext.Send(state => InvokeClientHandler(clientHandler, sender, e), null);
            }
            else
            {
              InvokeClientHandler(clientHandler, sender, e);
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

    private void InvokeClientHandler<TSender>(Delegate clientHandler,  TSender sender, TEventArgs e)
    {
      if (clientHandler is EventHandler defaultEventHandler)
      {
        defaultEventHandler.Invoke(sender, e as EventArgs);
      }
      else if (clientHandler is EventHandler<TEventArgs> genericDefaultEventHandler)
      {
        genericDefaultEventHandler.Invoke(sender, e);
      }
      else if (clientHandler is Action<TSender, TEventArgs> actionDelegate)
      {
        actionDelegate.Invoke(sender, e);
      }
      else
      {
        _ = clientHandler.DynamicInvoke(e);
      }
    }

    private void InvokeUnconventionalClientHandler(Delegate clientHandler, params object[] e)
    {
      _ = clientHandler.DynamicInvoke(e);
    }

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

        if (this.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<ClientHandlerInfo> clientHandlerInfos))
        {
          foreach (ClientHandlerInfo handlerInfo in clientHandlerInfos)
          {
            if (!handlerInfo.TryGetClientHandler(out Delegate clientHandler))
            {
              Debug.WriteLine($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown, because handler was dynamically generated as client handler does not follow C# conventions.");
              continue;
            }

            Debug.WriteLine($"Invoke client handler. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown, because handler was dynamically generated as client handler does not follow C# conventions.");

            if (handlerInfo.ClientContext != null)
            {
              handlerInfo.ClientContext.Send(state => InvokeUnconventionalClientHandler(clientHandler, args), null);
            }
            else
            {
              InvokeUnconventionalClientHandler(clientHandler, args);
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