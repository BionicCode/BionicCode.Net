namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about well-known method or constructor.
    /// </summary>
    /// <remarks>The <see cref="WellKnownMethodOrConstructorDescriptor"/> is used to provide information for well-known method symbols, which is when the caller has the direct <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation.
    /// <para/>When the caller does not have the direct <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation and only signature information is available the method symbol is considered anonymous.
    /// In such case use the <see cref="AnonymousMethodOrConstructorDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="MethodInfo"/> is available to ensure maximum accuracy and performance.
    internal readonly struct WellKnownMethodOrConstructorDescriptor : IEquatable<WellKnownMethodOrConstructorDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WellKnownMethodOrConstructorDescriptor"/> struct for a well-known method.
        /// </summary>
        /// <remarks>The <see cref="WellKnownMethodOrConstructorDescriptor"/> is used to provide information about a well-known method symbol, which is when the caller has a direct representation (a <see cref="MethodInfo"/>) of the method symbol.
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="methodInfo"/> must represent a <see cref="MethodInfo"/> obtained from an interface type.
        /// </remarks>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>. If <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> then the <see cref="MethodInfo"/>  must be obtained from the declaring interface type.</param>
        /// <param name="propertyAccessor">If the method is a property accessor, this parameter provides the accessor information.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="methodInfo"/> must represent a <see cref="MethodInfo"/> that  was obtained from the declaring interface type.</param>
        /// <returns>A new instance of <see cref="WellKnownMethodOrConstructorDescriptor"/> representing the specified well-known method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="methodInfo"/> is not from an interface type.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has a value that is not defined by the <see cref="PropertyAccessors"/> enum.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has value <see cref="PropertyAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public WellKnownMethodOrConstructorDescriptor(MethodInfo methodInfo, PropertyAccessors propertyAccessor, bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle,)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(propertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                propertyAccessor,
                [PropertyAccessors.None],
                nameof(propertyAccessor),
                $"Invalid argument '{nameof(propertyAccessor)}'. The argument '{nameof(propertyAccessor)}' has an undefined value. The value '{propertyAccessor}' is not allowed.");

            if (isExplicitInterfaceImplementation)
            {
                Type? declaringType = methodInfo.DeclaringType;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(methodInfo),
                    $"The declaring type represented by the argument '{nameof(methodInfo)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(methodInfo)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type of the argument '{nameof(methodInfo)}' is an interface. Reason: Only non-interface types can provide the explicit interface implementation.");
            }

            this.MethodName = methodInfo.Name;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.MethodHandle = methodInfo.MethodHandle;
            this.IsAnonymous = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WellKnownMethodOrConstructorDescriptor"/> struct for a well-known method.
        /// </summary>
        /// <remarks>The <see cref="WellKnownMethodOrConstructorDescriptor"/> is used to provide information about a well-known method symbol, which is when the caller has a direct representation (a <see cref="MethodInfo"/>) of the method symbol.
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="constructorInfo"/> must represent a <see cref="MethodInfo"/> obtained from an interface type.
        /// </remarks>
        /// <param name="constructorInfo">The <see cref="MethodInfo"/>.</param>
        /// <returns>A new instance of <see cref="WellKnownMethodOrConstructorDescriptor"/> representing the specified well-known constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorInfo"/> is <see langword="null"/>.</exception>
        public WellKnownMethodOrConstructorDescriptor(ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(constructorInfo);

            this.MethodName = string.Empty;
            this.IsExplicitInterfaceImplementation = false;
            this.MethodHandle = constructorInfo.MethodHandle;
            this.IsAnonymous = false;
        }

        public RuntimeMethodHandle MethodHandle { get; }

        public string MethodName { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsAnonymous { get; }

        public bool Equals(WellKnownMethodOrConstructorDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IsAnonymous == other.IsAnonymous
            && this.MethodHandle == other.MethodHandle
            && this.MethodName.Equals(other.MethodName, StringComparison.Ordinal);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.IsAnonymous);
            hashCode.Add(this.MethodHandle);
            hashCode.Add(this.MethodName);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(WellKnownMethodOrConstructorDescriptor left, WellKnownMethodOrConstructorDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownMethodOrConstructorDescriptor left, WellKnownMethodOrConstructorDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownMethodOrConstructorDescriptor other && Equals(other);
    }
}
