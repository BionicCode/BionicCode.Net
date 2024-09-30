namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  internal  class TypeData : SymbolInfoData
  {
    private string displayName;
    private string shortDisplayName;
    private string fullyQualifiedDisplayName;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private bool? canDeclareExtensionMethod;
    private bool? isAwaitable;
    private string signature;
    private string shortSignature;
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

#if !NETFRAMEWORK && !NETSTANDARD2_0
    private bool? isByRefLike;
#endif

    public TypeData(Type type) : base(type.Name)
    {
      this.Handle = type.TypeHandle;
      this.Namespace = type.Namespace;
    }

    new public Type GetType()
      => Type.GetTypeFromHandle(this.Handle);

    public PropertyData GetProperty(string propertyName)
    {
      ISymbolInfoDataCacheKey cacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(propertyName, this.Namespace, this.Handle);
      if (SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(cacheKey, out PropertyData propertyData))
      {
        return propertyData;
      }

      throw new ArgumentException($"Unable to find a property named '{propertyName}' on type '{this.Namespace}.{this.Name}'.", nameof(propertyName));
    }

    public MethodData GetMethod(string methodName)
    {
      ISymbolInfoDataCacheKey cacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(methodName, this.Namespace, this.Handle);
      if (SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(cacheKey, out MethodData methodData))
      {
        return methodData;
      }

      throw new ArgumentException($"Unable to find a method named '{methodName}' on type '{this.Namespace}.{this.Name}'.", nameof(methodName));
    }

    public FieldData GetField(string fieldName)
    {
      ISymbolInfoDataCacheKey cacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(fieldName, this.Namespace, this.Handle);
      if (SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(cacheKey, out FieldData methodData))
      {
        return methodData;
      }

      throw new ArgumentException($"Unable to find a field named '{fieldName}' on type '{this.Namespace}.{this.Name}'.", nameof(fieldName));
    }

    public EventData GetEvent(string eventName)
    {
      ISymbolInfoDataCacheKey cacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(eventName, this.Namespace, this.Handle);
      if (SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(cacheKey, out EventData methodData))
      {
        return methodData;
      }

      throw new ArgumentException($"Unable to find an event named '{eventName}' on type '{this.Namespace}.{this.Name}'.", nameof(eventName));
    }

    public ConstructorData GetConstructor(string constructorName)
    {
      ISymbolInfoDataCacheKey cacheKey = SymbolReflectionInfoCache.CreateMemberSymbolCacheKey(constructorName, this.Namespace, this.Handle);
      if (SymbolReflectionInfoCache.TryGetOrCreateSymbolInfoDataCacheEntry(cacheKey, out ConstructorData methodData))
      {
        return methodData;
      }

      throw new ArgumentException($"Unable to find a constructor named '{constructorName}' on type '{this.Namespace}.{this.Name}'.", nameof(constructorName));
    }

    public RuntimeTypeHandle Handle { get; }
    public string Namespace { get; }

    public bool IsAwaitable 
      => (bool)(this.isAwaitable ?? (this.isAwaitable = HelperExtensionsCommon.IsAwaitableInternal(this)));

    public bool IsValueType
      => (bool)(this.isValueType ?? (this.isValueType = GetType().IsValueType));

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
      => (bool)(this.canDeclareExtensionMethod ?? (this.canDeclareExtensionMethod = HelperExtensionsCommon.CanDeclareExtensionMethodsInternal(this)));

    public override IList<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = GetType().GetCustomAttributesData());

    public AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined 
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: false, isCompact: false));

    public override string ShortSignature
      => this.shortSignature ?? (this.shortSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override string DisplayName
      => this.displayName ?? (this.displayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: false, isCompact: true));

    public override string ShortDisplayName
      => this.shortDisplayName ?? (this.shortDisplayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: true));

    public override string FullyQualifiedDisplayName 
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: true, isShortName: false, isCompact: true));

    public override string AssemblyName
      => this.assemblyName ?? (this.assemblyName = GetType().Assembly.GetName().Name);

    public bool IsStatic 
      => (bool)(this.isStatic ?? (this.isStatic = HelperExtensionsCommon.IsStaticInternal(this)));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined 
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this)) 
      : this.symbolAttributes;

    public bool IsAbstract 
      => (bool)(this.isAbstract ?? (this.isAbstract = GetType().IsAbstract));

    public bool IsSealed 
      => (bool)(this.isSealed ?? (this.isSealed = GetType().IsSealed));

    public bool IsByRef
      => (bool)(this.isByRef ?? (this.isByRef = GetType().IsByRef));

#if !NETFRAMEWORK && !NETSTANDARD2_0
    public bool IsByRefLike
      => (bool)(this.isByRefLike ?? (this.isByRefLike = GetType().IsByRefLike));
#endif

    public bool IsDelegate
      => (bool)(this.isDelegate ?? (this.isDelegate = GetType().IsDelegateInternal()));

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

    public bool IsGenericType
      => (bool)(this.isGenericType ?? (this.isGenericType = GetType().IsGenericType));

    public bool IsBuiltInType
      => (bool)(this.isBuiltInType ?? (this.isBuiltInType = HelperExtensionsCommon.IsBuiltInTypeInternal(this)));

    public bool IsGenericTypeDefinition
      => (bool)(this.isGenericTypeDefinition ?? (this.isGenericTypeDefinition = GetType().IsGenericTypeDefinition));

    public GenericParameterAttributes GenericParameterAttributes
      => (GenericParameterAttributes)(this.genericParameterAttributes ?? (this.genericParameterAttributes = GetType().GenericParameterAttributes));

    public TypeData[] GenericParameterConstraintsData
      => this.genericParameterConstraintsData ?? (this.genericParameterConstraintsData = GetType().GetGenericParameterConstraints().Where(constraint => constraint != typeof(object) && constraint != typeof(ValueType)).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public TypeData[] InterfacesData
      => this.interfacesData ?? (this.interfacesData = GetType().GetInterfaces().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public PropertyData[] PropertiesData
      => this.propertiesData ?? (this.propertiesData = GetType().GetProperties(SymbolInfoData.AllMembersFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public MethodData[] MethodsData
      => this.methodsData ?? (this.methodsData = GetType().GetMethods(SymbolInfoData.AllMembersFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public FieldData[] FieldsData
      => this.fieldsData ?? (this.fieldsData = GetType().GetFields(SymbolInfoData.AllMembersFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public EventData[] EventsData
      => this.eventsData ?? (this.eventsData = GetType().GetEvents(SymbolInfoData.AllMembersFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

    public ConstructorData[] ConstructorsData
      => this.constructorsData ?? (this.constructorsData = GetType().GetConstructors(SymbolInfoData.AllMembersFlags).Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());
  }
}