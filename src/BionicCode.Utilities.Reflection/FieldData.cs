namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
using System.Reflection;

internal sealed class FieldData : MemberData, IFieldDataInvoker
{
    private string? _displayName;
    private string? _shortDisplayName;
    private string? _fullyQualifiedDisplayName;
    private string? _signature;
    private string? _shortSignature;
    private string? _shortCompactSignature;
    private string? _fullyQualifiedSignature;
    private string? _fullyQualifiedRuntimeSignature;
    private string? _runtimeSignature;
    private string? _runtimeShortSignature;
    private string? _runtimeShortCompactSignature;
    private SymbolAttributes _symbolAttributes;
    private AccessModifier _accessModifier;
    private bool? _isStatic;
    private TypeData? _fieldTypeData;
    private bool? _isRef;
    private bool? _isConst;
    private bool? _isInitOnly;
    private bool? _isPublic;
    private bool? _isPrivate;
    private bool? _isAssembly;
    private bool? _isFamily;
    private bool? _isFamilyOrAssembly;
    private bool? _isFamilyAndAssembly;
    private Func<object?, object?>? _getValueInvoker;
    private Action<object?, object?>? _referenceTypeSetValueInvoker;
    private readonly ConcurrentDictionary<RuntimeTypeHandle, Delegate> _valueTypeSetValueInvokerTable;
    private readonly WellKnownFieldDescriptor _descriptor;
    private string? _assemblyName;
    private SymbolComponentInfo? _symbolComponentInfo;
    private IFieldDataView? _fieldDataView;

    internal FieldData(SymbolReflectionInfoCacheKeyInternal symbolInfoDataCacheKey)
        : base(symbolInfoDataCacheKey.FieldDescriptor.FieldName, SymbolKind.MemberField, symbolInfoDataCacheKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(symbolInfoDataCacheKey);

        _descriptor = symbolInfoDataCacheKey.FieldDescriptor;
        _valueTypeSetValueInvokerTable = new ConcurrentDictionary<RuntimeTypeHandle, Delegate>();
        FieldInfo = symbolInfoDataCacheKey.FieldDescriptor.FieldInfo;
        Handle = FieldInfo.FieldHandle;
        DeclaringTypeHandle = FieldInfo.DeclaringType?.TypeHandle ?? throw new InvalidOperationException("Declaring type handle is not available.");
        IsExplicitInterfaceImplementation = false; // Fields cannot be explicit interface implementations
        ImplementingTypeHandle = DeclaringTypeHandle;
    }
    internal IFieldDataView View => _fieldDataView
        ??= new FieldDataView(SymbolReflectionInfoCacheKey.CreateForField(this));

    internal FieldInfo FieldInfo { get; }

    protected override MemberInfo GetMemberInfo() => FieldInfo;

    internal object? GetValue(object? target) => (_getValueInvoker ??= DelegateProvider.CreateGetter(this)).Invoke(target);

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
    internal void SetStructValue<TTarget, TValue>(ref TTarget target, TValue? value) where TTarget : struct
    {
        if (IsStatic)
        {
            throw new InvalidOperationException($"Cannot set struct instance field value on a static field. Call '{nameof(SetValue)}' instead");
        }

        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetValue)));
        }

        Type declaringType = DeclaringTypeData.Type;
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
            Type fieldType = FieldTypeData.Type;
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
        Delegate? cachedInvoker = _valueTypeSetValueInvokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStructSetter<TTarget, TValue>(this));
        var invoker = (ValueTypeMemberSetter<TTarget, TValue>)cachedInvoker;
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
    internal void SetValue(object? target, object? value)
    {
        if (!IsStatic)
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));
            Type targetType = target.GetType();
            Type declaringType = DeclaringTypeData.Type;
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
            Type fieldType = FieldTypeData.Type;
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                valueType,
                fieldType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        valueType,
                        nameof(value),
                        fieldType,
                        "field type"));
        }

        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetStructValue)));
        }

        _referenceTypeSetValueInvoker ??= DelegateProvider.CreateSetter(this);
        _referenceTypeSetValueInvoker.Invoke(target, value);
    }

    internal RuntimeFieldHandle Handle { get; }

    internal override RuntimeTypeHandle DeclaringTypeHandle { get; }
    internal override bool IsExplicitInterfaceImplementation { get; }
    internal override RuntimeTypeHandle ImplementingTypeHandle { get; }

    internal override AccessModifier AccessModifier => _accessModifier is AccessModifier.Undefined
        ? (_accessModifier = FieldData.GetAccessModifierInternal(this))
        : _accessModifier;

    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
        ? (_symbolAttributes = FieldData.GetAttributes(this))
        : _symbolAttributes;

    internal override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    internal override string Signature => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortSignature => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortCompactSignature => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    internal override string FullyQualifiedSignature => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string FullyQualifiedRuntimeSignature => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeSignature => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortSignature => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortCompactSignature => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    internal override string DisplayName => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string ShortDisplayName => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    internal override string FullyQualifiedDisplayName => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string AssemblyName => _assemblyName ??= DeclaringTypeData.AssemblyName;

    internal override bool IsStatic => _isStatic ??= FieldInfo.IsStatic;

    internal TypeData FieldTypeData => _fieldTypeData ??= SymbolReflectionInfoCache.GetOrCreateEntryInternal(FieldInfo.FieldType);

    internal bool IsRef => _isRef ??= FieldTypeData.IsByRef;

    internal bool IsConst => _isConst ??= IsFieldConst(this);

    internal bool IsInitOnly => _isInitOnly ??= FieldInfo.IsInitOnly;

    internal bool IsReadonly => IsInitOnly && !IsConst;

    internal override bool IsPublic => _isPublic ??= FieldInfo.IsPublic;

    internal override bool IsPrivate => _isPrivate ??= FieldInfo.IsPrivate;

    internal override bool IsAssembly => _isAssembly ??= FieldInfo.IsAssembly;

    internal override bool IsFamily => _isFamily ??= FieldInfo.IsFamily;

    internal override bool IsFamilyOrAssembly => _isFamilyOrAssembly ??= FieldInfo.IsFamilyOrAssembly;

    internal override bool IsFamilyAndAssembly => _isFamilyAndAssembly ??= FieldInfo.IsFamilyAndAssembly;

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

    private static bool IsFieldConst(FieldData fieldData) => fieldData.FieldInfo.IsLiteral;

    private static AccessModifier GetAccessModifierInternal(FieldData fieldData) => fieldData.IsPublic ? AccessModifier.Public
        : fieldData.IsPrivate ? AccessModifier.Private
        : fieldData.IsAssembly ? AccessModifier.Internal
        : fieldData.IsFamily ? AccessModifier.Protected
        : fieldData.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : fieldData.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");

    #region IFieldDataInvoker

    void IFieldDataInvoker.SetGetterInvoker(Func<object?, object?>? getInvoker) => throw new NotImplementedException();
    void IFieldDataInvoker.SetSetterInvoker(Action<object?, object?>? setInvoker) => throw new NotImplementedException();

    #endregion IFieldDataInvoker
}
