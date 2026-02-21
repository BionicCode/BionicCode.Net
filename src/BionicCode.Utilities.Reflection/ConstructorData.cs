namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

internal sealed class ConstructorData : ParameterizedMemberData
{
    public static string ConstructorDefaultName { get; } = ConstructorInfo.ConstructorName;
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
    private IConstructorDataView? _constructorDataView;
    private readonly WellKnownConstructorDescriptor _descriptor;

    internal ConstructorData(WellKnownConstructorDescriptor descriptor)
        : base(ConstructorDefaultName, SymbolKind.MemberConstructor)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(descriptor);

        _descriptor = descriptor;
        Handle = _descriptor.ConstructorInfo.MethodHandle;
        ConstructorInfo = _descriptor.ConstructorInfo;
        DeclaringTypeHandle = ConstructorInfo.DeclaringType?.TypeHandle ?? throw new InvalidOperationException("Declaring type handle is not available.");
        ImplementingTypeHandle = DeclaringTypeHandle;
        IsExplicitInterfaceImplementation = false;
    }

    internal new IConstructorDataView View => _constructorDataView ??= new ConstructorDataView(GetPublicCacheKey());

    internal ConstructorInfo ConstructorInfo { get; }

    internal override MethodBase MethodBase => ConstructorInfo;

    internal object Invoke(params object?[] arguments)
    {
        //  TODO::Implement fast invocator pattern
        if (_invocator is null)
        {
            InitializeInvocator();
        }

        return _invocator.Invoke(arguments);
    }

    internal Func<object[], object> GetInvocator()
    {
        if (_invocator is null)
        {
            InitializeInvocator();
        }

        return _invocator!;
    }

    private void InitializeInvocator() => _invocator = ConstructorInfo.Invoke;

    internal override RuntimeMethodHandle Handle { get; }

    internal override AccessModifier AccessModifier => _accessModifier is AccessModifier.Undefined
        ? (_accessModifier = ConstructorData.GetAccessModifierInternal(this))
        : _accessModifier;

    internal override ParameterList Parameters => _parameters ??= ParameterListBuilder.Create(this);

    internal override bool HasParamsParameter => _hasParamsParameter ??= Parameters.HasItems && Parameters[^1].IsParams;

    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
        ? (_symbolAttributes = ConstructorData.GetAttributesInternal(this))
        : _symbolAttributes;

    internal override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    internal override string Signature => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortSignature => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortCompactSignature => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    internal override string FullyQualifiedSignature => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string FullyQualifiedRuntimeSignature => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeSignature => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortSignature => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortCompactSignature => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    internal override string DisplayName => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string ShortDisplayName => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    internal override string FullyQualifiedDisplayName => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string AssemblyName => _assemblyName ??= DeclaringTypeData.AssemblyName;

    internal override bool IsStatic => _isStatic ??= ConstructorInfo.IsStatic;

    internal override bool IsPublic => _isPublic ??= ConstructorInfo.IsPublic;

    internal override bool IsPrivate => _isPrivate ??= ConstructorInfo.IsPrivate;

    internal override bool IsAssembly => _isAssembly ??= ConstructorInfo.IsAssembly;

    internal override bool IsFamily => _isFamily ??= ConstructorInfo.IsFamily;

    internal override bool IsFamilyOrAssembly => _isFamilyOrAssembly ??= ConstructorInfo.IsFamilyOrAssembly;

    internal override bool IsFamilyAndAssembly => _isFamilyAndAssembly ??= ConstructorInfo.IsFamilyAndAssembly;

    internal override ParameterizedSymbolKind ParameterizedSymbolKind => ParameterizedSymbolKind.MemberConstructor;

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
