namespace BionicCode.Utilities.Net.Reflection;

using System;

public interface IEventDataView : IMemberDataView
{
    IMethodDataView AddMethodData { get; }
    bool CanAdd { get; }
    bool CanRemove { get; }
    ITypeDataView EventHandlerTypeData { get; }
    IMethodDataView EventInvokerMethodData { get; }
    bool IsOverride { get; }
    IMethodDataView RemoveMethodData { get; }

    void AddEventHandler(object eventSource, Delegate handler);
    void AddEventHandler<TEventSource>(TEventSource eventSource, Delegate handler);
    object? RaiseEvent(object? target, params object?[]? arguments);
    void RemoveEventHandler(object eventSource, Delegate handler);
    void RemoveEventHandler<TEventSource>(TEventSource eventSource, Delegate handler);
}