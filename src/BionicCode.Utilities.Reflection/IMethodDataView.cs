namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Reflection;
using System.Threading.Tasks;

public interface IMethodDataView : IParameterizedMemberDataView, IMemberDataView, ISymbolInfoDataView
{
    IMemberDataView AccessedMember { get; }
    bool ContainsGenericParameters { get; }
    IMethodDataView GenericMethodDefinitionData { get; }
    ITypeListView GenericMethodParameters { get; }
    bool IsAccessorMethod { get; }
    bool IsAsync { get; }
    bool IsAwaitable { get; }
    bool IsAwaitableGenericTask { get; }
    bool IsAwaitableGenericValueTask { get; }
    bool IsAwaitableTask { get; }
    bool IsAwaitableValueTask { get; }
    bool IsDelegateBeginInvokeMethod { get; }
    bool IsDelegateEndInvokeMethod { get; }
    bool IsDelegateInvokeMethod { get; }
    bool IsDelegateMethod { get; }
    bool IsEventAccessorMethod { get; }
    bool IsEventAddMethod { get; }
    bool IsEventRemoveMethod { get; }
    bool IsExtensionMethod { get; }
    bool IsGenericMethod { get; }
    bool IsGenericMethodDefinition { get; }
    bool IsIndexerPropertyGetMethod { get; }
    bool IsIndexerPropertySetMethod { get; }
    bool IsOpenGenericMethodOrGenericMethodDefinition { get; }
    bool IsOperatorOverload { get; }
    bool IsOverride { get; }
    bool IsPropertyAccessorMethod { get; }
    bool IsPropertyGetMethod { get; }
    bool IsPropertySetMethod { get; }
    bool IsReturnValueByRef { get; }
    bool IsReturnValueReadOnly { get; }
    bool IsVoidMethod { get; }
    ITypeDataView ReturnTypeData { get; }
    IParameterDataView ReturnParameterData { get; }
    bool EqualsBySignature(IMethodDataView other);
    object? Invoke(object? target, params object?[]? args);
    object? Invoke(object? target, ReadOnlySpan<object?> args);
    TResult Invoke<TTarget, TResult>(TTarget target, params object?[]? args);
    TResult Invoke<TTarget, TResult>(TTarget target, ReadOnlySpan<object?> args);
    void Invoke<TTarget>(TTarget target, params object?[]? args);
    void Invoke<TTarget>(TTarget target, ReadOnlySpan<object?> args);
    Task InvokeAwaitableTaskAndDiscardResultAsync<TTarget>(TTarget target, params object?[] args);
    Task InvokeAwaitableTaskAsync(object? target, params object?[] args);
    Task<object?> InvokeAwaitableTaskWithResultAsync(object? target, params object?[]? args);
    ValueTask InvokeAwaitableValueTaskAndDiscardResultAsync<TTarget>(TTarget target, params object?[] args);
    ValueTask InvokeAwaitableValueTaskAsync(object? target, params object?[]? args);
    ValueTask<object?> InvokeAwaitableValueTaskWithResultAsync(object? target, params object?[] args);
    object? InvokeOpenGeneric(object? target, ITypeListView genericMethodParameters, params object?[]? args);
    object? InvokeOpenGeneric(object? target, ITypeListView genericMethodParameters, ReadOnlySpan<object?> args);
    Task InvokeOpenGenericAwaitableTaskAsync(object? target, ITypeListView genericMethodParameters, params object?[]? args);
    Task<object?> InvokeOpenGenericAwaitableTaskWithResultAsync(object? target, ITypeListView genericMethodParameters, params object?[]? args);
    ValueTask InvokeOpenGenericAwaitableValueTaskAsync(object? target, ITypeListView genericMethodParameters, params object?[]? args);
    ValueTask<object?> InvokeOpenGenericValueAwaitableTaskWithResultAsync(object? targetView, ITypeListView genericMethodParameters, params object?[] args);
    IMethodDataView MakeGenericMethodData(params Type[] typeArguments);
    IMethodDataView MakeGenericMethodData(ITypeListView typeDataArguments);
    MethodInfo MakeGenericMethodInfo(params Type[] typeArguments);
    MethodInfo MakeGenericMethodInfo(params ITypeDataView[] typeArguments);
    MethodInfo MakeGenericMethodInfo(ITypeListView typeArguments);
}