namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

/// <summary>
/// A descriptor that provides information about a well-known property or indexer property.
/// </summary>
/// <remarks>The <see cref="WellKnownPropertyDescriptor"/> is used to provide information for well-known property symbols, which is when the caller has the direct <see cref="System.Reflection.PropertyInfo"/> representation.
/// <para/>When the caller does not have the direct <see cref="System.Reflection.PropertyInfo"/> representation and only signature information is available the property symbol is considered anonymous. In such case use the <see cref="AnonymousPropertyDescriptor"/> instead.
/// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="System.Reflection.PropertyInfo"/> is available to ensure maximum accuracy and performance.
/// </remarks>
internal readonly struct WellKnownPropertyDescriptor : IEquatable<WellKnownPropertyDescriptor>
{
    /// <summary>
    /// Creates a new instance of the <see cref="WellKnownPropertyDescriptor"/> struct for a well-known property.
    /// </summary>
    /// <remarks>
    /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="propertyInfo"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
    /// <para/>If the <see cref="System.Reflection.PropertyInfo"/> or the corresponding accessor methods to satisfy <paramref name="explicitGetterImplementationMethod"/> and <paramref name="explicitSetterImplementationMethod"/> are unknow create an anonymous descriptor using the <see cref="AnonymousPropertyDescriptor"/>.
    /// <para/>For best accuracy and performance always use this <see cref="WellKnownPropertyDescriptor"/> when the caller has direct access to the <see cref="System.Reflection.PropertyInfo"/> representation of the property.
    /// </remarks>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> that the descriptor represents. If <paramref name="isExplicitInterfaceImplementation"/> is s et to <see langword="true"/> then the <see cref="PropertyInfo"/> must be obtained from the declaring interface type.</param>
    /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="propertyInfo"/> must be obtained from the declaring interface type.
    /// </param>
    /// <param name="declaringInterfaceTypeHandle">The runtime type handle representing the declaring interface type of the anonymous method.
    /// <para/>Must be provided when the method is an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/>).
    /// <br/>Otherwise, this parameter can be <see langword="null"/> and will be ignored.</param>
    /// <param name="isIndexerProperty">Specifies whether the property is an indexer.</param>
    /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous or well-known property.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when
    /// <list type="bullet">
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="propertyInfo"/> was not obtained from an interface type.</item>
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitGetterImplementationMethod"/> (for a readable property) nor <paramref name="explicitSetterImplementationMethod"/> (for a writeable property) is provided.</item>
    /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitGetterImplementationMethod"/> or <paramref name="explicitSetterImplementationMethod"/> is provided.</item>
    /// </list>
    /// </exception>
    public WellKnownPropertyDescriptor(PropertyInfo propertyInfo)
    {
        ArgumentNullException.ThrowIfNull(propertyInfo);

        IsAnonymous = false;
        PropertyInfo = propertyInfo;
        PropertyName = propertyInfo.Name;
    }

    public string PropertyName { get; }
    public PropertyAccessors DeclaredPropertyAccessors { get; }
    public bool IsExplicitInterfaceImplementation { get; init; }
    public bool IsIndexerProperty { get; }
    public bool IsAnonymous { get; }
    public bool HasPropertyGetAccessor { get; }
    public bool HasPropertySetAccessor { get; }
    public PropertyInfo PropertyInfo { get; }

    public bool Equals(WellKnownPropertyDescriptor other) => IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
        && IsIndexerProperty == other.IsIndexerProperty
        && IsAnonymous == other.IsAnonymous
        && ReferenceEquals(PropertyInfo, other.PropertyInfo)
        && HasPropertyGetAccessor == other.HasPropertyGetAccessor
        && HasPropertySetAccessor == other.HasPropertySetAccessor
        && PropertyName.Equals(other.PropertyName, StringComparison.Ordinal)
        && DeclaredPropertyAccessors == other.DeclaredPropertyAccessors;

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(IsExplicitInterfaceImplementation);
        hashCode.Add(IsIndexerProperty);
        hashCode.Add(HasPropertyGetAccessor);
        hashCode.Add(HasPropertySetAccessor);
        hashCode.Add(IsAnonymous);
        hashCode.Add(PropertyInfo);
        hashCode.Add(PropertyName, StringComparer.Ordinal);
        hashCode.Add(DeclaredPropertyAccessors);

        return hashCode.ToHashCode();
    }

    public static bool operator ==(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right) => left.Equals(right);
    public static bool operator !=(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right) => !(left == right);

    public override bool Equals(object obj) => obj is WellKnownPropertyDescriptor other && Equals(other);
}
