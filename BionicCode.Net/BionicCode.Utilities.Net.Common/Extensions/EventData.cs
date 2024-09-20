namespace BionicCode.Utilities.Net
{
  using System.Reflection;

  internal sealed class EventData : MemberInfoData
  {
    private char[] signature;
    private char[] fullyQualifiedSignature;
    private SymbolAttributes symbolAttributes;
    private readonly EventInfo eventInfo;
    private bool? isOverride;
    private MethodData addMethodData;
    private MethodData removeMethodData;
    private AccessModifier accessModifier;
    private bool? isStatic;
    private TypeData eventHandlerTypeData;

    public EventData(EventInfo eventInfo) : base(eventInfo)
    {
      this.eventInfo = eventInfo;
    }

    public EventInfo GetEventInfo()
      => this.eventInfo;

    protected override MemberInfo GetMemberInfo() 
      => GetEventInfo();

    public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
      ? (this.accessModifier = HelperExtensionsCommon.GetAccessModifierInternal(this))
      : this.accessModifier;

    public MethodData AddMethodData 
      => this.addMethodData ?? (this.addMethodData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(this.GetEventInfo().AddMethod));

    public MethodData RemoveMethodData
      => this.removeMethodData ?? (this.removeMethodData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(GetEventInfo().RemoveMethod));

    public TypeData EventHandlerTypeData
      => this.eventHandlerTypeData ?? (this.eventHandlerTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetEventInfo().EventHandlerType));

    public override bool IsStatic
      => (bool)(this.isStatic ?? (this.isStatic = this.AddMethodData.IsStatic));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override char[] Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false).ToCharArray());

    public override char[] FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false).ToCharArray());

    public bool IsOverride 
      => (bool)(this.isOverride ?? (this.isOverride = this.AddMethodData.IsOverride));
  }
}