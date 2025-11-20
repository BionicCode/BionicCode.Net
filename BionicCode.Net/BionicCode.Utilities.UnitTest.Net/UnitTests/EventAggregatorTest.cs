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
            this.NonGenericEventInvocationCount = 0;
            this.GenericEventInvocationCount = 0;
            this.EventManager = new EventAggregator();
            this.EventSource1 = new TestEventSource1();
            this.EventSource2 = new TestEventSource2();

            _ = this.EventManager.TryRegisterObservable(this.EventSource1, new[] { nameof(this.EventSource1.TestEvent), nameof(this.EventSource1.GenericTestEvent) });

            _ = this.EventManager.TryRegisterObservable(this.EventSource2, new[] { nameof(this.EventSource2.TestEvent), nameof(this.EventSource2.GenericTestEvent) });
        }

        [Fact]
        [Obsolete]
        public void RegisterWrongDelegateSignatureThrowsException()
        {
            _ = this.EventManager.TryRegisterObserver(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), new EventHandler<TestEventArgs>(OnTestEvent));
            _ = this.EventSource1.Invoking(eventSource => eventSource.RaiseAll()).Should().ThrowExactly<WrongEventHandlerSignatureException>();
        }

        [Fact]
        [Obsolete]
        public void RegisterWrongDelegateSignatureThrowsExceptionUsingActionDelegate()
        {
            _ = this.EventManager.TryRegisterObserver<TestEventArgs>(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), OnTestEvent);
            _ = this.EventSource1.Invoking(eventSource => eventSource.RaiseAll()).Should().ThrowExactly<WrongEventHandlerSignatureException>();
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsOfSpecificEventOfKnownInterfaceSource()
        {
            _ = this.EventManager.TryRegisterObserver(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), new EventHandler<EventArgs>(OnTestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsOfSpecificEventOfKnownInterfaceSourceUsingActionDelegate()
        {
            _ = this.EventManager.TryRegisterObserver<EventArgs>(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), OnTestEvent);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfUnknownSource()
        {
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.TestEvent), new EventHandler<EventArgs>(
              OnTestEvent));
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfSpecificEventOfUnknownSourceUsingActionDelegate()
        {
            _ = this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource1.TestEvent), OnTestEvent);
            _ = this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfUnspecificEventOfUnknownSourceButSpecificHandler()
        {
            _ = this.EventManager.TryRegisterGlobalObserver(new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterGlobalObserver(new EventHandler<TestEventArgs>(OnGenericTestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle4EventsOfUnspecificEventOfUnknownSourceButSpecificHandlerUsingActionDelegate()
        {
            _ = this.EventManager.TryRegisterGlobalObserver<EventArgs>(OnTestEvent);
            _ = this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(OnGenericTestEvent);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(2);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveObserver(
              nameof(this.EventSource1.TestEvent),
              this.EventSource1.GetType(),
              new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRemoveObserver(nameof(this.EventSource1.GenericTestEvent), this.EventSource1.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveObserver<EventArgs>(
              nameof(this.EventSource1.TestEvent),
              this.EventSource1.GetType(),
              OnTestEvent);
            _ = this.EventManager.TryRemoveObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), this.EventSource1.GetType(), OnGenericTestEvent);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromSpecificEventsOfAllUnknownSource()
        {
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = this.EventManager.TryRemoveGlobalObserver(
              nameof(this.EventSource1.TestEvent),
              new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRemoveGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(0);
            _ = this.GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromSpecificEventsOfAllUnknownSourceUsingActionDelegate()
        {
            _ = this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource1.TestEvent), OnTestEvent);
            _ = this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

            _ = this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource2.TestEvent), OnTestEvent);
            _ = this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource2.GenericTestEvent), OnGenericTestEvent);

            _ = this.EventManager.TryRemoveGlobalObserver<EventArgs>(
              nameof(this.EventSource1.TestEvent),
              OnTestEvent);
            _ = this.EventManager.TryRemoveGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(0);
            _ = this.GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromUnspecificEventOfAllUnknownSourcesButSpecificHandlers()
        {
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = this.EventManager.TryRemoveGlobalObserver(
              new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRemoveGlobalObserver(new EventHandler<TestEventArgs>(OnGenericTestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(0);
            _ = this.GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void HandleNoEventsAfterUnsubscribingFromUnspecificEventOfAllUnknownSourcesButSpecificHandlersUsingActionDelegate()
        {
            _ = this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource1.TestEvent), OnTestEvent);
            _ = this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

            _ = this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource2.TestEvent), OnTestEvent);
            _ = this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource2.GenericTestEvent), OnGenericTestEvent);

            _ = this.EventManager.TryRemoveGlobalObserver<EventArgs>(
              OnTestEvent);
            _ = this.EventManager.TryRemoveGlobalObserver<TestEventArgs>(OnGenericTestEvent);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(0);
            _ = this.GenericEventInvocationCount.Should().Be(0);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllUnspecificEventsOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveAllObservers(this.EventSource1.GetType());

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllUnspecificEventsOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveAllObservers(this.EventSource1.GetType());

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfAllUnknownSources()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(0);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfAllUnknownSourcesUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent));

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(0);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventAfterUnsubscribingFromSpecificEventOfKnownSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent), this.EventSource1.GetType());

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventAfterUnsubscribingFromSpecificEventOfKnownSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent), this.EventSource1.GetType());

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1SpecificEvent()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1, new[] { nameof(this.EventSource1.TestEvent) });

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1SpecificEventUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1, new[] { nameof(this.EventSource1.TestEvent) });

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownCompleteSource()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownCompleteSourceUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownSourceAllEvents()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1, true);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle2EventsAfterUnregister1KnownSourceAllEventsUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1, true);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(1);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1Event()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1, new[] { nameof(this.EventSource1.TestEvent) }, true);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Fact]
        [Obsolete]
        public void Handle3EventsAfterUnregister1KnownSource1EventUsingActionDelegate()
        {
            RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

            _ = this.EventManager.TryRemoveObservable(this.EventSource1, new[] { nameof(this.EventSource1.TestEvent) }, true);

            this.SenderType = this.EventSource1.GetType();
            this.EventSource1.RaiseAll();

            this.SenderType = this.EventSource2.GetType();
            this.EventSource2.RaiseAll();

            _ = this.NonGenericEventInvocationCount.Should().Be(1);
            _ = this.GenericEventInvocationCount.Should().Be(2);
        }

        [Obsolete]
        private void RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate()
        {
            _ = this.EventManager.TryRegisterObserver(nameof(this.EventSource1.TestEvent), this.EventSource1.GetType(), new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterObserver(nameof(this.EventSource1.GenericTestEvent), this.EventSource1.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));

            _ = this.EventManager.TryRegisterObserver(nameof(this.EventSource2.TestEvent), this.EventSource2.GetType(), new EventHandler<EventArgs>(OnTestEvent));
            _ = this.EventManager.TryRegisterObserver(nameof(this.EventSource2.GenericTestEvent), this.EventSource2.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));
        }

        [Obsolete]
        private void RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate()
        {
            _ = this.EventManager.TryRegisterObserver<EventArgs>(
              nameof(this.EventSource1.TestEvent),
              this.EventSource1.GetType(),
              OnTestEvent);
            _ = this.EventManager.TryRegisterObserver<TestEventArgs>(
              nameof(this.EventSource1.GenericTestEvent),
              this.EventSource1.GetType(),
              OnGenericTestEvent);

            _ = this.EventManager.TryRegisterObserver<EventArgs>(
              nameof(this.EventSource2.TestEvent),
              this.EventSource2.GetType(),
              OnTestEvent);
            _ = this.EventManager.TryRegisterObserver<TestEventArgs>(
              nameof(this.EventSource2.GenericTestEvent),
              this.EventSource2.GetType(),
              OnGenericTestEvent);
        }

        private void OnTestEvent(object sender, EventArgs e)
        {
            this.NonGenericEventInvocationCount++;

            _ = sender.Should().BeOfType(this.SenderType);
        }

        private void OnGenericTestEvent(object sender, TestEventArgs e)
        {
            this.GenericEventInvocationCount++;

            _ = sender.Should().BeOfType(this.SenderType);
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
