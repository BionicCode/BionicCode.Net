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
  public abstract class WeakEventManager
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

    private protected static ConcurrentDictionary<EventInfoTableKey, EventInfoTableEntry> EventInfoTable { get; } = new ConcurrentDictionary<EventInfoTableKey, EventInfoTableEntry>();
    private protected static ConcurrentDictionary<TypeData, MethodData> ProxyEventHandlerPool { get; } = new ConcurrentDictionary<TypeData, MethodData>();
    protected static ConcurrentDictionary<AddClientHandlerInvocatorTableKey, AddClientHandlerInvocatorTableEntry> AddClientHandlerInvocatorTable { get; } = new ConcurrentDictionary<AddClientHandlerInvocatorTableKey, AddClientHandlerInvocatorTableEntry>();
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

    #region ClientHandlerInfoCollection

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

    #endregion ClientHandlerInfoCollection
    
    #region ClientHandlerInfoCollection

    internal class ClientHandlerInfo : IDisposable
    {
      public ClientHandlerInfo(Delegate clientHandler, Action<object, object, ClientHandlerInfo> clientAdapterHandler, SynchronizationContext clientContext)
      {
        this.ClientHandler = WeakReferencePool.GetOrCreate(clientHandler);
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

      public event EventHandler Disposed;
      public bool IsDisposed { get; private set; }
      private WeakReference<object> ClientHandler { get; set; }
      public Action<object, object, ClientHandlerInfo> ClientAdapterHandler { get; }
      public SynchronizationContext ClientContext { get; }
      public bool IsClientHandlerAlive => !this.IsDisposed && TryGetClientHandler(out _);

      protected virtual void Dispose(bool disposing)
      {
        if (!this.IsDisposed)
        {
          if (disposing)
          {
            WeakReferencePool.Add(this.ClientHandler);
            this.ClientHandler = null;
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

    #endregion ClientHandlerInfoCollection

    #region AddClientHandlerInvocatorTableKey

    protected readonly struct AddClientHandlerInvocatorTableKey : IEquatable<AddClientHandlerInvocatorTableKey>
    {
      public AddClientHandlerInvocatorTableKey(Type eventSourceType, Type eventHandlerType)
      {
        this.EventSourceType = eventSourceType;
        this.EventHandlerType = eventHandlerType;
      }

      public Type EventHandlerType { get; }
      public Type EventSourceType { get; }

      public bool Equals(AddClientHandlerInvocatorTableKey other) => other.EventHandlerType.Equals(this.EventHandlerType) && other.EventSourceType.Equals(this.EventSourceType);
      public override bool Equals(object obj) => obj is AddClientHandlerInvocatorTableKey key && Equals(key);

      public override int GetHashCode()
      {
        int hashCode = 433094870;
        hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.EventHandlerType);
        hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(this.EventSourceType);
        return hashCode;
      }

      public static bool operator ==(AddClientHandlerInvocatorTableKey first, AddClientHandlerInvocatorTableKey second) => first.Equals(second);
      public static bool operator !=(AddClientHandlerInvocatorTableKey first, AddClientHandlerInvocatorTableKey second) => !first.Equals(second);
    }

    #endregion AddClientHandlerInvocatorTableKey

    #region AddClientHandlerInvocatorTableEntry

    protected class AddClientHandlerInvocatorTableEntry
    {
      private readonly Delegate addHandlerInvocator;

      public AddClientHandlerInvocatorTableEntry(Delegate addHandlerInvocator, bool useAddCustomHandlerMethod, Type eventSourceType)
      {
        this.addHandlerInvocator = addHandlerInvocator;
        this.UseAddCustomHandlerMethod = useAddCustomHandlerMethod;
        this.EventSourceType = eventSourceType;
      }

      public Action<TEventSource, string, Delegate, SynchronizationContext> GetAddHandlerInvocator<TEventSource>()
        => typeof(TEventSource) != this.EventSourceType
          ? throw new ArgumentException($"Type mismatch for generic type argument {nameof(TEventSource)}. Expected: {this.EventSourceType.FullName}; Found: {typeof(TEventSource).FullName}.")
          : (Action<TEventSource, string, Delegate, SynchronizationContext>)this.addHandlerInvocator;

      public bool UseAddCustomHandlerMethod { get; }
      public Type EventSourceType { get; }
    }

    #endregion AddClientHandlerInvocatorTableEntry
  }
}