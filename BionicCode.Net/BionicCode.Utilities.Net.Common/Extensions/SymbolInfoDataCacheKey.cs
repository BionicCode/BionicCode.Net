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
    internal readonly partial struct SymbolInfoDataCacheKey : IEquatable<SymbolInfoDataCacheKey>
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

        public CacheKeyParameterDescriptor CacheKeyParameterDescriptor => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<CacheKeyParameterDescriptor>()
            : this.SymbolKind is SymbolKind.Parameter
                ? this._cacheKeyParameterDescriptor
                : ThrowInvalidPropertyContextException<CacheKeyParameterDescriptor>([SymbolKind.Parameter]);

        private readonly CacheKeyParameterMemberDescriptor _cacheKeyParameterMemberDescriptor;

        public CacheKeyParameterMemberDescriptor CacheKeyParameterMemberDescriptor => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<CacheKeyParameterMemberDescriptor>()
            : this.SymbolKind is SymbolKind.Parameter
                ? this._cacheKeyParameterMemberDescriptor
                : ThrowInvalidPropertyContextException<CacheKeyParameterMemberDescriptor>([SymbolKind.Parameter]);

        public bool IsAnonymousSymbolKey { get; }

        private readonly int _hashCode;

        private SymbolInfoDataCacheKey(string name,
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
            bool isAnonymousSymbolKey)
        {
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(symbolKind, [SymbolKind.Undefined], nameof(symbolKind));
            ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(name, nameof(name));

            this.SymbolKind = symbolKind;
            this._cacheKeyParameterDescriptor = cacheKeyParameterDescriptor;
            this._cacheKeyParameterMemberDescriptor = cacheKeyParameterMemberDescriptor;
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

        public static SymbolInfoDataCacheKey CreateForEvent(EventInfo eventInfo)
        {
            ArgumentNullException.ThrowIfNull(eventInfo, nameof(eventInfo));

            Type? declaringType = eventInfo.DeclaringType;
            if (declaringType is null)
            {
                throw new NotSupportedException($"The argument '{nameof(eventInfo)}' provides a '{nameof(EventInfo)}' instance which does not have a declaring type. Members without a declaring type are not supported.");
            }

            RuntimeTypeHandle declaringTypeHandle = declaringType.TypeHandle;
            Type? eventHandlerType = eventInfo.EventHandlerType;
            if (eventHandlerType is null)
            {
                throw new NotSupportedException($"The argument '{nameof(eventInfo)}' provides a '{nameof(EventInfo)}' instance which does not have an event handler type. Events without an event handler type are not supported.");
            }

            RuntimeTypeHandle eventDelegateTypeHandle = eventHandlerType.TypeHandle;

            return new SymbolInfoDataCacheKey(eventInfo.Name,
                declaringTypeHandle,
                eventDelegateTypeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberEvent,
                default,
                default,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForProperty(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));

            Type? declaringType = propertyInfo.DeclaringType;
            if (declaringType is null)
            {
                throw new NotSupportedException($"The argument '{nameof(propertyInfo)}' provides a '{nameof(PropertyInfo)}' instance which does not have a declaring type. Members without a declaring type are not supported.");
            }

            RuntimeTypeHandle declaringTypeHandle = declaringType.TypeHandle;
            RuntimeTypeHandle typeHandle = propertyInfo.PropertyType.TypeHandle;

            int indexerParameterCount = propertyInfo.GetIndexParameters().Length;
            return new SymbolInfoDataCacheKey(propertyInfo.Name,
                declaringTypeHandle,
                typeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberProperty,
                default,
                default,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(methodInfo.Name,
                default,
                default,
                methodHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberMethod,
                default,
                default,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            RuntimeTypeHandle typeHandle = type.TypeHandle;

            return new SymbolInfoDataCacheKey(type.FullName ?? type.Name,
                default,
                typeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Type,
                default,
                default,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            RuntimeFieldHandle fieldHandle = fieldInfo.FieldHandle;

            return new SymbolInfoDataCacheKey(fieldInfo.Name,
                default,
                default,
                default,
                fieldHandle,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberField,
                default,
                default,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullException.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(constructorInfo.Name,
                default,
                default,
                methodHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberConstructor,
                default,
                default,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForParameter(ParameterInfo parameterInfo)
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
                SymbolInfoDataCacheKey normalizedParameterDataCacheKey = disambiguatedPropertyData.CacheKey;

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

            return new SymbolInfoDataCacheKey(string.Empty,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Parameter,
                cacheKeyParameterDescriptor,
                cacheKeyParameterMemberDescriptor,
                false);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous method, constructor using the specified declaring type, symbol name, parameters,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="MethodInfo"/> or <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.</param>
        /// <param name="memberName">The name of the anonymous method. Can only be null, empty, or consist only of white-space characters when the <paramref name="symbolKind"/> returns <see cref="SymbolKind.MemberConstructor"/>.</param>
        /// <param name="symbolParameters">The list of parameters for the anonymous method, constructor or indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="MethodParameterInfoList.Empty"/> for constructors or to indicate a non-generic method.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/> or when <paramref name="genericMethodParameters"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, ParameterList? symbolParameters, TypeList genericMethodParameters, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind);
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                genericMethodParameters,
                nameof(genericMethodParameters),
                $"Pass '{typeof(TypeList).ToFullyQualifiedSignatureName()}.{nameof(TypeList.Empty)}' for non-generic methods.");
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(
                symbolKind,
                [SymbolKind.MemberMethod, SymbolKind.MemberConstructor],
                nameof(symbolKind),
                "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");

            if (symbolKind == SymbolKind.MemberMethod)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(memberName);
            }
            else
            {
                // For constructors we allow empty or whitespace names.
                memberName = string.Empty;
            }

            return new SymbolInfoDataCacheKey(memberName,
                declaringTypeHandle,
                default,
                default,
                default,
                symbolParameters ?? ParameterList.Empty,
                MethodParameterInfoList.Empty,
                genericMethodParameters,
                genericMethodParameters.Count,
                symbolKind,
                default,
                default,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous method, constructor using the specified declaring type, symbol name, parameters,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="MethodInfo"/> or <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.</param>
        /// <param name="memberName">The name of the anonymous method, constructor . Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="symbolParameters">The list of parameters for the anonymous method, constructor or indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericMethodParameters">The list of generic method parameters for the anonymous method.<para/>
        /// Can be <see cref="MethodParameterInfoList.Empty"/> for constructors or to indicate a non-generic method.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/> or when <paramref name="genericMethodParameters"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, MethodParameterInfoList? symbolParameters, TypeList genericMethodParameters, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentNullExceptionAdvanced.ThrowIfNull(
                genericMethodParameters,
                nameof(genericMethodParameters),
                $"Pass '{typeof(TypeList).ToFullyQualifiedSignatureName()}.{nameof(TypeList.Empty)}' for non-generic methods.");
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(symbolKind, [SymbolKind.MemberMethod, SymbolKind.MemberConstructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentExceptionAdvanced.ThrowIfAny(
                symbolParameters ?? MethodParameterInfoList.Empty,
                methodParameterInfo => !methodParameterInfo.DeclaringTypeHandle.Equals(declaringTypeHandle),
                nameof(symbolParameters),
                $"Declaring type handle mismatch. The argument '{nameof(symbolParameters)}' sequence contains at least one item that holds a '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.DeclaringTypeHandle)}' value that is not equal to the provided argument '{nameof(declaringTypeHandle)}'.");

            return new SymbolInfoDataCacheKey(memberName,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                symbolParameters ?? MethodParameterInfoList.Empty,
                genericMethodParameters,
                genericMethodParameters.Count,
                symbolKind,
                default,
                default,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous property using the specified declaring type, symbol name, parameters, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.</param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="indexerParameters">The list of parameters for the anonymous indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of white-space characters</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousProperty(RuntimeTypeHandle declaringTypeHandle, string propertyName, ParameterList? indexerParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

            ParameterList parameterList = indexerParameters ?? ParameterList.Empty;
            return new SymbolInfoDataCacheKey(propertyName,
                declaringTypeHandle,
                default,
                default,
                default,
                parameterList,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberProperty,
                default,
                default,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous property using the specified declaring type, symbol name, parameters, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.</param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="indexerParameters">The list of parameters for the anonymous indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous property.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of white-space characters</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousProperty(RuntimeTypeHandle declaringTypeHandle, string propertyName, MethodParameterInfoList? indexerParameters)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

            MethodParameterInfoList methodParameterInfoList = indexerParameters ?? MethodParameterInfoList.Empty;
            return new SymbolInfoDataCacheKey(propertyName,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                methodParameterInfoList,
                TypeList.Empty,
                methodParameterInfoList.Count,
                SymbolKind.MemberProperty,
                default,
                default,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous parameter using the specified declaring type, symbol name, parameter position,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="MethodInfo"/> or corresponding <see cref="MethodData"/>) and instead only signature information is available.<para/>
        /// <para/>For best performance and zero ambiguity ensure to provide the <see cref="RuntimeMethodHandle"/> for the declaring method or constructor.</remarks>
        /// <param name="cacheKeyParameterDescriptor">The <see cref="CacheKeyParameterDescriptor"/> descriptor for the parameter.</param>
        /// <param name="cacheKeyParameterMemberDescriptor">The <see cref="CacheKeyParameterMemberDescriptor"/> descriptor for the member that declares the parameter.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous parameter.</returns>
        public static SymbolInfoDataCacheKey CreateForAnonymousParameter(CacheKeyParameterDescriptor cacheKeyParameterDescriptor, CacheKeyParameterMemberDescriptor cacheKeyParameterMemberDescriptor)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKeyParameterDescriptor);
            ArgumentNullExceptionAdvanced.ThrowIfDefault(cacheKeyParameterMemberDescriptor);


            return new SymbolInfoDataCacheKey(string.Empty,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Parameter,
                cacheKeyParameterDescriptor,
                cacheKeyParameterMemberDescriptor,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous field or event using the specified declaring type, symbol name and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="EventInfo"/> or <see cref="FieldInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous field or event.</param>
        /// <param name="symbolName">The name of the anonymous field or event. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberEvent"/> or <see cref="SymbolKind.MemberField"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous field or event.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="symbolName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberEvent"/> or <see cref="SymbolKind.MemberField"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousFieldOrEvent(RuntimeTypeHandle declaringTypeHandle, string symbolName, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle);
            ArgumentException.ThrowIfNullOrWhiteSpace(symbolName);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind);
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(symbolKind, [SymbolKind.MemberEvent, SymbolKind.MemberField], nameof(symbolKind), "The symbol kind must be 'MemberEvent' or 'MemberField' for anonymous event or field symbols.");

            return new SymbolInfoDataCacheKey(symbolName,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                TypeList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                symbolKind,
                default,
                default,
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

                foreach (ParameterData parameterData in this._parameterList)
                {
                    hashCode.Add(parameterData.ParameterTypeHandle);
                    hashCode.Add(parameterData.DeclaringTypeHandle);
                    hashCode.Add(parameterData.Position);
                }

                foreach (MethodParameterInfo parameterData in this._methodParameterInfoList)
                {
                    hashCode.Add(parameterData.ParameterTypeHandle);
                    hashCode.Add(parameterData.DeclaringTypeHandle);
                    hashCode.Add(parameterData.Position);
                }

                return hashCode.ToHashCode();
            }
        }

        public override bool Equals(object obj)
            => obj is SymbolInfoDataCacheKey other && Equals(other);

        public bool Equals(SymbolInfoDataCacheKey other) => this.SymbolName == other.SymbolName
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
            && this._methodParameterInfoList.Equals(other._methodParameterInfoList);

        public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
        public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);

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
