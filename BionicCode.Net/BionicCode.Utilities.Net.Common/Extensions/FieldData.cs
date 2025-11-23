namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    internal sealed class FieldData : MemberInfoData
    {
        private string displayName;
        private string shortDisplayName;
        private string fullyQualifiedDisplayName;
        private string signature;
        private string shortSignature;
        private string shortCompactSignature;
        private string fullyQualifiedSignature;
        private string fullyQualifiedRuntimeSignature;
        private string runtimeSignature;
        private string runtimeShortSignature;
        private string runtimeShortCompactSignature;
        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? isStatic;
        private TypeData fieldTypeData;
        private bool? isRef;
        private Func<object, object> getInvocator;
        private Action<object, object> setInvocator;
        private string assemblyName;
        private SymbolComponentInfo symbolComponentInfo;

        public FieldData(FieldInfo fieldInfo) : base(fieldInfo) => this.Handle = fieldInfo.FieldHandle;

        public FieldInfo GetFieldInfo()
          => FieldInfo.GetFieldFromHandle(this.Handle);

        protected override MemberInfo GetMemberInfo()
          => GetFieldInfo();

        public object GetValue(object target)
        {
            this.getInvocator ??= invocationTarget => GetFieldInfo().GetValue(invocationTarget);

            return this.getInvocator.Invoke(target);
        }

        public void SetValue(object target, object value)
        {
            this.setInvocator ??= (invocationTarget, fieldValue) => GetFieldInfo().SetValue(invocationTarget, fieldValue);

            this.setInvocator.Invoke(target, value);
        }

        public RuntimeFieldHandle Handle { get; set; }

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
          : this.accessModifier;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= HelperExtensionsCommon.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public override bool IsStatic => (bool)(bool?)(this.isStatic ??= GetFieldInfo().IsStatic);

        public TypeData FieldTypeData
          => this.fieldTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetFieldInfo().FieldType);

        public bool IsRef
          => (bool)(bool?)(this.isRef ??= this.FieldTypeData.IsByRef);
    }
}
