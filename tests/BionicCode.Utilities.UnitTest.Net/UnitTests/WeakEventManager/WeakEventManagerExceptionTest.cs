namespace BionicCode.Utilities.Net.UnitTest.WeakEventManagerTests
{
    using System;
    using System.Threading.Tasks;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.UnitTest.Resources;
    using FluentAssertions;
    using Xunit;

    public class WeakEventManagerExceptionTest : IDisposable
    {
        public WeakEventManagerExceptionTest()
        {
            registrationManager = new EventHandlerRegistrationManager();
            EventSource1 = new TestEventSource1();
            EventSource2 = new TestEventSource2();
            eventHandlerInvocationCount = 0;
        }

        #region Event validation

        [Fact]
        public async Task RegisterEvent_SpecifiyUndefinedEvent_ShouldThrowException() => _ = Invoking(testEnvironment => registrationManager.RegisterEventHandler(EventSource1, "Undefined Event", OnNonGenericTestEventFromTestEventSource1)).Should().Throw<ArgumentException>();

        [Fact]
        public async Task RegisterEvent_EventHandlerWithInvalidSignatureWrongParameterCount_ShouldThrowException() => _ = Invoking(testEnvironment => registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomSignatureThreeParametersTestEvent), OnNonGenericTestEventFromTestEventSource1)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("parameter count");

        [Fact]
        public async Task RegisterEvent_EventHandlerWithCustomSignatureTooManyParameters_MustNotThrowException() => _ = Invoking(testEnvironment => registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomSignatureThreeParametersTestEvent), OnTestEventFromTestEventSourceThreeParameterSignature)).Should().NotThrow();

        [Fact]
        public async Task RegisterEvent_EventHandlerWithInvalidSignatureWrongSenderType_ShouldThrowException() => _ = Invoking(testEnvironment => registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.CustomSignatureTwoParametersTestEvent), OnInvalidSender)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter of type").And.Contain("at parameter index '0'");

        [Fact]
        public async Task RegisterEvent_EventHandlerWithInvalidSignatureWrongEventArgsType_ShouldThrowException() => _ = Invoking(testEnvironment => registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter of type").And.Contain("at parameter index '1'");

        [Fact]
        public async Task RegisterEvent_EventHandlerWithWrongTEventArgs_ShouldThrowEventDelegateMismatchException() => _ = Invoking(testEnvironment => registrationManager.RegisterEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnInvalidEventArgs)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter of type").And.Contain("at parameter index '1'");

        #endregion Event validation

        #region Event handler validation

        //[Fact]
        //public async Task RegisterEvent_EventHandlerWithInvalidSignatureTooManyParameters_ShouldThrowException()
        //{
        //  _ = Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.RegisterEventHandler(EventSource1, nameof(TestEventSource1.GenericTestEvent), OnTestEventFromTestEventSourceWrongSignature)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Invalid parameter count");
        //}

        //[Fact]
        //public void RegisterEventHandler_WithMoreDerivedSenderThanEventDelegate_ShouldThrowException()
        //{
        //  _ = Invoking(testEnvironment => WeakEventManager<TestEventSource1, EventArgs>.RegisterEventHandler(EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter");
        //}

        //[Fact]
        //public async Task RegisterEvent_EventHandlerWithInvalidSignatureWrongEventArgsType_ShouldThrowException()
        //{
        //  _ = Invoking(testEnvironment => WeakEventManager<TestEventSource1, EventArgs>.RegisterEventHandler(EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter");
        //}

        //[Fact]
        //public async Task RegisterEvent_EventHandlerNonGenericWithWrongEventArgsType_ShouldThrowHandlerDelegateMismatchException()
        //{
        //  _ = Invoking(testEnvironment => WeakEventManager<TestEventSource1, EventArgs>.RegisterEventHandler(EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1)).Should().Throw<EventHandlerMismatchException>();
        //}

        #endregion Event handler validation

        private void OnInvalidSender(Point sender, EventArgs e) => eventHandlerInvocationCount++;

        private void OnInvalidEventArgs(object sender, Point e) => eventHandlerInvocationCount++;

        private void OnGenericTestEventFromTestEventSource1(object sender, EventArgs e)
        {
            eventHandlerInvocationCount++;
            _ = sender.Should().BeOfType<TestEventSource1>();
        }

        private void OnNonGenericTestEventFromTestEventSource1(object sender, EventArgs e)
        {
            eventHandlerInvocationCount++;
            _ = sender.Should().BeOfType<TestEventSource1>();
            _ = e.Should().BeSameAs(EventArgs.Empty);
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

        private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource1Static(TestEventSource1 sender, TestEventArgs e)
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

        private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static(TestEventSource2 sender, TestEventArgs e)
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

        private void OnTestEventFromTestEventSourceThreeParameterSignature(object sender, TestEventArgs e, int value)
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

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    registrationManager.UnregisterAllEventHandlers();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WeakEventManagerTest()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
