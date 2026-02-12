namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;

public class EventDataView : IEventDataView
{
    internal EventDataView(SymbolReflectionInfoCacheKey cacheKey) => CacheKey = cacheKey;

    public AccessModifier AccessModifier => SymbolReflectionInfoCache.GetOrCreateEventDataCacheEntry(CacheKey).AccessModifier;
    public IMethodDataView AddMethodDataView { get; }
    public string AssemblyName { get; }
    public bool CanAdd { get; }
    public bool CanRemove { get; }
    public RuntimeTypeHandle DeclaringInterfaceHandle { get; }
    public RuntimeTypeHandle DeclaringTypeHandle { get; }
    public string DisplayName { get; }
    public ITypeDataView EventHandlerTypeDataView { get; }
    public IMethodDataView EventInvokerMethodDataView { get; }
    public string FullyQualifiedDisplayName { get; }
    public string FullyQualifiedRuntimeSignature { get; }
    public string FullyQualifiedSignature { get; }
    public RuntimeTypeHandle ImplementingTypeHandle { get; }
    public bool IsAssembly { get; }
    public bool IsExplicitInterfaceImplementation { get; }
    public bool IsFamily { get; }
    public bool IsFamilyAndAssembly { get; }
    public bool IsFamilyOrAssembly { get; }
    public bool IsOverride { get; }
    public bool IsPrivate { get; }
    public bool IsPublic { get; }
    public bool IsStatic { get; }
    public IMethodDataView RemoveMethodDataView { get; }
    public string RuntimeShortCompactSignature { get; }
    public string RuntimeShortSignature { get; }
    public string RuntimeSignature { get; }
    public string ShortCompactSignature { get; }
    public string ShortDisplayName { get; }
    public string ShortSignature { get; }
    public string Signature { get; }
    public SymbolComponentInfo SymbolComponentInfo { get; }
    public IList<CustomAttributeData> AttributeData { get; }
    public BindingFlags BindingFlagsVisibilityMask { get; }
    public string Namespace { get; }
    public int FormattingIndentation { get; set; }
    public string IndentationString { get; }
    public string Name { get; }
    public SymbolAttributes SymbolAttributes { get; }
    SymbolAttributes ISymbolInfoDataView.SymbolAttributes { get; }
    private ITypeDataView DeclaringTypeDataView { get; }
    public SymbolReflectionInfoCacheKey CacheKey { get; }
    public SymbolKind SymbolKind { get; }
    public IMethodDataView AddMethodData { get; }
    public ITypeDataView EventHandlerTypeData { get; }
    public IMethodDataView EventInvokerMethodData { get; }
    public IMethodDataView RemoveMethodData { get; }
    public ITypeDataView DeclaringTypData { get; }
    public ITypeDataView ImplementingTypData { get; }

    public void AddEventHandler(object eventSource, Delegate handler) => throw new NotImplementedException();
    public void AddEventHandler<TEventSource>(TEventSource eventSource, Delegate handler) => throw new NotImplementedException();
    public object? RaiseEvent(object? target, params object?[]? arguments) => throw new NotImplementedException();
    public void RemoveEventHandler(object eventSource, Delegate handler) => throw new NotImplementedException();
    public void RemoveEventHandler<TEventSource>(TEventSource eventSource, Delegate handler) => throw new NotImplementedException();
}
