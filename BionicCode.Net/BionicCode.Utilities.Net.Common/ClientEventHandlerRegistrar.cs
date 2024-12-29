namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;

  internal abstract class ClientEventHandlerRegistrar<TEventSource> : IClientEventHandlerRegistrar
  {
    public string EventName { get; }
    public Type EventSourceType => typeof(TEventSource);
    protected WeakReference<object> ClientHandler { get; }
    protected SynchronizationContext SynchronizationContext { get; }

    protected ClientEventHandlerRegistrar(Delegate clientHandler, string eventName) : this(clientHandler, eventName, null)
    {
    }

    protected ClientEventHandlerRegistrar(Delegate clientHandler, string eventName, SynchronizationContext synchronizationContext)
    {
      this.ClientHandler = WeakReferencePool.GetOrCreate(clientHandler);
      this.EventName = eventName;
      this.SynchronizationContext = synchronizationContext;
    }

    public abstract void RegisterDelegate(TEventSource eventSource);

    public virtual void UnregisterDelegate(TEventSource eventSource)
    {
      if (TryGetClientHandler(out Delegate clientHandler))
      {
        WeakEventManager<TEventSource>.RemoveEventHandler(eventSource, this.EventName, clientHandler);
      }
    }

    public void RegisterDelegate(object eventSource)
      => RegisterDelegate((TEventSource)eventSource);

    public virtual void UnregisterDelegate(object eventSource)
      => UnregisterDelegate((TEventSource)eventSource);

    public bool ContainsDelegate(Delegate handler) => TryGetClientHandler(out Delegate clientHandler) && Delegate.Equals(handler, clientHandler);

    public bool TryGetClientHandler(out Delegate clientHandler)
    {
      clientHandler = null;
      if (this.ClientHandler.TryGetTarget(out object handlerReference) 
        && handlerReference is Delegate handler)
      {
        clientHandler = handler;
      }

      return clientHandler != null;
    }
  }
}