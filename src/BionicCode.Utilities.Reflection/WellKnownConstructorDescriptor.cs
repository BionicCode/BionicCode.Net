namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides information about well-known constructor.
/// </summary>
/// <remarks>The <see cref="WellKnownConstructorDescriptor"/> is used to provide information for well-known constructor symbols, which is when the caller has the direct <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation.
/// <para/>When the caller does not have the direct <see cref="ConstructorInfo"/> representation and only signature information is available, the constructor symbol is considered anonymous.
/// In such case use the <see cref="AnonymousConstructorDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="ConstructorInfo"/> is available to ensure maximum accuracy and performance.
internal readonly struct WellKnownConstructorDescriptor : IEquatable<WellKnownConstructorDescriptor>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WellKnownMethodDescriptor"/> struct for a well-known method.
    /// </summary>
    /// <remarks>The <see cref="WellKnownMethodDescriptor"/> is used to provide information about a well-known method symbol, which is when the caller has a direct representation (a <see cref="MethodInfo"/>) of the method symbol.
    /// <para/>If the method is an explicit interface implementation, then the <paramref name="constructorInfo"/> must represent a <see cref="MethodInfo"/> obtained from an interface type.
    /// </remarks>
    /// <param name="constructorInfo">The <see cref="MethodInfo"/>.</param>
    /// <returns>A new instance of <see cref="WellKnownConstructorDescriptor"/> representing the specified well-known constructor.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
    public WellKnownConstructorDescriptor(ConstructorInfo constructorInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo);

        ConstructorInfo = constructorInfo;
        ConstructorName = ConstructorData.ConstructorDefaultName;
        IsAnonymous = false;
    }

    public string ConstructorName { get; }
    public ConstructorInfo ConstructorInfo { get; }
    public RuntimeMethodHandle ConstructorHandle { get; }
    public bool IsAnonymous { get; }

    public bool Equals(WellKnownConstructorDescriptor other) => IsAnonymous == other.IsAnonymous
        && ConstructorHandle == other.ConstructorHandle
        && ReferenceEquals(ConstructorInfo, other.ConstructorInfo)
        && string.Equals(ConstructorName, other.ConstructorName, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsAnonymous);
        hashCode.Add(ConstructorInfo);
        hashCode.Add(ConstructorHandle);
        hashCode.Add(ConstructorName, StringComparer.Ordinal);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(WellKnownConstructorDescriptor left, WellKnownConstructorDescriptor right) => left.Equals(right);
    public static bool operator !=(WellKnownConstructorDescriptor left, WellKnownConstructorDescriptor right) => !(left == right);

    public override bool Equals(object obj) => obj is WellKnownConstructorDescriptor other && Equals(other);
}
