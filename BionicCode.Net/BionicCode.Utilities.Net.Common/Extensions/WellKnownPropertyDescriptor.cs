namespace BionicCode.Utilities.Net
{
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
        public WellKnownPropertyDescriptor(
            PropertyInfo propertyInfo,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle,
            bool isIndexerProperty)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            Type? declaringType = propertyInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

            this.HasPropertyGetAccessor = propertyInfo.CanRead;
            if (this.HasPropertyGetAccessor)
            {
                this.DeclaredPropertyAccessors = PropertyAccessors.Get;
            }

            this.HasPropertySetAccessor = propertyInfo.CanWrite;
            if (this.HasPropertySetAccessor)
            {
                this.DeclaredPropertyAccessors |= PropertyAccessors.Set;
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringInterfaceTypeHandle,
                    nameof(declaringInterfaceTypeHandle),
                    $"Invalid argument '{nameof(declaringInterfaceTypeHandle)}'. The provided declaring interface type handle is not 'NULL' which is not allowed for explicit interface implementations (which is when '{nameof(isExplicitInterfaceImplementation)}' is 'true').");
                ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringInterfaceTypeHandle!.Value);
                Type? declaringInterfaceType = Type.GetTypeFromHandle(declaringInterfaceTypeHandle!.Value);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringInterfaceType,
                    nameof(declaringInterfaceTypeHandle),
                    $"The declaring interface type represented by the argument '{nameof(declaringInterfaceTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringInterfaceType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(declaringInterfaceTypeHandle)}'. The argument '{nameof(declaringInterfaceTypeHandle)}' points to a non-interface type. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsIndexerProperty = isIndexerProperty;
            this.IsAnonymous = false;
            this.PropertyInfo = propertyInfo;
            this.PropertyName = propertyInfo.Name;
        }

        public string PropertyName { get; }
        public PropertyAccessors DeclaredPropertyAccessors { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool IsIndexerProperty { get; }
        public bool IsAnonymous { get; }
        public bool HasPropertyGetAccessor { get; }
        public bool HasPropertySetAccessor { get; }
        public PropertyInfo PropertyInfo { get; }

        public bool Equals(WellKnownPropertyDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IsIndexerProperty == other.IsIndexerProperty
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this.PropertyInfo, other.PropertyInfo)
            && this.HasPropertyGetAccessor == other.HasPropertyGetAccessor
            && this.HasPropertySetAccessor == other.HasPropertySetAccessor
            && this.PropertyName.Equals(other.PropertyName, StringComparison.Ordinal)
            && this.DeclaredPropertyAccessors == other.DeclaredPropertyAccessors;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.IsIndexerProperty);
            hashCode.Add(this.HasPropertyGetAccessor);
            hashCode.Add(this.HasPropertySetAccessor);
            hashCode.Add(this.IsAnonymous);
            hashCode.Add(this.PropertyInfo);
            hashCode.Add(this.PropertyName, StringComparer.Ordinal);
            hashCode.Add(this.DeclaredPropertyAccessors);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownPropertyDescriptor other && Equals(other);
    }
}
