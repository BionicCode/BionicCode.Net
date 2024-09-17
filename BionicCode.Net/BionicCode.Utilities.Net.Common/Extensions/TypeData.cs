namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;

  internal  class TypeData : SymbolInfoData
  {
    private HashSet<CustomAttributeData> attributeData;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private bool? canDeclareExtensionMethod;
    private bool? isAwaitable;
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private bool? isStatic;
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

    public TypeData(Type type) : base(type.Name)
    {
      this.Handle = type.TypeHandle;
      this.Namespace = type.Namespace.ToCharArray();
    }

    new public Type GetType()
      => Type.GetTypeFromHandle(this.Handle);

    public RuntimeTypeHandle Handle { get; }
    public char[] Namespace { get; }

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
          this.genericTypeDefinitionData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(genericTypeDefinitionType);
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
          this.genericTypeArguments = typeArguments.Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>).ToArray();
        }

        return this.genericTypeArguments;
      }
    }

    public bool CanDeclareExtensionMethod 
      => (bool)(this.canDeclareExtensionMethod ?? (this.canDeclareExtensionMethod = HelperExtensionsCommon.CanDeclareExtensionMethodsInternal(this)));

    public override HashSet<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new HashSet<CustomAttributeData>(GetType().GetCustomAttributesData()));

    public AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined 
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public override char[] Signature 
      => this.signature ?? (this.signature = GetType().ToSignatureShortName().ToCharArray());

    public override char[] FullyQualifiedSignature 
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = GetType().ToSignatureShortName(isFullyQualifiedName: true).ToCharArray());

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
          this.baseTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(baseType);
        }

        return this.baseTypeData;
      }
    }

    public bool IsGenericType
      => (bool)(this.isGenericType ?? (this.isGenericType = GetType().IsGenericType));

    public bool IsGenericTypeDefinition
      => (bool)(this.isGenericTypeDefinition ?? (this.isGenericTypeDefinition = GetType().IsGenericTypeDefinition));

    public GenericParameterAttributes GenericParameterAttributes
      => (GenericParameterAttributes)(this.genericParameterAttributes ?? (this.genericParameterAttributes = GetType().GenericParameterAttributes));

    public TypeData[] GenericParameterConstraintsData
      => this.genericParameterConstraintsData ?? (this.genericParameterConstraintsData = GetType().GetGenericParameterConstraints().Where(constraint => constraint != typeof(object) && constraint != typeof(ValueType)).Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>).ToArray());

    public TypeData[] InterfacesData
      => this.interfacesData ?? (this.interfacesData = GetType().GetInterfaces().Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>).ToArray());
  }
}