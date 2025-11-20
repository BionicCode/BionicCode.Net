namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
    using System;

    public readonly struct EventHandlerInfo<TEventsSource>
    {
        public EventHandlerInfo(TEventsSource eventSource, string eventName, Delegate eventHandler)
        {
            this.EventSource = eventSource;
            this.EventHandler = eventHandler;
            this.EventName = eventName;
        }

        public TEventsSource EventSource { get; }
        public Delegate EventHandler { get; }
        public string EventName { get; }
    }
}
