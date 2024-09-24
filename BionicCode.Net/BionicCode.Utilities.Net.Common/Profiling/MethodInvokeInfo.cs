namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading.Tasks;

  internal readonly struct MethodInvokeInfo
  {
    public MethodInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string signature,
      Func<object, object[], object> synchronousInvocator,
      Runtime runtime,
      ProfiledTargetType profiledTargetType,
      string assemblyName,
      TimeUnit baseUnit)
    {
      this.SynchronousInvocator = synchronousInvocator;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.Signature = signature;
      this.Runtime = runtime;
      this.ProfiledTargetType = profiledTargetType;
      this.AssemblyName = assemblyName;
      this.BaseUnit = baseUnit;
    }

    public MethodInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string signature,
      Func<object, object[], Task> asynchronousTaskInvocator,
      Runtime runtime,
      ProfiledTargetType profiledTargetType,
      string assemblyName,
      TimeUnit baseUnit)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = asynchronousTaskInvocator;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.Signature = signature;
      this.Runtime = runtime;
      this.ProfiledTargetType = profiledTargetType;
      this.AssemblyName = assemblyName;
      this.BaseUnit = baseUnit;
    }

    public MethodInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string signature,
      Func<object, object[], ValueTask> asynchronousValueTaskInvocator,
      Runtime runtime,
      ProfiledTargetType profiledTargetType,
      string assemblyName,
      TimeUnit baseUnit)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = asynchronousValueTaskInvocator;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.Signature = signature;
      this.Runtime = runtime;
      this.ProfiledTargetType = profiledTargetType;
      this.AssemblyName = assemblyName;
      this.BaseUnit = baseUnit;
    }

    public MethodInvokeInfo(object target,
      string signature,
      object[] arguments,
      int argumentListIndex,
      Func<object, object[], dynamic> asynchronousGenericValueTaskInvocator,
      Runtime runtime,
      ProfiledTargetType profiledTargetType,
      string assemblyName,
      TimeUnit baseUnit)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = asynchronousGenericValueTaskInvocator;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.Signature = signature;
      this.Runtime = runtime;
      this.ProfiledTargetType = profiledTargetType;
      this.AssemblyName = assemblyName;
      this.BaseUnit = baseUnit;
    }

    public Func<object, object[], object> SynchronousInvocator { get; }
    public Func<object, object[], Task> AsynchronousTaskInvocator { get; }
    public Func<object, object[], ValueTask> AsynchronousValueTaskInvocator { get; }
    public Func<object, object[], dynamic> AsynchronousGenericValueTaskInvocator { get; }
    public object[] Arguments { get; }
    public int ArgumentListIndex { get; }
    public object Target { get; }
    public string Signature { get; }
    public string AssemblyName { get; }
    public Runtime Runtime { get; }
    public ProfiledTargetType ProfiledTargetType { get; }
    public TimeUnit BaseUnit { get; }
  }
}