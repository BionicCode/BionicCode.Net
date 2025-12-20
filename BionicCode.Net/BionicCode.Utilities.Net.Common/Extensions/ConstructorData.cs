namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    internal sealed class ConstructorData : MemberInfoData
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
        private bool? isStatic;
        private Func<object[], object>? invocator;
        private string? assemblyName;
        private SymbolComponentInfo? symbolComponentInfo;

        public ConstructorData(ConstructorInfo constructorInfo) : base(constructorInfo)
        {
            ArgumentNullExceptionEx.ThrowIfNull(constructorInfo, nameof(constructorInfo));

            this.Handle = constructorInfo.MethodHandle;
        }

        public ConstructorInfo GetConstructorInfo()
          => (ConstructorInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle)!;

        protected override MemberInfo GetMemberInfo()
          => GetConstructorInfo();

        public object Invoke(params object[] arguments)
        {
            //  TODO::Implement fast invocator pattern
            if (this.invocator is null)
            {
                InitializeInvocator();
            }

            return this.invocator.Invoke(arguments);
        }

        public Func<object[], object> GetInvocator()
        {
            if (this.invocator is null)
            {
                InitializeInvocator();
            }

            return this.invocator;
        }

        private void InitializeInvocator()
          => this.invocator = invocationArguments => GetConstructorInfo().Invoke(invocationArguments);

        public RuntimeMethodHandle Handle { get; set; }
        public new RuntimeTypeHandle DeclaringTypeHandle { get; set; }

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = ConstructorData.GetAccessModifierInternal(this))
          : this.accessModifier;

        public ParameterList Parameters
          => this.parameters ??= ParameterListBuilder.Create(GetConstructorInfo().GetParameters());

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = ConstructorData.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string FullyQualifiedRuntimeSignature
          => this.fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeSignature
          => this.runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string RuntimeShortCompactSignature
          => this.runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

        public override string DisplayName
          => this.displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public override bool IsStatic => this.isStatic ??= GetConstructorInfo().IsStatic;

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

        private static AccessModifier GetAccessModifierInternal(ConstructorData constructorData)
        {
            return constructorData.IsPublic ? AccessModifier.Public
              : constructorData.IsPrivate ? AccessModifier.Private
              : constructorData.IsAssembly ? AccessModifier.Internal
              : constructorData.IsFamily ? AccessModifier.Protected
              : constructorData.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
              : constructorData.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
              : constructorData.IsStatic ? AccessModifier.Undefined
              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        }
    }
}
