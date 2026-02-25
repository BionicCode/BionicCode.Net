namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

internal abstract class MemberData : SymbolInfoData
{
    private ImmutableList<CustomAttributeData>? _attributeData;
    private TypeData? _declaringTypeData;
    private string? _namespace;
    private BindingFlags? _bindingFlagsVisibilityMask;
    private TypeData? _implementingTypeData;
    private IMemberDataView? _view;

    protected MemberData(string memberName, SymbolKind symbolKind)
        : base(memberName, symbolKind)
    {
    }

    internal new IMemberDataView View => _view ??= new MemberDataView(GetPublicCacheKey());

    protected abstract MemberInfo MemberInfo { get; }

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
            ? GetOrCreateCacheEntry(declaringType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(DeclaringTypeHandle)}' is not valid.");

    internal TypeData ImplementingTypeData
        => _implementingTypeData ??= Type.GetTypeFromHandle(ImplementingTypeHandle) is Type implementingType
            ? GetOrCreateCacheEntry(implementingType)
            : throw new InvalidOperationException($"The runtime type handle returned from the property '{nameof(ImplementingTypeHandle)}' is not valid.");

    internal override string Namespace
        => _namespace ??= DeclaringTypeData!.Namespace;

    internal override bool IsDefined(Type attributeType, bool inherit = false) => MemberInfo.IsDefined(attributeType, inherit);
    internal override bool IsDefined<TAttribute>(bool inherit = false) => MemberInfo.IsDefined(typeof(TAttribute), inherit);

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
    /// Gets a value indicating whether the member has <see langword="internal"> accessibility within its assembly.
    /// </summary>
    internal abstract bool IsAssembly { get; }
    /// <summary>
    /// Gets a value indicating whether the member is <see langword="protected"> and thus accessible only within its own class or by
    /// derived class instances.
    /// </summary>
    internal abstract bool IsFamily { get; }

    /// <summary>
    /// Gets a value indicating whether the member is <see langword="protected"/> <see langword="public"/> (accessible either by derived classes or by any code within the
    /// same assembly).
    /// </summary>
    /// <remarks>Use this property to determine if the member has 'protected internal' accessibility, meaning
    /// it can be accessed from derived types regardless of assembly, as well as from any code within the same
    /// assembly.</remarks>
    internal abstract bool IsFamilyOrAssembly { get; }

    /// <summary>
    /// Gets a value indicating whether the member is <see langword="private"/> <see langword="protected"/> (accessible only to derived classes within the same assembly).
    /// </summary>
    /// <remarks>Use this property to determine if the member has 'family and assembly' accessibility, meaning
    /// it is accessible to types that derive from the declaring type, but only if those types are also in the same
    /// assembly. This access level is more restrictive than 'protected internal' and is relevant when reflecting over
    /// member visibility in inheritance scenarios.</remarks>
    internal abstract bool IsFamilyAndAssembly { get; }
    internal abstract AccessModifier AccessModifier { get; }

    /// <summary>
    /// Gets the binding flags that determine the visibility of members during reflection operations.
    /// </summary>
    /// <remarks>This property computes the visibility binding flags mask based on the current context, which
    /// can affect how members are accessed through reflection. It is important to note that the visibility mask may
    /// change depending on the context in which it is used.</remarks>
    /// <value>The <see cref="BindingFlags"> that determine the visibility of members during reflection operations.</value>
    internal BindingFlags BindingFlagsVisibilityMask => _bindingFlagsVisibilityMask ??= ComputeVisibilityBindingFlagsMask();

    /// <inheritdoc/>
    internal override ImmutableList<CustomAttributeData> AttributeData => _attributeData ??= [.. MemberInfo.GetCustomAttributesData()];
}
