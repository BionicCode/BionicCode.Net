namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading;

  /// <summary>
  /// Provides a base class for the event manager that is used in the weak event pattern. The manager adds and removes listenerHandlerMap for events (or callbacks) that also use the pattern.
  /// </summary>
  public abstract partial class WeakEventManager
  {
    protected const string EventDelegateNotSupportedExceptionMessage = "The event delegate must follow the common design guidelines for .NET CLR events that is: two parameters, typed and ordered as follows: delegate(sender, e) where parameter 'sender' is either of genericTypeDefinition {0} or {1} and where parameter 'e' is of genericTypeDefinition {2} or {3}, where {3} must be a subclass of {2}. For example: {4}. Events that deviate from this common event guidelines are currently not supported. The found event delegate signature '{5}' violates these guidelines, because {6}.";
    protected const string HandlerDelegateSignatureMismatchExceptionMessage = "Event handler delegate signature mismatch. Expected signature as required by event source: '{0}'. Found signature on provided event handler: '{1}'. Because: {2}";
    protected const string InternalDelegateSignatureMismatchExceptionMessage = "Internal exception: Event handler delegate signature mismatch. Expected signature as required by event source: '{0}'. Found signature on provided event handler: '{1}'.";
    protected const string EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage = "Event delegate signature mismatch. The provided generic genericTypeDefinition argument '{0}' does not match the genericTypeDefinition found on the specified event '{1}'. The provided generic genericTypeDefinition argument '{0}' is '{2}'. But the genericTypeDefinition found on the event delegate is '{3}'.";

#if DEBUG
    internal static int InstanceCounter { get; private set; }
    internal int InstanceNumber { get; }
    protected static int registeredEventHandlerCount;
    protected static int unregisteredEventHandlerCount;
    private protected abstract Type EventSourceType { get; }
#endif

    internal Guid EventSourceId { get; }

    //private protected static ConcurrentDictionary<EventInfoTableKey, EventInfoTableEntry> EventInfoTable { get; } = new ConcurrentDictionary<EventInfoTableKey, EventInfoTableEntry>();
    private protected static ConcurrentDictionary<TypeData, MethodData> ProxyEventHandlerPool { get; } = new ConcurrentDictionary<TypeData, MethodData>();
    private protected static ConcurrentDictionary<AddClientHandlerInvocatorTableKey, AddClientHandlerInvocatorTableEntry> AddClientHandlerInvocatorTable { get; } = new ConcurrentDictionary<AddClientHandlerInvocatorTableKey, AddClientHandlerInvocatorTableEntry>();
    public bool IsListening { get; private set; }
    public bool IsPurged { get; protected set; }
    protected Delegate ProxyEventHandler { get; set; }
    private protected EventData EventSourceEventData { get; set; }
    protected HashSet<WeakReference<object>> EventListeners { get; }

    protected WeakEventManager()
    {
      this.EventListeners = new HashSet<WeakReference<object>>();
      this.EventSourceId = new Guid();

#if DEBUG
      this.InstanceNumber = ++InstanceCounter;
#endif
    }

    internal abstract void Purge();

    internal void LogDebug(string message)
    {
#if DEBUG
      Debug.WriteLine($"{nameof(WeakEventManager)} instance #{this.InstanceNumber} of {WeakEventManager.InstanceCounter}: {message}");
#endif
    }

    internal void StartListeningInternal(object eventSource)
    {
      if (this.IsListening)
      {
        return;
      }

#if DEBUG
      string eventSourceTypeName = $"{this.EventSourceType.FullName} {(eventSource is null ? "(static event)" : string.Empty)}";
      LogDebug($"Attaching proxy handler to {eventSourceTypeName}.");
      LogDebug($"Start listening to {eventSourceTypeName}.");
#endif

      this.EventSourceEventData.AddEventHandler(eventSource, this.ProxyEventHandler);
      this.IsListening = true;
    }

    internal void StopListeningInternal(object eventSource)
    {
#if DEBUG
      string eventSourceTypeName = $"{this.EventSourceType.FullName} {(eventSource is null ? "(static event)" : string.Empty)}";
      LogDebug($"Detaching proxy handler from {eventSourceTypeName}.");
      LogDebug($"Stop listening to {eventSourceTypeName}.");
#endif

      this.EventSourceEventData.RemoveEventHandler(eventSource, this.ProxyEventHandler);
      this.IsListening = false;
    }
  }
}