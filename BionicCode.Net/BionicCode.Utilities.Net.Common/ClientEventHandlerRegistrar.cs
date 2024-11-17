namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;

  internal abstract class ClientEventHandlerRegistrar<TEventSource> : IClientEventHandlerRegistrar
  {
    public string EventName { get; }
    protected Delegate ClientHandler { get; }
    protected SynchronizationContext SynchronizationContext { get; }

    protected ClientEventHandlerRegistrar(Delegate clientHandler, string eventName) : this(clientHandler, eventName, null)
    {
    }
    protected ClientEventHandlerRegistrar(Delegate clientHandler, string eventName, SynchronizationContext synchronizationContext)
    {
      this.ClientHandler = clientHandler;
      this.EventName = eventName;
      this.SynchronizationContext = synchronizationContext;
    }

    public abstract void RegisterDelegate(TEventSource eventSource);

    public virtual void UnregisterDelegate(TEventSource eventSource)
      => WeakEventManager<TEventSource>.RemoveEventHandler(eventSource, this.EventName, this.ClientHandler);

    public void RegisterDelegate(object eventSource)
    => RegisterDelegate((TEventSource)eventSource);

    public virtual void UnregisterDelegate(object eventSource)
      => UnregisterDelegate((TEventSource)eventSource);

    public bool ContainsDelegate(Delegate handler) => Delegate.Equals(handler, this.ClientHandler);
  }
}