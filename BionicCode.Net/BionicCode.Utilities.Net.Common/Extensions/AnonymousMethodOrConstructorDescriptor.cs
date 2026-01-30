namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an anonymous method (where the caller does not have a direct representation (a <see cref="MethodInfo"/>) of the method symbol).
    /// </summary>
    /// <remarks>The <see cref="AnonymousMethodOrConstructorDescriptor"/> is used to provide information for anonymous method symbols, which is when the caller does not have the direct <see cref="MethodInfo"/> representation.
    /// <para/>When the caller has the direct <see cref="MethodInfo"/> representation and the method is well-known, use the <see cref="WellKnownMethodOrConstructorDescriptor"/> instead.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="MethodInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct AnonymousMethodOrConstructorDescriptor : IEquatable<AnonymousMethodOrConstructorDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMethodOrConstructorDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="AnonymousMethodOrConstructorDescriptor"/> is used to provide information about a method symbol of which the caller does not have a direct representation <see cref="MethodInfo"/> or <see cref="MethodData"/> and instead only signature information is available.
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// <para/>In general, as much as possible optional parameters should be provided to ensure maximum accuracy when the descriptor is used for lookups.
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.
        /// <para/>If the method is an explicit interface implementation, then this must represent an interface type.
        /// </param>
        /// <param name="methodName">Conditionally optional. The name of the anonymous method. For methods the value cannot be null, empty, or consist only of white-space characters.
        /// <para/>For constructors this parameter is ignored (constructors don't have names).</param>
        /// <param name="isConstructor"><see langword="true"/> if the method is a constructor; otherwise, <see langword="false"/>.</param>
        /// <param name="methodOrConstructorParameters">The list of parameters for the anonymous method. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.
        /// <para/>For  constructors this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="declaringTypeHandle"/> must represent an interface type and the <paramref name="isConstructor"/> must be <see langword="false"/> (constructors can't be explicit interface implementations).</param>
        /// <param name="declaringInterfaceTypeHandle">The runtime type handle representing the declaring interface type of the anonymous method, constructor.
        /// <para/>Must be provided when the method is an explicit interface implementation. Otherwise, this parameter can be <see langword="null"/> and will be ignored.</param>
        /// <returns>A new instance of <see cref="AnonymousMethodOrConstructorDescriptor"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="methodName"/> is null, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringTypeHandle"/> is not an interface type.</item>
        /// </list>
        /// </exception>
        public AnonymousMethodOrConstructorDescriptor(RuntimeTypeHandle declaringTypeHandle,
            bool isConstructor,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle,
            ParameterList? methodOrConstructorParameters,
            string? methodName,
            TypeList? genericMethodParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);

            if (isConstructor)
            {
                methodName = string.Empty;

                // Constructors can't be generic
                genericMethodParameters = TypeList.Empty;
            }
            else
            {
                ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(methodName);
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    isConstructor,
                    nameof(isConstructor),
                    $"Invalid argument '{nameof(isConstructor)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the argument '{nameof(isConstructor)}' is also 'true'. Reason: Constructors cannot be explicit interface implementations.");

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(declaringTypeHandle)}'. The argument '{nameof(declaringTypeHandle)}' points to an interface and not the implementing type. Reason: Only non-interface types can provide the implementation of explicit interface members.");

                ArgumentNullExceptionAdvanced.ThrowIfNull(declaringInterfaceTypeHandle);
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

            this.DeclaringInterfaceTypeHandle = isExplicitInterfaceImplementation
                ? declaringInterfaceTypeHandle!.Value
                : default;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.MethodName = methodName;
            this.IsConstructor = isConstructor;
            this.MethodParameters = methodOrConstructorParameters.OrEmpty();
            this.MethodParameterInfoList = MethodParameterInfoList.Empty;
            this.GenericMethodParameters = genericMethodParameters.OrEmpty();
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="AnonymousMethodOrConstructorDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="AnonymousMethodOrConstructorDescriptor"/> is used to provide information about a method symbol of which the caller does not have a direct representation <see cref="MethodInfo"/> or <see cref="MethodData"/> and instead only signature information is available.
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// <para/>In general, as much as possible optional parameters should be provided to ensure maximum accuracy when the descriptor is used for lookups.
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.
        /// <para/>If the method is an explicit interface implementation, then this must represent an interface type.
        /// </param>
        /// <param name="methodName">Conditionally optional. The name of the anonymous method. For methods the value cannot be null, empty, or consist only of white-space characters.
        /// <para/>For constructors this parameter is ignored (constructors don't have names).</param>
        /// <param name="isConstructor"><see langword="true"/> if the method is a constructor; otherwise, <see langword="false"/>.</param>
        /// <param name="methodOrConstructorParameters">The list of parameters for the anonymous method. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.
        /// <para/>For  constructors this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="declaringTypeHandle"/> must represent an interface type and the <paramref name="isConstructor"/> must be <see langword="false"/> (constructors can't be explicit interface implementations).</param>
        /// <param name="declaringInterfaceTypeHandle">The runtime type handle representing the declaring interface type of the anonymous method, constructor.
        /// <para/>If the method is an explicit interface implementation, then this must represent an interface type. For non-explicit interface implementations this parameter is ignored.
        /// </param>
        /// <returns>A new instance of <see cref="AnonymousMethodOrConstructorDescriptor"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="methodName"/> is null, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringTypeHandle"/> is not an interface type.</item>
        /// </list>
        /// </exception>
        public AnonymousMethodOrConstructorDescriptor(RuntimeTypeHandle declaringTypeHandle,
            bool isConstructor,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle,
            MethodParameterInfoList? methodOrConstructorParameters,
            string? methodName,
            TypeList? genericMethodParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);

            if (isConstructor)
            {
                methodName = string.Empty;

                // Constructors can't be generic
                genericMethodParameters = TypeList.Empty;
            }
            else
            {
                ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(methodName);
            }

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    isConstructor,
                    nameof(isConstructor),
                    $"Invalid argument '{nameof(isConstructor)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the argument '{nameof(isConstructor)}' is also 'true'. Reason: Constructors cannot be explicit interface implementations.");

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(declaringTypeHandle)}'. The argument '{nameof(declaringTypeHandle)}' points to an interface and not the implementing type. Reason: Only non-interface types can provide the implementation of explicit interface members.");

                ArgumentNullExceptionAdvanced.ThrowIfNull(declaringInterfaceTypeHandle);
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

            this.DeclaringInterfaceTypeHandle = isExplicitInterfaceImplementation
                ? declaringInterfaceTypeHandle!.Value
                : default;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.MethodName = methodName;
            this.IsConstructor = isConstructor;
            this.MethodParameters = ParameterList.Empty;
            this.MethodParameterInfoList = methodOrConstructorParameters.OrEmpty();
            this.GenericMethodParameters = genericMethodParameters.OrEmpty();
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public string MethodName { get; }
        public bool IsConstructor { get; }
        public MethodParameterInfoList MethodParameterInfoList { get; }
        public ParameterList MethodParameters { get; }
        public TypeList GenericMethodParameters { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsAnonymous { get; }
        public RuntimeTypeHandle DeclaringInterfaceTypeHandle { get; }

        public bool Equals(AnonymousMethodOrConstructorDescriptor other)
            => this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.IsConstructor.Equals(other.IsConstructor)
            && this.DeclaringInterfaceTypeHandle.Equals(other.DeclaringInterfaceTypeHandle)
            && this.MethodName.Equals(other.MethodName, StringComparison.Ordinal)
            && this.MethodParameters.Equals(other.MethodParameters)
            && this.GenericMethodParameters.Equals(other.GenericMethodParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.MethodParameterInfoList.Equals(other.MethodParameterInfoList)
            && this.IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.DeclaringTypeHandle);
            hashCode.Add(this.IsConstructor);
            hashCode.Add(this.DeclaringInterfaceTypeHandle);
            hashCode.Add(this.MethodName);
            hashCode.Add(this.MethodParameters);
            hashCode.Add(this.GenericMethodParameters);
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.MethodParameterInfoList);
            hashCode.Add(this.IsAnonymous);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(AnonymousMethodOrConstructorDescriptor left, AnonymousMethodOrConstructorDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousMethodOrConstructorDescriptor left, AnonymousMethodOrConstructorDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is AnonymousMethodOrConstructorDescriptor other && Equals(other);
    }
}
