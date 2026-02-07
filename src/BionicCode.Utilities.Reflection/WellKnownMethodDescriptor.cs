namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides information about well-known method.
/// </summary>
/// <remarks>The <see cref="WellKnownMethodDescriptor"/> is used to provide information for well-known method symbols, which is when the caller has the direct <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation.
/// <para/>When the caller does not have the direct <see cref="MethodInfo"/> representation and only signature information is available the method symbol is considered anonymous.
/// In such case use the <see cref="AnonymousMethodDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="MethodInfo"/> is available to ensure maximum accuracy and performance.
internal readonly struct WellKnownMethodDescriptor : IEquatable<WellKnownMethodDescriptor>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WellKnownMethodDescriptor"/> struct for a well-known method.
    /// </summary>
    /// <remarks>The <see cref="WellKnownMethodDescriptor"/> is used to provide information about a well-known method symbol, which is when the caller has a direct representation (a <see cref="MethodInfo"/>) of the method symbol.
    /// </remarks>
    /// <param name="methodInfo">The <see cref="MethodInfo"/>.</param>
    /// <returns>A new instance of <see cref="WellKnownMethodDescriptor"/> representing the specified well-known method.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
    public WellKnownMethodDescriptor(MethodInfo methodInfo)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);

        MethodInfo = methodInfo;
        MethodName = methodInfo.Name;
        MethodHandle = methodInfo.MethodHandle;
        IsAnonymous = false;
    }

    public MethodInfo MethodInfo { get; }

    /// <summary>
    /// Returns the runtime method handle of the well-known method.
    /// </summary>
    /// <value>If the method is an explicit interface implementation (<see cref="IsExplicitInterfaceImplementation"/> is <see langword="true"/>), this property returns the runtime method handle of the <see cref="MethodInfo"/> that maps to the declaring interface type; otherwise, it returns handle to the implementation <see cref="MethodInfo"/>.</value>
    public RuntimeMethodHandle MethodHandle { get; }
    public string MethodName { get; }
    public bool IsAnonymous { get; }

    public bool Equals(WellKnownMethodDescriptor other)
        => MethodHandle == other.MethodHandle
        && MethodName.Equals(other.MethodName, StringComparison.Ordinal)
        && IsAnonymous == other.IsAnonymous;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsAnonymous);
        hashCode.Add(MethodHandle);
        hashCode.Add(MethodName, StringComparer.Ordinal);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(WellKnownMethodDescriptor left, WellKnownMethodDescriptor right)
        => left.Equals(right);
    public static bool operator !=(WellKnownMethodDescriptor left, WellKnownMethodDescriptor right)
        => !(left == right);

    public override bool Equals(object obj)
        => obj is WellKnownMethodDescriptor other && Equals(other);
}
