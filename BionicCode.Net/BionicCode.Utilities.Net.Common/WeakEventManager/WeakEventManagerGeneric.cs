namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  public class WeakEventManager<TEventSource, TEventArgs> : WeakEventManager where TEventArgs : EventArgs
  {
    private readonly ConditionalWeakTable<object, HashSet<Action<TEventSource, TEventArgs>>> eventListenerHandlerMap;
    private ReaderWriterLockSlim ListenerReaderWriterLock { get; }
    private string EventName { get; }

    internal WeakEventManager(string eventName)
    {
      this.EventSourceEventInfo = typeof(TEventSource).GetEvent(eventName);
      this.ProxyEventHandler = Delegate.CreateDelegate(this.EventSourceEventInfo.EventHandlerType, this, nameof(OnEvent));
      this.EventName = eventName;
      this.ListenerReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
      this.eventListenerHandlerMap = new ConditionalWeakTable<object, HashSet<Action<TEventSource, TEventArgs>>>();
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, Delegate handler)
    {
#if NET
      ThrowIfInvalidArguments(eventSource, eventName, handler);
#else
      ThrowIfInvalidArguments(eventSource, eventName, handler, nameof(eventSource), nameof(eventName), nameof(handler));
#endif

      if (handler is EventHandler eventHandler)
      {
        AddEventHandler(eventSource, eventName, eventHandler);
        return;
      }

      string validationErrorMessage = $"The custom delegate must meet the common design guidelines for .NET CLR events that is: two parameters, typed and ordered as follows: delegate(sender, e) where parameter 'sender' is either of type {nameof(TEventSource)} or {typeof(object).FullName} and where parameter 'e' is of type {nameof(TEventArgs)} where {nameof(TEventArgs)} must be a subclass of {typeof(EventArgs).FullName}. Events that deviate from this common event guidelines are currently not supported.";
      MethodInfo handlerMethod = handler.Method;
      ParameterInfo[] parameters = handlerMethod.GetParameters();
      if (parameters.Length > 2)
      {
        throw new ArgumentException($"Invalid parameter count. {validationErrorMessage}", nameof(handler));
      }

      Type parameterType = parameters[0].ParameterType;
      if (!(parameterType == typeof(TEventSource) || parameterType == typeof(object)))
      {
        throw new ArgumentException($"Invalid parameter type at parameter index 0. {validationErrorMessage}", nameof(handler));
      }

      parameterType = parameters[1].ParameterType;
      if (!(parameterType == typeof(TEventArgs)
        && parameterType.IsSubclassOf(typeof(EventArgs))))
      {
        throw new ArgumentException($"Invalid parameter type at parameter index 1. {validationErrorMessage}", nameof(handler));
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

      RegisterClientHandler(eventListener, eventHandlerInvocator, eventSource, eventName);
    }

    public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
    {
#if NET
      ThrowIfInvalidArguments(eventSource, eventName, handler);
#else
      ThrowIfInvalidArguments(eventSource, eventName, handler, nameof(eventSource), nameof(eventName), nameof(handler));
#endif

      Action<TEventSource, TEventArgs> eventHandlerInvocator = (sender, e) => handler.Invoke(sender, e);

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, eventSource, eventName);
    }

    private static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler)
    {
      Action<TEventSource, EventArgs> eventHandlerInvocator = (sender, e) => handler.Invoke(sender, e);

      // If the event handler is a static method, the delegate's target is NULL.
      // In this case, we need to provide a placeholder for the WeakTable entry.
      object eventListener = handler.Target ?? DummyEventListenerForStaticEventHandlers.Instance;

      RegisterClientHandler(eventListener, eventHandlerInvocator, eventSource, eventName);
    }

#if NET
    private static void ThrowIfInvalidArguments(TEventSource eventSource, string eventName, Delegate handler, [CallerArgumentExpression(nameof(eventSource))] string eventSourceArgumentName = null, [CallerArgumentExpression(nameof(eventName))] string eventNameArgumentName = null, [CallerArgumentExpression(nameof(handler))] string handlerArgumentAName = null)
#else
    private static void ThrowIfInvalidArguments(TEventSource eventSource, string eventName, Delegate handler, string eventSourceArgumentName = null, string eventNameArgumentName = null, string handlerArgumentAName = null)
#endif
    {
      if (handler is null)
      {
        throw new ArgumentNullException(handlerArgumentAName);
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

    private static void RegisterClientHandler(object eventListener, Action<TEventSource, TEventArgs> eventHandlerInvocator, object eventSource, string eventName)
    {
      if (eventSource is null)
      {
        // If the event is a static event, the eventSource is NULL.
        // In this case, we need to provide a placeholder for the WeakTable entry.
        eventSource = DummyEventSourceForStaticEventHandlers.Instance;
      }

      WeakEventManager<TEventSource, TEventArgs> weakEventManager;
      weakEventManager = WeakEventManagerTable.GetOrCreateWeakEventManager<TEventSource, TEventArgs>(eventSource, eventName);
      weakEventManager.ListenerReaderWriterLock.EnterWriteLock();
      if (!weakEventManager.eventListenerHandlerMap.TryGetValue(eventListener, out HashSet<Action<TEventSource, TEventArgs>> handlerInvocatorList))
      {
        handlerInvocatorList = new HashSet<Action<TEventSource, TEventArgs>>();
        weakEventManager.eventListenerHandlerMap.Add(eventListener, handlerInvocatorList);
        _ = weakEventManager.EventListeners.Add(new WeakReference<object>(eventListener, false));
        weakEventManager.StartListeningInternal(eventSource);
      }

      _ = handlerInvocatorList.Add(eventHandlerInvocator);

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

      if (WeakEventManagerTable.TryGetWeakEventManager(adjustedEventSource, eventName, out WeakEventManager<TEventSource, TEventArgs> weakEventManager))
      {
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
              Debug.WriteLine("Removed event handler");

              if (invocators.Count == 0)
              {
                bool isListenerRemoved = weakEventManager.eventListenerHandlerMap.Remove(eventListener)
                  && (weakEventManager.EventListeners.RemoveWhere(reference => reference.TryGetTarget(out object listener) && ReferenceEquals(listener, eventListener)) > 0);

                Debug.Assert(isListenerRemoved);
              }
            }
          }
        }

        if (!weakEventManager.EventListeners.Any())
        {
          weakEventManager.EndService(adjustedEventSource);
        }
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
        }
      }
#endif

      this.EventListeners.Clear();
      this.ListenerReaderWriterLock.Dispose();
    }

    public void StopListening(TEventSource eventSource)
      => StopListeningInternal(eventSource);

    public void StartListening(TEventSource eventSource)
      => StartListeningInternal(eventSource);

    private void OnEvent(object sender, TEventArgs e)
    {
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
        EndService(sender ?? DummyEventSourceForStaticEventHandlers.Instance);
      }

      this.ListenerReaderWriterLock.ExitReadLock();
    }

    private void EndService(object eventSource)
    {
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