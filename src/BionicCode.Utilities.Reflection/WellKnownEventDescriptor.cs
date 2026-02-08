namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides information about a well-known event.
/// </summary>
/// <remarks>The <see cref="WellKnownEventDescriptor"/> is used to provide information for well-known event symbols, which is when the caller has the direct <see cref="System.Reflection.EventInfo"/> representation.
/// <para/>When the caller does not have the direct <see cref="System.Reflection.EventInfo"/> representation and only signature information is available the event symbol is considered anonymous. In such case use the <see cref="AnonymousEventDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="System.Reflection.EventInfo"/> is available to ensure maximum accuracy and performance.
/// </remarks>
internal readonly struct WellKnownEventDescriptor : IEquatable<WellKnownEventDescriptor>
{
    /// <summary>
    /// Creates a new instance of the <see cref="WellKnownEventDescriptor"/> struct for a well-known event.
    /// </summary>
    /// <remarks>
    /// If the event is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="eventInfo"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
    /// <para/>If the <see cref="System.Reflection.EventInfo"/> or the corresponding accessor methods to satisfy <paramref name="explicitAddImplementationMethod"/> and <paramref name="explicitRemoveImplementationMethod"/> are unknown create an anonymous descriptor using the <see cref="AnonymousEventDescriptor"/>.
    /// <para/>For best accuracy and performance always use this <see cref="WellKnownEventDescriptor"/> when the caller has direct access to the <see cref="System.Reflection.EventInfo"/> representation of the event.
    /// </remarks>
    /// <param name="eventInfo">The <see cref="System.Reflection.EventInfo"/> that the descriptor represents. If <paramref name="isExplicitInterfaceImplementation"/> is set to <see langword="true"/> then the <see cref="System.Reflection.EventInfo"/> must be obtained from the declaring interface type.</param>
    /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the event is an explicit interface implementation; otherwise, <see langword="false"/>.
    /// <para/>If set to <see langword="true"/>, the <paramref name="eventInfo"/> must be obtained from the declaring interface type.
    /// </param>
    /// <param name="explicitAddImplementationMethod"></param>
    /// <param name="explicitRemoveImplementationMethod"></param>
    /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous or well-known property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when
    /// <list type="bullet">
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="eventInfo"/> was not obtained from an interface type.</item>
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitAddImplementationMethod"/> (for a readable property) nor <paramref name="explicitRemoveImplementationMethod"/> (for a writeable property) is provided.</item>
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitAddImplementationMethod"/> or <paramref name="explicitRemoveImplementationMethod"/> is provided.</item>
    /// </list>
    /// </exception>
    public WellKnownEventDescriptor(EventInfo eventInfo)
    {
        ArgumentNullException.ThrowIfNull(eventInfo);

        IsAnonymous = false;
        EventInfo = eventInfo;
        EventName = eventInfo.Name;
    }

    public bool IsAnonymous { get; }

    public EventInfo EventInfo { get; }
    public string EventName { get; }

    public bool Equals(WellKnownEventDescriptor other)
        => IsAnonymous == other.IsAnonymous
        && ReferenceEquals(EventInfo, other.EventInfo)
        && EventName.Equals(other.EventName, StringComparison.Ordinal);

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsAnonymous);
        hashCode.Add(EventInfo);
        hashCode.Add(EventName, StringComparer.Ordinal);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(WellKnownEventDescriptor left, WellKnownEventDescriptor right)
        => left.Equals(right);
    public static bool operator !=(WellKnownEventDescriptor left, WellKnownEventDescriptor right)
        => !(left == right);

    public override bool Equals(object obj)
        => obj is WellKnownEventDescriptor other && Equals(other);
}
