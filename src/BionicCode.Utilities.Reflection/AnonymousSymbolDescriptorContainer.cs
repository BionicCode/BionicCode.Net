namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

internal readonly struct AnonymousSymbolDescriptorContainer : IEquatable<AnonymousSymbolDescriptorContainer>
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
