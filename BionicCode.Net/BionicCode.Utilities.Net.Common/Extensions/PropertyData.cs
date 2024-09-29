namespace BionicCode.Utilities.Net
{
  using System;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Threading.Tasks;
  using Microsoft.CodeAnalysis;

  internal sealed class PropertyData : MemberInfoData
  {
    private static PropertyData _TaskResultPropertyData;
    private static PropertyData _ValueTaskResultPropertyData;
    private string displayName;
    private string shortDisplayName;
    private string fullyQualifiedDisplayName;
    private string signature;
    private string shortSignature;
    private string fullyQualifiedSignature;
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
    private Func<object, object[], object> getInvocator;
    private Action<object, object, object[]> setInvocator;
    private string assemblyName;

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

    public object Get(object target, object[] indexerPropertyIndex = null)
    {
      if (this.getInvocator is null)
      {
        InitializeGetInvocator();
      }

      return this.getInvocator.Invoke(target, indexerPropertyIndex);
    }

    public void Set(object target, object value, object[] indexerPropertyIndex = null)
    {
      if (this.setInvocator is null)
      {
        InitializeSetInvocator();
      }

      this.setInvocator.Invoke(target, value, indexerPropertyIndex);
    }

    public Func<object, object[], object> GetGetInvocator()
    {
      if (this.getInvocator is null)
      {
        InitializeGetInvocator();
      }

      return this.getInvocator;
    }

    public Action<object, object, object[]> GetSetInvocator()
    {
      if (this.setInvocator is null)
      {
        InitializeSetInvocator();
      }

      return this.setInvocator;
    }

    private void InitializeGetInvocator()
      => this.getInvocator = (invocationTarget, indexerIndex) => GetPropertyInfo().GetValue(invocationTarget, indexerIndex);

    private void InitializeSetInvocator()
      => this.setInvocator = (invocationTarget, propertyValue, indexerIndex) => GetPropertyInfo().SetValue(invocationTarget, propertyValue, indexerIndex);

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
      => this.indexerParameters ?? (this.indexerParameters = this.PropertyInfo.GetIndexParameters().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

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
      => this.propertyTypeData ?? (this.propertyTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().PropertyType)); 

    public PropertyInfo PropertyInfo { get; }

    public static PropertyData TaskResultPropertyData
      => PropertyData._TaskResultPropertyData ?? (PropertyData._TaskResultPropertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(Task<>).GetProperty(nameof(Task<object>.Result))));

    public static PropertyData ValueTaskResultPropertyData
      => PropertyData._ValueTaskResultPropertyData ?? (PropertyData._ValueTaskResultPropertyData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(ValueTask<>).GetProperty(nameof(ValueTask<object>.Result))));

    public bool IsSealed
      => (bool)(this.isSealed ?? (this.isSealed = this.CanRead ? this.GetMethodData.IsSealed : this.SetMethodData.IsSealed));

    public bool CanWrite 
      => (bool)(this.canWrite ?? (this.canWrite = GetPropertyInfo().CanWrite));

    public bool CanRead 
      => (bool)(this.canRead ?? (this.canRead = GetPropertyInfo().CanRead)); 

    public MethodData GetMethodData 
      => this.getMethodData ?? (this.getMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().GetGetMethod(true)));

    public MethodData SetMethodData 
      => this.setMethodData ?? (this.setMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetPropertyInfo().GetSetMethod(true)));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: false, isCompact: false));

    public override string ShortSignature
      => this.shortSignature ?? (this.shortSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override string DisplayName
      => this.displayName ?? (this.displayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: false));

    public override string ShortDisplayName
      => this.shortDisplayName ?? (this.shortDisplayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: true));

    public override string FullyQualifiedDisplayName 
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = GetPropertyInfo().ToFullDisplayName());

    public override string AssemblyName
      => this.assemblyName ?? (this.assemblyName = this.DeclaringTypeData.AssemblyName);

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