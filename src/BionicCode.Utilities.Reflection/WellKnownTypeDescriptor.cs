namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides information about well-known types.
/// </summary>
/// <remarks>The <see cref="WellKnownTypeDescriptor"/> is used to provide information for well-known type symbols, which is when the caller has the direct <see cref="Type"/> representation.
internal readonly struct WellKnownTypeDescriptor : IEquatable<WellKnownTypeDescriptor>
{

    /// <summary>
    /// Initializes a new instance of the <see cref="WellKnownTypeDescriptor"/> struct for a well-known type.
    /// </summary>
    /// <remarks>The <see cref="WellKnownTypeDescriptor"/> is used to provide information about a well-known type symbol, which is when the caller has a direct representation (a <see cref="Type"/>) of the type symbol.
    /// </remarks>
    /// <param name="type">The <see cref="MethodInfo"/>.</param>
    /// <returns>A new instance of <see cref="WellKnownTypeDescriptor"/> representing the specified well-known type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="type"/> is <see langword="null"/>.</exception>
    public WellKnownTypeDescriptor(Type type)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(type);

        Type = type;
        TypeHandle = type.TypeHandle;
        TypeName = type.FullName ?? type.Name;
        TypeNamespace = type.Namespace ?? string.Empty;
        IsAnonymous = false;
    }

    public Type Type { get; }
    public RuntimeTypeHandle TypeHandle { get; }
    public string TypeName { get; }
    public string TypeNamespace { get; }
    public bool IsAnonymous { get; }

    public bool Equals(WellKnownTypeDescriptor other) => IsAnonymous == other.IsAnonymous
        && Type == other.Type
        && TypeHandle.Equals(other.TypeHandle)
        && TypeName.Equals(other.TypeName, StringComparison.Ordinal)
        && TypeNamespace.Equals(other.TypeNamespace, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsAnonymous);
        hashCode.Add(Type);
        hashCode.Add(TypeHandle);
        hashCode.Add(TypeName, StringComparer.Ordinal);
        hashCode.Add(TypeNamespace, StringComparer.Ordinal);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(WellKnownTypeDescriptor left, WellKnownTypeDescriptor right)
        => left.Equals(right);
    public static bool operator !=(WellKnownTypeDescriptor left, WellKnownTypeDescriptor right)
        => !(left == right);

    public override bool Equals(object obj)
        => obj is WellKnownTypeDescriptor other && Equals(other);
}
