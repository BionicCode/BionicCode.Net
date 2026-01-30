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
        /// <param name="explicitGetterImplementationMethod"></param>
        /// <param name="explicitSetterImplementationMethod"></param>
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
            RuntimeTypeHandle? declaringInterfaceTypeHandle
            MethodInfo? explicitGetterImplementationMethod,
            MethodInfo? explicitSetterImplementationMethod,
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
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(propertyInfo)}.{nameof(propertyInfo.DeclaringType)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");

                if (this.HasPropertyGetAccessor && explicitGetterImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument combination for the explicit interface implementation. The argument '{nameof(propertyInfo)}' declares a get accessor, but the '{nameof(explicitGetterImplementationMethod)}' is null. Or vice versa. Reason: Both arguments must be provided to describe a getter accessor.");
                }

                if (this.HasPropertySetAccessor && explicitSetterImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument combination for the explicit interface implementation. The argument '{nameof(propertyInfo)}' declares a set accessor, but the '{nameof(explicitSetterImplementationMethod)}' is null. Or vice versa. Reason: Both arguments must be provided to describe a setter accessor.");
                }
            }
            else
            {
                if (explicitGetterImplementationMethod is not null || explicitSetterImplementationMethod is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitGetterImplementationMethod)}' or a '{nameof(explicitSetterImplementationMethod)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.HasExplicitPropertyGetAccessor = isExplicitInterfaceImplementation && this.HasPropertyGetAccessor;
            this.HasExplicitPropertySetAccessor = isExplicitInterfaceImplementation && this.HasPropertySetAccessor;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsIndexerProperty = isIndexerProperty;
            this._explicitGetterImplementationMethodDescriptor = this.HasPropertyGetAccessor && isExplicitInterfaceImplementation
                ? new PropertyAccessorDescriptor(propertyInfo.GetGetMethod(), PropertyAccessors.Get, true)
                : default;
            this._explicitSetterImplementationMethodDescriptor = this.HasPropertySetAccessor && isExplicitInterfaceImplementation
                ? new PropertyAccessorDescriptor(propertyInfo.GetSetMethod(), PropertyAccessors.Set, true)
                : default;
            this.IsAnonymous = false;
            this._propertyInfo = propertyInfo;
        }

        public PropertyAccessors DeclaredPropertyAccessors { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool HasExplicitPropertyGetAccessor { get; }
        public bool HasExplicitPropertySetAccessor { get; }
        public bool IsIndexerProperty { get; }
        public bool IsAnonymous { get; }
        public bool HasPropertyGetAccessor { get; }
        public bool HasPropertySetAccessor { get; }

        private readonly PropertyAccessorDescriptor _explicitGetterImplementationMethodDescriptor;
        public PropertyAccessorDescriptor ExplicitGetterImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.HasPropertyGetAccessor
                ? this._explicitGetterImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitGetterImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly PropertyAccessorDescriptor _explicitSetterImplementationMethodDescriptor;
        public PropertyAccessorDescriptor ExplicitSetterImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.HasPropertySetAccessor
                ? this._explicitSetterImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitSetterImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly PropertyInfo? _propertyInfo;
        public PropertyInfo PropertyInfo
            => this.IsAnonymous
                ? throw new InvalidOperationException($"The property '{nameof(this.PropertyInfo)}' cannot be accessed for an anonymous property descriptor.")
                : this._propertyInfo!;

        public bool Equals(WellKnownPropertyDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.HasExplicitPropertyGetAccessor.Equals(other.HasExplicitPropertyGetAccessor)
            && this.HasExplicitPropertySetAccessor.Equals(other.HasExplicitPropertySetAccessor)
            && this.ExplicitGetterImplementationMethodDescriptor.Equals(other.ExplicitGetterImplementationMethodDescriptor)
            && this.ExplicitSetterImplementationMethodDescriptor.Equals(other.ExplicitSetterImplementationMethodDescriptor)
            && this.IsIndexerProperty == other.IsIndexerProperty
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this._propertyInfo, other._propertyInfo)
            && this.HasPropertyGetAccessor == other.HasPropertyGetAccessor
            && this.HasPropertySetAccessor == other.HasPropertySetAccessor;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.HasExplicitPropertyGetAccessor);
            hasCode.Add(this.HasExplicitPropertySetAccessor);
            hasCode.Add(this.ExplicitGetterImplementationMethodDescriptor);
            hasCode.Add(this.ExplicitSetterImplementationMethodDescriptor);
            hasCode.Add(this.IsIndexerProperty);
            hasCode.Add(this.HasPropertyGetAccessor);
            hasCode.Add(this.HasPropertySetAccessor);
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this._propertyInfo);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownPropertyDescriptor left, WellKnownPropertyDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownPropertyDescriptor other && Equals(other);
    }
}
