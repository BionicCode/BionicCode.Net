namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using Microsoft.CodeAnalysis;

  internal sealed class ConstructorData : MemberInfoData
  {
    private string displayName;
    private string fullyQualifiedDisplayName;
    private string signature;
    private string fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private ParameterData[] parameters;
    private bool? isStatic;
    private Func<object, object[], object> invocator;

    public ConstructorData(ConstructorInfo constructorInfo) : base(constructorInfo)
    {
      this.Handle = constructorInfo.MethodHandle;
    }

    public ConstructorInfo GetConstructorInfo()
      => (ConstructorInfo)MethodInfo.GetMethodFromHandle(this.Handle);

    protected override MemberInfo GetMemberInfo() 
      => GetConstructorInfo();

    public object Invoke(object target, params object[] arguments)
    {
      if (this.invocator is null)
      {
        this.invocator = (invocationTarget, invocationArguments) => GetConstructorInfo().Invoke(invocationTarget, invocationArguments);
      }

      return this.invocator.Invoke(target, arguments);
    }

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
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override string DisplayName
      => this.displayName ?? (this.displayName = GetType().ToDisplayName());

    public override string FullyQualifiedDisplayName 
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = GetType().ToFullDisplayName());

    public override bool IsStatic => (bool)(this.isStatic ?? (this.isStatic = GetConstructorInfo().IsStatic));
  }
}