namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class WeakEventManagerTest : IDisposable
    {
        public WeakEventManagerTest()
        {
            EventSource1 = new TestEventSource1();
            EventSource2 = new TestEventSource2();
            eventHandlerInvocationCount = 0;
            eventHandlerInvocationThreadId = -1;
            registrationManager = new EventHandlerRegistrationManager();
        }

        [Fact]
        public void RegisterAllEventHandlersEventSource1()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomSignatureTwoParametersTestEvent), OnCustomSignatureTwoParametersTestEvent);
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomSignatureThreeParametersTestEvent), OnCustomSignatureThreeParametersTestEvent1);

            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource1Static);
            _ = registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1);
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource1Static);
            _ = registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static);

            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1);
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEventForStaticHandlers), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static);
            _ = registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticStronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static);

            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1);
        }

        [Fact]
        public void RegisterAllEventHandlersEventSource2()
        {
            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.TestEvent), OnNonGenericTestEventFromTestEventSource2);

            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.CustomSignatureTwoParametersTestEvent), OnCustomSignatureTwoParametersTestEvent);
            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.CustomSignatureThreeParametersTestEvent), OnCustomSignatureThreeParametersTestEvent2);

            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.GenericTestEvent), OnGenericTestEventFromTestEventSource2);
            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.GenericTestEventForStaticHandlers), OnGenericTestEventFromTestEventSource2Static);
            _ = registrationManager.RegisterEventHandler((TestEventSource2)null, nameof(TestEventSource2.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource2);
            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.CustomHandlerTestEventForStaticHandlers), OnStronglyTypedEventArgsTestEventFromTestEventSource2Static);
            _ = registrationManager.RegisterEventHandler((TestEventSource2)null, nameof(TestEventSource2.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2);
            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.StronglyTypedCustomHandlerTestEventForStaticHandlers), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static);
            _ = registrationManager.RegisterEventHandler((TestEventSource2)null, nameof(TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static);

            _ = registrationManager.RegisterEventHandler(EventSource2, nameof(TestEventSource2.StringEventArgsTestEvent), OnStronglyTypedSenderAndStringEventArgsFromTestEventSource2);
        }

        [Fact]
        public void UnregisterEventHandler_AllEventTypes_MustNotRaiseAnyEvent()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();
            registrationManager.UnregisterAllEventHandlers();

            EventSource1.RaiseAll();
            EventSource2.RaiseAll();

            _ = eventHandlerInvocationCount.Should().Be(0);
        }

        [Fact]
        public void RegisteredHandler_GarbageCollectListenerWhileEventSourceIsAlive_MustSucceed()
        {
            InitializeGcTest(nameof(TestEventSource1.TestEvent), out WeakReference<TestEventListener> weakReferenceToListener, out TestEventListener strongReferenceToListener);

            GcEx.ForceFullGC();
            EventSource1?.OnTestEvent();
            GC.KeepAlive(strongReferenceToListener);

            // Garbage collect the listener by discarding the strong reference
            GcEx.ForceFullGC();

            // This must not raise any events as the listener is expected to be garbage collected at this point
            EventSource1?.OnTestEvent();

            _ = weakReferenceToListener.TryGetTarget(out _).Should().BeFalse();
            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public async Task InvokeEventOnBackgroundThread_PassingSynchronizationContext_MustInvokeEventHandlerOnOriginalThread()
        {
            currentSynchronizationContext = new TestEnvironmentSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(currentSynchronizationContext);
            _ = registrationManager.RegisterEventHandlerWithSynchronizationContext(EventSource1, nameof(EventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1, currentSynchronizationContext);
            int currentThreadId = currentSynchronizationContext.ManagedThreadId;

            await Task.Run(EventSource1.OnGenericTestEvent);
            await currentSynchronizationContext?.ShutdownAsync();

            _ = eventHandlerInvocationThreadId.Should().Be(currentThreadId);
        }

        [Fact]
        public async Task InvokeEventOnBackgroundThread_CapturingSynchronizationContext_MustInvokeEventHandlerOnOriginalThread()
        {
            currentSynchronizationContext = new TestEnvironmentSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(currentSynchronizationContext);
            currentSynchronizationContext.Send(state => _ = registrationManager.RegisterEventHandlerWithCurrentSynchronizationContext(EventSource1, nameof(EventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1), null);
            int currentThreadId = currentSynchronizationContext.ManagedThreadId;

            await Task.Factory.StartNew(EventSource1.OnGenericTestEvent, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
            await currentSynchronizationContext?.ShutdownAsync();

            _ = eventHandlerInvocationThreadId.Should().Be(currentThreadId);
        }

        [Fact]
        public async Task InvokeEventOnBackgroundThread_NotPassingSynchronizationContext_MustInvokeEventHandlerOnEventInvocatorThread()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(EventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);

            int invocatorThreadId = await Task.Run(() =>
            {
                EventSource1.OnGenericTestEvent();

                return invocatorThreadId = Thread.CurrentThread.ManagedThreadId;
            });

            _ = eventHandlerInvocationThreadId.Should().Be(invocatorThreadId);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InitializeGcTest(string eventName, out WeakReference<TestEventListener> weakReferenceToListener, out TestEventListener strongReferenceToListener)
        {
            Action invocationCounterInvocator = WeakEventManagerTest.OnEventInvoked;
            strongReferenceToListener = new TestEventListener();
            weakReferenceToListener = new WeakReference<TestEventListener>(strongReferenceToListener);
            strongReferenceToListener.InitializeWeakEventTest(invocationCounterInvocator, registrationManager, EventSource1, eventName);
        }

        [Fact]
        public async Task RegisterEvent_EventDelegateWithEventArgsTypeNotDeriveFromEventArgsClass_ShouldInvokeClientHandlerOnce()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.StringEventArgsTestEvent), OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1);

            EventSource1.OnStringEventArgsTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public async Task HandleEvent_EventHandlerGeneric_ShouldInvokeHandlerOnce()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
            WeakEventManager<TestEventSource1>.AddEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);
            WeakEventManager<TestEventSource1>.RemoveEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource1);

            EventSource1.OnGenericTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public async Task HandleEvent_EventHandlerNonGeneric_ShouldInvokeHandlerOnce()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1);

            EventSource1.OnTestEvent();

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

            EventSource1.OnTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_AllEventTypesAndRaiseGenericTestEvent_MustInvokeHandlerOnce()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();

            EventSource1.OnGenericTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_AllEventTypesAndRaiseCustomHandlerTestEvent_MustInvokeHandlerOnce()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();

            EventSource1.OnCustomHandlerTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_AllEventTypesAndRaiseCustomThreeParameterHandlerTestEvent_MustInvokeHandlerOnce()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();

            EventSource1.OnCustomSignatureThreeParametersTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_AllEventTypesAndRaiseStringEventArgsTestEvent_MustInvokeHandlerOnce()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();

            EventSource1.OnStringEventArgsTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_AllEventTypesAndRaiseStronglyTypedCustomHandlerTestEvent_MustInvokeHandlerOnce()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();

            EventSource1.OnStronglyTypedCustomHandlerTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_AllEventTypesAndRaiseStronglyTypedCustomStaticHandlerTestEvent_MustInvokeHandlerOnce()
        {
            RegisterAllEventHandlersEventSource1();
            RegisterAllEventHandlersEventSource2();

            EventSource1.OnStronglyTypedCustomHandlerTestEventForStaticHandlers();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_WithLessDerivedEventArgsThanEventDelegate_MustInvokeHandlerOnce()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnNonGenericTestEventFromTestEventSource1);

            EventSource1.OnGenericTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_WithLessDerivedSenderThanEventDelegate_MustInvokeHandlerOnce()
        {
            _ = registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.StronglyTypedCustomHandlerTestEvent), OnNonGenericTestEventFromTestEventSource1);

            EventSource1.OnStronglyTypedCustomHandlerTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }

        [Fact]
        public void RegisterEventHandler_StaticHandlerWithLessDerivedEventArgsThanStaticEventDelegate_MustInvokeHandlerOnce()
        {
            _ = registrationManager.RegisterEventHandler((TestEventSource1)null, nameof(TestEventSource1.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSource1Static);

            TestEventSource1.OnStaticGenericTestEvent();

            _ = eventHandlerInvocationCount.Should().Be(1);
        }
        private void OnInvalidSender(Point sender, EventArgs e) => WeakEventManagerTest.OnEventInvoked();

        private void OnInvalidEventArgs(object sender, Point e) => WeakEventManagerTest.OnEventInvoked();

        private void OnGenericAllPurposeEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e) => WeakEventManagerTest.OnEventInvoked();

        private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSourceBase(TestEventSourceBase sender, string e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSourceBase>();
        }

        private void OnCustomSignatureTwoParametersTestEvent(int sender, TestEventArgs e) => WeakEventManagerTest.OnEventInvoked();

        private void OnCustomSignatureThreeParametersTestEvent1(object sender, TestEventArgs e, int value)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource1(TestEventSource1 sender, string e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private void OnStronglyTypedEventArgsFromTestEventSource1<TEventArgs>(object sender, TEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private void OnGenericTestEventFromTestEventSource1(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private void OnNonGenericTestEventFromTestEventSource1(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
            //_ = e.Should().BeSameAs(EventArgs.Empty);
        }

        private static void OnGenericTestEventFromTestEventSource1Static(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private static void OnGenericTestEventFromStaticTestEventSource1Static(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
        }

        private void OnStronglyTypedEventArgsTestEventFromTestEventSource1(object sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedEventArgsTestEventFromTestEventSource1Static(object sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1(TestEventSource1 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static(object sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1(TestEventSource1 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnCustomSignatureThreeParametersTestEvent2(object sender, TestEventArgs e, int value)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
        }

        private void OnStronglyTypedEventArgsFromTestEventSource2<TEventArgs>(object sender, TEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private void OnStronglyTypedSenderAndStringEventArgsFromTestEventSource2(TestEventSource2 sender, string e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
        }

        private void OnGenericTestEventFromTestEventSource2(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
        }

        private void OnNonGenericTestEventFromTestEventSource2(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
            _ = e.Should().BeSameAs(EventArgs.Empty);
        }

        private static void OnGenericTestEventFromTestEventSource2Static(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
        }

        private static void OnGenericTestEventFromStaticTestEventSource2Static(object sender, EventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
        }

        private void OnStronglyTypedEventArgsTestEventFromTestEventSource2(object sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedEventArgsTestEventFromTestEventSource2Static(object sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2(TestEventSource2 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource2>();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static(object sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2(TestEventSource2 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeNull();
            _ = e.Should().BeOfType<TestEventArgs>();
        }

        private void OnTestEventFromTestEventSourceWrongSignature(object sender, EventArgs e, int value)
        {
            WeakEventManagerTest.OnEventInvoked();

            _ = sender.Should().BeOfType<TestEventSource1>();
            _ = e.Should().BeOfType<EventArgs>();
        }

        private static void OnEventInvoked()
        {
            eventHandlerInvocationThreadId = Thread.CurrentThread.ManagedThreadId;
            eventHandlerInvocationCount++;
        }

        private readonly EventHandlerRegistrationManager registrationManager;
        private TestEventSource1 EventSource1 { get; set; }
        private TestEventSource2 EventSource2 { get; set; }
        private static int eventHandlerInvocationCount;
        private static int eventHandlerInvocationThreadId;
        private bool disposedValue;
        private TestEnvironmentSynchronizationContext currentSynchronizationContext;

        private static bool IsDisposing { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                WeakEventManagerTest.IsDisposing = true;
                if (disposing)
                {
                    currentSynchronizationContext = null;
                    registrationManager.UnregisterAllEventHandlers();
                    eventHandlerInvocationCount = 0;
                    eventHandlerInvocationThreadId = -1;
                    GcEx.ForceFullGC();
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
