﻿namespace BionicCode.Utilities.Net
{
  using System.Reflection;

  internal sealed class EventData : MemberInfoData
  {
    private string signature;
    private string fullyQualifiedSignature;
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

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      => this.addMethodData ?? (this.addMethodData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(this.GetEventInfo().AddMethod));
After:
      => this.addMethodData ?? (this.addMethodData = Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<MethodData>(this.GetEventInfo().AddMethod));
*/
      => this.addMethodData ?? (this.addMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<MethodData>(this.GetEventInfo().AddMethod));

    public MethodData RemoveMethodData

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      => this.removeMethodData ?? (this.removeMethodData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<MethodData>(GetEventInfo().RemoveMethod));
After:
      => this.removeMethodData ?? (this.removeMethodData = Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<MethodData>(GetEventInfo().RemoveMethod));
*/
      => this.removeMethodData ?? (this.removeMethodData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<MethodData>(GetEventInfo().RemoveMethod));

    public TypeData EventHandlerTypeData

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net48)'
Before:
      => this.eventHandlerTypeData ?? (this.eventHandlerTypeData = HelperExtensionsCommon.GetSymbolInfoDataCacheEntry<TypeData>(GetEventInfo().EventHandlerType));
After:
      => this.eventHandlerTypeData ?? (this.eventHandlerTypeData = Net.SymbolReflectionInfoCache.GetSymbolInfoDataCacheEntry<TypeData>(GetEventInfo().EventHandlerType));
*/
      => this.eventHandlerTypeData ?? (this.eventHandlerTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry<TypeData>(GetEventInfo().EventHandlerType));

    public override bool IsStatic
      => (bool)(this.isStatic ?? (this.isStatic = this.AddMethodData.IsStatic));

    public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
      ? (this.symbolAttributes = HelperExtensionsCommon.GetAttributesInternal(this))
      : this.symbolAttributes;

    public override string Signature
      => this.signature ?? (this.signature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: false, isShortName: true, isCompact: false));

    public override string FullyQualifiedSignature
      => this.fullyQualifiedSignature ?? (this.fullyQualifiedSignature = HelperExtensionsCommon.ToSignatureNameInternal(this, isFullyQualifiedName: true, isShortName: true, isCompact: false));

    public bool IsOverride 
      => (bool)(this.isOverride ?? (this.isOverride = this.AddMethodData.IsOverride));
  }
}