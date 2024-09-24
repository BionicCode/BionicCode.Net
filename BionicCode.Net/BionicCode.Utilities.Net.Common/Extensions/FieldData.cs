namespace BionicCode.Utilities.Net
{
  using System;
  using System.Reflection;

  internal sealed class FieldData : MemberInfoData
  {
    private string displayName;
    private string fullyQualifiedDisplayName;
    private string signature;
    private string fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private bool? isStatic;
    private TypeData fieldTypeData;
    private bool? isRef;
    private Func<object, object> getInvocator;
    private Action<object, object> setInvocator;
    private string assemblyName;

    public FieldData(FieldInfo fieldInfo) : base(fieldInfo)
    {
      this.Handle = fieldInfo.FieldHandle;
    }

    public FieldInfo GetFieldInfo()
      => FieldInfo.GetFieldFromHandle(this.Handle);

    protected override MemberInfo GetMemberInfo()
      => GetFieldInfo();

    public object Get(object target)
    {
      if (this.getInvocator is null)
      {
        this.getInvocator = invocationTarget => GetFieldInfo().GetValue(invocationTarget);
      }

      return this.getInvocator.Invoke(target);
    }

    public void Set(object target, object value)
    {
      if (this.setInvocator is null)
      {
        this.setInvocator = (invocationTarget, fieldValue) => GetFieldInfo().SetValue(invocationTarget, fieldValue);
      }

      this.setInvocator.Invoke(target, value);
    }

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

    public override string DisplayName
      => this.displayName ?? (this.displayName = GetType().ToDisplayName());

    public override string FullyQualifiedDisplayName
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = GetType().ToFullDisplayName());

    public override string AssemblyName
      => this.assemblyName ?? (this.assemblyName = this.DeclaringTypeData.AssemblyName);

    public override bool IsStatic => (bool)(this.isStatic ?? (this.isStatic = GetFieldInfo().IsStatic));

    public TypeData FieldTypeData
      => this.fieldTypeData ?? (this.fieldTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(this.GetFieldInfo().FieldType));

    public bool IsRef
      => (bool)(this.isRef ?? (this.isRef = this.FieldTypeData.IsByRef));
  }
}