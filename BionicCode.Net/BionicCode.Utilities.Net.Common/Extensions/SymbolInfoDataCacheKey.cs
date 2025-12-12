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
        /// <summary>
        /// Gets the number of generic type parameters defined for the current type or method.
        /// </summary>
        /// <value>The count of generic type parameters. For non-generic types or methods, this value defaults to <c>-1</c>. The value will only be &gt; -1 if the key was created with the <see cref="CreateForAnonymousSymbol(RuntimeTypeHandle, string, ParameterList?, int, SymbolKind)"/> method. In this case the value is the provided generic type parameter count but never &lt; 0.</value>
        public int GenericTypeParameterCount { get; }

        public int ParameterPosition { get; }

        private readonly int _hashCode;

        private SymbolInfoDataCacheKey(string name,
            RuntimeTypeHandle declaringTypeHandle,
            RuntimeTypeHandle typeHandle,
            RuntimeMethodHandle methodHandle,
            RuntimeMethodHandle getMethodHandle,
            RuntimeMethodHandle setMethodHandle,
            RuntimeMethodHandle addMethodHandle,
            RuntimeMethodHandle removeMethodHandle,
            ParameterList parameterList,
            int parameterPosition,
            int genericTypeParameterCount,
            SymbolKind symbolKind)
        {
            this.SymbolName = name;
            this.DeclaringTypeHandle = declaringTypeHandle;
            this.SymbolTypeHandle = typeHandle;
            this.MethodHandle = methodHandle;
            this.GetMethodHandle = getMethodHandle;
            this.SetMethodHandle = setMethodHandle;
            this.AddMethodHandle = addMethodHandle;
            this.RemoveMethodHandle = removeMethodHandle;
            this.ParameterList = parameterList;
            this.ParameterPosition = parameterPosition;
            this.GenericTypeParameterCount = genericTypeParameterCount;
            this.SymbolKind = symbolKind;

            this._hashCode = GetHashCode();
        }

        public static SymbolInfoDataCacheKey CreateForEvent(EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));
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
                addMethodHandle,
                removeMethodHandle,
                ParameterList.Empty,
                -1,
                -1,
                SymbolKind.MemberEvent);
        }

        public static SymbolInfoDataCacheKey CreateForProperty(PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));
            RuntimeTypeHandle declaringTypeHandle = propertyInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle typeHandle = propertyInfo.PropertyType.TypeHandle;
            RuntimeMethodHandle getMethodHandle = propertyInfo.GetMethod?.MethodHandle ?? default;
            RuntimeMethodHandle setMethodHandle = propertyInfo.SetMethod?.MethodHandle ?? default;

            return new SymbolInfoDataCacheKey(propertyInfo.Name,
                declaringTypeHandle,
                typeHandle,
                default,
                getMethodHandle,
                setMethodHandle,
                default,
                default,
                ParameterList.Empty,
                -1,
                -1,
                SymbolKind.MemberProperty);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));
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
                ParameterList.Empty,
                -1,
                -1,
                SymbolKind.MemberMethod);
        }

        /// <summary>
        /// Creates a new cache key for an anonymous symbol using the specified declaring type, symbol name, parameters,
        /// generic type parameter count, and symbol kind.
        /// </summary>
        /// <remarks>This method is used to create a unique cache key for symbols of which the caller does not have a direct representation (e.g. a <see cref="MethodInfo"/> or <see cref="PropertyInfo"/>) and instead only signature information is available.</remarks>
        /// <param name="declaringTypeHandle">The runtime type handle representing the declaring type of the anonymous symbol.</param>
        /// <param name="symbolName">The name of the anonymous symbol. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="symbolParameters">The list of parameters for the anonymous symbol, or null to indicate no parameters.</param>
        /// <param name="genericTypeParameterCount">The number of generic type parameters for the anonymous symbol. Must be zero or greater.</param>
        /// <param name="symbolKind">The kind of symbol to associate with the cache key. Must be a defined value of the SymbolKind enumeration.</param>
        /// <returns>A new instance of <see cref="SymbolInfoDataCacheKey"/> representing the specified anonymous symbol.</returns>
        public static SymbolInfoDataCacheKey CreateForAnonymousSymbol(RuntimeTypeHandle declaringTypeHandle, string symbolName, ParameterList? symbolParameters, int genericTypeParameterCount, int parameterPosition, SymbolKind symbolKind)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(symbolName, nameof(symbolName));
            ArgumentExceptionEx.ThrowIfEnumIsNotDefined<SymbolKind>(symbolKind, nameof(symbolKind));
            ArgumentOutOfRangeException.ThrowIfLessThan(genericTypeParameterCount, 0, nameof(genericTypeParameterCount));

            return new SymbolInfoDataCacheKey(symbolName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                symbolParameters ?? ParameterList.Empty,
                parameterPosition,
                genericTypeParameterCount,
                symbolKind);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

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
                ParameterList.Empty,
                -1,
                -1,
                SymbolKind.Type);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));
            RuntimeTypeHandle declaringTypeHandle = fieldInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle fieldTypeHandle = fieldInfo.FieldType.TypeHandle;

            return new SymbolInfoDataCacheKey(fieldInfo.Name,
                declaringTypeHandle,
                fieldTypeHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                -1,
                -1,
                SymbolKind.MemberField);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));
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
                ParameterList.Empty,
                -1,
                -1,
                SymbolKind.Constructor);
        }

        public static SymbolInfoDataCacheKey CreateForParameter(ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(parameterInfo, nameof(parameterInfo));
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
                ParameterList.Empty,
                parameterInfo.Position,
                -1,
                SymbolKind.MemberParameter);
        }

        public override int GetHashCode()
        {
            int hashCode = 1248511333;
            hashCode = (hashCode * -1521134295) + this.SymbolName.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.DeclaringTypeHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.SymbolTypeHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.MethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.GetMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.SetMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.AddMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.RemoveMethodHandle.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.SymbolKind.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.GenericTypeParameterCount.GetHashCode();
            hashCode = (hashCode * -1521134295) + this.ParameterPosition.GetHashCode();

            foreach (ParameterData parameterData in this.ParameterList)
            {
                hashCode = (hashCode * -1521134295) + parameterData.ParameterTypeData.Handle.GetHashCode();
                hashCode = (hashCode * -1521134295) + parameterData.DeclaringTypeHandle.GetHashCode();
                hashCode = (hashCode * -1521134295) + parameterData.Position.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj) => obj is SymbolInfoDataCacheKey other && Equals(other);

        public bool Equals(SymbolInfoDataCacheKey other) => this.SymbolName == other.SymbolName
            && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle)
            && this.SymbolTypeHandle.Equals(other.SymbolTypeHandle)
            && this.MethodHandle.Equals(other.MethodHandle)
            && this.GetMethodHandle.Equals(other.GetMethodHandle)
            && this.SetMethodHandle.Equals(other.SetMethodHandle)
            && this.AddMethodHandle.Equals(other.AddMethodHandle)
            && this.RemoveMethodHandle.Equals(other.RemoveMethodHandle)
            && this.SymbolKind == other.SymbolKind
            && this.GenericTypeParameterCount == other.GenericTypeParameterCount
            && this.ParameterPosition == other.ParameterPosition
            && this.ParameterList.Equals(other.ParameterList);

        public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
        public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
    }
}
