﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal sealed class ParameterData : SymbolInfoData
  {
    private string displayName;
    private string fullyQualifiedDisplayName;
    private string signature;
    private string fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private HashSet<CustomAttributeData> attributeData;
    private bool? isRef;
    private bool? isByRef;
    private TypeData parameterTypeData;
    private TypeData declaringTypeData;

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

    public TypeData ParameterTypeData
      => this.parameterTypeData ?? (this.parameterTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetParameterInfo().ParameterType));

    public TypeData DeclaringTypeData
      => this.declaringTypeData ?? (this.declaringTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetDeclaringType()));

    public override HashSet<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new HashSet<CustomAttributeData>(GetParameterInfo().GetCustomAttributesData()));

    public bool IsByRef 
      => (bool)(this.isByRef ?? (this.isByRef = this.ParameterTypeData.GetType().IsByRef));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = GetParameterInfo().Name);

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = GetParameterInfo().Name);

    public override string DisplayName
      => this.displayName ?? (this.displayName = GetParameterInfo().Name);

    public override string FullyQualifiedDisplayName 
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = GetParameterInfo().Name);
  }
}