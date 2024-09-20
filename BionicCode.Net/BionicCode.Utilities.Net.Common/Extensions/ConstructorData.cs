namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using Microsoft.CodeAnalysis;

  internal sealed class ConstructorData : MemberInfoData
  {
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private ParameterData[] parameters;
    private bool? isStatic;

    public ConstructorData(ConstructorInfo constructorInfo) : base(constructorInfo)
    {
      this.Handle = constructorInfo.MethodHandle;
    }

    public ConstructorInfo GetConstructorInfo()
      => (ConstructorInfo)MethodInfo.GetMethodFromHandle(this.Handle);

    protected override MemberInfo GetMemberInfo() 
      => GetConstructorInfo();

    public RuntimeMethodHandle Handle { get; set; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public ParameterData[] Parameters
      => this.parameters ?? (this.parameters = GetConstructorInfo().GetParameters().Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<ParameterData>).ToArray());

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override char[] Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false).ToCharArray());

    public override char[] FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false).ToCharArray());

    public override bool IsStatic => (bool)(this.isStatic ?? (this.isStatic = GetConstructorInfo().IsStatic));
  }
}