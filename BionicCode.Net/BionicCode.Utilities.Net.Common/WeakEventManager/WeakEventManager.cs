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
    private const string EventDelegateNotSupportedExceptionMessage = "The event delegate must follow the common design guidelines for .NET CLR events that is: two parameters, typed and ordered as follows: delegate(sender, e) where parameter 'sender' is either of genericTypeDefinition {0} or {1} and where parameter 'e' is of genericTypeDefinition {2} or {3}, where {3} must be a subclass of {2}. For example: {4}. Events that deviate from this common event guidelines are currently not supported. The found event delegate signature '{5}' violates these guidelines, because {6}.";
    private const string HandlerDelegateSignatureMismatchExceptionMessage = "Event handler delegate signature mismatch. Expected signature as required from event source: '{0}'. Found signature on provided event handler: '{1}'. Because: {2}";
    private const string InternalDelegateSignatureMismatchExceptionMessage = "Internal exception: Event handler delegate signature mismatch. Expected signature as required from event source: '{0}'. Found signature on provided event handler: '{1}'.";
    private const string EventDelegateSignatureMismatchWrongGenericClassTypeParameterExceptionMessage = "Event delegate signature mismatch. The provided generic genericTypeDefinition argument '{0}' does not match the genericTypeDefinition found on the specified event '{1}'. The provided generic genericTypeDefinition argument '{0}' is '{2}'. But the genericTypeDefinition found on the event delegate is '{3}'.";
   
#if DEBUG
    protected static int registeredEventHandlerCount;
    protected static int unregisteredEventHandlerCount;
#endif

    internal static ConcurrentDictionary<TypeData, MethodData> ProxyEventHandlerPool { get; } = new ConcurrentDictionary<TypeData, MethodData>();
    internal static ConcurrentDictionary<Type, Action<object, string, Delegate, SynchronizationContext>> AddClientHandlerInvocators { get; } = new ConcurrentDictionary<Type, Action<object, string, Delegate, SynchronizationContext>>();
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
      public ClientHandlerInfo(Delegate clientHandler, Action<object, object, ClientHandlerInfo> clientAdapterHandler, object eventSource, SynchronizationContext clientContext)
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

      public bool TryGetEventSource(out object eventSource)
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
  }
}