namespace BionicCode.Utilities.Net.UnitTest.WeakEventAggregatorTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Microsoft.CodeAnalysis;
    using Xunit;

    public class WeakEventAggregatorTest : IDisposable
    {
        public WeakEventAggregatorTest()
        {
            EventSource1 = new TestEventSource1();
            EventSource2 = new TestEventSource2();
            EventAggregatorListenerService = new WeakEventAggregator();
            unsubscribeDelegates = new List<Action>();
            EventAggregatorPublisherService = (IWeakEventAggregatorPublisherService)EventAggregatorListenerService;
            eventHandlerInvocationCount = 0;
            eventHandlerInvocationThreadId = -1;
        }

        //[Fact]
        //public void RegisterSingleEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
        //{
        //  const string eventName = nameof(TestEventSource1.TestEvent);
        //  var eventListener1 = new TestEventListener();
        //  var eventListener2 = new TestEventListener();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);

        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
        //  _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
        //}

        //[Fact]
        //public void RegisterSingleGenericEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
        //{
        //  const string eventName = nameof(TestEventSource1.GenericTestEvent);
        //  var eventListener1 = new TestEventListener();
        //  var eventListener2 = new TestEventListener();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler<TestEventArgs>>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler<TestEventArgs>>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);

        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
        //  _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
        //}

        //[Fact]
        //public void RegisterSingleGenericActionHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
        //{
        //  const string eventName = nameof(TestEventSource1.GenericTestEvent);
        //  var eventListener1 = new TestEventListener();
        //  var eventListener2 = new TestEventListener();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, Action<object, TestEventArgs>>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, Action<object, TestEventArgs>>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);

        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
        //  _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
        //}

        //[Fact]
        //public void RegisterSingleCustomEventHandlerEventSourceWithTwoListeners_RaiseSingleEventOnce_MustInvokeTwoListeners()
        //{
        //  const string eventName = nameof(TestEventSource1.CustomSignatureThreeParametersTestEvent);
        //  var eventListener1 = new TestEventListener();
        //  var eventListener2 = new TestEventListener();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, CustomSignatureMoreThanTwoParametersTestEventHandler>(eventName, eventListener1.OnGenericAllPurposeThreeParameterEventHandler);
        //  EventAggregatorListenerService.StartListening<TestEventSource1, CustomSignatureMoreThanTwoParametersTestEventHandler>(eventName, eventListener2.OnGenericAllPurposeThreeParameterEventHandler);

        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
        //  _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
        //}

        //[Fact]
        //public void RegisterTwoEventSourceWithSingleBaseClassListener_RaiseEachDerivedEventSourceOnce_MustInvokeListenerTwice()
        //{
        //  const string eventName = nameof(TestEventSource1.TestEvent);
        //  var eventListener1 = new TestEventListener();
        //  var eventListener2 = new TestEventListener();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource2, eventName);
        //  EventAggregatorListenerService.StartListening<TestEventSourceBase, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);

        //  EventSource1.RaiseAll();
        //  EventSource2.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(2);
        //}

        //[Fact]
        //public void RegisterTwoEventSourceWithSingleInterfaceListener_RaiseEachImplementingEventSourceOnce_MustInvokeListenerTwice()
        //{
        //  const string eventName = nameof(TestEventSource1.TestEvent);
        //  var eventListener1 = new TestEventListener();
        //  var eventListener2 = new TestEventListener();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource2, eventName);
        //  EventAggregatorListenerService.StartListening<ITestEventSourceCommonEventPractice, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);

        //  EventSource1.RaiseAll();
        //  EventSource2.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(2);
        //}

        [Fact]
        public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithSomeIncompatibleHandlers_RaiseAllEventsMustInvokeCompatibleEvents()
        {
            Debug.WriteLine($"Normal test thread: {Thread.CurrentThread.ManagedThreadId}");
            var eventListener1 = new TestEventListener();
            Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;
            int numberOfCompatibleEvents = EventSource1.GetType().GetEvents().Where(eventInfo => !eventInfo.AddMethod.IsStatic).Where(eventHandler.IsAssignable).ToList().Count;
            EventAggregatorPublisherService.StartBroadcasting(EventSource1);
            _ = EventAggregatorListenerService.TryStartListeningAll<TestEventSource1, Action<object, object>>(eventHandler);
            Action unsubscribe = EventAggregatorListenerService.StopListeningAll<TestEventSource1>;
            unsubscribeDelegates.Add(unsubscribe);

            EventSource1.RaiseAll();

            _ = eventListener1.EventHandlerInvocationCount.Should().Be(numberOfCompatibleEvents);
        }

        //[Fact]
        //public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithSomeIncompatibleHandlers_TryStartListeningAllMustReturnFalse()
        //{
        //  var eventListener1 = new TestEventListener();
        //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;

        //  bool hasNoIncompatibleHandlers = EventAggregatorListenerService.TryStartListeningAll<ITestEventSource, Action<object, object>>(eventHandler);

        //  _ = hasNoIncompatibleHandlers.Should().BeFalse();
        //}

        //[Fact]
        //public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithOnlyCompatibleHandlers_TryStartListeningAllMustReturnTrue()
        //{
        //  var eventListener1 = new TestEventListener();
        //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;

        //  bool hasNoIncompatibleHandlers = EventAggregatorListenerService.TryStartListeningAll<ITestEventSourceCommonEventPractice, Action<object, object>>(eventHandler);

        //  _ = hasNoIncompatibleHandlers.Should().BeTrue();
        //}

        //[Fact]
        //public void RegisterEventSourceWithSingleListener_StopBroadcastingWithoutRemovingListeners_MustRaiseNoEvents()
        //{
        //  var eventListener1 = new TestEventListener();
        //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1);
        //  _ = EventAggregatorListenerService.TryStartListeningAll<TestEventSource1, Action<object, object>>(eventHandler);

        //  EventAggregatorPublisherService.StopBroadcasting(EventSource1, removeListeners: false);
        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(0);
        //}

        //[Fact]
        //public void RegisterEventSourceWithSingleListener_StopBroadcastingWithRemovingListenersThenStartBroadcastingAgain_MustRaiseNoEvents()
        //{
        //  var eventListener1 = new TestEventListener();
        //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1);
        //  _ = EventAggregatorListenerService.TryStartListeningAll<TestEventSource1, Action<object, object>>(eventHandler);

        //  EventAggregatorPublisherService.StopBroadcasting(EventSource1, removeListeners: true);
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1);
        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(0);
        //}

        //[Fact]
        //public void RegisterEventSourceWithSingleListener_StopBroadcastingWithoutRemovingListenersThenStartBroadcastingAgain_MustRaiseAllCompatibleEvents()
        //{
        //  var eventListener1 = new TestEventListener();
        //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;
        //  int numberOfCompatibleEvents = EventSource1.GetType().GetEvents().Where(eventHandler.IsAssignable).ToList().Count;
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1);
        //  _ = EventAggregatorListenerService.TryStartListeningAll<TestEventSource1, Action<object, object>>(eventHandler);

        //  EventAggregatorPublisherService.StopBroadcasting(EventSource1, removeListeners: false);
        //  EventSource1.RaiseAll();
        //  EventAggregatorPublisherService.StartBroadcasting(EventSource1);
        //  EventSource1.RaiseAll();

        //  _ = eventListener1.EventHandlerInvocationCount.Should().Be(numberOfCompatibleEvents);
        //}

        //[Fact]
        //public void RegisteredHandler_GarbageCollectListenerWhileEventSourceIsAlive_MustSucceed()
        //{
        //  //WeakReference<TestEventListener> weakReferenceToListener;
        //  //TestEventListener strongReferenceToListener;
        //  //InitializeGcTest(nameof(TestEventSource1.TestEvent), out weakReferenceToListener, out strongReferenceToListener);

        //  //GcEx.ForceFullGC();
        //  //EventSource1?.OnTestEvent();
        //  //GC.KeepAlive(strongReferenceToListener);

        //  //// Garbage collect the listener by discarding the strong reference
        //  //strongReferenceToListener = null;
        //  //GcEx.ForceFullGC();

        //  //// This must not raise any events as the listener is expected to be garbage collected at this point
        //  //EventSource1?.OnTestEvent();

        //  //_ = weakReferenceToListener.TryGetTarget(out _).Should().BeFalse();
        //  //_ = eventHandlerInvocationCount.Should().Be(1);
        //}

        private void OnInvalidSender(Point sender, EventArgs e) => WeakEventAggregatorTest.OnEventInvoked();

        private void OnInvalidEventArgs(object sender, Point e) => WeakEventAggregatorTest.OnEventInvoked();

        private void OnGenericAllPurposeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) => WeakEventAggregatorTest.OnEventInvoked();

        private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSourceBase(TestEventSourceBase sender, string e)
        {
            WeakEventAggregatorTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSourceBase>();
        }

        private void OnCustomSignatureTwoParametersTestEvent(int sender, TestEventArgs e) => WeakEventAggregatorTest.OnEventInvoked();

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
            WeakEventAggregatorTest.eventHandlerInvocationThreadId = Thread.CurrentThread.ManagedThreadId;
            WeakEventAggregatorTest.eventHandlerInvocationCount++;
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
                WeakEventAggregatorTest.IsDisposing = true;
                if (disposing)
                {
                    //EventAggregatorListenerService.StopListeningAll<TestEventSource1>();
                    //EventAggregatorListenerService.StopListeningAll<TestEventSource2>();
                    foreach (Action unsubscribe in unsubscribeDelegates)
                    {
                        unsubscribe.Invoke();
                    }

                    EventAggregatorPublisherService.StopBroadcasting(EventSource1, true);
                    EventAggregatorPublisherService.StopBroadcasting(EventSource2, true);
                    EventAggregatorListenerService = null;
                    EventAggregatorPublisherService = null;
                    EventSource1 = null;
                    EventSource2 = null;
                    currentSynchronizationContext = null;
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
}
