namespace BionicCode.Utilities.Net
{
    using System;
    using System.Reflection;

    internal sealed class EventData : MemberInfoData
    {
        private string displayName;
        private string shortDisplayName;
        private string fullyQualifiedDisplayName;
        private string signature;
        private string shortSignature;
        private string runtimeShortSignature;
        private string shortCompactSignature;
        private string fullyQualifiedSignature;
        private SymbolAttributes symbolAttributes;
        private readonly EventInfo eventInfo;
        private bool? isOverride;
        private MethodData addMethodData;
        private MethodData removeMethodData;
        private MethodData invocatorMethodData;
        private AccessModifier accessModifier;
        private bool? isStatic;
        private TypeData eventHandlerTypeData;
        private Func<object, object[], object> invocator;
        private string assemblyName;
        private SymbolComponentInfo symbolComponentInfo;

        public EventData(EventInfo eventInfo) : base(eventInfo) => this.eventInfo = eventInfo;

        public EventInfo GetEventInfo()
          => this.eventInfo;

        protected override MemberInfo GetMemberInfo()
          => GetEventInfo();

        public object RaiseEvent(object target, params object[] arguments)
        {
            this.invocator ??= this.InvocatorMethodData.Invoke;

            return this.invocator.Invoke(target, arguments);
        }

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
          : this.accessModifier;

        public void AddEventHandler(object eventSource, Delegate handler)
          => GetEventInfo().AddEventHandler(eventSource, handler);

        public void RemoveEventHandler(object eventSource, Delegate handler)
          => GetEventInfo().RemoveEventHandler(eventSource, handler);

        public MethodData AddMethodData
          => this.addMethodData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().GetAddMethod(true));

        public MethodData RemoveMethodData
          => this.removeMethodData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().GetRemoveMethod(true));

        public MethodData InvocatorMethodData
          => this.invocatorMethodData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().GetRaiseMethod(true) ?? GetEventInfo().EventHandlerType.GetMethod("Invoke"));

        public TypeData EventHandlerTypeData
          => this.eventHandlerTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().EventHandlerType);

        public override bool IsStatic
          => (bool)((bool?)(this.isStatic ??= this.AddMethodData.IsStatic));

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
          : this.symbolAttributes;

        public override SymbolComponentInfo SymbolComponentInfo
          => this.symbolComponentInfo ??= HelperExtensionsCommon.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

        public override string Signature
          => this.signature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string ShortSignature
          => this.shortSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

        public override string ShortCompactSignature
          => this.shortCompactSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

        public override string RuntimeShortSignature
          => this.runtimeShortSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

        public override string FullyQualifiedSignature
          => this.fullyQualifiedSignature ??= HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

        public override string DisplayName
          => this.displayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string ShortDisplayName
          => this.shortDisplayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string FullyQualifiedDisplayName
          => this.fullyQualifiedDisplayName ??= HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public bool IsOverride
          => (bool)((bool?)(this.isOverride ??= this.AddMethodData.IsOverride));
    }
}
