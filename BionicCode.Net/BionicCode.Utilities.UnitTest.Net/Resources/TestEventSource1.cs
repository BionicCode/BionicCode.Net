namespace BionicCode.Utilities.Net.UnitTest.Resources
{
    using System;

    public delegate void TestEventHandler(object sender, TestEventArgs e);
    public delegate void StronglyTypedTestEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);
    public delegate void CustomSignatureMoreThanTwoParametersTestEventHandler(object sender, TestEventArgs e, int value);
    public delegate void CustomSignatureTwoParametersTestEventHandler(int sender, TestEventArgs e);

    public class TestEventArgs : EventArgs
    {

    }

    public class TestEventListener
    {
        public int EventHandlerInvocationCount { get; private set; }

        private Action eventAction;

        public void Initialize(Action eventAction, WeakEventManagerTests.EventHandlerRegistrationManager registrationManager, TestEventSource1 eventSource)
        {
            _ = registrationManager?.RegisterEventHandler<Action<object, EventArgs>>(eventSource, nameof(TestEventSource1.TestEvent), OnGenericAllPurposeTwoParameterEventHandler);
            this.eventAction = eventAction;
        }

        public void InitializeWeakEventTest(Action eventAction, WeakEventManagerTests.EventHandlerRegistrationManager registrationManager, TestEventSource1 eventSource, string eventName)
        {
            registrationManager?.RegisterEventHandlerWithoutEventSource<Action<object, EventArgs>>(eventSource, eventName, OnGenericAllPurposeTwoParameterEventHandler);
            this.eventAction = eventAction;
        }

        public void OnGenericAllPurposeTwoParameterEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e)
        {
            ++this.EventHandlerInvocationCount;
            this.eventAction?.Invoke();
        }

        public void OnGenericAllPurposeThreeParameterEventHandler<TValue1, TValue2, TValue3>(TValue1 value1, TValue2 value2, TValue3 value3)
        {
            ++this.EventHandlerInvocationCount;
            this.eventAction?.Invoke();
        }
    }

    public interface ITestEventSource<TEventSource> : ITestEventSource
    {
        event StronglyTypedTestEventHandler<TEventSource, TestEventArgs> StronglyTypedCustomHandlerTestEvent;
        event StronglyTypedTestEventHandler<TEventSource, TestEventArgs> StronglyTypedCustomHandlerTestEventForStaticHandlers;

        void OnStronglyTypedCustomHandlerTestEvent();
        void OnStronglyTypedCustomHandlerTestEventForStaticHandlers();
    }

    public interface ITestEventSourceCommonEventPractice
    {
        event EventHandler TestEvent;
        event TestEventHandler CustomHandlerTestEvent;
        event TestEventHandler CustomHandlerTestEventForStaticHandlers;
        event EventHandler<TestEventArgs> GenericTestEvent;
        event EventHandler<TestEventArgs> GenericTestEventForStaticHandlers;

        void OnTestEvent();
        void OnGenericTestEvent();
        void OnGenericTestEventForStaticHandlers();
        void OnCustomHandlerTestEvent();
        void OnCustomHandlerTestEventForStaticHandlers();
        void OnStringEventArgsTestEvent();
        void RaiseAll();
    }

    public interface ITestEventSource : ITestEventSourceCommonEventPractice
    {
        event CustomSignatureMoreThanTwoParametersTestEventHandler CustomSignatureThreeParametersTestEvent;
        event CustomSignatureTwoParametersTestEventHandler CustomSignatureTwoParametersTestEvent;

        void OnCustomSignatureThreeParametersTestEvent();
        void OnCustomSignatureTwoParametersTestEvent();
    }

    public abstract class TestEventSourceBase : ITestEventSource
    {
        public event EventHandler TestEvent;
        public event EventHandler<TestEventArgs> GenericTestEvent;
        public event EventHandler<TestEventArgs> GenericTestEventForStaticHandlers;
        public event TestEventHandler CustomHandlerTestEvent;
        public event TestEventHandler CustomHandlerTestEventForStaticHandlers;
        public event CustomSignatureMoreThanTwoParametersTestEventHandler CustomSignatureThreeParametersTestEvent;
        public event CustomSignatureTwoParametersTestEventHandler CustomSignatureTwoParametersTestEvent;

        public virtual void OnTestEvent() => this.TestEvent?.Invoke(this, EventArgs.Empty);

        public virtual void OnGenericTestEvent() => this.GenericTestEvent?.Invoke(this, new TestEventArgs());
        public virtual void OnGenericTestEventForStaticHandlers() => this.GenericTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());

        public virtual void OnCustomHandlerTestEvent() => this.CustomHandlerTestEvent?.Invoke(this, new TestEventArgs());
        public virtual void OnCustomHandlerTestEventForStaticHandlers() => this.CustomHandlerTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());
        public virtual void OnCustomSignatureThreeParametersTestEvent() => this.CustomSignatureThreeParametersTestEvent?.Invoke(this, new TestEventArgs(), 99);
        public virtual void OnCustomSignatureTwoParametersTestEvent() => this.CustomSignatureTwoParametersTestEvent?.Invoke(99, new TestEventArgs());
        public abstract void OnStringEventArgsTestEvent();
        public abstract void OnStringEventArgsTestEventForStaticHandlers();
        public abstract void OnStronglyTypedCustomHandlerTestEvent();
        public abstract void OnStronglyTypedCustomHandlerTestEventForStaticHandlers();

        public virtual void RaiseAll()
        {
            OnTestEvent();
            OnGenericTestEvent();
            OnGenericTestEventForStaticHandlers();
            OnCustomHandlerTestEvent();
            OnCustomHandlerTestEventForStaticHandlers();
            OnCustomSignatureTwoParametersTestEvent();
            OnCustomSignatureThreeParametersTestEvent();
            OnStringEventArgsTestEvent();
            OnStringEventArgsTestEventForStaticHandlers();
            OnStronglyTypedCustomHandlerTestEvent();
            OnStronglyTypedCustomHandlerTestEventForStaticHandlers();
        }
    }

    public class TestEventSource1 : TestEventSourceBase, ITestEventSource<TestEventSource1>
    {
        public static event EventHandler<TestEventArgs> StaticGenericTestEvent;
        public static event TestEventHandler StaticCustomHandlerTestEvent;
        public static event StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs> StaticStronglyTypedCustomHandlerTestEvent;
        public event StronglyTypedTestEventHandler<TestEventSource1, string> StringEventArgsTestEvent;
        public event StronglyTypedTestEventHandler<TestEventSource1, string> StringEventArgsTestEventForStaticHandlers;
        public event StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs> StronglyTypedCustomHandlerTestEvent;
        public event StronglyTypedTestEventHandler<TestEventSource1, TestEventArgs> StronglyTypedCustomHandlerTestEventForStaticHandlers;

        public override void RaiseAll()
        {
            base.RaiseAll();
            OnStaticStronglyTypedCustomHandlerTestEvent();
            OnStaticGenericTestEvent();
            OnStaticCustomHandlerTestEvent();
        }

        public override void OnStringEventArgsTestEvent()
          => this.StringEventArgsTestEvent?.Invoke(this, "Some string");

        public override void OnStringEventArgsTestEventForStaticHandlers()
          => this.StringEventArgsTestEventForStaticHandlers?.Invoke(this, "Some string");

        public override void OnStronglyTypedCustomHandlerTestEvent()
          => this.StronglyTypedCustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

        public override void OnStronglyTypedCustomHandlerTestEventForStaticHandlers()
          => this.StronglyTypedCustomHandlerTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());

        public static void OnStaticStronglyTypedCustomHandlerTestEvent()
          => TestEventSource1.StaticStronglyTypedCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

        public static void OnStaticCustomHandlerTestEvent() => StaticCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

        public static void OnStaticGenericTestEvent() => StaticGenericTestEvent?.Invoke(null, new TestEventArgs());
    }

    public class TestEventSource2 : TestEventSourceBase, ITestEventSource<TestEventSource2>
    {
        public static event EventHandler<TestEventArgs> StaticGenericTestEvent;
        public static event TestEventHandler StaticCustomHandlerTestEvent;
        public static event StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs> StaticStronglyTypedCustomHandlerTestEvent;
        public event StronglyTypedTestEventHandler<TestEventSource2, string> StringEventArgsTestEvent;
        public event StronglyTypedTestEventHandler<TestEventSource2, string> StringEventArgsTestEventForStaticHandlers;
        public event StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs> StronglyTypedCustomHandlerTestEvent;
        public event StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs> StronglyTypedCustomHandlerTestEventForStaticHandlers;

        public override void RaiseAll()
        {
            base.RaiseAll();
            OnStaticStronglyTypedCustomHandlerTestEvent();
            OnStaticGenericTestEvent();
            OnStaticCustomHandlerTestEvent();
        }

        public override void OnStringEventArgsTestEvent()
          => this.StringEventArgsTestEvent?.Invoke(this, "Some string");

        public override void OnStringEventArgsTestEventForStaticHandlers()
          => this.StringEventArgsTestEventForStaticHandlers?.Invoke(this, "Some string");

        public override void OnStronglyTypedCustomHandlerTestEvent()
          => this.StronglyTypedCustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

        public override void OnStronglyTypedCustomHandlerTestEventForStaticHandlers()
          => this.StronglyTypedCustomHandlerTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());

        public static void OnStaticStronglyTypedCustomHandlerTestEvent()
          => TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

        public static void OnStaticCustomHandlerTestEvent() => StaticCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

        public static void OnStaticGenericTestEvent() => StaticGenericTestEvent?.Invoke(null, new TestEventArgs());
    }
}
