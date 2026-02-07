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
    private TypeData? _implementingTypeData;

    protected MemberData(string memberName, SymbolKind symbolKind, SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
        : base(memberName, symbolKind, symbolInfoDataCacheKey)
    {
    }

    protected abstract MemberInfo GetMemberInfo();

    private BindingFlags ComputeVisibilityBindingFlagsMask()
    {
        BindingFlags visibilityMask = BindingFlags.Default;
        if (IsPublic)
        {
            visibilityMask = BindingFlags.Public;
        }
        else if (IsPrivate)
        {
            visibilityMask = BindingFlags.NonPublic;
        }

        if (IsStatic)
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
      => _declaringTypeData ??= Type.GetTypeFromHandle(DeclaringTypeHandle) is Type declaringType
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(DeclaringTypeHandle)}' is not valid.");

    public TypeData ImplementingTypeData
      => _implementingTypeData ??= Type.GetTypeFromHandle(ImplementingTypeHandle) is Type implementingType
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(implementingType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(ImplementingTypeHandle)}' is not valid.");

    public override string Namespace
        => _namespace ??= DeclaringTypeData!.Namespace;

    /// <summary>
    /// The declaring type handle of the member. This is the runtime type handle of the type that declares the member. For example, for a method declared in a class, this would be the runtime type handle of that class. 
    /// <para/>For an explicit interface implementation, this would be the runtime type handle of the interface that declares the member.
    /// </summary>
    /// <value>
    /// The runtime type handle of the type that declares the member.
    /// <para/> For an explicit interface implementation, this would be the runtime type handle of the interface that declares the member.
    /// </value>
    public abstract RuntimeTypeHandle DeclaringTypeHandle { get; }
    public abstract bool IsExplicitInterfaceImplementation { get; }

    /// <summary>
    /// The implementing type handle of the member. This is the runtime type handle of the type that implements the member. For example, for a method declared in a class, this would be the runtime type handle of that class.
    /// </summary>
    /// <remarks>
    /// For an explicit interface implementation, this would be the runtime type handle of the interface that implements the member.
    /// <br/> For a non-explicit interface implementation, this would be the same as the declaring type handle.
    /// </remarks>
    /// <value>
    /// The runtime type handle of the type that implements the member.
    /// <para/> For an explicit interface implementation, this would be the runtime type handle of the interface that implements the member.
    /// <para/> For a non-explicit interface implementation, this would be the same as the declaring type handle.
    /// </value>
    public abstract RuntimeTypeHandle ImplementingTypeHandle { get; }

    public abstract bool IsStatic { get; }
    public abstract bool IsPublic { get; }
    public abstract bool IsPrivate { get; }
    /// <summary>
    /// Gets a value indicating whether the member has internal accessibility within its assembly.
    /// </summary>
    public abstract bool IsAssembly { get; }
    /// <summary>
    /// Gets a value indicating whether the member is protected and thus accessible only within its own class or by
    /// derived class instances.
    /// </summary>
    public abstract bool IsFamily { get; }
    public abstract bool IsFamilyOrAssembly { get; }
    public abstract bool IsFamilyAndAssembly { get; }
    public abstract AccessModifier AccessModifier { get; }
    public BindingFlags BindingFlagsVisibilityMask => _bindingFlagsVisibilityMask ??= ComputeVisibilityBindingFlagsMask();

    /// <inheritdoc/>
    public override IList<CustomAttributeData> AttributeData
      => _attributeData ??= new List<CustomAttributeData>(GetMemberInfo().GetCustomAttributesData());
}
