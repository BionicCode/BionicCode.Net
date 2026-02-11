[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

internal class TypeData : SymbolInfoData, ITypeDataView
{
    private delegate bool MethodEqualityComparer<TParameterList>(ParameterList foundMethodParameters, TParameterList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
        where TParameterList : notnull, IEnumerable;

    private static readonly Type s_taskType = typeof(Task);
    private static readonly Type s_valueTaskType = typeof(ValueTask);
    private static readonly Type s_valueTaskGenericType = typeof(ValueTask<>);
    private static readonly Type s_delegateType = typeof(MulticastDelegate);
    private const BindingFlags BindingFlagsPublicMask = BindingFlags.Public | BindingFlags.NonPublic;
    private const BindingFlags BindingFlagsStaticMask = BindingFlags.Static | BindingFlags.Instance;

    private string? _displayName;
    private string? _shortDisplayName;
    private string? _fullyQualifiedDisplayName;
    private SymbolAttributes _symbolAttributes;
    private AccessModifier _accessModifier;
    private bool? _canDeclareExtensionMethod;
    private bool? _isAwaitable;
    private bool? _isAwaitableTask;
    private bool? _isAwaitableValueTask;
    private string? _signature;
    private string? _shortSignature;
    private string? _fullyQualifiedRuntimeSignature;
    private string? _runtimeSignature;
    private string? _runtimeShortSignature;
    private string? _runtimeShortCompactSignature;
    private string? _shortCompactSignature;
    private string? _fullyQualifiedSignature;
    private bool? _isStatic;
    private bool? _isBuiltInType;
    private bool? _isAbstract;
    private bool? _isSealed;
    private bool? _isValueType;
    private bool? _isByRef;
    private bool? _isSubclass;
    private bool? _isDelegate;
    private bool? _isGenericType;
    private bool? _isGenericTypeDefinition;
    private TypeList? _genericTypeArguments;
    private TypeList? _genericParameterConstraintsData;
    private GenericParameterAttributes? _genericParameterAttributes;
    private TypeData? _genericTypeDefinition;
    private TypeData? _baseTypes;
    private TypeList? _interfaces;
    private PropertyList? _properties;
    private MethodList? _methods;
    private FieldList? _fields;
    private EventList? _events;
    private ConstructorList? _constructors;
    private IList<CustomAttributeData>? _attributeData;
    private string? _assemblyName;
    private MethodData? _delegateInvokeMethodData;
    private SymbolComponentInfo? _symbolComponentInfo;
    private SymbolComponentInfo? _compactSymbolComponentInfo;
    private bool? _containsGenericParameters;
    private readonly ConcurrentHashSet<SymbolReflectionInfoCacheKey> _memberTable;
    private readonly ConcurrentDictionary<SymbolKind, bool> _memberTableStateFlagTable;
    private bool? _isByRefLike;
    private bool? _isGenericTypeParameter;
    private bool? _isGenericMethodParameter;
    private bool? _isGenericParameter;
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
    private string? _namespace;
    private PropertyList? _explicitInterfaceProperties;
    private MethodList? _explicitInterfaceMethods;
    private EventList? _explicitInterfaceEvents;
    private readonly WellKnownTypeDescriptor _descriptor;

    internal TypeData(SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
        : base(symbolInfoDataCacheKey.TypeDescriptor.TypeNamespace, SymbolKind.Type, symbolInfoDataCacheKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(symbolInfoDataCacheKey, nameof(symbolInfoDataCacheKey));

        _descriptor = symbolInfoDataCacheKey.TypeDescriptor;
        Type = _descriptor.Type;
        Handle = _descriptor.TypeHandle;
        _namespace = _descriptor.TypeNamespace;
        _memberTable = [];
        _memberTableStateFlagTable = new ConcurrentDictionary<SymbolKind, bool>();
    }

    internal Type Type { get; }

    /// <summary>
    /// Returns the underlying <see cref="Type"/> represented by this handle.
    /// </summary>
    /// <returns>A <see cref="Type"/> object that is referenced by this handle.</returns>
    //internal Type Type => Type.GetTypeFromHandle(Handle)!;

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
        return Properties.TryGetPropertyByName(propertyName, out propertyData);
    }

    public bool TryGetExplicitInterfacePropertyByName(string propertyName, out PropertyData? propertyData)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(propertyName);
        return ExplicitInterfaceProperties.TryGetPropertyByName(propertyName, out propertyData);
    }

    public bool TryGetIndexerPropertyByParameterList(ParameterList parameters, PropertyAccessors propertyAccessor, out PropertyData? propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(parameters);
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(propertyAccessor);

        propertyData = null;
        foreach (PropertyData property in Properties)
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

    public bool TryGetExplicitInterfaceIndexerPropertyByParameterList(ParameterList parameters, PropertyAccessors propertyAccessor, out PropertyData? propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(parameters);
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<PropertyAccessors>(propertyAccessor);

        propertyData = null;
        foreach (PropertyData property in ExplicitInterfaceProperties)
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
        foreach (PropertyData property in Properties)
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

    public bool TryGetExplicitInterfaceIndexerPropertyByParameterList(MethodParameterInfoList indexerParameters, PropertyAccessors indexerPropertyAccessor, out PropertyData? propertyData)
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
        foreach (PropertyData property in ExplicitInterfaceProperties)
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
    public IEnumerable<PropertyData> EnumerateProperties(BindingFlags bindingFlags = ReflectionHelperExtensions.AllMembersFullHierarchyFlags)
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
        return Methods.TryGetMethodsByName(methodName, out methods);
    }

    public bool TryGetMethod(string methodName, TypeList genericMethodParameters, ParameterList parameters, out MethodData? methodData)
    {
        parameters = parameters.OrEmpty();
        genericMethodParameters = genericMethodParameters.OrEmpty();

        static bool equalityComparer(ParameterList foundMethodParameters, ParameterList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
        {
            return foundMethodName.Equals(requestedMethodName, StringComparison.Ordinal)
                && foundMethodParameters.Equals(requestedMethodParameters)
                && foundGenericMethodParameters.Equals(requestedGenericMethodParameters);
        }

        return TryGetMethodInternal(methodName, isExplicitImplementation: false, genericMethodParameters, parameters, equalityComparer, out methodData);
    }

    public bool TryGetExplicitInterfaceMethod(string methodName, TypeList genericMethodParameters, ParameterList parameters, out MethodData? methodData)
    {
        parameters = parameters.OrEmpty();
        genericMethodParameters = genericMethodParameters.OrEmpty();

        static bool equalityComparer(ParameterList foundMethodParameters, ParameterList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
        {
            return foundMethodName.Equals(requestedMethodName, StringComparison.Ordinal)
                && foundMethodParameters.Equals(requestedMethodParameters)
                && foundGenericMethodParameters.Equals(requestedGenericMethodParameters);
        }

        return TryGetMethodInternal(methodName, isExplicitImplementation: true, genericMethodParameters, parameters, equalityComparer, out methodData);
    }

    public bool TryGetMethod(string methodName, TypeList genericMethodParameters, MethodParameterInfoList parameters, out MethodData? methodData)
    {
        parameters = parameters.OrEmpty();
        genericMethodParameters = genericMethodParameters.OrEmpty();

        static bool equalityComparer(ParameterList foundMethodParameters, MethodParameterInfoList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
        {
            return foundMethodName.Equals(requestedMethodName, StringComparison.Ordinal)
                && ParameterListEqualityComparer.Equals(foundMethodParameters, requestedMethodParameters)
                && foundGenericMethodParameters.Equals(requestedGenericMethodParameters);
        }

        return TryGetMethodInternal(methodName, isExplicitImplementation: false, genericMethodParameters, parameters, equalityComparer, out methodData);
    }

    public bool TryGetExplicitInterfaceMethod(string methodName, TypeList genericMethodParameters, MethodParameterInfoList parameters, out MethodData? methodData)
    {
        parameters = parameters.OrEmpty();
        genericMethodParameters = genericMethodParameters.OrEmpty();

        static bool equalityComparer(ParameterList foundMethodParameters, MethodParameterInfoList requestedMethodParameters, TypeList foundGenericMethodParameters, TypeList requestedGenericMethodParameters, string foundMethodName, string requestedMethodName)
        {
            return foundMethodName.Equals(requestedMethodName, StringComparison.Ordinal)
                && ParameterListEqualityComparer.Equals(foundMethodParameters, requestedMethodParameters)
                && foundGenericMethodParameters.Equals(requestedGenericMethodParameters);
        }

        return TryGetMethodInternal(methodName, isExplicitImplementation: true, genericMethodParameters, parameters, equalityComparer, out methodData);
    }

    private bool TryGetMethodInternal<TParameterList>(string requestedMethodName, bool isExplicitImplementation, TypeList requestedGenericMethodParameters, TParameterList requestedMethodParameters, MethodEqualityComparer<TParameterList> equalityComparer, out MethodData? requestedMethodData)
        where TParameterList : notnull, IEnumerable
    {
        requestedMethodData = null;
        MethodList source = isExplicitImplementation ? ExplicitInterfaceMethods : Methods;
        foreach (MethodData method in source)
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
    public IEnumerable<MethodData> EnumerateMethods(BindingFlags bindingFlags = ReflectionHelperExtensions.AllMembersFullHierarchyFlags)
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
        return Fields.TryGetFieldByName(fieldName, out fieldData);
    }

    public IEnumerable<FieldData> EnumerateFields(BindingFlags bindingFlags = ReflectionHelperExtensions.AllMembersFullHierarchyFlags)
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
        return Events.TryGetEventByName(eventName, out eventData);
    }
    public bool TryGetExplicitInterfaceEvent(RuntimeTypeHandle declaringInterfaceTypeHandle, string eventName, out EventData? eventData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(declaringInterfaceTypeHandle);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventName);
        return ExplicitInterfaceEvents.TryGetEventByName(eventName, out eventData);
    }

    public IEnumerable<EventData> EnumerateEvents(BindingFlags bindingFlags = ReflectionHelperExtensions.AllMembersFullHierarchyFlags)
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
        foreach (ConstructorData constructor in Constructors)
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
        foreach (ConstructorData constructor in Constructors)
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

    public IEnumerable<ConstructorData> EnumerateConstructors(BindingFlags bindingFlags = ReflectionHelperExtensions.AllMembersFullHierarchyFlags)
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

            SymbolKind.MemberMethod => Methods.Concat(ExplicitInterfaceMethods),
            SymbolKind.MemberProperty => Properties.Concat(ExplicitInterfaceProperties),
            SymbolKind.MemberField => Fields,
            SymbolKind.MemberEvent => Events.Concat(ExplicitInterfaceEvents),
            SymbolKind.MemberConstructor => Constructors,
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

    private void BuildAndEnumerateMemberKindCache<TMemberData>(MemberInfo[] members, BindingFlags bindingFlags) where TMemberData : MemberData
    {
        Func<MemberInfo, MemberData> readReflectionCache;
        Action<MemberData> addMemberToTypeDataMemberList;
        Action fieldInitializer;
        SymbolKind memberKind;
        switch (typeof(TMemberData))
        {
            case Type memberType when memberType == typeof(PropertyData):
                readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((PropertyInfo)memberInfo);
                IPropertyListBuilder propertyListBuilder = PropertyListBuilder.New(Handle);
                IPropertyListBuilder explicitPropertyListBuilder = PropertyListBuilder.New(Handle);
                addMemberToTypeDataMemberList = memberData =>
                {
                    var propertyData = (PropertyData)memberData;
                    if (propertyData.IsExplicitInterfaceImplementation)
                    {
                        _ = explicitPropertyListBuilder.Add(propertyData);
                    }
                    else
                    {
                        _ = propertyListBuilder.Add(propertyData);
                    }
                };

                fieldInitializer = () =>
                {
                    _explicitInterfaceProperties = explicitPropertyListBuilder.Build();
                    _properties = propertyListBuilder.Build();
                };
                memberKind = SymbolKind.MemberProperty;
                break;
            case Type memberType when memberType == typeof(MethodData):
                readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((MethodInfo)memberInfo);
                IMethodListBuilder methodListBuilder = MethodListBuilder.New(Handle);
                IMethodListBuilder explicitMethodListBuilder = MethodListBuilder.New(Handle);
                addMemberToTypeDataMemberList = methodData =>
                {
                    if (methodData.IsExplicitInterfaceImplementation)
                    {
                        _ = explicitMethodListBuilder.Add((MethodData)methodData);
                    }
                    else
                    {
                        _ = methodListBuilder.Add((MethodData)methodData);
                    }
                };

                fieldInitializer = () =>
                {
                    _methods = methodListBuilder.Build();
                    _explicitInterfaceMethods = explicitMethodListBuilder.Build();
                };
                memberKind = SymbolKind.MemberMethod;
                break;
            case Type memberType when memberType == typeof(FieldData):
                readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((FieldInfo)memberInfo);
                IFieldListBuilder fieldListBuilder = FieldListBuilder.New(Handle);
                addMemberToTypeDataMemberList = fieldData => fieldListBuilder.Add((FieldData)fieldData);
                fieldInitializer = () => _fields = fieldListBuilder.Build();
                memberKind = SymbolKind.MemberField;
                break;
            case Type memberType when memberType == typeof(EventData):
                readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((EventInfo)memberInfo);
                IEventListBuilder eventListBuilder = EventListBuilder.New(Handle);
                IEventListBuilder explicitEventListBuilder = EventListBuilder.New(Handle);
                addMemberToTypeDataMemberList = eventData =>
                {
                    if (eventData.IsExplicitInterfaceImplementation)
                    {
                        _ = explicitEventListBuilder.Add((EventData)eventData);
                    }
                    else
                    {
                        _ = eventListBuilder.Add((EventData)eventData);
                    }
                };

                fieldInitializer = () =>
                {
                    _events = eventListBuilder.Build();
                    _explicitInterfaceEvents = explicitEventListBuilder.Build();
                };
                memberKind = SymbolKind.MemberEvent;
                break;
            case Type memberType when memberType == typeof(ConstructorData):
                readReflectionCache = (memberInfo) => SymbolReflectionInfoCache.GetOrCreateSymbolReflectionInfoCacheEntry((ConstructorInfo)memberInfo);
                IConstructorListBuilder constructorListBuilder = ConstructorListBuilder.New(Handle);
                addMemberToTypeDataMemberList = constructorData => constructorListBuilder.Add((ConstructorData)constructorData);
                fieldInitializer = () => _constructors = constructorListBuilder.Build();
                memberKind = SymbolKind.MemberConstructor;
                break;
            default:
                throw new NotSupportedException($"The member type '{typeof(TMemberData).FullName}' is not supported.");
        }

        for (int memberIndex = 0; memberIndex < members.Length; memberIndex++)
        {
            MemberInfo memberInfo = members[memberIndex];
            if (memberInfo is null)
            {
                continue;
            }

            var memberDataFromReflectionCache = (TMemberData)readReflectionCache.Invoke(memberInfo);
            addMemberToTypeDataMemberList.Invoke(memberDataFromReflectionCache);
            SymbolReflectionInfoCacheKey cacheKey = memberDataFromReflectionCache.CacheKey;
            _ = _memberTable.TryAdd(cacheKey);
        }

        _ = _memberTableStateFlagTable.TryAdd(memberKind, true);
        fieldInitializer.Invoke();
    }

    private bool IsCacheBuildForMemberKind(SymbolKind memberKind) => _memberTableStateFlagTable.TryGetValue(memberKind, out bool isCacheReady)
        && isCacheReady;

    private bool IsValidMember(MemberData cachedMemberData, BindingFlags bindingFlags)
    {
        // FlattenHierarchy: inherited private static members are not returned when FlattenHierarchy is specified.
        if (bindingFlags.HasFlag(BindingFlags.FlattenHierarchy)
            && !cachedMemberData.DeclaringTypeHandle.Equals(Handle)
            && cachedMemberData.IsPrivate
            && cachedMemberData.IsStatic)
        {
            return false;
        }

        // DeclaredOnly: only members declared on this type
        if (bindingFlags.HasFlag(BindingFlags.DeclaredOnly)
            && !cachedMemberData.DeclaringTypeHandle.Equals(Handle))
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
    /// <inheritdoc/>
    public override string Namespace => _namespace ??= Type.GetTypeFromHandle(Handle) is Type type
        ? (type.Namespace ?? string.Empty)
        : throw new InvalidOperationException("The registered runtime handle is not valid.");

    public bool IsAwaitable => _isAwaitable ??= TypeData.IsTypeAwaitable(this);

    public bool IsAwaitableTask => _isAwaitableTask ??= TypeData.IsTypeAwaitableTask(this);

    public bool IsAwaitableValueTask => _isAwaitableValueTask ??= TypeData.IsTypeAwaitableValueTask(this);

    public TypeData GenericTypeDefinitionData
    {
        get
        {
            if (IsGenericTypeDefinition)
            {
                return this;
            }
            else
            {
                Type genericTypeDefinitionType = Type.GetGenericTypeDefinition();
                _genericTypeDefinition = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericTypeDefinitionType);
            }

            return _genericTypeDefinition;
        }
    }

    public TypeList GenericTypeArguments => _genericTypeArguments ??= TypeListBuilder.CreateGenericTypeArgumentList(this);

    public bool CanDeclareExtensionMethod => (bool)(bool?)(_canDeclareExtensionMethod ??= TypeData.CanDeclareExtensionMethods(this));

    /// <inheritdoc/>
    public override IList<CustomAttributeData> AttributeData => _attributeData ??= Type.GetCustomAttributesData();

    public AccessModifier AccessModifier => _accessModifier is AccessModifier.Undefined
        ? (_accessModifier = TypeData.GetAccessModifier(this))
        : _accessModifier;

    /// <inheritdoc/>
    public override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isCompact: false);

    /// <inheritdoc/>
    public SymbolComponentInfo CompactSymbolComponentInfo => _compactSymbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isCompact: true);

    /// <inheritdoc/>
    public override string Signature => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    public override string ShortSignature => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    public override string ShortCompactSignature => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: true, isRuntimeSymbol: false);

    /// <inheritdoc/>
    public override string FullyQualifiedRuntimeSignature => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    public override string RuntimeSignature => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    public override string RuntimeShortSignature => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: false, isRuntimeSymbol: true);

    /// <inheritdoc/>
    public override string RuntimeShortCompactSignature => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isCompact: true, isRuntimeSymbol: true);

    /// <inheritdoc/>
    public override string FullyQualifiedSignature => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isCompact: false, isRuntimeSymbol: false);

    /// <inheritdoc/>
    public override string DisplayName => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    /// <inheritdoc/>
    public override string ShortDisplayName => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    /// <inheritdoc/>
    public override string FullyQualifiedDisplayName => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    /// <inheritdoc/>
    public override string AssemblyName => _assemblyName ??= Type.Assembly.GetName().Name ?? string.Empty;

    public bool IsStatic => _isStatic ??= TypeData.IsTypeStatic(this);

    public override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
      ? (_symbolAttributes = TypeData.GetAttributes(this))
      : _symbolAttributes;

    public bool IsAbstract => _isAbstract ??= Type.IsAbstract;

    public bool IsSealed => _isSealed ??= Type.IsSealed;

    public bool IsByRef => _isByRef ??= Type.IsByRef;

    public bool IsByRefLike => _isByRefLike ??= Type.IsByRefLike;

    public bool IsDelegate => _isDelegate ??= TypeData.IsTypeDelegate(Type);

    public bool IsClass => _isClass ??= Type.IsClass;

    public bool IsInterface => _isInterface ??= Type.IsInterface;

    public bool IsEnum => _isEnum ??= Type.IsEnum;

    public bool IsValueType => _isValueType ??= Type.IsValueType;

    public bool IsReferenceType => !IsValueType;

    public bool IsStruct => _isStruct ??= !IsEnum && IsValueType;

    public bool IsReadOnlyStruct => _isReadOnlyStruct ??= IsReadOnlyStructInternal(this);

    public bool IsPublic => _isPublic ??= Type.IsPublic;

    public bool IsNestedPrivate => _isNestedPrivate ??= Type.IsNestedPrivate;

    public bool IsNestedAssembly => _isNestedAssembly ??= Type.IsNestedAssembly;

    public bool IsNestedFamily => _isNestedFamily ??= Type.IsNestedFamily;

    public bool IsNestedPublic => _isNestedPublic ??= Type.IsNestedPublic;

    public bool IsNestedFamORAssem => _isNestedFamORAssem ??= Type.IsNestedFamORAssem;

    public bool IsNestedFamANDAssem => _isNestedFamANDAssem ??= Type.IsNestedFamANDAssem;

    public bool IsVisible => _isVisible ??= Type.IsVisible;

    public bool IsSubclass
    {
        get
        {
            if (_isSubclass is null)
            {
                Type? baseType = Type.BaseType;
                _isSubclass = baseType is not null
                  && baseType != typeof(object)
                  && baseType != typeof(ValueType);
            }

            return (bool)_isSubclass;
        }
    }

    public TypeData? BaseTypeData
    {
        get
        {
            if (_baseTypes is null && IsSubclass)
            {
                Type? baseType = Type.BaseType;
                _baseTypes = baseType is null
                    ? null
                    : SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(baseType);
            }

            return _baseTypes;
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
            if (!IsDelegate)
            {
                throw new InvalidOperationException($"The current type is not a delegate. Call {nameof(IsDelegate)} before accessing this property to check whether the current type is a delegate.");
            }

            _delegateInvokeMethodData ??= Methods.TryGetMethodsByName(ReflectionConstants.DelegateInvocatorMethodName, out MethodList methods)
                ? methods[0]
                : throw new InvalidOperationException($"The delegate type '{Name}' does not have a valid invoke method.");

            return _delegateInvokeMethodData!;
        }
    }

    public bool IsGenericTypeParameter => _isGenericTypeParameter ??= Type.IsGenericTypeParameter;

    public bool IsGenericMethodParameter => _isGenericMethodParameter ??= Type.IsGenericMethodParameter;

    public bool IsGenericParameter => _isGenericParameter ??= Type.IsGenericParameter;

    public bool IsGenericType => _isGenericType ??= Type.IsGenericType;

    /// <summary>
    /// Gets a value indicating whether the type is a built-in .NET type.
    /// </summary>
    public bool IsBuiltInType => _isBuiltInType ??= TypeData.IsTypeBuiltInType(this);

    public bool IsGenericTypeDefinition => _isGenericTypeDefinition ??= Type.IsGenericTypeDefinition;

    public bool ContainsGenericParameters => _containsGenericParameters ??= Type.ContainsGenericParameters;

    public GenericParameterAttributes GenericParameterAttributes => _genericParameterAttributes ??= Type.GenericParameterAttributes;

    public TypeList GenericParameterConstraintsData => _genericParameterConstraintsData ??= TypeListBuilder.CreateGenericTypeArgumentConstraintList(this);

    public TypeList InterfacesData => _interfaces ??= TypeListBuilder.CreateImplementedInterfacesList(this);

    /// <summary>
    /// Gets all reachable properties of the current type.
    /// </summary>
    /// <remarks>The returned <see cref="PropertyList"/> contains all properties that are reachable from the current type:
    /// <list type="bullet">
    /// <item>public, protected and private instance and static properties of the current type</item>
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
            if (_properties is null)
            {
                PropertyInfo[] visibleProperties = Type.GetProperties(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<PropertyData>(visibleProperties, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _properties!;
        }
    }

    public PropertyList ExplicitInterfaceProperties
    {
        get
        {
            if (_explicitInterfaceProperties is null)
            {
                PropertyInfo[] visibleProperties = Type.GetProperties(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<PropertyData>(visibleProperties, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _explicitInterfaceProperties!;
        }
    }

    /// <summary>
    /// Gets all reachable methods of the current type.
    /// </summary>
    /// <remarks>The returned <see cref="MethodList"/> contains all methods that are reachable from the current type:
    /// <list type="bullet">
    /// <item>public, protected and private instance and static methods of the current type</item>
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
            if (_methods is null)
            {
                MethodInfo[] visibleMethods = Type.GetMethods(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<MethodData>(visibleMethods, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _methods!;
        }
    }
    public MethodList ExplicitInterfaceMethods
    {
        get
        {
            if (_explicitInterfaceMethods is null)
            {
                MethodInfo[] visibleMethods = Type.GetMethods(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<MethodData>(visibleMethods, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _explicitInterfaceMethods!;
        }
    }

    /// <summary>
    /// Gets all reachable fields of the current type.
    /// </summary>
    /// <remarks>The returned <see cref="FieldList"/> contains all fields that are reachable from the current type:
    /// <list type="bullet">
    /// <item>public, protected and private instance and static fields of the current type</item>
    /// <item>implemented public interface fields (either implemented directly or through a base class)</item>
    /// <item>inherited public and protected instance and static fields</item>
    /// </list>
    /// To obtain private fields of a superclass read the <see cref="Fields"/> property of that particular superclass.<br/> 
    /// <para/>Accessing this property triggers the build up of the field cache for the current type.</remarks>
    public FieldList Fields
    {
        get
        {
            if (_fields is null)
            {
                FieldInfo[] visibleFields = Type.GetFields(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<FieldData>(visibleFields, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _fields!;
        }
    }

    /// <summary>
    /// Gets all reachable events of the current type.
    /// </summary>
    /// <remarks>The returned <see cref="EventList"/> contains all events that are reachable from the current type:
    /// <list type="bullet">
    /// <item>public, protected and private instance and static events of the current type</item>
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
            if (_events is null)
            {
                EventInfo[] visibleEvents = Type.GetEvents(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<EventData>(visibleEvents, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _events!;
        }
    }
    public EventList ExplicitInterfaceEvents
    {
        get
        {
            if (_explicitInterfaceEvents is null)
            {
                EventInfo[] visibleEvents = Type.GetEvents(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<EventData>(visibleEvents, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _explicitInterfaceEvents!;
        }
    }

    /// <summary>
    /// Gets all reachable constructors of the current type.
    /// </summary>
    /// <remarks>The returned <see cref="ConstructorList"/> contains all constructors that are reachable from the current type:
    /// <list type="bullet">
    /// <item>public, protected and private instance and static constructors of the current type</item>
    /// <item>implemented public interface constructors (either implemented directly or through a base class)</item>
    /// <item>inherited public and protected instance and static constructors</item>
    /// </list>
    /// To obtain private constructors of a superclass read the <see cref="Constructors"/> property of that particular superclass.<br/> 
    /// <para/>Accessing this property triggers the build up of the constructor cache for the current type.</remarks>
    public ConstructorList Constructors
    {
        get
        {
            if (_constructors is null)
            {
                ConstructorInfo[] visibleConstructors = Type.GetConstructors(ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
                BuildAndEnumerateMemberKindCache<ConstructorData>(visibleConstructors, ReflectionHelperExtensions.AllMembersFullHierarchyFlags);
            }

            return _constructors!;
        }
    }

    private static bool IsTypeStatic(TypeData typeData)
      => typeData.IsAbstract && typeData.IsSealed;

    private static bool IsTypeBuiltInType(TypeData typeData)
    {
        var typeReference = new CodeTypeReference(typeData.Type);
        string typeName = ReflectionHelperExtensions.CodeProvider.GetTypeOutput(typeReference);
        int typeNameStartIndex = typeName.LastIndexOf('.') + 1;
        if (typeNameStartIndex > 0)
        {
            typeName = typeName[typeNameStartIndex..];
        }

        return !ReflectionHelperExtensions.CodeProvider.IsValidIdentifier(typeName);
    }

    private static bool CanDeclareExtensionMethods(TypeData typeData)
    {
        Type typeInfo = typeData.Type;
        if (!typeData.IsStatic || typeInfo.IsNested || typeInfo.IsGenericType)
        {
            return false;
        }

        Attribute? typeExtensionAttribute = typeInfo.GetCustomAttribute(ReflectionHelperExtensions.ExtensionAttributeType, false);
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
            && s_delegateType.IsAssignableFrom(type);
    }

    private static bool IsReadOnlyStructInternal(TypeData typeData) => typeData.IsStruct && typeData.Type.GetCustomAttribute(ReflectionHelperExtensions.IsReadOnlyAttributeType) != null;

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

        Type type = typeData.Type;
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

    private static bool IsTypeAwaitableTask(TypeData type) => TypeData.s_taskType.IsAssignableFrom(type.Type)
        || (type.BaseTypeData?.Type is Type baseType && TypeData.s_taskType.IsAssignableFrom(baseType));

    private static bool IsTypeAwaitableValueTask(TypeData type) => TypeData.s_valueTaskType == type.Type
        || (type.IsGenericType && TypeData.s_valueTaskGenericType == type.GenericTypeDefinitionData.Type);

    private static AccessModifier GetAccessModifier(TypeData typeData) => typeData.IsPublic ? AccessModifier.Public
        : typeData.IsNestedPrivate ? AccessModifier.Private
        : typeData.IsNestedAssembly ? AccessModifier.Internal
        : typeData.IsNestedFamily ? AccessModifier.Protected
        : typeData.IsNestedPublic ? AccessModifier.Public
        : typeData.IsNestedFamORAssem ? AccessModifier.ProtectedInternal
        : typeData.IsNestedFamANDAssem ? AccessModifier.PrivateProtected
        : !typeData.IsVisible ? AccessModifier.Internal
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
}
