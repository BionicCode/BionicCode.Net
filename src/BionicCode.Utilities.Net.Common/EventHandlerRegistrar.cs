namespace BionicCode.Utilities.Net
{
    #region Info
    // //  
    // BionicUtilities.Net.Standard
    #endregion

    using System;
    using System.Threading;

    internal class EventHandlerRegistrar<TEventSource> : ClientEventHandlerRegistrar<TEventSource>
    {
        public EventHandlerRegistrar(EventHandler clientHandler, string eventName) : base(clientHandler, eventName)
        {
        }

        public EventHandlerRegistrar(EventHandler clientHandler, string eventName, SynchronizationContext synchronizationContext) : base(clientHandler, eventName, synchronizationContext)
        {
        }

        public override void RegisterDelegate(TEventSource eventSource)
        {
            if (TryGetClientHandler(out Delegate clientHandler))
            {
                WeakEventManager<TEventSource>.AddEventHandler(eventSource, EventName, (EventHandler)clientHandler, SynchronizationContext);
            }
        }
    }
}
