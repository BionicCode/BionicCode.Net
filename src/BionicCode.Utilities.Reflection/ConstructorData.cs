namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

internal sealed class ConstructorData : ParameterizedMemberData
{
    private string? _displayName;
    private string? _shortDisplayName;
    private string? _fullyQualifiedDisplayName;
    private string? _signature;
    private string? _shortSignature;
    private string? _shortCompactSignature;
    private string? _fullyQualifiedSignature;
    private string? _fullyQualifiedRuntimeSignature;
    private string? _runtimeSignature;
    private string? _runtimeShortSignature;
    private string? _runtimeShortCompactSignature;
    private SymbolAttributes _symbolAttributes;
    private AccessModifier _accessModifier;
    private ParameterList? _parameters;
    private bool? _hasParamsParameter;
    private bool? _isStatic;
    private bool? _isPublic;
    private bool? _isPrivate;
    private bool? _isAssembly;
    private bool? _isFamily;
    private bool? _isFamilyOrAssembly;
    private bool? _isFamilyAndAssembly;
    private Func<object?[], object>? _invocator;
    private string? _assemblyName;
    private SymbolComponentInfo? _symbolComponentInfo;

    internal ConstructorData(SymbolReflectionInfoCacheKey symbolInfoDataCacheKey)
        : base(symbolInfoDataCacheKey)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(symbolInfoDataCacheKey);

        Handle = symbolInfoDataCacheKey.ConstructorDescriptor.ConstructorInfo.MethodHandle;
        ConstructorInfo = symbolInfoDataCacheKey.ConstructorDescriptor.ConstructorInfo;
    }

    public ConstructorInfo ConstructorInfo { get; }

    public override MethodBase GetMethodBase()
        => ConstructorInfo;

    protected override MemberInfo GetMemberInfo()
      => ConstructorInfo;

    public object Invoke(params object?[] arguments)
    {
        //  TODO::Implement fast invocator pattern
        if (_invocator is null)
        {
            InitializeInvocator();
        }

        return _invocator.Invoke(arguments);
    }

    public Func<object[], object> GetInvocator()
    {
        if (_invocator is null)
        {
            InitializeInvocator();
        }

        return _invocator!;
    }

    private void InitializeInvocator()
      => _invocator = ConstructorInfo.Invoke;

    public override RuntimeMethodHandle Handle { get; }

    public override AccessModifier AccessModifier => _accessModifier is AccessModifier.Undefined
      ? (_accessModifier = ConstructorData.GetAccessModifierInternal(this))
      : _accessModifier;

    public override ParameterList Parameters
      => _parameters ??= ParameterListBuilder.Create(this);

    public override bool HasParamsParameter
      => _hasParamsParameter ??= Parameters.HasItems && Parameters[^1].IsParams;

    public override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
      ? (_symbolAttributes = ConstructorData.GetAttributesInternal(this))
      : _symbolAttributes;

    public override SymbolComponentInfo SymbolComponentInfo
      => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    public override string Signature
      => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    public override string ShortSignature
      => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    public override string ShortCompactSignature
      => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    public override string FullyQualifiedSignature
      => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    public override string FullyQualifiedRuntimeSignature
      => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    public override string RuntimeSignature
      => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    public override string RuntimeShortSignature
      => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    public override string RuntimeShortCompactSignature
      => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    public override string DisplayName
      => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    public override string ShortDisplayName
      => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    public override string FullyQualifiedDisplayName
      => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    public override string AssemblyName
      => _assemblyName ??= DeclaringTypeData.AssemblyName;

    public override bool IsStatic
        => _isStatic ??= ConstructorInfo.IsStatic;

    public override bool IsPublic
        => _isPublic ??= ConstructorInfo.IsPublic;

    public override bool IsPrivate
        => _isPrivate ??= ConstructorInfo.IsPrivate;

    public override bool IsAssembly
        => _isAssembly ??= ConstructorInfo.IsAssembly;

    public override bool IsFamily
        => _isFamily ??= ConstructorInfo.IsFamily;

    public override bool IsFamilyOrAssembly
        => _isFamilyOrAssembly ??= ConstructorInfo.IsFamilyOrAssembly;

    public override bool IsFamilyAndAssembly
        => _isFamilyAndAssembly ??= ConstructorInfo.IsFamilyAndAssembly;

    public override ParameterizedSymbolKind ParameterizedSymbolKind
      => ParameterizedSymbolKind.MemberConstructor;

    public override RuntimeTypeHandle DeclaringTypeHandle { get; }
    public override bool IsExplicitInterfaceImplementation { get; }
    public override RuntimeTypeHandle ImplementingTypeHandle { get; }

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
