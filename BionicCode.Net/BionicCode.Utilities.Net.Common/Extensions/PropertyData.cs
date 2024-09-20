namespace BionicCode.Utilities.Net
{
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using Microsoft.CodeAnalysis;

  internal sealed class PropertyData : MemberInfoData
  {
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier? propertyAccessModifier;
    private AccessModifier? setAccessorAccessModifier;
    private AccessModifier? getAccessorAccessModifier;
    private ParameterData[] indexerParameters;
    private TypeData propertyTypeData;
    private MethodData getMethodData;
    private MethodData setMethodData;
    private bool? isStatic;
    private bool? isOverride;
    private bool? isSealed;
    private bool? canWrite;
    private bool? canRead;

#if !NETSTANDARD2_0
    private bool? isSetMethodReadOnly;
#endif

    public PropertyData(PropertyInfo propertyInfo) : base(propertyInfo)
    {
      this.PropertyInfo = propertyInfo;
    }

    public PropertyInfo GetPropertyInfo()
      => this.PropertyInfo;

    protected override MemberInfo GetMemberInfo() 
      => GetPropertyInfo();

    private void GetAccessors()
    {
      (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = HelperExtensionsCommon.GetPropertyAccessModifier(this.GetMethodData, this.SetMethodData);
      this.propertyAccessModifier = propertyModifier;
      this.setAccessorAccessModifier = setMethodModifier;
      this.getAccessorAccessModifier = getMethodModifier;
    }

    public bool IsIndexer 
      => this.IndexerParameters.Length > 0;

    public ParameterData[] IndexerParameters 
      => this.indexerParameters ?? (this.indexerParameters = this.PropertyInfo.GetIndexParameters().Select(HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<ParameterData>).ToArray());

    public override AccessModifier AccessModifier
    {
      get
      {
        if (this.propertyAccessModifier is null)
        {
          GetAccessors();
        }

        return this.propertyAccessModifier.Value;
      }
    }

    public AccessModifier SetAccessorAccessModifier
    {
      get
      {
        if (this.setAccessorAccessModifier is null)
        {
          GetAccessors();
        }
        
        return this.setAccessorAccessModifier.Value;
      }
    }

    public AccessModifier GetAccessorAccessModifier
    {
      get
      {
        if (this.getAccessorAccessModifier is null)
        {
          GetAccessors();
        }

        return this.getAccessorAccessModifier.Value;
      }
    }

    public TypeData PropertyTypeData 
      => this.propertyTypeData ?? (this.propertyTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetPropertyInfo().PropertyType)); 

    public PropertyInfo PropertyInfo { get; }

    public bool IsSealed
      => (bool)(this.isSealed ?? (this.isSealed = this.CanRead ? this.GetMethodData.IsSealed : this.SetMethodData.IsSealed));

    public bool CanWrite 
      => (bool)(this.canWrite ?? (this.canWrite = GetPropertyInfo().CanWrite));

    public bool CanRead 
      => (bool)(this.canRead ?? (this.canRead = GetPropertyInfo().CanRead)); 

    public MethodData GetMethodData 
      => this.getMethodData ?? (this.getMethodData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(GetPropertyInfo().GetMethod));

    public MethodData SetMethodData 
      => this.setMethodData ?? (this.setMethodData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(GetPropertyInfo().SetMethod));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override char[] Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false).ToCharArray());

    public override char[] FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false).ToCharArray());

    public override bool IsStatic 
      => (bool)(this.isStatic ?? (this.isStatic = this.CanRead ? this.GetMethodData.IsStatic : this.SetMethodData.IsStatic));

#if !NETSTANDARD2_0
    public bool IsSetMethodReadOnly
      => (bool)(this.isSetMethodReadOnly ?? (this.isSetMethodReadOnly = this.CanWrite && this.SetMethodData.AttributeData.Any(data => data.AttributeType == typeof(IsReadOnlyAttribute))));
#endif

    public bool IsOverride
      => (bool)(this.isOverride ?? (this.isOverride = this.CanRead ? this.GetMethodData.IsOverride : this.SetMethodData.IsOverride));
  }
}