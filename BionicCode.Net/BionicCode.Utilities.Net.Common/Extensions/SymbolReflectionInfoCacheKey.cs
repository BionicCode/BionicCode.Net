namespace BionicCode.Utilities.Net
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Represents a unique cache key for symbol metadata, encapsulating identifying information for types, members, and
    /// parameters used in reflection or symbol analysis scenarios.
    /// </summary>
    /// <remarks>This struct is used to efficiently identify and compare symbols such as types, methods,
    /// properties, events, fields, constructors, and parameters based on their metadata handles and characteristics. It
    /// is suitable for use as a key in caching mechanisms where symbol identity and equivalence are important, such as
    /// symbol information lookups or metadata-based caching. Instances are immutable and can be compared for
    /// equality.</remarks>
    internal readonly partial struct SymbolReflectionInfoCacheKey : IEquatable<SymbolReflectionInfoCacheKey>
    {
        /// <summary>
        /// Represents an unknown or unspecified parameter count.
        /// </summary>
        /// <remarks>Use this constant to indicate that the number of parameters is not known or cannot be
        /// determined. This value is typically used in APIs where the parameter count is optional or
        /// variable.</remarks>
        public const int UnknownParameterCountOrPosition = -1;

        // TODO::Throw exceptions based on SymbolKind and  IsAnonymousKey when properties are accessed that are not valid for the specific SymbolKind.

        /// <summary>
        /// Gets the name of the symbol represented by this instance.
        /// </summary>
        /// <value>The name of the symbol, such as the method name, property name, event name, field name, or type name.</value>
        public readonly string SymbolName { get; }

        /// <summary>
        /// Gets the kind of symbol represented by this instance.
        /// </summary>
        /// <value>The kind of symbol, such as type, method, property, event, field, constructor, or parameter.</value>
        public readonly SymbolKind SymbolKind { get; }

        private readonly WellKnownParameterDescriptor _wellKnownParameterDescriptor;
        public WellKnownParameterDescriptor CacheKeyParameterDescriptor => this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotWellKnownException<WellKnownParameterDescriptor>()
            : this.SymbolKind is SymbolKind.Parameter
                ? this._wellKnownParameterDescriptor
                : ThrowInvalidPropertyContextException<WellKnownParameterDescriptor>([SymbolKind.Parameter]);

        private readonly AnonymousParameterDescriptor _anonymousParameterDescriptor;
        public AnonymousParameterDescriptor AnonymousParameterDescriptor => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<AnonymousParameterDescriptor>()
            : this.SymbolKind is SymbolKind.Parameter
                ? this._anonymousParameterDescriptor
                : ThrowInvalidPropertyContextException<AnonymousParameterDescriptor>([SymbolKind.Parameter]);

        private readonly WellKnownPropertyDescriptor _wellKnownPropertyDescriptor;
        public WellKnownPropertyDescriptor WellKnownPropertyDescriptor => this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotWellKnownException<WellKnownPropertyDescriptor>()
            : this.SymbolKind is SymbolKind.MemberProperty
                ? this._wellKnownPropertyDescriptor
                : ThrowInvalidPropertyContextException<WellKnownPropertyDescriptor>([SymbolKind.MemberProperty]);

        private readonly AnonymousPropertyDescriptor _anonymousPropertyDescriptor;
        public AnonymousPropertyDescriptor AnonymousPropertyDescriptor => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<AnonymousPropertyDescriptor>()
            : this.SymbolKind is SymbolKind.MemberProperty
                ? this._anonymousPropertyDescriptor
                : ThrowInvalidPropertyContextException<AnonymousPropertyDescriptor>([SymbolKind.MemberProperty]);

        private readonly WellKnownMethodOrConstructorDescriptor _wellKnownMethodDescriptor;
        public WellKnownMethodOrConstructorDescriptor CacheKeyMethodDescriptor => this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotWellKnownException<WellKnownMethodOrConstructorDescriptor>()
            : this.SymbolKind is SymbolKind.MemberMethod
                ? this._wellKnownMethodDescriptor
                : ThrowInvalidPropertyContextException<WellKnownMethodOrConstructorDescriptor>([SymbolKind.MemberMethod]);

        private readonly AnonymousMethodOrConstructorDescriptor _anonymousMethodDescriptor;
        public AnonymousMethodOrConstructorDescriptor CacheKeyAnonymousMethodDescriptor => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<AnonymousMethodOrConstructorDescriptor>()
            : this.SymbolKind is SymbolKind.MemberMethod
                ? this._anonymousMethodDescriptor
                : ThrowInvalidPropertyContextException<AnonymousMethodOrConstructorDescriptor>([SymbolKind.MemberMethod]);

        private readonly WellKnownFieldDescriptor _wellKnownFieldDescriptor;
        public WellKnownFieldDescriptor CacheKeyFieldDescriptor => this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotWellKnownException<WellKnownFieldDescriptor>()
            : this.SymbolKind is SymbolKind.MemberField
                ? this._wellKnownFieldDescriptor
                : ThrowInvalidPropertyContextException<WellKnownFieldDescriptor>([SymbolKind.MemberField]);

        private readonly AnonymousFieldDescriptor _anonymousFieldDescriptor;
        public AnonymousFieldDescriptor CacheKeyAnonymousFieldDescriptor => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<AnonymousFieldDescriptor>()
            : this.SymbolKind is SymbolKind.MemberField
                ? this._anonymousFieldDescriptor
                : ThrowInvalidPropertyContextException<AnonymousFieldDescriptor>([SymbolKind.MemberField]);

        //private readonly PropertyAccessors _indexerPropertyAccessor;
        //public PropertyAccessors IndexerPropertyAccessor => !this.IsAnonymousSymbolKey
        //    ? ThrowCurrentInstanceIsNotAnonymousException<PropertyAccessors>()
        //    : this.SymbolKind.EqualsAny([SymbolKind.MemberProperty])
        //        ? this._indexerPropertyAccessor
        //        : ThrowInvalidPropertyContextException<PropertyAccessors>([SymbolKind.MemberProperty]);

        private readonly bool _isExplicitInterfaceImplementation;
        public bool IsExplicitInterfaceImplementation => this.SymbolKind is SymbolKind.MemberProperty
            ? this._isExplicitInterfaceImplementation
            : ThrowInvalidPropertyContextException<bool>([SymbolKind.MemberProperty]);

        public bool IsAnonymousSymbolKey { get; }

        private readonly int _hashCode;

        private SymbolReflectionInfoCacheKey(string name,
            SymbolKind symbolKind,
            WellKnownParameterDescriptor wellKnownParameterDescriptor,
            AnonymousParameterDescriptor anonymousParameterDescriptor,
            WellKnownMethodOrConstructorDescriptor wellKnownMethodDescriptor,
            AnonymousMethodOrConstructorDescriptor anonymousMethodDescriptor,
            WellKnownPropertyDescriptor wellKnownPropertyDescriptor,
            AnonymousPropertyDescriptor anonymousPropertyDescriptor,
            WellKnownFieldDescriptor wellKnownFieldDescriptor,
            AnonymousFieldDescriptor anonymousFieldDescriptor,
            bool isAnonymousSymbolKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));

            this.SymbolKind = symbolKind;
            this._wellKnownParameterDescriptor = wellKnownParameterDescriptor;
            this._anonymousParameterDescriptor = anonymousParameterDescriptor;
            this._wellKnownMethodDescriptor = wellKnownMethodDescriptor;
            this._anonymousMethodDescriptor = anonymousMethodDescriptor;
            this._wellKnownPropertyDescriptor = wellKnownPropertyDescriptor;
            this._anonymousPropertyDescriptor = anonymousPropertyDescriptor;
            this._wellKnownFieldDescriptor = wellKnownFieldDescriptor;
            this._anonymousFieldDescriptor = anonymousFieldDescriptor;
            this.SymbolName = name;
            this.IsAnonymousSymbolKey = isAnonymousSymbolKey;

            this._hashCode = ComputeHashCode();
        }

        /// <summary>
        /// Creates a cache key for an event symbol.
        /// </summary>
        /// <param name="eventInfo">The event information.</param>
        /// <param name="isExplicitInterfaceImplementation">Indicates whether the event is an explicit interface implementation. If <see langword="true" /> the <paramref name="eventInfo"/> must be obtained from the declaring interface type i.e. the <see cref="MemberInfo.DeclaringType"/> must return an interface type.</param>
        /// <returns>The unique cache key for the event symbol.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventInfo"/> or its declaring type is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> but the declaring type of <paramref name="eventInfo"/> is not an interface type.</exception>"
        public static SymbolReflectionInfoCacheKey CreateForEvent(EventInfo eventInfo, bool isExplicitInterfaceImplementation)
        {
            ArgumentNullException.ThrowIfNull(eventInfo, nameof(eventInfo));

            Type? declaringType = eventInfo.DeclaringType;
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                declaringType,
                nameof(eventInfo),
                $"The declaring type represented by the argument '{nameof(eventInfo)}' could not be resolved.");

            if (isExplicitInterfaceImplementation)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(declaringType is not null && declaringType.IsInterface,
                    nameof(isExplicitInterfaceImplementation),
                    $"Invalid argument '{nameof(isExplicitInterfaceImplementation)}'. The argument '{nameof(isExplicitInterfaceImplementation)}' returns 'true' while the declaring type represented by the argument '{nameof(eventInfo)}' is not an interface. Reason: Only interface types can provide the declaration of explicit interface implementations.");
            }

            RuntimeTypeHandle declaringTypeHandle = declaringType!.TypeHandle;
            Type? eventHandlerType = eventInfo.EventHandlerType;
            if (eventHandlerType is null)
            {
                throw new NotSupportedException($"The argument '{nameof(eventInfo)}' provides a '{nameof(EventInfo)}' instance which does not have an event handler type. Events without an event handler type are not supported.");
            }

            RuntimeTypeHandle eventDelegateTypeHandle = eventHandlerType.TypeHandle;

            return new SymbolReflectionInfoCacheKey(eventInfo.Name,
                declaringTypeHandle,
                eventDelegateTypeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberEvent,
                default,
                default,
                default,
                default,
                isExplicitInterfaceImplementation,
                false);
        }

        /// <summary>
        /// Creates a cache key for a well-known property symbol.
        /// </summary>
        /// <param name="propertyDescriptor">The <see cref="Net.WellKnownPropertyDescriptor"/> that describes a well-known property (which is where the <see cref="PropertyInfo"/> is available) or an anonymous property (which when only signature information is available).</param>
        /// <remarks>This method creates a unique cache key for well-known or anonymous property symbols, including indexer properties.
        /// <para/>For maximum performance and zero ambiguity, always prefer to create property cache keys using well-known <see cref="PropertyInfo"/> instances via the <see cref="Net.WellKnownPropertyDescriptor"/>.</remarks>
        /// <returns>The unique cache key for the well-known property symbol.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="propertyDescriptor"/> or its declaring type is <see langword="default"/>.</item>
        /// </list>
        /// </exception>
        public static SymbolReflectionInfoCacheKey CreateForWellknownProperty(WellKnownPropertyDescriptor propertyDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

            return new SymbolReflectionInfoCacheKey(
                propertyDescriptor.PropertyInfo.Name,
                SymbolKind.MemberProperty,
                default,
                default,
                default,
                default,
                propertyDescriptor,
                default,
                default,
                default,
                propertyDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a cache key for an anonymous property symbol.
        /// </summary>
        /// <param name="propertyDescriptor">The <see cref="Net.AnonymousPropertyDescriptor"/> that describes an anonymous property (which is where the <see cref="PropertyInfo"/> is not available  and instead only the signature information is available).</param>
        /// <remarks>This method creates a unique cache key for anonymous property symbols, including indexer properties.
        /// <para/>For maximum performance and zero ambiguity, always prefer to create property cache keys using well-known <see cref="PropertyInfo"/> instances via the <see cref="Net.WellKnownPropertyDescriptor"/>.</remarks>
        /// <returns>The unique cache key for the anonymous property symbol.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="propertyDescriptor"/> or its declaring type is <see langword="default"/>.</item>
        /// </list>
        /// </exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousProperty(AnonymousPropertyDescriptor propertyDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

            return new SymbolReflectionInfoCacheKey(
                propertyDescriptor.PropertyName,
                SymbolKind.MemberProperty,
                default,
                default,
                default,
                default,
                default,
                propertyDescriptor,
                default,
                default,
                propertyDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a cache key for a well-known method symbol.
        /// </summary>
        /// <param name="methodDescriptor">The <see cref="Net.WellKnownMethodOrConstructorDescriptor"/> that describes a well-known method (which is where the <see cref="MethodInfo"/> is available).</param>
        /// <remarks>This method creates a unique cache key for well-known or anonymous method symbols, including regular methods, property accessors, and event accessors.
        /// <para/>For maximum performance and zero ambiguity, always prefer to create method cache keys using well-known <see cref="MethodInfo"/> instances via the <see cref="Net.WellKnownMethodOrConstructorDescriptor"/>.</remarks>
        /// <returns>The unique cache key for the well-known method symbol.</returns>
        public static SymbolReflectionInfoCacheKey CreateForWellKnownMethod(WellKnownMethodOrConstructorDescriptor methodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

            return new SymbolReflectionInfoCacheKey(
                methodDescriptor.MethodName,
                SymbolKind.MemberMethod,
                default,
                default,
                methodDescriptor,
                default,
                default,
                default,
                default,
                default,
                methodDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a cache key for an anonymous method symbol.
        /// </summary>
        /// <param name="methodDescriptor">The <see cref="Net.AnonymousMethodOrConstructorDescriptor"/> that describes an anonymous method (which is where the <see cref="MethodInfo"/> is not available and instead only the signature information is available).</param>
        /// <remarks>This method creates a unique cache key for well-known or anonymous method symbols, including regular methods, property accessors, and event accessors.
        /// <para/>For maximum performance and zero ambiguity, always prefer to create method cache keys using well-known <see cref="MethodInfo"/> instances via the <see cref="Net.WellKnownMethodOrConstructorDescriptor"/>.</remarks>
        /// <returns>The unique cache key for the well-known method symbol.</returns>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousMethod(AnonymousMethodOrConstructorDescriptor methodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

            return new SymbolReflectionInfoCacheKey(
                methodDescriptor.MethodName,
                SymbolKind.MemberMethod,
                default,
                default,
                default,
                methodDescriptor,
                default,
                default,
                default,
                default,
                methodDescriptor.IsAnonymous);
        }

        public static SymbolReflectionInfoCacheKey CreateForType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            RuntimeTypeHandle typeHandle = type.TypeHandle;

            return new SymbolReflectionInfoCacheKey(type.FullName ?? type.Name,
                default,
                typeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Type,
                default,
                default,
                default,
                default,
                false,
                false);
        }

        public static SymbolReflectionInfoCacheKey CreateForWellKnownField(WellKnownFieldDescriptor fieldDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(fieldDescriptor);

            return new SymbolReflectionInfoCacheKey(fieldDescriptor.FieldName,
                SymbolKind.MemberField,
                default,
                default,
                default,
                default,
                default,
                default,
                fieldDescriptor,
                default,
                fieldDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous field using the specified <see cref="AnonymousFieldDescriptor"/>.
        /// </summary>
        /// <param name="fieldDescriptor">The <see cref="Net.AnonymousFieldDescriptor"/> that describes an anonymous field (which is where the <see cref="FieldInfo"/> is not available and instead only the signature information is available).</param>
        /// <remarks>This method is used to create a unique cache key for field symbols of which the caller does not have a direct representation <see cref="FieldInfo"/> and instead only signature information is available.</remarks>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous field.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldDescriptor"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousField(AnonymousFieldDescriptor fieldDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(fieldDescriptor);

            return new SymbolReflectionInfoCacheKey(fieldDescriptor.FieldName,
                SymbolKind.MemberField,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                fieldDescriptor,
                fieldDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a new cache key for a well-known constructor using the specified <see cref="WellKnownMethodOrConstructorDescriptor"/>.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.
        ///<para/>For maximum performance and zero ambiguity, always prefer to create constructor cache keys using well-known <see cref="ConstructorInfo"/> instances via the <see cref="Net.WellKnownMethodOrConstructorDescriptor"/>.</remarks>
        /// <param name="constructorDescriptor"></param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified well-known constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorDescriptor"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForWellKnownConstructor(WellKnownMethodOrConstructorDescriptor constructorDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(constructorDescriptor);

            return new SymbolReflectionInfoCacheKey(
                string.Empty,
                SymbolKind.MemberConstructor,
                default,
                default,
                constructorDescriptor,
                default,
                default,
                default,
                default,
                default,
                constructorDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous constructor using the specified <see cref="AnonymousMethodOrConstructorDescriptor"/>.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.
        ///<para/>For maximum performance and zero ambiguity, always prefer to create constructor cache keys using well-known <see cref="ConstructorInfo"/> instances via the <see cref="Net.WellKnownMethodOrConstructorDescriptor"/>.</remarks>
        /// <param name="constructorDescriptor"></param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="constructorDescriptor"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousConstructor(AnonymousMethodOrConstructorDescriptor constructorDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(constructorDescriptor);

            return new SymbolReflectionInfoCacheKey(
                string.Empty,
                SymbolKind.MemberConstructor,
                default,
                default,
                default,
                constructorDescriptor,
                default,
                default,
                default,
                default,
                constructorDescriptor.IsAnonymous);
        }

        public static SymbolReflectionInfoCacheKey CreateForWellKnownParameter(WellKnownParameterDescriptor parameterDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterDescriptor);

            return new SymbolReflectionInfoCacheKey(
                parameterDescriptor.ParameterName,
                SymbolKind.Parameter,
                parameterDescriptor,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                parameterDescriptor.IsAnonymous);
        }

        public static SymbolReflectionInfoCacheKey CreateForAnonymousParameter(AnonymousParameterDescriptor parameterDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterDescriptor);

            return new SymbolReflectionInfoCacheKey(
                parameterDescriptor.ParameterName,
                SymbolKind.Parameter,
                default,
                parameterDescriptor,
                default,
                default,
                default,
                default,
                default,
                default,
                parameterDescriptor.IsAnonymous);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous event using the specified declaring type, event name.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for event symbols of which the caller does not have a direct representation <see cref="EventInfo"/> and instead only signature information is available.
        /// <para/>
        /// If the event is an explicit interface implementation, ensure to set the <paramref name="isExplicitInterfaceImplementation"/> parameter to <see langword="true"/> and provide the interface type as the <paramref name="declaringTypeHandle"/> parameter (it's crucial to provide the interface type as the declaring type).
        /// </remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous event.
        /// <para/>If the event is an explicit interface implementation, this must be an interface type.
        /// </param>
        /// <param name="eventName">The name of the anonymous event. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="isExplicitInterfaceImplementation">Indicates whether the event is an explicit interface implementation. If set to <see langword="true"/>, the declaring type <paramref name="declaringTypeHandle"/> must be an interface type.</param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous event.</returns>
        /// <exception cref="ArgumentException">Thrown when
        /// <list>
        /// <item><paramref name="eventName"/> is null, empty, or consists only of white-space characters</item>
        /// <item>the provided <paramref name="isExplicitInterfaceImplementation"/> value is <see langword="true"/> but <paramref name="declaringTypeHandle"/> does not represent an interface type.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousEvent(RuntimeTypeHandle declaringTypeHandle, string eventName, bool isExplicitInterfaceImplementation)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);

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

            ArgumentException.ThrowIfNullOrWhiteSpace(eventName);

            return new SymbolReflectionInfoCacheKey(eventName,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberEvent,
                default,
                default,
                default,
                default,
                isExplicitInterfaceImplementation,
                true);
        }

        public override int GetHashCode()
            => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                if (this._hashCode != 0)
                {
                    return this._hashCode;
                }

                var hashCode = new HashCode();
                hashCode.Add(this.SymbolName);
                hashCode.Add(this._declaringTypeHandle);
                hashCode.Add(this._symbolTypeHandle);
                hashCode.Add(this._methodHandle);
                hashCode.Add(this._fieldHandle);
                hashCode.Add(this.SymbolKind);
                hashCode.Add(this._genericTypeParameterCount);
                hashCode.Add(this.IsAnonymousSymbolKey);
                hashCode.Add(this._anonymousParameterDescriptor);
                hashCode.Add(this._parameterMemberDescriptor);
                hashCode.Add(this._isExplicitInterfaceImplementation);

                foreach (ParameterData parameterData in this._parameterList)
                {
                    hashCode.Add(parameterData.ParameterTypeHandle);
                    hashCode.Add(parameterData.DeclaringTypeHandle);
                    hashCode.Add(parameterData.Position);
                }

                foreach (MethodParameterInfo parameterData in this._methodParameterInfoList)
                {
                    hashCode.Add(parameterData.DeclaringMemberDescriptor);
                    hashCode.Add(parameterData.ParameterDescriptor);
                }

                return hashCode.ToHashCode();
            }
        }

        public override bool Equals(object obj)
            => obj is SymbolReflectionInfoCacheKey other && Equals(other);

        public bool Equals(SymbolReflectionInfoCacheKey other) => this.SymbolName == other.SymbolName
            && this._declaringTypeHandle.Equals(other._declaringTypeHandle)
            && this._symbolTypeHandle.Equals(other._symbolTypeHandle)
            && this._methodHandle == other._methodHandle
            && this._fieldHandle == other._fieldHandle
            && this.SymbolKind == other.SymbolKind
            && this._genericTypeParameterCount == other._genericTypeParameterCount
            && this.IsAnonymousSymbolKey == other.IsAnonymousSymbolKey
            && this._anonymousParameterDescriptor == other._anonymousParameterDescriptor
            && this._parameterMemberDescriptor == other._parameterMemberDescriptor
            && this._parameterList.Equals(other._parameterList)
            && this._methodParameterInfoList.Equals(other._methodParameterInfoList)
            && this._isExplicitInterfaceImplementation == other._isExplicitInterfaceImplementation;

        public static bool operator ==(SymbolReflectionInfoCacheKey left, SymbolReflectionInfoCacheKey right) => left.Equals(right);
        public static bool operator !=(SymbolReflectionInfoCacheKey left, SymbolReflectionInfoCacheKey right) => !(left == right);

        [DoesNotReturn]
        private TResult ThrowInvalidPropertyContextException<TResult>(ReadOnlySpan<SymbolKind> allowedSymbolKinds, [CallerMemberName] string propertyName = null)
        {
            ArgumentExceptionAdvanced.ThrowIfTrue(allowedSymbolKinds.IsEmpty, nameof(allowedSymbolKinds), "At least one allowed symbol kind must be provided.");

            string allowedKinds = allowedSymbolKinds.JoinToString(kind => $"{typeof(SymbolKind).FullName}.{kind}", ", ");
            return allowedSymbolKinds.Length > 1
                ? throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(this.SymbolKind)}' returns any of the following values: {allowedKinds}.")
                : throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the property '{nameof(this.SymbolKind)}' returns the value '{allowedKinds[0]}'.");
        }

        [DoesNotReturn]
        private TResult ThrowCurrentInstanceIsNotAnonymousException<TResult>([CallerMemberName] string propertyName = null)
            => throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the current instance represents an anonymous symbol, which is when the property '{nameof(this.IsAnonymousSymbolKey)}' returns true.");

        [DoesNotReturn]
        private TResult ThrowCurrentInstanceIsNotWellKnownException<TResult>([CallerMemberName] string propertyName = null)
            => throw new InvalidOperationException($"The property '{propertyName}' is only available for symbols, where the current instance represents an well-known symbol, which is when the property '{nameof(this.IsAnonymousSymbolKey)}' returns false.");
    }
}
