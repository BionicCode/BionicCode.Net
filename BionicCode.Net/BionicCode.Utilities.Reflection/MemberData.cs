namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Reflection;

internal abstract class MemberData : SymbolInfoData
{
    private IList<CustomAttributeData> _attributeData;
    private TypeData _declaringTypeData;
    private string? _namespace;
    private BindingFlags? _bindingFlagsVisibilityMask;

    protected MemberData(string memberName, SymbolKind symbolKind, SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
        : base(memberName, symbolKind, symbolInfoDataCacheKey)
    {
    }

    protected abstract MemberInfo GetMemberInfo();

    private BindingFlags ComputeVisibilityBindingFlagsMask()
    {
        BindingFlags visibilityMask = BindingFlags.Default;
        if (this.IsPublic)
        {
            visibilityMask = BindingFlags.Public;
        }
        else if (this.IsPrivate)
        {
            visibilityMask = BindingFlags.NonPublic;
        }

        if (this.IsStatic)
        {
            visibilityMask |= BindingFlags.Static;
        }
        else
        {
            visibilityMask |= BindingFlags.Instance;
        }

        return visibilityMask;
    }

    public TypeData DeclaringTypeData
      => this._declaringTypeData ??= Type.GetTypeFromHandle(this.DeclaringTypeHandle) is Type declaringType
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(this.DeclaringTypeHandle)}' is not valid.");

    public string Namespace
        => this._namespace ??= this.DeclaringTypeData.Namespace;

    public abstract RuntimeTypeHandle DeclaringTypeHandle { get; }
    public abstract bool IsExplicitInterfaceImplementation { get; }
    public abstract RuntimeTypeHandle DeclaringInterfaceHandle { get; }
    public abstract RuntimeTypeHandle ImplementingTypeHandle { get; }

    public abstract bool IsStatic { get; }
    public abstract bool IsPublic { get; }
    public abstract bool IsPrivate { get; }
    public abstract bool IsAssembly { get; }
    public abstract bool IsFamily { get; }
    public abstract bool IsFamilyOrAssembly { get; }
    public abstract bool IsFamilyAndAssembly { get; }
    public abstract AccessModifier AccessModifier { get; }
    public BindingFlags BindingFlagsVisibilityMask => this._bindingFlagsVisibilityMask ??= ComputeVisibilityBindingFlagsMask();

    public override IList<CustomAttributeData> AttributeData
      => this._attributeData ??= new List<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData());
}
