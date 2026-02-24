namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides the specified parameter information for an anonymous parameter symbol.
/// </summary>
/// <remarks>The <see cref="ParameterDescriptor"/> is used to provide information for anonymous parameter symbols, which is when the caller does not have the direct <see cref="ParameterInfo"/> representation.
/// <para/>When the caller has the direct <see cref="ParameterInfo"/> representation and the method is well-known, use the <see cref="WellKnownParameterDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="ParameterInfo"/> is available to ensure maximum accuracy and performance.
/// </remarks>
public readonly struct ParameterDescriptor : IEquatable<ParameterDescriptor>
{
    /// <summary>
    /// Represents an unknown or unspecified parameter count.
    /// </summary>
    /// <remarks>Use this constant to indicate that the number of parameters is not known or cannot be
    /// determined. This value is typically used in APIs where the parameter count is optional or
    /// variable.</remarks>
    public const int UnknownParameterCountOrPosition = -1;

    /// <summary>
    /// Constructs a descriptor that provides the specified parameter information for an anonymous parameter.
    /// </summary>
    /// <param name="parameterTypeHandle">The runtime type handle representing the type of the anonymous parameter.<para/>
    /// <param name="parameterName">Optional. The name of the parameter.
    /// <para/>While this argument is optional, it is strongly recommended to provide it to disambiguate.<para/>
    /// <param name="parameterPosition">The zero based index of the parameter.<para/>
    /// <param name="parameterModifier">Optional. The modifier of the parameter. 
    /// <para/>While this argument is optional, it is strongly recommended to provide it to disambiguate.<para/>
    public ParameterDescriptor(
        int parameterPosition,
        RuntimeTypeHandle parameterTypeHandle,
        string? parameterName = null,
        ParameterModifier parameterModifier = ParameterModifier.Undefined,
        ParameterizedSymbolKind parameterizedSymbolKind = ParameterizedSymbolKind.Undefined)
    {
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNegative(parameterPosition, nameof(parameterPosition), $"The argument '{nameof(parameterPosition)}' must be greater than or equal to zero.");
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterModifier>(parameterModifier);
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterizedSymbolKind>(parameterizedSymbolKind);
        ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterTypeHandle);

        ParameterName = parameterName ?? string.Empty;
        ParameterPosition = parameterPosition;
        ParameterModifier = parameterModifier;
        ParameterizedSymbolKind = parameterizedSymbolKind;
        ParameterTypeHandle = parameterTypeHandle;
        IsAnonymous = true;
    }

    public bool HasParameterName => !string.IsNullOrWhiteSpace(ParameterName);
    public bool HasParameterModifier => ParameterModifier != ParameterModifier.Undefined;
    public bool HasParameterizedSymbolKind => ParameterizedSymbolKind != ParameterizedSymbolKind.Undefined;
    public bool IsAnonymous { get; }
    public string ParameterName { get; }
    public int ParameterPosition { get; }
    public ParameterModifier ParameterModifier { get; }
    public ParameterizedSymbolKind ParameterizedSymbolKind { get; }
    public RuntimeTypeHandle ParameterTypeHandle { get; }
    public bool IsAmbiguityExpected => !HasParameterName || !HasParameterModifier || !HasParameterizedSymbolKind;

    public bool Equals(ParameterDescriptor other) => ParameterName.Equals(other.ParameterName, StringComparison.Ordinal)
        && ParameterPosition == other.ParameterPosition
        && ParameterModifier == other.ParameterModifier
        && ParameterTypeHandle.Equals(other.ParameterTypeHandle)
        && IsAnonymous == other.IsAnonymous
        && ParameterizedSymbolKind == other.ParameterizedSymbolKind;

    public override int GetHashCode() => HashCode.Combine(
        ParameterName,
        ParameterPosition,
        ParameterModifier,
        ParameterTypeHandle,
        IsAnonymous,
        ParameterizedSymbolKind);

    public static bool operator ==(ParameterDescriptor left, ParameterDescriptor right) => left.Equals(right);
    public static bool operator !=(ParameterDescriptor left, ParameterDescriptor right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is ParameterDescriptor other && Equals(other);
}
