namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using FluentAssertions.Specialized;
  using Xunit;

  public class WeakEventManagerTest
  {
    public WeakEventManagerTest()
    {
      this.EventSource1 = new TestEventSource();
      this.EventSource2 = new TestEventSource2();
    }

    public void RegisterAllEventHandlersEventSource1()
    {
      WeakEventManager<TestEventSource, EventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.TestEvent), OnNonGenericTestEventFromTestEventSource);

      WeakEventManager<TestEventSource, EventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSource);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.GenericTestEvent), OnGenericTestEventFromTestEventSourceStatic);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource1.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSourceStatic);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource1.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSourceStatic);

      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSourceStatic);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource1.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSourceStatic);

      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(this.EventSource1, nameof(this.EventSource1.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSourceStatic);
      WeakEventManager<TestEventSource, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource1.StaticStronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSourceStatic);
    }

    public void RegisterAllEventHandlersEventSource2()
    {
      WeakEventManager<TestEventSource2, EventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.TestEvent), OnNonGenericTestEventFromTestEventSource2);

      WeakEventManager<TestEventSource2, EventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.GenericTestEvent), OnGenericTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.GenericTestEvent), OnGenericTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource2.StaticGenericTestEvent), OnGenericTestEventFromStaticTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource2.StaticGenericTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.CustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource2.StaticCustomHandlerTestEvent), OnStronglyTypedEventArgsTestEventFromStaticTestEventSource2Static);

      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(this.EventSource2, nameof(this.EventSource2.StronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource2Static);
      WeakEventManager<TestEventSource2, TestEventArgs>.AddEventHandler(null, nameof(this.EventSource2.StaticStronglyTypedCustomHandlerTestEvent), OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource2Static);
    }

    [Fact]
    public void HandleEventHandlerGenericShouldInvokeHandlerOnce()
    {
      this.EventSource1.OnGenericTestEvent();

      _ = eventHandlerInvocationCount.Should().Be(1);
    }

    private void OnGenericTestEventFromTestEventSource(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
    }

    private void OnNonGenericTestEventFromTestEventSource(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
      _ = e.Should().BeSameAs(EventArgs.Empty);
    }

    private static void OnGenericTestEventFromTestEventSourceStatic(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
    }

    private static void OnGenericTestEventFromStaticTestEventSourceStatic(object sender, EventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
    }

    private void OnStronglyTypedEventArgsTestEventFromTestEventSource(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromTestEventSourceStatic(object sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSource(TestEventSource sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromTestEventSourceStatic(TestEventSource sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedEventArgsTestEventFromStaticTestEventSource(TestEventSource sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedEventArgsTestEventFromStaticTestEventSourceStatic(TestEventSource sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSource(TestEventSource sender, TestEventArgs e)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeNull();
      _ = e.Should().BeOfType<TestEventArgs>();
    }

    private static void OnStronglyTypedSenderAndEventArgsTestEventFromStaticTestEventSourceStatic(TestEventSource sender, TestEventArgs e)
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

    private void OnTestEventFromTestEventSourceWrongSignature(object sender, EventArgs e, int value)
    {
      eventHandlerInvocationCount++;
      _ = sender.Should().BeOfType<TestEventSource>();
      _ = e.Should().BeOfType<EventArgs>();
    }

    private TestEventSource EventSource1 { get; }
    private TestEventSource2 EventSource2 { get; }
    private static int eventHandlerInvocationCount;
  }
}
