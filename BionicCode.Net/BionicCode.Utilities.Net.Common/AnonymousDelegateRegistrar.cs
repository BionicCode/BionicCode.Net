namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // BionicUtilities.Net.Standard
  #endregion

  using System;
  using System.Diagnostics.Tracing;
  using System.Threading;

  internal class AnonymousDelegateRegistrar<TEventSource> : ClientEventHandlerRegistrar<TEventSource>
  {
    public AnonymousDelegateRegistrar(Delegate clientHandler, string eventName) : base(clientHandler, eventName)
    {
    }

    public AnonymousDelegateRegistrar(Delegate clientHandler, string eventName, SynchronizationContext synchronizationContext) : base(clientHandler, eventName, synchronizationContext)
    {
    }

    public override void RegisterDelegate(TEventSource eventSource)
    {
      if (TryGetClientHandler(out Delegate clientHandler))
      {
        WeakEventManager<TEventSource>.AddCustomEventHandler(eventSource, this.EventName, clientHandler, this.SynchronizationContext);
      }
    }
  }
}