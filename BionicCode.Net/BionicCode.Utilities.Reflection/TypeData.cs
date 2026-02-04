[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net.Reflection
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    internal class TypeData : SymbolInfoData
    {
        private delegate bool MethodEqualityComparer<TParameterList>(ParameterList foundMethodParameters, TParameterList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
            where TParameterList : notnull, IEnumerable;
        private static readonly Type TaskType = typeof(Task);
        private static readonly Type ValueTaskType = typeof(ValueTask);
        private static readonly Type ValueTaskGenericType = typeof(ValueTask<>);
        private static readonly Type DelegateType = typeof(MulticastDelegate);
        private const BindingFlags BindingFlagsPublicMask = BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags BindingFlagsStaticMask = BindingFlags.Static | BindingFlags.Instance;

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
        private TypeData? _genericTypeDefinition;
        private TypeData? _baseTypes;
        private TypeList? _interfaces;
        private PropertyList? _properties;
        private MethodList? _methods;
        private FieldList? _fields;
        private EventList? _events;
        private ConstructorList? _constructors;
        private IList<CustomAttributeData>? attributeData;
        private string? assemblyName;
        private MethodData? delegateInvokeMethodData;
        private SymbolComponentInfo? symbolComponentInfo;
        private SymbolComponentInfo? compactSymbolComponentInfo;
        private bool? containsGenericParameters;
        private readonly ConcurrentHashSet<SymbolReflectionInfoCacheKey> _memberTable;
        private readonly ConcurrentDictionary<SymbolKind, bool> _memberTableStateFlagTable;
        private bool? isByRefLike;
        private bool? isGenericTypeParameter;
        private bool? isGenericMethodParameter;
        private bool? isGenericParameter;
        private bool? _isEnum;
        private bool? _isClass;
        private bool? _isInterface;
        private bool? _isStruct;
        private bool? _isReadOnlyStruct;

        private bool? _isPublic;
        private bool? _isNestedPrivate;
        private bool? _isNestedAssembly;
        private bool? _isNestedFamily;
        private bool? _isNestedPublic;
        private bool? _isNestedFamORAssem;
        private bool? _isNestedFamANDAssem;
        private bool? _isVisible;

        internal TypeData(Type type, SymbolReflectionInfoCacheKey symbolInfoDataCacheKey) : base(type.Name, SymbolKind.Type, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            this.Handle = type.TypeHandle;
            this.Namespace = type.Namespace ?? string.Empty;
            this._memberTable = new ConcurrentHashSet<SymbolReflectionInfoCacheKey>();
            this._memberTableStateFlagTable = new ConcurrentDictionary<SymbolKind, bool>();
        }

        /// <summary>
        /// Returns the underlying <see cref="Type"/> represented by this handle.
        /// </summary>
        /// <returns>A <see cref="Type"/> object that is referenced by this handle.</returns>
        public Type UnwrapType()
          => Type.GetTypeFromHandle(this.Handle)!;

        /// <summary>
        /// Attempts to retrieve the property data associated with the specified property name.
        /// </summary>
        /// <remarks>This method performs a case-sensitive O(1) search for the property name. The search is a O(n) operation if the cache has not been built yet (in this case, successive calls are guaranteed to be O(1) operation).
        /// <param name="propertyName">The name of the property to locate. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="propertyData">When this method returns, contains the property data associated with the specified name, if found;
        /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if a property with the specified name was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetPropertyByName(string propertyName, out PropertyData? propertyData)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
            return this.Properties.TryGetPropertyByName(propertyName, out propertyData);
        }

        public bool TryGetIndexerPropertyByParameterList(ParameterList parameters, PropertyAccessors propertyAccessor, out PropertyData? propertyData)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(parameters);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(propertyAccessor);

            propertyData = null;
            foreach (PropertyData property in this.Properties)
            {
                ParameterList indexerAccessorParameters = propertyAccessor switch
                {
                    // Since we use binary AND to bit mask flags, we we also catch the combined flag GetAndSet here.
                    var accessorSpecifier when (accessorSpecifier & PropertyAccessors.Get) != 0 && property.CanRead => property.PropertyGetMethodParameters,
                    var accessorSpecifier when (accessorSpecifier & PropertyAccessors.Set) != 0 && property.CanWrite => property.PropertySetMethodParameters,
                    _ => throw new NotSupportedException($"The value '{propertyAccessor}' is not supported."),
                };

                if (indexerAccessorParameters.Equals(parameters))
                {
                    propertyData = property;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetIndexerPropertyByParameterList(MethodParameterInfoList indexerParameters, PropertyAccessors indexerPropertyAccessor, out PropertyData? propertyData)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(indexerParameters);
            ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(indexerPropertyAccessor);

            Func<PropertyData, ParameterList> indexerAccessorParametersReader = indexerPropertyAccessor switch
            {
                // Since we use binary AND to bit mask flags, we we also catch the combined flag GetAndSet here.
                var accessorSpecifier when (accessorSpecifier & PropertyAccessors.Get) != 0 => property => property.IsIndexer && property.CanRead ? property.PropertyGetMethodParameters : ParameterList.Empty,
                var accessorSpecifier when (accessorSpecifier & PropertyAccessors.Set) != 0 => property => property.IsIndexer && property.CanWrite ? property.PropertySetMethodParameters : ParameterList.Empty,
                _ => throw new NotSupportedException($"The value '{indexerPropertyAccessor}' is not supported."),
            };
            propertyData = null;
            foreach (PropertyData property in this.Properties)
            {
                ParameterList indexerAccessorParameters = indexerAccessorParametersReader.Invoke(property);
                if (ParameterListEqualityComparer.Equals(indexerAccessorParameters, indexerParameters))
                {
                    propertyData = property;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns an enumerable collection of property metadata for the current type, using the specified binding
        /// flags to control which properties are included.
        /// </summary>
        /// <remarks>Properties are returned from a cache when available; otherwise, the first call builds the full cache for the properties.<para/>
        /// Therefore the first call is expensive in order to make subsequent calls an efficient O(1) lookup due to caching.<br/>
        /// While the caller can control the enumerated set by specifying <paramref name="bindingFlags"/>, the by the first call triggered cache build up routine will cache all properties that are reachable from the current <see cref="Type"/> that this <see cref="TypeData"/> represents.</remarks>
        /// <param name="bindingFlags">A bitwise combination of <see cref="BindingFlags"/> values that determines which properties to include in the enumeration.
        /// The default value includes all public and non-public instance and static properties from the entire
        /// inheritance hierarchy.</param>
        /// <returns>An enumerable collection of <see cref="PropertyData"/> objects representing the properties of the current type that match
        /// the specified binding flags.</returns>
        public IEnumerable<PropertyData> EnumerateProperties(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            // Return already cached properties if available
            foreach (PropertyData propertyData in EnumerateMemberKindCache<PropertyData>(bindingFlags))
            {
                yield return propertyData;
            }
        }

        /// <summary>
        /// Attempts to retrieve all methods with the specified name.
        /// </summary>
        /// <remarks>This method performs a case-sensitive O(1) search for the method name. The search is a O(n) operation if the cache has not been built yet (in this case, successive calls are guaranteed to be O(1) operation).
        /// <param name="methodName">The name of the method to search for. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="methods">When this method returns, contains a collection of methods with the specified name, if found; otherwise,
        /// null.</param>
        /// <returns>true if one or more methods with the specified name are found; otherwise, false.</returns>
        public bool TryGetMethodByName(string methodName, out MethodList? methods)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(methodName);
            return this.Methods.TryGetMethodsByName(methodName, out methods);
        }

        public bool TryGetMethod(string methodName, TypeList genericMethodParameters, ParameterList parameters, out MethodData? methodData)
        {
            parameters = parameters.OrEmpty();
            genericMethodParameters = genericMethodParameters.OrEmpty();

            static bool equalityComparer(ParameterList foundMethodParameters, ParameterList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
                => foundMethodName.Equals(requestedMethodName, StringComparison.Ordinal)
                    && foundMethodParameters.Equals(requestedMethodParameters)
                    && foundGenericMethodParameters.Equals(requestedGenericMethodParameters);

            return TryGetMethodInternal(methodName, genericMethodParameters, parameters, equalityComparer, out methodData);
        }

        public bool TryGetMethod(string methodName, TypeList genericMethodParameters, MethodParameterInfoList parameters, out MethodData? methodData)
        {
            parameters = parameters.OrEmpty();
            genericMethodParameters = genericMethodParameters.OrEmpty();

            static bool equalityComparer(ParameterList foundMethodParameters, MethodParameterInfoList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
                => foundMethodName.Equals(requestedMethodName, StringComparison.Ordinal)
                    && ParameterListEqualityComparer.Equals(foundMethodParameters, requestedMethodParameters)
                    && foundGenericMethodParameters.Equals(requestedGenericMethodParameters);

            return TryGetMethodInternal(methodName, genericMethodParameters, parameters, equalityComparer, out methodData);
        }

        private bool TryGetMethodInternal<TParameterList>(string requestedMethodName, TypeList requestedGenericMethodParameters, TParameterList requestedMethodParameters, MethodEqualityComparer<TParameterList> equalityComparer, out MethodData? requestedMethodData)
            where TParameterList : notnull, IEnumerable
        {
            requestedMethodData = null;
            foreach (MethodData method in this.Methods)
            {
                ParameterList methodParameters = method.Parameters;
                TypeList methodGenericMethodParameters = method.GenericMethodParameters;

                if (equalityComparer(methodParameters, requestedMethodParameters, methodGenericMethodParameters, requestedGenericMethodParameters, method.Name, requestedMethodName))
                {
                    requestedMethodData = method;
                    return true;
                }
            }

            return false;
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
            // Return already cached methods if available
            foreach (MethodData methodData in EnumerateMemberKindCache<MethodData>(bindingFlags))
            {
                yield return methodData;
            }
        }

        /// <summary>
        /// Attempts to retrieve the field data associated with the specified field name.
        /// </summary>
        /// <remarks>This method performs a case-sensitive O(1) search for the field name. The search is a O(n) operation if the cache has not been built yet (in this case, successive calls are guaranteed to be O(1) operation).
        /// <param name="fieldName">The name of the field to locate. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="fieldData">When this method returns, contains the field data associated with the specified field name, if found;
        /// otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the field was found and <paramref name="fieldData"/> contains the associated data;
        /// otherwise, <see langword="false"/>.</returns>
        public bool TryGetFieldByName(string fieldName, out FieldData? fieldData)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fieldName);
            return this.Fields.TryGetFieldByName(fieldName, out fieldData);
        }

        public IEnumerable<FieldData> EnumerateFields(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            // Return already cached fields if available
            foreach (FieldData fieldData in EnumerateMemberKindCache<FieldData>(bindingFlags))
            {
                yield return fieldData;
            }
        }

        /// <summary>
        /// Attempts to retrieve event data for the specified event name.
        /// </summary>
        /// <remarks>This method performs a case-sensitive O(1) search for the event name. The search is a O(n) operation if the cache has not been built yet (in this case, successive calls are guaranteed to be O(1) operation).
        /// <param name="eventName">The name of the event to locate. Cannot be null, empty, or consist only of white-space characters.</param>
        /// <param name="eventData">When this method returns, contains the event data associated with the specified event name, if found;
        /// otherwise, null. This parameter is passed uninitialized.</param>
        /// <returns>true if the event data was found and returned in eventData; otherwise, false.</returns>
        public bool TryGetEventByName(string eventName, out EventData? eventData)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            return this.Events.TryGetEventByName(eventName, out eventData);
        }
        public bool TryGetExplicitEvent(RuntimeTypeHandle declaringInterfaceTypeHandle, string eventName, out EventData? eventData)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringInterfaceTypeHandle);
            ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
            return this.ExplicitEventImplementations.TryGetEventByName(eventName, out eventData);
        }

        public IEnumerable<EventData> EnumerateEvents(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            foreach (EventData eventData in EnumerateMemberKindCache<EventData>(bindingFlags))
            {
                yield return eventData;
            }
        }

        public bool TryGetConstructorByParameterList(ParameterList parameters, out ConstructorData? constructorData)
        {
            parameters = parameters.OrEmpty();

            constructorData = null;
            foreach (ConstructorData constructor in this.Constructors)
            {
                ParameterList constructorParameters = constructorData!.Parameters;

                if (parameters.Equals(constructorParameters))
                {
                    constructorData = constructor;
                    return true;
                }
            }

            return false;
        }

        public bool TryGetConstructorByParameterList(MethodParameterInfoList parameters, out ConstructorData? constructorData)
        {
            parameters = parameters.OrEmpty();

            constructorData = null;
            foreach (ConstructorData constructor in this.Constructors)
            {
                ParameterList constructorParameters = constructorData!.Parameters;

                if (ParameterListEqualityComparer.Equals(parameters, constructorParameters))
                {
                    constructorData = constructor;
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<ConstructorData> EnumerateConstructors(BindingFlags bindingFlags = HelperExtensionsCommon.AllMembersFullHierarchyFlags)
        {
            // Return already cached constructors if available
            foreach (ConstructorData constructorData in EnumerateMemberKindCache<ConstructorData>(bindingFlags))
            {
                yield return constructorData;
            }
        }

        private IEnumerable<TMemberData> EnumerateMemberKindCache<TMemberData>(BindingFlags bindingFlags) where TMemberData : MemberData
        {
            SymbolKind memberKind = typeof(TMemberData) switch
            {
                Type memberType when memberType == typeof(PropertyData) => SymbolKind.MemberProperty,
                Type memberType when memberType == typeof(MethodData) => SymbolKind.MemberMethod,
                Type memberType when memberType == typeof(FieldData) => SymbolKind.MemberField,
                Type memberType when memberType == typeof(EventData) => SymbolKind.MemberEvent,
                Type memberType when memberType == typeof(ConstructorData) => SymbolKind.MemberConstructor,
                _ => throw new NotSupportedException($"The member type '{typeof(TMemberData).FullName}' is not supported."),
            };

            IEnumerable<MemberData> cachedMembers = memberKind switch
            {
                // Accessing member list properties ensure cache is built up

                SymbolKind.MemberMethod => this.Methods,
                SymbolKind.MemberProperty => this.Properties,
                SymbolKind.MemberField => this.Fields,
                SymbolKind.MemberEvent => this.Events,
                SymbolKind.MemberConstructor => this.Constructors,
                _ => throw new NotSupportedException($"The member kind '{typeof(SymbolKind).FullName}.{memberKind}' is not supported."),
            };

            foreach (MemberData memberData in cachedMembers)
            {
                if (IsValidMember(memberData!, bindingFlags))
                {
                    yield return (TMemberData)memberData;
                }
            }
        }

        private IEnumerable<TMemberData> BuildAndEnumerateMemberKindCache<TMemberData>(MemberInfo[] members, BindingFlags bindingFlags) where TMemberData : MemberData
        {
            Func<MemberInfo, MemberData> readReflectionCache;
            Action<MemberData> addMemberToTypeDataMemberList;
            SymbolKind memberKind;
            switch (typeof(TMemberData))
            {
                case Type memberType when memberType == typeof(PropertyData):
                    readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((PropertyInfo)memberInfo);
                    IPropertyListBuilder propertyListBuilder = PropertyListBuilder.New(this.Handle);
                    addMemberToTypeDataMemberList = propertyData => _ = propertyListBuilder.Add((PropertyData)propertyData);
                    memberKind = SymbolKind.MemberProperty;
                    break;
                case Type memberType when memberType == typeof(MethodData):
                    readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((MethodInfo)memberInfo);
                    IMethodListBuilder methodListBuilder = MethodListBuilder.New(this.Handle);
                    addMemberToTypeDataMemberList = methodData => methodListBuilder.Add((MethodData)methodData);
                    memberKind = SymbolKind.MemberMethod;
                    break;
                case Type memberType when memberType == typeof(FieldData):
                    readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((FieldInfo)memberInfo);
                    IFieldListBuilder fieldListBuilder = FieldListBuilder.New(this.Handle);
                    addMemberToTypeDataMemberList = fieldData => fieldListBuilder.Add((FieldData)fieldData);
                    memberKind = SymbolKind.MemberField;
                    break;
                case Type memberType when memberType == typeof(EventData):
                    readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((EventInfo)memberInfo);
                    IEventListBuilder eventListBuilder = EventListBuilder.New(this.Handle);
                    addMemberToTypeDataMemberList = eventData => eventListBuilder.Add((EventData)eventData);
                    memberKind = SymbolKind.MemberEvent;
                    break;
                case Type memberType when memberType == typeof(ConstructorData):
                    readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((ConstructorInfo)memberInfo);
                    IConstructorListBuilder constructorListBuilder = ConstructorListBuilder.New(this.Handle);
                    addMemberToTypeDataMemberList = constructorData => constructorListBuilder.Add((ConstructorData)constructorData);
                    memberKind = SymbolKind.MemberConstructor;
                    break;
                default:
                    throw new NotSupportedException($"The member type '{typeof(TMemberData).FullName}' is not supported.");
            }

            int memberIndex = 0;
            try
            {
                for (; memberIndex < members.Length; memberIndex++)
                {
                    MemberInfo memberInfo = members[memberIndex];
                    if (memberInfo is null)
                    {
                        continue;
                    }

                    TMemberData memberDataFromReflectionCache = (TMemberData)readReflectionCache.Invoke(memberInfo);
                    addMemberToTypeDataMemberList.Invoke(memberDataFromReflectionCache);
                    SymbolReflectionInfoCacheKey cacheKey = memberDataFromReflectionCache.CacheKey;
                    if (this._memberTable.TryAdd(cacheKey))
                    {
                        bool isValidMember = IsValidMember(memberDataFromReflectionCache, bindingFlags);
                        if (isValidMember)
                        {
                            yield return memberDataFromReflectionCache;
                        }
                    }
                }
            }
            finally
            {
                // Caller may has broke out of enumeration prematurely. So we need to finish cache building.
                for (; memberIndex < members.Length; memberIndex++)
                {
                    MemberInfo memberInfo = members[memberIndex];
                    TMemberData memberDataFromReflectionCache = (TMemberData)readReflectionCache.Invoke(memberInfo);
                    addMemberToTypeDataMemberList.Invoke(memberDataFromReflectionCache);
                    SymbolReflectionInfoCacheKey cacheKey = memberDataFromReflectionCache.CacheKey;
                    _ = this._memberTable.TryAdd(cacheKey);
                }

                _ = this._memberTableStateFlagTable.TryAdd(memberKind, true);
            }
        }

        private bool IsCacheBuildForMemberKind(SymbolKind memberKind)
        {
            return this._memberTableStateFlagTable.TryGetValue(memberKind, out bool isCacheReady)
                && isCacheReady;
        }

        private bool IsValidMember(MemberData cachedMemberData, BindingFlags bindingFlags)
        {
            // FlattenHierarchy: inherited private static members are not returned when FlattenHierarchy is specified.
            if (bindingFlags.HasFlag(BindingFlags.FlattenHierarchy)
                && !cachedMemberData.DeclaringTypeHandle.Equals(this.Handle)
                && cachedMemberData.IsPrivate
                && cachedMemberData.IsStatic)
            {
                return false;
            }

            // DeclaredOnly: only members declared on this type
            if (bindingFlags.HasFlag(BindingFlags.DeclaredOnly)
                && !cachedMemberData.DeclaringTypeHandle.Equals(this.Handle))
            {
                return false;
            }

            // must specify at least one of Instance/Static and one of Public/NonPublic
            if ((bindingFlags & BindingFlagsStaticMask) == 0
                || (bindingFlags & BindingFlagsPublicMask) == 0)
            {
                return false;
            }

            // Check static/instance: caller's requested (Instance|Static) must intersect member's instance/static bit
            if ((bindingFlags & BindingFlagsStaticMask & cachedMemberData.BindingFlagsVisibilityMask & BindingFlagsStaticMask) == 0)
            {
                return false;
            }

            // Check public/non-public: caller's requested (Public|NonPublic) must intersect member's public/non-public bit
            if ((bindingFlags & BindingFlagsPublicMask & cachedMemberData.BindingFlagsVisibilityMask & BindingFlagsPublicMask) == 0)
            {
                return false;
            }

            return true;
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
                    this._genericTypeDefinition = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericTypeDefinitionType);
                }

                return this._genericTypeDefinition;
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

        public bool IsPublic
            => this._isPublic ??= UnwrapType().IsPublic;

        public bool IsNestedPrivate
            => this._isNestedPrivate ??= UnwrapType().IsNestedPrivate;

        public bool IsNestedAssembly
            => this._isNestedAssembly ??= UnwrapType().IsNestedAssembly;

        public bool IsNestedFamily
            => this._isNestedFamily ??= UnwrapType().IsNestedFamily;

        public bool IsNestedPublic
            => this._isNestedPublic ??= UnwrapType().IsNestedPublic;

        public bool IsNestedFamORAssem
            => this._isNestedFamORAssem ??= UnwrapType().IsNestedFamORAssem;

        public bool IsNestedFamANDAssem
            => this._isNestedFamANDAssem ??= UnwrapType().IsNestedFamANDAssem;

        public bool IsVisible
            => this._isVisible ??= UnwrapType().IsVisible;

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
                if (this._baseTypes is null && this.IsSubclass)
                {
                    Type? baseType = UnwrapType().BaseType;
                    this._baseTypes = baseType is null
                        ? null
                        : SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(baseType);
                }

                return this._baseTypes;
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

                this.delegateInvokeMethodData ??= this.Methods.TryGetMethodsByName(ReflectionConstants.DelegateInvocatorMethodName, out MethodList methods)
                    ? methods[0]
                    : throw new InvalidOperationException($"The delegate type '{this.Name}' does not have a valid invoke method.");

                return this.delegateInvokeMethodData!;
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
          => this._interfaces ??= TypeListBuilder.CreateImplementedInterfacesList(this);

        /// <summary>
        /// Gets all reachable properties of the current type.
        /// </summary>
        /// <remarks>The returned <see cref="PropertyList"/> contains all properties that are reachable from the current type:
        /// <list type="bullet">
        /// <item>public protected and private instance and static properties of the current type</item>
        /// <item>implemented public interface properties (either implemented directly or through a base class)</item>
        /// <item>inherited public and protected instance and static properties</item>
        /// </list>
        /// To obtain private properties of a superclass read the <see cref="Properties"/> property of that particular superclass.<br/> />
        /// Note: explicit interface implementation properties are not included in the returned list as they are not directly reachable via the type instance.<br/>
        /// <para/>Accessing this property triggers the build up of the property cache for the current type.</remarks>
        public PropertyList Properties
        {
            get
            {
                if (this._properties is null)
                {
                    Type thisType = UnwrapType();
                    PropertyInfo[] visibleProperties = thisType.GetProperties(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    this._properties = BuildAndEnumerateMemberKindCache<PropertyData>(visibleProperties, HelperExtensionsCommon.AllMembersFullHierarchyFlags)
                        .ToPropertyList();
                }

                return this._properties!;
            }
        }

        public PropertyList ExplicitInterfaceProperties
        {
            get
            {
                if (this._explicitInterfaceProperties is null)
                {
                    Type thisType = UnwrapType();
                    Type[] implementedInterfaces = thisType.GetInterfaces();
                    IPropertyListBuilder propertyListBuilder = PropertyListBuilder.New(this.Handle);
                    foreach (Type implementedInterface in implementedInterfaces)
                    {
                        InterfaceMapping interfaceMap = thisType.GetInterfaceMap(implementedInterface);

                        foreach (PropertyInfo interfacePropertyInfo in implementedInterface.GetProperties())
                        {
                            (MethodData? getMethodData, MethodData? setMethodData) propertyAccessors;
                            if (interfacePropertyInfo.CanRead)
                            {
                                MethodInfo? interfaceGetAccessorMethod = interfacePropertyInfo.GetMethod;
                                int mapIndex = Array.IndexOf(interfaceMap.InterfaceMethods, interfaceGetAccessorMethod);
                                var implementedPropertyAccessorMethodData = interfaceMap.TargetMethods[mapIndex].ToMethodData();
                                if (implementedPropertyAccessorMethodData.IsPublic)
                                {
                                    // Not an explicit interface implementation (it's a normal public interface implementation)
                                    continue;
                                }

                                propertyAccessors.getMethodData = implementedPropertyAccessorMethodData;
                            }

                            if (interfacePropertyInfo.CanWrite)
                            {
                                MethodInfo? interfaceSetAccessorMethod = interfacePropertyInfo.SetMethod;
                                int mapIndex = Array.IndexOf(interfaceMap.InterfaceMethods, interfaceSetAccessorMethod);
                                var implementedPropertyAccessorMethodData = interfaceMap.TargetMethods[mapIndex].ToMethodData();
                                if (implementedPropertyAccessorMethodData.IsPublic)
                                {
                                    // Not an explicit interface implementation (it's a normal public interface implementation)
                                    continue;
                                }

                                propertyAccessors.setMethodData = implementedPropertyAccessorMethodData;
                            }

                            var explicitPropertyImplementationDescriptor = new WellKnownPropertyDescriptor(
                                )
                        }

                        MethodInfo[] implementedPropertyAccessors = interfaceMap.TargetMethods;
                        foreach (MethodInfo implementedPropertyAccessor in implementedPropertyAccessors)
                        {
                            if (implementedPropertyAccessor.IsPublic)
                            {
                                // Not an explicit interface implementation (it's a normal public interface implementation)
                                continue;
                            }

                            RuntimeMethodHandle accessorMethodHandle = implementedPropertyAccessor.MethodHandle;
                            if (propertyAccessorMap.TryGetValue(accessorMethodHandle, out PropertyData? propertyData))
                            {
                                // Property is an explicit interface implementation
                                propertyListBuilder.Add(propertyData);
                            }
                        }
                        RuntimeMethodHandle
                    }

                    this._properties = BuildAndEnumerateMemberKindCache<PropertyData>(visibleProperties, HelperExtensionsCommon.AllMembersFullHierarchyFlags)
                        .ToPropertyList();
                }

                return this._properties!;
            }
        }


        /// <summary>
        /// Gets all reachable methods of the current type.
        /// </summary>
        /// <remarks>The returned <see cref="MethodList"/> contains all methods that are reachable from the current type:
        /// <list type="bullet">
        /// <item>public protected and private instance and static methods of the current type</item>
        /// <item>implemented public interface methods (either implemented directly or through a base class)</item>
        /// <item>inherited public and protected instance and static methods</item>
        /// </list>
        /// To obtain private methods of a superclass read the <see cref="Methods"/> property of that particular superclass.<br/>/>
        /// Note: explicit interface implementation methods are not included in the returned list as they are not directly reachable via the type instance.<br/>
        /// <para/>Accessing this property triggers the build up of the method cache for the current type.</remarks>
        public MethodList Methods
        {
            get
            {
                if (this._methods is null)
                {
                    Type thisType = UnwrapType();
                    MethodInfo[] visibleMethods = thisType.GetMethods(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    this._methods = BuildAndEnumerateMemberKindCache<MethodData>(visibleMethods, HelperExtensionsCommon.AllMembersFullHierarchyFlags)
                        .ToMethodList();
                }

                return this._methods!;
            }
        }

        /// <summary>
        /// Gets all reachable fields of the current type.
        /// </summary>
        /// <remarks>The returned <see cref="FieldList"/> contains all fields that are reachable from the current type:
        /// <list type="bullet">
        /// <item>public protected and private instance and static fields of the current type</item>
        /// <item>implemented public interface fields (either implemented directly or through a base class)</item>
        /// <item>inherited public and protected instance and static fields</item>
        /// </list>
        /// To obtain private fields of a superclass read the <see cref="Fields"/> property of that particular superclass.<br/> 
        /// <para/>Accessing this property triggers the build up of the field cache for the current type.</remarks>
        public FieldList Fields
        {
            get
            {
                if (this._fields is null)
                {
                    Type thisType = UnwrapType();
                    FieldInfo[] visibleFields = thisType.GetFields(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    this._fields = BuildAndEnumerateMemberKindCache<FieldData>(visibleFields, HelperExtensionsCommon.AllMembersFullHierarchyFlags)
                        .ToFieldList();
                }

                return this._fields!;
            }
        }

        /// <summary>
        /// Gets all reachable events of the current type.
        /// </summary>
        /// <remarks>The returned <see cref="EventList"/> contains all events that are reachable from the current type:
        /// <list type="bullet">
        /// <item>public protected and private instance and static events of the current type</item>
        /// <item>implemented public interface events (either implemented directly or through a base class)</item>
        /// <item>inherited public and protected instance and static events</item>
        /// </list>
        /// To obtain private events of a superclass read the <see cref="Events"/> property of that particular superclass.<br/>
        /// Note: explicit interface implementation events are not included in the returned list as they are not directly reachable via the type instance.<br/>
        /// <para/>Accessing this property triggers the build up of the event cache for the current type.</remarks>
        public EventList Events
        {
            get
            {
                if (this._events is null)
                {
                    Type thisType = UnwrapType();
                    EventInfo[] visibleEvents = thisType.GetEvents(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    this._events = BuildAndEnumerateMemberKindCache<EventData>(visibleEvents, HelperExtensionsCommon.AllMembersFullHierarchyFlags)
                        .ToEventList();
                }

                return this._events!;
            }
        }

        /// <summary>
        /// Gets all reachable constructors of the current type.
        /// </summary>
        /// <remarks>The returned <see cref="ConstructorList"/> contains all constructors that are reachable from the current type:
        /// <list type="bullet">
        /// <item>public protected and private instance and static constructors of the current type</item>
        /// <item>implemented public interface constructors (either implemented directly or through a base class)</item>
        /// <item>inherited public and protected instance and static constructors</item>
        /// </list>
        /// To obtain private constructors of a superclass read the <see cref="Constructors"/> property of that particular superclass.<br/> 
        /// <para/>Accessing this property triggers the build up of the constructor cache for the current type.</remarks>
        public ConstructorList Constructors
        {
            get
            {
                if (this._constructors is null)
                {
                    Type thisType = UnwrapType();
                    ConstructorInfo[] visibleConstructors = thisType.GetConstructors(HelperExtensionsCommon.AllMembersFullHierarchyFlags);
                    this._constructors = BuildAndEnumerateMemberKindCache<ConstructorData>(visibleConstructors, HelperExtensionsCommon.AllMembersFullHierarchyFlags)
                        .ToConstructorList();
                }

                return this._constructors!;
            }
        }

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

        /// <summary>
        /// Determines whether the specified type represents a delegate type, excluding the base <see cref="MulticastDelegate"/> and <see cref="Delegate"/> type
        /// itself.
        /// </summary>
        /// <param name="type">The type to evaluate. Cannot be null.</param>
        /// <returns>true if the specified type is a delegate type other than <see cref="MulticastDelegate"/> and <see cref="Delegate"/>; otherwise, false.</returns>
        private static bool IsTypeDelegate(Type type)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(type, nameof(type));

            return type != typeof(Delegate)
                && type != typeof(MulticastDelegate)
                && TypeData.DelegateType.IsAssignableFrom(type);
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
