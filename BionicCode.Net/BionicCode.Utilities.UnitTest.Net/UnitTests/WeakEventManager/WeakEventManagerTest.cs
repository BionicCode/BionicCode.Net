namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
  using System;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using FluentAssertions.Specialized;
  using Xunit;

  public class WeakEventManagerTest : IDisposable
  {
    public WeakEventManagerTest()
    {
      ForceGC();
      this.EventSource1 = new TestEventSource1();
      this.EventSource2 = new TestEventSource2();
      eventHandlerInvocationCount = 0;
    }

    public void RegisterAllEventHandlersEventSource1()
    {
      WeakEventManager<TestEventSource1, EventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

      WeakEventManager<TestEventSource1, string>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedEventArgsFromTestEventSource1);

      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource1Static);
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource1.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1);
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource1Static);
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource1.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1));
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEventForStaticHandlers), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static));
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource1.StaticStronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static));
    }

    public void RegisterAllEventHandlersEventSource2()
    {
      WeakEventManager<TestEventSource2, EventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.TestEvent), OnNonGenericTestEventFromTestEventSource2);

      WeakEventManager<TestEventSource2, string>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.StringEventArgsTestEvent), OnStronglyTypedEventArgsFromTestEventSource2);

      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.GenericTestEvent), OnGenericTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource2.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource2.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2));
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEventForStaticHandlers), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static));
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static));
    }

    public void UnregisterAllEventHandlersEventSource1()
    {
      WeakEventManager<TestEventSource1, EventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

      WeakEventManager<TestEventSource1, string>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedEventArgsFromTestEventSource1);

      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource1Static);
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource1.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSource1Static);
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource1.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1);
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource1Static);
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource1.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1));
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEventForStaticHandlers), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static));
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource1.StaticStronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static));

      
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnNonGenericTestEventFromTestEventSource1);
      WeakEventManager<TestEventSource1, TestEventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnNonGenericTestEventFromTestEventSource1);
      WeakEventManager<TestEventSource1, EventArgs>.RemoveEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), new StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1));
    }

    public void UnregisterAllEventHandlersEventSource2()
    {
      WeakEventManager<TestEventSource2, EventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.TestEvent), OnNonGenericTestEventFromTestEventSource2);

      WeakEventManager<TestEventSource2, string>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.StringEventArgsTestEvent), OnStronglyTypedEventArgsFromTestEventSource2);

      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.GenericTestEvent), OnGenericTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource2.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource2.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource2.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2));
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEventForStaticHandlers), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static));
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(null, nameof(TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static));

      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource1.GenericTestEvent), OnNonGenericTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnNonGenericTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, EventArgs>.RemoveEventHandler(this.EventSource2, nameof(TestEventSource1.TestEvent), new StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs>(OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2));
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypes_MustNotThrow()
    {
      _ = this.Invoking(test => test.RegisterAllEventHandlersEventSource1()).Should().NotThrow("because all delegates and target methods match.");
    }

    [Fact]
    public void UnregisterEventHandler_AllEventTypes_MustNotRaiseAnyEvent()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();
      UnregisterAllEventHandlersEventSource1();
      UnregisterAllEventHandlersEventSource2();

      this.EventSource1.RaiseAll();
      this.EventSource2.RaiseAll();

      _ = eventHandlerInvocationCount.Should().Be(0);
    }

    [Fact]
    public void UnregisterEventHandler_AllEventTypes_MustLeaveEmptyManagedWeakTable()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();
      UnregisterAllEventHandlersEventSource1();
      UnregisterAllEventHandlersEventSource2();

      this.EventSource1.RaiseAll();
      this.EventSource2.RaiseAll();

      _ = ManagedWeakTable.Count.Should().Be(0);
    }

    [Fact]
    public async Task RegisterEvent_EventDelegateWithEventArgsTypeNotDeriveFromEventArgsClass_ShouldInvokeClientHandlerOnce()
    {
      WeakEventManager<TestEventSource1, string>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedEventArgsFromTestEventSource1);

      this.EventSource1.OnStringEventArgsTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleEvent_EventHandlerGeneric_ShouldInvokeHandlerOnce()
    {
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);

      this.EventSource1.OnGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleEvent_EventHandlerNonGeneric_ShouldInvokeHandlerOnce()
    {
      WeakEventManager<TestEventSource1, EventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

      this.EventSource1.OnTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStaticGenericEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      TestEventSource1.OnStaticGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStaticCustomHandlerTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      TestEventSource1.OnStaticCustomHandlerTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStaticGenericTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      TestEventSource1.OnStaticGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStaticStronglyTypedCustomHandlerTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      TestEventSource1.OnStaticStronglyTypedCustomHandlerTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      this.EventSource1.OnTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseGenericTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      this.EventSource1.OnGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseCustomHandlerTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      this.EventSource1.OnCustomHandlerTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStringEventArgsTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      this.EventSource1.OnStringEventArgsTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStronglyTypedCustomHandlerTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      this.EventSource1.OnStronglyTypedCustomHandlerTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_AllEventTypesAndRaiseStronglyTypedCustomStaticHandlerTestEvent_MustInvokeHandlerOnce()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();

      this.EventSource1.OnStronglyTypedCustomHandlerTestEventForStaticHandlers();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_WithLessDerivedEventArgsThanEventDelegate_MustInvokeHandlerOnce()
    {
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnNonGenericTestEventFromTestEventSource1);

      this.EventSource1.OnGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_WithLessDerivedSenderThanEventDelegate_MustInvokeHandlerOnce()
    {
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnNonGenericTestEventFromTestEventSource1);

      this.EventSource1.OnStronglyTypedCustomHandlerTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_StaticHandlerWithLessDerivedEventArgsThanStaticEventDelegate_MustInvokeHandlerOnce()
    {
      WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(null, nameof(TestEventSource1.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSource1Static);

      TestEventSource1.OnStaticGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    private void OnStronglyTypedEventArgsFromTestEventSource1<TEventArgs>(object sender, TEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnGenericTestEventFromTestEventSource1(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnNonGenericTestEventFromTestEventSource1(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
      //_ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSource1Static(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private static void OnGenericTestEventFromStaticTestEventSource1Static(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource1(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSource1Static(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsFromTestEventSource2<TEventArgs>(object sender, TEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnGenericTestEventFromTestEventSource2(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnNonGenericTestEventFromTestEventSource2(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSource2Static(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private static void OnGenericTestEventFromStaticTestEventSource2Static(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource2(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSource2Static(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnTestEventFromTestEventSourceWrongSignature(object sender, EventArgs e, int value)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<EventArgs>();
    }

    private void ForceGC()
    {
      for (int i = 0; i < 10; i++)
      {
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
      }
    }

    private TestEventSource1 EventSource1 { get; }
    private TestEventSource2 EventSource2 { get; }
    private static int eventHandlerInvocationCount;
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          UnregisterAllEventHandlersEventSource1();
          UnregisterAllEventHandlersEventSource2();
          eventHandlerInvocationCount = 0;
          ForceGC();
        }

        disposedValue = true;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}
