namespace BionicCode.Utilities.Net.UnitTest.Resources
{
  using System;
  using System.Diagnostics.Tracing;

  public delegate void TestEventHandler(object sender, TestEventArgs e);
  public delegate void StronglyTypedTestEventHandler<TSender, TEventArgs>(TSender sender, TEventArgs e);
  public delegate void WrongSignatureTooManyParametersTestEventHandler(object sender, TestEventArgs e, int value);
  public delegate void WrongSignatureInvalidSenderTypeTestEventHandler(int sender, TestEventArgs e);

  public class TestEventArgs : EventArgs
  {

  }

  public interface ITestEventSource<TEventSource> : ITestEventSource
  {
    event StronglyTypedTestEventHandler<TEventSource, TestEventArgs> StronglyTypedCustomHandlerTestEvent;
    event StronglyTypedTestEventHandler<TEventSource, TestEventArgs> StronglyTypedCustomHandlerTestEventForStaticHandlers;

    void OnStronglyTypedCustomHandlerTestEvent();
    void OnStronglyTypedCustomHandlerTestEventForStaticHandlers();
  }

  public interface ITestEventSource
  {
    event EventHandler TestEvent;
    event TestEventHandler CustomHandlerTestEvent;
    event TestEventHandler CustomHandlerTestEventForStaticHandlers;
    event EventHandler<TestEventArgs> GenericTestEvent;
    event EventHandler<TestEventArgs> GenericTestEventForStaticHandlers;
    event WrongSignatureTooManyParametersTestEventHandler WrongSignatureTooManyParametersTestEvent;
    event WrongSignatureInvalidSenderTypeTestEventHandler WrongSignatureInvalidSenderTypeTestEvent;

    void OnTestEvent();
    void OnGenericTestEvent();
    void OnGenericTestEventForStaticHandlers();
    void OnCustomHandlerTestEvent();
    void OnCustomHandlerTestEventForStaticHandlers();
    void OnWrongSignatureTooManyParametersTestEvent(); 
    void OnWrongSignatureInvalidSenderTypeTestEvent();
    void OnStringEventArgsTestEvent();
    void RaiseAll();
  }

  public abstract class TestEventSourceBase : ITestEventSource
  {
    public event EventHandler TestEvent;
    public event EventHandler<TestEventArgs> GenericTestEvent;
    public event EventHandler<TestEventArgs> GenericTestEventForStaticHandlers;
    public event TestEventHandler CustomHandlerTestEvent;
    public event TestEventHandler CustomHandlerTestEventForStaticHandlers;
    public event WrongSignatureTooManyParametersTestEventHandler WrongSignatureTooManyParametersTestEvent;
    public event WrongSignatureInvalidSenderTypeTestEventHandler WrongSignatureInvalidSenderTypeTestEvent;

    public virtual void OnTestEvent() => this.TestEvent?.Invoke(this, EventArgs.Empty);

    public virtual void OnGenericTestEvent() => this.GenericTestEvent?.Invoke(this, new TestEventArgs());
    public virtual void OnGenericTestEventForStaticHandlers() => this.GenericTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());

    public virtual void OnCustomHandlerTestEvent() => this.CustomHandlerTestEvent?.Invoke(this, new TestEventArgs());
    public virtual void OnCustomHandlerTestEventForStaticHandlers() => this.CustomHandlerTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());
    public virtual void OnWrongSignatureTooManyParametersTestEvent() => this.WrongSignatureTooManyParametersTestEvent?.Invoke(this, new TestEventArgs(), 99);
    public virtual void OnWrongSignatureInvalidSenderTypeTestEvent() => this.WrongSignatureInvalidSenderTypeTestEvent?.Invoke(99, new TestEventArgs());
    public abstract void OnStringEventArgsTestEvent();

    public virtual void RaiseAll()
    {
      OnTestEvent();
      OnGenericTestEvent();
      OnGenericTestEventForStaticHandlers ();
      OnCustomHandlerTestEvent();
      OnCustomHandlerTestEventForStaticHandlers();
      OnStringEventArgsTestEvent();
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
      OnStronglyTypedCustomHandlerTestEvent();
      OnStronglyTypedCustomHandlerTestEventForStaticHandlers();
      OnStaticStronglyTypedCustomHandlerTestEvent();
      OnStaticGenericTestEvent();
      OnStaticCustomHandlerTestEvent();
    }

    public override void OnStringEventArgsTestEvent() 
      => this.StringEventArgsTestEvent?.Invoke(this, "Some string");

    public void OnStronglyTypedCustomHandlerTestEvent()
      => this.StronglyTypedCustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

    public void OnStronglyTypedCustomHandlerTestEventForStaticHandlers()
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
    public event StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs> StronglyTypedCustomHandlerTestEvent;
    public event StronglyTypedTestEventHandler<TestEventSource2, TestEventArgs> StronglyTypedCustomHandlerTestEventForStaticHandlers;

    public override void RaiseAll()
    {
      base.RaiseAll();
      OnStronglyTypedCustomHandlerTestEvent();
      OnStronglyTypedCustomHandlerTestEventForStaticHandlers();
      OnStaticStronglyTypedCustomHandlerTestEvent();
      OnStaticGenericTestEvent();
      OnStaticCustomHandlerTestEvent();
    }

    public override void OnStringEventArgsTestEvent()
      => this.StringEventArgsTestEvent?.Invoke(this, "Some string");

    public void OnStronglyTypedCustomHandlerTestEvent()
      => this.StronglyTypedCustomHandlerTestEvent?.Invoke(this, new TestEventArgs());

    public void OnStronglyTypedCustomHandlerTestEventForStaticHandlers()
      => this.StronglyTypedCustomHandlerTestEventForStaticHandlers?.Invoke(this, new TestEventArgs());

    public static void OnStaticStronglyTypedCustomHandlerTestEvent()
      => TestEventSource2.StaticStronglyTypedCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

    public static void OnStaticCustomHandlerTestEvent() => StaticCustomHandlerTestEvent?.Invoke(null, new TestEventArgs());

    public static void OnStaticGenericTestEvent() => StaticGenericTestEvent?.Invoke(null, new TestEventArgs());
  }
}
