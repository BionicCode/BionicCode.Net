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
        /// Creates a new instance of the <see cref="AnonymousPropertyDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="AnonymousPropertyDescriptor"/> is used to provide information for property symbols of which the caller does not have a direct representation <see cref="PropertyInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the interface type as the <paramref name="declaringInterfaceTypeHandle"/> parameter.
        /// <para/> For best accuracy and performance always use the <see cref="WellKnownPropertyDescriptor"/> when the caller has direct access to the <see cref="PropertyInfo"/> representation of the property.
        /// </remarks>
        /// <param name="implementingTypeHandle">The runtime type handle representing the implementing type of the anonymous property.
        /// </param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="declaredPropertyAccessors">Specifies the accessor that the property declares. Can't be <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</param>
        /// <param name="indexerGetterParameters">The list of parameters for the anonymous indexer getter. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="indexerSetterParameters">The list of parameters for the anonymous indexer setter. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="implementingTypeHandle"/> must represent an interface type.
        /// </param>
        /// <param name="declaringInterfaceTypeHandle">The runtime type handle representing the declaring interface type of the anonymous property.
        /// <para/>Must be provided when the property is an explicit interface implementation (which is when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/>).
        /// <br/>Otherwise, this parameter can be <see langword="null"/> and will be ignored.</param>
        /// <returns>A new instance of <see cref="AnonymousPropertyDescriptor"/> representing the specified anonymous property.</returns>

        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="implementingTypeHandle"/> is <see langword="default"/>.</item>
        /// <item><paramref name="propertyName"/> is <see langword="null"/>, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringInterfaceTypeHandle"/> is <see langword="default"/> or <see langword="null"/>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringInterfaceTypeHandle"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="implementingTypeHandle"/> refers to an interface type.</item>
        /// <item>the provided <paramref name="declaredPropertyAccessors"/> value is not defined by the enum <see cref="PropertyAccessors"/>.</item>
        /// <item>the provided <paramref name="declaredPropertyAccessors"/> value is <see cref="PropertyAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public AnonymousPropertyDescriptor(
            RuntimeTypeHandle implementingTypeHandle,
            string propertyName,
            PropertyAccessors declaredPropertyAccessors,
            MethodParameterInfoList? indexerGetterParameters,
            MethodParameterInfoList? indexerSetterParameters,
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

            this.ImplementingTypeHandle = implementingTypeHandle;
            this.DeclaringInterfaceTypeHandle = isExplicitInterfaceImplementation
                ? declaringInterfaceTypeHandle!.Value
                : default;
            this.PropertyName = propertyName;
            this.DeclaredAccessors = declaredPropertyAccessors;
            this.IsIndexerProperty = this.IndexerGetterMethodParameterInfoList.HasItems || this.IndexerSetterMethodParameterInfoList.HasItems;
            this.IndexerGetterMethodParameterInfoList = indexerGetterParameters;
            this.IndexerSetterMethodParameterInfoList = indexerSetterParameters;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
        }

        public RuntimeTypeHandle ImplementingTypeHandle { get; }
        public RuntimeTypeHandle DeclaringInterfaceTypeHandle { get; }
        public string PropertyName { get; }
        public PropertyAccessors DeclaredAccessors { get; }
        public MethodParameterInfoList IndexerGetterMethodParameterInfoList { get; }
        public MethodParameterInfoList IndexerSetterMethodParameterInfoList { get; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool IsIndexerProperty { get; }
        public bool IsAnonymous { get; }
        public bool HasPropertyGetAccessor { get; }
        public bool HasPropertySetAccessor { get; }

        public bool Equals(AnonymousPropertyDescriptor other)
            => this.ImplementingTypeHandle.Equals(other.ImplementingTypeHandle)
            && this.DeclaringInterfaceTypeHandle.Equals(other.DeclaringInterfaceTypeHandle)
            && this.PropertyName.Equals(other.PropertyName, StringComparison.Ordinal)
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
            hasCode.Add(this.ImplementingTypeHandle);
            hasCode.Add(this.DeclaringInterfaceTypeHandle);
            hasCode.Add(this.PropertyName);
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
