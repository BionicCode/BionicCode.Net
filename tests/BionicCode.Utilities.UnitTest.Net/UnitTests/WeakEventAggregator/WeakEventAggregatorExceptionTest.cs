namespace BionicCode.Utilities.Net.UnitTest.WeakEventAggregatorTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class WeakEventAggregatorExceptionTest : IDisposable
    {
        public WeakEventAggregatorExceptionTest()
        {
            EventSource1 = new TestEventSource1();
            EventSource2 = new TestEventSource2();
            EventAggregatorListenerService = new WeakEventAggregator();
            unsubscribeDelegates = new List<Action>();
            EventAggregatorPublisherService = (IWeakEventAggregatorPublisherService)EventAggregatorListenerService;
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
            EventAggregatorPublisherService.StartBroadcasting(EventSource1, eventName);
            EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
            EventAggregatorListenerService.StartListening<TestEventSource1, EventHandler>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);
            void unsubscribebDelegate()
            {
                EventAggregatorListenerService.StopListening<TestEventSource1, EventHandler>(eventName, eventListener1.OnGenericAllPurposeTwoParameterEventHandler);
                EventAggregatorListenerService.StopListening<TestEventSource1, EventHandler>(eventName, eventListener2.OnGenericAllPurposeTwoParameterEventHandler);
            }

            unsubscribeDelegates.Add(unsubscribebDelegate);

            EventSource1.RaiseAll();
            //EventSource1.OnCustomHandlerTestEvent();
            //EventSource1.OnTestEvent();
            //EventSource1.OnGenericTestEvent();
            //EventSource1.OnCustomHandlerTestEventForStaticHandlers();
            //EventSource1.OnCustomSignatureThreeParametersTestEvent();
            //EventSource1.OnCustomSignatureTwoParametersTestEvent();
            //EventSource1.OnGenericTestEventForStaticHandlers();
            //EventSource1.OnStringEventArgsTestEvent();
            //EventSource1.OnStronglyTypedCustomHandlerTestEvent();
            //EventSource1.OnStronglyTypedCustomHandlerTestEventForStaticHandlers();

            _ = eventListener1.EventHandlerInvocationCount.Should().Be(1);
            _ = eventListener2.EventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithAnyIncompatibleHandler_MustRaiseEventHandlerMismatchException()
        {
            var eventListener1 = new TestEventListener();
            Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;

            _ = Invoking(testEnvironment => EventAggregatorListenerService.StartListeningAll<ITestEventSource, Action<object, object>>(eventHandler)).Should().ThrowExactly<EventHandlerMismatchException>();
        }

        //[Fact]
        //public void RegisterEventSourceWithSingleListener_ListenToAllEventsAnonymouslyWithAnyIncompatibleHandlerUsingTryMethod_MustNotRaiseEventHandlerMismatchException()
        //{
        //  var eventListener1 = new TestEventListener();
        //  Action<object, object> eventHandler = eventListener1.OnGenericAllPurposeTwoParameterEventHandler;

        //  _ = Invoking(testEnvironment => EventAggregatorListenerService.TryStartListeningAll<ITestEventSource, Action<object, object>>(eventHandler)).Should().NotThrow();
        //}

        private void OnInvalidSender(Point sender, EventArgs e) => WeakEventAggregatorExceptionTest.OnEventInvoked();

        private void OnInvalidEventArgs(object sender, Point e) => WeakEventAggregatorExceptionTest.OnEventInvoked();

        private void OnGenericAllPurposeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) => WeakEventAggregatorExceptionTest.OnEventInvoked();

        private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSourceBase(TestEventSourceBase sender, string e)
        {
            WeakEventAggregatorExceptionTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSourceBase>();
        }

        private void OnCustomSignatureTwoParametersTestEvent(int sender, TestEventArgs e) => WeakEventAggregatorExceptionTest.OnEventInvoked();

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
                    //EventAggregatorListenerService.StopListeningAll<TestEventSource1>();
                    //EventAggregatorListenerService.StopListeningAll<TestEventSource2>();
                    unsubscribeDelegates.ForEach(unsubscribe => unsubscribe.Invoke());
                    EventAggregatorPublisherService.StopBroadcasting(EventSource1, true);
                    EventAggregatorPublisherService.StopBroadcasting(EventSource2, true);
                    EventSource1 = null;
                    EventSource2 = null;
                    EventAggregatorListenerService = null;
                    EventAggregatorPublisherService = null;
                    currentSynchronizationContext = null;
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
