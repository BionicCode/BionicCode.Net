namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;

  internal abstract class ClientEventHandlerRegistrar<TEventSource> : IClientEventHandlerRegistrar<TEventSource>
  {
    protected Delegate ClientHandler { get; }
    protected string EventName { get; }
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

    public bool ContainsDelegate(Delegate handler) => Delegate.Equals(handler, this.ClientHandler);
  }
}