﻿namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal abstract class MemberInfoData : SymbolInfoData
  {
    private HashSet<CustomAttributeData> attributeData;
    private TypeData declaringTypeData;

    protected MemberInfoData(MemberInfo memberInfo) : base(memberInfo.Name)
    {
      this.DeclaringTypeHandle = memberInfo.DeclaringType.TypeHandle;
      this.Namespace = memberInfo.DeclaringType.Namespace.ToCharArray();
    }

    public Type GetDeclaringType()
      => Type.GetTypeFromHandle(this.DeclaringTypeHandle);

    protected abstract MemberInfo GetMemberInfo();
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public abstract bool IsStatic { get; }
    public abstract AccessModifier AccessModifier { get; }
    public char[] Namespace { get; }

    public override HashSet<CustomAttributeData> AttributeData
      => this.attributeData ?? (this.attributeData = new HashSet<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData()));

    public TypeData DeclaringTypeData
      => this.declaringTypeData ?? (this.declaringTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetDeclaringType()));
  }
}