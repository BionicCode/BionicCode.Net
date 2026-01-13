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
    internal readonly struct SymbolInfoDataCacheKey : IEquatable<SymbolInfoDataCacheKey>
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

        private readonly RuntimeMethodHandle _declaringTypeHandle;
        /// <summary>
        /// Gets the runtime handle for the type that declares the current member.
        /// </summary>
        /// <remarks>Use this property to obtain a low-level identifier for the declaring type, which can
        /// be used with reflection APIs that require a RuntimeTypeHandle. The value is typically used for advanced
        /// scenarios involving type metadata or dynamic type operations.</remarks>
        /// <value>The runtime type handle of the declaring type.</value>
        public readonly System.RuntimeTypeHandle DeclaringTypeHandle { get; }

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
            => this.SymbolKind.EqualsAny([SymbolKind.Type, SymbolKind.MemberEvent, SymbolKind.MemberProperty, SymbolKind.Parameter])
            ? this._symbolTypeHandle
            : ThrowInvalidPropertyContextException<RuntimeTypeHandle>([SymbolKind.Type, SymbolKind.MemberEvent, SymbolKind.MemberProperty, SymbolKind.Parameter]);

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

        //private readonly RuntimeMethodHandle _getMethodHandle;
        ///// <summary>
        ///// Gets the handle to the underlying runtime method used to get the value of the property.
        ///// </summary>
        ///// <remarks>The method handle can be used to access low-level metadata or invoke the method via
        ///// reflection. The value is valid only while the associated type is loaded and may become invalid if the type
        ///// is unloaded.</remarks>
        ///// <value>The runtime method handle of the get accessor method. For non-property members the value is <see langword="default"/>.</value>
        //public readonly RuntimeMethodHandle GetMethodHandle => this.SymbolKind.Equals(SymbolKind.MemberProperty)
        //    ? this._getMethodHandle
        //    : ThrowInvalidPropertyContextException<RuntimeMethodHandle>([SymbolKind.MemberProperty]);

        //private readonly RuntimeMethodHandle _setMethodHandle;
        ///// <summary>
        ///// Gets the handle to the underlying runtime method used to set the value of the property.
        ///// </summary>
        ///// <value>The runtime method handle of the set accessor method. For non-property members the value is <see langword="default"/>.</value>
        //public readonly RuntimeMethodHandle SetMethodHandle => this.SymbolKind.Equals(SymbolKind.MemberProperty)
        //    ? this._setMethodHandle
        //    : ThrowInvalidPropertyContextException<RuntimeMethodHandle>([SymbolKind.MemberProperty]);

        //private readonly RuntimeMethodHandle _addMethodHandle;
        ///// <summary>
        ///// Gets the runtime method handle for the add accessor of the event.
        ///// </summary>
        ///// <value>The runtime method handle of the add accessor method. For non-event members the value is <see langword="default"/>.</value>
        //public readonly RuntimeMethodHandle AddMethodHandle => this.SymbolKind.Equals(SymbolKind.MemberEvent)
        //    ? this._addMethodHandle
        //    : ThrowInvalidPropertyContextException<RuntimeMethodHandle>([SymbolKind.MemberEvent]);

        //private readonly RuntimeMethodHandle _removeMethodHandle;
        ///// <summary>
        ///// Gets the runtime method handle for the remove accessor of the event.
        ///// </summary>
        ///// <value>The runtime method handle of the remove accessor method. For non-event members the value is <see langword="default"/>.</value>
        //public readonly RuntimeMethodHandle RemoveMethodHandle => this.SymbolKind.Equals(SymbolKind.MemberEvent)
        //    ? this._removeMethodHandle
        //    : ThrowInvalidPropertyContextException<RuntimeMethodHandle>([SymbolKind.MemberEvent]);

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

        private readonly int _genericTypeParameterCount;
        /// <summary>
        /// If the current cache key is anonymous, then this property gets the number of generic type parameters defined for a type, the member tha defines the parameter or a method.
        /// </summary>
        /// <value>The count of generic type parameters.
        public int GenericTypeParameterCount => !this.IsAnonymousSymbolKey
            ? ThrowCurrentInstanceIsNotAnonymousException<int>()
            : this.SymbolKind.EqualsAny([SymbolKind.Parameter, SymbolKind.Type, SymbolKind.MemberMethod])
                ? this._genericTypeParameterCount
                : ThrowInvalidPropertyContextException<int>([SymbolKind.Parameter, SymbolKind.Type, SymbolKind.MemberMethod]);

        private readonly int _parameterMemberParameterCount;
        /// <summary>
        /// For parameters, this returns the number of parameters defined for the member the parameter belongs to.
        /// </summary>
        /// <value>The count of parameters for methods, properties (indexers), and constructors.
        public int ParameterMemberParameterCount => this.SymbolKind == SymbolKind.Parameter
            ? this._parameterMemberParameterCount
            : ThrowInvalidPropertyContextException<int>([SymbolKind.Parameter]);

        private readonly int _parameterMemberGenericTypeParameterCount;
        /// <summary>
        /// Gets the number of generic type parameters declared by the member that defines a parameter.
        /// </summary>
        public int ParameterMemberGenericTypeParameterCount => this.SymbolKind == SymbolKind.Parameter
            ? this._parameterMemberGenericTypeParameterCount
            : ThrowInvalidPropertyContextException<int>([SymbolKind.Parameter]);

        private readonly string _parameterMemberName;
        /// <summary>
        /// Gets the name of the member that is associated with the parameter.
        /// </summary>
        public string ParameterMemberName => this.SymbolKind == SymbolKind.Parameter
            ? this._parameterMemberName
            : ThrowInvalidPropertyContextException<string>([SymbolKind.Parameter]);

        private readonly int _parameterPosition;
        /// <summary>
        /// Gets the zero-based position of the parameter in the formal parameter list.
        /// </summary>
        public int ParameterPosition => this.SymbolKind == SymbolKind.Parameter
            ? this._parameterPosition
            : ThrowInvalidPropertyContextException<int>([SymbolKind.Parameter]);

        private readonly ParameterKind _parameterKind;
        /// <summary>
        /// If the current cache key is anonymous, then this property get the modifier kind of parameter represented by this instance.
        /// </summary>
        public ParameterKind ParameterKind => this.SymbolKind == SymbolKind.Parameter
            ? this._parameterKind
            : ThrowInvalidPropertyContextException<ParameterKind>([SymbolKind.Parameter]);

        private readonly ParameterizedSymbolKind _parameterizedMemberKind;
        /// <summary>
        /// Gets the kind of parameterized symbol represented by this instance.
        /// </summary>
        public ParameterizedSymbolKind ParameterizedMemberKind => this.SymbolKind == SymbolKind.Parameter
            ? this._parameterizedMemberKind
            : ThrowInvalidPropertyContextException<ParameterizedSymbolKind>([SymbolKind.Parameter]);

        public bool IsAnonymousSymbolKey { get; }

        private readonly int _hashCode;

        private SymbolInfoDataCacheKey(string name,
            string parameterMemberName,
            RuntimeTypeHandle declaringTypeHandle,
            RuntimeTypeHandle typeHandle,
            RuntimeMethodHandle methodHandle,
            RuntimeFieldHandle fieldHandle,
            ParameterList parameterList,
            MethodParameterInfoList methodParameterInfoList,
            int parameterPosition,
            int genericTypeParameterCount,
            int memberParameterCount,
            SymbolKind symbolKind,
            ParameterKind parameterKind,
            ParameterizedSymbolKind parameterizedSymbolKind,
            bool isAnonymousSymbolKey)
        {
            this.SymbolName = name;
            this.ParameterMemberName = parameterMemberName;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.SymbolTypeHandle = typeHandle;
            this.MethodHandle = methodHandle;
            this.FieldHandle = fieldHandle;
            //this.GetMethodHandle = getMethodHandle;
            //this.SetMethodHandle = setMethodHandle;
            //this.AddMethodHandle = addMethodHandle;
            //this.RemoveMethodHandle = removeMethodHandle;
            this.ParameterList = parameterList;
            this.MethodParameterInfoList = methodParameterInfoList;
            this.ParameterPosition = parameterPosition;
            this.ParameterMemberParameterCount = memberParameterCount;
            this.GenericTypeParameterCount = genericTypeParameterCount;
            this.SymbolKind = symbolKind;
            this.ParameterKind = parameterKind;
            this.ParameterizedMemberKind = parameterizedSymbolKind;
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
                string.Empty,
                declaringTypeHandle,
                eventDelegateTypeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberEvent,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Delegate,
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
                string.Empty,
                declaringTypeHandle,
                typeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                indexerParameterCount,
                SymbolKind.MemberProperty,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(methodInfo.Name,
                string.Empty,
                default,
                default,
                methodHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberMethod,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.MemberMethod,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            RuntimeTypeHandle typeHandle = type.TypeHandle;

            return new SymbolInfoDataCacheKey(type.FullName ?? type.Name,
                string.Empty,
                default,
                typeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Type,
                ParameterKind.Undefined,
                type.IsDelegate() ? ParameterizedSymbolKind.Delegate : ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));

            RuntimeFieldHandle fieldHandle = fieldInfo.FieldHandle;

            return new SymbolInfoDataCacheKey(fieldInfo.Name,
                string.Empty,
                default,
                default,
                default,
                fieldHandle,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberField,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullException.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(constructorInfo.Name,
                string.Empty,
                default,
                default,
                methodHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.MemberConstructor,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.MemberConstructor,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForParameter(ParameterInfo parameterInfo)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            MemberInfo member = parameterInfo.Member;
            Type? declaringType = member.DeclaringType;
            if (declaringType is null)
            {
                throw new NotSupportedException($"The argument '{nameof(parameterInfo)}' provides a '{nameof(ParameterInfo)}' instance which belongs to a member that does not have a declaring type. Members without a declaring type are not supported.");
            }

            RuntimeTypeHandle declaringTypeHandle = declaringType.TypeHandle;
            Type parameterType = parameterInfo.ParameterType;
            RuntimeTypeHandle parameterTypeHandle = parameterType.TypeHandle;
            RuntimeMethodHandle methodHandle = member is MethodBase methodBaseInfo // Method or constructor parameter
                ? methodBaseInfo.MethodHandle
                : parameterInfo.Member is PropertyInfo propertyInfo // Indexer parameter
                    ? propertyInfo.GetMethod?.MethodHandle ?? propertyInfo.SetMethod?.MethodHandle ?? default
                    : default;
            ParameterizedSymbolKind parameterizedSymbolKind = member is MethodInfo
                ? ParameterizedSymbolKind.MemberMethod
                : member is ConstructorInfo
                    ? ParameterizedSymbolKind.MemberConstructor
                    : member is PropertyInfo
                        ? ParameterizedSymbolKind.MemberIndexerProperty
                        : ParameterizedSymbolKind.Undefined;

            return new SymbolInfoDataCacheKey(parameterInfo.Name ?? string.Empty,
                parameterInfo.Member.Name,
                declaringTypeHandle,
                parameterTypeHandle,
                methodHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                parameterInfo.Position,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Parameter,
                ParameterKind.Undefined,
                parameterizedSymbolKind,
                false);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous method, constructor using the specified declaring type, symbol name, parameters,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="MethodInfo"/> or <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous method, constructor.</param>
        /// <param name="memberName">The name of the anonymous method, constructor . Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="symbolParameters">The list of parameters for the anonymous method, constructor or indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters.</param>
        /// <param name="genericTypeParameterCount">The number of generic type parameters for the anonymous method or constructor. Must be zero or greater. For properties the value is ignored.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="genericTypeParameterCount"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, ParameterList? symbolParameters, int genericTypeParameterCount, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(symbolKind, [SymbolKind.MemberMethod, SymbolKind.MemberConstructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, paramName: nameof(genericTypeParameterCount));

            return new SymbolInfoDataCacheKey(memberName,
                string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                symbolParameters ?? ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                genericTypeParameterCount,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                symbolKind,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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
        /// <param name="genericTypeParameterCount">The number of generic type parameters for the anonymous method or constructor. Must be zero or greater. For properties the value is ignored.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.MemberConstructor"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="genericTypeParameterCount"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, MethodParameterInfoList? symbolParameters, int genericTypeParameterCount, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(symbolKind, [SymbolKind.MemberMethod, SymbolKind.MemberConstructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));
            ArgumentExceptionAdvanced.ThrowIfAny(
                symbolParameters ?? MethodParameterInfoList.Empty,
                methodParameterInfo => !methodParameterInfo.DeclaringTypeHandle.Equals(declaringTypeHandle),
                nameof(symbolParameters),
                $"Declaring type handle mismatch. The argument '{nameof(symbolParameters)}' sequence contains at least one item that holds a '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.DeclaringTypeHandle)}' value that is not equal to the provided argument '{nameof(declaringTypeHandle)}'.");

            return new SymbolInfoDataCacheKey(memberName,
                string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                symbolParameters ?? MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                genericTypeParameterCount,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                symbolKind,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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
                string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                parameterList,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                parameterList.Count,
                SymbolKind.MemberProperty,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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
                string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                methodParameterInfoList,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                methodParameterInfoList.Count,
                SymbolKind.MemberProperty,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous parameter using the specified declaring type, symbol name, parameter position,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="ParameterInfo"/> and instead only signature information is available.<para/>
        /// The parameter <paramref name="memberName"/> is optional. However, if not provided, the created key will be less efficient when used for lookups in caches. If parameter name and position matches multiple parameters, providing <paramref name="memberName"/> or even better the member's runtime handle via the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle, ParameterizedSymbolKind)"/> overload will allow to resolve ambiguities that otherwise may throw an exception.
        /// <para/>For better performance, the <paramref name="memberName"/> must be provided.
        /// <br/>For best performance the overload <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle, ParameterizedSymbolKind)"/> should be used.</remarks>
        /// <param name="parameterTypeHandle">The runtime type handle representing the type of the anonymous parameter.</param>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="parameterName">The name of the anonymous parameter. </param>
        /// <param name="position">The index of the parameter.</param>
        /// <param name="parameterKind">Optional.Must be provided to avoid ambiguity which can throw exceptions during lookup when using this key.</param>
        /// <param name="memberName">Optional. The name of the member that declares the parameter. You should provide the member name to improve performance. For best efficiency, use the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle, ParameterizedSymbolKind)"/> overload instead.
        /// <br/>If the parameter belongs to an indexer property, the <paramref name="memberName"/> can be null, empty, or consist only of white-space characters (in this case <paramref name="parameterizedSymbolKind"/> must be <see cref="ParameterizedSymbolKind.MemberIndexerProperty"/>. For indexer properties this value will be ignored.).
        /// For indexer properties, the value must be <see cref="HelperExtensionsCommon.IndexerName"/>.</param>
        /// <param name="memberGenericParameterCount"></param>
        /// <param name="memberParameterCount"></param>
        /// <param name="parameterizedSymbolKind">Optional. Provides a hint about the kind of member that the parameter belongs to. Should be provided too improve efficiency of the key.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName"/> is null, empty, or consists only of white-space characters or <paramref name="parameterKind"/> is <see cref="ParameterKind.Undefined"/> or <paramref name="parameterizedSymbolKind"/> is <see cref="ParameterizedSymbolKind.Undefined"/> or the provided enum values for <paramref name="parameterKind"/> or <paramref name="parameterizedSymbolKind"/> are not defined.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is negative.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousParameter(RuntimeTypeHandle parameterTypeHandle, RuntimeTypeHandle declaringTypeHandle, string parameterName, int position, ParameterKind parameterKind, ParameterizedSymbolKind parameterizedSymbolKind, string? memberName = null, int memberGenericParameterCount = SymbolInfoDataCacheKey.UnknownParameterCountOrPosition, int memberParameterCount = SymbolInfoDataCacheKey.UnknownParameterCountOrPosition)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(parameterTypeHandle, nameof(parameterTypeHandle));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            ArgumentOutOfRangeException.ThrowIfNegative(position, paramName: nameof(position));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterKind>(parameterKind, nameof(parameterKind));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterizedSymbolKind>(parameterizedSymbolKind, nameof(parameterizedSymbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                parameterizedSymbolKind,
                [ParameterizedSymbolKind.Undefined],
                nameof(parameterizedSymbolKind), $"Invalid argument '{nameof(parameterizedSymbolKind)}'. The value '{nameof(ParameterizedMemberKind)}.{ParameterizedSymbolKind.Undefined}' is not allowed.");
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                parameterKind,
                [ParameterKind.Undefined],
                nameof(parameterKind), $"Invalid argument '{nameof(parameterKind)}'. The value '{nameof(ParameterKind)}.{ParameterKind.Undefined}' is not allowed.");

            return new SymbolInfoDataCacheKey(parameterName,
                memberName ?? string.Empty,
                declaringTypeHandle,
                parameterTypeHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                position,
                memberGenericParameterCount,
                memberParameterCount,
                SymbolKind.Parameter,
                parameterKind,
                parameterizedSymbolKind,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous parameter using the specified declaring type, symbol name, parameter position and a declaring member reference.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="ParameterInfo"/> and instead only signature information is available.<para/>
        /// The parameter <paramref name="memberHandle"/> is not optional nd therefore <see langword="default"/> is not a valid value. If parameter name and position matches multiple parameters, providing <paramref name="memberHandle"/> or the member name via the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, RuntimeTypeHandle, string, int, ParameterKind, ParameterizedSymbolKind, string?, int, int)"/> overload will allow to resolve ambiguities that otherwise may throw an exception.
        /// For best performance, the <paramref name="memberHandle"/> must be provided.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="parameterName">The name of the anonymous parameter. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="position">The index of the parameter. This value can't be negative or <see cref="SymbolInfoDataCacheKey.UnknownParameterCountOrPosition"/>.</param>
        /// <param name="memberHandle">For best performance provide a <see cref="RuntimeMethodHandle"/> to the method or constructor that defines the parameter should be provided. Alternatively, call <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, RuntimeTypeHandle, string, int, ParameterKind, ParameterizedSymbolKind, string?, int, int)"/> which accepts the defining member name but will perform worse, but still better than the case where a member handle or member name are not provided.</param>
        /// <param name="parameterizedSymbolKind">The kind of the parameterized symbol.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous parameter.</returns>
        /// <remarks></remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberHandle"/> is <see langword="default"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousParameter(RuntimeTypeHandle declaringTypeHandle, string parameterName, int position, RuntimeMethodHandle memberHandle, ParameterizedSymbolKind parameterizedSymbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            ArgumentOutOfRangeException.ThrowIfNegative(position, paramName: nameof(position));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(memberHandle, nameof(memberHandle));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<ParameterizedSymbolKind>(parameterizedSymbolKind, nameof(parameterizedSymbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumEqualsAny(
                parameterizedSymbolKind,
                [ParameterizedSymbolKind.Undefined],
                nameof(parameterizedSymbolKind), $"Invalid argument '{nameof(parameterizedSymbolKind)}'. The value '{nameof(ParameterizedMemberKind)}.{ParameterizedSymbolKind.Undefined}' is not allowed.");

            return new SymbolInfoDataCacheKey(parameterName,
                string.Empty,
                declaringTypeHandle,
                default,
                memberHandle,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                position,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolKind.Parameter,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(symbolName, nameof(symbolName));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumNotEqualsAny(symbolKind, [SymbolKind.MemberEvent, SymbolKind.MemberField], nameof(symbolKind), "The symbol kind must be 'MemberEvent' or 'MemberField' for anonymous event or field symbols.");

            return new SymbolInfoDataCacheKey(symbolName,
                string.Empty,
                declaringTypeHandle,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                SymbolInfoDataCacheKey.UnknownParameterCountOrPosition,
                symbolKind,
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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

                int hashCode = 1248511333;
                hashCode = ((hashCode * -1521134295) + this.SymbolName?.GetHashCode(StringComparison.Ordinal)) ?? 1521134295;
                hashCode = (hashCode * -1521134295) + this.DeclaringTypeHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.SymbolTypeHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.MethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.FieldHandle.GetHashCode();
                //hashCode = (hashCode * -1521134295) + this.GetMethodHandle.GetHashCode();
                //hashCode = (hashCode * -1521134295) + this.SetMethodHandle.GetHashCode();
                //hashCode = (hashCode * -1521134295) + this.AddMethodHandle.GetHashCode();
                //hashCode = (hashCode * -1521134295) + this.RemoveMethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.SymbolKind.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.GenericTypeParameterCount.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.ParameterPosition.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.IsAnonymousSymbolKey.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.ParameterMemberParameterCount.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.ParameterizedMemberKind.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.ParameterKind.GetHashCode();
                hashCode = ((hashCode * -1521134295) + this.ParameterMemberName?.GetHashCode(StringComparison.Ordinal)) ?? 1521134295;

                foreach (ParameterData parameterData in this.ParameterList)
                {
                    hashCode = (hashCode * -1521134295) + parameterData.ParameterTypeHandle.GetHashCode();
                    hashCode = (hashCode * -1521134295) + parameterData.DeclaringTypeHandle.GetHashCode();
                    hashCode = (hashCode * -1521134295) + parameterData.Position.GetHashCode();
                }

                foreach (MethodParameterInfo parameterData in this.MethodParameterInfoList)
                {
                    hashCode = (hashCode * -1521134295) + parameterData.ParameterTypeHandle.GetHashCode();
                    hashCode = (hashCode * -1521134295) + parameterData.DeclaringTypeHandle.GetHashCode();
                    hashCode = (hashCode * -1521134295) + parameterData.Position.GetHashCode();
                }

                return hashCode;
            }
        }

        public override bool Equals(object obj) => obj is SymbolInfoDataCacheKey other && Equals(other);

        public bool Equals(SymbolInfoDataCacheKey other) => this.SymbolName == other.SymbolName
            && this.DeclaringTypeHandle == other.DeclaringTypeHandle
            && this.SymbolTypeHandle == other.SymbolTypeHandle
            && this.MethodHandle == other.MethodHandle
            && this.FieldHandle == other.FieldHandle
            //&& this.GetMethodHandle == other.GetMethodHandle
            //&& this.SetMethodHandle == other.SetMethodHandle
            //&& this.AddMethodHandle == other.AddMethodHandle
            //&& this.RemoveMethodHandle == other.RemoveMethodHandle
            && this.SymbolKind == other.SymbolKind
            && this.GenericTypeParameterCount == other.GenericTypeParameterCount
            && this.ParameterPosition == other.ParameterPosition
            && this.ParameterList.Equals(other.ParameterList)
            && this.MethodParameterInfoList.Equals(other.MethodParameterInfoList)
            && this.ParameterMemberParameterCount == other.ParameterMemberParameterCount
            && this.ParameterMemberName.Equals(other.ParameterMemberName, StringComparison.OrdinalIgnoreCase)
            && this.ParameterizedMemberKind == other.ParameterizedMemberKind
            && this.ParameterKind == other.ParameterKind
            && this.IsAnonymousSymbolKey == other.IsAnonymousSymbolKey;

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
