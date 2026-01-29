namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about the member that declares the parameter for an anonymous or known method cache key.
    /// </summary>
    internal readonly struct CacheKeyMethodDescriptor : IEquatable<CacheKeyMethodDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKeyMethodDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="CacheKeyMethodDescriptor"/> is used to provide information about a method symbol of which the caller does not have a direct representation <see cref="MethodInfo"/> or <see cref="MethodData"/> and instead only signature information is available.
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="methodInfo"/> must represent a <see cref="MethodInfo"/> obtained from an interface type.
        /// </remarks>
        /// <param name="methodInfo">The <see cref="MethodInfo"/>. If <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> then the <see cref="MethodInfo"/>  must be obtained from the declaring interface type.</param>
        /// <param name="propertyAccessor">If the method is a property accessor, this parameter provides the accessor information.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="methodInfo"/> must represent a <see cref="MethodInfo"/> that  was obtained from the declaring interface type.</param>
        /// <returns>A new instance of <see cref="CacheKeyMethodDescriptor"/> representing the specified well-known method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="methodInfo"/> is not from an interface type.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has a value that is not defined by the <see cref="PropertyAccessor"/> enum.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has value <see cref="PropertyAccessor.Undefined"/>.</item>
        /// </list>
        /// </exception>
        public CacheKeyMethodDescriptor(MethodInfo methodInfo, PropertyAccessor propertyAccessor, bool isExplicitInterfaceImplementation)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(methodInfo);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(propertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                propertyAccessor,
                [PropertyAccessor.Undefined],
                nameof(propertyAccessor),
                $"Invalid argument '{nameof(propertyAccessor)}'. The argument '{nameof(propertyAccessor)}' has an undefined value. The value '{propertyAccessor}' is not allowed.");

            if (isExplicitInterfaceImplementation)
            {
                Type? declaringType = methodInfo.DeclaringType;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(methodInfo),
                    $"The declaring type represented by the argument '{nameof(methodInfo)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type of the argument '{nameof(methodInfo)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            this.DeclaringTypeHandle = default;
            this.MethodName = string.Empty;
            this.PropertyAccessor = propertyAccessor;
            this.IsPropertyAccessor = propertyAccessor is not PropertyAccessor.Undefined and not PropertyAccessor.None;
            this.MethodParameters = ParameterList.Empty;
            this.MethodParameterInfoList = MethodParameterInfoList.Empty;
            this.GenericMethodParameters = TypeList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._methodHandle = methodInfo.MethodHandle;
            this.IsAnonymous = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKeyMethodDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="CacheKeyMethodDescriptor"/> is used to provide information about a method symbol of which the caller does not have a direct representation <see cref="MethodInfo"/> or <see cref="MethodData"/> and instead only signature information is available.
        /// <para/>If the method is an explicit interface implementation, then the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.
        /// <para/>If the method is an explicit interface implementation, then this must represent an interface type.
        /// </param>
        /// <param name="methodName">The name of the anonymous method. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="propertyAccessor">If the method is a property accessor, this parameter provides the accessor information.</param>
        /// <param name="methodParameters">The list of parameters for the anonymous method. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="TypeList.Empty"/> for constructors or to indicate a non-generic method.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the method is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="declaringTypeHandle"/> must represent an interface type.</param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="methodName"/> is null, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringTypeHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has a value that is not defined by the <see cref="PropertyAccessor"/> enum.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has value <see cref="PropertyAccessor.Undefined"/>.</item>
        /// </list>
        /// </exception>
        public CacheKeyMethodDescriptor(RuntimeTypeHandle declaringTypeHandle,
            string methodName,
            PropertyAccessor propertyAccessor,
            ParameterList? methodParameters,
            TypeList? genericMethodParameters,
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
            this.MethodParameters = methodParameters.OrEmpty();
            this.MethodParameterInfoList = MethodParameterInfoList.Empty;
            this.GenericMethodParameters = genericMethodParameters.OrEmpty();
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = true;
            this._methodHandle = default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheKeyMethodDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="CacheKeyMethodDescriptor"/> is used to provide information about a method symbol of which the caller does not have a direct representation <see cref="MethodInfo"/> or <see cref="MethodData"/> and instead only signature information is available.
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
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="methodName"/> is null, empty, or consists only of white-space characters.</item>
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="declaringTypeHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has a value that is not defined by the <see cref="PropertyAccessor"/> enum.</item>
        /// <item>Is also thrown when <paramref name="propertyAccessor"/> has value <see cref="PropertyAccessor.Undefined"/>.</item>
        /// </list>
        /// </exception>
        public CacheKeyMethodDescriptor(RuntimeTypeHandle declaringTypeHandle,
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
            this._methodHandle = default;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; init; }

        private readonly RuntimeMethodHandle _methodHandle;
        public RuntimeMethodHandle MethodHandle
            => this.IsAnonymous
                ? throw new InvalidOperationException($"The property '{nameof(this.MethodHandle)}' cannot be accessed for an anonymous property descriptor.")
                : this._methodHandle!;

        public string MethodName { get; init; }
        public PropertyAccessor PropertyAccessor { get; }
        public MethodParameterInfoList MethodParameterInfoList { get; }
        public ParameterList MethodParameters { get; }
        public TypeList GenericMethodParameters { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsPropertyAccessor { get; }
        public bool IsAnonymous { get; }

        public bool Equals(CacheKeyMethodDescriptor other)
            => this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.MethodName.Equals(other.MethodName, StringComparison.Ordinal)
            && this.MethodParameters.Equals(other.MethodParameters)
            && this.GenericMethodParameters.Equals(other.GenericMethodParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.MethodParameterInfoList.Equals(other.MethodParameterInfoList)
            && this.PropertyAccessor.Equals(other.PropertyAccessor)
            && this.IsPropertyAccessor.Equals(other.IsPropertyAccessor)
            && this.IsAnonymous == other.IsAnonymous
            && this._methodHandle == other._methodHandle;

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
            hashCode.Add(this._methodHandle);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(CacheKeyMethodDescriptor left, CacheKeyMethodDescriptor right)
            => left.Equals(right);
        public static bool operator !=(CacheKeyMethodDescriptor left, CacheKeyMethodDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is CacheKeyMethodDescriptor other && Equals(other);
    }

    /// <summary>
    /// A descriptor that provides information about the property for an anonymous or well-known property cache key.
    /// </summary>
    internal readonly struct CacheKeyPropertyDescriptor : IEquatable<CacheKeyPropertyDescriptor>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CacheKeyPropertyDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="CacheKeyPropertyDescriptor"/> is used to provide information for property symbols of which the caller does not have a direct representation <see cref="PropertyInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the <paramref name="propertyInfo"/> obtained from the declaring interface type (it's crucial to provide the interface type as the declaring type).
        /// </remarks>
        /// <param name="propertyInfo">The <see cref="PropertyInfo"/> that the descriptor represents. If <paramref name="isExplicitInterfaceImplementation"/> is s et to <see langword="true"/> then the <see cref="PropertyInfo"/> must be obtained from the declaring interface type.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="propertyInfo"/> must be obtained from the declaring interface type.
        /// </param>
        /// <param name="explicitGetterImplementationMethod"></param>
        /// <param name="explicitSetterImplementationMethod"></param>
        /// <returns>A new instance of <see cref="CacheKeyPropertyDescriptor"/> representing the specified anonymous or well-known property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyInfo"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="propertyInfo"/> was not obtained from an interface type.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but neither <paramref name="explicitGetterImplementationMethod"/> (for a readable property) nor <paramref name="explicitSetterImplementationMethod"/> (for a writeable property) is provided.</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="false"/> but either <paramref name="explicitGetterImplementationMethod"/> or <paramref name="explicitSetterImplementationMethod"/> is provided.</item>
        /// </list>
        /// </exception>
        public CacheKeyPropertyDescriptor(PropertyInfo propertyInfo, bool isExplicitInterfaceImplementation, MethodInfo? explicitGetterImplementationMethod, MethodInfo? explicitSetterImplementationMethod)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo);
            Type? declaringType = propertyInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);

            this.IsReadableProperty = propertyInfo.CanRead;
            this.IsWriteableProperty = propertyInfo.CanWrite;
            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(propertyInfo)}.{nameof(propertyInfo.DeclaringType)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");

                if (this.IsReadableProperty && explicitGetterImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a readable property, but a '{nameof(explicitGetterImplementationMethod)}' is not provided. Reason: All property accessors must have a matching explicit implementation.");
                }

                if (this.IsWriteableProperty && explicitSetterImplementationMethod is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true' for a writeable property, but a '{nameof(explicitSetterImplementationMethod)}' is not provided. Reason: All property accessors must have a matching explicit implementation.");
                }
            }
            else
            {
                if (explicitGetterImplementationMethod is not null || explicitSetterImplementationMethod is not null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'false', but either a '{nameof(explicitGetterImplementationMethod)}' or a '{nameof(explicitSetterImplementationMethod)}' is provided. Reason: Accessor method descriptors can only be provided for explicit interface implementation properties.");
                }
            }

            this.DeclaringTypeHandle = default;
            this.PropertyName = string.Empty;
            this.HasExplicitGetPropertyAccessor = explicitGetterImplementationMethod is not null;
            this.HasExplicitSetPropertyAccessor = explicitSetterImplementationMethod is not null;
            this.IndexerGetterMethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IndexerSetterMethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IndexerGetterParameters = ParameterList.Empty;
            this.IndexerSetterParameters = ParameterList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._explicitGetterImplementationMethodDescriptor = this.IsReadableProperty && isExplicitInterfaceImplementation ? new CacheKeyMethodDescriptor(propertyInfo.GetGetMethod(), PropertyAccessor.Get, true) : default;
            this._explicitSetterImplementationMethodDescriptor = this.IsWriteableProperty && isExplicitInterfaceImplementation ? new CacheKeyMethodDescriptor(propertyInfo.GetSetMethod(), PropertyAccessor.Set, true) : default;
            this.IsAnonymous = false;
            this._propertyInfo = propertyInfo;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CacheKeyPropertyDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="CacheKeyPropertyDescriptor"/> is used to provide information for property symbols of which the caller does not have a direct representation <see cref="PropertyInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the interface type as the <paramref name="declaringTypeHandle"/> parameter (it's crucial to provide the interface type as the declaring type).
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.
        /// <para/>If the property is an explicit interface implementation, ensure to provide the interface type as the declaring type handle.
        /// </param>
        /// <param name="declaredPropertyAccessor">Specifies the accessor that the property declares. Can't be <see cref="PropertyAccessor.None"/> or <see cref="PropertyAccessor.Undefined"/>.</param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="indexerGetterParameters">The list of parameters for the anonymous indexer getter. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="indexerSetterParameters">The list of parameters for the anonymous indexer setter. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// </param>
        /// <param name="explicitGetterImplementationMethodDescriptor"></param>
        /// <param name="explicitSetterImplementationMethodDescriptor"></param>
        /// <returns>A new instance of <see cref="CacheKeyPropertyDescriptor"/> representing the specified anonymous property.</returns>
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
        public CacheKeyPropertyDescriptor(RuntimeTypeHandle declaringTypeHandle,
            string propertyName,
            PropertyAccessor declaredPropertyAccessor,
            ParameterList? indexerGetterParameters,
            ParameterList? indexerSetterParameters,
            bool isExplicitInterfaceImplementation,
            CacheKeyMethodDescriptor? explicitGetterImplementationMethodDescriptor,
            CacheKeyMethodDescriptor? explicitSetterImplementationMethodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(declaredPropertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                declaredPropertyAccessor,
                [PropertyAccessor.None, PropertyAccessor.Undefined],
                $"invalid argument '{nameof(declaredPropertyAccessor)}'. A property must declare at least a getter or a setter.");

            this.IsReadableProperty = (declaredPropertyAccessor & PropertyAccessor.Get) != 0;
            this.IsWriteableProperty = (declaredPropertyAccessor & PropertyAccessor.Set) != 0;
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(declaredPropertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                declaredPropertyAccessor,
                [PropertyAccessor.None, PropertyAccessor.Undefined],
                $"invalid argument '{nameof(declaredPropertyAccessor)}'. A property must declare at least a getter or a setter.");

            this.IsReadableProperty = (declaredPropertyAccessor & PropertyAccessor.Get) != 0;
            this.IsWriteableProperty = (declaredPropertyAccessor & PropertyAccessor.Set) != 0;

            this.IndexerGetterParameters = indexerGetterParameters.OrEmpty();
            this.IndexerSetterParameters = indexerSetterParameters.OrEmpty();

            if (isExplicitInterfaceImplementation)
            {
                if (explicitGetterImplementationMethodDescriptor is null && explicitSetterImplementationMethodDescriptor is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true', but neither a '{nameof(explicitGetterImplementationMethodDescriptor)}' nor a '{nameof(explicitSetterImplementationMethodDescriptor)}' is provided. Reason: At least one accessor method descriptor must be provided for a valid property.");
                }

                if (this.IndexerGetterParameters.HasItems ^ explicitGetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination. The argument '{nameof(indexerGetterParameters)}' has items, but the '{nameof(explicitGetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe an indexer getter accessor.");
                }

                if (this.IndexerSetterParameters.HasItems ^ explicitSetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination. The argument '{nameof(indexerSetterParameters)}' has items, but the '{nameof(explicitSetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe an indexer setter accessor.");
                }

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
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
            this.HasExplicitGetPropertyAccessor = explicitGetterImplementationMethodDescriptor is not null;
            this.HasExplicitSetPropertyAccessor = explicitSetterImplementationMethodDescriptor is not null;
            this.IsIndexerProperty = this.IndexerGetterParameters.HasItems || this.IndexerSetterParameters.HasItems;
            this.IndexerGetterMethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IndexerSetterMethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._explicitGetterImplementationMethodDescriptor = explicitGetterImplementationMethodDescriptor ?? default;
            this._explicitSetterImplementationMethodDescriptor = explicitSetterImplementationMethodDescriptor ?? default;
            this.IsAnonymous = true;
            this._propertyInfo = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CacheKeyPropertyDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="CacheKeyPropertyDescriptor"/> is used to provide information for property symbols of which the caller does not have a direct representation <see cref="PropertyInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the property is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the interface type as the <paramref name="declaringTypeHandle"/> parameter (it's crucial to provide the interface type as the declaring type).
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.
        /// <para/>If the property is an explicit interface implementation, ensure to provide the interface type as the declaring type handle.
        /// </param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="declaredPropertyAccessor">Specifies the accessor that the property declares. Can't be <see cref="PropertyAccessor.None"/> or <see cref="PropertyAccessor.Undefined"/>.</param>
        /// <param name="indexerGetterParameters">The list of parameters for the anonymous indexer getter. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="indexerSetterParameters">The list of parameters for the anonymous indexer setter. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property. For normal properties, this parameter is ignored.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>. If set to <see langword="true"/>, the <paramref name="declaringTypeHandle"/> must represent an interface type.
        /// </param>
        /// <param name="explicitGetterImplementationMethodDescriptor"></param>
        /// <param name="explicitSetterImplementationMethodDescriptor"></param>
        /// <returns>A new instance of <see cref="CacheKeyPropertyDescriptor"/> representing the specified anonymous property.</returns>
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
        public CacheKeyPropertyDescriptor(RuntimeTypeHandle declaringTypeHandle,
        string propertyName,
        PropertyAccessor declaredPropertyAccessor,
        MethodParameterInfoList? indexerGetterParameters,
        MethodParameterInfoList? indexerSetterParameters,
        bool isExplicitInterfaceImplementation,
        CacheKeyMethodDescriptor? explicitGetterImplementationMethodDescriptor,
        CacheKeyMethodDescriptor? explicitSetterImplementationMethodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(propertyName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessor>(declaredPropertyAccessor);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                declaredPropertyAccessor,
                [PropertyAccessor.None, PropertyAccessor.Undefined],
                $"invalid argument '{nameof(declaredPropertyAccessor)}'. A property must declare at least a getter or a setter.");

            this.IsReadableProperty = (declaredPropertyAccessor & PropertyAccessor.Get) != 0;
            this.IsWriteableProperty = (declaredPropertyAccessor & PropertyAccessor.Set) != 0;

            this.IndexerGetterMethodParameterInfoList = indexerGetterParameters.OrEmpty();
            this.IndexerSetterMethodParameterInfoList = indexerSetterParameters.OrEmpty();

            if (isExplicitInterfaceImplementation)
            {
                if (explicitGetterImplementationMethodDescriptor is null && explicitSetterImplementationMethodDescriptor is null)
                {
                    throw new ArgumentException($"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' is set to 'true', but neither a '{nameof(explicitGetterImplementationMethodDescriptor)}' nor a '{nameof(explicitSetterImplementationMethodDescriptor)}' is provided. Reason: At least one accessor method descriptor must be provided for a valid property.");
                }

                if (this.IndexerGetterMethodParameterInfoList.HasItems ^ explicitGetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination. The argument '{nameof(indexerGetterParameters)}' has items, but the '{nameof(explicitGetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe an indexer getter accessor.");
                }

                if (this.IndexerSetterMethodParameterInfoList.HasItems ^ explicitSetterImplementationMethodDescriptor is not null)
                {
                    throw new ArgumentException($"Invalid argument combination. The argument '{nameof(indexerSetterParameters)}' has items, but the '{nameof(explicitSetterImplementationMethodDescriptor)}' is null. Or vice versa. Reason: Both arguments must be provided to describe an indexer setter accessor.");
                }

                Type? declaringType = Type.GetTypeFromHandle(declaringTypeHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    declaringType,
                    nameof(declaringTypeHandle),
                    $"The declaring type represented by the argument '{nameof(declaringTypeHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
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
            this.DeclaredPropertyAccessor = declaredPropertyAccessor;
            this.HasExplicitGetPropertyAccessor = explicitGetterImplementationMethodDescriptor is not null;
            this.HasExplicitSetPropertyAccessor = explicitSetterImplementationMethodDescriptor is not null;
            this.IsIndexerProperty = this.IndexerGetterMethodParameterInfoList.HasItems || this.IndexerSetterMethodParameterInfoList.HasItems;
            this.IndexerGetterMethodParameterInfoList = indexerGetterParameters;
            this.IndexerSetterMethodParameterInfoList = indexerSetterParameters;
            this.IndexerGetterParameters = ParameterList.Empty;
            this.IndexerSetterParameters = ParameterList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this._explicitGetterImplementationMethodDescriptor = explicitGetterImplementationMethodDescriptor ?? default;
            this._explicitSetterImplementationMethodDescriptor = explicitSetterImplementationMethodDescriptor ?? default;
            this.IsAnonymous = true;
            this._propertyInfo = null;
        }

        public RuntimeTypeHandle DeclaringTypeHandle { get; init; }
        public string PropertyName { get; init; }
        public PropertyAccessor DeclaredPropertyAccessor { get; }
        public ParameterList IndexerGetterParameters { get; init; }
        public ParameterList IndexerSetterParameters { get; init; }
        public MethodParameterInfoList IndexerGetterMethodParameterInfoList { get; init; }
        public MethodParameterInfoList IndexerSetterMethodParameterInfoList { get; init; }
        public bool IsExplicitInterfaceImplementation { get; init; }
        public bool HasExplicitGetPropertyAccessor { get; init; }
        public bool HasExplicitSetPropertyAccessor { get; init; }
        public bool IsIndexerProperty { get; init; }
        public bool IsAnonymous { get; }
        private readonly CacheKeyMethodDescriptor _explicitGetterImplementationMethodDescriptor;
        public CacheKeyMethodDescriptor ExplicitGetterImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsReadableProperty
                ? this._explicitGetterImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitGetterImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly CacheKeyMethodDescriptor _explicitSetterImplementationMethodDescriptor;
        public CacheKeyMethodDescriptor ExplicitSetterImplementationMethodDescriptor
            => this.IsExplicitInterfaceImplementation && this.IsWriteableProperty
                ? this._explicitSetterImplementationMethodDescriptor
                : throw new InvalidOperationException($"The property '{nameof(this.ExplicitSetterImplementationMethodDescriptor)}' cannot be accessed for implicit property implementations or write-only properties.");

        private readonly PropertyInfo? _propertyInfo;
        public PropertyInfo PropertyInfo
            => this.IsAnonymous
                ? throw new InvalidOperationException($"The property '{nameof(this.PropertyInfo)}' cannot be accessed for an anonymous property descriptor.")
                : this._propertyInfo!;

        public bool IsReadableProperty { get; }
        public bool IsWriteableProperty { get; }

        public bool Equals(CacheKeyPropertyDescriptor other)
            => this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.PropertyName.Equals(other.PropertyName, StringComparison.Ordinal)
            && this.IndexerGetterParameters.Equals(other.IndexerGetterParameters)
            && this.IndexerSetterParameters.Equals(other.IndexerSetterParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.IndexerGetterMethodParameterInfoList.Equals(other.IndexerGetterMethodParameterInfoList)
            && this.IndexerSetterMethodParameterInfoList.Equals(other.IndexerSetterMethodParameterInfoList)
            && this.HasExplicitGetPropertyAccessor.Equals(other.HasExplicitGetPropertyAccessor)
            && this.HasExplicitSetPropertyAccessor.Equals(other.HasExplicitSetPropertyAccessor)
            && this.ExplicitGetterImplementationMethodDescriptor.Equals(other.ExplicitGetterImplementationMethodDescriptor)
            && this.ExplicitSetterImplementationMethodDescriptor.Equals(other.ExplicitSetterImplementationMethodDescriptor)
            && this.IsIndexerProperty == other.IsIndexerProperty
            && this.IsAnonymous == other.IsAnonymous
            && ReferenceEquals(this._propertyInfo, other._propertyInfo)
            && this.IsReadableProperty == other.IsReadableProperty
            && this.IsWriteableProperty == other.IsWriteableProperty;

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
            hasCode.Add(this.HasExplicitGetPropertyAccessor);
            hasCode.Add(this.HasExplicitSetPropertyAccessor);
            hasCode.Add(this.ExplicitGetterImplementationMethodDescriptor);
            hasCode.Add(this.ExplicitSetterImplementationMethodDescriptor);
            hasCode.Add(this.IsIndexerProperty);
            hasCode.Add(this.IsReadableProperty);
            hasCode.Add(this.IsWriteableProperty);
            hasCode.Add(this.IsAnonymous);
            hasCode.Add(this._propertyInfo);

            return hasCode.ToHashCode();
        }

        public static bool operator ==(CacheKeyPropertyDescriptor left, CacheKeyPropertyDescriptor right)
            => left.Equals(right);
        public static bool operator !=(CacheKeyPropertyDescriptor left, CacheKeyPropertyDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is CacheKeyPropertyDescriptor other && Equals(other);
    }

    /// <summary>
    /// A descriptor that provides information about the member that declares the parameter for an anonymous parameter cache key.
    /// </summary>
    internal readonly struct CacheKeyParameterMemberDescriptor : IEquatable<CacheKeyParameterMemberDescriptor>
    {
        /// <summary>
        /// Constructs a descriptor that provides information about the member that declares the parameter for an anonymous parameter cache key.
        /// </summary>
        /// <param name="memberGenericMethodParameters">Optional. The list of generic method parameters for the anonymous parameter type.<para/>
        /// Can be <see cref="MethodParameterInfoList.Empty"/> to indicate a non-generic parameter or unknown. Providing <paramref name="memberGenericMethodParameters"/> helps to avoid ambiguity.</param>
        /// <param name="memberName">
        /// Optional. The name of the member that declares the parameter. Providing this information can help to avoid ambiguity and improve lookup performance.
        /// <para/>Will be ignored when <paramref name="parameterizedSymbolKind"/> is <see cref="ParameterizedSymbolKind.MemberConstructor"/> or when the <paramref name="parameterizedSymbolKind"/> indicates an indexer property (<see cref="ParameterizedSymbolKind.MemberIndexerPropertyGet"/> or <see cref="ParameterizedSymbolKind.MemberIndexerPropertySet"/>).
        /// </param>
        /// <param name="memberTypeHandle">Optional. The runtime type handle representing the return type of a method or property type of a property.</param>
        /// <param name="memberParameterCount">Optional. The number of parameters for the member that declares the parameter. Provide to narrow down ambiguity and improve performance.</param>
        /// <param name="parameterizedSymbolKind">Optional. Provides a hint about the kind of member that the parameter belongs to. Should be provided too improve efficiency of the key and to avoid ambiguity.</param>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="memberHandle">Optional. For best performance provide a <see cref="RuntimeMethodHandle"/> to the method or constructor that defines the parameter. 
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is the default value.</exception>
        public CacheKeyParameterMemberDescriptor(RuntimeTypeHandle declaringTypeHandle,
            RuntimeMethodHandle? memberHandle = null,
            string? memberName = null,
            int memberParameterCount = SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
            ParameterizedSymbolKind parameterizedSymbolKind = ParameterizedSymbolKind.Undefined,
            TypeList? memberGenericMethodParameters = null,
            RuntimeTypeHandle? memberTypeHandle = null)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterizedSymbolKind>(parameterizedSymbolKind);
            if (parameterizedSymbolKind is ParameterizedSymbolKind.MemberIndexerPropertyGet
                or ParameterizedSymbolKind.MemberIndexerPropertySet
                or ParameterizedSymbolKind.MemberIndexerPropertyGetOrSet
                or ParameterizedSymbolKind.MemberConstructor)
            {
                // For indexer properties and constructors we allow empty or whitespace names i.e. ignore provided value.
                memberName = string.Empty;
            }

            this.DeclaringMemberName = memberName ?? string.Empty;
            this.MemberParameterCount = memberParameterCount;
            this.ParameterizedMemberKind = parameterizedSymbolKind;
            this.MemberGenericMethodParameters = memberGenericMethodParameters ?? TypeList.Empty;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.MemberHandle = memberHandle ?? default;
            this.MemberTypeHandle = memberTypeHandle ?? default;
        }

        public bool HasDeclaringMemberName
            => !string.IsNullOrWhiteSpace(this.DeclaringMemberName);

        public bool HasMemberGenericMethodParameters
            => !this.MemberGenericMethodParameters.IsEmpty;

        public bool HasMemberHandle
            => !this.MemberHandle.Equals(default);

        public bool HasMemberParameterCount
            => this.MemberParameterCount > SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition;

        public bool HasDeclaringTypeHandle
            => !this.DeclaringTypeHandle.Equals(default);

        public bool HasParameterizedMemberKind
            => this.ParameterizedMemberKind != ParameterizedSymbolKind.Undefined;

        public bool HasMemberTypeHandle
            => !this.MemberTypeHandle.Equals(default);

        public string DeclaringMemberName { get; init; }
        public int MemberParameterCount { get; init; }
        public ParameterizedSymbolKind ParameterizedMemberKind { get; init; }
        public TypeList MemberGenericMethodParameters { get; init; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; init; }

        /// <summary>
        /// The handle for the return type of a method or the property type of a property.
        /// </summary>
        public RuntimeTypeHandle MemberTypeHandle { get; init; }
        public RuntimeMethodHandle MemberHandle { get; }

        public bool Equals(CacheKeyParameterMemberDescriptor other) => this.DeclaringMemberName.Equals(other.DeclaringMemberName, StringComparison.Ordinal)
            && this.MemberParameterCount == other.MemberParameterCount
            && this.ParameterizedMemberKind == other.ParameterizedMemberKind
            && this.MemberGenericMethodParameters.Equals(other.MemberGenericMethodParameters)
            && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.MemberHandle.Equals(other.MemberHandle)
            && this.MemberTypeHandle.Equals(other.MemberTypeHandle);

        public override int GetHashCode() => HashCode.Combine(
            this.DeclaringMemberName,
            this.MemberParameterCount,
            this.ParameterizedMemberKind,
            this.MemberGenericMethodParameters,
            this.DeclaringTypeHandle,
            this.MemberHandle,
            this.MemberTypeHandle);

        public static bool operator ==(CacheKeyParameterMemberDescriptor left, CacheKeyParameterMemberDescriptor right)
            => left.Equals(right);
        public static bool operator !=(CacheKeyParameterMemberDescriptor left, CacheKeyParameterMemberDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is CacheKeyParameterMemberDescriptor other && Equals(other);
    }
}
