namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Threading;

    /// <summary>
    /// Provides a base class for the event manager that is used in the weak event pattern. The manager adds and removes listenerHandlerMap for events (or callbacks) that also use the pattern.
    /// </summary>
    public abstract partial class WeakEventManager
    {
        private protected const string EventDelegateNotSupportedExceptionMessage = "The event delegate must follow the common design guidelines for .NET CLR events that is: two parameters, typed and ordered as follows: delegate(sender, e) where parameter 'sender' is either of genericTypeDefinition {0} or {1} and where parameter 'e' is of genericTypeDefinition {2} or {3}, where {3} must be a subclass of {2}. For example: {4}. Events that deviate from this common event guidelines are currently not supported. The found event delegate signature '{5}' violates these guidelines, because {6}.";
        private protected const string InternalDelegateSignatureMismatchExceptionMessage = "Internal exception: Event handler delegate signature mismatch. Expected signature as required by event source: '{0}'. Found signature on provided event handler: '{1}'.";
        private protected const string EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage = "Event delegate signature mismatch. The provided generic genericTypeDefinition argument '{0}' does not match the genericTypeDefinition found on the specified event '{1}'. The provided generic genericTypeDefinition argument '{0}' is '{2}'. But the genericTypeDefinition found on the event delegate is '{3}'.";

#if DEBUG
        internal static int InstanceCounter { get; private set; }
        internal int InstanceNumber { get; }
        private protected int registeredEventHandlerCount;
        private protected int unregisteredEventHandlerCount;
        private protected abstract Type EventSourceType { get; }
#endif

        internal Guid Id { get; }

        private protected static ConcurrentDictionary<AddClientHandlerInvocatorTableKey, AddClientHandlerInvocatorTableEntry> AddClientHandlerInvocatorTable { get; } = new ConcurrentDictionary<AddClientHandlerInvocatorTableKey, AddClientHandlerInvocatorTableEntry>();
        public bool IsListening { get; private set; }
        public bool IsPurged { get; protected set; }
        protected Delegate ProxyEventHandler { get; set; }
        private protected EventData EventSourceEventData { get; set; }

        protected WeakEventManager()
        {
            Id = Guid.NewGuid();

#if DEBUG
            InstanceNumber = ++InstanceCounter;
#endif
        }

        internal abstract void Purge();

#if DEBUG
        internal void LogDebug(string message) => Debug.WriteLine($"[ThreadID: {Thread.CurrentThread.ManagedThreadId}] [ID: {Id}] [{nameof(WeakEventManager)} instance #{InstanceNumber} of {WeakEventManager.InstanceCounter}]: {message}");
#endif

        internal void StartListeningInternal(object eventSource)
        {
            if (IsListening)
            {
                return;
            }

#if DEBUG
            string eventSourceTypeName = $"{EventSourceType.FullName} {(eventSource is null ? "(static event)" : string.Empty)}";
            LogDebug($"Attaching proxy handler to {eventSourceTypeName}.");
            LogDebug($"Start listening to {eventSourceTypeName}.");
#endif

            EventSourceEventData.AddEventHandler(eventSource, ProxyEventHandler);
            IsListening = true;
        }

        internal void StopListeningInternal(object eventSource)
        {
#if DEBUG
            string eventSourceTypeName = $"{EventSourceType.FullName} {(eventSource is null ? "(static event)" : string.Empty)}";
            LogDebug($"Detaching proxy handler from {eventSourceTypeName}.");
            LogDebug($"Stop listening to {eventSourceTypeName}.");
#endif

            EventSourceEventData.RemoveEventHandler(eventSource, ProxyEventHandler);
            IsListening = false;
        }
    }
}
