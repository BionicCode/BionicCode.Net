namespace BionicCode.Utilities.Net.UnitTest.Resources
{
  using System;
  using System.Diagnostics.Tracing;

  public delegate void TestEventHandler(object sender, TestEventArgs e);
  public delegate void StronglyTypedTestEventHandler<TSender>(TSender sender, TestEventArgs e);
  public delegate void WrongSignatureTestEventHandler(object sender, TestEventArgs e, int value);

  public class TestEventArgs : EventArgs
  {

  }

  public interface ITestEventSource<TEventSource> : ITestEventSource
  {
    event StronglyTypedTestEventHandler<TEventSource> StronglyTypedCustomHandlerTestEvent;
    static event StronglyTypedTestEventHandler<TEventSource> StaticStronglyTypedCustomHandlerTestEvent;

    void OnStronglyTypedCustomHandlerTestEvent();
  }

  public interface ITestEventSource
  {
    event EventHandler TestEvent;
    event TestEventHandler CustomHandlerTestEvent;
    event EventHandler<TestEventArgs> GenericTestEvent;
    static event TestEventHandler StaticCustomHandlerTestEvent;
    static event EventHandler<TestEventArgs> StaticGenericTestEvent;

    void OnTestEvent();

    void OnGenericTestEvent();

    void OnCustomHandlerTestEvent();

    void RaiseAll();
  }

  public abstract class TestEventSourceBase : ITestEventSource
  {
    public static event EventHandler<TestEventArgs> StaticGenericTestEvent;
    public static event TestEventHandler StaticCustomHandlerTestEvent;
    public event EventHandler TestEvent;
    public event EventHandler<TestEventArgs> GenericTestEvent;
    public event TestEventHandler CustomHandlerTestEvent;

    public virtual void OnTestEvent() => this.TestEvent?.Invoke(this, EventArgs.Empty);

    public virtual void OnGenericTestEvent() => this.GenericTestEvent?.Invoke(this, new TestEventArgs());

    public virtual void OnCustomHandlerTestEvent() => this.CustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

    public virtual void RaiseAll()
    {
      OnTestEvent();
      OnGenericTestEvent();
      OnCustomHandlerTestEvent();
      OnStaticGenericTestEvent();
      OnStaticCustomHandlerTestEvent();
    }

    public static void OnStaticCustomHandlerTestEvent() => StaticCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

    public static void OnStaticGenericTestEvent() => StaticGenericTestEvent?.Invoke(null, new TestEventArgs());
  }

  public class TestEventSource : TestEventSourceBase, ITestEventSource<TestEventSource>
  {
    public event StronglyTypedTestEventHandler<TestEventSource> StronglyTypedCustomHandlerTestEvent;
    public static event StronglyTypedTestEventHandler<TestEventSource> StaticStronglyTypedCustomHandlerTestEvent;

    public void OnStronglyTypedCustomHandlerTestEvent()
      => this.StronglyTypedCustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

    public static void OnStaticStronglyTypedCustomHandlerTestEvent()
      => TestEventSource.StaticStronglyTypedCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());
  }

  public class TestEventSource2 : TestEventSourceBase, ITestEventSource<TestEventSource2>
  {
    public event StronglyTypedTestEventHandler<TestEventSource2> StronglyTypedCustomHandlerTestEvent;
    public static event StronglyTypedTestEventHandler<TestEventSource> StaticStronglyTypedCustomHandlerTestEvent;

    public void OnStronglyTypedCustomHandlerTestEvent()
      => this.StronglyTypedCustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

    public static void OnStaticStronglyTypedCustomHandlerTestEvent()
      => TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());
  }
}
