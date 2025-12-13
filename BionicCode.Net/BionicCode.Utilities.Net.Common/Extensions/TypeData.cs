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

        private string displayName;
        private string shortDisplayName;
        private string fullyQualifiedDisplayName;
        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? canDeclareExtensionMethod;
        private bool? isAwaitable;
        private bool? isAwaitableTask;
        private bool? isAwaitableValueTask;
        private string signature;
        private string shortSignature;
        private string fullyQualifiedRuntimeSignature;
        private string runtimeSignature;
        private string runtimeShortSignature;
        private string runtimeShortCompactSignature;
        private string shortCompactSignature;
        private string fullyQualifiedSignature;
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
        private TypeData[] genericTypeArguments;
        private TypeData[] genericParameterConstraintsData;
        private GenericParameterAttributes? genericParameterAttributes;
        private TypeData genericTypeDefinitionData;
        private TypeData baseTypeData;
        private TypeData[] interfacesData;
        private PropertyData[] propertiesData;
        private MethodData[] methodsData;
        private FieldData[] fieldsData;
        private EventData[] eventsData;
        private ConstructorData[] constructorsData;
        private IList<CustomAttributeData> attributeData;
        private string assemblyName;
        private MethodData? delegateInvokeMethodData;
        private SymbolComponentInfo symbolComponentInfo;
        private SymbolComponentInfo compactSymbolComponentInfo;
        private bool? containsGenericParameters;
        private readonly ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData> memberTable;
        private bool isAllPropertiesGenerated;
        private bool isAllMethodsGenerated;
        private bool isAllFieldsGenerated;
        private bool isAllEventsGenerated;
        private bool isAllConstructorsGenerated;
        private bool? isByRefLike;
        private bool? isGenericTypeParameter;

        public TypeData(Type type) : base(type.Name)
        {
            this.Handle = type.TypeHandle;
            this.Namespace = type.Namespace;
            this.memberTable = new ConcurrentDictionary<SymbolInfoDataCacheKey, SymbolInfoData>();
        }

        public new Type GetType()
          => Type.GetTypeFromHandle(this.Handle);

        public PropertyData GetProperty(string propertyName, params MethodParameterInfo[] indexerPropertyParameters)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName, nameof(propertyName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousProperty(this.Handle, propertyName, new MethodParameterInfoList(indexerPropertyParameters), SymbolKind.MemberProperty);
            bool isKeyNormalized = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                _ = this.memberTable.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);

                return (PropertyData)symbolInfoData;
            }

            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(cacheKey, out PropertyData propertyData);
            _ = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out normalizedCacheKey);
            _ = this.memberTable.TryAdd(normalizedCacheKey, propertyData);

            return propertyData;
        }

        public IEnumerable<PropertyData> EnumerateProperties()
        {
            if (this.isAllPropertiesGenerated)
            {
                foreach (PropertyData property in this.memberTable.Values.OfType<PropertyData>())
                {
                    yield return property;
                }

                yield break;
            }

            foreach (PropertyInfo property in GetType().GetProperties(HelperExtensionsCommon.AllMembersFullHierarchyFlags))
            {
                PropertyData propertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(property);
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForProperty(property);
                _ = this.memberTable.TryAdd(cacheKey, propertyData);

                yield return propertyData;
            }

            this.isAllPropertiesGenerated = true;
        }

        public MethodData GetMethod(string methodName, int genericTypeParameterCount, params MethodParameterInfo[] parameterList)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));
            ArgumentException.ThrowIfNullOrWhiteSpace(methodName, nameof(methodName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousMethodOrConstructor(this.Handle, methodName, new MethodParameterInfoList(parameterList), genericTypeParameterCount, SymbolKind.MemberMethod);
            bool isKeyNormalized = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                _ = this.memberTable.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);

                return (MethodData)symbolInfoData;
            }

            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(cacheKey, out MethodData methodData);
            _ = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out normalizedCacheKey);
            _ = this.memberTable.get(normalizedCacheKey, methodData);

            return methodData;
        }

        public IEnumerable<MethodData> EnumerateMethods()
        {
            if (this.isAllMethodsGenerated)
            {
                foreach (MethodData method in this.memberTable.Values.OfType<MethodData>())
                {
                    yield return method;
                }

                yield break;
            }

            foreach (MethodInfo method in GetType().GetMethods(HelperExtensionsCommon.AllMembersFullHierarchyFlags))
            {
                MethodData methodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(method);
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForMethod(method);
                _ = this.memberTable.TryAdd(cacheKey, methodData);

                yield return methodData;
            }

            this.isAllMethodsGenerated = true;
        }

        public FieldData GetField(string fieldName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName, nameof(fieldName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousFieldOrEvent(this.Handle, fieldName, SymbolKind.MemberField);
            bool isKeyNormalized = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                _ = this.memberTable.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);

                return (FieldData)symbolInfoData;
            }

            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(cacheKey, out FieldData fieldData);
            _ = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out normalizedCacheKey);
            _ = this.memberTable.TryAdd(normalizedCacheKey, fieldData);

            return fieldData;
        }

        public IEnumerable<FieldData> EnumerateFields()
        {
            if (this.isAllFieldsGenerated)
            {
                foreach (FieldData field in this.memberTable.Values.OfType<FieldData>())
                {
                    yield return field;
                }

                yield break;
            }

            foreach (FieldInfo field in GetType().GetFields(HelperExtensionsCommon.AllMembersFullHierarchyFlags))
            {
                FieldData fieldData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(field);
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForField(field);
                _ = this.memberTable.TryAdd(cacheKey, fieldData);

                yield return fieldData;
            }

            this.isAllFieldsGenerated = true;
        }

        public EventData GetEvent(string eventName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventName, nameof(eventName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousFieldOrEvent(this.Handle, eventName, SymbolKind.MemberEvent);
            bool isKeyNormalized = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                _ = this.memberTable.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);

                return (EventData)symbolInfoData;
            }

            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(cacheKey, out EventData eventData);
            _ = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out normalizedCacheKey);
            _ = this.memberTable.TryAdd(normalizedCacheKey, eventData);

            return eventData;
        }

        public IEnumerable<EventData> EnumerateEvents()
        {
            if (this.isAllEventsGenerated)
            {
                foreach (EventData eventData in this.memberTable.Values.OfType<EventData>())
                {
                    yield return eventData;
                }

                yield break;
            }

            foreach (EventInfo eventInfo in GetType().GetEvents(HelperExtensionsCommon.AllMembersFullHierarchyFlags))
            {
                EventData eventData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(eventInfo);
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForEvent(eventInfo);
                _ = this.memberTable.TryAdd(cacheKey, eventData);

                yield return eventData;
            }

            this.isAllEventsGenerated = true;
        }

        public ConstructorData GetConstructor(string constructorName, int genericTypeParameterCount, params MethodParameterInfo[] parameterList)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(genericTypeParameterCount, nameof(genericTypeParameterCount));
            ArgumentException.ThrowIfNullOrWhiteSpace(constructorName, nameof(constructorName));

            SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForAnonymousMethodOrConstructor(this.Handle, constructorName, new MethodParameterInfoList(parameterList), genericTypeParameterCount, SymbolKind.Constructor);
            bool isKeyNormalized = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out SymbolInfoDataCacheKey normalizedCacheKey);
            if (isKeyNormalized)
            {
                _ = this.memberTable.TryGetValue(normalizedCacheKey, out SymbolInfoData symbolInfoData);

                return (ConstructorData)symbolInfoData;
            }

            SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(cacheKey, out ConstructorData constructorData);
            _ = SymbolReflectionInfoCache.TryGetNormalizedKey(cacheKey, out normalizedCacheKey);
            _ = this.memberTable.TryAdd(normalizedCacheKey, constructorData);

            return constructorData;
        }

        public IEnumerable<ConstructorData> EnumerateConstructors()
        {
            if (this.isAllConstructorsGenerated)
            {
                foreach (ConstructorData constructor in this.memberTable.Values.OfType<ConstructorData>())
                {
                    yield return constructor;
                }

                yield break;
            }

            foreach (ConstructorInfo constructor in GetType().GetConstructors(HelperExtensionsCommon.AllMembersFullHierarchyFlags))
            {
                ConstructorData constructorData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructor);
                SymbolInfoDataCacheKey cacheKey = SymbolInfoDataCacheKey.CreateForConstructor(constructor);
                _ = this.memberTable.TryAdd(cacheKey, constructorData);

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

        public bool IsValueType
          => this.isValueType ??= GetType().IsValueType;

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
                    Type genericTypeDefinitionType = GetType().GetGenericTypeDefinition();
                    this.genericTypeDefinitionData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericTypeDefinitionType);
                }

                return this.genericTypeDefinitionData;
            }
        }

        public TypeData[] GenericTypeArguments
        {
            get
            {
                if (this.genericTypeArguments is null)
                {
                    Type[] typeArguments = GetType().GetGenericArguments();
                    this.genericTypeArguments = typeArguments.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();
                }

                return this.genericTypeArguments;
            }
        }

        public bool CanDeclareExtensionMethod
          => (bool)(bool?)(this.canDeclareExtensionMethod ??= TypeData.CanDeclareExtensionMethods(this));

        public override IList<CustomAttributeData> AttributeData
          => this.attributeData ??= GetType().GetCustomAttributesData();

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
          => this.assemblyName ??= GetType().Assembly.GetName().Name;

        public bool IsStatic
          => this.isStatic ??= TypeData.IsTypeStatic(this);

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = TypeData.GetAttributes(this))
          : this.symbolAttributes;

        public bool IsAbstract
          => this.isAbstract ??= GetType().IsAbstract;

        public bool IsSealed
          => this.isSealed ??= GetType().IsSealed;

        public bool IsByRef
          => this.isByRef ??= GetType().IsByRef;

        public bool IsByRefLike
          => this.isByRefLike ??= GetType().IsByRefLike;

        public bool IsDelegate
          => this.isDelegate ??= TypeData.IsTypeDelegate(GetType());

        public bool IsSubclass
        {
            get
            {
                if (this.isSubclass is null)
                {
                    Type baseType = GetType().BaseType;
                    this.isSubclass = baseType != null
                      && baseType != typeof(object)
                      && baseType != typeof(ValueType);
                }

                return (bool)this.isSubclass;
            }
        }

        public TypeData BaseTypeData
        {
            get
            {
                Type baseType = GetType().BaseType;
                if (this.baseTypeData is null && this.IsSubclass)
                {
                    this.baseTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(baseType);
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
                    throw new InvalidOperationException($"The current type is not a delegate. Call {nameof(this.IsDelegate)} to check whether the current type is a delegate.");
                }

                if (this.delegateInvokeMethodData is null)
                {
                    MethodInfo methodInfo = GetType().GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName);
                    this.delegateInvokeMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
                }

                return this.delegateInvokeMethodData;
            }
        }

        public bool IsGenericTypeParameter
          => this.isGenericTypeParameter ??= GetType().IsGenericParameter;

        public bool IsGenericType
          => this.isGenericType ??= GetType().IsGenericType;

        public bool IsBuiltInType
          => this.isBuiltInType ??= TypeData.IsTypeBuiltInType(this);

        public bool IsGenericTypeDefinition
          => this.isGenericTypeDefinition ??= GetType().IsGenericTypeDefinition;

        public bool ContainsGenericParameters
          => this.containsGenericParameters ??= GetType().ContainsGenericParameters;

        public GenericParameterAttributes GenericParameterAttributes
          => (GenericParameterAttributes)(GenericParameterAttributes?)(this.genericParameterAttributes ??= GetType().GenericParameterAttributes);

        public TypeData[] GenericParameterConstraintsData
          => this.genericParameterConstraintsData ??= GetType().GetGenericParameterConstraints().Where(constraint => constraint != typeof(object) && constraint != typeof(ValueType)).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public TypeData[] InterfacesData
          => this.interfacesData ??= GetType().GetInterfaces().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public PropertyData[] PropertiesData
          => this.propertiesData ??= GetType().GetProperties(HelperExtensionsCommon.AllMembersFullHierarchyFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public MethodData[] MethodsData
          => this.methodsData ??= GetType().GetMethods(HelperExtensionsCommon.AllMembersFullHierarchyFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public FieldData[] FieldsData
          => this.fieldsData ??= GetType().GetFields(HelperExtensionsCommon.AllMembersFullHierarchyFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public EventData[] EventsData
          => this.eventsData ??= GetType().GetEvents(HelperExtensionsCommon.AllMembersFullHierarchyFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        public ConstructorData[] ConstructorsData
          => this.constructorsData ??= GetType().GetConstructors(HelperExtensionsCommon.AllMembersFullHierarchyFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();

        private static bool IsTypeStatic(TypeData typeData)
          => typeData.IsAbstract && typeData.IsSealed;

        private static bool IsTypeBuiltInType(TypeData typeData)
        {
            var typeReference = new CodeTypeReference(typeData.GetType());
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
            Type typeInfo = typeData.GetType();
            if (!typeData.IsStatic || typeInfo.IsNested || typeInfo.IsGenericType)
            {
                return false;
            }

            Attribute typeExtensionAttribute = typeInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
            return typeExtensionAttribute != null;
        }

        /// <summary>
        /// Determines the symbol attributes for the specified type represented by the given TypeData instance.
        /// </summary>
        /// <remarks>The returned SymbolAttributes value may include multiple flags combined using a
        /// bitwise OR to represent all applicable characteristics of the type. This method does not perform validation
        /// on the input; callers should ensure that typeData is valid and represents a supported type.</remarks>
        /// <param name="typeData">The TypeData instance representing the type for which to retrieve symbol attributes. Cannot be null.</param>
        /// <returns>A SymbolAttributes value that describes the kind and characteristics of the specified type, such as whether
        /// it is a class, struct, interface, enum, delegate, generic, static, abstract, or final. Returns
        /// SymbolAttributes.Undefined if the type does not match any recognized category.</returns>
        private static SymbolAttributes GetAttributes(TypeData typeData)
        {
            Type type = typeData.GetType();
            if (typeData.IsDelegate)
            {
                SymbolAttributes delegateAttributes = SymbolAttributes.Delegate;
                if (typeData.IsGenericType)
                {
                    delegateAttributes |= SymbolAttributes.Generic;
                }

                return delegateAttributes;
            }

            if (type.IsClass)
            {
                SymbolAttributes classAttributes = SymbolAttributes.Class;
                if (type.IsAbstract)
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

            if (type.IsInterface)
            {
                SymbolAttributes interfaceAttributes = SymbolAttributes.Interface;
                return interfaceAttributes;
            }

            if (type.IsEnum)
            {
                SymbolAttributes enumAttributes = SymbolAttributes.Enum;
                return enumAttributes;
            }

            if (type.IsValueType)
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

                bool isReadOnlyStruct = TypeData.IsReadOnlyStructInternal(type);
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
            ArgumentNullExceptionEx.ThrowIfNull(type, nameof(type));

            return TypeData.DelegateType.IsAssignableFrom(type);
        }

        private static bool IsReadOnlyStructInternal(Type type)
          => type.IsValueType && type.GetCustomAttribute(HelperExtensionsCommon.IsReadOnlyAttributeType) != null;

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

            Type type = typeData.GetType();
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

                    MethodInfo extensionMethodInfo = exportedType.GetMethod(nameof(Task.GetAwaiter), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { type }, null);
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
          => TypeData.TaskType.IsAssignableFrom(type.GetType())
            || TypeData.TaskType.IsAssignableFrom(type.BaseTypeData.GetType());

        private static bool IsTypeAwaitableValueTask(TypeData type)
          => TypeData.ValueTaskType == type.GetType()
            || (type.IsGenericType && TypeData.ValueTaskGenericType == type.GenericTypeDefinitionData.GetType());

        private static AccessModifier GetAccessModifier(TypeData typeData)
        {
            Type typeInfo = typeData.GetType();
            return typeInfo.IsPublic ? AccessModifier.Public
              : typeInfo.IsNestedPrivate ? AccessModifier.Private
              : typeInfo.IsNestedAssembly ? AccessModifier.Internal
              : typeInfo.IsNestedFamily ? AccessModifier.Protected
              : typeInfo.IsNestedPublic ? AccessModifier.Public
              : typeInfo.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
              : typeInfo.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
              : !typeInfo.IsVisible ? AccessModifier.Internal
              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        }
    }
}
