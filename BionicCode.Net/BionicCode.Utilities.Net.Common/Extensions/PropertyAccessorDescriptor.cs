namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A descriptor that provides information about an well-known property accessor, where the caller does has a direct <see cref="MethodInfo"/> representation of the method symbol.
    /// </summary>
    /// <remarks>The <see cref="PropertyAccessorDescriptor"/> is used to provide information for well-known property accessor method symbols, which is when the caller has the direct <see cref="MethodInfo"/> representation.
    /// <para/>Important: well-known descriptors are preferred over anonymous descriptors when the <see cref="MethodInfo"/> is available to ensure maximum accuracy and performance.
    /// </remarks>
    internal readonly struct PropertyAccessorDescriptor : IEquatable<PropertyAccessorDescriptor>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessorDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="PropertyAccessorDescriptor"/> is used to provide information about a property accessor method symbol of which the caller does not have a direct <see cref="MethodInfo"/> representation and instead only signature information is available.
        /// <para/>If the property is an explicit interface implementation, then the <paramref name="methodHandle"/> must represent an interface type.
        /// </remarks>
        /// <param name="methodHandle">The runtime type handle representing the declaring type of the anonymous property accessor method.
        /// <para/>If the property is an explicit interface implementation, then the value must not be obtained from an interface type but from the implementing type instead.
        /// </param>
        /// <param name="propertyName">The name of the property that the anonymous accessor method is associated with. Cannot be null, empty, or consist only of white-space characters unless the property is an indexer. For indexer properties this parameter is ignored.</param>
        /// <param name="accessorKind">This parameter specifies the kind of the accessor.
        /// <para/>This parameter is required and is not allowed to be <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</param>
        /// <param name="indexerParameters">For indexer properties, this parameter specifies the parameter list for the accessor specified by <paramref name="accessorKind"/>. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters. This parameter is ignored for non-indexer properties and events.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="methodHandle"/> must represent the declaring interface type.</param>
        /// <returns>A new instance of <see cref="PropertyAccessorDescriptor"/> representing the specified anonymous property accessor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodHandle"/> is <see langword="default"/> or <paramref name="propertyName"/> is <see langword="null"/> (or only consists of white-space characters or  is an empty string).</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="methodHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="accessorKind"/> has a value that is not defined by the <see cref="PropertyAccessorKind"/> enum.</item>
        /// <item>Is also thrown when <paramref name="accessorKind"/> has value <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public PropertyAccessorDescriptor(
            RuntimeMethodHandle methodHandle,
            bool isExplicitInterfaceImplementation,
            ParameterList? indexerParameters,
            string? propertyName,
            PropertyAccessors accessorKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodHandle);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(accessorKind);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                accessorKind,
                [PropertyAccessors.None, PropertyAccessors.GetAndSet],
                nameof(accessorKind),
                $"Invalid argument '{nameof(accessorKind)}'. The argument '{nameof(accessorKind)}' has an undefined value. The value '{accessorKind}' is not allowed.");

            indexerParameters = indexerParameters.OrEmpty();
            this.IsIndexerPropertyAccessor = indexerParameters.HasItems;
            this.IsPropertyAccessor = !this.IsIndexerPropertyAccessor;

            if (this.IsIndexerPropertyAccessor)
            {
                propertyName = string.Empty;
            }
            else
            {
                ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(propertyName);
            }

            if (isExplicitInterfaceImplementation)
            {
                MethodBase? accessor = MethodInfo.GetMethodFromHandle(methodHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    accessor,
                    nameof(methodHandle),
                    $"The method represented by the argument '{nameof(methodHandle)}' could not be resolved.");
                Type? declaringType = accessor?.DeclaringType;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    accessor,
                    nameof(declaringType),
                    $"The declaring type of the argument '{nameof(methodHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(methodHandle)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type of the accessor method represented by the argument '{nameof(methodHandle)}' is an interface type. Reason: Only non-interface types can provide the explicit implementation.");
            }

            this.IndexerParameters = indexerParameters.OrEmpty();
            this.MethodHandle = methodHandle;
            this.PropertyName = propertyName;
            this.PropertyAccessorKind = accessorKind;
            this.MethodParameterInfoList = MethodParameterInfoList.Empty;
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAccessorDescriptor"/> struct.
        /// </summary>
        /// <remarks>The <see cref="PropertyAccessorDescriptor"/> is used to provide information about a property or event accessor method symbol of which the caller does not have a direct <see cref="MethodInfo"/> representation and instead only signature information is available.
        /// <para/>If the property or event is an explicit interface implementation, then the <paramref name="methodHandle"/> must represent an interface type.
        /// </remarks>
        /// <param name="methodHandle">The runtime type handle representing the declaring type of the anonymous property or event accessor method.
        /// <para/>If the property is an explicit interface implementation, then the value must be obtaind an interface type.
        /// </param>
        /// <param name="propertyName">The name of the property or event that the anonymous accessor method is associated with. Cannot be null, empty, or consist only of white-space characters unless the property is an indexer. For indexer properties this parameter is ignored.</param>
        /// <param name="accessorKind">This parameter specifies the kind of the accessor.
        /// <para/>This parameter is required and is not allowed to be <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</param>
        /// <param name="indexerParameters">For indexer properties, this parameter specifies the parameter list for the accessor specified by <paramref name="accessorKind"/>. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters. This parameter is ignored for non-indexer properties and events.</param>
        /// <param name="isExplicitInterfaceImplementation"><see langword="true"/> if the property or event is an explicit interface implementation; otherwise, <see langword="false"/>.
        /// <para/>If set to <see langword="true"/>, then the <paramref name="methodHandle"/> must represent the declaring interface type.</param>
        /// <returns>A new instance of <see cref="PropertyAccessorDescriptor"/> representing the specified anonymous property or event accessor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="methodHandle"/> is <see langword="default"/> or <paramref name="propertyName"/> is <see langword="null"/> (or only consists of white-space characters or  is an empty string).</exception>
        /// <exception cref="ArgumentException">Thrown when
        /// <list type="bullet">
        /// <item>Is also thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but <paramref name="methodHandle"/> is not an interface type.</item>
        /// <item>Is also thrown when <paramref name="accessorKind"/> has a value that is not defined by the <see cref="PropertyAccessorKind"/> enum.</item>
        /// <item>Is also thrown when <paramref name="accessorKind"/> has value <see cref="PropertyAccessors.None"/> or <see cref="PropertyAccessors.None"/>.</item>
        /// </list>
        /// </exception>
        public PropertyAccessorDescriptor(
            RuntimeMethodHandle methodHandle,
            bool isExplicitInterfaceImplementation,
            MethodParameterInfoList? indexerParameters,
            string? propertyName,
            PropertyAccessors accessorKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodHandle);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(accessorKind);
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                accessorKind,
                [PropertyAccessors.None, PropertyAccessors.GetAndSet],
                nameof(accessorKind),
                $"Invalid argument '{nameof(accessorKind)}'. The argument '{nameof(accessorKind)}' has an undefined value. The value '{accessorKind}' is not allowed.");

            indexerParameters = indexerParameters.OrEmpty();
            this.IsIndexerPropertyAccessor = indexerParameters.HasItems;
            this.IsPropertyAccessor = !this.IsIndexerPropertyAccessor;

            if (this.IsIndexerPropertyAccessor)
            {
                propertyName = string.Empty;
            }
            else
            {
                ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(propertyName);
            }

            if (isExplicitInterfaceImplementation)
            {
                MethodBase? accessor = MethodInfo.GetMethodFromHandle(methodHandle);
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    accessor,
                    nameof(methodHandle),
                    $"The method represented by the argument '{nameof(methodHandle)}' could not be resolved.");
                Type? declaringType = accessor?.DeclaringType;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    accessor,
                    nameof(declaringType),
                    $"The declaring type of the argument '{nameof(methodHandle)}' could not be resolved.");
                ArgumentExceptionAdvanced.ThrowIfTrue(declaringType!.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(methodHandle)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type of the accessor method represented by the argument '{nameof(methodHandle)}' is an interface type. Reason: Only non-interface types can provide the explicit implementation.");
            }

            this.IndexerParameters = ParameterList.Empty;
            this.MethodHandle = methodHandle;
            this.PropertyName = propertyName;
            this.PropertyAccessorKind = accessorKind;
            this.MethodParameterInfoList = indexerParameters.OrEmpty();
            this.IsExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.IsAnonymous = false;
        }

        public RuntimeMethodHandle MethodHandle { get; }
        public string PropertyName { get; }
        public bool IsIndexerPropertyAccessor { get; }
        public bool IsPropertyAccessor { get; }
        public PropertyAccessors PropertyAccessorKind { get; }
        public MethodParameterInfoList MethodParameterInfoList { get; }
        public ParameterList IndexerParameters { get; }
        public bool IsExplicitInterfaceImplementation { get; }
        public bool IsAnonymous { get; }

        public bool Equals(PropertyAccessorDescriptor other)
            => this.MethodHandle.Equals(other.MethodHandle)
            && this.PropertyName.Equals(other.PropertyName, StringComparison.Ordinal)
            && this.IndexerParameters.Equals(other.IndexerParameters)
            && this.IsExplicitInterfaceImplementation.Equals(other.IsExplicitInterfaceImplementation)
            && this.MethodParameterInfoList.Equals(other.MethodParameterInfoList)
            && this.PropertyAccessorKind.Equals(other.PropertyAccessorKind)
            && this.IsPropertyAccessor.Equals(other.IsPropertyAccessor)
            && this.IsIndexerPropertyAccessor.Equals(other.IsIndexerPropertyAccessor)
            && this.IsAnonymous == other.IsAnonymous;

        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.MethodHandle);
            hashCode.Add(this.PropertyName);
            hashCode.Add(this.IndexerParameters);
            hashCode.Add(this.IsIndexerPropertyAccessor);
            hashCode.Add(this.IsExplicitInterfaceImplementation);
            hashCode.Add(this.MethodParameterInfoList);
            hashCode.Add(this.PropertyAccessorKind);
            hashCode.Add(this.IsPropertyAccessor);
            hashCode.Add(this.IsAnonymous);

            return hashCode.ToHashCode();
        }

        public static bool operator ==(PropertyAccessorDescriptor left, PropertyAccessorDescriptor right)
            => left.Equals(right);
        public static bool operator !=(PropertyAccessorDescriptor left, PropertyAccessorDescriptor right)
            => !(left == right);

        public override bool Equals(object obj)
            => obj is PropertyAccessorDescriptor other && Equals(other);
    }
}
