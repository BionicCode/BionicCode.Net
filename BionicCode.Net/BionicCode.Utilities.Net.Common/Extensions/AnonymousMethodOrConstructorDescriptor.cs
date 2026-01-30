namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an anonymous method (where the caller does not have a direct representation (a see cref="MethodInfo"/>) of the method symbol).
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
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.
        /// <para/>If the method is an explicit interface implementation, then this must represent an interface type.
        /// </param>
        /// <param name="methodName">The name of the anonymous method. Cannot be null, empty, or consist only of white-space characters.
        /// <para/>For constructors this parameter is ignored (constructors don't have names).</param>
        /// <param name="isConstructor"><see langword="true"/> if the method is a constructor; otherwise, <see langword="false"/>.</param>
        /// <param name="methodPropertyAccessor">If the method is a property accessor, this parameter provides the accessor information.
        /// <para/>For property accessors, this parameter is required and is not allowed to be <see cref="PropertyAccessor.Undefined"/>.
        /// <para/>For constructors this parameter is ignored.</param>
        /// <param name="methodOrConstructorParameters">The list of parameters for the anonymous method. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.
        /// <para/>For  constructors this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="declaringTypeHandle"/> must represent an interface type and the <paramref name="isConstructor"/> must be <see langword="false"/> (constructors can't be explicit interface implementations).</param>
        /// <returns>A new instance of <see cref="AnonymousMethodOrConstructorDescriptor"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="methodName"/> is null, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringTypeHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="methodPropertyAccessor"/> has a value that is not defined by the <see cref="PropertyAccessor"/> enum.</item>
        /// <item>Is also thrown when <paramref name="methodPropertyAccessor"/> has value <see cref="PropertyAccessor.Undefined"/>.</item>
        /// </list>
        /// </exception>
        public AnonymousMethodOrConstructorDescriptor(RuntimeTypeHandle declaringTypeHandle,
            bool isConstructor,
            bool isExplicitInterfaceImplementation,
            ParameterList? methodOrConstructorParameters,
            string methodName,
            PropertyAccessor methodPropertyAccessor,
            TypeList? genericMethodParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);

            if (isConstructor)
            {
                methodName = string.Empty;
                methodPropertyAccessor = PropertyAccessor.None;

                // Constructors can't be generic
                genericMethodParameters = TypeList.Empty;
            }
            else
            {
                ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(methodName);
            }

            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(methodPropertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                methodPropertyAccessor,
                [PropertyAccessor.Undefined],
                nameof(methodPropertyAccessor),
                $"Invalid argument '{nameof(methodPropertyAccessor)}'. The argument '{nameof(methodPropertyAccessor)}' has an undefined value. The value '{methodPropertyAccessor}' is not allowed.");

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfTrue(
                    isConstructor,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the argument '{nameof(isConstructor)}' is also 'true'. Reason: Constructors cannot be explicit interface implementations.");

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(declaringTypeHandle)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            this.DeclaringTypeHandle = declaringTypeHandle;
            this.MethodName = methodName;
            this.IsConstructor = isConstructor;
            this.PropertyAccessor = methodPropertyAccessor;
            this.IsPropertyAccessor = methodPropertyAccessor is not PropertyAccessor.Undefined and not PropertyAccessor.None;
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
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.
        /// <para/>If the method is an explicit interface implementation, then this must represent an interface type.
        /// </param>
        /// <param name="methodName">The name of the anonymous method. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="propertyAccessor">If the method is a property accessor, this parameter provides the accessor information.</param>
        /// <param name="methodParameters">The list of parameters for the anonymous method. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="declaringTypeHandle"/> must represent an interface type.</param>
        /// <returns>A new instance of <see cref="AnonymousMethodOrConstructorDescriptor"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="methodName"/> is null, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringTypeHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has a value that is not defined by the <see cref="PropertyAccessor"/> enum.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has value <see cref="PropertyAccessor.Undefined"/>.</item>
        /// </list>
        /// </exception>
        public AnonymousMethodOrConstructorDescriptor(RuntimeTypeHandle declaringTypeHandle,
            string methodName,
            MethodParameterInfoList? methodParameters,
            TypeList? genericMethodParameters,
            PropertyAccessor propertyAccessor,
            bool isExplicitInterfaceImplementation)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(methodName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(propertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                propertyAccessor,
                [PropertyAccessor.Undefined],
                nameof(propertyAccessor),
                $"Invalid argument '{nameof(propertyAccessor)}'. The argument '{nameof(propertyAccessor)}' has an undefined value. The value '{propertyAccessor}' is not allowed.");

            if (isExplicitInterfaceImplementation)
            {
                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(declaringTypeHandle)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            this.DeclaringTypeHandle = declaringTypeHandle;
            this.MethodName = methodName;
            this.PropertyAccessor = propertyAccessor;
            this.IsPropertyAccessor = propertyAccessor is not PropertyAccessor.Undefined and not PropertyAccessor.None;
            this.MethodParameterInfoList = methodParameters.OrEmpty();
            this.MethodParameters = ParameterList.Empty;
            this.GenericMethodParameters = genericMethodParameters.OrEmpty();
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public string MethodName { get; }
        public bool IsConstructor { get; }
        public PropertyAccessor PropertyAccessor { get; }
        public MethodParameterInfoList MethodParameterInfoList { get; }
        public ParameterList MethodParameters { get; }
        public TypeList GenericMethodParameters { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsPropertyAccessor { get; }
        public bool IsAnonymous { get; }

        public bool Equals(AnonymousMethodOrConstructorDescriptor other)
            => this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.MethodName.Equals(other.MethodName, StringComparison.Ordinal)
            && this.MethodParameters.Equals(other.MethodParameters)
            && this.GenericMethodParameters.Equals(other.GenericMethodParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.MethodParameterInfoList.Equals(other.MethodParameterInfoList)
            && this.PropertyAccessor.Equals(other.PropertyAccessor)
            && this.IsPropertyAccessor.Equals(other.IsPropertyAccessor)
            && this.IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.DeclaringTypeHandle);
            hashCode.Add(this.MethodName);
            hashCode.Add(this.MethodParameters);
            hashCode.Add(this.GenericMethodParameters);
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.MethodParameterInfoList);
            hashCode.Add(this.PropertyAccessor);
            hashCode.Add(this.IsPropertyAccessor);
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
