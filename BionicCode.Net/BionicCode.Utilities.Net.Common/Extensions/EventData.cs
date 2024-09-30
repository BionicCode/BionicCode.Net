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

    public EventData(EventInfo eventInfo) : base(eventInfo)
    {
      this.eventInfo = eventInfo;
    }

    public EventInfo GetEventInfo()
      => this.eventInfo;

    protected override MemberInfo GetMemberInfo() 
      => GetEventInfo();

    public object RaiseEvent(object target, params object[] arguments)
    {
      if (this.invocator is null)
      {
        this.invocator = this.InvocatorMethodData.Invoke;
      }

      return this.invocator.Invoke(target, arguments);
    }

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public MethodData AddMethodData 
      => this.addMethodData ?? (this.addMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(this.GetEventInfo().GetAddMethod(true)));

    public MethodData RemoveMethodData
      => this.removeMethodData ?? (this.removeMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().GetRemoveMethod(true)));

    public MethodData InvocatorMethodData
      => this.invocatorMethodData ?? (this.invocatorMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().GetRaiseMethod(true)));

    public TypeData EventHandlerTypeData
      => this.eventHandlerTypeData ?? (this.eventHandlerTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetEventInfo().EventHandlerType));

    public override bool IsStatic
      => (bool)(this.isStatic ?? (this.isStatic = this.AddMethodData.IsStatic));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: false, isCompact: false));

    public override string ShortSignature
      => this.shortSignature ?? (this.shortSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public override string DisplayName
      => this.displayName ?? (this.displayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: false, isCompact: true));

    public override string ShortDisplayName
      => this.shortDisplayName ?? (this.shortDisplayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: true));

    public override string FullyQualifiedDisplayName 
      => this.fullyQualifiedDisplayName ?? (this.fullyQualifiedDisplayName = HelperExtensionsCommon.ToDisplayNameInternal(this, isFullyQualifiedName: true, isShortName: false, isCompact: true));

    public override string AssemblyName
      => this.assemblyName ?? (this.assemblyName = this.DeclaringTypeData.AssemblyName);

    public bool IsOverride 
      => (bool)(this.isOverride ?? (this.isOverride = this.AddMethodData.IsOverride));
  }
}