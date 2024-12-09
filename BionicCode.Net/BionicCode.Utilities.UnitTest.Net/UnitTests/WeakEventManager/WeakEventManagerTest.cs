namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Diagnostics.Tracing;
  using System.Linq;
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
      this.registrationManager = new EventHandlerRegistrationManager();
    }

    public void RegisterAllEventHandlersEventSource1()
    {
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.CustomSignatureTwoParametersTestEvent), OnCustomSignatureTwoParametersTestEvent);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.CustomSignatureThreeParametersTestEvent), OnCustomSignatureThreeParametersTestEvent1);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource1Static);
      _ = this.registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource1Static);
      _ = this.registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEventForStaticHandlers), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static);
      _ = this.registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticStronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1);
    }

    public void RegisterAllEventHandlersEventSource2()
    {
      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.TestEvent), OnNonGenericTestEventFromTestEventSource2);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.CustomSignatureTwoParametersTestEvent), OnCustomSignatureTwoParametersTestEvent);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.CustomSignatureThreeParametersTestEvent), OnCustomSignatureThreeParametersTestEvent2);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.GenericTestEvent), OnGenericTestEventFromTestEventSource2);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource2Static);
      _ = this.registrationManager.RegisterEventHandler((TestEventSource2)null, nameof(TestEventSource2.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource2);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource2Static);
      _ = this.registrationManager.RegisterEventHandler((TestEventSource2)null, nameof(TestEventSource2.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2);
      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEventForStaticHandlers), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static);
      _ = this.registrationManager.RegisterEventHandler((TestEventSource2)null, nameof(TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static);

      _ = this.registrationManager.RegisterEventHandler(this.EventSource2, nameof(TestEventSource2.StringEventArgsTestEvent), OnStronglyTypedSenderAndStringEventArgsFromTestEventSource2);
    }

    [Fact]
    public void UnregisterEventHandler_AllEventTypes_MustNotRaiseAnyEvent()
    {
      RegisterAllEventHandlersEventSource1();
      RegisterAllEventHandlersEventSource2();
      this.registrationManager.UnregisterAllEventHandlers();

      this.EventSource1.RaiseAll();
      this.EventSource2.RaiseAll();

      _ = eventHandlerInvocationCount.Should().Be(0);
    }

    //[Fact]
    //public void UnregisterEventHandler_AllEventTypes_MustLeaveEmptyManagedWeakTable()
    //{
    //  RegisterAllEventHandlersEventSource1();
    //  RegisterAllEventHandlersEventSource2();
    //  this.registrationManager.UnregisterAllEventHandlers();
      
    //  this.EventSource1.RaiseAll();
    //  this.EventSource2.RaiseAll();

    //  _ = ManagedWeakTable.Count.Should().Be(0);
    //}

    [Fact]
    public async Task RegisterEvent_EventDelegateWithEventArgsTypeNotDeriveFromEventArgsClass_ShouldInvokeClientHandlerOnce()
    {
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1);

      this.EventSource1.OnStringEventArgsTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleEvent_EventHandlerGeneric_ShouldInvokeHandlerOnce()
    {
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
      this.EventSource1.OnGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleEvent_EventHandlerNonGeneric_ShouldInvokeHandlerOnce()
    {
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

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
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnNonGenericTestEventFromTestEventSource1);

      this.EventSource1.OnGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_WithLessDerivedSenderThanEventDelegate_MustInvokeHandlerOnce()
    {
      _ = this.registrationManager.RegisterEventHandler(this.EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnNonGenericTestEventFromTestEventSource1);

      this.EventSource1.OnStronglyTypedCustomHandlerTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventHandler_StaticHandlerWithLessDerivedEventArgsThanStaticEventDelegate_MustInvokeHandlerOnce()
    {
      _ = this.registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSource1Static);

      TestEventSource1.OnStaticGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }
    private void OnInvalidSender(Point sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
    }

    private void OnInvalidEventArgs(object sender, Point e)
    {
      eventHandlerInvocationCount++;
    }

    private void OnGenericAllPurposeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e)
    {
      eventHandlerInvocationCount++;
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSourceBase(TestEventSourceBase sender, string e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSourceBase>();
    }

    private void OnCustomSignatureTwoParametersTestEvent(int sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
    }

    private void OnCustomSignatureThreeParametersTestEvent1(object sender, TestEventArgs e, int value)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1(TestEventSource1 sender, string e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
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

    private void OnCustomSignatureThreeParametersTestEvent2(object sender, TestEventArgs e, int value)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnStronglyTypedEventArgsFromTestEventSource2<TEventArgs>(object sender, TEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource2(TestEventSource2 sender, string e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource2>();
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

    private readonly EventHandlerRegistrationManager registrationManager;
    private TestEventSource1 EventSource1 { get; }
    private TestEventSource2 EventSource2 { get; }
    private static int eventHandlerInvocationCount;
    private bool disposedValue;
    private static bool IsDisposing { get; set; }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        WeakEventManagerTest.IsDisposing = true;
        if (disposing)
        {
          this.registrationManager.UnregisterAllEventHandlers();
          eventHandlerInvocationCount = 0;
          ForceGC();
        }

        disposedValue = true;
        WeakEventManagerTest.IsDisposing = false;
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
