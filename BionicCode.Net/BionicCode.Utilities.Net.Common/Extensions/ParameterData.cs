﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal sealed class ParameterData : SymbolInfoData
  {
    private char[] signature;
    private char[] fullyQualifiedSignature;
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
      => this.parameterTypeData ?? (this.parameterTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetParameterInfo().ParameterType));

    public TypeData DeclaringTypeData
      => this.declaringTypeData ?? (this.declaringTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetDeclaringType()));

    public override HashSet<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new HashSet<CustomAttributeData>(GetParameterInfo().GetCustomAttributesData()));

    public bool IsByRef 
      => (bool)(this.isByRef ?? (this.isByRef = this.ParameterTypeData.GetType().IsByRef));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override char[] Signature
      => this.signature ?? (this.signature = GetType().ToSignatureShortName().ToCharArray());

    public override char[] FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = GetType().ToSignatureShortName(isFullyQualifiedName: true).ToCharArray());
  }
}