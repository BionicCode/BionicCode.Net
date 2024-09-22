namespace BionicCode.Utilities.Net
{
  using System;
  using System.Reflection;

  internal sealed class FieldData : MemberInfoData
  {
    private string signature;
    private string fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private bool? isStatic;
    private TypeData fieldTypeData;
    private bool? isRef;

    public FieldData(FieldInfo fieldInfo) : base(fieldInfo)
    {
      this.Handle = fieldInfo.FieldHandle;
    }

    public FieldInfo GetFieldInfo()
      => FieldInfo.GetFieldFromHandle(this.Handle);

    protected override MemberInfo GetMemberInfo()
      => GetFieldInfo();

    public RuntimeFieldHandle Handle { get; set; }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override bool IsStatic => (bool)(this.isStatic ?? (this.isStatic = GetFieldInfo().IsStatic));

    public TypeData FieldTypeData

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      => this.fieldTypeData ?? (this.fieldTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(this.GetFieldInfo().FieldType));
After:
      => this.fieldTypeData ?? (this.fieldTypeData = Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<TypeData>(this.GetFieldInfo().FieldType));
*/
      => this.fieldTypeData ?? (this.fieldTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<TypeData>(this.GetFieldInfo().FieldType));

    public bool IsRef
      => (bool)(this.isRef ?? (this.isRef = this.FieldTypeData.IsByRef));
  }
}