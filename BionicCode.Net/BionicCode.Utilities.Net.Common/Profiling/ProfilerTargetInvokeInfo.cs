namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading.Tasks;

  internal readonly struct ProfilerTargetInvokeInfo
  {
    public ProfilerTargetInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName,
      Func<object, object[], object> synchronousInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.SynchronousInvocator = synchronousInvocator;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.ProfiledTargetType = profiledTargetType;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }

    public ProfilerTargetInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName,
      Func<object, object[], Task> asynchronousTaskInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = asynchronousTaskInvocator;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.ProfiledTargetType = profiledTargetType;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }

    public ProfilerTargetInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName,
      Func<object, object[], ValueTask> asynchronousValueTaskInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = asynchronousValueTaskInvocator;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.ProfiledTargetType = profiledTargetType;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }

    public ProfilerTargetInvokeInfo(object target, 
      string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName,
      object[] arguments,
      int argumentListIndex,
      Func<object, object[], dynamic> asynchronousGenericValueTaskInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = asynchronousGenericValueTaskInvocator;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.ProfiledTargetType = profiledTargetType;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }

    public ProfilerTargetInvokeInfo(string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName)
    {
      this.SynchronousInvocator = null;
      this.AsynchronousTaskInvocator = null;
      this.AsynchronousValueTaskInvocator = null;
      this.AsynchronousGenericValueTaskInvocator = null;
      this.Arguments = Array.Empty<object>();
      this.ArgumentListIndex = -1;
      this.Target = null;
      this.ProfiledTargetType = ProfiledTargetType.Scope;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }

    public Func<object, object[], object> SynchronousInvocator { get; }
    public Func<object, object[], Task> AsynchronousTaskInvocator { get; }
    public Func<object, object[], ValueTask> AsynchronousValueTaskInvocator { get; }
    public Func<object, object[], dynamic> AsynchronousGenericValueTaskInvocator { get; }
    public object[] Arguments { get; }
    public int ArgumentListIndex { get; }
    public object Target { get; }
    public string Signature { get; }
    public string DisplayName { get; }
    public string Namespace { get; }
    public string AssemblyName { get; }
    public ProfiledTargetType ProfiledTargetType { get; }
  }
}