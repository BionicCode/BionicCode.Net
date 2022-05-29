namespace BionicCode.Utilities.UnitTest.Net.Standard
{
  using System;
  using BionicCode.Utilities.Net;
  using Xunit;
  using FluentAssertions;
  using BionicCode.Utilities.UnitTest.Net.Standard.Resources;



  public class EventAggregatorTest
  {    
    public EventAggregatorTest()
    {
      this.NonGenericEventInvocationCount = 0;
      this.GenericEventInvocationCount = 0;
      this.EventManager = new EventAggregator();
      this.EventSource1 = new TestEventSource();
      this.EventSource2 = new TestEventSource2();

      this.EventManager.TryRegisterObservable(this.EventSource1, new[] { nameof(this.EventSource1.TestEvent), nameof(this.EventSource1.GenericTestEvent) });

      this.EventManager.TryRegisterObservable(this.EventSource2, new[] { nameof(this.EventSource2.TestEvent), nameof(this.EventSource2.GenericTestEvent) });
    }

    [Fact]
    public void RegisterWrongDelegateSignatureThrowsException()
    {
      this.EventManager.TryRegisterObserver(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), new EventHandler<TestEventArgs>(OnTestEvent));
      this.EventSource1.Invoking(eventSource => eventSource.RaiseAll()).Should().ThrowExactly<WrongEventHandlerSignatureException>();
    }

    [Fact]
    public void RegisterWrongDelegateSignatureThrowsExceptionUsingActionDelegate()
    {
      this.EventManager.TryRegisterObserver<TestEventArgs>(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), OnTestEvent);
      this.EventSource1.Invoking(eventSource => eventSource.RaiseAll()).Should().ThrowExactly<WrongEventHandlerSignatureException>();
    }

    [Fact]
    public void Handle2EventsOfSpecificEventOfKnownInterfaceSource()
    {
      this.EventManager.TryRegisterObserver(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), new EventHandler<EventArgs>(OnTestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void Handle2EventsOfSpecificEventOfKnownInterfaceSourceUsingActionDelegate()
    {
      this.EventManager.TryRegisterObserver<EventArgs>(nameof(this.EventSource1.TestEvent), typeof(ITestEventSource), OnTestEvent);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void Handle4EventsOfSpecificEventOfKnownSource()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle4EventsOfSpecificEventOfKnownSourceUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle4EventsOfSpecificEventOfUnknownSource()
    {
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.TestEvent), new EventHandler<EventArgs>(
        OnTestEvent));
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle4EventsOfSpecificEventOfUnknownSourceUsingActionDelegate()
    {
      this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource1.TestEvent), OnTestEvent);
      this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle4EventsOfUnspecificEventOfUnknownSourceButSpecificHandler()
    {
      this.EventManager.TryRegisterGlobalObserver(new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterGlobalObserver(new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle4EventsOfUnspecificEventOfUnknownSourceButSpecificHandlerUsingActionDelegate()
    {
      this.EventManager.TryRegisterGlobalObserver<EventArgs>(OnTestEvent);
      this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(OnGenericTestEvent);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(2);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfKnownSource()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveObserver(
        nameof(this.EventSource1.TestEvent),
        this.EventSource1.GetType(),
        new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRemoveObserver(nameof(this.EventSource1.GenericTestEvent), this.EventSource1.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfKnownSourceUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveObserver<EventArgs>(
        nameof(this.EventSource1.TestEvent),
        this.EventSource1.GetType(),
        OnTestEvent);
      this.EventManager.TryRemoveObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), this.EventSource1.GetType(), OnGenericTestEvent);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void HandleNoEventsAfterUnsubscribingFromSpecificEventsOfAllUnknownSource()
    {
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.EventManager.TryRemoveGlobalObserver(
        nameof(this.EventSource1.TestEvent),
        new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRemoveGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(0);
      this.GenericEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void HandleNoEventsAfterUnsubscribingFromSpecificEventsOfAllUnknownSourceUsingActionDelegate()
    {
      this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource1.TestEvent), OnTestEvent);
      this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

      this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource2.TestEvent), OnTestEvent);
      this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource2.GenericTestEvent), OnGenericTestEvent);

      this.EventManager.TryRemoveGlobalObserver<EventArgs>(
        nameof(this.EventSource1.TestEvent),
        OnTestEvent);
      this.EventManager.TryRemoveGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(0);
      this.GenericEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void HandleNoEventsAfterUnsubscribingFromUnspecificEventOfAllUnknownSourcesButSpecificHandlers()
    {
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource1.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.TestEvent), new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterGlobalObserver(nameof(this.EventSource2.GenericTestEvent), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.EventManager.TryRemoveGlobalObserver(
        new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRemoveGlobalObserver(new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(0);
      this.GenericEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void HandleNoEventsAfterUnsubscribingFromUnspecificEventOfAllUnknownSourcesButSpecificHandlersUsingActionDelegate()
    {
      this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource1.TestEvent), OnTestEvent);
      this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource1.GenericTestEvent), OnGenericTestEvent);

      this.EventManager.TryRegisterGlobalObserver<EventArgs>(nameof(this.EventSource2.TestEvent), OnTestEvent);
      this.EventManager.TryRegisterGlobalObserver<TestEventArgs>(nameof(this.EventSource2.GenericTestEvent), OnGenericTestEvent);

      this.EventManager.TryRemoveGlobalObserver<EventArgs>(
        OnTestEvent);
      this.EventManager.TryRemoveGlobalObserver<TestEventArgs>(OnGenericTestEvent);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(0);
      this.GenericEventInvocationCount.Should().Be(0);
    }

    [Fact]
    public void Handle2EventsAfterUnsubscribingFromAllUnspecificEventsOfKnownSource()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveAllObservers(this.EventSource1.GetType());

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle2EventsAfterUnsubscribingFromAllUnspecificEventsOfKnownSourceUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveAllObservers(this.EventSource1.GetType());

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfAllUnknownSources()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(0);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle2EventsAfterUnsubscribingFromAllSpecificEventsOfAllUnknownSourcesUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent));

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(0);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle3EventAfterUnsubscribingFromSpecificEventOfKnownSource()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent), this.EventSource1.GetType());

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle3EventAfterUnsubscribingFromSpecificEventOfKnownSourceUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveAllObservers(nameof(this.EventSource1.TestEvent), this.EventSource1.GetType());

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle3EventsAfterUnregister1KnownSource1SpecificEvent()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1, new []{ nameof(this.EventSource1.TestEvent) });

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle3EventsAfterUnregister1KnownSource1SpecificEventUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1, new []{ nameof(this.EventSource1.TestEvent) });

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle2EventsAfterUnregister1KnownCompleteSource()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle2EventsAfterUnregister1KnownCompleteSourceUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle2EventsAfterUnregister1KnownSourceAllEvents()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1, true);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle2EventsAfterUnregister1KnownSourceAllEventsUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1, true);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(1);
    }

    [Fact]
    public void Handle3EventsAfterUnregister1KnownSource1Event()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1, new []{ nameof(this.EventSource1.TestEvent)}, true);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    [Fact]
    public void Handle3EventsAfterUnregister1KnownSource1EventUsingActionDelegate()
    {
      RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate();

      this.EventManager.TryRemoveObservable(this.EventSource1, new []{ nameof(this.EventSource1.TestEvent)}, true);

      this.SenderType = this.EventSource1.GetType();
      this.EventSource1.RaiseAll();

      this.SenderType = this.EventSource2.GetType();
      this.EventSource2.RaiseAll();

      this.NonGenericEventInvocationCount.Should().Be(1);
      this.GenericEventInvocationCount.Should().Be(2);
    }

    private void RegisterAllEventsUsingConcreteEventSourceTypeAndExplicitDelegate()
    {
      this.EventManager.TryRegisterObserver(nameof(this.EventSource1.TestEvent), this.EventSource1.GetType(), new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterObserver(nameof(this.EventSource1.GenericTestEvent), this.EventSource1.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));

      this.EventManager.TryRegisterObserver(nameof(this.EventSource2.TestEvent), this.EventSource2.GetType(), new EventHandler<EventArgs>(OnTestEvent));
      this.EventManager.TryRegisterObserver(nameof(this.EventSource2.GenericTestEvent), this.EventSource2.GetType(), new EventHandler<TestEventArgs>(OnGenericTestEvent));
    }

    private void RegisterAllEventsUsingConcreteEventSourceTypeAndActionDelegate()
    {
      this.EventManager.TryRegisterObserver<EventArgs>(
        nameof(this.EventSource1.TestEvent),
        this.EventSource1.GetType(),
        OnTestEvent);
      this.EventManager.TryRegisterObserver<TestEventArgs>(
        nameof(this.EventSource1.GenericTestEvent),
        this.EventSource1.GetType(),
        OnGenericTestEvent);

      this.EventManager.TryRegisterObserver<EventArgs>(
        nameof(this.EventSource2.TestEvent),
        this.EventSource2.GetType(),
        OnTestEvent);
      this.EventManager.TryRegisterObserver<TestEventArgs>(
        nameof(this.EventSource2.GenericTestEvent),
        this.EventSource2.GetType(),
        OnGenericTestEvent);
    }

    private void OnTestEvent(object sender, EventArgs e)
    {
      this.NonGenericEventInvocationCount++;

      sender.Should().BeOfType(this.SenderType);
    }

    private void OnGenericTestEvent(object sender, TestEventArgs e)
    {
      this.GenericEventInvocationCount++;

      sender.Should().BeOfType(this.SenderType);
    }

    delegate void TestEventHandler(object sender, EventArgs e);
    private int NonGenericEventInvocationCount { get; set; }
    private int GenericEventInvocationCount { get; set; }
    private TestEventSource EventSource1 { get; }
    private TestEventSource2 EventSource2 { get; }
    private Type SenderType { get; set; }

    private IEventAggregator EventManager { get; }
  }
}
