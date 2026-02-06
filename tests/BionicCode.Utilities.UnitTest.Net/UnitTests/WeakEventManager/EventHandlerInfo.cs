namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
    using System;

    public readonly struct EventHandlerInfo<TEventsSource>
    {
        public EventHandlerInfo(TEventsSource eventSource, string eventName, Delegate eventHandler)
        {
            EventSource = eventSource;
            EventHandler = eventHandler;
            EventName = eventName;
        }

        public TEventsSource EventSource { get; }
        public Delegate EventHandler { get; }
        public string EventName { get; }
    }
}
