namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Runtime.CompilerServices;

public readonly struct SymbolReflectionInfoCacheKey : IEquatable<SymbolReflectionInfoCacheKey>
{
    private readonly RuntimeTypeHandle _declaringTypeHandle;
    private readonly RuntimeMethodHandle _methodHandle;
    private readonly RuntimeTypeHandle _typeHandle;
    private readonly RuntimeFieldHandle _fieldHandle;
    private readonly RuntimeMethodHandle _propertySetMethodHandle;
    private readonly RuntimeMethodHandle _propertyGetMethodHandle;
    private readonly RuntimeTypeHandle _propertyTypeHandle;
    private readonly RuntimeMethodHandle _eventAddMethodHandle;
    private readonly RuntimeMethodHandle _eventRemoveMethodHandle;
    private readonly ParameterKind _parameterModifier;
    private readonly int _parameterPosition;
    private readonly RuntimeTypeHandle _parameterTypeHandle;
    private readonly RuntimeMethodHandle _parameterDeclaringMethodHandle;

    private SymbolReflectionInfoCacheKey(RuntimeTypeHandle declaringTypeHandle, RuntimeMethodHandle methodHandle, RuntimeTypeHandle typeHandle, RuntimeFieldHandle fieldHandle, RuntimeMethodHandle propertySetMethodHandle, RuntimeMethodHandle propertyGetMethodHandle, RuntimeTypeHandle propertyTypeHandle, RuntimeMethodHandle eventAddMethodHandle, RuntimeMethodHandle eventRemoveMethodHandle, ParameterKind parameterModifier, int parameterPosition, RuntimeTypeHandle parameterTypeHandle, RuntimeMethodHandle parameterDeclaringMethodHandle, string symbolName, SymbolKind symbolKind, bool isAnonymousKey) : this()
    {
        _declaringTypeHandle = declaringTypeHandle;
        _methodHandle = methodHandle;
        _typeHandle = typeHandle;
        _fieldHandle = fieldHandle;
        _propertySetMethodHandle = propertySetMethodHandle;
        _propertyGetMethodHandle = propertyGetMethodHandle;
        _propertyTypeHandle = propertyTypeHandle;
        _eventAddMethodHandle = eventAddMethodHandle;
        _eventRemoveMethodHandle = eventRemoveMethodHandle;
        _parameterModifier = parameterModifier;
        _parameterPosition = parameterPosition;
        _parameterTypeHandle = parameterTypeHandle;
        _parameterDeclaringMethodHandle = parameterDeclaringMethodHandle;
        SymbolName = symbolName;
        SymbolKind = symbolKind;
        IsAnonymousKey = isAnonymousKey;
    }

    internal static SymbolReflectionInfoCacheKey CreateForProperty(PropertyData propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyData);

        return new(
            propertyData.DeclaringTypeHandle,
            default,
            default,
            default,
            propertyData.PropertySetMethodData.Handle,
            propertyData.PropertyGetMethodData.Handle,
            propertyData.PropertyTypeData.Handle,
            default,
            default,
            default,
            default,
            default,
            default,
            propertyData.Name,
            SymbolKind.MemberProperty,
            false);
    }

    internal static SymbolReflectionInfoCacheKey CreateForField(FieldData fieldData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(fieldData);

        return new(
            fieldData.DeclaringTypeHandle,
            default,
            default,
            fieldData.Handle,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            fieldData.Name,
            SymbolKind.MemberField,
            false);
    }

    internal static SymbolReflectionInfoCacheKey CreateForType(TypeData typeData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(typeData);

        return new(
            default,
            default,
            typeData.Handle,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            typeData.Name,
            SymbolKind.Type,
            false);
    }

    internal static SymbolReflectionInfoCacheKey CreateForMethod(MethodData methodData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodData);

        return new(
            methodData.DeclaringTypeHandle,
            methodData.Handle,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            methodData.Name,
            SymbolKind.MemberMethod,
            false);
    }

    internal static SymbolReflectionInfoCacheKey CreateForConstructor(ConstructorData constructorData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorData);

        return new(
            constructorData.DeclaringTypeHandle,
            constructorData.Handle,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            string.Empty,
            SymbolKind.MemberConstructor,
            false);
    }

    internal static SymbolReflectionInfoCacheKey CreateForEvent(EventData eventData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(eventData);

        return new(
            eventData.DeclaringTypeHandle,
            default,
            default,
            default,
            default,
            default,
            default,
            eventData.AddMethodData.Handle,
            eventData.RemoveMethodData.Handle,
            default,
            default,
            default,
            default,
            eventData.Name,
            SymbolKind.MemberEvent,
            false);
    }

    public string SymbolName { get; }
    public SymbolKind SymbolKind { get; }
    public bool IsAnonymousKey { get; }
    public RuntimeMethodHandle MethodHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberMethod, SymbolKind.MemberConstructor], _methodHandle);
    public RuntimeTypeHandle DeclaringTypeHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberMethod, SymbolKind.MemberConstructor, SymbolKind.MemberProperty, SymbolKind.MemberField, SymbolKind.MemberEvent], _declaringTypeHandle);
    public RuntimeTypeHandle TypHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.Type], _typeHandle);
    public RuntimeFieldHandle FieldHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberField], _fieldHandle);
    public bool IsProperty => SymbolKind is SymbolKind.MemberProperty;
    public RuntimeMethodHandle PropertySetMethodHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberProperty], _propertySetMethodHandle);
    public RuntimeMethodHandle PropertyGetMethodHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberProperty], _propertyGetMethodHandle);
    public RuntimeTypeHandle PropertyTypeHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberProperty], _propertyTypeHandle);
    public bool IsField => SymbolKind is SymbolKind.MemberField;
    public bool IsType => SymbolKind is SymbolKind.Type;
    public bool IsEvent => SymbolKind is SymbolKind.MemberEvent;
    public RuntimeMethodHandle EventAddMethodHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberEvent], _eventAddMethodHandle);
    public RuntimeMethodHandle EventRemoveMethodHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.MemberEvent], _eventRemoveMethodHandle);
    public bool IsMethodOrConstructor => SymbolKind is SymbolKind.MemberConstructor or SymbolKind.MemberMethod;
    public bool IsParameter => SymbolKind is SymbolKind.Parameter;
    public ParameterKind ParameterModifier => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.Parameter], _parameterModifier);
    public RuntimeTypeHandle ParameterTypeHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.Parameter], _parameterTypeHandle);
    public int ParameterPosition => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.Parameter], _parameterPosition);
    public RuntimeMethodHandle ParameterDeclaringMethodHandle => ThrowIfPropertyContextIsInvalidOrReturn([SymbolKind.Parameter], _parameterDeclaringMethodHandle);

    public bool Equals(SymbolReflectionInfoCacheKey other) => SymbolKind == other.SymbolKind
        && SymbolName.Equals(other.SymbolName, StringComparison.Ordinal)
        && IsAnonymousKey == other.IsAnonymousKey
        && MethodHandle.Equals(other.MethodHandle)
        && DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
        && TypHandle.Equals(other.TypHandle)
        && FieldHandle.Equals(other.FieldHandle)
        && PropertySetMethodHandle.Equals(other.PropertySetMethodHandle)
        && PropertyGetMethodHandle.Equals(other.PropertyGetMethodHandle)
        && PropertyTypeHandle.Equals(other.PropertyTypeHandle)
        && EventAddMethodHandle.Equals(other.EventAddMethodHandle)
        && EventRemoveMethodHandle.Equals(other.EventRemoveMethodHandle)
        && ParameterModifier == other.ParameterModifier
        && ParameterTypeHandle.Equals(other.ParameterTypeHandle);

    public override bool Equals(object obj) => obj is SymbolReflectionInfoCacheKey other && Equals(other);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(SymbolKind);
        hashCode.Add(SymbolName);
        hashCode.Add(IsAnonymousKey);
        hashCode.Add(MethodHandle);
        hashCode.Add(DeclaringTypeHandle);
        hashCode.Add(TypHandle);
        hashCode.Add(FieldHandle);
        hashCode.Add(PropertySetMethodHandle);
        hashCode.Add(PropertyGetMethodHandle);
        hashCode.Add(PropertyTypeHandle);
        hashCode.Add(EventAddMethodHandle);
        hashCode.Add(EventRemoveMethodHandle);
        hashCode.Add(ParameterModifier);
        hashCode.Add(ParameterTypeHandle);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(SymbolReflectionInfoCacheKey left, SymbolReflectionInfoCacheKey right) => left.Equals(right);

    public static bool operator !=(SymbolReflectionInfoCacheKey left, SymbolReflectionInfoCacheKey right) => !(left == right);

    private TResult ThrowIfPropertyContextIsInvalidOrReturn<TResult>(HashSet<SymbolKind> allowedSymbolKinds, TResult propertyValue, [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(allowedSymbolKinds, nameof(allowedSymbolKinds), "At least one allowed symbol kind must be provided.");

        if (allowedSymbolKinds.Contains(SymbolKind))
        {
            return propertyValue;
        }

        string allowedKinds = allowedSymbolKinds.JoinToString(kind => $"{typeof(SymbolKind).FullName}.{kind}", ", ");
        return allowedSymbolKinds.Count > 1
            ? throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns any of the following values: {allowedKinds}.")
            : throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns the value '{allowedKinds[0]}'.");
    }
}
