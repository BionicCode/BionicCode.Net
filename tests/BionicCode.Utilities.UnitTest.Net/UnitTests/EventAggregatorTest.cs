namespace BionicCode.Utilities.Net.UnitTest
{
    using System;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class EventAggregatorTest
    {
        [Obsolete]
        public EventAggregatorTest()
        {
            NonGenericEventInvocationCount = 0;
            GenericEventInvocationCount = 0;
            EventManager = new EventAggregator();
            EventSource1 = new TestEventSource1();
            EventSource2 = new TestEventSource2();

            _ = EventManager.TryRegisterObservable(EventSource1, new[] { nameof(EventSource1.TestEvent), nameof(EventSource1.GenericTestEvent) });

            _ = EventManager.TryRegisterObservable(EventSource2, new[] { nameof(EventSource2.TestEvent), nameof(EventSource2.GenericTestEvent) });
        }

        [Fact]
        [Obsolete]
        public void RegisterWrongDelegateSignatureThrowsException()
        {
            _ = EventManager.TryRegisterObserver(nameof(EventSource1.TestEvent), typeof(ITestEventSource), new EventHandler<TestEventArgs>(OnTestEvent));
            _ = EventSource1.Invoking(eventSource => eventSource.RaiseAll()).Should().ThrowExactly<WrongEventHandlerSignatureException>();
        }

        [Fact]
        [Obsolete]
        public void RegisterWrongDelegateSignatureThrowsExceptionUsingActionDelegate()
        {
            _ = EventManager.TryRegisterObserver<TestEventArgs>(nameof(EventSource1.TestEvent), typeof(ITestEventSource), OnTestEvent);
            _ = EventSource1.Invoking(eventSource => eventSource.RaiseAll()).Should().ThrowExactly<WrongEventHandlerSignatureException>();
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsOfSpecificEventOfKnownInterfaceSource()
        {
            _ = EventManager.TryRegisterObserver(nameof(EventSource1.TestEvent), typeof(ITestEventSource), new EventHandler<EventArgs>(OnTestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsOfSpecificEventOfKnownInterfaceSourceUsingActionDelegate()
        {
            _ = EventManager.TryRegisterObserver<EventArgs>(nameof(EventSource1.TestEvent), typeof(ITestEventSource), OnTestEvent);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfUnknownSource()
        {
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource1.TestEvent), new EventHandler<EventArgs>(
              OnTestEvent));
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfUnknownSourceUsingActionDelegate()
        {
            _ = EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(EventSource1.TestEvent), OnTestEvent);
            _ = EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(EventSource1.GenericTestEvent), OnGenericTestEvent);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfUnspecificEventOfUnknownSourceButSpecificHandler()
        {
            _ = EventManager.TryRegisterGlobalObserver(new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterGlobalObserver(new EventHandler<TestEventArgs>(OnGenericTestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfUnspecificEventOfUnknownSourceButSpecificHandlerUsingActionDelegate()
        {
            _ = EventManager.TryRegisterGlobalObserver<EventArgs>(OnTestEvent);
            _ = EventManager.TryRegisterGlobalObserver<TestEventArgs>(OnGenericTestEvent);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(2);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveObserver(
              nameof(EventSource1.TestEvent),
              EventSource1.GetType(),
              new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRemoveObserver(nameof(EventSource1.GenericTestEvent), EventSource1.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveObserver<EventArgs>(
              nameof(EventSource1.TestEvent),
              EventSource1.GetType(),
              OnTestEvent);
            _ = EventManager.TryRemoveObserver<TestEventArgs>(nameof(EventSource1.GenericTestEvent), EventSource1.GetType(), OnGenericTestEvent);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromSpecificEventsOfAllUnknownSource()
        {
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource1.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource2.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource2.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = EventManager.TryRemoveGlobalObserver(
              nameof(EventSource1.TestEvent),
              new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRemoveGlobalObserver(nameof(EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(0);
            _ = GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromSpecificEventsOfAllUnknownSourceUsingActionDelegate()
        {
            _ = EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(EventSource1.TestEvent), OnTestEvent);
            _ = EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(EventSource1.GenericTestEvent), OnGenericTestEvent);

            _ = EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(EventSource2.TestEvent), OnTestEvent);
            _ = EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(EventSource2.GenericTestEvent), OnGenericTestEvent);

            _ = EventManager.TryRemoveGlobalObserver<EventArgs>(
              nameof(EventSource1.TestEvent),
              OnTestEvent);
            _ = EventManager.TryRemoveGlobalObserver<TestEventArgs>(nameof(EventSource1.GenericTestEvent), OnGenericTestEvent);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(0);
            _ = GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromUnspecificEventOfAllUnknownSourcesButSpecificHandlers()
        {
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource1.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource2.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterGlobalObserver(nameof(EventSource2.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = EventManager.TryRemoveGlobalObserver(
              new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRemoveGlobalObserver(new EventHandler<TestEventArgs>(OnGenericTestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(0);
            _ = GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromUnspecificEventOfAllUnknownSourcesButSpecificHandlersUsingActionDelegate()
        {
            _ = EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(EventSource1.TestEvent), OnTestEvent);
            _ = EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(EventSource1.GenericTestEvent), OnGenericTestEvent);

            _ = EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(EventSource2.TestEvent), OnTestEvent);
            _ = EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(EventSource2.GenericTestEvent), OnGenericTestEvent);

            _ = EventManager.TryRemoveGlobalObserver<EventArgs>(
              OnTestEvent);
            _ = EventManager.TryRemoveGlobalObserver<TestEventArgs>(OnGenericTestEvent);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(0);
            _ = GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllUnspecificEventsOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveAllObservers(EventSource1.GetType());

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllUnspecificEventsOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveAllObservers(EventSource1.GetType());

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfAllUnknownSources()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveAllObservers(nameof(EventSource1.TestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(0);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfAllUnknownSourcesUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveAllObservers(nameof(EventSource1.TestEvent));

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(0);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventAfterUnsubscribingFromSpecificEventOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveAllObservers(nameof(EventSource1.TestEvent), EventSource1.GetType());

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventAfterUnsubscribingFromSpecificEventOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveAllObservers(nameof(EventSource1.TestEvent), EventSource1.GetType());

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1SpecificEvent()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1, new[] { nameof(EventSource1.TestEvent) });

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1SpecificEventUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1, new[] { nameof(EventSource1.TestEvent) });

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownCompleteSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownCompleteSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownSourceAllEvents()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1, true);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownSourceAllEventsUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1, true);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1Event()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1, new[] { nameof(EventSource1.TestEvent) }, true);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1EventUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = EventManager.TryRemoveObservable(EventSource1, new[] { nameof(EventSource1.TestEvent) }, true);

            SenderType = EventSource1.GetType();
            EventSource1.RaiseAll();

            SenderType = EventSource2.GetType();
            EventSource2.RaiseAll();

            _ = NonGenericEventInvocationCount.Should().Be(1);
            _ = GenericEventInvocationCount.Should().Be(2);
        }

        [Obsolete]
        private void RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate()
        {
            _ = EventManager.TryRegisterObserver(nameof(EventSource1.TestEvent), EventSource1.GetType(), new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterObserver(nameof(EventSource1.GenericTestEvent), EventSource1.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = EventManager.TryRegisterObserver(nameof(EventSource2.TestEvent), EventSource2.GetType(), new EventHandler<EventArgs>(OnTestEvent));
            _ = EventManager.TryRegisterObserver(nameof(EventSource2.GenericTestEvent), EventSource2.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));
        }

        [Obsolete]
        private void RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate()
        {
            _ = EventManager.TryRegisterObserver<EventArgs>(
              nameof(EventSource1.TestEvent),
              EventSource1.GetType(),
              OnTestEvent);
            _ = EventManager.TryRegisterObserver<TestEventArgs>(
              nameof(EventSource1.GenericTestEvent),
              EventSource1.GetType(),
              OnGenericTestEvent);

            _ = EventManager.TryRegisterObserver<EventArgs>(
              nameof(EventSource2.TestEvent),
              EventSource2.GetType(),
              OnTestEvent);
            _ = EventManager.TryRegisterObserver<TestEventArgs>(
              nameof(EventSource2.GenericTestEvent),
              EventSource2.GetType(),
              OnGenericTestEvent);
        }

        private void OnTestEvent(object sender, EventArgs e)
        {
            NonGenericEventInvocationCount++;

            _ = sender.Should().BeOfType(SenderType);
        }

        private void OnGenericTestEvent(object sender, TestEventArgs e)
        {
            GenericEventInvocationCount++;

            _ = sender.Should().BeOfType(SenderType);
        }

        private delegate void TestEventHandler(object sender, EventArgs e);
        private int NonGenericEventInvocationCount { get; set; }
        private int GenericEventInvocationCount { get; set; }
        private TestEventSource1 EventSource1 { get; }
        private TestEventSource2 EventSource2 { get; }
        private Type SenderType { get; set; }

        [Obsolete]
        private IEventAggregator EventManager { get; }
    }
}
