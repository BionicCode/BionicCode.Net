namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;

    internal sealed class FieldData : MemberData, IFieldDataInvoker
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
        private Func<object?, object?>? _getValueInvoker;
        private Action<object?, object?>? _referenceTypeSetValueInvoker;
        private readonly ConcurrentDictionary<RuntimeTypeHandle, Delegate> _valueTypeSetValueInvokerTable;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;

        internal FieldData(FieldInfo fieldInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey)
            : base(fieldInfo, SymbolKind.MemberField, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            this._valueTypeSetValueInvokerTable = new ConcurrentDictionary<RuntimeTypeHandle, Delegate>();
            this.Handle = fieldInfo.FieldHandle;
        }

        public FieldInfo GetFieldInfo()
          => FieldInfo.GetFieldFromHandle(this.Handle);

        protected override MemberInfo GetMemberInfo()
          => GetFieldInfo();

        public object? GetValue(object? target)
            => (this._getValueInvoker ??= DelegateProvider.CreateGetter(this)).Invoke(target);

        /// <summary>
        /// Sets the value of a field on a struct instance using the specified value.
        /// </summary>
        /// <remarks>Use this method to set the value of a field on a struct instance when direct
        /// assignment is not possible, such as when working with reflection. This method enforces type compatibility
        /// between the target struct and the value being assigned.<para/>
        /// For fields declared on a reference type or static fields use <see cref="SetValue(object?, object?)"/> instead.</remarks>
        /// <typeparam name="TTarget">The type of the struct containing the field to set. Must be a value type.</typeparam>
        /// <typeparam name="TValue">The type of the value to assign to the field.</typeparam>
        /// <param name="target">A reference to the struct instance whose field value will be set.</param>
        /// <param name="value">The value to assign to the field. The type must match the field's type. Can be null for nullable fields.</param>
        /// <exception cref="InvalidOperationException">Thrown if the field is static or if the declaring type is not a value type.</exception>
        /// <exception cref="ArgumentException">Thrown if the target type does not match the declaring type of the field.</exception>
        /// <exception cref="ArgumentException">Thrown if the value type does not match the field type.</exception>
        public void SetStructValue<TTarget, TValue>(ref TTarget target, TValue? value) where TTarget : struct
        {
            if (this.IsStatic)
            {
                throw new InvalidOperationException($"Cannot set struct instance field value on a static field. Call '{nameof(SetValue)}' instead");
            }

            if (!this.DeclaringTypeData.IsValueType)
            {
                throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetValue)));
            }

            Type declaringType = this.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                declaringType,
                targetType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(TTarget),
                        declaringType,
                        "declaring type"));

            if (value is not null)
            {
                Type valueType = typeof(TValue);
                Type fieldType = this.FieldTypeData.UnwrapType();
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    valueType,
                    fieldType,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            valueType,
                            nameof(TValue),
                            fieldType,
                            "field type"));
            }

            RuntimeTypeHandle targetTypeHandle = targetType.TypeHandle;
            Delegate? cachedInvoker = this._valueTypeSetValueInvokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStructSetter<TTarget, TValue>(this));
            ValueTypeMemberSetter<TTarget, TValue> invoker = (ValueTypeMemberSetter<TTarget, TValue>)cachedInvoker;
            invoker.Invoke(ref target, value);
        }

        /// <summary>
        /// Sets the value of the field on the specified target object.
        /// </summary>
        /// <param name="target">The object whose field value will be set. Must be an instance of the declaring type if the field is not
        /// static; otherwise, this parameter is ignored.</param>
        /// <param name="value">The value to assign to the field. The value must be of the same type as the field or null if the field type
        /// is a reference type.</param>
        /// <remarks>Use this method to set the value of a field on an object instance or a static field.
        /// For fields declared on value types use <see cref="SetStructValue{TTarget, TValue}(ref TTarget, TValue)"/> instead.</remarks>
        /// <exception cref="InvalidOperationException">Thrown if the declaring type of the field is not a value type.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the target is null for an instance field.</exception>
        /// <exception cref="ArgumentException">Thrown if the value is not of the same type as the field.</exception>
        /// <exception cref="ArgumentException">Thrown if the target is not of the declaring type for an instance field.</exception>"
        public void SetValue(object? target, object? value)
        {
            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
                Type targetType = target.GetType();
                Type declaringType = this.DeclaringTypeData.UnwrapType();
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    targetType,
                    declaringType,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            targetType,
                            nameof(target),
                            declaringType,
                            "declaring type"));
            }

            if (value is not null)
            {
                Type valueType = value.GetType();
                Type fieldType = this.FieldTypeData.UnwrapType();
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    valueType,
                    fieldType,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            valueType,
                            nameof(value),
                            fieldType,
                            "field type"));
            }

            if (this.DeclaringTypeData.IsValueType)
            {
                throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetStructValue)));
            }

            this._referenceTypeSetValueInvoker ??= DelegateProvider.CreateSetter(this);
            this._referenceTypeSetValueInvoker.Invoke(target, value);
        }

        public RuntimeFieldHandle Handle { get; }

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

        bool IFieldDataInvoker.IsInvocable { get; }

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

        #region IFieldDataInvoker

        void IFieldDataInvoker.SetGetterInvoker(Func<object?, object?>? getInvoker) => throw new NotImplementedException();
        void IFieldDataInvoker.SetSetterInvoker(Action<object?, object?>? setInvoker) => throw new NotImplementedException();

        #endregion IFieldDataInvoker
    }
}
