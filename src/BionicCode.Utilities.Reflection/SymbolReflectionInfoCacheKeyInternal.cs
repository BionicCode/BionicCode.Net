namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a unique cache key for a well-known symbol, such as a type, method, property, event, field, constructor, or
/// parameter, used in reflection-based symbol lookup and caching scenarios.
/// </summary>
/// <remarks>A <see cref="SymbolReflectionInfoCacheKeyInternal"/> encapsulates identifying information for a symbol, supporting
/// both well-known symbols (with runtime metadata) and anonymous symbols (identified by signature). This struct is
/// used to efficiently cache and retrieve reflection information for various symbol kinds, including support for
/// explicit interface implementations and anonymous members. Instances are typically created using the provided
/// static factory methods, which enforce correct construction for each symbol kind. <see cref="SymbolReflectionInfoCacheKeyInternal"/> is
/// immutable and can be used as a key in hash-based collections.
/// <para/>A <see cref="AnonymousSymbolDescriptorContainer"/> is used to temporarily store symbol information for anonymous symbols (which is when no direct runtime metadata representation is available).
/// <br/>Such anonymous symbol keys are typically converted to well-known symbol keys by attempting to resolve the runtime metadata object based on the provided signature information before being used for caching purposes.
/// <br/>Therefore anonymous keys are never used directly for caching purposes and must be resolved to well-known keys first.
/// </remarks>
internal readonly struct SymbolReflectionInfoCacheKeyInternal : IEquatable<SymbolReflectionInfoCacheKeyInternal>
{
    /// <summary>
    /// Represents an unknown or unspecified parameter count.
    /// </summary>
    /// <remarks>Use this constant to indicate that the number of parameters is not known or cannot be
    /// determined. This value is typically used in APIs where the parameter count is optional or
    /// variable.</remarks>
    public const int UnknownParameterCountOrPosition = -1;

    // TODO::Throw exceptions based on SymbolKind and  IsAnonymousKey when properties are accessed that are not valid for the specific SymbolKind.

    /// <summary>
    /// Gets the name of the symbol represented by this instance.
    /// </summary>
    /// <value>The name of the symbol, such as the method name, property name, event name, field name, or type name.</value>
    public readonly string SymbolName { get; }

    /// <summary>
    /// Gets the kind of symbol represented by this instance.
    /// </summary>
    /// <value>The kind of symbol, such as type, method, property, event, field, constructor, or parameter.</value>
    public readonly SymbolKind SymbolKind { get; }

    private readonly WellKnownParameterDescriptor _parameterDescriptor;
    public WellKnownParameterDescriptor ParameterDescriptor => SymbolKind is SymbolKind.Parameter
        ? _parameterDescriptor
        : ThrowInvalidPropertyContextException<WellKnownParameterDescriptor>([SymbolKind.Parameter]);

    private readonly WellKnownPropertyDescriptor _propertyDescriptor;
    public WellKnownPropertyDescriptor PropertyDescriptor => SymbolKind is SymbolKind.MemberProperty
        ? _propertyDescriptor
        : ThrowInvalidPropertyContextException<WellKnownPropertyDescriptor>([SymbolKind.MemberProperty]);

    private readonly WellKnownMethodDescriptor _methodDescriptor;
    public WellKnownMethodDescriptor MethodDescriptor => SymbolKind is SymbolKind.MemberMethod
        ? _methodDescriptor
        : ThrowInvalidPropertyContextException<WellKnownMethodDescriptor>([SymbolKind.MemberMethod]);

    private readonly WellKnownConstructorDescriptor _constructorDescriptor;
    public WellKnownConstructorDescriptor ConstructorDescriptor => SymbolKind is SymbolKind.MemberConstructor
        ? _constructorDescriptor
        : ThrowInvalidPropertyContextException<WellKnownConstructorDescriptor>([SymbolKind.MemberConstructor]);

    private readonly WellKnownTypeDescriptor _typeDescriptor;
    public WellKnownTypeDescriptor TypeDescriptor => SymbolKind is SymbolKind.Type
        ? _typeDescriptor
        : ThrowInvalidPropertyContextException<WellKnownTypeDescriptor>([SymbolKind.Type]);

    private readonly WellKnownFieldDescriptor _fieldDescriptor;
    public WellKnownFieldDescriptor FieldDescriptor => SymbolKind is SymbolKind.MemberField
        ? _fieldDescriptor
        : ThrowInvalidPropertyContextException<WellKnownFieldDescriptor>([SymbolKind.MemberField]);

    private readonly WellKnownEventDescriptor _eventDescriptor;
    public WellKnownEventDescriptor EventDescriptor => SymbolKind is SymbolKind.MemberEvent
        ? _eventDescriptor
        : ThrowInvalidPropertyContextException<WellKnownEventDescriptor>([SymbolKind.MemberEvent]);

    private readonly int _hashCode;

    private SymbolReflectionInfoCacheKeyInternal(
        string name,
        SymbolKind symbolKind,
        WellKnownTypeDescriptor typeDescriptor,
        WellKnownParameterDescriptor parameterDescriptor,
        WellKnownMethodDescriptor methodDescriptor,
        WellKnownConstructorDescriptor constructorDescriptor,
        WellKnownPropertyDescriptor propertyDescriptor,
        WellKnownFieldDescriptor fieldDescriptor,
        WellKnownEventDescriptor eventDescriptor)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
        ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));

        SymbolKind = symbolKind;
        _typeDescriptor = typeDescriptor;
        _parameterDescriptor = parameterDescriptor;
        _methodDescriptor = methodDescriptor;
        _constructorDescriptor = constructorDescriptor;
        _propertyDescriptor = propertyDescriptor;
        _fieldDescriptor = fieldDescriptor;
        _eventDescriptor = eventDescriptor;
        SymbolName = name;

        _hashCode = ComputeHashCode();
    }

    /// <summary>
    /// Creates a cache key for an event symbol.
    /// </summary>
    /// <param name="eventDescriptor">The event descriptor.</param>
    /// <returns>The unique cache key for the event symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventDescriptor"/> is <see langword="null"/>.</exception>
    internal static SymbolReflectionInfoCacheKeyInternal CreateForEvent(WellKnownEventDescriptor eventDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(eventDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            eventDescriptor.EventInfo.Name,
            SymbolKind.MemberEvent,
            default,
            default,
            default,
            default,
            default,
            default,
            default);
    }

    /// <summary>
    /// Creates a cache key for a well-known property symbol.
    /// </summary>
    /// <param name="propertyDescriptor">The <see cref="Net.WellKnownPropertyDescriptor"/> that describes a well-known property (which is where the <see cref="PropertyInfo"/> is available) or an anonymous property (which when only signature information is available).</param>
    /// <remarks>This method creates a unique cache key for well-known or anonymous property symbols, including indexer properties.
    /// <para/>For maximum performance and zero ambiguity, always prefer to create property cache keys using well-known <see cref="PropertyInfo"/> instances via the <see cref="Net.WellKnownPropertyDescriptor"/>.</remarks>
    /// <returns>The unique cache key for the well-known property symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when
    /// <list type="bullet">
    /// <item><paramref name="propertyDescriptor"/> or its declaring type is <see langword="default"/>.</item>
    /// </list>
    /// </exception>
    internal static SymbolReflectionInfoCacheKeyInternal CreateForProperty(WellKnownPropertyDescriptor propertyDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            propertyDescriptor.PropertyInfo.Name,
            SymbolKind.MemberProperty,
            default,
            default,
            default,
            default,
            propertyDescriptor,
            default,
            default);
    }

    /// <summary>
    /// Creates a cache key for a well-known method symbol.
    /// </summary>
    /// <param name="methodDescriptor">The <see cref="Net.WellKnownMethodDescriptor"/> that describes a well-known method (which is where the <see cref="MethodInfo"/> is available).</param>
    /// <remarks>This method creates a unique cache key for well-known or anonymous method symbols, including regular methods, property accessors, and event accessors.
    /// <para/>For maximum performance and zero ambiguity, always prefer to create method cache keys using well-known <see cref="MethodInfo"/> instances via the <see cref="Net.WellKnownMethodDescriptor"/>.</remarks>
    /// <returns>The unique cache key for the well-known method symbol.</returns>
    internal static SymbolReflectionInfoCacheKeyInternal CreateForMethod(WellKnownMethodDescriptor methodDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            methodDescriptor.MethodName,
            SymbolKind.MemberMethod,
            default,
            default,
            methodDescriptor,
            default,
            default,
            default,
            default);
    }

    internal static SymbolReflectionInfoCacheKeyInternal CreateForType(WellKnownTypeDescriptor typeDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(typeDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            typeDescriptor.TypeName,
            SymbolKind.Type,
            typeDescriptor,
            default,
            default,
            default,
            default,
            default,
            default);
    }

    internal static SymbolReflectionInfoCacheKeyInternal CreateForField(WellKnownFieldDescriptor fieldDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(fieldDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            fieldDescriptor.FieldName,
            SymbolKind.MemberField,
            default,
            default,
            default,
            default,
            default,
            fieldDescriptor,
            default);
    }

    /// <summary>
    /// Creates a new cache key for a well-known constructor using the specified <see cref="WellKnownMethodDescriptor"/>.
    /// </summary>
    /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.
    ///<para/>For maximum performance and zero ambiguity, always prefer to create constructor cache keys using well-known <see cref="ConstructorInfo"/> instances via the <see cref="Net.WellKnownMethodDescriptor"/>.</remarks>
    /// <param name="constructorDescriptor"></param>
    /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKeyInternal"/> representing the specified well-known constructor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorDescriptor"/> is <see langword="default"/>.</exception>
    internal static SymbolReflectionInfoCacheKeyInternal CreateForConstructor(WellKnownConstructorDescriptor constructorDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(constructorDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            string.Empty,
            SymbolKind.MemberConstructor,
            default,
            default,
            default,
            constructorDescriptor,
            default,
            default,
            default);
    }

    internal static SymbolReflectionInfoCacheKeyInternal CreateForWellKnownParameter(WellKnownParameterDescriptor parameterDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterDescriptor);

        return new SymbolReflectionInfoCacheKeyInternal(
            parameterDescriptor.ParameterName,
            SymbolKind.Parameter,
            default,
            parameterDescriptor,
            default,
            default,
            default,
            default,
            default);
    }

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            if (_hashCode != 0)
            {
                return _hashCode;
            }

            var hashCode = new HashCode();
            hashCode.Add(SymbolName);
            hashCode.Add(SymbolKind);
            hashCode.Add(_parameterDescriptor);
            hashCode.Add(_methodDescriptor);
            hashCode.Add(_constructorDescriptor);
            hashCode.Add(_propertyDescriptor);
            hashCode.Add(_fieldDescriptor);
            hashCode.Add(_eventDescriptor);
            hashCode.Add(_typeDescriptor);

            return hashCode.ToHashCode();
        }
    }

    public override bool Equals(object obj) => obj is SymbolReflectionInfoCacheKeyInternal other && Equals(other);

    public bool Equals(SymbolReflectionInfoCacheKeyInternal other) => SymbolName == other.SymbolName
        && SymbolKind == other.SymbolKind
        && _parameterDescriptor == other._parameterDescriptor
        && _methodDescriptor == other._methodDescriptor
        && _constructorDescriptor == other._constructorDescriptor
        && _propertyDescriptor == other._propertyDescriptor
        && _fieldDescriptor == other._fieldDescriptor
        && _eventDescriptor == other._eventDescriptor
        && _typeDescriptor == other._typeDescriptor;

    public static bool operator ==(SymbolReflectionInfoCacheKeyInternal left, SymbolReflectionInfoCacheKeyInternal right) => left.Equals(right);
    public static bool operator !=(SymbolReflectionInfoCacheKeyInternal left, SymbolReflectionInfoCacheKeyInternal right) => !(left == right);

    [DoesNotReturn]
    private TResult ThrowInvalidPropertyContextException<TResult>(ReadOnlySpan<SymbolKind> allowedSymbolKinds, [CallerMemberName] string? propertyName = null)
    {
        ArgumentExceptionAdvanced.ThrowIfTrue(allowedSymbolKinds.IsEmpty, nameof(allowedSymbolKinds), "At least one allowed symbol kind must be provided.");

        string allowedKinds = allowedSymbolKinds.JoinToString(kind => $"{typeof(SymbolKind).FullName}.{kind}", ", ");
        return allowedSymbolKinds.Length > 1
            ? throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns any of the following values: {allowedKinds}.")
            : throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns the value '{allowedKinds[0]}'.");
    }
}

internal readonly partial struct AnonymousSymbolDescriptorContainer : IEquatable<AnonymousSymbolDescriptorContainer>
{
    /// <summary>
    /// Represents an unknown or unspecified parameter count.
    /// </summary>
    /// <remarks>Use this constant to indicate that the number of parameters is not known or cannot be
    /// determined. This value is typically used in APIs where the parameter count is optional or
    /// variable.</remarks>
    public const int UnknownParameterCountOrPosition = SymbolReflectionInfoCacheKeyInternal.UnknownParameterCountOrPosition;

    /// <summary>
    /// Gets the name of the symbol represented by this instance.
    /// </summary>
    /// <value>The name of the symbol, such as the method name, property name, event name, field name, or type name.</value>
    public readonly string SymbolName { get; }

    /// <summary>
    /// Gets the kind of symbol represented by this instance.
    /// </summary>
    /// <value>The kind of symbol, such as type, method, property, event, field, constructor, or parameter.</value>
    public readonly SymbolKind SymbolKind { get; }

    private readonly AnonymousParameterDescriptor _parameterDescriptor;
    public AnonymousParameterDescriptor ParameterDescriptor => SymbolKind is SymbolKind.Parameter
        ? _parameterDescriptor
        : ThrowInvalidPropertyContextException<AnonymousParameterDescriptor>([SymbolKind.Parameter]);

    private readonly AnonymousPropertyDescriptor _propertyDescriptor;
    public AnonymousPropertyDescriptor PropertyDescriptor => SymbolKind is SymbolKind.MemberProperty
        ? _propertyDescriptor
        : ThrowInvalidPropertyContextException<AnonymousPropertyDescriptor>([SymbolKind.MemberProperty]);

    private readonly AnonymousMethodDescriptor _methodDescriptor;
    public AnonymousMethodDescriptor MethodDescriptor => SymbolKind is SymbolKind.MemberMethod
        ? _methodDescriptor
        : ThrowInvalidPropertyContextException<AnonymousMethodDescriptor>([SymbolKind.MemberMethod]);

    private readonly AnonymousConstructorDescriptor _constructorDescriptor;
    public AnonymousConstructorDescriptor ConstructorDescriptor => SymbolKind is SymbolKind.MemberConstructor
        ? _constructorDescriptor
        : ThrowInvalidPropertyContextException<AnonymousConstructorDescriptor>([SymbolKind.MemberConstructor]);

    private readonly AnonymousFieldDescriptor _fieldDescriptor;
    public AnonymousFieldDescriptor FieldDescriptor => SymbolKind is SymbolKind.MemberField
        ? _fieldDescriptor
        : ThrowInvalidPropertyContextException<AnonymousFieldDescriptor>([SymbolKind.MemberField]);

    private readonly AnonymousEventDescriptor _eventDescriptor;
    public AnonymousEventDescriptor EventDescriptor => SymbolKind is SymbolKind.MemberEvent
        ? _eventDescriptor
        : ThrowInvalidPropertyContextException<AnonymousEventDescriptor>([SymbolKind.MemberEvent]);

    private readonly bool _isExplicitInterfaceImplementation;
    public bool IsExplicitInterfaceImplementation => SymbolKind is SymbolKind.MemberProperty or SymbolKind.MemberEvent or SymbolKind.MemberMethod
        ? _isExplicitInterfaceImplementation
        : ThrowInvalidPropertyContextException<bool>([SymbolKind.MemberProperty, SymbolKind.MemberEvent, SymbolKind.MemberMethod]);

    private readonly int _hashCode;

    private AnonymousSymbolDescriptorContainer(
        string name,
        SymbolKind symbolKind,
        AnonymousParameterDescriptor parameterDescriptor,
        AnonymousMethodDescriptor methodDescriptor,
        AnonymousConstructorDescriptor constructorDescriptor,
        AnonymousPropertyDescriptor propertyDescriptor,
        AnonymousFieldDescriptor fieldDescriptor,
        AnonymousEventDescriptor eventDescriptor)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
        ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));

        SymbolKind = symbolKind;
        _parameterDescriptor = parameterDescriptor;
        _constructorDescriptor = constructorDescriptor;
        _methodDescriptor = methodDescriptor;
        _propertyDescriptor = propertyDescriptor;
        _fieldDescriptor = fieldDescriptor;
        _eventDescriptor = eventDescriptor;
        SymbolName = name;

        _hashCode = ComputeHashCode();
    }

    /// <summary>
    /// Creates a cache key for an event symbol.
    /// </summary>
    /// <param name="eventDescriptor">The event descriptor.</param>
    /// <returns>The unique cache key for the event symbol.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventDescriptor"/> is <see langword="null"/>.</exception>
    public static AnonymousSymbolDescriptorContainer CreateForEvent(AnonymousEventDescriptor eventDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(eventDescriptor);

        return new AnonymousSymbolDescriptorContainer(
            eventDescriptor.EventName,
            SymbolKind.MemberEvent,
            default,
            default,
            default,
            default,
            default,
            eventDescriptor);
    }

    public static AnonymousSymbolDescriptorContainer CreateForProperty(AnonymousPropertyDescriptor propertyDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

        // Anonymous keys will never be used to cache items.
        // Instead they will always be converted to well-known keys by attempting to identify the runtime metadata object
        // based on the provided information. Therefore we can use an empty AssemblyId here.
        return new AnonymousSymbolDescriptorContainer(
            propertyDescriptor.PropertyName,
            SymbolKind.MemberProperty,
            default,
            default,
            default,
            propertyDescriptor,
            default,
            default);
    }

    /// <summary>
    /// Creates a cache key for an anonymous method symbol.
    /// </summary>
    /// <param name="methodDescriptor">The <see cref="Net.AnonymousMethodDescriptor"/> that describes an anonymous method (which is where the <see cref="MethodInfo"/> is not available and instead only the signature information is available).</param>
    /// <remarks>This method creates a unique cache key for well-known or anonymous method symbols, including regular methods, property accessors, and event accessors.
    /// <para/>For maximum performance and zero ambiguity, always prefer to create method cache keys using well-known <see cref="MethodInfo"/> instances via the <see cref="Net.WellKnownMethodDescriptor"/>.</remarks>
    /// <returns>The unique cache key for the well-known method symbol.</returns>
    public static AnonymousSymbolDescriptorContainer CreateForMethod(AnonymousMethodDescriptor methodDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

        return new AnonymousSymbolDescriptorContainer(
            methodDescriptor.MethodName,
            SymbolKind.MemberMethod,
            default,
            methodDescriptor,
            default,
            default,
            default,
            default);
    }

    /// <summary>
    /// Creates a new cache key for an anonymous field using the specified <see cref="AnonymousFieldDescriptor"/>.
    /// </summary>
    /// <param name="fieldDescriptor">The <see cref="Net.AnonymousFieldDescriptor"/> that describes an anonymous field (which is where the <see cref="FieldInfo"/> is not available and instead only the signature information is available).</param>
    /// <remarks>This method is used to create a unique cache key for field symbols of which the caller does not have a direct representation <see cref="FieldInfo"/> and instead only signature information is available.</remarks>
    /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKeyInternal"/> representing the specified anonymous field.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldDescriptor"/> is <see langword="default"/>.</exception>
    public static AnonymousSymbolDescriptorContainer CreateForField(AnonymousFieldDescriptor fieldDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(fieldDescriptor);

        return new AnonymousSymbolDescriptorContainer(
            fieldDescriptor.FieldName,
            SymbolKind.MemberField,
            default,
            default,
            default,
            default,
            fieldDescriptor,
            default);
    }

    /// <summary>
    /// Creates a new cache key for an anonymous constructor using the specified <see cref="AnonymousMethodDescriptor"/>.
    /// </summary>
    /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.
    ///<para/>For maximum performance and zero ambiguity, always prefer to create constructor cache keys using well-known <see cref="ConstructorInfo"/> instances via the <see cref="Net.WellKnownMethodDescriptor"/>.</remarks>
    /// <param name="constructorDescriptor"></param>
    /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKeyInternal"/> representing the specified anonymous constructor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorDescriptor"/> is <see langword="default"/>.</exception>
    public static AnonymousSymbolDescriptorContainer CreateForConstructor(AnonymousConstructorDescriptor constructorDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(constructorDescriptor);

        return new AnonymousSymbolDescriptorContainer(
            string.Empty,
            SymbolKind.MemberConstructor,
            default,
            default,
            constructorDescriptor,
            default,
            default,
            default);
    }

    public static AnonymousSymbolDescriptorContainer CreateForParameter(AnonymousParameterDescriptor parameterDescriptor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterDescriptor);

        return new AnonymousSymbolDescriptorContainer(
            parameterDescriptor.ParameterName,
            SymbolKind.Parameter,
            parameterDescriptor,
            default,
            default,
            default,
            default,
            default);
    }

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            if (_hashCode != 0)
            {
                return _hashCode;
            }

            var hashCode = new HashCode();
            hashCode.Add(SymbolName);
            hashCode.Add(SymbolKind);
            hashCode.Add(_parameterDescriptor);
            hashCode.Add(_methodDescriptor);
            hashCode.Add(_constructorDescriptor);
            hashCode.Add(_propertyDescriptor);
            hashCode.Add(_fieldDescriptor);
            hashCode.Add(_eventDescriptor);
            hashCode.Add(_isExplicitInterfaceImplementation);

            return hashCode.ToHashCode();
        }
    }

    public override bool Equals(object obj) => obj is AnonymousSymbolDescriptorContainer other && Equals(other);

    public bool Equals(AnonymousSymbolDescriptorContainer other) => SymbolName == other.SymbolName
        && SymbolKind == other.SymbolKind
        && _parameterDescriptor == other._parameterDescriptor
        && _methodDescriptor == other._methodDescriptor
        && _constructorDescriptor == other._constructorDescriptor
        && _propertyDescriptor == other._propertyDescriptor
        && _fieldDescriptor == other._fieldDescriptor
        && _eventDescriptor == other._eventDescriptor
        && _isExplicitInterfaceImplementation == other._isExplicitInterfaceImplementation;

    public static bool operator ==(AnonymousSymbolDescriptorContainer left, AnonymousSymbolDescriptorContainer right) => left.Equals(right);
    public static bool operator !=(AnonymousSymbolDescriptorContainer left, AnonymousSymbolDescriptorContainer right) => !(left == right);

    [DoesNotReturn]
    private TResult ThrowInvalidPropertyContextException<TResult>(ReadOnlySpan<SymbolKind> allowedSymbolKinds, [CallerMemberName] string? propertyName = null)
    {
        ArgumentExceptionAdvanced.ThrowIfTrue(allowedSymbolKinds.IsEmpty, nameof(allowedSymbolKinds), "At least one allowed symbol kind must be provided.");

        string allowedKinds = allowedSymbolKinds.JoinToString(kind => $"{typeof(SymbolKind).FullName}.{kind}", ", ");
        return allowedSymbolKinds.Length > 1
            ? throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns any of the following values: {allowedKinds}.")
            : throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(SymbolKind)}' returns the value '{allowedKinds[0]}'.");
    }
}
