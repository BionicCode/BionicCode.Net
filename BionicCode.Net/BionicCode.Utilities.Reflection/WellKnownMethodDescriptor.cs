namespace BionicCode.Utilities.Net.Reflection
{
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
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="methodInfo"/> must represent a <see cref="MethodInfo"/> obtained from an interface type.
        /// </remarks>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>. If <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> then the <see cref="MethodInfo"/> must be obtained from the declaring interface type.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="methodInfo"/> must represent a <see cref="MethodInfo"/> that  was obtained from the declaring interface type.</param>
        /// <param name="implementingType">The runtime type handle representing the implementing type of the explicitly implemented interface method.
        /// <para/>Must be provided when the method is an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/>).
        /// <br/>Otherwise, this parameter can be <see langword="null"/> and will be ignored.</param>
        /// <returns>A new instance of <see cref="WellKnownMethodDescriptor"/> representing the specified well-known method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="methodInfo"/> is not from an interface type.</item>
        /// </list>
        /// </exception>
        public WellKnownMethodDescriptor(
            MethodInfo methodInfo,
            bool isExplicitInterfaceImplementation,
            Type? implementingType)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);
            if (isExplicitInterfaceImplementation)
            {
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    implementingType,
                    nameof(implementingType),
                    $"Invalid argument '{nameof(implementingType)}'. The provided implementor type handle is 'NULL' which is not allowed for explicit interface implementations (which is when '{nameof(isExplicitInterfaceImplementation)}' is 'true').");
                ArgumentExceptionAdvanced.ThrowIfTrue(implementingType!.IsInterface,
                    nameof(implementingType),
                    $"Invalid argument '{nameof(implementingType)}'. The argument '{nameof(implementingType)}' points to an interface type. Reason: Only non-interface types can provide the explicit interface implementations.");

                this._implementingTypeHandle = implementingType.TypeHandle;

                Type? declaringInterfaceType = methodInfo.DeclaringType;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringInterfaceType,
                    nameof(implementingType),
                    $"The declaring type represented by the argument '{nameof(methodInfo)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringInterfaceType!.IsInterface,
                    nameof(methodInfo),
                    $"Invalid argument '{nameof(methodInfo)}'. The argument '{nameof(methodInfo)}' points to a non-interface type which is not allowed when '{nameof(isExplicitInterfaceImplementation)}' is 'true'. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            this.MethodName = methodInfo.Name;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.MethodHandle = methodInfo.MethodHandle;
            this.IsAnonymous = false;
        }

        /// <summary>
        /// Returns the runtime method handle of the well-known method.
        /// </summary>
        /// <value>If the method is an explicit interface implementation (<see cref="this.IsExplicitInterfaceImplementation"/> is <see langword="true"/>), this property returns the runtime method handle of the <see cref="MethodInfo"/> that maps to the declaring interface type; otherwise, it returns handle to the implementation <see cref="MethodInfo"/>.</value>
        public RuntimeMethodHandle MethodHandle { get; }

        private readonly RuntimeTypeHandle _implementingTypeHandle;
        public RuntimeTypeHandle ImplementingTypeHandle
            => this.IsExplicitInterfaceImplementation
                ? this._implementingTypeHandle
                : throw new InvalidOperationException($"The property '{nameof(this.ImplementingTypeHandle)}' can only be accessed when the method is an explicit interface implementation (i.e. when the property '{nameof(this.IsExplicitInterfaceImplementation)}' is 'true').");

        public string MethodName { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsAnonymous { get; }

        public bool Equals(WellKnownMethodDescriptor other)
            => this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IsAnonymous == other.IsAnonymous
            && this.MethodHandle == other.MethodHandle
            && this.MethodName.Equals(other.MethodName, StringComparison.Ordinal)
            && this._implementingTypeHandle.Equals(other._implementingTypeHandle);

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.IsAnonymous);
            hashCode.Add(this.MethodHandle);
            hashCode.Add(this.MethodName, StringComparer.Ordinal);
            hashCode.Add(this._implementingTypeHandle);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(WellKnownMethodDescriptor left, WellKnownMethodDescriptor right)
            => left.Equals(right);
        public static bool operator !=(WellKnownMethodDescriptor left, WellKnownMethodDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is WellKnownMethodDescriptor other && Equals(other);
    }
}
