[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net
{
    using System;
    using System.CodeDom;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal class TypeData : SymbolInfoData
    {
        private static readonly Type TaskType = typeof(Task);
        private static readonly Type ValueTaskType = typeof(ValueTask);
        private static readonly Type ValueTaskGenericType = typeof(ValueTask<>);
        private static readonly Type DelegateType = typeof(Delegate);

        private string? displayName;
        private string? shortDisplayName;
        private string? fullyQualifiedDisplayName;
        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? canDeclareExtensionMethod;
        private bool? isAwaitable;
        private bool? isAwaitableTask;
        private bool? isAwaitableValueTask;
        private string? signature;
        private string? shortSignature;
        private string? fullyQualifiedRuntimeSignature;
        private string? runtimeSignature;
        private string? runtimeShortSignature;
        private string? runtimeShortCompactSignature;
        private string? shortCompactSignature;
        private string? fullyQualifiedSignature;
        private bool? isStatic;
        private bool? isBuiltInType;
        private bool? isAbstract;
        private bool? isSealed;
        private bool? isValueType;
        private bool? isByRef;
        private bool? isSubclass;
        private bool? isDelegate;
        private bool? isGenericType;
        private bool? isGenericTypeDefinition;
        private TypeList? genericTypeArguments;
        private TypeList? genericParameterConstraintsData;
        private GenericParameterAttributes? genericParameterAttributes;
        private TypeData? genericTypeDefinitionData;
        private TypeData? baseTypeData;
        private TypeList? interfacesData;
        private PropertyList? propertiesData;
        private MethodList? methodsData;
        private FieldList? fieldsData;
        private EventList? eventsData;
        private ConstructorList? constructorsData;
        private IList<CustomAttributeData>? attributeData;
        private string? assemblyName;
        private MethodData? delegateInvokeMethodData;
        private SymbolComponentInfo? symbolComponentInfo;
        private SymbolComponentInfo? compactSymbolComponentInfo;
        private bool? containsGenericParameters;
        private readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData> memberTable;
        private bool isAllPropertiesGenerated;
        private bool isAllMethodsGenerated;
        private bool isAllFieldsGenerated;
        private bool isAllEventsGenerated;
        private bool isAllConstructorsGenerated;
        private bool? isByRefLike;
        private bool? isGenericTypeParameter;
        private bool? isGenericMethodParameter;
        private bool? isGenericParameter;
        private bool? _isEnum;
        private bool? _isClass;
        private bool? _isInterface;
        private bool? _isStruct;
        private bool? _isReadOnlyStruct;

        public TypeData(Type type, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(type.Name, SymbolKind.Type, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            this.Handle = type.TypeHandle;
            this.Namespace = type.Namespace ?? string.Empty;
            this.memberTable = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData>();
        }

        /// <summary>
        /// Returns the underlying <see cref="Type"/> represented by this handle.
        /// </summary>
        /// <returns>A <see cref="Type"/> object that is referenced by this handle.</returns>
        public Type UnwrapType()
          => Type.GetTypeFromHandle(this.Handle)!;

        public PropertyData GetProperty(string propertyName, params MethodParameterInfo[] indexerPropertyParameters)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

            MethodParameterInfoList indexerParameters = indexerPropertyParameters is null || indexerPropertyParameters.Length == 0
                ? MethodParameterInfoList.Empty
                : new MethodParameterInfoList(indexerPropertyParameters);
            ArgumentExceptionAdvanced.ThrowIfAny(
                indexerParameters,
                methodParameterInfo => !methodParameterInfo.DeclaringTypeHandle.Equals(this.Handle),
                nameof(indexerPropertyParameters),
                $"At least one item in the argument sequence '{nameof(indexerPropertyParameters)}' has a different value for the '{nameof(MethodParameterInfo)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type '{this.FullyQualifiedSignature}'.");

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousProperty(this.Handle, propertyName, indexerParameters);
            PropertyData propertyData = (PropertyData)this.memberTable.GetOrAdd(cacheKey, key => SymbolReflectionInfoCache.GetOrCreatePropertyDataCacheEntry(ref key));

            return propertyData;
        }

        /// <summary>
        /// Returns an enumerable collection of property metadata for the current type, using the specified binding
        /// flags to control which properties are included.
        /// </summary>
        /// <remarks>Properties are returned from a cache when available; otherwise, they are retrieved
        /// and cached on demand. Subsequent calls may be more efficient due to caching. The enumeration includes
        /// properties from base types according to the specified binding flags.</remarks>
        /// <param name="bindingFlags">A bitwise combination of BindingFlags values that determines which properties to include in the enumeration.
        /// The default value includes all public and non-public instance and static properties from the entire
        /// inheritance hierarchy.</param>
        /// <returns>An enumerable collection of PropertyData objects representing the properties of the current type that match
        /// the specified binding flags.</returns>
        public IEnumerable<PropertyData> EnumerateProperties(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            int cachedPropertyCount = 0;

            // Return already cached cachedProperties first
            IEnumerable<PropertyData> cachedProperties = this.memberTable.Values.OfType<PropertyData>();
            foreach (PropertyData cachedPropertyData in cachedProperties)
            {
                cachedPropertyCount++;
                yield return cachedPropertyData;
            }

            // If all cachedProperties are already generated, exit. Else generate the remaining cachedProperties.
            if (this.isAllPropertiesGenerated)
            {
                yield break;
            }

            IEnumerable<PropertyInfo> remainingProperties = UnwrapType().GetProperties(bindingFlags)
                .Skip(cachedPropertyCount);
            foreach (PropertyInfo propertyInfo in remainingProperties)
            {
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForProperty(propertyInfo);
                PropertyData propertyData = (PropertyData)this.memberTable.GetOrAdd(cacheKey, _ => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo));

                yield return propertyData;
            }

            this.isAllPropertiesGenerated = true;
        }

        public MethodData GetMethod(string methodName, int genericTypeParameterCount, params MethodParameterInfo[] parameterList)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));
            ArgumentException.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));

            MethodParameterInfoList symbolParameters = parameterList is null || parameterList.Length == 0
                ? MethodParameterInfoList.Empty
                : new MethodParameterInfoList(parameterList);
            ArgumentExceptionAdvanced.ThrowIfAny(
                symbolParameters,
                methodParameterInfo => !methodParameterInfo.DeclaringTypeHandle.Equals(this.Handle),
                nameof(parameterList),
                $"At least one item in the argument sequence '{nameof(parameterList)}' has a different value for the '{nameof(MethodParameterInfo)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type '{this.FullyQualifiedSignature}'.");

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousMethodOrConstructor(
                this.Handle,
                methodName,
                symbolParameters,
                genericTypeParameterCount,
                SymbolKind.MemberMethod);
            MethodData methodData = (MethodData)this.memberTable.GetOrAdd(cacheKey, key => SymbolReflectionInfoCache.GetOrCreateMethodDataCacheEntry(ref key));

            return methodData;
        }

        /// <summary>
        /// Returns an enumerable collection of method metadata for the current type, using the specified binding flags
        /// to control method selection.
        /// </summary>
        /// <remarks>The returned collection includes both cached and newly discovered methods. Methods
        /// are enumerated in the order they are retrieved. Subsequent calls may return cached results for previously
        /// enumerated methods. This method does not guarantee thread safety; concurrent access may require external
        /// synchronization.</remarks>
        /// <param name="bindingFlags">A bitwise combination of BindingFlags values that determines which methods to include in the enumeration.
        /// The default value includes all instance and static methods declared on the type and its base types.</param>
        /// <returns>An enumerable collection of MethodData objects representing the methods defined on the current type and its
        /// base types, as specified by the binding flags.</returns>
        public IEnumerable<MethodData> EnumerateMethods(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            int cachedMethodCount = 0;

            // Return already cached cachedMethods first
            IEnumerable<MethodData> cachedMethods = this.memberTable.Values.OfType<MethodData>();
            foreach (MethodData cachedMethodData in cachedMethods)
            {
                cachedMethodCount++;
                yield return cachedMethodData;
            }

            // If all cachedMethods are already generated, exit. Else generate the remaining cachedMethods.
            if (this.isAllMethodsGenerated)
            {
                yield break;
            }

            IEnumerable<MethodInfo> remainingMethods = UnwrapType().GetMethods(bindingFlags)
                .Skip(cachedMethodCount);
            foreach (MethodInfo methodInfo in remainingMethods)
            {
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForMethod(methodInfo);
                MethodData methodData = (MethodData)this.memberTable.GetOrAdd(cacheKey, _ => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo));

                yield return methodData;
            }

            this.isAllMethodsGenerated = true;
        }

        public FieldData GetField(string fieldName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousFieldOrEvent(
                this.Handle,
                fieldName,
                SymbolKind.MemberField);
            FieldData fieldData = (FieldData)this.memberTable.GetOrAdd(cacheKey, key => SymbolReflectionInfoCache.GetOrCreateFieldDataCacheEntry(ref key));

            return fieldData;
        }

        public IEnumerable<FieldData> EnumerateFields(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            int cachedFieldCount = 0;

            // Return already cached cachedFields first
            IEnumerable<FieldData> cachedFields = this.memberTable.Values.OfType<FieldData>();
            foreach (FieldData cachedFieldData in cachedFields)
            {
                cachedFieldCount++;
                yield return cachedFieldData;
            }

            // If all cachedFields are already generated, exit. Else generate the remaining cachedFields.
            if (this.isAllFieldsGenerated)
            {
                yield break;
            }

            IEnumerable<FieldInfo> remainingFields = UnwrapType().GetFields(bindingFlags)
                .Skip(cachedFieldCount);
            foreach (FieldInfo fieldInfo in remainingFields)
            {
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForField(fieldInfo);
                FieldData fieldData = (FieldData)this.memberTable.GetOrAdd(cacheKey, _ => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(fieldInfo));

                yield return fieldData;
            }

            this.isAllFieldsGenerated = true;
        }

        public EventData GetEvent(string eventName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousFieldOrEvent(
                this.Handle,
                eventName,
                SymbolKind.MemberEvent);
            EventData eventData = (EventData)this.memberTable.GetOrAdd(cacheKey, key => SymbolReflectionInfoCache.GetOrCreateEventDataCacheEntry(ref key));

            return eventData;
        }

        public IEnumerable<EventData> EnumerateEvents(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            int cachedEventCount = 0;

            // Return already cached cachedFields first
            IEnumerable<EventData> cachedEvents = this.memberTable.Values.OfType<EventData>();
            foreach (EventData cachedEventData in cachedEvents)
            {
                cachedEventCount++;
                yield return cachedEventData;
            }

            // If all events are already generated, exit. Else generate the remaining events.
            if (this.isAllEventsGenerated)
            {
                yield break;
            }

            IEnumerable<EventInfo> remainingEvents = UnwrapType().GetEvents(bindingFlags)
                .Skip(cachedEventCount);
            foreach (EventInfo eventInfo in remainingEvents)
            {
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventInfo);
                EventData eventData = (EventData)this.memberTable.GetOrAdd(cacheKey, _ => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo));

                yield return eventData;
            }

            this.isAllEventsGenerated = true;
        }

        public ConstructorData GetConstructor(string constructorName, int genericTypeParameterCount, params MethodParameterInfo[] parameterList)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));
            ArgumentException.ThrowIfNullOrWhiteSpace(constructorName, nameof(constructorName));

            MethodParameterInfoList symbolParameters = parameterList is null || parameterList.Length == 0
                ? MethodParameterInfoList.Empty
                : new MethodParameterInfoList(parameterList);
            ArgumentExceptionAdvanced.ThrowIfAny(
                symbolParameters,
                methodParameterInfo => !methodParameterInfo.DeclaringTypeHandle.Equals(this.Handle),
                nameof(parameterList),
                $"At least one item in the argument sequence '{nameof(parameterList)}' has a different value for the '{nameof(MethodParameterInfo)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type '{this.FullyQualifiedSignature}'.");

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousMethodOrConstructor(
                this.Handle,
                constructorName,
                symbolParameters,
                genericTypeParameterCount,
                SymbolKind.MemberConstructor);
            ConstructorData constructorData = (ConstructorData)this.memberTable.GetOrAdd(cacheKey, key => SymbolReflectionInfoCache.GetOrCreateConstructorDataCacheEntry(ref key));

            return constructorData;
        }

        public IEnumerable<ConstructorData> EnumerateConstructors(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            int cachedConstructorCount = 0;

            // Return already cached cachedConstructors first
            IEnumerable<ConstructorData> cachedConstructors = this.memberTable.Values.OfType<ConstructorData>();
            foreach (ConstructorData cachedConstructorData in cachedConstructors)
            {
                cachedConstructorCount++;
                yield return cachedConstructorData;
            }

            // If all cachedConstructors are already generated, exit. Else generate the remaining cachedConstructors.
            if (this.isAllConstructorsGenerated)
            {
                yield break;
            }

            IEnumerable<ConstructorInfo> remainingConstructors = UnwrapType().GetConstructors(bindingFlags)
                .Skip(cachedConstructorCount);
            foreach (ConstructorInfo constructorInfo in remainingConstructors)
            {
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructorInfo);
                ConstructorData constructorData = (ConstructorData)this.memberTable.GetOrAdd(cacheKey, _ => SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo));

                yield return constructorData;
            }

            this.isAllConstructorsGenerated = true;
        }

        public RuntimeTypeHandle Handle { get; }
        public string Namespace { get; }

        public bool IsAwaitable
          => this.isAwaitable ??= TypeData.IsTypeAwaitable(this);

        public bool IsAwaitableTask
          => this.isAwaitableTask ??= TypeData.IsTypeAwaitableTask(this);

        public bool IsAwaitableValueTask
          => this.isAwaitableValueTask ??= TypeData.IsTypeAwaitableValueTask(this);

        public TypeData GenericTypeDefinitionData
        {
            get
            {
                if (this.IsGenericTypeDefinition)
                {
                    return this;
                }
                else
                {
                    Type genericTypeDefinitionType = UnwrapType().GetGenericTypeDefinition();
                    this.genericTypeDefinitionData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericTypeDefinitionType);
                }

                return this.genericTypeDefinitionData;
            }
        }

        public TypeList GenericTypeArguments
            => this.genericTypeArguments ??= TypeListBuilder.CreateGenericTypeArgumentList(this);

        public bool CanDeclareExtensionMethod
          => (bool)(bool?)(this.canDeclareExtensionMethod ??= TypeData.CanDeclareExtensionMethods(this));

        public override IList<CustomAttributeData> AttributeData
          => this.attributeData ??= UnwrapType().GetCustomAttributesData();

        public AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = TypeData.GetAccessModifier(this))
          : this.accessModifier;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isCompact: false);

        public SymbolComponentInfo CompactSymbolComponentInfo
          => this.compactSymbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isCompact: true);

        public override string Signature
          => this.signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: true, isRuntimeSymbol: true);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isCompact: false, isRuntimeSymbol: false);

        public override string DisplayName
          => this.displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string ShortDisplayName
          => this.shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string AssemblyName
          => this.assemblyName ??= UnwrapType().Assembly.GetName().Name ?? string.Empty;

        public bool IsStatic
          => this.isStatic ??= TypeData.IsTypeStatic(this);

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = TypeData.GetAttributes(this))
          : this.symbolAttributes;

        public bool IsAbstract
          => this.isAbstract ??= UnwrapType().IsAbstract;

        public bool IsSealed
          => this.isSealed ??= UnwrapType().IsSealed;

        public bool IsByRef
          => this.isByRef ??= UnwrapType().IsByRef;

        public bool IsByRefLike
          => this.isByRefLike ??= UnwrapType().IsByRefLike;

        public bool IsDelegate
          => this.isDelegate ??= TypeData.IsTypeDelegate(UnwrapType());

        public bool IsClass
          => this._isClass ??= UnwrapType().IsClass;

        public bool IsInterface
            => this._isInterface ??= UnwrapType().IsInterface;

        public bool IsEnum
          => this._isEnum ??= UnwrapType().IsEnum;

        public bool IsValueType
          => this.isValueType ??= UnwrapType().IsValueType;

        public bool IsReferenceType
          => !this.IsValueType;

        public bool IsStruct
            => this._isStruct ??= !this.IsEnum && this.IsValueType;

        public bool IsReadOnlyStruct
            => this._isReadOnlyStruct ??= IsReadOnlyStructInternal(this);

        public bool IsSubclass
        {
            get
            {
                if (this.isSubclass is null)
                {
                    Type? baseType = UnwrapType().BaseType;
                    this.isSubclass = baseType is not null
                      && baseType != typeof(object)
                      && baseType != typeof(ValueType);
                }

                return (bool)this.isSubclass;
            }
        }

        public TypeData? BaseTypeData
        {
            get
            {
                if (this.baseTypeData is null && this.IsSubclass)
                {
                    Type? baseType = UnwrapType().BaseType;
                    this.baseTypeData = baseType is null
                        ? null
                        : SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(baseType);
                }

                return this.baseTypeData;
            }
        }

        /// <summary>
        /// If the TypeData represents a delegate, this property returns metadata information for the delegate's Invoke method.
        /// </summary>
        /// <remarks>This property is only valid when the current type represents a delegate. Accessing
        /// this property when the type is not a delegate will result in an exception.</remarks>
        /// <exception cref="InvalidOperationException">Thrown when the current TypeData does not represent a delegate type.</exception>
        public MethodData DelegateInvokeMethodData
        {
            get
            {
                if (!this.IsDelegate)
                {
                    throw new InvalidOperationException($"The current type is not a delegate. Call {nameof(this.IsDelegate)} before accessing this property to check whether the current type is a delegate.");
                }

                this.delegateInvokeMethodData ??= GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName, 0);

                return this.delegateInvokeMethodData;
            }
        }

        public bool IsGenericTypeParameter
          => this.isGenericTypeParameter ??= UnwrapType().IsGenericTypeParameter;

        public bool IsGenericMethodParameter
          => this.isGenericMethodParameter ??= UnwrapType().IsGenericMethodParameter;

        public bool IsGenericParameter
          => this.isGenericParameter ??= UnwrapType().IsGenericParameter;

        public bool IsGenericType
          => this.isGenericType ??= UnwrapType().IsGenericType;

        /// <summary>
        /// Gets a value indicating whether the type is a built-in .NET type.
        /// </summary>
        public bool IsBuiltInType
          => this.isBuiltInType ??= TypeData.IsTypeBuiltInType(this);

        public bool IsGenericTypeDefinition
          => this.isGenericTypeDefinition ??= UnwrapType().IsGenericTypeDefinition;

        public bool ContainsGenericParameters
          => this.containsGenericParameters ??= UnwrapType().ContainsGenericParameters;

        public GenericParameterAttributes GenericParameterAttributes
          => this.genericParameterAttributes ??= UnwrapType().GenericParameterAttributes;

        public TypeList GenericParameterConstraintsData
          => this.genericParameterConstraintsData ??= TypeListBuilder.CreateGenericTypeArgumentConstraintList(this);

        public TypeList InterfacesData
          => this.interfacesData ??= TypeListBuilder.CreateGenericTypeArgumentList(this);

        public PropertyList PropertiesData
          => this.propertiesData ??= PropertyListBuilder.Create(this);

        public MethodList MethodsData
          => this.methodsData ??= MethodListBuilder.Create(this);

        public FieldList FieldsData
          => this.fieldsData ??= FieldListBuilder.Create(this);

        public EventList EventsData
          => this.eventsData ??= EventListBuilder.Create(this);

        public ConstructorList ConstructorsData
          => this.constructorsData ??= ConstructorListBuilder.Create(this);

        private static bool IsTypeStatic(TypeData typeData)
          => typeData.IsAbstract && typeData.IsSealed;

        private static bool IsTypeBuiltInType(TypeData typeData)
        {
            var typeReference = new CodeTypeReference(typeData.UnwrapType());
            string typeName = HelperExtensionsCommon.CodeProvider.GetTypeOutput(typeReference);
            int typeNameStartIndex = typeName.LastIndexOf('.') + 1;
            if (typeNameStartIndex > 0)
            {
                typeName = typeName.Substring(typeNameStartIndex);
            }

            return !HelperExtensionsCommon.CodeProvider.IsValidIdentifier(typeName);
        }

        private static bool CanDeclareExtensionMethods(TypeData typeData)
        {
            Type typeInfo = typeData.UnwrapType();
            if (!typeData.IsStatic || typeInfo.IsNested || typeInfo.IsGenericType)
            {
                return false;
            }

            Attribute? typeExtensionAttribute = typeInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
            return typeExtensionAttribute != null;
        }

        /// <summary>
        /// Determines the symbol attributes for the specified type represented by the given TypeData instance.
        /// </summary>
        /// <remarks>The returned SymbolAttributes value may include multiple flags combined using a
        /// bitwise OR to represent all applicable characteristics of the type. This method does not perform validation
        /// on the input; callers should ensure that typeData is valid and represents a supported type.<para/>
        /// For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
        /// <param name="typeData">The TypeData instance representing the type for which to retrieve symbol attributes. Cannot be null.</param>
        /// <returns>A SymbolAttributes value that describes the kind and characteristics of the specified type, such as whether
        /// it is a class, struct, interface, enum, delegate, generic, static, abstract, or final. Returns
        /// SymbolAttributes.Undefined if the type does not match any recognized category.</returns>
        private static SymbolAttributes GetAttributes(TypeData typeData)
        {
            if (typeData.IsDelegate)
            {
                SymbolAttributes delegateAttributes = SymbolAttributes.Delegate;
                if (typeData.IsGenericType)
                {
                    delegateAttributes |= SymbolAttributes.Generic;
                }

                return delegateAttributes;
            }

            if (typeData.IsClass)
            {
                SymbolAttributes classAttributes = SymbolAttributes.Class;
                if (typeData.IsAbstract)
                {
                    classAttributes |= SymbolAttributes.Abstract;
                }

                if (typeData.IsSealed)
                {
                    classAttributes |= SymbolAttributes.Final;
                }

                if (typeData.IsStatic)
                {
                    classAttributes |= SymbolAttributes.Static;
                }

                if (typeData.IsGenericType)
                {
                    classAttributes |= SymbolAttributes.Generic;
                }

                return classAttributes;
            }

            if (typeData.IsInterface)
            {
                SymbolAttributes interfaceAttributes = SymbolAttributes.Interface;
                return interfaceAttributes;
            }

            if (typeData.IsEnum)
            {
                SymbolAttributes enumAttributes = SymbolAttributes.Enum;
                return enumAttributes;
            }

            if (typeData.IsValueType)
            {
                SymbolAttributes structAttributes = SymbolAttributes.Struct;

                if (typeData.IsGenericType)
                {
                    structAttributes |= SymbolAttributes.Generic;
                }

                if (typeData.IsByRefLike)
                {
                    structAttributes |= SymbolAttributes.ByReference;
                }

                bool isReadOnlyStruct = typeData.IsReadOnlyStruct;
                if (isReadOnlyStruct)
                {
                    structAttributes |= SymbolAttributes.Final;
                }

                return structAttributes;
            }

            return SymbolAttributes.Undefined;
        }

        private static bool IsTypeDelegate(Type type)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

            return TypeData.DelegateType.IsAssignableFrom(type);
        }

        private static bool IsReadOnlyStructInternal(TypeData typeData)
          => typeData.IsStruct && typeData.UnwrapType().GetCustomAttribute(HelperExtensionsCommon.IsReadOnlyAttributeType) != null;

        /// <summary>
        /// Checks if the provided <see cref="MethodInfo"/> belongs to an asynchronous/awaitable method.
        /// </summary>
        /// <param genericTypeParameterIdentifier="methodInfo">The <see cref="MethodInfo"/> to check if it belongs to an awaitable method.</param>
        /// <returns><see langword="true"/> if the associated method is awaitable. Otherwise <see langword="false"/>.</returns>
        /// <remarks>The method first checks if the return valueType is either <see cref="Task"/> or <see cref="ValueTask"/>. If that fails, it checks if the returned valueType (by compiler convention) exposes a "GetAwaiter" named method that returns an appropriate valueType (awaiter).
        /// <br/>If that fails too, it checks whether there exists any extension method named "GetAwaiter" for the returned valueType that would make the valueType awaitable. If this fails too, the method is not awaitable.</remarks>
        private static bool IsTypeAwaitable(TypeData typeData)
        {
            if (TypeData.IsTypeAwaitableTask(typeData) || TypeData.IsTypeAwaitableValueTask(typeData))
            {
                return true;
            }

            Type type = typeData.UnwrapType();
            if (type.GetMethod(nameof(Task.GetAwaiter)) != null)
            {
                return true;
            }

            /* The return valueType of the method is not directly returning an awaitable valueType.
               So, search for an extension method named "GetAwaiter" for the return valueType of the currently validated method that effectively converts the valueType into an awaitable object.
               By compiler convention the "GetAwaiter" method must return an awaiter object that implements the INotifyComplete interface
            */
            Assembly[] assemblies;
            try
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            catch (AppDomainUnloadedException)
            {
                // REVIEW::Maybe better throw? But in which context would this be used where AppDomain is already unloaded?
                // Considering that the type is not awaitable at this point could be wrong. Notifying the caller that the test could not be completely performed seems better.
                // On the other habd if exception is thrown the user operates on an invalid AppDomain anyway. So, maybe it is better to fail fast (if application not already crashed).
                return false;
            }

            foreach (Assembly assembly in assemblies)
            {
                Type[] exportedTypes;
                try
                {
                    exportedTypes = assembly.GetExportedTypes();
                }
                catch (FileNotFoundException)
                {
                    // REVIEW::Same reasoning: maybe better throw? 
                    // Considering that the type is not awaitable at this point could be wrong. Notifying the caller that the test could not be completely performed seems better.
                    continue;
                }
                catch (NotSupportedException)
                {
                    // REVIEW::Same reasoning: maybe better throw? 
                    // Considering that the type is not awaitable at this point could be wrong. Notifying the caller that the test could not be completely performed seems better.
                    continue;
                }

                foreach (Type exportedType in exportedTypes)
                {
                    if (!exportedType.CanDeclareExtensionMethods())
                    {
                        continue;
                    }

                    MethodInfo? extensionMethodInfo = exportedType.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { type }, null);
                    if (extensionMethodInfo == null
                      || !extensionMethodInfo.IsExtensionMethodOf(type))
                    {
                        continue;
                    }

                    if (extensionMethodInfo.ReturnType.GetProperty("IsCompleted") != null
                      && extensionMethodInfo.ReturnType.GetInterface(nameof(INotifyCompletion)) != null
                      && extensionMethodInfo.ReturnType.GetMethod("GetResult") is MethodInfo getResultMethodInfo
                      && getResultMethodInfo.GetParameters().Length == 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsTypeAwaitableTask(TypeData type)
          => TypeData.TaskType.IsAssignableFrom(type.UnwrapType())
            || (type.BaseTypeData?.UnwrapType() is Type baseType && TypeData.TaskType.IsAssignableFrom(baseType));

        private static bool IsTypeAwaitableValueTask(TypeData type)
          => TypeData.ValueTaskType == type.UnwrapType()
            || (type.IsGenericType && TypeData.ValueTaskGenericType == type.GenericTypeDefinitionData.UnwrapType());

        private static AccessModifier GetAccessModifier(TypeData typeData)
        {
            return typeData.IsPublic ? AccessModifier.Public
              : typeData.IsNestedPrivate ? AccessModifier.Private
              : typeData.IsNestedAssembly ? AccessModifier.Internal
              : typeData.IsNestedFamily ? AccessModifier.Protected
              : typeData.IsNestedPublic ? AccessModifier.Public
              : typeData.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
              : typeData.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
              : !typeData.IsVisible ? AccessModifier.Internal
              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        }
    }
}
