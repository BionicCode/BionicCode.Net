namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

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
        /// Gets the name of the symbol represented by this instance.
        /// </summary>
        /// <value>The name of the symbol, such as the method name, property name, event name, field name, or type name.</value>
        public readonly string SymbolName { get; }
        /// <summary>
        /// Gets the runtime handle for the type that declares the current member.
        /// </summary>
        /// <remarks>Use this property to obtain a low-level identifier for the declaring type, which can
        /// be used with reflection APIs that require a RuntimeTypeHandle. The value is typically used for advanced
        /// scenarios involving type metadata or dynamic type operations.</remarks>
        /// <value>The runtime type handle of the declaring type.</value>
        public readonly RuntimeTypeHandle DeclaringTypeHandle { get; }
        /// <summary>
        /// Gets the runtime type handle that represents the symbol's type.
        /// </summary>
        /// <remarks>In case of a method this property returns the method type (return type of the method). For events, this returns the type handle of the event delegate. And for properties and fields this is the simple handle of the field/property type.</remarks>
        /// <value>The runtime type handle of the symbol's type. In case of a method this property returns the method type (return type of the method).
        /// For events, this returns the type handle of the event delegate.
        /// And for properties and fields this is the simple handle of the field/property type.<br/>
        /// For anonymous members (if the key was created with the <see cref="CreateForAnonymousSymbol(RuntimeTypeHandle, string, ParameterList?, SymbolKind)"/> method, the value is <see langword="default"/>.</value>
        public readonly RuntimeTypeHandle SymbolTypeHandle { get; }
        /// <summary>
        /// Gets the handle that represents the internal metadata of the method.
        /// </summary>
        /// <remarks>The method handle provides a low-level representation of the method's metadata and
        /// can be used for advanced reflection scenarios. Accessing the handle is typically only necessary when
        /// interoperating with unmanaged code or performing operations that require direct access to method
        /// metadata.</remarks>
        /// <value>The runtime method handle of the method. For anonymous members (if the key was created with the <see cref="CreateForAnonymousSymbol(RuntimeTypeHandle, string, ParameterList?, SymbolKind)"/> method, the value is <see langword="default"/>.</value>
        public readonly RuntimeMethodHandle MethodHandle { get; }
        /// <summary>
        /// Gets a handle to the internal metadata representation of the field.
        /// </summary>
        /// <remarks>The returned handle can be used for low-level reflection operations or
        /// interoperability scenarios. The value is primarily intended for advanced scenarios and should be used with
        /// care, as it exposes runtime-specific details.</remarks>
        /// <value>The runtime field handle of the field. For non-field members the value is <see langword="default"/>.</value>
        public readonly RuntimeFieldHandle FieldHandle { get; }
        /// <summary>
        /// Gets the handle to the underlying runtime method used to get the value of the property.
        /// </summary>
        /// <remarks>The method handle can be used to access low-level metadata or invoke the method via
        /// reflection. The value is valid only while the associated type is loaded and may become invalid if the type
        /// is unloaded.</remarks>
        /// <value>The runtime method handle of the get accessor method. For non-property members the value is <see langword="default"/>.</value>
        public readonly RuntimeMethodHandle GetMethodHandle { get; }
        /// <summary>
        /// Gets the handle to the underlying runtime method used to set the value of the property.
        /// </summary>
        /// <value>The runtime method handle of the set accessor method. For non-property members the value is <see langword="default"/>.</value>
        public readonly RuntimeMethodHandle SetMethodHandle { get; }
        /// <summary>
        /// Gets the runtime method handle for the add accessor of the event.
        /// </summary>
        /// <value>The runtime method handle of the add accessor method. For non-event members the value is <see langword="default"/>.</value>
        public readonly RuntimeMethodHandle AddMethodHandle { get; }
        /// <summary>
        /// Gets the runtime method handle for the remove accessor of the event.
        /// </summary>
        /// <value>The runtime method handle of the remove accessor method. For non-event members the value is <see langword="default"/>.</value>
        public readonly RuntimeMethodHandle RemoveMethodHandle { get; }
        /// <summary>
        /// Gets the kind of symbol represented by this instance.
        /// </summary>
        /// <value>The kind of symbol, such as type, method, property, event, field, constructor, or parameter.</value>
        public readonly SymbolKind SymbolKind { get; }
        /// <summary>
        /// Gets the list of parameters associated with the current member.
        /// </summary>
        /// <value>The list of parameters for methods, properties (indexers), and constructors. For non-parameterized members, this is an empty list.</value>
        public ParameterList ParameterList { get; }

        public MethodParameterInfoList MethodParameterInfos { get; }
        /// <summary>
        /// Gets the number of generic type parameters defined for the current type or method.
        /// </summary>
        /// <value>The count of generic type parameters. For non-generic types or methods, this value defaults to <c>-1</c>. The value will only be &gt; -1 if the key was created with the <see cref="CreateForAnonymousMemberSymbol(RuntimeTypeHandle, string, ParameterList?, int, int, SymbolKind)"/> method. In this case the value is the provided generic type parameter count but never &lt; 0.</value>
        public int GenericTypeParameterCount { get; }
        /// <summary>
        /// Gets the zero-based position of the parameter in the parameter list.
        /// </summary>
        /// <value>The position of the parameter. For non-parameter symbols, this value defaults to <c>-1</c>. The value will only be &gt; -1 if the key was created with the <see cref="CreateForAnonymousMemberSymbol(RuntimeTypeHandle, string, ParameterList?, int, int, SymbolKind)"/> method for a parameter symbol.</value>
        public int ParameterPosition { get; }

        private readonly int _hashCode;
        public bool IsAnonymousSymbolKey { get; }

        private SymbolInfoDataCacheKey(string name,
            RuntimeTypeHandle declaringTypeHandle,
            RuntimeTypeHandle typeHandle,
            RuntimeMethodHandle methodHandle,
            RuntimeFieldHandle fieldHandle,
            RuntimeMethodHandle getMethodHandle,
            RuntimeMethodHandle setMethodHandle,
            RuntimeMethodHandle addMethodHandle,
            RuntimeMethodHandle removeMethodHandle,
            ParameterList parameterList,
            MethodParameterInfoList methodParameterInfos,
            int parameterPosition,
            int genericTypeParameterCount,
            SymbolKind symbolKind,
            bool isAnonymousSymbolKey)
        {
            this.SymbolName = name;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.SymbolTypeHandle = typeHandle;
            this.MethodHandle = methodHandle;
            this.FieldHandle = fieldHandle;
            this.GetMethodHandle = getMethodHandle;
            this.SetMethodHandle = setMethodHandle;
            this.AddMethodHandle = addMethodHandle;
            this.RemoveMethodHandle = removeMethodHandle;
            this.ParameterList = parameterList;
            this.MethodParameterInfos = methodParameterInfos;
            this.ParameterPosition = parameterPosition;
            this.GenericTypeParameterCount = genericTypeParameterCount;
            this.SymbolKind = symbolKind;
            this.IsAnonymousSymbolKey = isAnonymousSymbolKey;

            this._hashCode = ComputeHashCode();
        }

        public static SymbolInfoDataCacheKey CreateForEvent(EventInfo eventInfo)
        {
            ArgumentNullException.ThrowIfNull(eventInfo, nameof(eventInfo));
            RuntimeTypeHandle declaringTypeHandle = eventInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle eventDelegateTypeHandle = eventInfo.EventHandlerType.TypeHandle;
            RuntimeMethodHandle addMethodHandle = eventInfo.AddMethod.MethodHandle;
            RuntimeMethodHandle removeMethodHandle = eventInfo.RemoveMethod.MethodHandle;

            return new SymbolInfoDataCacheKey(eventInfo.Name,
                declaringTypeHandle,
                eventDelegateTypeHandle,
                default,
                default,
                default,
                default,
                addMethodHandle,
                removeMethodHandle,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                SymbolKind.MemberEvent,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForProperty(PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(propertyInfo, nameof(propertyInfo));
            RuntimeTypeHandle declaringTypeHandle = propertyInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle typeHandle = propertyInfo.PropertyType.TypeHandle;
            RuntimeMethodHandle getMethodHandle = propertyInfo.GetMethod?.MethodHandle ?? default;
            RuntimeMethodHandle setMethodHandle = propertyInfo.SetMethod?.MethodHandle ?? default;

            return new SymbolInfoDataCacheKey(propertyInfo.Name,
                declaringTypeHandle,
                typeHandle,
                default,
                default,
                getMethodHandle,
                setMethodHandle,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                SymbolKind.MemberProperty,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));
            RuntimeTypeHandle declaringTypeHandle = methodInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle typeHandle = methodInfo.ReturnType.TypeHandle;
            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(methodInfo.Name,
                declaringTypeHandle,
                typeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                SymbolKind.MemberMethod,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            RuntimeTypeHandle typeHandle = type.TypeHandle;
            RuntimeTypeHandle declaringTypeHandle = default;

            return new SymbolInfoDataCacheKey(type.FullName ?? type.Name,
                declaringTypeHandle,
                typeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                SymbolKind.Type,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));
            RuntimeTypeHandle declaringTypeHandle = fieldInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle fieldTypeHandle = fieldInfo.FieldType.TypeHandle;
            RuntimeFieldHandle handle = fieldInfo.FieldHandle;

            return new SymbolInfoDataCacheKey(fieldInfo.Name,
                declaringTypeHandle,
                fieldTypeHandle,
                default,
                handle,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                SymbolKind.MemberField,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullException.ThrowIfNull(constructorInfo, nameof(constructorInfo));
            RuntimeTypeHandle declaringTypeHandle = constructorInfo.DeclaringType.TypeHandle;
            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(constructorInfo.Name,
                declaringTypeHandle,
                default,
                methodHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                SymbolKind.Constructor,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForParameter(ParameterInfo parameterInfo)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            RuntimeTypeHandle declaringTypeHandle = parameterInfo.Member.DeclaringType.TypeHandle;
            RuntimeTypeHandle parameterTypeHandle = parameterInfo.ParameterType.TypeHandle;
            RuntimeMethodHandle methodHandle = parameterInfo.Member is MethodBase methodInfo
                ? methodInfo.MethodHandle
                : default;

            return new SymbolInfoDataCacheKey(parameterInfo.Name,
                declaringTypeHandle,
                parameterTypeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                parameterInfo.Position,
                -1,
                SymbolKind.MemberParameter,
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
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="genericTypeParameterCount"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, ParameterList? symbolParameters, int genericTypeParameterCount, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberMethod, SymbolKind.Constructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, paramName: nameof(genericTypeParameterCount));

            return new SymbolInfoDataCacheKey(memberName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                symbolParameters ?? ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                genericTypeParameterCount,
                symbolKind,
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
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="genericTypeParameterCount"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, MethodParameterInfoList? symbolParameters, int genericTypeParameterCount, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberMethod, SymbolKind.Constructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));

            return new SymbolInfoDataCacheKey(memberName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                symbolParameters ?? MethodParameterInfoList.Empty,
                -1,
                genericTypeParameterCount,
                symbolKind,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous property using the specified declaring type, symbol name, parameters, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.</param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="indexerParameters">The list of parameters for the anonymous indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberProperty"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous property.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberProperty"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousProperty(RuntimeTypeHandle declaringTypeHandle, string propertyName, ParameterList? indexerParameters, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberProperty], nameof(symbolKind), "The symbol kind must be 'MemberProperty' for anonymous property symbols.");

            return new SymbolInfoDataCacheKey(propertyName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                indexerParameters ?? ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                symbolKind,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous property using the specified declaring type, symbol name, parameters, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous property.</param>
        /// <param name="propertyName">The name of the anonymous property. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="indexerParameters">The list of parameters for the anonymous indexer property. Can be <see cref="MethodParameterInfoList.Empty"/> or <see langword="null"/> to indicate no parameters in case of a normal property.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberProperty"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous property.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="propertyName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberProperty"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousProperty(RuntimeTypeHandle declaringTypeHandle, string propertyName, MethodParameterInfoList? indexerParameters, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberProperty], nameof(symbolKind), "The symbol kind must be 'MemberProperty' for anonymous property symbols.");

            return new SymbolInfoDataCacheKey(propertyName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                indexerParameters ?? MethodParameterInfoList.Empty,
                -1,
                -1,
                symbolKind,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous parameter using the specified declaring type, symbol name, parameter position,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="ParameterInfo"/> and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="parameterName">The name of the anonymous parameter. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="position">The index of the parameter.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberParameter"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous parameter.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberParameter"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousParameter(RuntimeTypeHandle declaringTypeHandle, string parameterName, int position, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberParameter], nameof(symbolKind), "The symbol kind must be 'MemberParameter' for anonymous parameter symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(position, paramName: nameof(position));

            return new SymbolInfoDataCacheKey(parameterName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                position,
                -1,
                symbolKind,
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
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberEvent"/> or <see cref="SymbolKind.MemberField"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousFieldOrEvent(RuntimeTypeHandle declaringTypeHandle, string symbolName, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbolName, nameof(symbolName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionEx.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberEvent, SymbolKind.MemberField], nameof(symbolKind), "The symbol kind must be 'MemberEvent' or 'MemberField' for anonymous event or field symbols.");

            return new SymbolInfoDataCacheKey(symbolName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                -1,
                -1,
                symbolKind,
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
                hashCode = (hashCode * -1521134295) + this.SymbolName.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.DeclaringTypeHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.SymbolTypeHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.MethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.FieldHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.GetMethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.SetMethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.AddMethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.RemoveMethodHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.SymbolKind.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.GenericTypeParameterCount.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.ParameterPosition.GetHashCode();
                hashCode = (hashCode * -1521134295) + this.IsAnonymousSymbolKey.GetHashCode();

                foreach (ParameterData parameterData in this.ParameterList)
                {
                    hashCode = (hashCode * -1521134295) + parameterData.ParameterTypeHandle.GetHashCode();
                    hashCode = (hashCode * -1521134295) + parameterData.DeclaringTypeHandle.GetHashCode();
                    hashCode = (hashCode * -1521134295) + parameterData.Position.GetHashCode();
                }

                foreach (MethodParameterInfo parameterData in this.MethodParameterInfos)
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
            && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.SymbolTypeHandle.Equals(other.SymbolTypeHandle)
            && this.MethodHandle.Equals(other.MethodHandle)
            && this.FieldHandle.Equals(other.FieldHandle)
            && this.GetMethodHandle.Equals(other.GetMethodHandle)
            && this.SetMethodHandle.Equals(other.SetMethodHandle)
            && this.AddMethodHandle.Equals(other.AddMethodHandle)
            && this.RemoveMethodHandle.Equals(other.RemoveMethodHandle)
            && this.SymbolKind == other.SymbolKind
            && this.GenericTypeParameterCount == other.GenericTypeParameterCount
            && this.ParameterPosition == other.ParameterPosition
            && this.ParameterList.Equals(other.ParameterList)
            && this.MethodParameterInfos.Equals(other.MethodParameterInfos)
            && this.IsAnonymousSymbolKey == other.IsAnonymousSymbolKey;

        public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
        public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
    }
}
