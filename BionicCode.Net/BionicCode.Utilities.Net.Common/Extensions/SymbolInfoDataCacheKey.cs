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
        /// Gets the kind of parameter represented by this instance.
        /// </summary>
        /// <value>The kind of parameter, such as regular, out, ref, or params. For non-field members the value is <see cref="ParameterKind.Undefined"/>.</value>
        public ParameterKind ParameterKind { get; }
        /// <summary>
        /// Gets the list of parameters associated with the current member.
        /// </summary>
        /// <value>The list of parameters for methods, properties (indexers), and constructors. For non-parameterized members, this is an empty list.</value>
        public ParameterList ParameterList { get; }

        private SymbolInfoDataCacheKey(string name,
            RuntimeTypeHandle declaringTypeHandle,
            RuntimeTypeHandle typeHandle,
            RuntimeMethodHandle methodHandle,
            RuntimeMethodHandle getMethodHandle,
            RuntimeMethodHandle setMethodHandle,
            RuntimeMethodHandle addMethodHandle,
            RuntimeMethodHandle removeMethodHandle,
            ParameterList parameterList,
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
            this.SymbolKind = symbolKind;
        }

        public static SymbolInfoDataCacheKey CreateForEvent(EventInfo eventInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(eventInfo, nameof(eventInfo));
            RuntimeTypeHandle declaringTypeHandle = eventInfo.DeclaringType.TypeHandle;
            RuntimeTypeHandle eventDelegateTypeHandle = eventInfo.EventHandlerType.TypeHandle;
            string name = eventInfo.Name;
            RuntimeMethodHandle addMethodHandle = eventInfo.AddMethod.MethodHandle;
            RuntimeMethodHandle removeMethodHandle = eventInfo.RemoveMethod.MethodHandle;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                eventDelegateTypeHandle,
                default,
                default,
                default,
                addMethodHandle,
                removeMethodHandle,
                ParameterList.Empty,
                SymbolKind.MemberEvent);
        }

        public static SymbolInfoDataCacheKey CreateForProperty(PropertyInfo propertyInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(propertyInfo, nameof(propertyInfo));
            RuntimeTypeHandle declaringTypeHandle = propertyInfo.DeclaringType.TypeHandle;
            string name = propertyInfo.Name;
            RuntimeTypeHandle typeHandle = propertyInfo.PropertyType.TypeHandle;
            RuntimeMethodHandle getMethodHandle = propertyInfo.GetMethod?.MethodHandle ?? default;
            RuntimeMethodHandle setMethodHandle = propertyInfo.SetMethod?.MethodHandle ?? default;
            ParameterList parameterList = ParameterListBuilder.Create(propertyInfo.GetIndexParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                typeHandle,
                default,
                getMethodHandle,
                setMethodHandle,
                default,
                default,
                parameterList,
                SymbolKind.MemberProperty);
        }

        public static SymbolInfoDataCacheKey CreateForMethod(MethodInfo methodInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(methodInfo, nameof(methodInfo));
            RuntimeTypeHandle declaringTypeHandle = methodInfo.DeclaringType.TypeHandle;
            string name = methodInfo.Name;
            RuntimeTypeHandle typeHandle = methodInfo.ReturnType.TypeHandle;
            RuntimeMethodHandle methodHandle = methodInfo.MethodHandle;
            ParameterList parameterList = ParameterListBuilder.Create(methodInfo.GetParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                typeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList,
                SymbolKind.MemberMethod);
        }

        public static SymbolInfoDataCacheKey CreateForAnonymousSymbol(RuntimeTypeHandle declaringTypeHandle, string symbolName, ParameterList? symbolParameters, SymbolKind symbolKind)
        {
            ArgumentNullExceptionEx.ThrowIfNullOrWhiteSpace(symbolName, nameof(symbolName));

            return new SymbolInfoDataCacheKey(symbolName,
                declaringTypeHandle,
                default,
                default,
                default,
                default,
                default,
                default,
                symbolParameters ?? ParameterList.Empty,
                symbolKind);
        }

        public static SymbolInfoDataCacheKey CreateForType(Type type)
        {
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            if (type.IsDelegate())
            {
                MethodInfo invokeMethod = type.GetMethod("Invoke");

                return CreateForMethod(invokeMethod);
            }

            RuntimeTypeHandle typeHandle = type.TypeHandle;
            RuntimeTypeHandle declaringTypeHandle = default;
            ParameterList parameterList = ParameterList.Empty;
            string name = type.FullName ?? type.Name;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                typeHandle,
                default,
                default,
                default,
                default,
                default,
                parameterList,
                SymbolKind.Type);
        }

        public static SymbolInfoDataCacheKey CreateForField(FieldInfo fieldInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(fieldInfo, nameof(fieldInfo));
            RuntimeTypeHandle declaringTypeHandle = fieldInfo.DeclaringType.TypeHandle;
            string name = fieldInfo.Name;
            RuntimeTypeHandle fieldTypeHandle = fieldInfo.FieldType.TypeHandle;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                fieldTypeHandle,
                default,
                default,
                default,
                default,
                default,
                ParameterList.Empty,
                SymbolKind.MemberField);
        }

        public static SymbolInfoDataCacheKey CreateForConstructor(ConstructorInfo constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));
            RuntimeTypeHandle declaringTypeHandle = constructorInfo.DeclaringType.TypeHandle;
            string name = constructorInfo.Name;
            RuntimeMethodHandle methodHandle = constructorInfo.MethodHandle;
            ParameterList parameterList = ParameterListBuilder.Create(constructorInfo.GetParameters());
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                default,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList,
                SymbolKind.Constructor);
        }

        public static SymbolInfoDataCacheKey CreateForParameter(ParameterInfo parameterInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(parameterInfo, nameof(parameterInfo));
            RuntimeTypeHandle declaringTypeHandle = parameterInfo.Member.DeclaringType.TypeHandle;
            RuntimeTypeHandle parameterTypeHandle = parameterInfo.ParameterType.TypeHandle;
            string name = parameterInfo.Name;
            RuntimeMethodHandle methodHandle = parameterInfo.Member is MethodBase methodInfo
                ? methodInfo.MethodHandle
                : default;
            ParameterList parameterList = ParameterList.Empty;
            return new SymbolInfoDataCacheKey(name,
                declaringTypeHandle,
                parameterTypeHandle,
                methodHandle,
                default,
                default,
                default,
                default,
                parameterList,
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

            foreach (object argument in this.ParameterList)
            {
                hashCode = (hashCode * -1521134295) + argument.GetHashCode();
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
            && this.ParameterList.Equals(other.ParameterList);

        public static bool operator ==(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => left.Equals(right);
        public static bool operator !=(SymbolInfoDataCacheKey left, SymbolInfoDataCacheKey right) => !(left == right);
    }
}
