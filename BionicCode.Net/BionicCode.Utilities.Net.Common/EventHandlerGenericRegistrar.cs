namespace BionicCode.Utilities.Net
{
    #region Info
    // //  
    // BionicUtilities.Net.Standard
    #endregion

    using System;
    using System.Threading;

    internal class EventHandlerGenericRegistrar<TEventSource, TEventArgs> : ClientEventHandlerRegistrar<TEventSource>
    {
        public EventHandlerGenericRegistrar(EventHandler<TEventArgs> clientHandler, string eventName) : base(clientHandler, eventName)
        {
        }

        public EventHandlerGenericRegistrar(EventHandler<TEventArgs> clientHandler, string eventName, SynchronizationContext synchronizationContext) : base(clientHandler, eventName, synchronizationContext)
        {
        }

        public override void RegisterDelegate(TEventSource eventSource)
        {
            if (TryGetClientHandler(out Delegate clientHandler))
            {
                WeakEventManager<TEventSource>.AddEventHandler(eventSource, EventName, (EventHandler<TEventArgs>)clientHandler, SynchronizationContext);
            }
        }
    }
}
