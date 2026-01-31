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
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="declaringInterfaceTypeHandle"/> must provide a <see cref="RuntimeTypeHandle"/> that refers to the originally declaring interface type.
        /// <para/>For best accuracy and performance always use the <see cref="WellKnownMethodOrConstructorDescriptor"/>, which requires the caller to have direct access to the <see cref="MethodInfo"/> or <see cref="ConstructorInfo"/> representation of the method or constructor.
        /// </remarks>
        /// <param name="implementingTypeHandle">The runtime type handle representing the implementing type of the anonymous method or constructor.</param>
        /// <param name="methodName">Conditionally optional. The name of the anonymous method. For methods the value cannot be null, empty, or consist only of white-space characters.
        /// <para/>For constructors this parameter is ignored (constructors don't have names).</param>
        /// <param name="isConstructor"><see langword="true"/> if the method is a constructor; otherwise, <see langword="false"/>.</param>
        /// <param name="methodOrConstructorParameters">The list of parameters for the anonymous method. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.
        /// <para/>For  constructors this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/> and the descriptor describes an explicitly implemented method,
        /// then the <paramref name="declaringInterfaceTypeHandle"/> must provide a <see cref="RuntimeTypeHandle"/> that was obtained from the declaring interface type.</param>
        /// <param name="declaringInterfaceTypeHandle">The runtime type handle representing the declaring interface type of the anonymous method.
        /// <para/>Must be provided when the method is an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/>).
        /// <br/>Otherwise, this parameter can be <see langword="null"/> and will be ignored.</param>
        /// <returns>A new instance of <see cref="AnonymousMethodOrConstructorDescriptor"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="implementingTypeHandle"/> is <see langword="default"/>.</item>
        /// <item><paramref name="methodName"/> is <see langword="null"/>, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringInterfaceTypeHandle"/> is <see langword="default"/>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringInterfaceTypeHandle"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> and <paramref name="isConstructor"/> is also <see langword="true"/> since constructors can't be explicitly implemented.</item>
        /// <item>the provided <paramref name="implementingTypeHandle"/> refers to an interface type.</item>
        /// </list>
        /// </exception>
        public AnonymousMethodOrConstructorDescriptor(
            RuntimeTypeHandle implementingTypeHandle,
            bool isConstructor,
            MethodParameterInfoList? methodOrConstructorParameters,
            string? methodName,
            TypeList? genericMethodParameters,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(implementingTypeHandle);
            Type? implementingType = Type.GetTypeFromHandle(implementingTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                implementingType,
                nameof(implementingTypeHandle),
                $"Invalid argument '{nameof(implementingTypeHandle)}'. The provided declaring type handle does not resolve to a runtime type.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                implementingType!.IsInterface,
                nameof(implementingTypeHandle),
                $"Invalid argument '{nameof(implementingTypeHandle)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is pointing to an interface type. Reason: Only non-interface types can provide the implementations.");

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

            this.DeclaringInterfaceTypeHandle = isExplicitInterfaceImplementation
                ? declaringInterfaceTypeHandle!.Value
                : default;
            this.ImplementingTypeHandle = implementingTypeHandle;
            this.MethodName = methodName;
            this.IsConstructor = isConstructor;
            this.MethodOrConstructorParameterList = methodOrConstructorParameters.OrEmpty();
            this.GenericMethodParameters = genericMethodParameters.OrEmpty();
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public RuntimeTypeHandle ImplementingTypeHandle { get; }
        public RuntimeTypeHandle DeclaringInterfaceTypeHandle { get; }
        public string MethodName { get; }
        public bool IsConstructor { get; }
        public MethodParameterInfoList MethodOrConstructorParameterList { get; }
        public TypeList GenericMethodParameters { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsAnonymous { get; }

        public bool Equals(AnonymousMethodOrConstructorDescriptor other)
            => this.ImplementingTypeHandle.Equals(other.ImplementingTypeHandle)
            && this.IsConstructor.Equals(other.IsConstructor)
            && this.DeclaringInterfaceTypeHandle.Equals(other.DeclaringInterfaceTypeHandle)
            && this.MethodName.Equals(other.MethodName, StringComparison.Ordinal)
            && this.GenericMethodParameters.Equals(other.GenericMethodParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.MethodOrConstructorParameterList.Equals(other.MethodOrConstructorParameterList)
            && this.IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.ImplementingTypeHandle);
            hashCode.Add(this.IsConstructor);
            hashCode.Add(this.DeclaringInterfaceTypeHandle);
            hashCode.Add(this.MethodName);
            hashCode.Add(this.GenericMethodParameters);
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.MethodOrConstructorParameterList);
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
