namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;
  using Microsoft.CodeAnalysis;

  internal sealed class MethodData : MemberInfoData
  {
    private string displayName;
    private string fullyQualifiedDisplayName;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private bool? isAwaitable;
    private bool? isAwaitableTask;
    private bool? isAwaitableValueTask;
    private bool? isAwaitableGenericValueTask;
    private bool? isAsync;
    private bool? isSealed;
    private bool? isExtensionMethod;
    private ParameterData[] parameters;
    private TypeData[] genericTypeArguments;
    private bool? isOverride;
    private bool? isStatic;
    private string signature;
    private string fullyQualifiedSignature;
    private TypeData returnTypeData;
    private bool? isGenericMethod;
    private bool? isGenericTypeMethod;
    private MethodData genericMethodDefinitionData;
    private bool? isReturnValueByRef;
    private Func<object, object[], object> invocator;
    private Func<object, object[], Task> awaitableTaskInvocator;
    private Func<object, object[], dynamic> awaitableGenericValueTaskInvocator;
    private Func<object, object[], ValueTask> awaitableValueTaskInvocator;

#if !NETSTANDARD2_0
    private bool? isReturnValueReadOnly;
#endif

    public MethodData(MethodInfo methodInfo) : base(methodInfo)
    {
      this.Handle = methodInfo.MethodHandle;
    }

    public MethodInfo GetMethodInfo()
      => (MethodInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle);

    protected override MemberInfo GetMemberInfo() 
      => GetMethodInfo();

    public object Invoke(object target, params object[] arguments)
    {
      if (this.invocator is null)
      {
        InitializeInvocator();
      }

      return this.invocator.Invoke(target, arguments);
    }

    public Task InvokeAwaitableTaskAsync(object target, params object[] arguments)
    {
      if (!this.IsAwaitableTask)
      {
        throw new InvalidOperationException($"Method does not return an awaitable 'Task'. Call {nameof(this.IsAwaitableTask)} to ensure the method is awaitable returns a 'Task'.");
      }

      if (this.awaitableTaskInvocator is null)
      {
        InitializeAwaitableTaskInvocator();
      }

      return this.awaitableTaskInvocator.Invoke(target, arguments);
    }

    public async Task InvokeAwaitableValueTaskAsync(object target, params object[] arguments)
    {
      if (!this.IsAwaitableValueTask)
      {
        throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask'. Call {nameof(this.IsAwaitableValueTask)} to ensure the method is awaitable and returns a 'ValueTask'.");
      }

      if (this.ReturnTypeData.IsGenericType)
      {
        if (this.awaitableGenericValueTaskInvocator is null)
        {
          InitializeAwaitableGenericValueTaskInvocator();
        }

        await this.awaitableGenericValueTaskInvocator.Invoke(target, arguments);
      }
      else
      {
        if (this.awaitableValueTaskInvocator is null)
        {
          InitializeAwaitableValueTaskInvocator();
        }

        await this.awaitableValueTaskInvocator.Invoke(target, arguments);
      }
    }

    public Func<object, object[], object> GetInvocator()
    {
      if (!this.IsAwaitableGenericValueTask)
      {
        throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask'. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the method is awaitable and returns a 'ValueTask'.");
      }

      if (this.awaitableGenericValueTaskInvocator is null)
      {
        InitializeInvocator();
      }

      return this.invocator;
    }

    public Func<object, object[], dynamic> GetAwaitableGenericValueTaskInvocator()
    {
      if (!this.IsAwaitableGenericValueTask)
      {
        throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask'. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the method is awaitable and returns a 'ValueTask'.");
      }

      if (this.awaitableGenericValueTaskInvocator is null)
      {
        InitializeAwaitableGenericValueTaskInvocator();
      }

      return this.awaitableGenericValueTaskInvocator;
    }

    public Func<object, object[], ValueTask> GetAwaitableValueTaskInvocator()
    {
      if (!this.IsAwaitableValueTask)
      {
        throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask'. Call {nameof(this.IsAwaitableValueTask)} to ensure the method is awaitable and returns a 'ValueTask'.");
      }

      InitializeAwaitableValueTaskInvocator();

      return this.awaitableValueTaskInvocator;
    }

    public Func<object, object[], Task> GetAwaitableTaskInvocator()
    {
      if (!this.IsAwaitableTask)
      {
        throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask'. Call {nameof(this.IsAwaitableTask)} to ensure the method is awaitable and returns a 'ValueTask'.");
      }

      if (this.awaitableTaskInvocator is null)
      {
        InitializeAwaitableTaskInvocator();
      }

      return this.awaitableTaskInvocator;
    }

    private void InitializeInvocator()
    {
      if (this.awaitableTaskInvocator is null)
      {
        this.invocator = (invocationTarget, invocationArguments) => GetMethodInfo().Invoke(invocationTarget, invocationArguments);
      }
    }

    private void InitializeAwaitableTaskInvocator()
    {
      if (this.awaitableTaskInvocator is null)
      {
        this.awaitableTaskInvocator = (invocationTarget, invocationArguments) => (Task)GetMethodInfo().Invoke(invocationTarget, invocationArguments);
      }
    }

    private void InitializeAwaitableValueTaskInvocator()
    {
      if (this.awaitableValueTaskInvocator is null)
      {
        this.awaitableValueTaskInvocator = (invocationTarget, invocationArguments) => (ValueTask)GetMethodInfo().Invoke(invocationTarget, invocationArguments);
      }
    }

    private void InitializeAwaitableGenericValueTaskInvocator()
    {
      if (this.awaitableGenericValueTaskInvocator is null)
      {
        this.awaitableGenericValueTaskInvocator = (invocationTarget, invocationArguments) => GetMethodInfo().Invoke(invocationTarget, invocationArguments);
      }
    }

    public RuntimeMethodHandle Handle { get; }

    public MethodData GenericMethodDefinitionData
    {
      get
      {
        if (this.IsGenericMethodDefinition)
        {
          return this;
        }
        else
        {
          MethodInfo genericMethodDefinitionMethodInfo = GetMethodInfo().GetGenericMethodDefinition();
          this.genericMethodDefinitionData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericMethodDefinitionMethodInfo);
        }

        return this.genericMethodDefinitionData;
      }
    }

    public ParameterData[] Parameters 
      => this.parameters ?? (this.parameters = GetMethodInfo().GetParameters().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public TypeData[] GenericTypeArguments 
    {
      get
      {
        if (this.genericTypeArguments is null)
        {
          Type[] typeArguments = GetMethodInfo().GetGenericArguments();
          this.genericTypeArguments = typeArguments.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();
        }

        return this.genericTypeArguments;
      }
    }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public bool IsExtensionMethod 
      => (bool)(this.isExtensionMethod ?? (this.isExtensionMethod = HelperExtensionsCommon.IsExtensionMethodInternal(this)));

    public bool IsAsync 
      => (bool)(this.isAsync ?? (this.isAsync = HelperExtensionsCommon.IsMarkedAsyncInternal(this)));

    public bool IsAwaitable 
      => (bool)(this.isAwaitable ?? (this.isAwaitable = HelperExtensionsCommon.IsAwaitableInternal(this)));

    public bool IsAwaitableTask
    {
      get
      {
        if (this.isAwaitableTask is null)
        {
          if (!this.IsAwaitable)
          {
            this.isAwaitableTask = false;
          }
          else
          {
            this.isAwaitableTask = (this.isAwaitableValueTask.HasValue
              && !this.isAwaitableValueTask.Value
              && HelperExtensionsCommon.IsAwaitableValueTask(this.ReturnTypeData.GetType()))
              || (!this.isAwaitableValueTask.HasValue && HelperExtensionsCommon.IsAwaitableValueTask(this.ReturnTypeData.GetType()));
          }
        }

        return this.isAwaitableTask.Value;
      }
    }

    public bool IsAwaitableValueTask
    {
      get
      {
        if (this.isAwaitableValueTask is null)
        {
          if (!this.IsAwaitable || this.ReturnTypeData.IsGenericType)
          {
            this.isAwaitableValueTask = false;
          }
          else
          {
            this.isAwaitableValueTask = (this.isAwaitableTask.HasValue
              && !this.isAwaitableTask.Value
              && HelperExtensionsCommon.IsAwaitableValueTask(this.ReturnTypeData.GetType()))
              || (!this.isAwaitableTask.HasValue && HelperExtensionsCommon.IsAwaitableValueTask(this.ReturnTypeData.GetType()));
          }
        }

        return this.isAwaitableValueTask.Value;
      }
    }

    public bool IsAwaitableGenericValueTask
    {
      get
      {
        if (this.isAwaitableGenericValueTask is null)
        {
          if (!this.IsAwaitable || !this.ReturnTypeData.IsGenericType)
          {
            this.isAwaitableGenericValueTask = false;
          }
          else
          {
            this.isAwaitableGenericValueTask = (this.isAwaitableTask.HasValue
              && !this.isAwaitableTask.Value
              && HelperExtensionsCommon.IsAwaitableValueTask(this.ReturnTypeData.GetType()))
              || (!this.isAwaitableTask.HasValue && HelperExtensionsCommon.IsAwaitableValueTask(this.ReturnTypeData.GetType()));
          }
        }

        return this.isAwaitableValueTask.Value;
      }
    }

    public bool IsOverride 
      => (bool)(this.isOverride ?? (this.isOverride = HelperExtensionsCommon.IsOverrideInternal(this)));

    public override bool IsStatic 
      => (bool)(this.isStatic ?? (this.isStatic = GetMethodInfo().IsStatic));

    public bool IsSealed
      => (bool)(this.isSealed ?? (this.isSealed = GetMethodInfo().IsFinal));

#if !NETSTANDARD2_0
    public bool IsReturnValueReadOnly
      => (bool)(this.isReturnValueReadOnly ?? (this.isReturnValueReadOnly = GetMethodInfo().ReturnParameter.GetCustomAttribute(typeof(IsReadOnlyAttribute)) != null));
#endif

    public bool IsReturnValueByRef
      => (bool)(this.isReturnValueByRef ?? (this.isReturnValueByRef = this.ReturnTypeData.IsByRef));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature 
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override string DisplayName
      => this.displayName ?? (this.displayName = GetType().ToDisplayName());

    public override string FullyQualifiedDisplayName
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = GetType().ToFullDisplayName());

    public TypeData ReturnTypeData 
      => this.returnTypeData ?? (this.returnTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetMethodInfo().ReturnType));

    public bool IsGenericMethod
      => (bool)(this.isGenericMethod ?? (this.isGenericMethod = GetMethodInfo().IsGenericMethod));

    public bool IsGenericMethodDefinition
      => (bool)(this.isGenericTypeMethod ?? (this.isGenericTypeMethod = GetMethodInfo().IsGenericMethodDefinition));
  }
}