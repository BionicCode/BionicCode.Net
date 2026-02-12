namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Reflection;

internal abstract class MemberData : SymbolInfoData
{
    private IList<CustomAttributeData>? _attributeData;
    private TypeData? _declaringTypeData;
    private string? _namespace;
    private BindingFlags? _bindingFlagsVisibilityMask;
    private TypeData? _implementingTypeData;

    protected MemberData(string memberName, SymbolKind symbolKind, SymbolReflectionInfoCacheKeyInternal symbolInfoDataCacheKey)
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

    internal TypeData DeclaringTypeData
      => _declaringTypeData ??= Type.GetTypeFromHandle(DeclaringTypeHandle) is Type declaringType
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(declaringType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(DeclaringTypeHandle)}' is not valid.");

    internal TypeData ImplementingTypeData
        => _implementingTypeData ??= Type.GetTypeFromHandle(ImplementingTypeHandle) is Type implementingType
            ? SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(implementingType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(ImplementingTypeHandle)}' is not valid.");

    internal override string Namespace
        => _namespace ??= DeclaringTypeData!.Namespace;

    /// <summary>
    /// The declaring type handle of the member. This is the runtime type handle of the type that declares the member. For example, for a method declared in a class, this would be the runtime type handle of that class. 
    /// <para/>For an explicit interface implementation, this would be the runtime type handle of the interface that declares the member.
    /// </summary>
    /// <value>
    /// The runtime type handle of the type that declares the member.
    /// <para/> For an explicit interface implementation, this would be the runtime type handle of the interface that declares the member.
    /// </value>
    internal abstract RuntimeTypeHandle DeclaringTypeHandle { get; }
    internal abstract bool IsExplicitInterfaceImplementation { get; }

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
    internal abstract RuntimeTypeHandle ImplementingTypeHandle { get; }

    internal abstract bool IsStatic { get; }
    internal abstract bool IsPublic { get; }
    internal abstract bool IsPrivate { get; }
    /// <summary>
    /// Gets a value indicating whether the member has internal accessibility within its assembly.
    /// </summary>
    internal abstract bool IsAssembly { get; }
    /// <summary>
    /// Gets a value indicating whether the member is protected and thus accessible only within its own class or by
    /// derived class instances.
    /// </summary>
    internal abstract bool IsFamily { get; }
    internal abstract bool IsFamilyOrAssembly { get; }
    internal abstract bool IsFamilyAndAssembly { get; }
    internal abstract AccessModifier AccessModifier { get; }
    internal BindingFlags BindingFlagsVisibilityMask => _bindingFlagsVisibilityMask ??= ComputeVisibilityBindingFlagsMask();

    /// <inheritdoc/>
    internal override IList<CustomAttributeData> AttributeData => _attributeData ??= [.. GetMemberInfo().GetCustomAttributesData()];
}
