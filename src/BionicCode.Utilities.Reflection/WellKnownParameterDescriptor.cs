namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides the specified parameter information for a well-known parameter symbol.
/// </summary>
/// <remarks>The <see cref="WellKnownParameterDescriptor"/> is used to provide information for well-known parameter symbols, which is when the caller has the direct <see cref="ParameterInfo"/> representation.
/// <para/>When the caller does not have the direct <see cref="ParameterInfo"/> representation and the parameter is anonymous, use the <see cref="ParameterDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="ParameterInfo"/> is available to ensure maximum accuracy and performance.
/// </remarks>
internal readonly struct WellKnownParameterDescriptor : IEquatable<WellKnownParameterDescriptor>
{
    /// <summary>
    /// Constructs a descriptor that provides the specified parameter information for a well-known parameter.
    /// </summary>
    /// <param name="parameterInfo">The <see cref="ParameterInfo"/> representation of the parameter.</param>
    public WellKnownParameterDescriptor(ParameterInfo parameterInfo)
    {
        ArgumentNullException.ThrowIfNull(parameterInfo);

        ParameterInfo = parameterInfo;
        ParameterName = ParameterInfo.Name ?? string.Empty;
    }

    public static bool IsAnonymous => false;

    public string ParameterName { get; }

    public ParameterInfo ParameterInfo { get; }

    public bool Equals(WellKnownParameterDescriptor other) => ReferenceEquals(ParameterInfo, other.ParameterInfo)
        && IsAnonymous == IsAnonymous
        && ParameterName == other.ParameterName;

    public override int GetHashCode() => HashCode.Combine(ParameterInfo, IsAnonymous, ParameterName);

    public static bool operator ==(WellKnownParameterDescriptor left, WellKnownParameterDescriptor right) => left.Equals(right);
    public static bool operator !=(WellKnownParameterDescriptor left, WellKnownParameterDescriptor right) => !left.Equals(right);

    public override bool Equals(object? obj) => obj is WellKnownParameterDescriptor other && Equals(other);
}
