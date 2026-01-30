namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an anonymous property or indexer property.
    /// </summary>
    /// <remarks>The <see cref="AnonymousPropertyDescriptor"/> is used to provide information for anonymous property symbols, which is when the caller does not have the direct <see cref="PropertyInfo"/> representation and only signature information is available.
    /// <para/>When the caller has the direct <see cref="PropertyInfo"/> representation then the property is considered well-known. In such case use the <see cref="WellKnownPropertyDescriptor"/> instead.
    /// <para/>
    /// Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="PropertyInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct AnonymousPropertyDescriptor : IEquatable<AnonymousPropertyDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="WellKnownPropertyDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="AnonymousPropertyDescriptor"/> is used to provide information for anonymous property symbols, which is when the caller does not have a direct representation <see cref="PropertyInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the interface type as the <paramref name="declaringTypeHandle"/> parameter (it's crucial to provide the interface type as the declaring type).
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.
        /// <para/>If the property is an explicit interface implementation, ensure to provide the interface type as the declaring type handle.
        /// </param>
        /// <param name="declaredPropertyAccessors">Specifies the accessor that the property declares. Can't be <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="indexerGetterParameters">The list of parameters for the anonymous indexer getter. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="indexerSetterParameters">The list of parameters for the anonymous indexer setter. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// </param>
        /// <param name="explicitGetterImplementationMethodDescriptor"></param>
        /// <param name="explicitSetterImplementationMethodDescriptor"></param>
        /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/> or <paramref name="propertyName"/> is <see langword="null"/> (or is empty or only consists of white-space characters).</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringTypeHandle"/> does not represent an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitGetterImplementationMethodDescriptor"/> nor <paramref name="explicitSetterImplementationMethodDescriptor"/> is provided.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitGetterImplementationMethodDescriptor"/> or <paramref name="explicitSetterImplementationMethodDescriptor"/> is provided.</item>
        /// <item>the provided <paramref name="indexerGetterParameters"/> has items, but the <paramref name="explicitGetterImplementationMethodDescriptor"/> is <see langword="null"/>. Or vice versa.</item>
        /// <item>the provided <paramref name="indexerSetterParameters"/> has items, but the <paramref name="explicitSetterImplementationMethodDescriptor"/> is <see langword="null"/>. Or vice versa.</item>
        /// </list>
        /// </exception>
        public AnonymousPropertyDescriptor(RuntimeTypeHandle declaringTypeHandle,
            string propertyName,
            PropertyAccessors declaredPropertyAccessors,
                ParameterList? indexerGetterParameters,
                ParameterList? indexerSetterParameters,
            bool isExplicitInterfaceImplementation,
            RuntimeTypeHandle? declaringInterfaceTypeHandle,)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(declaredPropertyAccessors);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                declaredPropertyAccessors,
                [PropertyAccessors.None, PropertyAccessors.None],
                $"invalid argument '{nameof(declaredPropertyAccessors)}'. A property must declare at least a getter or a setter.");

            this.HasPropertyGetAccessor = (declaredPropertyAccessors & PropertyAccessors.Get) != 0;
            this.HasPropertySetAccessor = (declaredPropertyAccessors & PropertyAccessors.Set) != 0;

            this.IndexerGetterParameters = indexerGetterParameters.OrEmpty();
            this.IndexerSetterParameters = indexerSetterParameters.OrEmpty();

            if (isExplicitInterfaceImplementation)
            {
                if (explicitGetterImplementationMethodDescriptor is null && explicitSetterImplementationMethodDescriptor is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true', but neither a '{nameof(explicitGetterImplementationMethodDescriptor)}' nor a '{nameof(explicitSetterImplementationMethodDescriptor)}' is provided. Reason: At least one accessor method descriptor must be provided for a valid property.");
                }

                if (this.HasPropertyGetAccessor ^ explicitGetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination. The argument '{nameof(declaredPropertyAccessors)}' specifies a get accessor, but the '{nameof(explicitGetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe a getter accessor.");
                }

                if (this.HasPropertySetAccessor ^ explicitSetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination. The argument '{nameof(declaredPropertyAccessors)}' specifies a set accessor, but the '{nameof(explicitSetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe a setter accessor.");
                }

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(declaringTypeHandle)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }
            else
            {
                if (explicitGetterImplementationMethodDescriptor is not null || explicitSetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitGetterImplementationMethodDescriptor)}' or a '{nameof(explicitSetterImplementationMethodDescriptor)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.DeclaringTypeHandle = declaringTypeHandle;
            this.PropertyName = propertyName;
            this.IsIndexerProperty = this.IndexerGetterParameters.HasItems || this.IndexerSetterParameters.HasItems;
            this.IndexerGetterMethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IndexerSetterMethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="WellKnownPropertyDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="WellKnownPropertyDescriptor"/> is used to provide information for property symbols of which the caller does not have a direct representation <see cref="PropertyInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the interface type as the <paramref name="declaringTypeHandle"/> parameter (it's crucial to provide the interface type as the declaring type).
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.
        /// <para/>If the property is an explicit interface implementation, ensure to provide the interface type as the declaring type handle.
        /// </param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="declaredPropertyAccessors">Specifies the accessor that the property declares. Can't be <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</param>
        /// <param name="indexerGetterParameters">The list of parameters for the anonymous indexer getter. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="indexerSetterParameters">The list of parameters for the anonymous indexer setter. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// </param>
        /// <param name="explicitGetterImplementationMethodDescriptor"></param>
        /// <param name="explicitSetterImplementationMethodDescriptor"></param>
        /// <returns>A new instance of <see cref="WellKnownPropertyDescriptor"/> representing the specified anonymous property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/> or <paramref name="propertyName"/> is <see langword="null"/> (or is empty or only consists of white-space characters).</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringTypeHandle"/> does not represent an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitGetterImplementationMethodDescriptor"/> nor <paramref name="explicitSetterImplementationMethodDescriptor"/> is provided.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitGetterImplementationMethodDescriptor"/> or <paramref name="explicitSetterImplementationMethodDescriptor"/> is provided.</item>
        /// <item>the provided <paramref name="indexerGetterParameters"/> has items, but the <paramref name="explicitGetterImplementationMethodDescriptor"/> is <see langword="null"/>. Or vice versa.</item>
        /// <item>the provided <paramref name="indexerSetterParameters"/> has items, but the <paramref name="explicitSetterImplementationMethodDescriptor"/> is <see langword="null"/>. Or vice versa.</item>
        /// </list>
        /// </exception>
        public AnonymousPropertyDescriptor(RuntimeTypeHandle declaringTypeHandle,
        string propertyName,
        PropertyAccessors declaredPropertyAccessors,
        MethodParameterInfoList? indexerGetterParameters,
        MethodParameterInfoList? indexerSetterParameters,
        bool isExplicitInterfaceImplementation)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(declaredPropertyAccessors);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                declaredPropertyAccessors,
                [PropertyAccessors.None],
                $"invalid argument '{nameof(declaredPropertyAccessors)}'. A property must declare at least a getter or a setter.");

            this.HasPropertyGetAccessor = (declaredPropertyAccessors & PropertyAccessors.Get) != 0;
            this.HasPropertySetAccessor = (declaredPropertyAccessors & PropertyAccessors.Set) != 0;

            this.IndexerGetterMethodParameterInfoList = indexerGetterParameters.OrEmpty();
            this.IndexerSetterMethodParameterInfoList = indexerSetterParameters.OrEmpty();

            if (isExplicitInterfaceImplementation)
            {
                if (explicitGetterImplementationMethodDescriptor is null && explicitSetterImplementationMethodDescriptor is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true', but neither a '{nameof(explicitGetterImplementationMethodDescriptor)}' nor a '{nameof(explicitSetterImplementationMethodDescriptor)}' is provided. Reason: At least one accessor method descriptor must be provided for a valid property.");
                }

                if (this.HasPropertyGetAccessor ^ explicitGetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination for the explicit interface implementation. The argument '{nameof(declaredPropertyAccessors)}' specifies a get accessor, but the '{nameof(explicitGetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe a getter accessor.");
                }

                if (this.HasPropertySetAccessor ^ explicitSetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination for the explicit interface implementation. The argument '{nameof(declaredPropertyAccessors)}' specifies a set accessor, but the '{nameof(explicitSetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe a setter accessor.");
                }

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(declaringTypeHandle)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }
            else
            {
                if (explicitGetterImplementationMethodDescriptor is not null || explicitSetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitGetterImplementationMethodDescriptor)}' or a '{nameof(explicitSetterImplementationMethodDescriptor)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.DeclaringTypeHandle = declaringTypeHandle;
            this.PropertyName = propertyName;
            this.DeclaredAccessors = declaredPropertyAccessors;
            this.IsIndexerProperty = this.IndexerGetterMethodParameterInfoList.HasItems || this.IndexerSetterMethodParameterInfoList.HasItems;
            this.IndexerGetterMethodParameterInfoList = indexerGetterParameters;
            this.IndexerSetterMethodParameterInfoList = indexerSetterParameters;
            this.IndexerGetterParameters = ParameterList.Empty;
            this.IndexerSetterParameters = ParameterList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; }
        public string PropertyName { get; }
        public PropertyAccessors DeclaredAccessors { get; }
        public ParameterList IndexerGetterParameters { get; }
        public ParameterList IndexerSetterParameters { get; }
        public MethodParameterInfoList IndexerGetterMethodParameterInfoList { get; }
        public MethodParameterInfoList IndexerSetterMethodParameterInfoList { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool IsIndexerProperty { get; }
        public bool IsAnonymous { get; }
        public bool HasPropertyGetAccessor { get; }
        public bool HasPropertySetAccessor { get; }

        public bool Equals(AnonymousPropertyDescriptor other)
            => this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.PropertyName.Equals(other.PropertyName, StringComparison.Ordinal)
            && this.IndexerGetterParameters.Equals(other.IndexerGetterParameters)
            && this.IndexerSetterParameters.Equals(other.IndexerSetterParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IndexerGetterMethodParameterInfoList.Equals(other.IndexerGetterMethodParameterInfoList)
            && this.IndexerSetterMethodParameterInfoList.Equals(other.IndexerSetterMethodParameterInfoList)
            && this.IsIndexerProperty == other.IsIndexerProperty
            && this.IsAnonymous == other.IsAnonymous
            && this.HasPropertyGetAccessor == other.HasPropertyGetAccessor
            && this.HasPropertySetAccessor == other.HasPropertySetAccessor;

        public override int GetHashCode()
        {
            var hasCode = new HashCode();
            hasCode.Add(this.DeclaringTypeHandle);
            hasCode.Add(this.PropertyName);
            hasCode.Add(this.IndexerGetterParameters);
            hasCode.Add(this.IndexerSetterParameters);
            hasCode.Add(this.IsExplicitInterfaceImplementation);
            hasCode.Add(this.IndexerGetterMethodParameterInfoList);
            hasCode.Add(this.IndexerSetterMethodParameterInfoList);
            hasCode.Add(this.IsIndexerProperty);
            hasCode.Add(this.HasPropertyGetAccessor);
            hasCode.Add(this.HasPropertySetAccessor);
            hasCode.Add(this.IsAnonymous);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(AnonymousPropertyDescriptor left, AnonymousPropertyDescriptor right)
            => left.Equals(right);
        public static bool operator !=(AnonymousPropertyDescriptor left, AnonymousPropertyDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is AnonymousPropertyDescriptor other && Equals(other);
    }
}
