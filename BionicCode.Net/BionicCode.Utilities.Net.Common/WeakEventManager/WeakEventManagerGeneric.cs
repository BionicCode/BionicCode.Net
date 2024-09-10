namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Threading;

  public class WeakEventManager<TEventSource, TEventArgs> : WeakEventManager
  {
    private const string EventDelegateNotSupportedExceptionMessage = "The event delegate must follow the common design guidelines for .NET CLR events that is: two parameters, typed and ordered as follows: delegate(sender, e) where parameter 'sender' is either of type {0} or {1} and where parameter 'e' is of type {2} or {3}, where {3} must be a subclass of {2}. For example: {4}. Events that deviate from this common event guidelines are currently not supported. The found event delegate signature '{5}' violates these guidelines, because {6}.";
    private const string HandlerDelegateSignatureMismatchExceptionMessage = "Event handler delegate signature mismatch. Expected signature as required from event source: '{0}'. Found signature on provided event handler: '{1}'. Because: {2}";
    private const string InternalDelegateSignatureMismatchExceptionMessage = "Internal exception: Event handler delegate signature mismatch. Expected signature as required from event source: '{0}'. Found signature on provided event handler: '{1}'.";
    private const string EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage = "Event delegate signature mismatch. The provided generic type argument '{0}' does not match the type found on the specified event '{1}'. The provided generic type argument '{0}' is '{2}'. But the type found on the event delegate is '{3}'.";
    private readonly ConditionalWeakTable<object, HashSet<Action<TEventSource, TEventArgs>>> eventListenerHandlerMap;
    private ReaderWriterLockSlim ListenerReaderWriterLock { get; }
    private string EventName { get; }

    internal WeakEventManager(string eventName, EventInfo eventInfo)
    {
      // Use BindingFlags.FlattenHierarchy to also get base type static events via the subclass (but only public)
      this.EventSourceEventInfo = eventInfo;

      Type eventHandlerType = this.EventSourceEventInfo.EventHandlerType;
      if (eventHandlerType.GetMethod("Invoke").GetParameters()[0].ParameterType == typeof(object))
      {
        try
        {
          Debug.WriteLine("OnEvent attached to event source");
          this.ProxyEventHandler = Delegate.CreateDelegate(eventHandlerType, this, nameof(OnEvent));
        }
        catch (ArgumentException e)
        {
          string exceptionMessage = string.Format(InternalDelegateSignatureMismatchExceptionMessage, eventHandlerType.ToSignatureName(), GetType().GetMethod(nameof(OnEvent), BindingFlags.NonPublic | BindingFlags.Instance).ToSignatureName());

          throw new EventHandlerMismatchException(exceptionMessage, e);
        }
      }
      else
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

      this.EventName = eventName;
      this.ListenerReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this.eventListenerHandlerMap = new ConditionalWeakTable<object, HashSet<Action<TEventSource, TEventArgs>>>();
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, Delegate handler)
    {
      if (handler is EventHandler eventHandler)
      {
        AddEventHandler(eventSource, eventName, eventHandler);
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

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName);
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
    {
      Action<TEventSource, TEventArgs> eventHandlerInvocator = (sender, e) => handler.Invoke(sender, e);

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName);
    }

    private static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler)
    {
      Action<TEventSource, TEventArgs> eventHandlerInvocator = (sender, e) => handler.Invoke(sender, e as EventArgs);

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, handler, eventSource, eventName);
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

    private static void RegisterClientHandler(object eventListener, Action<TEventSource, TEventArgs> eventHandlerInvocator, Delegate originalHandler, object eventSource, string eventName)
    {
      // Use BindingFlags.FlattenHierarchy to also get base type static events via the subclass (but only public)
      EventInfo eventInfo = typeof(TEventSource).GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
      if (eventInfo is null)
      {
        throw new ArgumentException($"The specified event '{eventName}' on event source type '{typeof(TEventSource).FullName}' could not be found. Please check the provided event name, event source type.");
      }

      ThrowIfInvalidHandler(eventInfo, originalHandler);

      if (eventSource is null)
      {
        // If the event is a static event, the eventSource is NULL.
        // In this case, we need to provide a placeholder for the WeakTable entry.
        eventSource = DummyEventSourceForStaticEventHandlers.Instance;
      }

      WeakEventManager<TEventSource, TEventArgs> weakEventManager;
      weakEventManager = WeakEventManagerTable.GetOrCreateWeakEventManager<TEventSource, TEventArgs>(eventSource, eventName, eventInfo);
      
      if (weakEventManager.IsPurged)
      {
        return;
      }

      weakEventManager.ListenerReaderWriterLock.EnterWriteLock();
      if (!weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<Action<TEventSource, TEventArgs>> handlerInvocatorList))
      {
        handlerInvocatorList = new HashSet<Action<TEventSource, TEventArgs>>();
        weakEventManager.eventListenerHandlerMap.Add(eventListener, handlerInvocatorList);
        WeakReference<object> eventListsnerWeakReference = ManagedWeakTable.GetOrCreateWeakReference(eventListener);
        _ = weakEventManager.EventListeners.Add(eventListsnerWeakReference);
        weakEventManager.StartListeningInternal(eventSource);
      }

      _ = handlerInvocatorList.Add(eventHandlerInvocator);
      Debug.WriteLine(">>> Add event handler");
      registeredEventHandlerCount++;
      Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");
      weakEventManager.ListenerReaderWriterLock.ExitWriteLock();
    }

    public static void RemoveEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
      => RemoveEventHandler(eventSource, eventName, (Delegate)handler);

    public static void RemoveEventHandler(TEventSource eventSource, string eventName, Delegate handler)
    {
      // If the event is a static event, the eventSource is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object adjustedEventSource = eventSource;
      if (eventSource == null)
      {
        adjustedEventSource = DummyEventSourceForStaticEventHandlers.Instance;
      }

      if (!WeakEventManagerTable.TryGetWeakEventManager(adjustedEventSource, eventName, out WeakEventManager<TEventSource, TEventArgs> weakEventManager))
      {
        unregisteredEventHandlerCount++;
        Debug.WriteLine("Unable to remove event handler because event source has expired");
        return;
      }

      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;
      if (weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<Action<TEventSource, TEventArgs>> invocators))
      {
        var invocatorList = invocators.ToList();
        var delegateEqualityComparer = new DelegateSignatureEqualityComparer();
        foreach (Action<TEventSource, TEventArgs> invocator in invocatorList)
        {
          Delegate eventHandler = invocator;

          // Check if the delegate is a closure (created to capture the WeakEventManger's TEventSource and TEventArgs)
          if (invocator.Target != null && invocator.Target.GetType() != eventListener.GetType() && invocator.Target.GetType() != typeof(Delegate))
          {
            FieldInfo handlerField = invocator.Target.GetType().GetField("handler");
            if (handlerField is null)
            {
              continue;
            }

            object originalHandler = handlerField.GetValue(invocator.Target);
            if (!(originalHandler is Delegate invocatorDelegate))
            {
              continue;
            }

            eventHandler = invocatorDelegate;
          }

          if (delegateEqualityComparer.Equals(eventHandler, handler))
          {
            bool isRemoved = invocators.Remove(invocator);
            Debug.Assert(isRemoved);
            Debug.WriteLine("<<< Removed event handler");
            unregisteredEventHandlerCount++;
            Debug.WriteLine($"Registered event handlers: {registeredEventHandlerCount}; Unregistered event handlers: {unregisteredEventHandlerCount}");
            if (invocators.Count == 0)
            {
              bool isListenerRemoved = weakEventManager.eventListenerHandlerMap.Remove(eventListener)
                && (weakEventManager.EventListeners.RemoveWhere(reference => reference.TryGetTarget(out object listener) && ReferenceEquals(listener, eventListener)) > 0);

              Debug.Assert(isListenerRemoved);
            }

            break;
          }
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

    private void OnStronglyTypedEvent(TEventSource sender, TEventArgs e)
    {
      if (this.IsPurged)
      {
        return;
      }

      Debug.WriteLine($"Invoke deliver event habndler");
      int eventCounter = 0;
      this.ListenerReaderWriterLock.EnterReadLock();
      foreach (WeakReference<object> eventListenerReference in this.EventListeners)
      {
        if (!eventListenerReference.TryGetTarget(out object eventListener))
        {
          continue;
        }

        if (this.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<Action<TEventSource, TEventArgs>> clientEventHandlerInvocationList))
        {
          foreach (Action<TEventSource, TEventArgs> invocator in clientEventHandlerInvocationList)
          {
            Debug.WriteLine($"Invoke client ({eventListener.GetType().FullName}) event handler #{eventCounter++}. Event source: {sender?.GetType().FullName ?? "STATIC"}");
            invocator.Invoke(sender, e);
          }
        }
      }

      bool hasListeners = this.EventListeners.Any();
      if (!hasListeners)
      {
        object adjustedEventSource = sender;
        if (adjustedEventSource is null)
        {
          adjustedEventSource = DummyEventSourceForStaticEventHandlers.Instance;
        }

        EndService(adjustedEventSource);
      }

      this.ListenerReaderWriterLock.ExitReadLock();
    }

    private void OnEvent(object sender, TEventArgs e)
    {
      if (this.IsPurged)
      {
        return;
      }

      Debug.WriteLine($"Invoke deliver event habndler");
      int eventCounter = 0;
      this.ListenerReaderWriterLock.EnterReadLock();
      foreach (WeakReference<object> eventListenerReference in this.EventListeners)
      {
        if (!eventListenerReference.TryGetTarget(out object eventListener))
        {
          continue;
        }

        if (this.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<Action<TEventSource, TEventArgs>> clientEventHandlerInvocationList))
        {
          foreach (Action<TEventSource, TEventArgs> invocator in clientEventHandlerInvocationList)
          {
            Debug.WriteLine($"Invoke client ({eventListener.GetType().FullName}) event handler #{eventCounter++}. Event source: {sender?.GetType().FullName ?? "STATIC"}");
            invocator.Invoke((TEventSource)sender, e);
          }
        }
      }

      bool hasListeners = this.EventListeners.Any();
      if (!hasListeners)
      {
        object adjustedEventSource = sender ?? DummyEventSourceForStaticEventHandlers.Instance;
        EndService(adjustedEventSource);
      }

      this.ListenerReaderWriterLock.ExitReadLock();
    }

    private void EndService(object eventSource)
    {
      Debug.WriteLine("End Service called");
      StopListeningInternal(eventSource);
      WeakEventManagerTable.RemoveWeakEventManager<TEventSource>(eventSource, this.EventName);
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
}