namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Reflection;

  /// <summary>
  /// Provides a base class for the event manager that is used in the weak event pattern. The manager adds and removes listenerHandlerMap for events (or callbacks) that also use the pattern.
  /// </summary>
  public abstract class WeakEventManager
  {
    protected static int registeredEventHandlerCount;
    protected static int unregisteredEventHandlerCount;
    public bool IsListening { get; private set; }
    public bool IsPurged { get; protected set; }
    protected Delegate ProxyEventHandler { get; set; }
    protected EventInfo EventSourceEventInfo { get; set; }
    protected HashSet<WeakReference<object>> EventListeners { get; }

    protected WeakEventManager()
      => this.EventListeners = new HashSet<WeakReference<object>>();

    internal abstract void Purge();

    internal void StartListeningInternal(object eventSource)
    {
      if (this.IsListening)
      {
        return;
      }

      Debug.WriteLine($"WeakEventManager starts listening to {eventSource.GetType().FullName}");
      this.EventSourceEventInfo.AddEventHandler(eventSource, this.ProxyEventHandler);
      this.IsListening = true;
    }

    internal void StopListeningInternal(object eventSource)
    {
      Debug.WriteLine($"WeakEventManager stops listening to {eventSource.GetType().FullName}");
      this.EventSourceEventInfo.RemoveEventHandler(eventSource, this.ProxyEventHandler);
      this.IsListening = false;
    }
  }
}