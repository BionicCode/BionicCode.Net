namespace BionicCode.Utilities.Net.UnitTest.WeakEventAggregatorTests
{
  using System;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Diagnostics.Tracing;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Channels;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using FluentAssertions.Specialized;
  using Microsoft.CodeAnalysis;
  using Microsoft.CodeAnalysis.CSharp.Syntax;
  using Xunit;

  public class WeakEventAggregatorExceptionTest : IDisposable
  {
    public WeakEventAggregatorExceptionTest()
    {
      this.EventSource1 = new TestEventSource1();
      this.EventSource2 = new TestEventSource2();
      this.EventAggregatorListenerService = new WeakEventAggregator();
      this.unsubscribeDelegates = new List<Action>();
      this.EventAggregatorPublisherService = (IWeakEventAggregatorPublisherService)this.EventAggregatorListenerService;
      eventHandlerInvocationCount = 0;
      eventHandlerInvocationThreadId = -1;
    }

    [Fact]
    public void RegisterSingleEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
    {
      Debug.WriteLine($"Exception test thread: {Thread.CurrentThread.ManagedThreadId}");
      const string eventName = nameof(TestEventSource1.TestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);
      Action unsubscribebDelegate = () =>
      {
        this.EventAggregatorListenerService.StopListening<TestEventSource1, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
        this.EventAggregatorListenerService.StopListening<TestEventSource1, EventHandler>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);
      };
      this.unsubscribeDelegates.Add(unsubscribebDelegate);

      this.EventSource1.RaiseAll();
      //this.EventSource1.OnCustomHandlerTestEvent();
      //this.EventSource1.OnTestEvent();
      //this.EventSource1.OnGenericTestEvent();
      //this.EventSource1.OnCustomHandlerTestEventForStaticHandlers();
      //this.EventSource1.OnCustomSignatureThreeParametersTestEvent();
      //this.EventSource1.OnCustomSignatureTwoParametersTestEvent();
      //this.EventSource1.OnGenericTestEventForStaticHandlers();
      //this.EventSource1.OnStringEventArgsTestEvent();
      //this.EventSource1.OnStronglyTypedCustomHandlerTestEvent();
      //this.EventSource1.OnStronglyTypedCustomHandlerTestEventForStaticHandlers();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
      _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithAnyIncompatibleHandler_MustRaiseEventHandlerMismatchException()
    {
      var eventListener1 = new TestEventListener();
      Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;

      _ = this.Invoking(testEnvironment => this.EventAggregatorListenerService.StartListeningAll<ITestEventSource, Action<object, object>>(eventHandler)).Should().ThrowExactly<EventHandlerMismatchException>();
    }

    //[Fact]
    //public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithAnyIncompatibleHandlerUsingTryMethod_MustNotRaiseEventHandlerMismatchException()
    //{
    //  var eventListener1 = new TestEventListener();
    //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;

    //  _ = this.Invoking(testEnvironment => this.EventAggregatorListenerService.TryStartListeningAll<ITestEventSource, Action<object, object>>(eventHandler)).Should().NotThrow();
    //}

    private void OnInvalidSender(Point sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();
    }

    private void OnInvalidEventArgs(object sender, Point e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();
    }

    private void OnGenericAllPurposeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSourceBase(TestEventSourceBase sender, string e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSourceBase>();
    }

    private void OnCustomSignatureTwoParametersTestEvent(int sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();
    }

    private void OnCustomSignatureThreeParametersTestEvent1(object sender, TestEventArgs e, int value)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1(TestEventSource1 sender, string e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedEventArgsFromTestEventSource1<TEventArgs>(object sender, TEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnGenericTestEventFromTestEventSource1(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnNonGenericTestEventFromTestEventSource1(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      //_ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSource1Static(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private static void OnGenericTestEventFromStaticTestEventSource1Static(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource1(object sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSource1Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnCustomSignatureThreeParametersTestEvent2(object sender, TestEventArgs e, int value)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnStronglyTypedEventArgsFromTestEventSource2<TEventArgs>(object sender, TEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource2(TestEventSource2 sender, string e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnGenericTestEventFromTestEventSource2(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnNonGenericTestEventFromTestEventSource2(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSource2Static(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private static void OnGenericTestEventFromStaticTestEventSource2Static(object sender, EventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource2(object sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSource2Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnTestEventFromTestEventSourceWrongSignature(object sender, EventArgs e, int value)
    {
      WeakEventAggregatorExceptionTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<EventArgs>();
    }    

    private static void OnEventInvoked()
    {
      WeakEventAggregatorExceptionTest.eventHandlerInvocationThreadId = Thread.CurrentThread.ManagedThreadId;
      WeakEventAggregatorExceptionTest.eventHandlerInvocationCount++;
    }

    private TestEventSource1 EventSource1 { get; set; }
    private TestEventSource2 EventSource2 { get; set; }
    public IWeakEventAggregatorListenerService EventAggregatorListenerService { get; set; }
    public IWeakEventAggregatorPublisherService EventAggregatorPublisherService { get; set; }

    private static int eventHandlerInvocationCount;
    private static int eventHandlerInvocationThreadId;
    private bool disposedValue;
    private TestEnvironmentSynchronizationContext currentSynchronizationContext;
    private readonly List<Action> unsubscribeDelegates;

    private static bool IsDisposing { get; set; }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        WeakEventAggregatorExceptionTest.IsDisposing = true;
        if (disposing)
        {
          //this.EventAggregatorListenerService.StopListeningAll<TestEventSource1>();
          //this.EventAggregatorListenerService.StopListeningAll<TestEventSource2>();
          this.unsubscribeDelegates.ForEach(unsubscribe => unsubscribe.Invoke());
          this.EventAggregatorPublisherService.StopBroadcasting(this.EventSource1, true);
          this.EventAggregatorPublisherService.StopBroadcasting(this.EventSource2, true);
          this.EventSource1 = null;
          this.EventSource2 = null;
          this.EventAggregatorListenerService = null;
          this.EventAggregatorPublisherService = null;
          this.currentSynchronizationContext = null;
          eventHandlerInvocationCount = 0;
          eventHandlerInvocationThreadId = -1;
          GcEx.ForceFullGC();
        }

        disposedValue = true;
        WeakEventAggregatorExceptionTest.IsDisposing = false;
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
