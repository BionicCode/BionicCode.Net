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

  public class WeakEventAggregatorTest : IDisposable
  {
    public WeakEventAggregatorTest()
    {
      this.EventSource1 = new TestEventSource1();
      this.EventSource2 = new TestEventSource2();
      this.EventAggregatorListenerService = new WeakEventAggregator();
      this.EventAggregatorPublisherService = (IWeakEventAggregatorPublisherService)this.EventAggregatorListenerService;
      eventHandlerInvocationCount = 0;
      eventHandlerInvocationThreadId = -1;
    }

    [Fact]
    public void RegisterSingleEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
    {
      const string eventName = nameof(TestEventSource1.TestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);

      this.EventSource1.RaiseAll();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
      _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterSingleGenericEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
    {
      const string eventName = nameof(TestEventSource1.GenericTestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler<TestEventArgs>>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler<TestEventArgs>>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);

      this.EventSource1.RaiseAll();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
      _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterSingleGenericActionHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
    {
      const string eventName = nameof(TestEventSource1.GenericTestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, Action<object, TestEventArgs>>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, Action<object, TestEventArgs>>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);

      this.EventSource1.RaiseAll();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
      _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterSingleCustomEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
    {
      const string eventName = nameof(TestEventSource1.CustomSignatureThreeParametersTestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, CustomSignatureMoreThanTwoParametersTestEventHandler>(eventName, eventListener1.OnGenericAllPurposeThreeParameterEventHandler);
      this.EventAggregatorListenerService.StartListening<TestEventSource1, CustomSignatureMoreThanTwoParametersTestEventHandler>(eventName, eventListener2.OnGenericAllPurposeThreeParameterEventHandler);

      this.EventSource1.RaiseAll();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
      _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
    }

    [Fact]
    public void RegisterTwoEventSourceWithSingleBaseClassListener_RaiseEachDerivedEventSourceOnce_MustInvokeListenerTwice()
    {
      const string eventName = nameof(TestEventSource1.TestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource2, eventName);
      this.EventAggregatorListenerService.StartListening<TestEventSourceBase, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);

      this.EventSource1.RaiseAll();
      this.EventSource2.RaiseAll();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(2);
    }

    [Fact]
    public void RegisterTwoEventSourceWithSingleInterfaceListener_RaiseEachImplementingEventSourceOnce_MustInvokeListenerTwice()
    {
      const string eventName = nameof(TestEventSource1.TestEvent);
      var eventListener1 = new TestEventListener();
      var eventListener2 = new TestEventListener();
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource1, eventName);
      this.EventAggregatorPublisherService.StartBroadcasting(this.EventSource2, eventName);
      this.EventAggregatorListenerService.StartListening<ITestEventSource, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);

      this.EventSource1.RaiseAll();
      this.EventSource2.RaiseAll();

      _ = eventListener1.EventHandlerInvocationCount.Should().Be(2);
    }

    [Fact]
    public void RegisteredHandler_GarbageCollectListenerWhileEventSourceIsAlive_MustSucceed()
    {
      //WeakReference<TestEventListener> weakReferenceToListener;
      //TestEventListener strongReferenceToListener;
      //InitializeGcTest(nameof(TestEventSource1.TestEvent), out weakReferenceToListener, out strongReferenceToListener);

      //GcEx.ForceFullGC();
      //this.EventSource1?.OnTestEvent();
      //GC.KeepAlive(strongReferenceToListener);

      //// Garbage collect the listener by discarding the strong reference
      //strongReferenceToListener = null;
      //GcEx.ForceFullGC();

      //// This must not raise any events as the listener is expected to be garbage collected at this point
      //this.EventSource1?.OnTestEvent();

      //_ = weakReferenceToListener.TryGetTarget(out _).Should().BeFalse();
      //_ = eventHandlerInvocationCount.Should().Be(1);
    }

    private void OnInvalidSender(Point sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();
    }

    private void OnInvalidEventArgs(object sender, Point e)
    {
      WeakEventAggregatorTest.OnEventInvoked();
    }

    private void OnGenericAllPurposeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSourceBase(TestEventSourceBase sender, string e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSourceBase>();
    }

    private void OnCustomSignatureTwoParametersTestEvent(int sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();
    }

    private void OnCustomSignatureThreeParametersTestEvent1(object sender, TestEventArgs e, int value)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1(TestEventSource1 sender, string e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedEventArgsFromTestEventSource1<TEventArgs>(object sender, TEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnGenericTestEventFromTestEventSource1(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnNonGenericTestEventFromTestEventSource1(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      //_ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSource1Static(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private static void OnGenericTestEventFromStaticTestEventSource1Static(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource1(object sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSource1Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnCustomSignatureThreeParametersTestEvent2(object sender, TestEventArgs e, int value)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnStronglyTypedEventArgsFromTestEventSource2<TEventArgs>(object sender, TEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
    }

    private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource2(TestEventSource2 sender, string e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnGenericTestEventFromTestEventSource2(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private void OnNonGenericTestEventFromTestEventSource2(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSource2Static(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
    }

    private static void OnGenericTestEventFromStaticTestEventSource2Static(object sender, EventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource2(object sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSource2Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource2>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static(object sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnTestEventFromTestEventSourceWrongSignature(object sender, EventArgs e, int value)
    {
      WeakEventAggregatorTest.OnEventInvoked();

      _ = sender.Should().BeOfType<TestEventSource1>();
      _ = e.Should().BeOfType<EventArgs>();
    }    

    private static void OnEventInvoked()
    {
      eventHandlerInvocationThreadId = Thread.CurrentThread.ManagedThreadId;
      eventHandlerInvocationCount++;
    }

    private TestEventSource1 EventSource1 { get; set; }
    private TestEventSource2 EventSource2 { get; set; }
    public IWeakEventAggregatorListenerService EventAggregatorListenerService { get; }
    public IWeakEventAggregatorPublisherService EventAggregatorPublisherService { get; }

    private static int eventHandlerInvocationCount;
    private static int eventHandlerInvocationThreadId;
    private bool disposedValue;
    private TestEnvironmentSynchronizationContext currentSynchronizationContext;

    private static bool IsDisposing { get; set; }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        WeakEventAggregatorTest.IsDisposing = true;
        if (disposing)
        {
          this.EventAggregatorListenerService.StopListeningAll<TestEventSource1>();
          this.EventAggregatorListenerService.StopListeningAll<TestEventSource2>();
          this.currentSynchronizationContext = null;
          eventHandlerInvocationCount = 0;
          eventHandlerInvocationThreadId = -1;
          GcEx.ForceFullGC();
        }

        disposedValue = true;
        WeakEventAggregatorTest.IsDisposing = false;
      }
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }

  internal class TestEnvironmentSynchronizationContext : SynchronizationContext
  {
    public int ManagedThreadId { get; }
    private bool isShutdown;
    private readonly BlockingCollection<Action> unitOfWorkItems;
    private readonly TaskCompletionSource completionSource;
    private readonly TaskCompletionSource<bool> unitOfWorkExecutedCompletionSource;
    private readonly object syncLock;
    private bool unitOfWorkExecuted;
    private bool canExecuteUnitOfWork;

    public TestEnvironmentSynchronizationContext()
    {
      this.syncLock = new object();
      this.completionSource = new TaskCompletionSource();
      this.unitOfWorkExecutedCompletionSource = new TaskCompletionSource<bool>();
      this.unitOfWorkExecutedCompletionSource.SetResult(true);
      this.unitOfWorkItems = new BlockingCollection<Action>();

      var mainThread = new Thread(OnMessageLoopStarted);
      this.ManagedThreadId = mainThread.ManagedThreadId;
      mainThread.Start();
    }

    public async Task ShutdownAsync()
    {
      this.unitOfWorkItems.CompleteAdding();
      await this.completionSource.Task;
    }

    private void OnMessageLoopStarted(object obj)
    {
      while (!this.unitOfWorkItems.IsCompleted)
      {
        lock (this.syncLock)
        {
          if (this.canExecuteUnitOfWork && this.unitOfWorkItems.TryTake(out Action unitOfWorkItem))
          {
            unitOfWorkItem.Invoke();
            this.unitOfWorkExecuted = true;
          } 
        }
      }
      
      this.isShutdown = true;
      this.unitOfWorkItems.Dispose();
      this.completionSource.SetResult();
    }

    public override SynchronizationContext CreateCopy() => base.CreateCopy();
    public override void OperationCompleted() => base.OperationCompleted();
    public override void OperationStarted() => base.OperationStarted();
    public override void Post(SendOrPostCallback d, object state)
    {
      if (this.isShutdown)
      {
        throw new InvalidOperationException("SynchronizationContext has been shutdown.");
      }

      this.unitOfWorkItems.Add(() => d.Invoke(state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
      if (this.isShutdown)
      {
        throw new InvalidOperationException("SynchronizationContext has been shutdown.");
      }

      lock (this.syncLock)
      {
        this.canExecuteUnitOfWork = false;
        this.unitOfWorkExecuted = false;
      }

      this.unitOfWorkItems.Add(() => d.Invoke(state));
      lock (this.syncLock)
      {
        this.canExecuteUnitOfWork = true; 
      }

      while (!this.unitOfWorkExecuted) 
      {
        ;
      }
    }

    public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout) => base.Wait(waitHandles, waitAll, millisecondsTimeout);
  }
}
