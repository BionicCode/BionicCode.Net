namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using Microsoft.CodeAnalysis;

  internal sealed class ConstructorData : MemberInfoData
  {
    private string displayName;
    private string shortDisplayName;
    private string fullyQualifiedDisplayName;
    private string signature;
    private string shortSignature;
    private string fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private ParameterData[] parameters;
    private bool? isStatic;
    private Func<object[], object> invocator;
    private string assemblyName;

    public ConstructorData(ConstructorInfo constructorInfo) : base(constructorInfo)
    {
      this.Handle = constructorInfo.MethodHandle;
    }

    public ConstructorInfo GetConstructorInfo()
      => (ConstructorInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle);

    protected override MemberInfo GetMemberInfo() 
      => GetConstructorInfo();

    public object Invoke(params object[] arguments)
    {
      if (this.invocator is null)
      {
        InitializeInvocator();
      }

      return this.invocator.Invoke(arguments);
    }

    public Func<object[], object> GetInvocator()
    {
      if (this.invocator is null)
      {
        InitializeInvocator();
      }

      return this.invocator;
    }

    private void InitializeInvocator()
      => this.invocator = invocationArguments => GetConstructorInfo().Invoke(invocationArguments);

    public RuntimeMethodHandle Handle { get; set; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public ParameterData[] Parameters
      => this.parameters ?? (this.parameters = GetConstructorInfo().GetParameters().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: false, isCompact: false));

    public override string ShortSignature
      => this.shortSignature ?? (this.shortSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override string DisplayName
      => this.displayName ?? (this.displayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: false));

    public override string ShortDisplayName
      => this.shortDisplayName ?? (this.shortDisplayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: true));

    public override string FullyQualifiedDisplayName 
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = GetType().ToFullDisplayName());

    public override string AssemblyName
      => this.assemblyName ?? (this.assemblyName = this.DeclaringTypeData.AssemblyName);

    public override bool IsStatic => (bool)(this.isStatic ?? (this.isStatic = GetConstructorInfo().IsStatic));
  }
}