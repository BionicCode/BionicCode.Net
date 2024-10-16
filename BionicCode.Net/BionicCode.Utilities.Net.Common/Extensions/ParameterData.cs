﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal sealed class ParameterData : SymbolInfoData
  {
    private SymbolAttributes symbolAttributes;
    private IList<CustomAttributeData> attributeData;
    private bool? isRef;
    private bool? isByRef;
    private TypeData parameterTypeData;
    private TypeData declaringTypeData;
    private MemberInfoData member;
    private string assemblyName;
    private SymbolComponentInfo symbolComponentInfo;

    public ParameterData(ParameterInfo parameterInfo) : base(parameterInfo.Name)
    {
      this.DeclaringTypeHandle = parameterInfo.Member.DeclaringType.TypeHandle;
      this.ParameterInfo = parameterInfo;
    }

    public ParameterInfo GetParameterInfo()
      => this.ParameterInfo;

    public Type GetDeclaringType()
      => Type.GetTypeFromHandle(this.DeclaringTypeHandle);

    public RuntimeTypeHandle DeclaringTypeHandle { get; set; }

    public bool IsRef 
      => (bool)(this.isRef ?? (this.isRef = HelperExtensionsCommon.IsRefInternal(this)));

    public bool IsIn
      => GetParameterInfo().IsIn;

    public bool IsOut
      => GetParameterInfo().IsOut;

    public bool IsOptional
      => GetParameterInfo().IsOptional;

    public ParameterInfo ParameterInfo { get; }

    public MemberInfoData Member
    {
      get
      {
        MemberInfo member = GetParameterInfo().Member;
        if (this.member is null)
        {
          if (member is ConstructorInfo constructorInfo)
          {
            this.member = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(constructorInfo);
          }
          else if (member is PropertyInfo propertyInfo)
          {
            this.member = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(propertyInfo);
          }
          else if (member is MethodInfo methodInfo)
          {
            this.member = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(methodInfo);
          }
          else
          {
            throw new NotImplementedException();
          }
        }

        return this.member;
      }
    }

    public TypeData ParameterTypeData
      => this.parameterTypeData ?? (this.parameterTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetParameterInfo().ParameterType));

    public TypeData DeclaringTypeData
      => this.declaringTypeData ?? (this.declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetDeclaringType()));

    public override IList<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new List<CustomAttributeData>(GetParameterInfo().GetCustomAttributesData()));

    public bool IsByRef 
      => (bool)(this.isByRef ?? (this.isByRef = this.ParameterTypeData.GetType().IsByRef));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override SymbolComponentInfo SymbolComponentInfo
      => this.symbolComponentInfo ?? (this.symbolComponentInfo = HelperExtensionsCommon.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false));

    public override string Signature
      => this.Name;

    public override string ShortSignature
      => this.Name;

    public override string ShortCompactSignature
      => this.Name;

    public override string FullyQualifiedSignature
      => this.Name;

    public override string DisplayName
      => this.Name ;

    public override string ShortDisplayName
      => this.Name;

    public override string FullyQualifiedDisplayName 
      => this.Name;

    public override string AssemblyName
      => this.assemblyName ?? (this.assemblyName = this.Member.AssemblyName);
  }
}