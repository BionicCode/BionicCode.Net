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
        public string ParameterMemberName { get; }
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
        /// For anonymous members (if the key was created with one of the <c>CreateForAnonymousSymbol()</c> overloads, the value is <see langword="default"/>.</value>
        public readonly RuntimeTypeHandle SymbolTypeHandle { get; }
        /// <summary>
        /// Gets the handle that represents the internal metadata of the method.
        /// </summary>
        /// <remarks>The method handle provides a low-level representation of the method's metadata and
        /// can be used for advanced reflection scenarios. Accessing the handle is typically only necessary when
        /// interoperating with unmanaged code or performing operations that require direct access to method
        /// metadata.</remarks>
        /// <value>The runtime method handle of the method. For anonymous members (if the key was created with one of the <c>CreateForAnonymousSymbol()</c> overloads, the value is <see langword="default"/> (except the overloads for a parameter).</value>
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
        /// <value>The count of generic type parameters. For non-generic types or methods, this value defaults to <c>-1</c>. The value will only be &gt; -1 if the key was created with the <see cref="CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle, string, MethodParameterInfoList?, int, SymbolKind)"/> and <see cref="CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle, string, ParameterList?, int, SymbolKind)"/> methods. In this case the value is the provided generic type parameter count but never &lt; 0.</value>
        public int GenericTypeParameterCount { get; }
        /// <summary>
        /// Gets the zero-based position of the parameter in the parameter list.
        /// </summary>
        /// <value>The position of the parameter. For non-parameter symbols, this value defaults to <c>-1</c>. The value will only be &gt; -1 if the key was created with the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, ParameterKind, string?, ParameterizedSymbolKind)"/> or <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle)"/> methods for a parameter symbol.</value>
        public int ParameterPosition { get; }

        /// <summary>
        /// Gets the kind of parameter represented by this instance.
        /// </summary>
        /// <value>Returns <see cref="ParameterKind.Undefined"/> except when explicitly set via the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, ParameterKind, string?, ParameterizedSymbolKind)"/> method to later help to resolve ambiguities.</value>
        public ParameterKind ParameterKind { get; }

        /// <summary>
        /// Gets the kind of parameterized symbol represented by this instance.
        /// </summary>
        /// <value>Returns <see cref="ParameterizedSymbolKind.Undefined"/> except when explicitly set via the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, ParameterKind, string?, ParameterizedSymbolKind)"/> method to later help to resolve ambiguities.</value>
        public ParameterizedSymbolKind ParameterizedSymbolKind { get; }

        private readonly int _hashCode;
        public bool IsAnonymousSymbolKey { get; }

        private SymbolInfoDataCacheKey(string name,
            string parameterMemberName,
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
            this.GetMethodHandle = getMethodHandle;
            this.SetMethodHandle = setMethodHandle;
            this.AddMethodHandle = addMethodHandle;
            this.RemoveMethodHandle = removeMethodHandle;
            this.ParameterList = parameterList;
            this.MethodParameterInfos = methodParameterInfos;
            this.ParameterPosition = parameterPosition;
            this.GenericTypeParameterCount = genericTypeParameterCount;
            this.SymbolKind = symbolKind;
            this.ParameterKind = parameterKind;
            this.ParameterizedSymbolKind = parameterizedSymbolKind;
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
                string.Empty,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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
                string.Empty,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));
            RuntimeTypeHandle declaringTypeHandle = methodInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle typeHandle = methodInfo.ReturnType.TypeHandle;
            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(methodInfo.Name,
                string.Empty,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            RuntimeTypeHandle typeHandle = type.TypeHandle;
            RuntimeTypeHandle declaringTypeHandle = default;

            return new SymbolInfoDataCacheKey(type.FullName ?? type.Name,
                string.Empty,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullException.ThrowIfNull(fieldInfo, nameof(fieldInfo));
            RuntimeTypeHandle declaringTypeHandle = fieldInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle fieldTypeHandle = fieldInfo.FieldType.TypeHandle;
            RuntimeFieldHandle handle = fieldInfo.FieldHandle;

            return new SymbolInfoDataCacheKey(fieldInfo.Name,
                string.Empty,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullException.ThrowIfNull(constructorInfo, nameof(constructorInfo));
            RuntimeTypeHandle declaringTypeHandle = constructorInfo.DeclaringType.TypeHandle;
            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;

            return new SymbolInfoDataCacheKey(constructorInfo.Name,
                string.Empty,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
                false);
        }

        public static SymbolInfoDataCacheKey CreateForParameter(ParameterInfo parameterInfo)
        {
            ArgumentNullException.ThrowIfNull(parameterInfo, nameof(parameterInfo));

            RuntimeTypeHandle declaringTypeHandle = parameterInfo.Member.DeclaringType.TypeHandle;
            RuntimeTypeHandle parameterTypeHandle = parameterInfo.ParameterType.TypeHandle;
            RuntimeMethodHandle methodHandle = parameterInfo.Member is MethodBase methodInfo // Method or Constructor parameter
                ? methodInfo.MethodHandle
                : parameterInfo.Member is PropertyInfo propertyInfo // Indexer parameter
                    ? propertyInfo.GetMethod?.MethodHandle ?? propertyInfo.SetMethod?.MethodHandle ?? default
                    : default;

            return new SymbolInfoDataCacheKey(parameterInfo.Name,
                parameterInfo.Member.Name,
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
                ParameterKind.Undefined,
                ParameterizedSymbolKind.Undefined,
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
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="genericTypeParameterCount"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, ParameterList? symbolParameters, int genericTypeParameterCount, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberMethod, SymbolKind.Constructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, paramName: nameof(genericTypeParameterCount));

            return new SymbolInfoDataCacheKey(memberName,
                string.Empty,
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
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous method, constructor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="memberName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not defined in <see cref="SymbolKind"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the provided <paramref name="symbolKind"/> value is not equal to <see cref="SymbolKind.MemberMethod"/> or <see cref="SymbolKind.Constructor"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="genericTypeParameterCount"/> is negative.</exception>"
        public static SymbolInfoDataCacheKey CreateForAnonymousMethodOrConstructor(RuntimeTypeHandle declaringTypeHandle, string memberName, MethodParameterInfoList? symbolParameters, int genericTypeParameterCount, SymbolKind symbolKind)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(memberName, nameof(memberName));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberMethod, SymbolKind.Constructor], nameof(symbolKind), "The symbol kind must be 'MemberMethod' or 'Constructor' for anonymous method symbols.");
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));

            return new SymbolInfoDataCacheKey(memberName,
                string.Empty,
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

            return new SymbolInfoDataCacheKey(propertyName,
                string.Empty,
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

            return new SymbolInfoDataCacheKey(propertyName,
                string.Empty,
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
        /// The parameter <paramref name="memberName"/> is optional. However, if not provided, the created key will be less efficient when used for lookups in caches. If parameter name and position matches multiple parameters, providing <paramref name="memberName"/> or even better the member's runtime handle via the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle)"/> overload will allow to resolve ambiguities that otherwise may throw an exception.
        /// <para/>For better performance, the <paramref name="memberName"/> must be provided.
        /// <br/>For best perfromance the overload <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle)"/> should be used.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="parameterName">The name of the anonymous parameter. </param>
        /// <param name="position">The index of the parameter.</param>
        /// <param name="parameterKind">Optional.Must be provided to avoid ambiguity which can throw exceptions during lookup when using this key.</param>
        /// <param name="memberName">Optional. The name of the member that declares the parameter. You should provide the member name to improve performance. For best efficiency, use the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, RuntimeMethodHandle)"/> overload instead.
        /// <br/>If the parameter belongs to an indexer property, the <paramref name="memberName"/> can be null, empty, or consist only of white-space characters (in this case <paramref name="parameterizedSymbolKind"/> must be <see cref="ParameterizedSymbolKind.MemberIndexerProperty"/>. For indexer properties this value will be ignored.).
        /// For indexer properties, the value must be <see cref="HelperExtensionsCommon.IndexerName"/>.</param>
        /// <param name="parameterizedSymbolKind">Optional. Provides a hint about the kind of memeber that the parameter belongs to. Should be provided too improve efficiency of the key.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous parameter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is negative.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousParameter(RuntimeTypeHandle declaringTypeHandle, string parameterName, int position, ParameterKind parameterKind = ParameterKind.Undefined, string? memberName = null, ParameterizedSymbolKind parameterizedSymbolKind = ParameterizedSymbolKind.Undefined)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            ArgumentOutOfRangeException.ThrowIfNegative(position, paramName: nameof(position));

            return new SymbolInfoDataCacheKey(parameterName,
                memberName ?? string.Empty,
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
                SymbolKind.MemberParameter,
                parameterKind,
                parameterizedSymbolKind,
                true);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous parameter using the specified declaring type, symbol name, parameter position and a declaring memeber reference.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="ParameterInfo"/> and instead only signature information is available.<para/>
        /// The parameter <paramref name="memberHandle"/> is not optionala nd therefore <see langword="default"/> is not a valid value. If parameter name and position matches multiple parameters, providing <paramref name="memberHandle"/> or the member name via the <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, ParameterKind, string?, ParameterizedSymbolKind)"/> overload will allow to resolve ambiguities that otherwise may throw an exception.
        /// For best performance, the <paramref name="memberHandle"/> must be provided.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the member that defines the anonymous parameter.</param>
        /// <param name="parameterName">The name of the anonymous parameter. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="position">The index of the parameter.</param>
        /// <param name="memberHandle">For best performance provide a <see cref="RuntimeMethodHandle"/> to the method or constructor that defines the parameter should be provided. Alternatively, call <see cref="CreateForAnonymousParameter(RuntimeTypeHandle, string, int, ParameterKind, string?, ParameterizedSymbolKind)"/> which accepts the defining member name but will perform worse, but still better than the case where a member handle or member name are not provided.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous parameter.</returns>
        /// <remarks></remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="declaringTypeHandle"/> is <see langword="default"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="parameterName"/> is null, empty, or consists only of white-space characters</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is negative.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="memberHandle"/> is <see langword="default"/>.</exception>
        public static SymbolInfoDataCacheKey CreateForAnonymousParameter(RuntimeTypeHandle declaringTypeHandle, string parameterName, int position, RuntimeMethodHandle memberHandle)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(parameterName, nameof(parameterName));
            ArgumentOutOfRangeException.ThrowIfNegative(position, paramName: nameof(position));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringTypeHandle, nameof(declaringTypeHandle));
            ArgumentNullExceptionAdvanced.ThrowIfDefault(memberHandle, nameof(memberHandle));

            return new SymbolInfoDataCacheKey(parameterName,
                string.Empty,
                declaringTypeHandle,
                default,
                memberHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                MethodParameterInfoList.Empty,
                position,
                -1,
                SymbolKind.MemberParameter,
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
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotEqual(symbolKind, [SymbolKind.MemberEvent, SymbolKind.MemberField], nameof(symbolKind), "The symbol kind must be 'MemberEvent' or 'MemberField' for anonymous event or field symbols.");

            return new SymbolInfoDataCacheKey(symbolName,
                string.Empty,
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
