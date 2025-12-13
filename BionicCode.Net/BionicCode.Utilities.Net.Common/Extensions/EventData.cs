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
        private string shortCompactSignature;
        private string fullyQualifiedSignature;
        private string fullyQualifiedRuntimeSignature;
        private string runtimeSignature;
        private string runtimeShortSignature;
        private string runtimeShortCompactSignature;
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
          ? (this.accessModifier = EventData.GetAccessModifierInternal(this))
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
          => this.invocatorMethodData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().GetRaiseMethod(true) ?? GetEventInfo().EventHandlerType.GetMethod(HelperExtensionsCommon.DelegateInvocatorMethodName));

        public TypeData EventHandlerTypeData
          => this.eventHandlerTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().EventHandlerType);

        public override bool IsStatic
          => this.isStatic ??= this.AddMethodData.IsStatic;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = EventData.GetAttributesInternal(this))
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

        public bool IsOverride
          => this.isOverride ??= this.AddMethodData.IsOverride;

        /// <summary>
        /// Determines the set of symbol attributes for the specified event based on its add method characteristics.
        /// </summary>
        /// <param name="eventData">The event metadata used to evaluate and derive the corresponding symbol attributes.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that represent the attributes of the event, such as Final,
        /// Abstract, Static, Virtual, or Override.</returns>
        private static SymbolAttributes GetAttributesInternal(EventData eventData)
        {
            MethodData eventAddMethodData = eventData.AddMethodData;
            SymbolAttributes eventAttributes = SymbolAttributes.Event;
            MethodInfo addHandlerMethod = eventAddMethodData.GetMethodInfo();
            if (addHandlerMethod.IsFinal)
            {
                eventAttributes |= SymbolAttributes.Final;
            }

            if (addHandlerMethod.IsAbstract)
            {
                eventAttributes |= SymbolAttributes.Abstract;
            }

            if (eventAddMethodData.IsStatic)
            {
                eventAttributes |= SymbolAttributes.Static;
            }

            if (addHandlerMethod.IsVirtual)
            {
                eventAttributes |= SymbolAttributes.Virtual;
            }

            if (eventAddMethodData.IsOverride)
            {
                eventAttributes |= SymbolAttributes.Override;
            }

            return eventAttributes;
        }

        private static AccessModifier GetAccessModifierInternal(EventData eventData)
          => eventData.AddMethodData.AccessModifier;
    }
}
