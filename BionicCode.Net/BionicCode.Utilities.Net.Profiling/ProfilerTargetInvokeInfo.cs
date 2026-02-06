namespace BionicCode.Utilities.Net.Profiling;

using System;
using System.Threading.Tasks;

internal abstract class ProfilerTargetInvokeInfo
{
    /// <summary>
    /// Use for property set()
    /// </summary>
    /// <param name="target"></param>
    /// <param name="methodArguments"></param>
    /// <param name="argumentListIndex"></param>
    /// <param name="targetSignature"></param>
    /// <param name="targetDisplayName"></param>
    /// <param name="targetNamespace"></param>
    /// <param name="targetAssemblyName"></param>
    /// <param name="propertySetInvocator"></param>
    /// <param name="profiledTargetType"></param>
    protected ProfilerTargetInvokeInfo(object? target,
      string targetSignature,
      string targetDisplayName,
      string targetShortSignature,
      string targetShortDisplayName,
      SymbolComponentInfo symbolComponentInfo,
      string targetNamespace,
      string targetAssemblyName,
      ProfiledTargetType profiledTargetType)
    {
        Target = target;
        ProfiledTargetType = profiledTargetType;
        Signature = targetSignature;
        AssemblyName = targetAssemblyName;
        DisplayName = targetDisplayName;
        Namespace = targetNamespace;
        ShortDisplayName = targetShortDisplayName;
        ShortSignature = targetShortSignature;
        SymbolComponentInfo = symbolComponentInfo;
    }

    public string Signature { get; }
    public string DisplayName { get; }
    public string ShortSignature { get; }
    public string ShortDisplayName { get; }
    public SymbolComponentInfo? SymbolComponentInfo { get; }
    public string Namespace { get; }
    public string AssemblyName { get; }
    public object? Target { get; }
    public ProfiledTargetType ProfiledTargetType { get; }
}

internal class ProfilerMethodInvokeInfo : ProfilerTargetInvokeInfo
{
    #region Constructors

    private ProfilerMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, object?> synchronousMethodInvocator,
        ProfiledTargetType profiledTargetType) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            profiledTargetType)
    {
        ArgumentNullException.ThrowIfNull(synchronousMethodInvocator);
        SynchronousMethodInvoker = synchronousMethodInvocator;
        MethodArgument = methodArgument;
    }

    private ProfilerMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, Task> asynchronousTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            profiledTargetType)
    {
        ArgumentNullException.ThrowIfNull(asynchronousTaskMethodInvocator);
        AsynchronousTaskMethodInvoker = asynchronousTaskMethodInvocator;
        MethodArgument = methodArgument;
    }

    private ProfilerMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, Task<object?>> asynchronousGenericTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            profiledTargetType)
    {
        ArgumentNullException.ThrowIfNull(asynchronousGenericTaskMethodInvocator);
        AsynchronousGenericTaskMethodInvoker = asynchronousGenericTaskMethodInvocator;
        MethodArgument = methodArgument;
    }

    private ProfilerMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, ValueTask> asynchronousValueTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            profiledTargetType)
    {
        ArgumentNullException.ThrowIfNull(asynchronousValueTaskMethodInvocator);
        AsynchronousValueTaskMethodInvoker = asynchronousValueTaskMethodInvocator;
        MethodArgument = methodArgument;
    }

    private ProfilerMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, ValueTask<object?>> asynchronousGenericValueTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            profiledTargetType)
    {
        ArgumentNullException.ThrowIfNull(asynchronousGenericValueTaskMethodInvocator);
        AsynchronousGenericValueTaskMethodInvoker = asynchronousGenericValueTaskMethodInvocator;
        MethodArgument = methodArgument;
    }

    #endregion Constructors

    public static ProfilerMethodInvokeInfo CreateSynchronousMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, object?> synchronousMethodInvocator,
        ProfiledTargetType profiledTargetType) => new ProfilerMethodInvokeInfo(target,
            methodArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            synchronousMethodInvocator,
            profiledTargetType);

    public static ProfilerMethodInvokeInfo CreateAsynchronousTaskMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, Task> asynchronousTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) => new ProfilerMethodInvokeInfo(target,
            methodArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            asynchronousTaskMethodInvocator,
            profiledTargetType);

    public static ProfilerMethodInvokeInfo CreateAsynchronousGenericTaskMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, Task<object?>> asynchronousGenericTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) => new ProfilerMethodInvokeInfo(target,
            methodArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            asynchronousGenericTaskMethodInvocator,
            profiledTargetType);

    public static ProfilerMethodInvokeInfo CreateAsynchronousValueTaskMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, ValueTask> asynchronousValueTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) => new ProfilerMethodInvokeInfo(target,
            methodArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            asynchronousValueTaskMethodInvocator,
            profiledTargetType);

    public static ProfilerMethodInvokeInfo CreateAsynchronousGenericValueTaskMethodInvokeInfo(object? target,
        MethodArgumentInfo methodArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, ValueTask<object?>> asynchronousGenericValueTaskMethodInvocator,
        ProfiledTargetType profiledTargetType) => new ProfilerMethodInvokeInfo(target,
            methodArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            asynchronousGenericValueTaskMethodInvocator,
            profiledTargetType);

    public Func<object?, object?[]?, object?>? SynchronousMethodInvoker { get; }
    public Func<object?, object?[]?, Task>? AsynchronousTaskMethodInvoker { get; }
    public Func<object?, object?[]?, Task<object?>>? AsynchronousGenericTaskMethodInvoker { get; }
    public Func<object?, object?[]?, ValueTask>? AsynchronousValueTaskMethodInvoker { get; }
    public Func<object?, object?[]?, ValueTask<object?>>? AsynchronousGenericValueTaskMethodInvoker { get; }
    public MethodArgumentInfo MethodArgument { get; }
}

internal class ProfilerConstructorInvokeInfo : ProfilerTargetInvokeInfo
{
    /// <summary>
    /// Use for property set()
    /// </summary>
    /// <param name="target"></param>
    /// <param name="methodArguments"></param>
    /// <param name="argumentListIndex"></param>
    /// <param name="targetSignature"></param>
    /// <param name="targetDisplayName"></param>
    /// <param name="targetNamespace"></param>
    /// <param name="targetAssemblyName"></param>
    /// <param name="propertySetInvocator"></param>
    /// <param name="profiledTargetType"></param>
    private ProfilerConstructorInvokeInfo(object? target,
      MethodArgumentInfo methodArgument,
      string targetSignature,
      string targetDisplayName,
      string targetShortSignature,
      string targetShortDisplayName,
      SymbolComponentInfo symbolComponentInfo,
      string targetNamespace,
      string targetAssemblyName,
      Func<object?[]?, object> constructorInvocator,
      ProfiledTargetType profiledTargetType) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            profiledTargetType)
    {
        ArgumentNullException.ThrowIfNull(constructorInvocator);
        ConstructorInvoker = constructorInvocator;
        MethodArgument = methodArgument;
    }

    public static ProfilerConstructorInvokeInfo CreateConstructorInvokeInfo(object? target,
      MethodArgumentInfo methodArgument,
      string targetSignature,
      string targetDisplayName,
      string targetShortSignature,
      string targetShortDisplayName,
      SymbolComponentInfo symbolComponentInfo,
      string targetNamespace,
      string targetAssemblyName,
      Func<object?[]?, object> constructorInvocator,
      ProfiledTargetType profiledTargetType) => new ProfilerConstructorInvokeInfo(target,
          methodArgument,
          targetSignature,
          targetDisplayName,
          targetShortSignature,
          targetShortDisplayName,
          symbolComponentInfo,
          targetNamespace,
          targetAssemblyName,
          constructorInvocator,
          profiledTargetType);

    public Func<object?[]?, object>? ConstructorInvoker { get; }
    public MethodArgumentInfo MethodArgument { get; }
}

internal class ProfilerPropertyInvokeInfo : ProfilerTargetInvokeInfo
{
    private ProfilerPropertyInvokeInfo(object? target,
        PropertyArgumentInfo propertyArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName) : base(target,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            ProfiledTargetType.PropertyGet) => PropertyArgument = propertyArgument;

    public static ProfilerPropertyGetInvokeInfo CreatePropertyGetInvokeInfo(object? target,
        PropertyArgumentInfo propertyArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Func<object?, object?[]?, object?> propertyGetInvocator) => new ProfilerPropertyGetInvokeInfo(target,
            propertyArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            propertyGetInvocator);

    public static ProfilerPropertySetInvokeInfo CreatePropertySetInvokeInfo(object target,
        PropertyArgumentInfo propertyArgument,
        string targetSignature,
        string targetDisplayName,
        string targetShortSignature,
        string targetShortDisplayName,
        SymbolComponentInfo symbolComponentInfo,
        string targetNamespace,
        string targetAssemblyName,
        Action<object?, object?, object?[]?> propertySetInvocator) => new ProfilerPropertySetInvokeInfo(target,
            propertyArgument,
            targetSignature,
            targetDisplayName,
            targetShortSignature,
            targetShortDisplayName,
            symbolComponentInfo,
            targetNamespace,
            targetAssemblyName,
            propertySetInvocator);

    public PropertyArgumentInfo PropertyArgument { get; }

    public class ProfilerPropertyGetInvokeInfo : ProfilerPropertyInvokeInfo
    {
        public ProfilerPropertyGetInvokeInfo(object? target,
            PropertyArgumentInfo propertyArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Func<object?, object?[]?, object?> propertyGetInvocator) : base(target,
                propertyArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName)
        {
            ArgumentNullException.ThrowIfNull(propertyGetInvocator);
            PropertyGetInvoker = propertyGetInvocator;
        }

        public Func<object, object?[]?, object?> PropertyGetInvoker { get; }
    }

    public class ProfilerPropertySetInvokeInfo : ProfilerPropertyInvokeInfo
    {
        public ProfilerPropertySetInvokeInfo(object target,
            PropertyArgumentInfo propertyArgument,
            string targetSignature,
            string targetDisplayName,
            string targetShortSignature,
            string targetShortDisplayName,
            SymbolComponentInfo symbolComponentInfo,
            string targetNamespace,
            string targetAssemblyName,
            Action<object?, object?, object?[]?> propertySetInvocator) : base(target,
                propertyArgument,
                targetSignature,
                targetDisplayName,
                targetShortSignature,
                targetShortDisplayName,
                symbolComponentInfo,
                targetNamespace,
                targetAssemblyName)
        {
            ArgumentNullException.ThrowIfNull(propertySetInvocator);
            PropertySetInvoker = propertySetInvocator;
        }

        public Action<object?, object?, object?[]?> PropertySetInvoker { get; }
    }
}
