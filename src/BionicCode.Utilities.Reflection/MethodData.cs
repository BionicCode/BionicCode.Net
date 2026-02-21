namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

internal sealed partial class MethodData : ParameterizedMemberData, IMethodDataInvoker, IStrictMethodDataInvoker
{
    private static readonly Type s_asyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);
    private static readonly ConcurrentDictionary<InterfaceMappingKey, InterfaceMappingEntry> s_interfaceMappingTable = new();

    private SymbolAttributes _symbolAttributes;
    private AccessModifier _accessModifier;
    private bool? _isAwaitable;
    private bool? _isAwaitableTask;
    private bool? _isAwaitableValueTask;
    private bool? _isAwaitableGenericValueTask;
    private bool? _isAsync;
    private bool? _isExtensionMethod;
    private ParameterList? _parameters;
    private TypeList? _genericMethodArguments;
    private bool? _isOverride;
    private SymbolComponentInfo? _symbolComponentInfo;
    private string? _displayName;
    private string? _shortDisplayName;
    private string? _fullyQualifiedDisplayName;
    private string? _signature;
    private string? _shortSignature;
    private string? _fullyQualifiedRuntimeSignature;
    private string? _runtimeSignature;
    private string? _runtimeShortSignature;
    private string? _runtimeShortCompactSignature;
    private string? _shortCompactSignature;
    private string? _fullyQualifiedSignature;
    private TypeData? _returnTypeData;
    private ParameterData? _returnParameterData;
    private bool? _isGenericMethod;
    private bool? _isGenericTypeMethod;
    private MethodData? _genericMethodDefinitionData;
    private bool? _isReturnValueByRef;
    private readonly ConcurrentDictionary<MethodDataGenericTypeVariantKey, Delegate> _invokerTable;
    private volatile Func<object?, object?[]?, object?>? _invoker;
    private volatile Func<object?, object?[]?, Task>? _asyncTaskInvoker;
    private volatile Func<object?, object?[]?, Task<object?>>? _asyncGenericTaskInvoker;
    private volatile Func<object?, object?[]?, ValueTask<object?>>? _asyncGenericValueTaskInvoker;
    private volatile Func<object?, object?[]?, ValueTask>? _asyncValueTaskInvoker;
    private string? _assemblyName;
    private bool? _isReturnValueReadOnly;
    private bool? _containsGenericParameters;
    private bool? _hasParamsParameter;
    private bool? _isVoidMethod;
    private bool? _isAwaitableGenericTask;
    private bool? _isPropertySetMethod;
    private bool? _isPropertyGetMethod;
    private bool? _isDelegateInvokeMethod;
    private bool? _isDelegateBeginInvokeMethod;
    private bool? _isDelegateEndInvokeMethod;
    private bool? _isIndexerPropertyGetMethod;
    private bool? _isIndexerPropertySetMethod;
    private bool? _isEventAccessorMethod;
    private bool? _isEventAddMethod;
    private bool? _isEventRemoveMethod;
    private bool? _isOperatorOverload;
    private ParameterizedSymbolKind? _parameterizedSymbolKind;
    private BasicMethodFingerprint? _basicMethodFingerprint;
    private MemberData? _accessedMember;
    private readonly WellKnownMethodDescriptor _descriptor;
    private readonly MethodSignatureEqualityComparer _methodSignatureEqualityComparer;
    private RuntimeTypeHandle? _declaringTypeHandle;
    private RuntimeTypeHandle? _implementingTypeHandle;
    private bool? _isExplicitInterfaceImplementation;
    private IMethodDataView? _methodDataView;

    internal MethodData(WellKnownMethodDescriptor descriptor)
        : base(descriptor.MethodName, SymbolKind.MemberMethod)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(descriptor);

        _descriptor = descriptor;
        _invokerTable = new ConcurrentDictionary<MethodDataGenericTypeVariantKey, Delegate>();
        Handle = _descriptor.MethodHandle;
        MethodInfo = _descriptor.MethodInfo;

        _methodSignatureEqualityComparer = new MethodSignatureEqualityComparer();
    }

    internal new IMethodDataView View => _methodDataView ??= new MethodDataView(GetPublicCacheKey());

    internal MethodInfo MethodInfo { get; }

    internal override MethodBase MethodBase => MethodInfo;

    internal MethodData MakeGenericMethodData(TypeList typeDataArguments)
    {
        Type[] typeArguments = typeDataArguments.Select(t => t.Type).ToArray();
        MethodInfo genericMethodInfo = MethodInfo.MakeGenericMethod(typeArguments);
        return GetOrCreateCacheEntry(genericMethodInfo);
    }

    internal MethodData MakeGenericMethodData(params Type[] typeArguments)
    {
        MethodInfo genericMethodInfo = MethodInfo.MakeGenericMethod(typeArguments);
        return GetOrCreateCacheEntry(genericMethodInfo);
    }

    internal MethodInfo MakeGenericMethodInfo(TypeList typeArguments) => MethodInfo.MakeGenericMethod(typeArguments.Select(t => t.Type).ToArray());

    internal MethodInfo MakeGenericMethodInfo(params TypeData[] typeArguments) => MethodInfo.MakeGenericMethod(typeArguments.Select(t => t.Type).ToArray());

    internal MethodInfo MakeGenericMethodInfo(params Type[] typeArguments) => MethodInfo.MakeGenericMethod(typeArguments);

    /// <summary>
    /// Invokes the represented method on the specified target object using the provided arguments.
    /// </summary>
    /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
    /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
    /// method's parameters.</param>
    /// <returns>The return entryFactory of the invoked method, or null if the method has no return entryFactory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
    /// parameters, or if the method itself is a generic method definition or contains unassigned generic
    /// parameters.</exception>
    /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="true"/>)
    /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, object?[]?)"/> overload and provide the generic type parameter arguments.</remarks>
    internal object? Invoke(object? target, params object?[]? args) => Invoke(target, args.AsSpan());

    /// <summary>
    /// Invokes the represented method on the specified target object using the provided arguments.
    /// </summary>
    /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
    /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
    /// method's parameters.</param>
    /// <returns>The return entryFactory of the invoked method, or null if the method has no return entryFactory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
    /// parameters, or if the method itself is a generic method definition or contains unassigned generic
    /// parameters.</exception>
    /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="true"/>)
    /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, ReadOnlySpan{object?})"/> overload and provide the generic type parameter arguments.</remarks>
    internal object? Invoke(object? target, ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._invoker!.Invoke(target, args.ToArray());
    }

    /// <summary>
    /// Invokes the represented void method on the specified target object using the provided arguments.
    /// </summary>
    /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
    /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
    /// method's parameters.</param>
    /// <returns>The return entryFactory of the invoked method, or null if the method has no return entryFactory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
    /// parameters, or if the method itself is a generic method definition or contains unassigned generic
    /// parameters.</exception>
    /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="true"/>)
    /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, object?[]?)"/> overload and provide the generic type parameter arguments.</remarks>
    internal void Invoke<TTarget>(TTarget target, params object?[]? args) => Invoke<TTarget>(target, args.AsSpan());

    /// <summary>
    /// Invokes the represented void method on the specified target object using the provided arguments and returns the
    /// result.
    /// </summary>
    /// <remarks>This method performs validation to ensure that the invocation is valid for the
    /// method's signature and type constraints. Attempting to invoke an asynchronous or open generic method, or
    /// providing invalid arguments, will result in an exception. For methods with no return entryFactory, use a compatible
    /// <typeparamref name="TResult"/> type such as void or object.</remarks>
    /// <typeparam name="TTarget">The type of the object on which the method is invoked.</typeparam>
    /// <typeparam name="TResult">The type of the entryFactory returned by the invoked method.</typeparam>
    /// <param name="target">The instance of the target object on which to invoke the method. For static methods, this parameter is
    /// ignored.</param>
    /// <param name="args">A read-only span containing the arguments to pass to the method. The number, order, and types of arguments
    /// must match the method's parameters.</param>
    /// <returns>The result returned by the invoked method.</returns>
    internal void Invoke<TTarget>(TTarget target, ReadOnlySpan<object?> args)
    {
        ThrowIfNonVoidMethodIsInvokedAsVoid();
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        var invokerMethod = (MethodVoidInvoker<TTarget>)GetInvokerInternal<TTarget>(TypeList.Empty);
        Debug.Assert(invokerMethod is not null);

        invokerMethod.Invoke(target, args.ToArray());
    }

    /// <summary>
    /// Invokes the represented method on the specified target object using the provided arguments.
    /// </summary>
    /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
    /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
    /// method's parameters.</param>
    /// <returns>The return entryFactory of the invoked method, or null if the method has no return entryFactory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
    /// parameters, or if the method itself is a generic method definition or contains unassigned generic
    /// parameters.</exception>
    /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="true"/>)
    /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, object?[]?)"/> overload and provide the generic type parameter arguments.</remarks>
    internal TResult Invoke<TTarget, TResult>(TTarget target, params object?[]? args) => Invoke<TTarget, TResult>(target, args.AsSpan());

    /// <summary>
    /// Invokes the represented method on the specified target object using the provided arguments and returns the
    /// result.
    /// </summary>
    /// <remarks>This method performs validation to ensure that the invocation is valid for the
    /// method's signature and type constraints. Attempting to invoke an asynchronous or open generic method, or
    /// providing invalid arguments, will result in an exception. For methods with no return entryFactory, use a compatible
    /// <typeparamref name="TResult"/> type such as void or object.</remarks>
    /// <typeparam name="TTarget">The type of the object on which the method is invoked.</typeparam>
    /// <typeparam name="TResult">The type of the entryFactory returned by the invoked method.</typeparam>
    /// <param name="target">The instance of the target object on which to invoke the method. For static methods, this parameter is
    /// ignored.</param>
    /// <param name="args">A read-only span containing the arguments to pass to the method. The number, order, and types of arguments
    /// must match the method's parameters.</param>
    /// <returns>The result returned by the invoked method.</returns>
    internal TResult Invoke<TTarget, TResult>(TTarget target, ReadOnlySpan<object?> args)
    {
        ThrowIfVoidMethodIsInvokedAsNonVoid();
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        var invokerMethod = (MethodInvoker<TTarget, TResult>)GetInvokerInternal<TTarget, TResult>(TypeList.Empty);
        Debug.Assert(invokerMethod is not null);

        return invokerMethod.Invoke(target, args);
    }

    internal async Task InvokeAwaitableTaskAndDiscardResultAsync<TTarget>(TTarget target, params object?[] args)
    {
        ThrowIfVoidMethodIsInvokedAsNonVoid();
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        // TResult will be ignored by the 'DelegateProvider' since isDiscard is TRUE.
        // Hence, 'Task' is acting as a dummy generic type parameter in this special case.
        var invokerMethod = (MethodAwaitableTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, Task>(TypeList.Empty, isDiscard: true);
        Debug.Assert(invokerMethod is not null);

        await invokerMethod.Invoke(target, args).ConfigureAwait(false);
    }

    internal async ValueTask InvokeAwaitableValueTaskAndDiscardResultAsync<TTarget>(TTarget target, params object?[] args)
    {
        ThrowIfVoidMethodIsInvokedAsNonVoid();
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        // TResult will be ignored by the 'DelegateProvider' since isDiscard is TRUE.
        // Hence, 'ValueTask' is acting as a dummy generic type parameter in this special case.
        var invokerMethod = (MethodAwaitableValueTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, ValueTask>(TypeList.Empty, isDiscard: true);
        Debug.Assert(invokerMethod is not null);

        await invokerMethod.Invoke(target, args.ToArray());
    }

    /// <summary>
    /// Invokes an open generic method on the specified target, using the provided generic type arguments and method
    /// parameters.
    /// </summary>
    /// <remarks>The method validates that the provided generic type arguments and method parameters
    /// match the requirements of the method being invoked. For methods with a 'params' parameter, it is valid to
    /// omit arguments for the parameter, in which case an empty array is passed.</remarks>
    /// <param name="target">The object instance on which to invoke the method. Must be non-null for instance methods; ignored for static
    /// methods.</param>
    /// <param name="genericMethodParameters">A sequence of generic type arguments to use when constructing the closed generic method. The number of
    /// elements must match the method's generic parameter count.</param>
    /// <param name="args">An array of arguments to pass to the method. The number and types of arguments must match the method's
    /// parameters.</param>
    /// <returns>The return entryFactory of the invoked method, or null if the method has no return entryFactory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the declaring type is a generic type definition or contains unassigned generic parameters, or if
    /// the method itself is not a closed generic method.</exception>
    internal object? InvokeOpenGeneric(object? target, TypeList genericMethodParameters, params object?[]? args) => InvokeOpenGeneric(target, genericMethodParameters, args.AsSpan());

    /// <summary>
    /// Invokes an open generic method on the specified target, using the provided generic type arguments and method
    /// parameters.
    /// </summary>
    /// <remarks>The method validates that the provided generic type arguments and method parameters
    /// match the requirements of the method being invoked. For methods with a 'params' parameter, it is valid to
    /// omit arguments for the parameter, in which case an empty array is passed.</remarks>
    /// <param name="target">The object instance on which to invoke the method. Must be non-null for instance methods; ignored for static
    /// methods.</param>
    /// <param name="genericMethodParameters">A sequence of generic type arguments to use when constructing the closed generic method. The number of
    /// elements must match the method's generic parameter count.</param>
    /// <param name="args">An array of arguments to pass to the method. The number and types of arguments must match the method's
    /// parameters.</param>
    /// <returns>The return entryFactory of the invoked method, or null if the method has no return entryFactory.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the declaring type is a generic type definition or contains unassigned generic parameters, or if
    /// the method itself is not a closed generic method.</exception>
    internal object? InvokeOpenGeneric(object? target, TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGeneric), nameof(Invoke));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));
        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._invoker!.Invoke(target, args.ToArray());
    }

    internal async Task InvokeAwaitableTaskAsync(object? target, params object?[] args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call '{nameof(IsAwaitableTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task'.");
        }

        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableTaskAsync), nameof(InvokeOpenGenericAwaitableTaskAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        await invocatorMethod._asyncTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async Task InvokeOpenGenericAwaitableTaskAsync(object? target, TypeList genericMethodParameters, params object?[]? args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call '{nameof(IsAwaitableTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task'.");
        }

        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericAwaitableTaskAsync), nameof(InvokeAwaitableTaskAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));
        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        await invocatorMethod._asyncTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async Task<object?> InvokeAwaitableTaskWithResultAsync(object? target, params object?[]? args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableGenericTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call '{nameof(IsAwaitableGenericTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task<T>'.");
        }

        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableTaskWithResultAsync), nameof(InvokeOpenGenericAwaitableTaskWithResultAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return await invocatorMethod._asyncGenericTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async Task<object?> InvokeOpenGenericAwaitableTaskWithResultAsync(object? target, TypeList genericMethodParameters, params object?[]? args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableGenericTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call '{nameof(IsAwaitableGenericTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task<T>'.");
        }

        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericAwaitableTaskWithResultAsync), nameof(InvokeAwaitableTaskWithResultAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return await invocatorMethod._asyncGenericTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async ValueTask InvokeAwaitableValueTaskAsync(object? target, params object?[]? args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableValueTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call '{nameof(IsAwaitableValueTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask'.");
        }

        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableValueTaskAsync), nameof(InvokeOpenGenericAwaitableValueTaskAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        await invocatorMethod._asyncValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async ValueTask InvokeOpenGenericAwaitableValueTaskAsync(object? target, TypeList genericMethodParameters, params object?[]? args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableValueTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(IsAwaitableValueTask)} to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask'.");
        }

        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericAwaitableValueTaskAsync), nameof(InvokeAwaitableValueTaskAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        await invocatorMethod._asyncValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async ValueTask<object?> InvokeAwaitableValueTaskWithResultAsync(object? target, params object?[] args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableGenericValueTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(IsAwaitableValueTask)} to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask<T>'.");
        }

        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableValueTaskWithResultAsync), nameof(InvokeOpenGenericValueAwaitableTaskWithResultAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return await invocatorMethod._asyncGenericValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal async ValueTask<object?> InvokeOpenGenericValueAwaitableTaskWithResultAsync(object? target, TypeList genericMethodParameters, params object?[] args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        if (!IsAwaitableValueTask)
        {
            throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(IsAwaitableValueTask)} to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask<T>'.");
        }

        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericValueAwaitableTaskWithResultAsync), nameof(InvokeAwaitableValueTaskWithResultAsync));
        ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));
        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return await invocatorMethod._asyncGenericValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
    }

    internal Func<object?, object?[]?, object?> GetInvoker(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetInvoker), nameof(GetOpenGenericInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._invoker!;
    }

    internal Func<object?, object?[]?, object?> GetOpenGenericInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericInvoker), nameof(GetInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._invoker!;
    }

    internal Func<object?, object?[]?, Task> GetAwaitableTaskInvoker(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableTaskInvoker), nameof(GetOpenGenericAwaitableTaskInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncTaskInvoker!;
    }

    internal Func<object?, object?[]?, Task> GetOpenGenericAwaitableTaskInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskInvoker), nameof(GetAwaitableTaskInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncTaskInvoker!;
    }

    internal Func<object?, object?[]?, Task<object?>> GetAwaitableTaskWithResultInvoker(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableTaskWithResultInvoker), nameof(GetOpenGenericAwaitableTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncGenericTaskInvoker!;
    }

    internal Func<object?, object?[]?, Task<object?>> GetOpenGenericAwaitableTaskWithResultInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncGenericTaskInvoker!;
    }

    internal Func<object?, object?[]?, ValueTask> GetAwaitableValueTaskInvoker(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableValueTaskInvoker), nameof(GetOpenGenericAwaitableValueTaskInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncValueTaskInvoker!;
    }

    internal Func<object?, object?[]?, ValueTask> GetOpenGenericAwaitableValueTaskInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableValueTaskInvoker), nameof(GetAwaitableValueTaskInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncValueTaskInvoker!;
    }

    internal Func<object?, object?[]?, ValueTask<object?>> GetAwaitableValueTaskWithResultInvoker(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableValueTaskWithResultInvoker), nameof(GetOpenGenericAwaitableValueTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncGenericValueTaskInvoker!;
    }

    internal Func<object?, object?[]?, ValueTask<object?>> GetOpenGenericAwaitableValueTaskWithResultInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableValueTaskWithResultInvoker), nameof(GetAwaitableValueTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod._asyncGenericValueTaskInvoker!;
    }

    internal MethodAwaitableTaskDiscardInvoker<TTarget> GetAwaitableTaskDiscardInvoker<TTarget>(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        var invocatorMethod = (MethodAwaitableTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, Task>(TypeList.Empty, isDiscard: true);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod;
    }

    internal MethodAwaitableTaskDiscardInvoker<TTarget> GetOpenGenericAwaitableTaskDiscardInvoker<TTarget>(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        var invocatorMethod = (MethodAwaitableTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, Task>(genericMethodParameters, isDiscard: true);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod;
    }

    internal MethodAwaitableValueTaskDiscardInvoker<TTarget> GetAwaitableValueTaskDiscardInvoker<TTarget>(ReadOnlySpan<object?> args)
    {
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableValueTaskInvoker), nameof(GetOpenGenericAwaitableValueTaskInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        var invocatorMethod = (MethodAwaitableValueTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, ValueTask>(TypeList.Empty, isDiscard: true);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod!;
    }

    internal MethodAwaitableValueTaskDiscardInvoker<TTarget> GetOpenGenericAwaitableValueTaskDiscardInvoker<TTarget>(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(genericMethodParameters);
        ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
        ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
        ThrowIfDeclaringTypeIsAnOpenGenericType();
        ThrowIfInvalidMethodArguments(args, nameof(args));

        ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, GenericMethodParameters.Count, nameof(genericMethodParameters));

        var invocatorMethod = (MethodAwaitableValueTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, ValueTask<object>>(genericMethodParameters, isDiscard: true);
        Debug.Assert(invocatorMethod is not null);

        return invocatorMethod;
    }

    private void ThrowIfInvalidMethodArguments(ReadOnlySpan<object?> args, string paramName)
    {
        // Validate arguments against method parameters
        if (Parameters.HasItems)
        {
            if (HasParamsParameter)
            {
                // NULL is valid for 'args' if there is only a single non-params parameter since params can be empty.
                // Additionally, no need to check 'args' for NULL if there is only the params parameter.
                // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                if (Parameters.Count > 2)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                        args.Length,
                        Parameters.Count - 1,
                        paramName,
                        $"Parameter count mismatch. The number of method arguments provided by the argument list {paramName} does not match the method's signature. Expected: '{Parameters.Count - 1}' mandatory and optional 'params' arguments. Found: '{args.Length}' arguments.");
                }
            }
            else
            {
                ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                    args.Length,
                    Parameters.Count,
                    paramName,
                    $"Parameter count mismatch. The number of method arguments provided by the argument list {paramName} does not match the method's signature. Expected: '{Parameters.Count}' arguments. Found: '{args.Length}' arguments.");
            }
        }
        else if (!args.IsEmpty) // Method has no parameters but arguments were provided.
        {
            throw new ArgumentOutOfRangeExceptionAdvanced(
                paramName,
                $"Parameter count mismatch. The number of method arguments provided by the argument list {paramName} does not match the method's signature. Expected: '{Parameters.Count}' arguments. Found: '{args.Length}' arguments.");
        }
    }

    private void ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously()
    {
        if (!IsAsync)
        {
            throw new InvalidOperationException("Cannot invoke synchronous methods using asynchronous invocation methods. Call the synchronous invocation methods instead.");
        }
    }

    private void ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously()
    {
        if (IsAsync)
        {
            throw new InvalidOperationException("Cannot invoke asynchronous methods using synchronous invocation methods. Call the asynchronous invocation methods instead.");
        }
    }

    private void ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(object? target)
    {
        if (!IsStatic)
        {
            ArgumentNullException.ThrowIfNull(target, nameof(target));

            Type targetType = target.GetType();
            Type declaringType = DeclaringTypeData.Type;
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(target),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(target),
                        declaringType,
                        "declaring type"));
        }
    }

    private void ThrowIfNonVoidMethodIsInvokedAsVoid()
    {
        if (!IsVoidMethod)
        {
            throw new InvalidOperationException($"The method '{Name}' does not have a void return type. Use the '{nameof(Invoke)}<TTarget, TResult>()' overload to invoke methods with a return entryFactory.");
        }
    }

    private void ThrowIfVoidMethodIsInvokedAsNonVoid()
    {
        if (IsVoidMethod)
        {
            throw new InvalidOperationException($"The method '{Name}' has a void return type. Use the '{nameof(Invoke)}<TTarge>()' overload to invoke the void method.");
        }
    }

    private void ThrowIfDeclaringTypeIsAnOpenGenericType()
    {
        if (DeclaringTypeData.IsGenericTypeDefinition || DeclaringTypeData.ContainsGenericParameters)
        {
            throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic type parameters. Ensure the declaring generic type is properly closed.");
        }
    }

    private void ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(string nameOfWrongMethod, string nameOfCorrectMethod)
    {
        if (!IsOpenGenericMethodOrGenericMethodDefinition)
        {
            throw new InvalidOperationException($"Cannot invoke non-generic methods using '{nameOfWrongMethod}'. Call '{nameOfCorrectMethod}' instead.");
        }
    }

    private void ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(string nameOfWrongMethod, string nameOfCorrectMethod)
    {
        if (IsOpenGenericMethodOrGenericMethodDefinition)
        {
            throw new InvalidOperationException($"Cannot invoke generic methods that are not closed using {nameOfWrongMethod}. Call {nameOfCorrectMethod} instead to ensure the generic method is properly closed by specifying the required generic type arguments.");
        }
    }

    private MethodData GetInvokerInternal(TypeList genericMethodArguments)
    {
        // IMethodDataInvoker.IsInvocable checks whether 'this' is already a closed generic method with constructed invocator based on the provided generic type arguments.
        // If the result is FALSE, we need to ask the 'DelegateProvider' to generates or get the cached closed generic method variant if the current 'MethodData' is an open generic member
        // and additionally ask to generate the requested invocator based on the provided generic type arguments (if not already cached).
        MethodData invocatorSource = ((IMethodDataInvoker)this).IsInvocable
            // 'this' is already a closed generic method with constructed invocator.
            // Reason: only closed generic methods can have invocator/are invocable...
            ? this

            // ...otherwise generate or get cached invocator
            : DelegateProvider.GetOrCreateFastMethodInvoker(this, genericMethodArguments);

        return invocatorSource;
    }

    private Delegate GetInvokerInternal<TTarget>(TypeList genericMethodArguments)
    {
        Type resultType = typeof(void);
        Type targetType = typeof(TTarget);
        var genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
            genericMethodArguments,
            resultType.TypeHandle,
            targetType.TypeHandle,
            BasicMethodFingerprint);

        // IStrictMethodDataInvoker.IsInvocable checks whether 'this' is already a closed generic method with constructed invocator based on the provided generic type arguments.
        // If the result is FALSE, it generates or gets the closed generic method variant if the current 'MethodData' is an open generic member
        // and additionally generates the requested invocator based on the provided generic type arguments (if not already cached).
        MethodData invocatorSource = ((IStrictMethodDataInvoker)this).IsInvocable(genericTypedMethodVariantKey)
            // 'this' is already a closed generic method with a constructed invocator based on the provided generic type arguments.
            // Reason: per definition, only closed generic methods can have invocator/are invocable...
            ? this

            // ...otherwise generate or get the closed generic method variant if the current 'MethodData' is an open generic member
            // and generate the requested invocator based on the provided generic type arguments (if not already cached).
            // In other words, the returned MethodData is always a closed generic method variant with the correct invocator delegate generated when necessary.
            : DelegateProvider.GetOrCreateFastVoidMethodInvoker<TTarget>(this, genericMethodArguments);

        return invocatorSource._invokerTable.TryGetValue(genericTypedMethodVariantKey, out Delegate? invoker)
            ? invoker
            : throw new InvalidOperationException("Unable to create the strictly typed method invoker.");
    }

    private Delegate GetInvokerInternal<TTarget, TResult>(TypeList genericMethodArguments, bool isDiscard = false)
    {
        Type targetType = typeof(TTarget);
        Type resultType = typeof(TResult);
        var genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
            genericMethodArguments,
            resultType.TypeHandle,
            targetType.TypeHandle,
            BasicMethodFingerprint);

        // IStrictMethodDataInvoker.IsInvocable checks whether 'this' is already a closed generic method with constructed invocator based on the provided generic type arguments.
        // If the result is FALSE, it generates or gets the closed generic method variant if the current 'MethodData' is an open generic member
        // and additionally generates the requested invocator based on the provided generic type arguments (if not already cached).
        MethodData invocatorSource = ((IStrictMethodDataInvoker)this).IsInvocable(genericTypedMethodVariantKey)
            // 'this' is already a closed generic method with constructed invocator.
            // Reason: only closed generic methods can have invocator/are invocable...
            ? this

            // ...otherwise generate or get the closed generic method variant if the current 'MethodData' is an open generic member
            // and generate the requested invocator based on the provided generic type arguments (if not already cached).
            // In other words, the returned MethodData is always a closed generic method variant with the correct invocator delegate generated when necessary.
            : DelegateProvider.GetOrCreateFastMethodInvoker<TTarget, TResult>(this, genericMethodArguments, isDiscardDelegate: isDiscard);

        return invocatorSource._invokerTable.TryGetValue(genericTypedMethodVariantKey, out Delegate? invoker)
            ? invoker
            : throw new InvalidOperationException("Unable to create  the strictly typed method invoker.");
    }

    internal override RuntimeMethodHandle Handle { get; }

    /// <summary>
    /// Provides a basic fingerprint for this method based on its name, declaring type, return type, parameters and generic method parameters (if the method is a generic method).
    /// </summary>
    /// <remarks>The <see cref="BasicMethodFingerprint"/> allows to identify methods based on the least required attributes.
    /// This allows to identify methods that belong to the same generic family (based on the open generic method signature).<br/>
    /// For example, the open generic method would share the same basic fingerprint like all  it's closed generic variants.<br/>
    /// In other words, the <see cref="BasicMethodFingerprint"/> enables the identification of generic methods that share the same generic structure since it is based on the raw unconstructed generic method signature.</remarks>
    internal BasicMethodFingerprint BasicMethodFingerprint
    {
        get
        {
            if (_basicMethodFingerprint is null)
            {
                if (IsGenericMethod && !IsGenericMethodDefinition)
                {
                    MethodInfo genericMethodDefinition = MethodInfo.GetGenericMethodDefinition();
                    MethodData genericMethodDefinitionData = GetOrCreateCacheEntry(genericMethodDefinition);
                    _basicMethodFingerprint = genericMethodDefinitionData.BasicMethodFingerprint;
                }
                else
                {
                    _basicMethodFingerprint ??= new BasicMethodFingerprint(
                        Name,
                        DeclaringTypeHandle,
                        ReturnTypeData.Handle,
                        Parameters,
                        GenericMethodParameters);
                }
            }

            return (BasicMethodFingerprint)_basicMethodFingerprint;
        }
    }

    internal MethodData GenericMethodDefinitionData
    {
        get
        {
            if (IsGenericMethodDefinition)
            {
                return this;
            }
            else
            {
                MethodInfo genericMethodDefinitionMethodInfo = MethodInfo.GetGenericMethodDefinition();
                _genericMethodDefinitionData = GetOrCreateCacheEntry(genericMethodDefinitionMethodInfo);
            }

            return _genericMethodDefinitionData;
        }
    }

    internal bool EqualsBySignature(MethodData other) => _methodSignatureEqualityComparer.Equals(this, other);

    internal override ParameterList Parameters => _parameters ??= ParameterListBuilder.Create(parameterizedMember: this);

    internal override bool HasParamsParameter => _hasParamsParameter ??= Parameters.HasItems && Parameters[^1].IsParams;

    internal override bool IsExplicitInterfaceImplementation
    {
        get
        {
            if (_isExplicitInterfaceImplementation is null)
            {
                InitializeDeclaringTypeAndExplicitImplementationContext(this);
            }

            return _isExplicitInterfaceImplementation!.Value;
        }
    }

    /// <inheritdoc/>
    internal override RuntimeTypeHandle DeclaringTypeHandle
    {
        get
        {
            if (_declaringTypeHandle is null)
            {
                InitializeDeclaringTypeAndExplicitImplementationContext(this);
            }

            return _declaringTypeHandle!.Value;
        }
    }

    /// <inheritdoc/>
    internal override RuntimeTypeHandle ImplementingTypeHandle
    {
        get
        {
            if (_implementingTypeHandle is null)
            {
                InitializeDeclaringTypeAndExplicitImplementationContext(this);
            }

            return _implementingTypeHandle!.Value;
        }
    }

    /// <summary>
    /// Checks whether the method is a property set method. Will not include indexer set methods.<br/>
    /// Use <see cref="IsIndexerPropertySetMethod"/> to specifically check for indexer set methods and exclude normal properties.
    /// </summary>
    /// <entryFactory>Returns <see langword="true"/> if the method is a property set method; otherwise, <see langword="false"/>.<para/>
    /// This entryFactory exclusively describes non-indexer properties and therefore also returns <see langword="false"/> for indexer property setters.</entryFactory>
    internal bool IsPropertySetMethod => _isPropertySetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: false, isSetter: true);

    /// <summary>
    /// Checks whether the method is a property set method. Will not include indexer set methods.<br/>
    /// Use <see cref="IsIndexerPropertyGetMethod"/> to specifically check for indexer get methods and exclude normal properties.
    /// </summary>
    /// <entryFactory>Returns <see langword="true"/> if the method is a property get method; otherwise, <see langword="false"/>.<para/>
    /// This entryFactory exclusively describes non-indexer properties and therefore also returns <see langword="false"/> for indexer property getters.</entryFactory>
    internal bool IsPropertyGetMethod => _isPropertyGetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: false, isSetter: false);

    /// <summary>
    /// Checks whether the method is an indexer property set method.
    /// Will not include non-indexer property set methods.<br/>
    /// Use <see cref="IsPropertySetMethod"/> to specifically check for non-indexer property set methods and exclude indexer properties.
    /// </summary>
    /// <entryFactory>Returns <see langword="true"/> if the method is an indexer property set method; otherwise, <see langword="false"/>.<para/>
    /// This entryFactory exclusively describes indexer properties and therefore also returns <see langword="false"/> for non-indexer property setters.</entryFactory>
    internal bool IsIndexerPropertySetMethod => _isIndexerPropertySetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: true, isSetter: true);

    /// <summary>
    /// Checks whether the method is an indexer property get method.
    /// </summary>
    /// <entryFactory>Returns <see langword="true"/> if the method is an indexer property get method; otherwise, <see langword="false"/>.<para/>
    /// This entryFactory exclusively describes indexer properties and therefore also returns <see langword="false"/> for non-indexer property getters.</entryFactory>
    internal bool IsIndexerPropertyGetMethod => _isIndexerPropertyGetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: true, isSetter: false);

    /// <summary>
    /// Gets a entryFactory indicating whether the method is a property (non-indexer and indexer) accessor method (get or set).<br/>
    /// </summary>
    /// <entryFactory>Returns <see langword="true"/> if the method is a property accessor method; otherwise, <see langword="false"/>.<para/>
    internal bool IsPropertyAccessorMethod => IsPropertyGetMethod || IsPropertySetMethod || IsIndexerPropertyGetMethod || IsIndexerPropertySetMethod;

    internal bool IsDelegateInvokeMethod => _isDelegateInvokeMethod ??= MethodData.IsDelegateInvoke(this);

    internal bool IsDelegateBeginInvokeMethod => _isDelegateBeginInvokeMethod ??= MethodData.IsDelegateBeginInvoke(this);

    internal bool IsDelegateEndInvokeMethod => _isDelegateEndInvokeMethod ??= MethodData.IsDelegateEndInvoke(this);

    internal bool IsDelegateMethod => IsDelegateInvokeMethod || IsDelegateBeginInvokeMethod || IsDelegateEndInvokeMethod;

    internal bool IsEventAddMethod => _isEventAddMethod ??= MethodData.IsEventAccessor(this, isAddAccessor: true);

    internal bool IsEventRemoveMethod => _isEventRemoveMethod ??= MethodData.IsEventAccessor(this, isAddAccessor: false);

    internal bool IsEventAccessorMethod => _isEventAccessorMethod ??= IsEventAddMethod || IsEventRemoveMethod;

    /// <summary>
    /// Gets a entryFactory indicating whether the method is an accessor method (property or event).<br/>
    /// This entryFactory exclusively describes accessor methods and therefore also returns <see langword="false"/> for non-accessor methods.
    /// </summary>
    internal bool IsAccessorMethod => IsPropertyAccessorMethod || IsEventAccessorMethod;

    /// <summary>
    /// If the current <see cref="MethodData"/> instance is an accessor method, gets the <see cref="PropertyData"/> or <see cref="EventData"/> that is accessed by this accessor method.
    /// </summary>
    /// <remarks>This property is only valid when the current method is an accessor (such as a
    /// property getter or setter, or an event adder or remover). Call <see cref="MethodData.IsAccessorMethod"/> to determine whether the
    /// method is an accessor before accessing this property.</remarks>
    /// <exception cref="InvalidOperationException">Thrown if the current method is not an accessor method.</exception>
    /// <entryFactory>The <see cref="PropertyData"/> or <see cref="EventData"/> representing the property or event accessed by this accessor method.</entryFactory>
    internal MemberData AccessedMember => IsAccessorMethod
        ? _accessedMember! // The earlier call to MethodData.IsAccessorMethod ensured that _accessedMember is set.
        : throw new InvalidOperationException($"The current method is not an accessor. Call {nameof(MethodData.IsAccessorMethod)} before accessing this property to avoid this exception.");

    internal bool IsOperatorOverload => _isOperatorOverload ??= MethodData.IsOperator(this);

    internal bool IsVoidMethod => _isVoidMethod ??= ReturnTypeData.Type == typeof(void);

    internal TypeList GenericMethodParameters => _genericMethodArguments ??= TypeListBuilder.CreateGenericTypeArgumentList(this);

    internal override AccessModifier AccessModifier => _accessModifier is AccessModifier.Undefined
        ? (_accessModifier = MethodData.GetAccessModifier(this))
        : _accessModifier;

    internal bool IsExtensionMethod => _isExtensionMethod ??= MethodData.IsMethodExtensionMethod(this);

    internal bool IsAsync => _isAsync ??= IsMarkedAsync(this);

    internal bool IsAwaitable => _isAwaitable ??= ReturnTypeData.IsAwaitable;

    internal bool IsAwaitableTask
    {
        get
        {
            if (_isAwaitableTask is null)
            {
                if (!IsAwaitable)
                {
                    _isAwaitableTask = false;
                }
                else
                {
                    _isAwaitableTask = (_isAwaitableValueTask.HasValue
                      && !_isAwaitableValueTask.Value
                      && ReturnTypeData.IsAwaitableTask)
                      || (!_isAwaitableValueTask.HasValue && ReturnTypeData.IsAwaitableTask);
                }
            }

            return _isAwaitableTask.Value;
        }
    }

    internal bool IsAwaitableValueTask
    {
        get
        {
            if (_isAwaitableValueTask is null)
            {
                if (!IsAwaitable || ReturnTypeData.IsGenericType)
                {
                    _isAwaitableValueTask = false;
                }
                else
                {
                    _isAwaitableValueTask = (_isAwaitableTask.HasValue
                      && !_isAwaitableTask.Value
                      && ReturnTypeData.IsAwaitableValueTask)
                      || (!_isAwaitableTask.HasValue && ReturnTypeData.IsAwaitableValueTask);
                }
            }

            return _isAwaitableValueTask.Value;
        }
    }

    internal bool IsAwaitableGenericValueTask
    {
        get
        {
            if (_isAwaitableGenericValueTask is null)
            {
                if (!IsAwaitable || !ReturnTypeData.IsGenericType)
                {
                    _isAwaitableGenericValueTask = false;
                }
                else
                {
                    _isAwaitableGenericValueTask = (_isAwaitableTask.HasValue
                      && !_isAwaitableTask.Value
                      && ReturnTypeData.IsAwaitableValueTask)
                      || (!_isAwaitableTask.HasValue && ReturnTypeData.IsAwaitableValueTask);
                }
            }

            return _isAwaitableGenericValueTask.Value;
        }
    }

    internal bool IsAwaitableGenericTask
    {
        get
        {
            if (_isAwaitableGenericTask is null)
            {
                if (!IsAwaitable || !ReturnTypeData.IsGenericType)
                {
                    _isAwaitableGenericTask = false;
                }
                else
                {
                    _isAwaitableGenericTask = (_isAwaitableTask.HasValue
                      && !_isAwaitableTask.Value
                      && ReturnTypeData.IsAwaitableTask)
                      || (!_isAwaitableTask.HasValue && ReturnTypeData.IsAwaitableTask);
                }
            }

            return _isAwaitableGenericTask.Value;
        }
    }

    internal bool IsOverride => _isOverride ??= MethodData.IsMethodOverride(this);

    internal bool IsReturnValueRefReadOnly => _isReturnValueReadOnly ??= ReturnParameterData.IsRefReadOnly;

    internal bool IsReturnValueByRef => _isReturnValueByRef ??= ReturnTypeData.IsByRef;

    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
        ? (_symbolAttributes = MethodData.GetAttributes(this))
        : _symbolAttributes;

    internal override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    internal override string Signature => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortSignature => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortCompactSignature => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    internal override string FullyQualifiedSignature => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string FullyQualifiedRuntimeSignature => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeSignature => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortSignature => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortCompactSignature => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    internal override string DisplayName => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string ShortDisplayName => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    internal override string FullyQualifiedDisplayName => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    internal override string AssemblyName => _assemblyName ??= DeclaringTypeData.AssemblyName;

    internal TypeData ReturnTypeData => _returnTypeData ??= GetOrCreateCacheEntry(MethodInfo.ReturnType);

    internal ParameterData ReturnParameterData => _returnParameterData ??= GetOrCreateCacheEntry(MethodInfo.ReturnParameter);

    internal bool IsGenericMethod => _isGenericMethod ??= MethodInfo.IsGenericMethod;

    internal bool IsGenericMethodDefinition => _isGenericTypeMethod ??= MethodInfo.IsGenericMethodDefinition;

    internal bool ContainsGenericParameters => _containsGenericParameters ??= MethodInfo.ContainsGenericParameters;

    internal bool IsOpenGenericMethodOrGenericMethodDefinition => (IsGenericMethod && ContainsGenericParameters) || IsGenericMethodDefinition;

    internal override ParameterizedSymbolKind ParameterizedSymbolKind => _parameterizedSymbolKind ??= IsPropertySetMethod
        ? ParameterizedSymbolKind.MemberNormalPropertySet
        : IsIndexerPropertyGetMethod
            ? ParameterizedSymbolKind.MemberIndexerPropertyGet
            : IsIndexerPropertySetMethod
                ? ParameterizedSymbolKind.MemberIndexerPropertySet
                : ParameterizedSymbolKind.MemberMethod;

    private static bool IsMethodExtensionMethod(MethodData methodData)
    {
        // Check if the declaring class satisfies the constraints to declare extension methods
        TypeData declaringTypeData = methodData.DeclaringTypeData;
        if (!declaringTypeData.CanDeclareExtensionMethod)
        {
            return false;
        }

        /* Check if the closedGenericMethoMethodInfo satisfies the constraints to act as an extension methods */

        if (!methodData.IsStatic)
        {
            return false;
        }

        bool hasExtensionMethodMarker = methodData.HasCompilerAttribute<ExtensionAttribute>(ReflectionConstants.ExtensionAttributeFullName);
        if (!hasExtensionMethodMarker)
        {
            return false;
        }

        ParameterList parameterInfoData = methodData.Parameters;

        // Must have at least the 'this' parameter
        if (parameterInfoData.Count < 1)
        {
            return false;
        }

        return true;
    }

    private static bool IsMarkedAsync(MethodData methodData) => methodData.MethodInfo.GetCustomAttribute(MethodData.s_asyncStateMachineAttributeType) != null;

    private static bool IsPropertyAccessor(MethodData methodData, bool isIndexer, bool isSetter)
    {
        /* Attempt to branch early out */

        if (!methodData.IsSpecialName)
        {
            return false;
        }

        if (methodData._isOperatorOverload == true)
        {
            // Method is already identified as operator overload
            return false;
        }

        /* 
         * Dereference fields and not properties to avoid reflection in case the property was not materialized yet.
         * Additionally, the 'Nullable<bool>' allows to identify whether the property was already evaluated.
         */

        if (methodData._isDelegateBeginInvokeMethod == true
            || methodData._isDelegateEndInvokeMethod == true
            || methodData._isDelegateInvokeMethod == true)
        {
            // Method is already identified as delegate invoke, begin invoke or end invoke method
            return false;
        }

        if (isIndexer && (methodData._isPropertyGetMethod == true || methodData._isPropertySetMethod == true))
        {
            // Method is already identified as non-indexer property accessor
            return false;
        }

        if (isSetter && methodData._isIndexerPropertyGetMethod == true)
        {
            // Method is already identified as indexer getter.
            return false;
        }

        if (!isSetter && methodData._isIndexerPropertySetMethod == true)
        {
            // Method is already identified as indexer setter.
            return false;
        }

        /* Detailed check is required */

        TypeData declaringTypeData = methodData.DeclaringTypeData;

        // TypeData.EnumerateProperties() uses caching internally to improve performance on repeated calls. Hence no need to cache here.
        // After caching enumeration becomes a simple O(1) operation and completely avoids any reflection.
        foreach (PropertyData propertyData in declaringTypeData.EnumerateProperties())
        {
            // Skip properties that do not match the 'isIndexer' parameter criteria
            if (isIndexer != propertyData.IsIndexer)
            {
                continue;
            }

            MethodData? accessorMethod = isSetter
                ? propertyData.CanWrite
                    ? propertyData.PropertySetMethodData
                    : null
                : propertyData.CanRead
                    ? propertyData.PropertyGetMethodData
                    : null;

            if (ReferenceEquals(accessorMethod, methodData))
            {
                methodData._accessedMember = propertyData;
                return true;
            }
        }

        return false;
    }

    private static bool IsEventAccessor(MethodData methodData, bool isAddAccessor)
    {
        /* Attempt to branch early out */

        if (!methodData.IsSpecialName)
        {
            return false;
        }

        if (methodData._isOperatorOverload == true)
        {
            // Method is already identified as operator overload
            return false;
        }

        /* 
         * Dereference fields and not properties to avoid reflection in case the property was not materialized yet.
         * Additionally, the 'Nullable<bool>' allows to identify whether the property was already evaluated.
         */

        if (methodData._isDelegateBeginInvokeMethod == true
            || methodData._isDelegateEndInvokeMethod == true
            || methodData._isDelegateInvokeMethod == true)
        {
            // Method is already identified as delegate invoke, begin invoke or end invoke method
            return false;
        }

        if (isAddAccessor && methodData._isEventRemoveMethod == true)
        {
            // Method is already identified as event remove accessor
            return false;
        }

        if (!isAddAccessor && methodData._isEventAddMethod == true)
        {
            // Method is already identified as event add accessor
            return false;
        }

        TypeData declaringTypeData = methodData.DeclaringTypeData;

        // TypeData.EnumerateProperties() uses caching internally to improve performance on repeated calls. Hence no need to cache here.
        // After caching enumeration becomes a simple O(1) operation and completely avoids any reflection.
        foreach (EventData eventData in declaringTypeData.EnumerateEvents())
        {
            MethodData? accessorMethod = isAddAccessor
                ? eventData.CanAdd
                    ? eventData.AddMethodData
                    : null
                : eventData.CanRemove
                    ? eventData.RemoveMethodData
                    : null;

            if (ReferenceEquals(accessorMethod, methodData))
            {
                methodData._accessedMember = eventData;
                return true;
            }
        }

        return false;
    }

    private static bool IsOperator(MethodData methodData) => methodData.IsSpecialName && methodData.Name.StartsWith(ReflectionConstants.OperatorMethodNamePrefix, StringComparison.Ordinal);

    private static bool IsDelegateInvoke(MethodData methodData) => methodData.IsSpecialName
        && methodData.DeclaringTypeData.IsDelegate
        && methodData.Name.Equals(ReflectionConstants.DelegateInvocatorMethodName, StringComparison.Ordinal);

    private static bool IsDelegateBeginInvoke(MethodData methodData) => methodData.IsSpecialName
        && methodData.DeclaringTypeData.IsDelegate
        && methodData.Name.Equals("BeginInvoke", StringComparison.Ordinal);

    private static bool IsDelegateEndInvoke(MethodData methodData) => methodData.IsSpecialName
        && methodData.DeclaringTypeData.IsDelegate
        && methodData.Name.Equals("EndInvoke", StringComparison.Ordinal);

    private static void InitializeDeclaringTypeAndExplicitImplementationContext(MethodData methodData)
    {
        if (TryGetExplicitInterfaceImplementationInfo(methodData, out TypeData? declaringInterfaceTypeData, out TypeData? implementingTypeData, out _))
        {
            methodData._isExplicitInterfaceImplementation = true;
            methodData._declaringTypeHandle = declaringInterfaceTypeData!.Handle;
            methodData._implementingTypeHandle = implementingTypeData!.Handle;
        }
        else
        {
            methodData._isExplicitInterfaceImplementation = false;
            methodData._declaringTypeHandle = methodData.MethodInfo.DeclaringType?.TypeHandle ?? throw new InvalidOperationException("Declaring type handle is not available.");
            methodData._implementingTypeHandle = methodData._declaringTypeHandle;
        }
    }

    private static bool TryGetExplicitInterfaceImplementationInfo(MethodData methodData, out TypeData? declaringInterfaceTypeData, out TypeData? implementingTypeData, out MethodData? interfaceDeclaration)
    {
        implementingTypeData = null;
        interfaceDeclaration = null;
        declaringInterfaceTypeData = null;

        TypeData implementingType = GetOrCreateCacheEntry(methodData.MethodInfo.DeclaringType ?? throw new InvalidOperationException("Declaring type is not available."));

        if (implementingType.IsInterface
            || methodData.IsPublic)
        {
            // Case 1: Explicit interface implementations cannot be declared directly on interfaces since they require an implementing type to implement the interface method explicitly.
            // Therefore, if the declaring type is an interface, we can directly return false without further checks.
            // Case 2: Explicit interface implementations must be non-public.
            // Therefore, if the method is public, it cannot be an explicit interface implementation and we can directly return false without further checks.

            return false;
        }

        TypeList implementedInterfaces = implementingType.InterfacesData;
        foreach (TypeData interfaceTypeData in implementedInterfaces)
        {
            Type interfaceType = interfaceTypeData.Type;

            InterfaceMappingEntry interfaceMapping = s_interfaceMappingTable.GetOrAdd(
                new InterfaceMappingKey(implementingType.Handle, interfaceTypeData.Handle),
                _ => InterfaceMappingEntryFactory(implementingType, interfaceTypeData));
            if (interfaceMapping.ImplementedMethodCacheKeyTable.TryGetValue(methodData, out int mappingIndex)
                && interfaceMapping.ReverseInterfaceMethodsCacheKeyTable.TryGetValue(mappingIndex, out MethodData interfaceMethod))
            {
                declaringInterfaceTypeData = interfaceMethod.DeclaringTypeData;
                interfaceDeclaration = interfaceMethod;
                implementingTypeData = implementingType;

                return true;
            }
        }

        return false;
    }

    private static InterfaceMappingEntry InterfaceMappingEntryFactory(TypeData declaringTypeData, TypeData interfaceTypeData)
    {
        InterfaceMapping mapping = declaringTypeData.Type.GetInterfaceMap(interfaceTypeData.Type);
        IEnumerable<MethodData> implementedMethodCacheKeys = mapping.TargetMethods.Select(GetOrCreateCacheEntry);
        IEnumerable<MethodData> interfaceMethodCacheKeys = mapping.InterfaceMethods.Select(GetOrCreateCacheEntry);

        return new InterfaceMappingEntry(implementedMethodCacheKeys, interfaceMethodCacheKeys);
    }

    /// <summary>
    /// Determines the set of symbol attributes for the specified closedGenericMethoMethodInfo based on its metadata and characteristics.
    /// </summary>
    /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
    /// <param name="methodData">The closedGenericMethoMethodInfo metadata used to evaluate and determine the applicable symbol attributes.</param>
    /// <returns>A bitwise combination of SymbolAttributes values that describe the closedGenericMethoMethodInfo's characteristics, such as whether
    /// it is static, abstract, virtual, final, override, or generic.</returns>
    private static SymbolAttributes GetAttributes(MethodData methodData)
    {
        SymbolAttributes methodAttributes = SymbolAttributes.Method;
        if (methodData.IsSealed)
        {
            methodAttributes |= SymbolAttributes.Final;
        }

        if (methodData.IsAbstract)
        {
            methodAttributes |= SymbolAttributes.Abstract;
        }

        if (methodData.IsStatic)
        {
            methodAttributes |= SymbolAttributes.Static;
        }

        if (methodData.IsVirtual)
        {
            methodAttributes |= SymbolAttributes.Virtual;
        }

        if (methodData.IsOverride)
        {
            methodAttributes |= SymbolAttributes.Override;
        }

        if (methodData.IsGenericMethod)
        {
            methodAttributes |= SymbolAttributes.Generic;
        }

        return methodAttributes;
    }

    private static bool IsMethodOverride(MethodData methodData)
    {
        MethodInfo methodInfo = methodData.MethodInfo;
        return !methodInfo.Equals(methodInfo.GetBaseDefinition());
    }

    private static AccessModifier GetAccessModifier(MethodData methodData) => methodData.IsPublic ? AccessModifier.Public
        : methodData.IsPrivate ? AccessModifier.Private
        : methodData.IsAssembly ? AccessModifier.Internal
        : methodData.IsFamily ? AccessModifier.Protected
        : methodData.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
        : methodData.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
        : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");

    #region IMethodDataInvoker

    bool IMethodDataInvoker.IsInvocable
        => !IsOpenGenericMethodOrGenericMethodDefinition
            && (_invoker is not null
            || _asyncTaskInvoker is not null
            || _asyncGenericTaskInvoker is not null
            || _asyncValueTaskInvoker is not null
            || _asyncGenericValueTaskInvoker is not null);

    void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, object?>? invocator) => _invoker = invocator;
    void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, Task>? asyncTaskInvocator) => _asyncTaskInvoker = asyncTaskInvocator;
    void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, Task<object?>>? asyncGenericTaskInvocator) => _asyncGenericTaskInvoker = asyncGenericTaskInvocator;
    void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, ValueTask>? asyncValueTaskInvocator) => _asyncValueTaskInvoker = asyncValueTaskInvocator;
    void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, ValueTask<object?>>? asyncGenericValueTaskInvocator) => _asyncGenericValueTaskInvoker = asyncGenericValueTaskInvocator;

    #endregion IMethodDataInvoker

    #region IStrictMethodDataInvoker

    bool IStrictMethodDataInvoker.IsInvocable(MethodDataGenericTypeVariantKey methodDataGenericTypeVariantKey) => !IsOpenGenericMethodOrGenericMethodDefinition
        && _invokerTable.ContainsKey(methodDataGenericTypeVariantKey);

    void IStrictMethodDataInvoker.SetInvoker(MethodDataGenericTypeVariantKey methodDataGenericTypeVariantKey, Delegate strictlyTypedInvoker)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDataGenericTypeVariantKey);
        ArgumentNullException.ThrowIfNull(strictlyTypedInvoker);

        // Store the invoker in a concurrent dictionary for later use
        _ = _invokerTable.TryAdd(methodDataGenericTypeVariantKey, strictlyTypedInvoker);
    }

    #endregion IStrictMethodDataInvoker

    #region InterfaceMappingKey
    private readonly struct InterfaceMappingKey : IEquatable<InterfaceMappingKey>
    {
        public InterfaceMappingKey(RuntimeTypeHandle implementingTypeHandle, RuntimeTypeHandle interfaceTypeHandle)
        {
            ImplementingTypeHandle = implementingTypeHandle;
            InterfaceTypeHandle = interfaceTypeHandle;
        }

        public RuntimeTypeHandle ImplementingTypeHandle { get; }
        public RuntimeTypeHandle InterfaceTypeHandle { get; }
        public bool Equals(InterfaceMappingKey other) => ImplementingTypeHandle.Equals(other.ImplementingTypeHandle) && InterfaceTypeHandle.Equals(other.InterfaceTypeHandle);
        public override bool Equals(object? obj) => obj is InterfaceMappingKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(ImplementingTypeHandle.GetHashCode(), InterfaceTypeHandle.GetHashCode());

        public static bool operator ==(InterfaceMappingKey left, InterfaceMappingKey right) => left.Equals(right);
        public static bool operator !=(InterfaceMappingKey left, InterfaceMappingKey right) => !(left == right);
    }
    #endregion InterfaceMappingKey

    #region InterfaceMappingEntry
    private readonly struct InterfaceMappingEntry : IEquatable<InterfaceMappingEntry>
    {
        public InterfaceMappingEntry(IEnumerable<MethodData> implementedMethodCacheKeys, IEnumerable<MethodData> interfaceMethodsCacheKeys)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(implementedMethodCacheKeys);
            ArgumentNullExceptionAdvanced.ThrowIfNull(interfaceMethodsCacheKeys);

            int index = 0;
            Dictionary<MethodData, int> implementedMethodCacheKeyDictionary = [];
            Dictionary<int, MethodData> reverseImplementedMethodCacheKeyDictionary = [];
            foreach (MethodData cacheKey in implementedMethodCacheKeys)
            {
                implementedMethodCacheKeyDictionary[cacheKey] = index;
                reverseImplementedMethodCacheKeyDictionary[index] = cacheKey;
                index++;
            }

            ImplementedMethodCacheKeyTable = implementedMethodCacheKeyDictionary.ToImmutableDictionary();
            ReverseImplementedMethodCacheKeyTable = reverseImplementedMethodCacheKeyDictionary.ToImmutableDictionary();

            index = 0;
            Dictionary<MethodData, int> interfaceMethodCacheKeyDictionary = [];
            Dictionary<int, MethodData> reverseInterfaceMethodCacheKeyDictionary = [];
            foreach (MethodData cacheKey in interfaceMethodsCacheKeys)
            {
                interfaceMethodCacheKeyDictionary[cacheKey] = index;
                reverseInterfaceMethodCacheKeyDictionary[index] = cacheKey;
                index++;
            }

            InterfaceMethodsCacheKeyTable = interfaceMethodCacheKeyDictionary.ToImmutableDictionary();
            ReverseInterfaceMethodsCacheKeyTable = reverseInterfaceMethodCacheKeyDictionary.ToImmutableDictionary();
        }

        public ImmutableDictionary<MethodData, int> ImplementedMethodCacheKeyTable { get; }
        public ImmutableDictionary<int, MethodData> ReverseImplementedMethodCacheKeyTable { get; }
        public ImmutableDictionary<MethodData, int> InterfaceMethodsCacheKeyTable { get; }
        public ImmutableDictionary<int, MethodData> ReverseInterfaceMethodsCacheKeyTable { get; }
        public bool Equals(InterfaceMappingEntry other) => ImplementedMethodCacheKeyTable.Equals(other.ImplementedMethodCacheKeyTable)
            && InterfaceMethodsCacheKeyTable.Equals(other.InterfaceMethodsCacheKeyTable)
            && ReverseImplementedMethodCacheKeyTable.Equals(other.ReverseImplementedMethodCacheKeyTable)
            && ReverseInterfaceMethodsCacheKeyTable.Equals(other.ReverseInterfaceMethodsCacheKeyTable);
        public override bool Equals(object? obj) => obj is InterfaceMappingEntry other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(
            ImplementedMethodCacheKeyTable,
            InterfaceMethodsCacheKeyTable,
            ReverseImplementedMethodCacheKeyTable,
            ReverseInterfaceMethodsCacheKeyTable);

        public static bool operator ==(InterfaceMappingEntry left, InterfaceMappingEntry right) => left.Equals(right);
        public static bool operator !=(InterfaceMappingEntry left, InterfaceMappingEntry right) => !(left == right);
    }
    #endregion InterfaceMappingEntry
}
