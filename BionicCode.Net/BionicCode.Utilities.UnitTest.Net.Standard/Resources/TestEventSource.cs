using System;

namespace BionicCode.Utilities.UnitTest.Net.Standard.Resources
{
  public delegate void TestEventHandler(object sender, TestEventArgs e);

  public class TestEventArgs : EventArgs
  {
    
  }

  public interface ITestEventSource
  {
    event EventHandler TestEvent;
    event TestEventHandler CustomHandlerTestEvent;
  }

  public class TestEventSource : ITestEventSource
  {
    public event EventHandler TestEvent;
    public event EventHandler<TestEventArgs> GenericTestEvent;
    public event TestEventHandler CustomHandlerTestEvent;

    public virtual void OnTestEvent()
    {
      this.TestEvent?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnGenericTestEvent()
    {
      this.GenericTestEvent?.Invoke(this, new TestEventArgs());
    }

    protected virtual void OnCustomHandlerTestEvent()
    {
      this.CustomHandlerTestEvent?.Invoke(this, new TestEventArgs());
    }

    public virtual void RaiseAll()
    {
      OnTestEvent();
      OnGenericTestEvent();
      OnCustomHandlerTestEvent();
    }
  }

  public class TestEventSource2 : ITestEventSource
  {
    public event EventHandler TestEvent;
    public event EventHandler<TestEventArgs> GenericTestEvent;
    public event TestEventHandler CustomHandlerTestEvent;

    public virtual void OnTestEvent()
    {
      this.TestEvent?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnGenericTestEvent()
    {
      this.GenericTestEvent?.Invoke(this, new TestEventArgs());
    }

    protected virtual void OnCustomHandlerTestEvent()
    {
      this.CustomHandlerTestEvent?.Invoke(this, new TestEventArgs());
    }

    public virtual void RaiseAll()
    {
      OnTestEvent();
      OnGenericTestEvent();
      OnCustomHandlerTestEvent();
    }
  }
}
