namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides the specified parameter information for an anonymous parameter symbol.
/// </summary>
/// <remarks>The <see cref="AnonymousParameterDescriptor"/> is used to provide information for anonymous parameter symbols, which is when the caller does not have the direct <see cref="ParameterInfo"/> representation.
/// <para/>When the caller has the direct <see cref="ParameterInfo"/> representation and the method is well-known, use the <see cref="WellKnownParameterDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="ParameterInfo"/> is available to ensure maximum accuracy and performance.
/// </remarks>
internal readonly struct AnonymousParameterDescriptor : IEquatable<AnonymousParameterDescriptor>
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
    /// <param name="parameterTypeHandle">Conditionally optional. The runtime type handle representing the type of the anonymous parameter.<para/>
    /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterModifier"/> AND <paramref name="parameterPosition"/>.</param>
    /// <param name="parameterName">Conditionally optional. The name of the anonymous parameter.<para/>
    /// Must be provided if all of the following arguments are missing: <paramref name="parameterTypeHandle"/> AND <paramref name="parameterModifier"/> AND <paramref name="parameterPosition"/>.</param>
    /// <param name="parameterPosition">Conditionally optional.The index of the parameter.<para/>
    /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterTypeHandle"/> AND <paramref name="parameterModifier"/>.</param>
    /// <param name="parameterModifier">Conditionally optional. The modifier of the parameter.<para/>
    /// Must be provided if all of the following arguments are missing: <paramref name="parameterName"/> AND <paramref name="parameterTypeHandle"/> AND <paramref name="parameterPosition"/>.</param>
    public AnonymousParameterDescriptor(
        string? parameterName = null,
        int parameterPosition = UnknownParameterCountOrPosition,
        ParameterModifier parameterModifier = ParameterModifier.Undefined,
        RuntimeTypeHandle? parameterTypeHandle = null)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterModifier>(parameterModifier);
        if (parameterTypeHandle.Equals(default)
            && string.IsNullOrWhiteSpace(parameterName)
            && parameterModifier == ParameterModifier.Undefined
            && parameterPosition == UnknownParameterCountOrPosition)
        {
            throw new ArgumentException($"At least one of the following arguments must be provided to avoid ambiguity when using the created key for lookups: '{nameof(parameterTypeHandle)}', '{nameof(parameterName)}', '{nameof(parameterModifier)}', '{nameof(parameterPosition)}'.");
        }

        ParameterName = parameterName ?? string.Empty;
        ParameterPosition = parameterPosition;
        ParameterModifier = parameterModifier;
        ParameterTypeHandle = parameterTypeHandle ?? default;
        IsAnonymous = true;
    }

    public bool HasParameterName => !string.IsNullOrWhiteSpace(ParameterName);

    public bool HasParameterPosition => ParameterPosition > UnknownParameterCountOrPosition;

    public bool HasParameterModifier => ParameterModifier != ParameterModifier.Undefined;

    public bool HasParameterTypeHandle => !ParameterTypeHandle.Equals(default);

    public bool IsAnonymous { get; }

    public string ParameterName { get; }
    public int ParameterPosition { get; }
    public ParameterModifier ParameterModifier { get; }
    public RuntimeTypeHandle ParameterTypeHandle { get; }

    public bool Equals(AnonymousParameterDescriptor other) => ParameterName.Equals(other.ParameterName, StringComparison.Ordinal)
        && ParameterPosition == other.ParameterPosition
        && ParameterModifier == other.ParameterModifier
        && ParameterTypeHandle.Equals(other.ParameterTypeHandle)
        && IsAnonymous == other.IsAnonymous;

    public override int GetHashCode() => HashCode.Combine(
        ParameterName,
        ParameterPosition,
        ParameterModifier,
        ParameterTypeHandle,
        IsAnonymous);

    public static bool operator ==(AnonymousParameterDescriptor left, AnonymousParameterDescriptor right) => left.Equals(right);
    public static bool operator !=(AnonymousParameterDescriptor left, AnonymousParameterDescriptor right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is AnonymousParameterDescriptor other && Equals(other);
}
