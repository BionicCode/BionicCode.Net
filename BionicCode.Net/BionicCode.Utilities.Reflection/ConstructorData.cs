namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

internal sealed class ConstructorData : ParameterizedMemberData
{
    private string? displayName;
    private string? shortDisplayName;
    private string? fullyQualifiedDisplayName;
    private string? signature;
    private string? shortSignature;
    private string? shortCompactSignature;
    private string? fullyQualifiedSignature;
    private string? fullyQualifiedRuntimeSignature;
    private string? runtimeSignature;
    private string? runtimeShortSignature;
    private string? runtimeShortCompactSignature;
    private SymbolAttributes symbolAttributes;
    private AccessModifier accessModifier;
    private ParameterList? parameters;
    private bool? _hasParamsParameter;
    private bool? isStatic;
    private bool? _isPublic;
    private bool? _isPrivate;
    private bool? _isAssembly;
    private bool? _isFamily;
    private bool? _isFamilyOrAssembly;
    private bool? _isFamilyAndAssembly;
    private Func<object?[], object>? invocator;
    private string? assemblyName;
    private SymbolComponentInfo? symbolComponentInfo;

    internal ConstructorData(ConstructorInfo constructorInfo, SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
        : base(constructorInfo, SymbolKind.MemberConstructor, symbolInfoDataCacheKey)
    {
        ArgumentNullException.ThrowIfNull(constructorInfo, nameof(constructorInfo));

        Handle = constructorInfo.MethodHandle;
    }

    internal ConstructorInfo GetConstructorInfo()
      => (ConstructorInfo)MethodInfo.GetMethodFromHandle(Handle, DeclaringTypeHandle)!;

    internal override MethodBase GetMethodBase()
        => GetConstructorInfo();

    protected override MemberInfo GetMemberInfo()
      => GetConstructorInfo();

    internal object Invoke(params object?[] arguments)
    {
        //  TODO::Implement fast invocator pattern
        if (invocator is null)
        {
            InitializeInvocator();
        }

        return invocator.Invoke(arguments);
    }

    internal Func<object[], object> GetInvocator()
    {
        if (invocator is null)
        {
            InitializeInvocator();
        }

        return invocator;
    }

    private void InitializeInvocator()
      => invocator = invocationArguments => GetConstructorInfo().Invoke(invocationArguments);

    internal override RuntimeMethodHandle Handle { get; }
    internal new RuntimeTypeHandle DeclaringTypeHandle { get; }

    internal override AccessModifier AccessModifier => accessModifier is AccessModifier.Undefined
      ? (accessModifier = ConstructorData.GetAccessModifierInternal(this))
      : accessModifier;

    internal override ParameterList Parameters
      => parameters ??= ParameterListBuilder.Create(this);

    internal override bool HasParamsParameter
      => _hasParamsParameter ??= Parameters.HasItems && Parameters[^1].IsParams;

    internal override SymbolAttributes SymbolAttributes => symbolAttributes is SymbolAttributes.Undefined
      ? (symbolAttributes = ConstructorData.GetAttributesInternal(this))
      : symbolAttributes;

    internal override SymbolComponentInfo SymbolComponentInfo
      => symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    internal override string Signature
      => signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortSignature
      => shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortCompactSignature
      => shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    internal override string FullyQualifiedSignature
      => fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string FullyQualifiedRuntimeSignature
      => fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeSignature
      => runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortSignature
      => runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortCompactSignature
      => runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    internal override string DisplayName
      => displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string ShortDisplayName
      => shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    internal override string FullyQualifiedDisplayName
      => fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string AssemblyName
      => assemblyName ??= DeclaringTypeData.AssemblyName;

    internal override bool IsStatic
        => isStatic ??= GetConstructorInfo().IsStatic;

    internal override bool IsPublic
        => _isPublic ??= GetConstructorInfo().IsPublic;

    internal override bool IsPrivate
        => _isPrivate ??= GetConstructorInfo().IsPrivate;

    internal override bool IsAssembly
        => _isAssembly ??= GetConstructorInfo().IsAssembly;

    internal override bool IsFamily
        => _isFamily ??= GetConstructorInfo().IsFamily;

    internal override bool IsFamilyOrAssembly
        => _isFamilyOrAssembly ??= GetConstructorInfo().IsFamilyOrAssembly;

    internal override bool IsFamilyAndAssembly
        => _isFamilyAndAssembly ??= GetConstructorInfo().IsFamilyAndAssembly;

    internal override ParameterizedSymbolKind ParameterizedSymbolKind
      => ParameterizedSymbolKind.MemberConstructor;

    internal override RuntimeTypeHandle DeclaringTypeHandle { get; }
    internal override bool IsExplicitInterfaceImplementation { get; }
    internal override RuntimeTypeHandle ImplementingTypeHandle { get; }

    /// <summary>
    /// Determines the symbol attributes for a constructor based on the specified constructor data.
    /// </summary>
    /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
    /// <param name="constructorData">The data describing the constructor, including whether it is static.</param>
    /// <returns>A combination of symbol attributes representing the constructor's characteristics. Includes the static
    /// attribute if the constructor is static.</returns>
    private static SymbolAttributes GetAttributesInternal(ConstructorData constructorData)
    {
        SymbolAttributes constructorAttributes = SymbolAttributes.Constructor;

        if (constructorData.IsStatic)
        {
            constructorAttributes |= SymbolAttributes.Static;
        }

        return constructorAttributes;
    }

    private static AccessModifier GetAccessModifierInternal(ConstructorData constructorData) => constructorData.IsPublic ? AccessModifier.Public
          : constructorData.IsPrivate ? AccessModifier.Private
          : constructorData.IsAssembly ? AccessModifier.Internal
          : constructorData.IsFamily ? AccessModifier.Protected
          : constructorData.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
          : constructorData.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
          : constructorData.IsStatic ? AccessModifier.Undefined
          : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
}
