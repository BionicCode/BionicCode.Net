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
        private HashSet<WeakReference<object>> EventListeners { get; }
        private ReaderWriterLockSlim ListenerReaderWriterLock { get; set; }
        private static object SyncLock { get; }
        private static readonly DummyEventSourceForStaticEventHandlers DummyEventSourceForStaticEventHandlers;
        private readonly DummyEventListenerForStaticEventHandlers dummyEventListenerForStaticEventHandlers;
#if DEBUG
        private protected override Type EventSourceType { get; }
#endif

        static WeakEventManager()
        {
            SyncLock = new object();
            DummyEventSourceForStaticEventHandlers = new DummyEventSourceForStaticEventHandlers();
        }

        internal WeakEventManager(string eventName, bool isCustomClientDelegate)
        {
            this.EventListeners = new HashSet<WeakReference<object>>();
            this.ListenerReaderWriterLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            this.eventListenerHandlerMap = new ConditionalWeakTable<object, ClientHandlerInfoCollection>();
            this.dummyEventListenerForStaticEventHandlers = new DummyEventListenerForStaticEventHandlers();

            Type eventSourceType = typeof(TEventSource);
#if DEBUG
            this.EventSourceType = eventSourceType;
#endif

            SymbolReflectionInfoCacheKey key = SymbolReflectionInfoCacheKey.CreateForAnonymousFieldOrEvent(eventSourceType.TypeHandle, eventName, SymbolKind.MemberEvent);
            EventData eventData = SymbolReflectionInfoCache.GetOrCreateEventDataCacheEntry(ref key);
            this.EventSourceEventData = eventData;
            Debug.Assert(this.EventSourceEventData != null);

            TypeData eventHandlerTypeData = this.EventSourceEventData.EventHandlerTypeData;
            MethodData invocatorData = eventHandlerTypeData.DelegateInvokeMethodData;
            ParameterList proxyDelegateParameters = invocatorData.Parameters;
            string proxyDelegateName;

            // Check whether this is a standard EventHandler/EventHandler<TEventArgs> signature (sender, event args) or a custom delegate signature
            if (!isCustomClientDelegate
              && invocatorData.Parameters.Count == 2)
            {
                proxyDelegateName = nameof(OnStronglyTypedEvent);
                LogDebug($"Using '{proxyDelegateName}' event source proxy event handler.");
            }
            else
            {
                proxyDelegateName = nameof(OnEventHandlerCustomDynamicSignature);
                LogDebug("Using dynamically generated event source proxy event handler.");
            }

            try
            {
                this.ProxyEventHandler = ProxyEventHandlerGenerator.Generate<TEventSource>(eventName, this, proxyDelegateName, proxyDelegateParameters);

                Debug.Assert(this.ProxyEventHandler != null);
            }
            catch (ArgumentException e)
            {
                MethodInfo attachingMethodInfo = e.Data[ProxyEventHandlerGenerator.ConflictingMethodInfoExceptionDataKey] as MethodInfo
                  ?? GetType().GetMethod(proxyDelegateName, BindingFlags.NonPublic | BindingFlags.Instance);
                string exceptionMessage = string.Format(InternalDelegateSignatureMismatchExceptionMessage, eventData.EventInvokerMethodData.RuntimeShortSignature, attachingMethodInfo.ToRuntimeSignatureShortName());
                throw new EventHandlerMismatchException(exceptionMessage, e);
            }

            this.EventName = eventName;
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
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
            ArgumentNullExceptionAdvanced.ThrowIfNull(handler, nameof(handler));

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
            static void eventHandlerInvocator(object sender, object[] e, ClientHandlerInfo handlerInfo)
            {
                if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
                {
                    //MethodInfo invokeMethod = clientHandler.GetType().GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName);
                    //_ = invokeMethod.Invoke(clientHandler.Target, new object[] { e });
                    _ = clientHandler.DynamicInvoke(e);
                }
            }

            RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: true, (TEventSource)eventSource, eventName, synchronizationContext);
        }

        //public static void AddEventHandler<TEventArgs>(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
        //  => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

        private static void AddGenericEventHandler<TEventArgs>(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler, SynchronizationContext synchronizationContext)
        {
            static void eventHandlerInvocator(object sender, object[] e, ClientHandlerInfo handlerInfo)
            {
                if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
                {
                    var eventHandler = (EventHandler<TEventArgs>)clientHandler;
                    eventHandler.Invoke(sender, (TEventArgs)e.FirstOrDefault());
                }
            }

            RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: false, eventSource, eventName, synchronizationContext);
        }

        //public static void AddActionHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, bool executeOnCurrentSynchronizationContext = false)
        //  => AddActionHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

        private static void AddActionHandler<TSender, TEventArgs>(TEventSource eventSource, string eventName, Action<TSender, TEventArgs> handler, SynchronizationContext synchronizationContext)
        {
            static void eventHandlerInvocator(object sender, object[] e, ClientHandlerInfo handlerInfo)
            {
                if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
                {
                    var eventHandler = (Action<TSender, TEventArgs>)clientHandler;
                    eventHandler.Invoke((TSender)sender, (TEventArgs)e.FirstOrDefault());
                }
            }

            RegisterClientHandler(eventHandlerInvocator, handler, isCustomClientDelegate: false, eventSource, eventName, synchronizationContext);
        }

        //public static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, bool executeOnCurrentSynchronizationContext = false)
        //  => AddEventHandler(eventSource, eventName, handler, executeOnCurrentSynchronizationContext ? SynchronizationContext.Current : null);

        private static void AddEventHandler(TEventSource eventSource, string eventName, EventHandler handler, SynchronizationContext synchronizationContext)
        {
            static void eventHandlerInvocator(object sender, object[] e, ClientHandlerInfo handlerInfo)
            {
                if (handlerInfo.TryGetClientHandler(out Delegate clientHandler))
                {
                    var eventHandler = (EventHandler)clientHandler;
                    eventHandler.Invoke(sender, e.FirstOrDefault() as EventArgs);
                }
            }

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

        private static void RegisterClientHandler(Action<object, object[], ClientHandlerInfo> clientHandlerAdapterInvocator, Delegate clientHandler, bool isCustomClientDelegate, object eventSource, string eventName, SynchronizationContext capturedSynchronizationContext)
        {
            WeakEventManager<TEventSource> weakEventManager;
            lock (ManagedWeakTable.TableLock)
            {
                // If the event is a static event, the eventSource is NULL.
                object adjustedEventSource = eventSource ?? WeakEventManager<TEventSource>.DummyEventSourceForStaticEventHandlers;

                weakEventManager = WeakEventManagerTable.GetOrCreateWeakEventManager<TEventSource>(adjustedEventSource, eventName, isCustomClientDelegate);
                ArgumentExceptionAdvanced.ThrowIfEventHandlerNotAssignable(clientHandler, weakEventManager.EventSourceEventData);

                weakEventManager.RegisterHandler(clientHandlerAdapterInvocator, clientHandler, adjustedEventSource, capturedSynchronizationContext);
            }
        }

        //public static void RemoveEventHandler(TEventSource eventSource, string eventName, EventHandler<TEventArgs> handler)
        //  => RemoveEventHandler(eventSource, eventName, (Delegate)handler);

        public static void RemoveEventHandler<THandler>(TEventSource eventSource, string eventName, THandler handler) where THandler : Delegate
        {
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));
            ArgumentNullExceptionAdvanced.ThrowIfNull(handler, nameof(handler));

            lock (ManagedWeakTable.TableLock)
            {
                // If the event is a static event, the eventSource is NULL.
                object adjustedEventSource = (object)eventSource ?? WeakEventManager<TEventSource>.DummyEventSourceForStaticEventHandlers;

                if (!WeakEventManagerTable.TryGetWeakEventManager(adjustedEventSource, eventName, out WeakEventManager<TEventSource> weakEventManager))
                {
#if DEBUG
                    Debug.WriteLine($"Unable to remove event handler because event source has expired or the event was never registered and therefore the WeakEventManager instance has bee garbage collected.");
#endif
                    return;
                }

                weakEventManager.UnregisterHandler(handler, adjustedEventSource);
            }
        }

        private void RegisterHandler(Action<object, object[], ClientHandlerInfo> clientHandlerAdapterInvocator, Delegate clientHandler, object eventSource, SynchronizationContext capturedSynchronizationContext)
        {
            try
            {
                this.ListenerReaderWriterLock.EnterWriteLock();

                // If the event handler is a static method, the delegate's target is NULL.
                // In this case, we need to provide a placeholder for the WeakTable entry.
                object eventListener = clientHandler.Target ?? this.dummyEventListenerForStaticEventHandlers;

                if (!this.eventListenerHandlerMap.TryGetValue(eventListener, out ClientHandlerInfoCollection clientHandlerInfos))
                {
                    clientHandlerInfos = new ClientHandlerInfoCollection();
                    this.eventListenerHandlerMap.Add(eventListener, clientHandlerInfos);
                    WeakReference<object> eventListenerWeakReference = ManagedWeakTable.GetOrCreateWeakReference(eventListener);
                    _ = this.EventListeners.Add(eventListenerWeakReference);
                    StartListeningInternal(eventSource is DummyEventSourceForStaticEventHandlers ? null : eventSource);
                }

                var clientHandlerInfo = new ClientHandlerInfo(clientHandler, clientHandlerAdapterInvocator, capturedSynchronizationContext);
                clientHandlerInfos.Add(clientHandlerInfo);

#if DEBUG
                LogDebug($">>> Add client event handler.");
                this.registeredEventHandlerCount++;
                LogDebug($"Registered client event handlers: {this.registeredEventHandlerCount}; Unregistered client event handlers: {this.unregisteredEventHandlerCount}.");
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

                object eventListener = handler.Target ?? this.dummyEventListenerForStaticEventHandlers;
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
                            if (originalHandler is not Delegate invocatorDelegate)
                            {
                                continue;
                            }

                            eventHandler = invocatorDelegate;
                        }

                        if (delegateEqualityComparer.Equals(eventHandler, handler))
                        {
                            clientHandlerInfos.Remove(handlerInfo);
                            handlerInfo.Dispose();

#if DEBUG
                            LogDebug($"<<< Removed client event handler.");
                            this.unregisteredEventHandlerCount++;
                            LogDebug($"Registered client event handlers: {this.registeredEventHandlerCount}; Unregistered client event handlers: {this.unregisteredEventHandlerCount}.");
#endif
                            break;
                        }
                    }

                    if (clientHandlerInfos.Count == 0)
                    {
                        /* Force cleanup instead of waiting for garbage collection to free and safe resources */

                        WeakReference<object> eventListenerWeakReference = this.EventListeners.FirstOrDefault(reference => reference.TryGetTarget(out object listener) && ReferenceEquals(listener, eventListener));
                        if (eventListenerWeakReference != null)
                        {
                            this.EventListeners.Remove(eventListenerWeakReference);
                            ManagedWeakTable.RecycleWeakReference(eventListenerWeakReference);
                        }

                        bool isListenerRemoved = this.eventListenerHandlerMap.Remove(eventListener)
                          && eventListenerWeakReference != null;

                        Debug.Assert(isListenerRemoved);
                        LogDebug($"Retained client event handlers in collection: {this.EventListeners.Count}.");
                    }
                }

                if (!this.EventListeners.Any())
                {
                    LogDebug($"Empty client handler list ==> call End Service from RemoveEventHandler() API.");

                    // Force cleanup instead of waiting for garbage collection to free and safe resources
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

            LogDebug($"Internal purge called. Is listening: {this.IsListening}.");
            LogDebug($"Stopping WeakEventManager and clearing {this.EventListeners.Count} event listener entries from {nameof(this.eventListenerHandlerMap)}.");

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

        private void OnStronglyTypedEvent<TSender, TEventArgs>(TSender sender, TEventArgs e)
        {
            LogDebug($"Invoking proxy event handler and deliver event to client.");

            if (this.IsPurged)
            {
                LogDebug($"Invoked proxy event handler of already purged WeakEventManager instance.");

                EndService(sender);
                return;
            }

            int eventCounter = 0;
            try
            {
                this.ListenerReaderWriterLock.EnterUpgradeableReadLock();

                HashSet<WeakReference<object>> eventListeners = this.EventListeners;
                foreach (WeakReference<object> eventListenerReference in eventListeners)
                {
                    if (!eventListenerReference.TryGetTarget(out object eventListener))
                    {
                        _ = this.EventListeners.Remove(eventListenerReference);
                        ManagedWeakTable.RecycleWeakReference(eventListenerReference);

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

                                    LogDebug($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");

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

                            LogDebug($"Invoke client handler. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: {sender?.GetType().FullName ?? "STATIC"}");

                            if (handlerInfo.ClientContext != null)
                            {
                                handlerInfo.ClientContext.Send(state => handlerInfo.ClientAdapterHandler.Invoke(sender, new object[] { e }, handlerInfo), null);
                            }
                            else
                            {
                                handlerInfo.ClientAdapterHandler.Invoke(sender, new object[] { e }, handlerInfo);
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
            LogDebug($"Invoking proxy event handler and deliver event to client.");

            var tableKey = new ManagedWeakTableKey(this.EventName, typeof(TEventSource));
            if (!(ManagedWeakTable.TryGetEntry(this.Id, tableKey, out ManagedWeakTableEntry entry)
              && entry.TryGetReferenceTarget(out object eventSource)))
            {
                return;
            }

            if (this.IsPurged)
            {
                LogDebug($"Invoked proxy event handler of already purged WeakEventManager instance.");

                EndService(eventSource);
            }

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

                                    LogDebug($"Skip client handler invocation because the client's delegate has been garbage collected. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown from custom event delegate.");

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

                            LogDebug($"Invoke client handler. Client: {eventListener.GetType().FullName}; Event handler #: {eventCounter++}; Event source: unknown from custom event delegate.");

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
            LogDebug($"End Service called.");

            StopListeningInternal(eventSource is DummyEventSourceForStaticEventHandlers ? null : eventSource);
            WeakEventManagerTable.RemoveWeakEventManager<TEventSource>(this.Id, this.EventName);
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
              && !this.ListenerReaderWriterLock.IsWriteLockHeld && this.ListenerReaderWriterLock.WaitingWriteCount == 0
              && !this.ListenerReaderWriterLock.IsUpgradeableReadLockHeld && this.ListenerReaderWriterLock.WaitingUpgradeCount == 0)
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
        //public static readonly object Instance = new DummyEventListenerForStaticEventHandlers();
    }

    /// <summary>
    /// Event listener for static events (eventListenerHandlerMap that don't have a instance as target) used as key for storing static handlers in a table.
    /// </summary>
    internal class DummyEventSourceForStaticEventHandlers
    {
        //public static readonly object Instance = new DummyEventSourceForStaticEventHandlers();
    }
}
