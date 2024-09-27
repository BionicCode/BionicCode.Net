namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading.Tasks;

  internal readonly struct ProfilerTargetInvokeInfo
  {
    /// <summary>
    /// Use for property set()
    /// </summary>
    /// <param name="target"></param>
    /// <param name="arguments"></param>
    /// <param name="argumentListIndex"></param>
    /// <param name="targetSignature"></param>
    /// <param name="targetDisplayName"></param>
    /// <param name="targetNamespace"></param>
    /// <param name="targetAssemblyName"></param>
    /// <param name="propertySetInvocator"></param>
    /// <param name="profiledTargetType"></param>
    public ProfilerTargetInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName,
      Action<object, object, object[]> propertySetInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.PropertySetInvocator = propertySetInvocator;
      this.ConstructorInvocator = null;
      this.SynchronousMethodInvocator = null;
      this.AsynchronousTaskMethodInvocator = null;
      this.AsynchronousValueTaskMethodInvocator = null;
      this.AsynchronousGenericValueTaskMethodInvocator = null;
      this.Arguments = arguments;
      this.ArgumentListIndex = argumentListIndex;
      this.Target = target;
      this.ProfiledTargetType = profiledTargetType;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }
    /// <summary>
    /// Use for constructors
    /// </summary>
    /// <param name="target"></param>
    /// <param name="arguments"></param>
    /// <param name="argumentListIndex"></param>
    /// <param name="targetSignature"></param>
    /// <param name="targetDisplayName"></param>
    /// <param name="targetNamespace"></param>
    /// <param name="targetAssemblyName"></param>
    /// <param name="constructorInvocator"></param>
    /// <param name="profiledTargetType"></param>
    public ProfilerTargetInvokeInfo(object target,
      object[] arguments,
      int argumentListIndex,
      string targetSignature,
      string targetDisplayName,
      string targetNamespace,
      string targetAssemblyName,
      Func<object[], object> constructorInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.PropertySetInvocator = null;
      this.ConstructorInvocator = constructorInvocator;
      this.SynchronousMethodInvocator = null;
      this.AsynchronousTaskMethodInvocator = null;
      this.AsynchronousValueTaskMethodInvocator = null;
      this.AsynchronousGenericValueTaskMethodInvocator = null;
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
      Func<object, object[], object> synchronousMethodInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.PropertySetInvocator = null;
      this.ConstructorInvocator = null;
      this.SynchronousMethodInvocator = synchronousMethodInvocator;
      this.AsynchronousTaskMethodInvocator = null;
      this.AsynchronousValueTaskMethodInvocator = null;
      this.AsynchronousGenericValueTaskMethodInvocator = null;
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
      Func<object, object[], Task> asynchronousTaskMethodInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.PropertySetInvocator = null;
      this.ConstructorInvocator = null;
      this.SynchronousMethodInvocator = null;
      this.AsynchronousTaskMethodInvocator = asynchronousTaskMethodInvocator;
      this.AsynchronousValueTaskMethodInvocator = null;
      this.AsynchronousGenericValueTaskMethodInvocator = null;
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
      Func<object, object[], ValueTask> asynchronousValueTaskMethodInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.PropertySetInvocator = null;
      this.ConstructorInvocator = null;
      this.SynchronousMethodInvocator = null;
      this.AsynchronousTaskMethodInvocator = null;
      this.AsynchronousValueTaskMethodInvocator = asynchronousValueTaskMethodInvocator;
      this.AsynchronousGenericValueTaskMethodInvocator = null;
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
      Func<object, object[], dynamic> asynchronousGenericValueTaskMethodInvocator,
      ProfiledTargetType profiledTargetType)
    {
      this.PropertySetInvocator = null;
      this.ConstructorInvocator = null;
      this.SynchronousMethodInvocator = null;
      this.AsynchronousTaskMethodInvocator = null;
      this.AsynchronousValueTaskMethodInvocator = null;
      this.AsynchronousGenericValueTaskMethodInvocator = asynchronousGenericValueTaskMethodInvocator;
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
      this.PropertySetInvocator = null;
      this.ConstructorInvocator = null;
      this.SynchronousMethodInvocator = null;
      this.AsynchronousTaskMethodInvocator = null;
      this.AsynchronousValueTaskMethodInvocator = null;
      this.AsynchronousGenericValueTaskMethodInvocator = null;
      this.Arguments = Array.Empty<object>();
      this.ArgumentListIndex = -1;
      this.Target = null;
      this.ProfiledTargetType = ProfiledTargetType.Scope;
      this.Signature = targetSignature;
      this.AssemblyName = targetAssemblyName;
      this.DisplayName = targetDisplayName;
      this.Namespace = targetNamespace;
    }

    public Action<object, object, object[]> PropertySetInvocator { get; }
    public Func<object[], object> ConstructorInvocator { get; }
    public Func<object, object[], object> SynchronousMethodInvocator { get; }
    public Func<object, object[], Task> AsynchronousTaskMethodInvocator { get; }
    public Func<object, object[], ValueTask> AsynchronousValueTaskMethodInvocator { get; }
    public Func<object, object[], dynamic> AsynchronousGenericValueTaskMethodInvocator { get; }
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