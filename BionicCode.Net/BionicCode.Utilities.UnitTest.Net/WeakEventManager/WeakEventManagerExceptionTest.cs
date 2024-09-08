namespace BionicCode.Utilities.Net.UnitTest.WeakEventManager
{
  using System;
  using System.Diagnostics;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using FluentAssertions.Specialized;
  using Xunit;

  public class WeakEventManagerExceptionTest : IDisposable
  {
    public WeakEventManagerExceptionTest()
    {
      this.EventSource1 = new TestEventSource1();
      this.EventSource2 = new TestEventSource2();
      eventHandlerInvocationCount = 0;
    }

    #region Event validation

    [Fact]
    public async Task RegisterEvent_SpecifiyUndefinedEvent_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, "Undefined Event", OnNonGenericTestEventFromTestEventSource1)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task RegisterEvent_EventDelegateWithInvalidSignatureTooManyParameters_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.WrongSignatureTooManyParametersTestEvent), OnNonGenericTestEventFromTestEventSource1)).Should().Throw<EventDelegateNotSupportedException>().Which.Message.Should().Contain("because the parameter count");
    }

    [Fact]
    public async Task RegisterEvent_EventDelegateWithInvalidSignatureWrongSenderType_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.WrongSignatureInvalidSenderTypeTestEvent), OnNonGenericTestEventFromTestEventSource1)).Should().Throw<EventDelegateNotSupportedException>().Which.Message.Should().Contain("because the parameter at index '0' is not of type");
    }

    [Fact]
    public async Task RegisterEvent_EventDelegateWithInvalidSignatureWrongEventArgsType_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1)).Should().Throw<EventDelegateMismatchException>();
    }

    [Fact]
    public async Task RegisterEvent_EventDelegateWithWrongTEventArgs_ShouldThrowEventDelegateMismatchException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource1)).Should().Throw<EventDelegateMismatchException>().Which.Message.Should().Contain("Event delegate signature mismatch. The provided generic type argument");
    }

    #endregion Event validation

    #region Event handler validation

    [Fact]
    public async Task RegisterEvent_EventHandlerWithInvalidSignatureTooManyParameters_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.GenericTestEvent), OnTestEventFromTestEventSourceWrongSignature)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Invalid parameter count");
    }

    [Fact]
    public void RegisterEventHandler_WithMoreDerivedSenderThanEventDelegate_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, EventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource1)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter");
    }

    [Fact]
    public async Task RegisterEvent_EventHandlerWithInvalidSignatureWrongEventArgsType_ShouldThrowException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, EventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1)).Should().Throw<EventHandlerMismatchException>().Which.Message.Should().Contain("Unable to cast parameter");
    }

    [Fact]
    public async Task RegisterEvent_EventHandlerNonGenericWithWrongEventArgsType_ShouldThrowHandlerDelegateMismatchException()
    {
      _ = this.Invoking(testEnvironment => WeakEventManager<TestEventSource1, EventArgs>.AddEventHandler(this.EventSource1, nameof(TestEventSource1.TestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource1)).Should().Throw<EventHandlerMismatchException>();
    }

    #endregion Event handler validation

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

    private void OnTestEventFromTestEventSourceWrongSignature(object sender, TestEventArgs e, int value)
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
