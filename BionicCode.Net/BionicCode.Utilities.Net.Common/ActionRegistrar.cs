namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Threading;

  internal class ActionRegistrar<TEventSource, TSender, TEventArgs> : ClientEventHandlerRegistrar<TEventSource>
  {
    public ActionRegistrar(Action<TSender, TEventArgs> clientHandler, string eventName) : base(clientHandler, eventName)
    {
    }

    public ActionRegistrar(Action<TSender, TEventArgs> clientHandler, string eventName, SynchronizationContext synchronizationContext) : base(clientHandler, eventName, synchronizationContext)
    {
    }

    public override void RegisterDelegate(TEventSource eventSource)
    {
      if (TryGetClientHandler(out Delegate clientHandler))
      {
        WeakEventManager<TEventSource>.AddEventHandler(eventSource, this.EventName, (Action<TSender, TEventArgs>)clientHandler, this.SynchronizationContext);
      }
    }
  }
}