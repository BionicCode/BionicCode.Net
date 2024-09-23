namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading.Tasks;

  internal readonly struct MethodInvokeInfo
  {
    public MethodInvokeInfo(object target,
      object[] arguments,
      string signature,
      Func<object, object[], object> synchronousInvocator)
    {
      this.SynchronousInvocator = synchronousInvocator;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.Target = target;
      this.Signature = signature;
    }

    public MethodInvokeInfo(object target,
      object[] arguments,
      string signature,
      Func<object, object[], Task> asynchronousTaskInvocator)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = asynchronousTaskInvocator;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.Target = target;
      this.Signature = signature;
    }

    public MethodInvokeInfo(object target,
      object[] arguments,
      string signature,
      Func<object, object[], ValueTask> asynchronousValueTaskInvocator)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = asynchronousValueTaskInvocator;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.Target = target;
      this.Signature = signature;
    }

    public MethodInvokeInfo(object target,
      string signature,
      object[] arguments,
      Func<object, object[], dynamic> asynchronousGenericValueTaskInvocator)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = asynchronousGenericValueTaskInvocator;
      this.Arguments = arguments;
      this.Target = target;
      this.Signature = signature;
    }

    public Func<object, object[], object> SynchronousInvocator { get; }
    public Func<object, object[], Task> AsynchronousTaskInvocator { get; }
    public Func<object, object[], ValueTask> AsynchronousValueTaskInvocator { get; }
    public Func<object, object[], dynamic> AsynchronousGenericValueTaskInvocator { get; }
    public object[] Arguments { get; }
    public object Target { get; }
    public string Signature { get; }
  }
}