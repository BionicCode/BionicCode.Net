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

        private readonly RuntimeTypeHandle _declaringTypeHandle;
        /// <summary>
        /// Gets the runtime handle for the type that declares the current member.
        /// </summary>
        /// <remarks>Use this property to obtain a low-level identifier for the declaring type, which can
        /// be used with reflection APIs that require a RuntimeTypeHandle. The value is typically used for advanced
        /// scenarios involving type metadata or dynamic type operations.</remarks>
        /// <value>The runtime type handle of the declaring type.</value>
        public readonly RuntimeTypeHandle DeclaringTypeHandle
            => (!this.IsAnonymousSymbolKey && this.SymbolKind.EqualsAny([SymbolKind.MemberProperty, SymbolKind.Parameter, SymbolKind.MemberEvent])) || (this.IsAnonymousSymbolKey && this.SymbolKind is not SymbolKind.Type)
                ? this._declaringTypeHandle
                : this.IsAnonymousSymbolKey
                    ? throw new InvalidOperationException($"For anonymous keys, which is when '{nameof(this.IsAnonymousSymbolKey)}' returns TRUE, the property '{nameof(this.SymbolKind)}' must not be {typeof(SymbolKind).FullName}.{nameof(SymbolKind.Type)}'")
                    : throw new InvalidOperationException($"For non-anonymous keys, which is when '{nameof(this.IsAnonymousSymbolKey)}' returns FALSE, the property '{nameof(this.SymbolKind)}' must be: {typeof(SymbolKind).FullName}.{nameof(SymbolKind.MemberProperty)}', {typeof(SymbolKind).FullName}.{nameof(SymbolKind.Parameter)}', or {typeof(SymbolKind).FullName}.{nameof(SymbolKind.MemberEvent)}'.");

        private readonly RuntimeTypeHandle _symbolTypeHandle;
        /// <summary>
        /// Gets the runtime type handle that represents the symbol's type.
        /// </summary>
        /// <remarks>In case of a method this property returns the method type (return type of the method). For events, this returns the type handle of the event delegate. And for properties and fields this is the simple handle of the field/property type.</remarks>
        /// <value>The runtime type handle of the symbol's type.
        /// For events, this returns the type handle of the event delegate.
        /// And for properties and parameters this is the handle of the property/parameter type.<br/>
        /// For anonymous members (if the key was created with one of the <c>CreateForAnonymousSymbol()</c> overloads, the value is <see langword="default"/>.</value>
        public readonly RuntimeTypeHandle SymbolTypeHandle
            => this.SymbolKind.EqualsAny([SymbolKind.Type, SymbolKind.MemberEvent, SymbolKind.MemberProperty])
            ? this._symbolTypeHandle
            : ThrowInvalidPropertyContextException<RuntimeTypeHandle>([SymbolKind.Type, SymbolKind.MemberEvent, SymbolKind.MemberProperty]);

        private readonly RuntimeMethodHandle _methodHandle;
        /// <summary>
        /// Gets the handle that represents the internal metadata of the method.
        /// </summary>
        /// <remarks>The method handle provides a low-level representation of the method's metadata and
        /// can be used for advanced reflection scenarios. Accessing the handle is typically only necessary when
        /// interoperating with unmanaged code or performing operations that require direct access to method
        /// metadata.</remarks>
        /// <value>The runtime method handle of the method.
        public readonly RuntimeMethodHandle MethodHandle => this.SymbolKind.EqualsAny([SymbolKind.MemberMethod, SymbolKind.MemberConstructor])
            ? this._methodHandle
            : ThrowInvalidPropertyContextException<RuntimeMethodHandle>([SymbolKind.MemberMethod, SymbolKind.MemberConstructor]);

        private readonly RuntimeFieldHandle _fieldHandle;
        /// <summary>
        /// Gets a handle to the internal metadata representation of the field.
        /// </summary>
        /// <remarks>The returned handle can be used for low-level reflection operations or
        /// interoperability scenarios. The value is primarily intended for advanced scenarios and should be used with
        /// care, as it exposes runtime-specific details.</remarks>
        /// <value>The runtime field handle of the field.
        public readonly RuntimeFieldHandle FieldHandle => this.SymbolKind == SymbolKind.MemberField
            ? this._fieldHandle
            : ThrowInvalidPropertyContextException<RuntimeFieldHandle>([SymbolKind.MemberField]);

        /// <summary>
        /// Gets the kind of symbol represented by this instance.
        /// </summary>
        /// <value>The kind of symbol, such as type, method, property, event, field, constructor, or parameter.</value>
        public readonly SymbolKind SymbolKind { get; }

        private readonly ParameterList _parameterList;
        /// <summary>
        /// If the current cache key is anonymous, then this property get the list of parameters associated with a method, constructor or indexer property.
        /// </summary>
        /// <value>The list of parameters for methods, properties (indexers), and constructors. 
        public ParameterList ParameterList => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<ParameterList>()
            : this.SymbolKind.EqualsAny([SymbolKind.MemberMethod, SymbolKind.MemberConstructor, SymbolKind.MemberProperty])
                ? this._parameterList
                : ThrowInvalidPropertyContextException<ParameterList>([SymbolKind.MemberMethod, SymbolKind.MemberConstructor, SymbolKind.MemberProperty]);

        private readonly MethodParameterInfoList _methodParameterInfoList;
        public MethodParameterInfoList MethodParameterInfoList => this.SymbolKind.EqualsAny([SymbolKind.MemberMethod, SymbolKind.MemberConstructor, SymbolKind.MemberProperty])
            ? this._methodParameterInfoList
            : ThrowInvalidPropertyContextException<MethodParameterInfoList>([SymbolKind.MemberMethod, SymbolKind.MemberConstructor, SymbolKind.MemberProperty]);

        private readonly TypeList _genericParameterList;
        /// <summary>
        /// If the current cache key is anonymous, then this property get the list of parameters associated with a method, constructor or indexer property.
        /// </summary>
        /// <value>The list of parameters for methods, properties (indexers), and constructors. 
        public TypeList GenericParameterList => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<TypeList>()
            : this.SymbolKind.EqualsAny([SymbolKind.MemberMethod, SymbolKind.Type])
                ? this._genericParameterList
                : ThrowInvalidPropertyContextException<TypeList>([SymbolKind.MemberMethod, SymbolKind.Type]);

        private readonly int _genericTypeParameterCount;
        /// <summary>
        /// If the current cache key is anonymous, then this property gets the number of generic type parameters defined for a type, the member tha defines the parameter or a method.
        /// </summary>
        /// <value>The count of generic type parameters.
        public int GenericTypeParameterCount => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<int>()
            : this.SymbolKind.EqualsAny([SymbolKind.Type, SymbolKind.MemberMethod])
                ? this._genericTypeParameterCount
                : ThrowInvalidPropertyContextException<int>([SymbolKind.Type, SymbolKind.MemberMethod]);

        private readonly CacheKeyParameterDescriptor _cacheKeyParameterDescriptor;
        public CacheKeyParameterDescriptor CacheKeyParameterDescriptor => this.SymbolKind is SymbolKind.Parameter
            ? this._cacheKeyParameterDescriptor
            : ThrowInvalidPropertyContextException<CacheKeyParameterDescriptor>([SymbolKind.Parameter]);

        private readonly CacheKeyParameterMemberDescriptor _cacheKeyParameterMemberDescriptor;
        public CacheKeyParameterMemberDescriptor CacheKeyParameterMemberDescriptor => this.SymbolKind is SymbolKind.Parameter
            ? this._cacheKeyParameterMemberDescriptor
            : ThrowInvalidPropertyContextException<CacheKeyParameterMemberDescriptor>([SymbolKind.Parameter]);

        private readonly CacheKeyPropertyDescriptor _propertyDescriptor;
        public CacheKeyPropertyDescriptor CacheKeyPropertyDescriptor => this.SymbolKind is SymbolKind.MemberProperty
            ? this._propertyDescriptor
            : ThrowInvalidPropertyContextException<CacheKeyPropertyDescriptor>([SymbolKind.MemberProperty]);

        private readonly CacheKeyMethodDescriptor _methodDescriptor;
        public CacheKeyMethodDescriptor CacheKeyMethodDescriptor => this.SymbolKind is SymbolKind.MemberMethod
            ? this._methodDescriptor
            : ThrowInvalidPropertyContextException<CacheKeyMethodDescriptor>([SymbolKind.MemberMethod]);

        //private readonly PropertyAccessor _indexerPropertyAccessor;
        //public PropertyAccessor IndexerPropertyAccessor => !this.IsAnonymousSymbolKey
        //    ? ThrowCurrentInstanceIsNotAnonymousException<PropertyAccessor>()
        //    : this.SymbolKind.EqualsAny([SymbolKind.MemberProperty])
        //        ? this._indexerPropertyAccessor
        //        : ThrowInvalidPropertyContextException<PropertyAccessor>([SymbolKind.MemberProperty]);

        private readonly bool _isExplicitInterfaceImplementation;
        public bool IsExplicitInterfaceImplementation => this.SymbolKind is SymbolKind.MemberProperty
            ? this._isExplicitInterfaceImplementation
            : ThrowInvalidPropertyContextException<bool>([SymbolKind.MemberProperty]);

        public bool IsAnonymousSymbolKey { get; }

        private readonly int _hashCode;

        private SymbolReflectionInfoCacheKey(string name,
            RuntimeTypeHandle declaringTypeHandle,
            RuntimeTypeHandle typeHandle,
            RuntimeMethodHandle methodHandle,
            RuntimeFieldHandle fieldHandle,
            ParameterList parameterList,
            MethodParameterInfoList methodParameterInfoList,
            TypeList genericParameterList,
            int genericTypeParameterCount,
            SymbolKind symbolKind,
            CacheKeyParameterDescriptor cacheKeyParameterDescriptor,
            CacheKeyParameterMemberDescriptor cacheKeyParameterMemberDescriptor,
            CacheKeyPropertyDescriptor propertyDescriptor,
            CacheKeyMethodDescriptor methodDescriptor,
            bool isExplicitInterfaceImplementation,
            bool isAnonymousSymbolKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));

            this.SymbolKind = symbolKind;
            this._cacheKeyParameterDescriptor = cacheKeyParameterDescriptor;
            this._cacheKeyParameterMemberDescriptor = cacheKeyParameterMemberDescriptor;
            this._propertyDescriptor = propertyDescriptor;
            this._methodDescriptor = methodDescriptor;
            this._isExplicitInterfaceImplementation = isExplicitInterfaceImplementation;
            this.SymbolName = name;
            this._declaringTypeHandle = declaringTypeHandle;
            this._symbolTypeHandle = typeHandle;
            this._methodHandle = methodHandle;
            this._fieldHandle = fieldHandle;
            this._parameterList = parameterList;
            this._methodParameterInfoList = methodParameterInfoList;
            this._genericParameterList = genericParameterList;
            this._genericTypeParameterCount = genericTypeParameterCount;
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
        /// Creates a cache key for a property symbol.
        /// </summary>
        /// <param name="propertyInfo">The property information.</param>
        /// <param name="isExplicitInterfaceImplementation">Indicates whether the property is an explicit interface implementation.
        /// <para/>If <see langword="true" /> the <paramref name="propertyInfo"/> must be obtained from the declaring interface type i.e. the <see cref="MemberInfo.DeclaringType"/> must return an interface type.</param>
        /// <param name="getterImplementation">The getter implementation method, if any. This is  only relevant for explicit interface implementations when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> and therefore <paramref name="propertyInfo"/> maps to the interface declaration. This value will be ignored if <paramref name="isExplicitInterfaceImplementation"/> is <see langword="false"/>.</param>
        /// <param name="setterImplementation">The setter implementation method, if any. This is  only relevant for explicit interface implementations when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> and therefore <paramref name="propertyInfo"/> maps to the interface declaration. This value will be ignored if <paramref name="isExplicitInterfaceImplementation"/> is <see langword="false"/>.</param>
        /// <returns>The unique cache key for the property symbol.</returns>
        /// <exception cref="ArgumentNullException">Thrown when
        /// <list type="bullet">
        /// <item><paramref name="propertyInfo"/> or its declaring type is <see langword="null"/>.</item>
        /// <item>the <paramref name="getterImplementation"/> is <see langword="null"/> when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> and the property is readable.</item>
        /// <item>the <paramref name="setterImplementation"/> is <see langword="null"/> when <paramref name="isExplicitInterfaceImplementation"/> is <see langword="true"/> and the property is writable.</item>
        /// </list>
        /// </exception>
        public static SymbolReflectionInfoCacheKey CreateForProperty(CacheKeyPropertyDescriptor propertyDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

            return new SymbolReflectionInfoCacheKey(propertyDescriptor.PropertyName,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberProperty,
                default,
                default,
                propertyDescriptor,
                default,
                propertyDescriptor.IsExplicitInterfaceImplementation,
                false);
        }

        /// <summary>
        /// Creates a cache key for a method symbol.
        /// </summary>
        /// <param name="methodInfo">The method information.</param>
        /// <param name="isExplicitInterfaceImplementation">Indicates whether the method is an explicit interface implementation.
        /// <para/>If <see langword="true" /> the <paramref name="methodInfo"/> must be obtained from the declaring interface type i.e. the <see cref="MemberInfo.DeclaringType"/> must return an interface type.</param>
        /// <returns>The unique cache key for the method symbol.</returns>
        public static SymbolReflectionInfoCacheKey CreateForMethod(CacheKeyMethodDescriptor methodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

            return new SymbolReflectionInfoCacheKey(methodDescriptor.MethodName,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberMethod,
                default,
                default,
                default,
                methodDescriptor,
                methodDescriptor.IsExplicitInterfaceImplementation,
                false);
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

        public static SymbolReflectionInfoCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            RuntimeFieldHandle fieldHandle = fieldInfo.FieldHandle;

            return new SymbolReflectionInfoCacheKey(fieldInfo.Name,
                default,
                default,
                default,
                fieldHandle,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberField,
                default,
                default,
                default,
                default,
                false,
                false);
        }

        public static SymbolReflectionInfoCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullException.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;

            return new SymbolReflectionInfoCacheKey(constructorInfo.Name,
                default,
                default,
                methodHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberConstructor,
                default,
                default,
                default,
                default,
                false,
                false);
        }

        public static SymbolReflectionInfoCacheKey CreateForParameter(ParameterInfo parameterInfo)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            MemberInfo member = parameterInfo.Member;

            // If the 'ParameterInfo.Member' property returns a 'PropertyInfo' then the current parameter 'parameterInfo'
            // was obtained via PropertyInfo.GetIndexParameters method call. As a result, the parameter's association to the property's accessors is ambiguous.
            // We need to normalize it to remove association ambiguity by explicitly associating it with a property's accessor method.
            // We basically replace the current 'GetIndexerParameters()' based 'propertyInfo' argument with a 'PropertyInfo' from an accessor method.
            if (member is PropertyInfo propertyInfo)
            {
                ParameterData? disambiguatedPropertyData = SymbolReflectionInfoCache.ConvertAmbiguousIndexerPropertyParameterToAccessorAssociatedParameter(parameterInfo);
                SymbolReflectionInfoCacheKey normalizedParameterDataCacheKey = disambiguatedPropertyData.CacheKey;

                return normalizedParameterDataCacheKey;
            }

            Type? declaringType = member.DeclaringType;
            if (declaringType is null)
            {
                throw new NotSupportedException($"The argument '{nameof(parameterInfo)}' provides a '{nameof(ParameterInfo)}' instance which belongs to a member that does not have a declaring type. Members without a declaring type are not supported.");
            }

            RuntimeTypeHandle declaringTypeHandle = declaringType.TypeHandle;
            Type parameterType = parameterInfo.ParameterType;
            RuntimeTypeHandle parameterTypeHandle = parameterType.TypeHandle;

            // Method or constructor parameter or property setter or getter parameter where the parameter was obtained via MethodInfo.GetParameters method call
            // But never a property setter or getter parameter where the parameter was obtained via 'PropertyInfo.GetIndexParameters()'.
            RuntimeMethodHandle methodHandle = member is MethodBase methodBaseInfo
                ? methodBaseInfo.MethodHandle

                // Since we already handled PropertyInfo case above, this should never happen.
                : throw new NotSupportedException($"The member '{member.Name}' is not supported. '{typeof(ParameterInfo).ToFullyQualifiedSignatureName}.{nameof(ParameterInfo.Member)} must return a '{typeof(MethodBase).ToFullyQualifiedSignatureName()}'.");

            ParameterizedSymbolKind parameterizedSymbolKind = member is MethodInfo
                ? ParameterizedSymbolKind.MemberMethod
                : ParameterizedSymbolKind.MemberConstructor;

            CacheKeyParameterDescriptor cacheKeyParameterDescriptor = new CacheKeyParameterDescriptor(parameterInfo.Name, parameterInfo.Position, parameterTypeHandle: parameterTypeHandle);
            CacheKeyParameterMemberDescriptor cacheKeyParameterMemberDescriptor = new CacheKeyParameterMemberDescriptor(declaringTypeHandle, methodHandle, parameterizedSymbolKind: parameterizedSymbolKind);

            return new SymbolReflectionInfoCacheKey(string.Empty,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Parameter,
                cacheKeyParameterDescriptor,
                cacheKeyParameterMemberDescriptor,
                default,
                default,
                false,
                false);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous method using the specified method information.
        /// </summary>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousMethod(CacheKeyMethodDescriptor methodDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDescriptor);

            return new SymbolReflectionInfoCacheKey(methodDescriptor.MethodName,
                methodDescriptor.DeclaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberMethod,
                default,
                default,
                default,
                methodDescriptor,
                methodDescriptor.IsExplicitInterfaceImplementation,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous constructor using the specified declaring type and constructor parameters.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous constructor.</param>
        /// <param name="constructorParameters">The list of parameters for the anonymous constructor. Can be <see cref="ParameterList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousConstructor(RuntimeTypeHandle declaringTypeHandle, ParameterList? constructorParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            constructorParameters = constructorParameters.OrEmpty();

            return new SymbolReflectionInfoCacheKey(string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                constructorParameters,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                0,
                SymbolKind.MemberConstructor,
                default,
                default,
                default,
                default,
                false,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous constructor using the specified declaring type, symbol name, parameters,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for constructor symbols of which the caller does not have a direct representation <see cref="ConstructorInfo"/> and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous constructor.</param>
        /// <param name="constructorParameters">The list of parameters for the anonymous constructor. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousConstructor(RuntimeTypeHandle declaringTypeHandle, MethodParameterInfoList? constructorParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            constructorParameters = constructorParameters.OrEmpty();

            return new SymbolReflectionInfoCacheKey(string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                constructorParameters,
                TypeList.Empty,
                0,
                SymbolKind.MemberConstructor,
                default,
                default,
                default,
                default,
                false,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous property (indexer or normal) using the specified declaring type, property name and parameters (if the property is an indexer).
        /// </summary>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousProperty(CacheKeyPropertyDescriptor propertyDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(propertyDescriptor);

            return new SymbolReflectionInfoCacheKey(propertyDescriptor.PropertyName,
                propertyDescriptor.DeclaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberProperty,
                default,
                default,
                propertyDescriptor,
                default,
                propertyDescriptor.IsExplicitInterfaceImplementation,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous parameter that is described by <see cref="CacheKeyParameterDescriptor"/> and <see cref="CacheKeyParameterMemberDescriptor"/> to provide the specific signature information.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for parameter symbols of which the caller does not have a direct representation <see cref="ParameterInfo"/> or the corresponding <see cref="ParameterData"/>
        /// and instead only signature information is available.<para/>
        /// The anonymous key allows a huge degree af ambiguity e.g. omitting position or type etc. This can yield a successful result if the declaring type and member are unique enough to identify the parameter.
        /// Otherwise, avoiding critical disambiguation information will to ambiguities in which case the cache will not be able to provide a result and instead throw exceptions.<para/>
        /// For best performance and zero ambiguity
        /// <list type="bullet">
        /// <item>
        /// Always map parameters to methods or constructors instead of properties. For properties, focus on the getter and setter methods (that's also how the CLR interprets properties).
        /// </item>
        /// <item>
        /// Ensure to provide the <see cref="RuntimeMethodHandle"/> for the declaring method or constructor.
        /// </item>
        /// </list>
        /// <param name="cacheKeyParameterDescriptor">The <see cref="CacheKeyParameterDescriptor"/> descriptor for the parameter.</param>
        /// <param name="cacheKeyParameterMemberDescriptor">The <see cref="CacheKeyParameterMemberDescriptor"/> descriptor for the member that declares the parameter.</param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous parameter.</returns>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousParameter(CacheKeyParameterDescriptor cacheKeyParameterDescriptor, CacheKeyParameterMemberDescriptor cacheKeyParameterMemberDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKeyParameterDescriptor);
            ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKeyParameterMemberDescriptor);


            return new SymbolReflectionInfoCacheKey(string.Empty,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Parameter,
                cacheKeyParameterDescriptor,
                cacheKeyParameterMemberDescriptor,
                default,
                default,
                false,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous field using the specified declaring type and field name.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for field symbols of which the caller does not have a direct representation <see cref="FieldInfo"/> or <see cref="FieldData"/> and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous field.</param>
        /// <param name="fieldName">The name of the anonymous field. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <returns>A new instance of <see cref="SymbolReflectionInfoCacheKey"/> representing the specified anonymous field.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="fieldName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        public static SymbolReflectionInfoCacheKey CreateForAnonymousField(RuntimeTypeHandle declaringTypeHandle, string fieldName)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);

            return new SymbolReflectionInfoCacheKey(fieldName,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolReflectionInfoCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberField,
                default,
                default,
                default,
                default,
                false,
                true);
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
                hashCode.Add(this._cacheKeyParameterDescriptor);
                hashCode.Add(this._cacheKeyParameterMemberDescriptor);
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
            && this._cacheKeyParameterDescriptor == other._cacheKeyParameterDescriptor
            && this._cacheKeyParameterMemberDescriptor == other._cacheKeyParameterMemberDescriptor
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
    }
}
