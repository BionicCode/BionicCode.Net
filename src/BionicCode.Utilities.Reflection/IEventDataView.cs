namespace BionicCode.Utilities.Net.Reflection;

using System;

public interface IEventDataView : IMemberDataView, ISymbolInfoDataView
{
    bool CanAdd { get; }
    bool CanRemove { get; }
    ITypeDataView EventHandlerType { get; }
    IMethodDataView EventInvokerMethod { get; }
    bool IsOverride { get; }
    IMethodDataView AddMethod { get; }
    IMethodDataView RemoveMethod { get; }

    void AddEventHandler(object eventSource, Delegate handler);
    void AddEventHandler<TEventSource>(TEventSource eventSource, Delegate handler);
    object? RaiseEvent(object? target, params object?[]? arguments);
    void RemoveEventHandler(object eventSource, Delegate handler);
    void RemoveEventHandler<TEventSource>(TEventSource eventSource, Delegate handler);
}