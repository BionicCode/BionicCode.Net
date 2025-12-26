namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    internal sealed class FieldData : MemberInfoData
    {
        private string? displayName;
        private string? shortDisplayName;
        private string? fullyQualifiedDisplayName;
        private string? signature;
        private string? shortSignature;
        private string? shortCompactSignature;
        private string? fullyQualifiedSignature;
        private string? fullyQualifiedRuntimeSignature;
        private string? runtimeSignature;
        private string? runtimeShortSignature;
        private string? runtimeShortCompactSignature;
        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? isStatic;
        private TypeData? fieldTypeData;
        private bool? isRef;
        private bool? isConst;
        private bool? isInitOnly;
        private bool? _isPublic;
        private bool? _isPrivate;
        private bool? _isAssembly;
        private bool? _isFamily;
        private bool? _isFamilyOrAssembly;
        private bool? _isFamilyAndAssembly;
        private MethodData? getValueInvocator;
        private MethodData? setValueInvocator;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;

        public FieldData(FieldInfo fieldInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(fieldInfo, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            this.Handle = fieldInfo.FieldHandle;
        }

        public FieldInfo GetFieldInfo()
          => FieldInfo.GetFieldFromHandle(this.Handle);

        protected override MemberInfo GetMemberInfo()
          => GetFieldInfo();

        public object? GetValue(object? target)
            => this.GetValueInvocator.Invoke(target);

        public void SetValue(object? target, object? value)
            => _ = this.SetValueInvocator.Invoke(target, value);

        public RuntimeFieldHandle Handle { get; }

        public MethodData GetValueInvocator
        {
            get
            {
                if (this.getValueInvocator is null)
                {
                    MethodInfo? fieldAccessor = GetFieldInfo().GetType().GetMethod(nameof(FieldInfo.GetValue));
                    if (fieldAccessor is null)
                    {
                        throw new InvalidOperationException("Unable to retrieve field accessor method info.");
                    }

                    MethodData fieldAccessorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldAccessor!);
                    this.getValueInvocator = fieldAccessorData.GetInvoker();
                }

                return this.getValueInvocator;

            }
        }

        public MethodData SetValueInvocator
        {
            get
            {
                if (this.setValueInvocator is null)
                {
                    MethodInfo? fieldAccessor = GetFieldInfo().GetType().GetMethod(nameof(FieldInfo.SetValue));
                    if (fieldAccessor is null)
                    {
                        throw new InvalidOperationException("Unable to retrieve field accessor method info.");
                    }

                    MethodData fieldAccessorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldAccessor!);
                    this.setValueInvocator = fieldAccessorData.GetInvoker();
                }

                return this.setValueInvocator;
            }
        }

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = FieldData.GetAccessModifierInternal(this))
          : this.accessModifier;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = FieldData.GetAttributes(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public override bool IsStatic
            => this.isStatic ??= GetFieldInfo().IsStatic;

        public TypeData FieldTypeData
          => this.fieldTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetFieldInfo().FieldType);

        public bool IsRef
          => this.isRef ??= this.FieldTypeData.IsByRef;

        public bool IsConst
          => this.isConst ??= IsFieldConst(this);

        public bool IsInitOnly
          => this.isInitOnly ??= GetFieldInfo().IsInitOnly;

        public bool IsReadonly
          => this.IsInitOnly && !this.IsConst;

        public override bool IsPublic
            => this._isPublic ??= GetFieldInfo().IsPublic;

        public override bool IsPrivate
            => this._isPrivate ??= GetFieldInfo().IsPrivate;

        public override bool IsAssembly
            => this._isAssembly ??= GetFieldInfo().IsAssembly;

        public override bool IsFamily
            => this._isFamily ??= GetFieldInfo().IsFamily;

        public override bool IsFamilyOrAssembly
            => this._isFamilyOrAssembly ??= GetFieldInfo().IsFamilyOrAssembly;

        public override bool IsFamilyAndAssembly
            => this._isFamilyAndAssembly ??= GetFieldInfo().IsFamilyAndAssembly;

        /// <summary>
        /// Determines the set of symbol attributes for the specified field based on its metadata and characteristics.
        /// </summary>
        /// <remarks>The returned attributes reflect the field's characteristics, including whether it is
        /// static, constant, read-only, or by-reference. This method is intended for internal use when mapping field
        /// metadata to symbol attributes.<para/>
        /// For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
        /// <param name="fieldData">The field metadata used to evaluate and construct the corresponding symbol attributes.</param>
        /// <returns>A bitwise combination of <see cref="SymbolAttributes"/> values that represent the attributes of the field,
        /// such as static, constant, or by-reference.</returns>
        private static SymbolAttributes GetAttributes(FieldData fieldData)
        {
            SymbolAttributes fieldAttributes = SymbolAttributes.Field;
            if (fieldData.IsInitOnly)
            {
                fieldAttributes |= SymbolAttributes.Final;
            }

            if (fieldData.IsRef)
            {
                fieldAttributes |= SymbolAttributes.ByReference;
            }

            if (fieldData.IsStatic)
            {
                fieldAttributes |= SymbolAttributes.Static;
            }

            if (fieldData.IsConst)
            {
                fieldAttributes |= SymbolAttributes.Constant;
            }

            return fieldAttributes;
        }

        private static bool IsFieldConst(FieldData fieldData)
          => fieldData.GetFieldInfo().IsLiteral;

        private static AccessModifier GetAccessModifierInternal(FieldData fieldData)
        {
            return fieldData.IsPublic ? AccessModifier.Public
              : fieldData.IsPrivate ? AccessModifier.Private
              : fieldData.IsAssembly ? AccessModifier.Internal
              : fieldData.IsFamily ? AccessModifier.Protected
              : fieldData.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
              : fieldData.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        }
    }
}
