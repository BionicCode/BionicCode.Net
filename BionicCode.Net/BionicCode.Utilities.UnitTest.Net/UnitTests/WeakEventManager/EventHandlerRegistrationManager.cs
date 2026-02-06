namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;

    public class EventHandlerRegistrationManager
    {
        public int RegisteredEventHandlerCount { get; private set; }
        private readonly List<EventHandlerInfo<TestEventSource1>> testEventSource1RegisteredEventHandlerInfo;
        private readonly List<EventHandlerInfo<TestEventSource2>> testEventSource2RegisteredEventHandlerInfo;

        public EventHandlerRegistrationManager()
        {
            testEventSource1RegisteredEventHandlerInfo = new List<EventHandlerInfo<TestEventSource1>>();
            testEventSource2RegisteredEventHandlerInfo = new List<EventHandlerInfo<TestEventSource2>>();
        }

        public EventHandlerInfo<TestEventSource1> RegisterEventHandler<TEventHandler>(TestEventSource1 eventSource, string eventName, TEventHandler eventHandler) where TEventHandler : Delegate
        {
            WeakEventManager<TestEventSource1>.AddEventHandler(eventSource, eventName, eventHandler);
            var eventHandlerInfo = new EventHandlerInfo<TestEventSource1>(eventSource, eventName, eventHandler);
            testEventSource1RegisteredEventHandlerInfo.Add(eventHandlerInfo);
            ++RegisteredEventHandlerCount;

            return eventHandlerInfo;
        }

        public EventHandlerInfo<TestEventSource1> RegisterEventHandlerWithSynchronizationContext<TEventHandler>(TestEventSource1 eventSource, string eventName, TEventHandler eventHandler, SynchronizationContext synchronizationContext) where TEventHandler : Delegate
        {
            WeakEventManager<TestEventSource1>.AddEventHandler(eventSource, eventName, eventHandler, synchronizationContext);
            var eventHandlerInfo = new EventHandlerInfo<TestEventSource1>(eventSource, eventName, eventHandler);
            testEventSource1RegisteredEventHandlerInfo.Add(eventHandlerInfo);
            ++RegisteredEventHandlerCount;

            return eventHandlerInfo;
        }

        public EventHandlerInfo<TestEventSource1> RegisterEventHandlerWithCurrentSynchronizationContext<TEventHandler>(TestEventSource1 eventSource, string eventName, TEventHandler eventHandler) where TEventHandler : Delegate
        {
            WeakEventManager<TestEventSource1>.AddEventHandler(eventSource, eventName, eventHandler, executeOnCurrentSynchronizationContext: true);
            var eventHandlerInfo = new EventHandlerInfo<TestEventSource1>(eventSource, eventName, eventHandler);
            testEventSource1RegisteredEventHandlerInfo.Add(eventHandlerInfo);
            ++RegisteredEventHandlerCount;

            return eventHandlerInfo;
        }

        public void RegisterEventHandlerWithoutEventSource<TEventHandler>(TestEventSource1 eventSource, string eventName, TEventHandler eventHandler) where TEventHandler : Delegate => WeakEventManager<TestEventSource1>.AddEventHandler(eventSource, eventName, eventHandler);//++RegisteredEventHandlerCount;

        public EventHandlerInfo<TestEventSource2> RegisterEventHandler<TEventHandler>(TestEventSource2 eventSource, string eventName, TEventHandler eventHandler) where TEventHandler : Delegate
        {
            WeakEventManager<TestEventSource2>.AddEventHandler(eventSource, eventName, eventHandler);
            var eventHandlerInfo = new EventHandlerInfo<TestEventSource2>(eventSource, eventName, eventHandler);
            testEventSource2RegisteredEventHandlerInfo.Add(eventHandlerInfo);
            ++RegisteredEventHandlerCount;

            return eventHandlerInfo;
        }

        public void UnregisterEventHandler(EventHandlerInfo<TestEventSource1> eventHandlerInfo)
        {
            WeakEventManager<TestEventSource1>.RemoveEventHandler(eventHandlerInfo.EventSource, eventHandlerInfo.EventName, eventHandlerInfo.EventHandler);
            if (testEventSource1RegisteredEventHandlerInfo.Remove(eventHandlerInfo))
            {
                --RegisteredEventHandlerCount;
            }
        }

        public void UnregisterEventHandler(EventHandlerInfo<TestEventSource2> eventHandlerInfo)
        {
            WeakEventManager<TestEventSource2>.RemoveEventHandler(eventHandlerInfo.EventSource, eventHandlerInfo.EventName, eventHandlerInfo.EventHandler);
            if (testEventSource2RegisteredEventHandlerInfo.Remove(eventHandlerInfo))
            {
                --RegisteredEventHandlerCount;
            }
        }

        public void UnregisterAllEventHandlers()
        {
            UnregisterAllEventHandlersEventSource1();
            UnregisterAllEventHandlersEventSource2();

            _ = RegisteredEventHandlerCount.Should().Be(0);
        }

        public void UnregisterAllEventHandlersEventSource1()
        {
            foreach (EventHandlerInfo<TestEventSource1> eventHandlerInfo in testEventSource1RegisteredEventHandlerInfo)
            {
                WeakEventManager<TestEventSource1>.RemoveEventHandler(eventHandlerInfo.EventSource, eventHandlerInfo.EventName, eventHandlerInfo.EventHandler);
                --RegisteredEventHandlerCount;
            }

            testEventSource1RegisteredEventHandlerInfo.Clear();
        }

        public void UnregisterAllEventHandlersEventSource2()
        {
            foreach (EventHandlerInfo<TestEventSource2> eventHandlerInfo in testEventSource2RegisteredEventHandlerInfo)
            {
                WeakEventManager<TestEventSource2>.RemoveEventHandler(eventHandlerInfo.EventSource, eventHandlerInfo.EventName, eventHandlerInfo.EventHandler);
                --RegisteredEventHandlerCount;
            }

            testEventSource2RegisteredEventHandlerInfo.Clear();
        }
    }
}
