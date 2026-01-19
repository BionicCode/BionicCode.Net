namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal sealed class MethodData : ParameterizedMemberData, IMethodDataInvoker, IStrictMethodDataInvoker
    {
        private static readonly Type AsyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);

        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? isAwaitable;
        private bool? isAwaitableTask;
        private bool? isAwaitableValueTask;
        private bool? isAwaitableGenericValueTask;
        private bool? isAsync;
        private bool? isExtensionMethod;
        private ParameterList? parameters;
        private TypeList? genericMethodArguments;
        private bool? isOverride;
        private SymbolComponentInfo? symbolComponentInfo;
        private string? displayName;
        private string? shortDisplayName;
        private string? fullyQualifiedDisplayName;
        private string? signature;
        private string? shortSignature;
        private string? fullyQualifiedRuntimeSignature;
        private string? runtimeSignature;
        private string? runtimeShortSignature;
        private string? runtimeShortCompactSignature;
        private string? shortCompactSignature;
        private string? fullyQualifiedSignature;
        private TypeData? returnTypeData;
        private bool? isGenericMethod;
        private bool? isGenericTypeMethod;
        private MethodData? genericMethodDefinitionData;
        private bool? isReturnValueByRef;
        private readonly ConcurrentDictionary<MethodDataGenericTypeVariantKey, Delegate> _invokerTable;
        private volatile Func<object?, object?[]?, object?>? _invoker;
        private volatile Func<object?, object?[]?, Task>? _asyncTaskInvoker;
        private volatile Func<object?, object?[]?, Task<object?>>? _asyncGenericTaskInvoker;
        private volatile Func<object?, object?[]?, ValueTask<object?>>? _asyncGenericValueTaskInvoker;
        private volatile Func<object?, object?[]?, ValueTask>? _asyncValueTaskInvoker;
        private string? assemblyName;
        private bool? isReturnValueReadOnly;
        private bool? containsGenericParameters;
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
        private BasicMethodFingerprint? _basicMethodFingerprint;

        public MethodData(MethodInfo methodInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(methodInfo, SymbolKind.MemberMethod, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

            this._invokerTable = new ConcurrentDictionary<MethodDataGenericTypeVariantKey, Delegate>();
            this.Handle = methodInfo.MethodHandle;
        }

        public MethodInfo GetMethodInfo()
          => (MethodInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle)!;

        protected override MemberInfo GetMemberInfo()
          => GetMethodInfo();

        public override MethodBase GetMethodBase()
            => GetMethodInfo();

        public MethodData MakeGenericMethodData(TypeList typeDataArguments)
        {
            Type[] typeArguments = typeDataArguments.Select(t => t.UnwrapType()).ToArray();
            MethodInfo genericMethodInfo = GetMethodInfo().MakeGenericMethod(typeArguments);
            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericMethodInfo);
        }

        public MethodData MakeGenericMethodData(params Type[] typeArguments)
        {
            MethodInfo genericMethodInfo = GetMethodInfo().MakeGenericMethod(typeArguments);
            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericMethodInfo);
        }

        public MethodInfo MakeGenericMethodInfo(TypeList typeArguments)
          => GetMethodInfo().MakeGenericMethod(typeArguments.Select(t => t.UnwrapType()).ToArray());

        public MethodInfo MakeGenericMethodInfo(params TypeData[] typeArguments)
          => GetMethodInfo().MakeGenericMethod(typeArguments.Select(t => t.UnwrapType()).ToArray());

        public MethodInfo MakeGenericMethodInfo(params Type[] typeArguments)
          => GetMethodInfo().MakeGenericMethod(typeArguments);

        /// <summary>
        /// Invokes the represented method on the specified target object using the provided arguments.
        /// </summary>
        /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
        /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
        /// method's parameters.</param>
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
        /// parameters, or if the method itself is a generic method definition or contains unassigned generic
        /// parameters.</exception>
        /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="ture"/>)
        /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, object?[]?)"/> overload and provide the generic type parameter arguments.</remarks>
        public object? Invoke(object? target, params object?[]? args)
            => Invoke(target, args.AsSpan());

        /// <summary>
        /// Invokes the represented method on the specified target object using the provided arguments.
        /// </summary>
        /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
        /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
        /// method's parameters.</param>
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
        /// parameters, or if the method itself is a generic method definition or contains unassigned generic
        /// parameters.</exception>
        /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="ture"/>)
        /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, ReadOnlySpan{object?})"/> overload and provide the generic type parameter arguments.</remarks>
        public object? Invoke(object? target, ReadOnlySpan<object?> args)
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
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
        /// parameters, or if the method itself is a generic method definition or contains unassigned generic
        /// parameters.</exception>
        /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="ture"/>)
        /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, object?[]?)"/> overload and provide the generic type parameter arguments.</remarks>
        public void Invoke<TTarget>(TTarget target, params object?[]? args)
            => Invoke<TTarget>(target, args.AsSpan());

        /// <summary>
        /// Invokes the represented void method on the specified target object using the provided arguments and returns the
        /// result.
        /// </summary>
        /// <remarks>This method performs validation to ensure that the invocation is valid for the
        /// method's signature and type constraints. Attempting to invoke an asynchronous or open generic method, or
        /// providing invalid arguments, will result in an exception. For methods with no return value, use a compatible
        /// <typeparamref name="TResult"/> type such as void or object.</remarks>
        /// <typeparam name="TTarget">The type of the object on which the method is invoked.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the invoked method.</typeparam>
        /// <param name="target">The instance of the target object on which to invoke the method. For static methods, this parameter is
        /// ignored.</param>
        /// <param name="args">A read-only span containing the arguments to pass to the method. The number, order, and types of arguments
        /// must match the method's parameters.</param>
        /// <returns>The result returned by the invoked method.</returns>
        public void Invoke<TTarget>(TTarget target, ReadOnlySpan<object?> args)
        {
            ThrowIfNonVoidMethodIsInvokedAsVoid();
            ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodVoidInvoker<TTarget> invokerMethod = (MethodVoidInvoker<TTarget>)GetInvokerInternal<TTarget>(TypeList.Empty);
            Debug.Assert(invokerMethod is not null);

            invokerMethod.Invoke(target, args.ToArray());
        }

        /// <summary>
        /// Invokes the represented method on the specified target object using the provided arguments.
        /// </summary>
        /// <param name="target">The object on which to invoke the method. For static methods, this parameter is ignored.</param>
        /// <param name="args">An array of arguments to pass to the method. The number, order, and type of the arguments must match the
        /// method's parameters.</param>
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the method is declared on a generic type definition or a type containing unassigned generic
        /// parameters, or if the method itself is a generic method definition or contains unassigned generic
        /// parameters.</exception>
        /// <remarks>Note: For a generic method that is not closed (<see cref="IsGenericMethodDefinition"/> or <see cref="ContainsGenericParameters"/> returns <see langword="ture"/>)
        /// you must call the <see cref="InvokeOpenGeneric(object?, TypeList, object?[]?)"/> overload and provide the generic type parameter arguments.</remarks>
        public TResult Invoke<TTarget, TResult>(TTarget target, params object?[]? args)
            => Invoke<TTarget, TResult>(target, args.AsSpan());

        /// <summary>
        /// Invokes the represented method on the specified target object using the provided arguments and returns the
        /// result.
        /// </summary>
        /// <remarks>This method performs validation to ensure that the invocation is valid for the
        /// method's signature and type constraints. Attempting to invoke an asynchronous or open generic method, or
        /// providing invalid arguments, will result in an exception. For methods with no return value, use a compatible
        /// <typeparamref name="TResult"/> type such as void or object.</remarks>
        /// <typeparam name="TTarget">The type of the object on which the method is invoked.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the invoked method.</typeparam>
        /// <param name="target">The instance of the target object on which to invoke the method. For static methods, this parameter is
        /// ignored.</param>
        /// <param name="args">A read-only span containing the arguments to pass to the method. The number, order, and types of arguments
        /// must match the method's parameters.</param>
        /// <returns>The result returned by the invoked method.</returns>
        public TResult Invoke<TTarget, TResult>(TTarget target, ReadOnlySpan<object?> args)
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

        public async Task InvokeAwaitableTaskAndDiscardResultAsync<TTarget>(TTarget target, params object?[] args)
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

        public async ValueTask InvokeAwaitableValueTaskAndDiscardResultAsync<TTarget>(TTarget target, params object?[] args)
        {
            ThrowIfVoidMethodIsInvokedAsNonVoid();
            ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            // TResult will be ignored by the 'DelegateProvider' since isDiscard is TRUE.
            // Hence, 'ValueTask' is acting as a dummy generic type parameter in this special case.
            MethodAwaitableValueTaskDiscardInvoker<TTarget>? invokerMethod = (MethodAwaitableValueTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, ValueTask>(TypeList.Empty, isDiscard: true);
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
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the declaring type is a generic type definition or contains unassigned generic parameters, or if
        /// the method itself is not a closed generic method.</exception>
        public object? InvokeOpenGeneric(object? target, TypeList genericMethodParameters, params object?[]? args)
            => InvokeOpenGeneric(target, genericMethodParameters, args.AsSpan());

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
        /// <returns>The return value of the invoked method, or null if the method has no return value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the declaring type is a generic type definition or contains unassigned generic parameters, or if
        /// the method itself is not a closed generic method.</exception>
        public object? InvokeOpenGeneric(object? target, TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGeneric), nameof(Invoke));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invoker!.Invoke(target, args.ToArray());
        }

        public async Task InvokeAwaitableTaskAsync(object? target, params object?[] args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call '{nameof(this.IsAwaitableTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableTaskAsync), nameof(InvokeOpenGenericAwaitableTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task InvokeOpenGenericAwaitableTaskAsync(object? target, TypeList genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call '{nameof(this.IsAwaitableTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task'.");
            }

            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericAwaitableTaskAsync), nameof(InvokeAwaitableTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object?> InvokeAwaitableTaskWithResultAsync(object? target, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableGenericTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call '{nameof(this.IsAwaitableGenericTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task<T>'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableTaskWithResultAsync), nameof(InvokeOpenGenericAwaitableTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object?> InvokeOpenGenericAwaitableTaskWithResultAsync(object? target, TypeList genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableGenericTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call '{nameof(this.IsAwaitableGenericTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'Task<T>'.");
            }

            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericAwaitableTaskWithResultAsync), nameof(InvokeAwaitableTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask InvokeAwaitableValueTaskAsync(object? target, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call '{nameof(this.IsAwaitableValueTask)}' to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableValueTaskAsync), nameof(InvokeOpenGenericAwaitableValueTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask InvokeOpenGenericAwaitableValueTaskAsync(object? target, TypeList genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask'.");
            }

            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericAwaitableValueTaskAsync), nameof(InvokeAwaitableValueTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask<object?> InvokeAwaitableValueTaskWithResultAsync(object? target, params object?[] args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableGenericValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask<T>'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeAwaitableValueTaskWithResultAsync), nameof(InvokeOpenGenericValueAwaitableTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask<object?> InvokeOpenGenericValueAwaitableTaskWithResultAsync(object? target, TypeList genericMethodParameters, params object?[] args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the current '{nameof(MethodData)}' is awaitable and returns a 'ValueTask<T>'.");
            }

            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericValueAwaitableTaskWithResultAsync), nameof(InvokeAwaitableValueTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public Func<object?, object?[]?, object?> GetInvoker(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetInvoker), nameof(GetOpenGenericInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invoker!;
        }

        public Func<object?, object?[]?, object?> GetOpenGenericInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericInvoker), nameof(GetInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invoker!;
        }

        public Func<object?, object?[]?, Task> GetAwaitableTaskInvoker(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableTaskInvoker), nameof(GetOpenGenericAwaitableTaskInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncTaskInvoker!;
        }

        public Func<object?, object?[]?, Task> GetOpenGenericAwaitableTaskInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskInvoker), nameof(GetAwaitableTaskInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncTaskInvoker!;
        }

        public Func<object?, object?[]?, Task<object?>> GetAwaitableTaskWithResultInvoker(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableTaskWithResultInvoker), nameof(GetOpenGenericAwaitableTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncGenericTaskInvoker!;
        }

        public Func<object?, object?[]?, Task<object?>> GetOpenGenericAwaitableTaskWithResultInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncGenericTaskInvoker!;
        }

        public Func<object?, object?[]?, ValueTask> GetAwaitableValueTaskInvoker(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableValueTaskInvoker), nameof(GetOpenGenericAwaitableValueTaskInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncValueTaskInvoker!;
        }

        public Func<object?, object?[]?, ValueTask> GetOpenGenericAwaitableValueTaskInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableValueTaskInvoker), nameof(GetAwaitableValueTaskInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncValueTaskInvoker!;
        }

        public Func<object?, object?[]?, ValueTask<object?>> GetAwaitableValueTaskWithResultInvoker(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableValueTaskWithResultInvoker), nameof(GetOpenGenericAwaitableValueTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            MethodData invocatorMethod = GetInvokerInternal(TypeList.Empty);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncGenericValueTaskInvoker!;
        }

        public Func<object?, object?[]?, ValueTask<object?>> GetOpenGenericAwaitableValueTaskWithResultInvoker(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableValueTaskWithResultInvoker), nameof(GetAwaitableValueTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodParameters);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._asyncGenericValueTaskInvoker!;
        }

        public MethodAwaitableTaskDiscardInvoker<TTarget> GetAwaitableTaskDiscardInvoker<TTarget>(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            var invocatorMethod = (MethodAwaitableTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, Task>(TypeList.Empty, isDiscard: true);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        public MethodAwaitableTaskDiscardInvoker<TTarget> GetOpenGenericAwaitableTaskDiscardInvoker<TTarget>(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            var invocatorMethod = (MethodAwaitableTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, Task>(genericMethodParameters, isDiscard: true);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        public MethodAwaitableValueTaskDiscardInvoker<TTarget> GetAwaitableValueTaskDiscardInvoker<TTarget>(ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetAwaitableValueTaskInvoker), nameof(GetOpenGenericAwaitableValueTaskInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            var invocatorMethod = (MethodAwaitableValueTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, ValueTask>(TypeList.Empty, isDiscard: true);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod!;
        }

        public MethodAwaitableValueTaskDiscardInvoker<TTarget> GetOpenGenericAwaitableValueTaskDiscardInvoker<TTarget>(TypeList genericMethodParameters, ReadOnlySpan<object?> args)
        {
            ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously();
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericAwaitableTaskWithResultInvoker), nameof(GetAwaitableTaskWithResultInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args, nameof(args));

            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodParameters.Count, this.GenericMethodParameters.Count, nameof(genericMethodParameters));

            var invocatorMethod = (MethodAwaitableValueTaskDiscardInvoker<TTarget>)GetInvokerInternal<TTarget, ValueTask<object>>(genericMethodParameters, isDiscard: true);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        private void ThrowIfInvalidMethodArguments(ReadOnlySpan<object?> args, string paramName)
        {
            // Validate arguments against method parameters
            if (this.Parameters.HasItems)
            {
                if (this.HasParamsParameter)
                {
                    // NULL is valid for 'args' if there is only a single non-params parameter since params can be empty.
                    // Additionally, no need to check 'args' for NULL if there is only the params parameter.
                    // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                    if (this.Parameters.Count > 2)
                    {
                        // Insufficient number of arguments provided for method invocation with 'params' parameter.
                        // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                            args.Length,
                            this.Parameters.Count - 1,
                            paramName,
                            $"Parameter count mismatch. The number of method arguments provided by the argument list {paramName} does not match the method's signature. Expected: '{this.Parameters.Count - 1}' mandatory and optional 'params' arguments. Found: '{args.Length}' arguments.");
                    }
                }
                else
                {
                    ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                        args.Length,
                        this.Parameters.Count,
                        paramName,
                        $"Parameter count mismatch. The number of method arguments provided by the argument list {paramName} does not match the method's signature. Expected: '{this.Parameters.Count}' arguments. Found: '{args.Length}' arguments.");
                }
            }
            else if (!args.IsEmpty) // Method has no parameters but arguments were provided.
            {
                throw new ArgumentOutOfRangeExceptionAdvanced(
                    paramName,
                    $"Parameter count mismatch. The number of method arguments provided by the argument list {paramName} does not match the method's signature. Expected: '{this.Parameters.Count}' arguments. Found: '{args.Length}' arguments.");
            }
        }

        private void ThrowIfAttemptingToInvokeSynchronousMethodAsynchronously()
        {
            if (!this.IsAsync)
            {
                throw new InvalidOperationException("Cannot invoke synchronous methods using asynchronous invocation methods. Call the synchronous invocation methods instead.");
            }
        }

        private void ThrowIfAttemptingToInvokeAsynchronousMethodSynchronously()
        {
            if (this.IsAsync)
            {
                throw new InvalidOperationException("Cannot invoke asynchronous methods using synchronous invocation methods. Call the asynchronous invocation methods instead.");
            }
        }

        private void ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(object? target)
        {
            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));

                Type targetType = target.GetType();
                Type declaringType = this.DeclaringTypeData.UnwrapType();
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
            if (!this.IsVoidMethod)
            {
                throw new InvalidOperationException($"The method '{this.Name}' does not have a void return type. Use the '{nameof(Invoke)}<TTarget, TResult>()' overload to invoke methods with a return value.");
            }
        }

        private void ThrowIfVoidMethodIsInvokedAsNonVoid()
        {
            if (this.IsVoidMethod)
            {
                throw new InvalidOperationException($"The method '{this.Name}' has a void return type. Use the '{nameof(Invoke)}<TTarge>()' overload to invoke the void method.");
            }
        }

        private void ThrowIfDeclaringTypeIsAnOpenGenericType()
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic type parameters. Ensure the declaring generic type is properly closed.");
            }
        }

        private void ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(string nameOfWrongMethod, string nameOfCorrectMethod)
        {
            if (!this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                throw new InvalidOperationException($"Cannot invoke non-generic methods using '{nameOfWrongMethod}'. Call '{nameOfCorrectMethod}' instead.");
            }
        }

        private void ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(string nameOfWrongMethod, string nameOfCorrectMethod)
        {
            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
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
            MethodDataGenericTypeVariantKey genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
                genericMethodArguments,
                resultType.TypeHandle,
                targetType.TypeHandle,
                this.BasicMethodFingerprint);

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
                this.BasicMethodFingerprint);

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

        public override RuntimeMethodHandle Handle { get; }

        /// <summary>
        /// Provides a basic fingerprint for this method based on its name, declaring type, return type, parameters and generic method parameters (if the method is a generic method).
        /// </summary>
        /// <remarks>The <see cref="BasicMethodFingerprint"/> allows to identify methods based on the least required attributes.
        /// This allows to identify methods that belong to the same generic family (based on the open generic method signature).<br/>
        /// For example, the open generic method would share the same basic fingerprint like all  it's closed generic variants.<br/>
        /// In other words, the <see cref="BasicMethodFingerprint"/> enables the identification of generic methods that share the same generic structure since it is based on the raw unconstructed generic method signature.</remarks>
        public BasicMethodFingerprint BasicMethodFingerprint
        {
            get
            {
                if (this._basicMethodFingerprint is null)
                {
                    if (this.IsGenericMethod && !this.IsGenericMethodDefinition)
                    {
                        MethodInfo genericMethodDefinition = GetMethodInfo().GetGenericMethodDefinition();
                        MethodData genericMethodDefinitionData = genericMethodDefinition.ToMethodData();
                        this._basicMethodFingerprint = genericMethodDefinitionData.BasicMethodFingerprint;
                    }
                    else
                    {
                        this._basicMethodFingerprint ??= new BasicMethodFingerprint(
                            this.Name,
                            this.DeclaringTypeHandle,
                            this.ReturnTypeData.Handle,
                            this.Parameters,
                            this.GenericMethodParameters);
                    }
                }

                return (BasicMethodFingerprint)this._basicMethodFingerprint;
            }
        }

        public MethodData GenericMethodDefinitionData
        {
            get
            {
                if (this.IsGenericMethodDefinition)
                {
                    return this;
                }
                else
                {
                    MethodInfo genericMethodDefinitionMethodInfo = GetMethodInfo().GetGenericMethodDefinition();
                    this.genericMethodDefinitionData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericMethodDefinitionMethodInfo);
                }

                return this.genericMethodDefinitionData;
            }
        }

        public override ParameterList Parameters
          => this.parameters ??= ParameterListBuilder.Create(this);

        public override bool HasParamsParameter
          => this._hasParamsParameter ??= this.Parameters.HasItems && this.Parameters[^1].IsParams;

        /// <summary>
        /// Checks whether the method is a property set method. Will not include indexer set methods.<br/>
        /// Use <see cref="IsIndexerPropertySetMethod"/> to specifically check for indexer set methods and exclude normal properties.
        /// </summary>
        public bool IsPropertySetMethod
            => this._isPropertySetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: false, isSetter: true, isValidationEnabled: false);

        /// <summary>
        /// Checks whether the method is a property set method. Will not include indexer set methods.<br/>
        /// Use <see cref="IsIndexerPropertyGetMethod"/> to specifically check for indexer get methods and exclude normal properties.
        /// </summary>
        public bool IsPropertyGetMethod
            => this._isPropertyGetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: false, isSetter: false, isValidationEnabled: false);

        /// <summary>
        /// Checks whether the method is an indexer property set method.
        /// </summary>
        public bool IsIndexerPropertySetMethod
            => this._isIndexerPropertySetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: true, isSetter: true, isValidationEnabled: false);

        /// <summary>
        /// Checks whether the method is an indexer property get method.
        /// </summary>
        public bool IsIndexerPropertyGetMethod
            => this._isIndexerPropertyGetMethod ??= MethodData.IsPropertyAccessor(this, isIndexer: true, isSetter: false, isValidationEnabled: false);

        public bool IsDelegateInvokeMethod
            => this._isDelegateInvokeMethod ??= MethodData.IsDelegateInvoke(this);

        public bool IsDelegateBeginInvokeMethod
            => this._isDelegateBeginInvokeMethod ??= MethodData.IsDelegateBeginInvoke(this);

        public bool IsDelegateEndInvokeMethod
            => this._isDelegateEndInvokeMethod ??= MethodData.IsDelegateEndInvoke(this);

        public bool IsDelegateMethod
            => this.IsDelegateInvokeMethod || this.IsDelegateBeginInvokeMethod || this.IsDelegateEndInvokeMethod;

        public bool IsEventAddMethod
            => this._isEventAddMethod ??= MethodData.IsEventAccessor(this, isAddAccessor: true, isValidationEnabled: false);

        public bool IsEventRemoveMethod
            => this._isEventRemoveMethod ??= MethodData.IsEventAccessor(this, isAddAccessor: false, isValidationEnabled: false);

        public bool IsEventAccessorMethod
            => this._isEventAccessorMethod ??= this.IsEventAddMethod || this.IsEventRemoveMethod;

        public bool IsOperatorOverload
            => this._isOperatorOverload ??= MethodData.IsOperator(this);

        public bool IsVoidMethod
          => this._isVoidMethod ??= this.ReturnTypeData.UnwrapType() == typeof(void);

        public TypeList GenericMethodParameters
            => this.genericMethodArguments ??= TypeListBuilder.CreateGenericTypeArgumentList(this);

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = MethodData.GetAccessModifier(this))
          : this.accessModifier;

        public bool IsExtensionMethod
          => this.isExtensionMethod ??= MethodData.IsMethodExtensionMethod(this);

        public bool IsAsync
          => this.isAsync ??= IsMarkedAsync(this);

        public bool IsAwaitable
          => this.isAwaitable ??= this.ReturnTypeData.IsAwaitable;

        public bool IsAwaitableTask
        {
            get
            {
                if (this.isAwaitableTask is null)
                {
                    if (!this.IsAwaitable)
                    {
                        this.isAwaitableTask = false;
                    }
                    else
                    {
                        this.isAwaitableTask = (this.isAwaitableValueTask.HasValue
                          && !this.isAwaitableValueTask.Value
                          && this.ReturnTypeData.IsAwaitableTask)
                          || (!this.isAwaitableValueTask.HasValue && this.ReturnTypeData.IsAwaitableTask);
                    }
                }

                return this.isAwaitableTask.Value;
            }
        }

        public bool IsAwaitableValueTask
        {
            get
            {
                if (this.isAwaitableValueTask is null)
                {
                    if (!this.IsAwaitable || this.ReturnTypeData.IsGenericType)
                    {
                        this.isAwaitableValueTask = false;
                    }
                    else
                    {
                        this.isAwaitableValueTask = (this.isAwaitableTask.HasValue
                          && !this.isAwaitableTask.Value
                          && this.ReturnTypeData.IsAwaitableValueTask)
                          || (!this.isAwaitableTask.HasValue && this.ReturnTypeData.IsAwaitableValueTask);
                    }
                }

                return this.isAwaitableValueTask.Value;
            }
        }

        public bool IsAwaitableGenericValueTask
        {
            get
            {
                if (this.isAwaitableGenericValueTask is null)
                {
                    if (!this.IsAwaitable || !this.ReturnTypeData.IsGenericType)
                    {
                        this.isAwaitableGenericValueTask = false;
                    }
                    else
                    {
                        this.isAwaitableGenericValueTask = (this.isAwaitableTask.HasValue
                          && !this.isAwaitableTask.Value
                          && this.ReturnTypeData.IsAwaitableValueTask)
                          || (!this.isAwaitableTask.HasValue && this.ReturnTypeData.IsAwaitableValueTask);
                    }
                }

                return this.isAwaitableGenericValueTask.Value;
            }
        }

        public bool IsAwaitableGenericTask
        {
            get
            {
                if (this._isAwaitableGenericTask is null)
                {
                    if (!this.IsAwaitable || !this.ReturnTypeData.IsGenericType)
                    {
                        this._isAwaitableGenericTask = false;
                    }
                    else
                    {
                        this._isAwaitableGenericTask = (this.isAwaitableTask.HasValue
                          && !this.isAwaitableTask.Value
                          && this.ReturnTypeData.IsAwaitableTask)
                          || (!this.isAwaitableTask.HasValue && this.ReturnTypeData.IsAwaitableTask);
                    }
                }

                return this._isAwaitableGenericTask.Value;
            }
        }

        public bool IsOverride
          => this.isOverride ??= MethodData.IsMethodOverride(this);

        public bool IsReturnValueReadOnly
          => this.isReturnValueReadOnly ??= GetMethodInfo().ReturnParameter.GetCustomAttribute<IsReadOnlyAttribute>() != null;

        public bool IsReturnValueByRef
          => this.isReturnValueByRef ??= this.ReturnTypeData.IsByRef;

        public override SymbolAttributes SymbolAttributes => this.symbolAttributes is SymbolAttributes.Undefined
          ? (this.symbolAttributes = MethodData.GetAttributes(this))
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
          => this.fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

        public override string AssemblyName
          => this.assemblyName ??= this.DeclaringTypeData.AssemblyName;

        public TypeData ReturnTypeData
          => this.returnTypeData ??= SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(GetMethodInfo().ReturnType);

        public bool IsGenericMethod
          => this.isGenericMethod ??= GetMethodInfo().IsGenericMethod;

        public bool IsGenericMethodDefinition
          => this.isGenericTypeMethod ??= GetMethodInfo().IsGenericMethodDefinition;

        public bool ContainsGenericParameters
          => this.containsGenericParameters ??= GetMethodInfo().ContainsGenericParameters;

        public bool IsOpenGenericMethodOrGenericMethodDefinition
          => (this.IsGenericMethod && this.ContainsGenericParameters) || this.IsGenericMethodDefinition;

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

            MethodInfo methodInfo = methodData.GetMethodInfo();
            Attribute methodExtensionAttribute = methodInfo.GetCustomAttribute(HelperExtensionsCommon.ExtensionAttributeType, false);
            if (methodExtensionAttribute == null)
            {
                return false;
            }

            // Must have at least the 'this' parameter
            ParameterList parameterInfoData = methodData.Parameters;
            if (parameterInfoData.Count < 1)
            {
                return false;
            }

            return true;
        }

        private static bool IsMarkedAsync(MethodData methodData)
          => methodData.GetMethodInfo().GetCustomAttribute(MethodData.AsyncStateMachineAttributeType) != null;

        private static bool IsPropertyAccessor(MethodData methodData, bool isIndexer, bool isSetter, bool isValidationEnabled)
        {
            if (!methodData.IsSpecialName)
            {
                return false;
            }

            ParameterList parameters = methodData.Parameters;
            bool isLookingLikeIndexer;
            if (isIndexer)
            {
                isLookingLikeIndexer = isSetter
                ? methodData.Name.StartsWith("set_", StringComparison.Ordinal) && parameters.Count >= 2 // at least one index parameter + "value" parameter
                : methodData.Name.StartsWith("get_", StringComparison.Ordinal) && parameters.Count >= 1; // at least one index parameter
            }
            else
            {
                isLookingLikeIndexer = isSetter
                ? methodData.Name.StartsWith("set_", StringComparison.Ordinal) && parameters.Count == 1 // only "value" parameter
                : methodData.Name.StartsWith("get_", StringComparison.Ordinal) && parameters.Count == 0; // no parameters
            }

            if (!isValidationEnabled)
            {
                return isLookingLikeIndexer;
            }

            if (!isLookingLikeIndexer)
            {
                return false;
            }

            TypeData declaringTypeData = methodData.DeclaringTypeData;
            foreach (PropertyData propertyData in declaringTypeData.EnumerateProperties())
            {
                if (!propertyData.IsIndexer)
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
                    return true;
                }
            }

            return false;
        }

        private static bool IsEventAccessor(MethodData methodData, bool isAddAccessor, bool isValidationEnabled = false)
        {
            return methodData.IsSpecialName &&
                methodData.Name.StartsWith("add_", StringComparison.Ordinal);

            if (!methodData.IsSpecialName)
            {
                return false;
            }

            ParameterList parameters = methodData.Parameters;
            bool isLookingLikeEventAccessor = isAddAccessor
                ? methodData.Name.StartsWith("add_", StringComparison.Ordinal) && parameters.Count == 1 // at least one index parameter + "value" parameter
                : methodData.Name.StartsWith("remove_", StringComparison.Ordinal) && parameters.Count == 1; // at least one index parameter


            if (!isValidationEnabled)
            {
                return isLookingLikeEventAccessor;
            }

            if (!isLookingLikeEventAccessor)
            {
                return false;
            }

            TypeData declaringTypeData = methodData.DeclaringTypeData;
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
                    return true;
                }
            }

            return false;
        }

        private static bool IsOperator(MethodData methodData)
            => methodData.IsSpecialName && methodData.Name.StartsWith("op_", StringComparison.Ordinal);

        private static bool IsDelegateInvoke(MethodData methodData)
            => methodData.IsSpecialName
                && methodData.DeclaringTypeData.IsDelegate
                && methodData.Name.Equals("Invoke", StringComparison.Ordinal);

        private static bool IsDelegateBeginInvoke(MethodData methodData)
            => methodData.IsSpecialName
                && methodData.DeclaringTypeData.IsDelegate
                && methodData.Name.Equals("BeginInvoke", StringComparison.Ordinal);

        private static bool IsDelegateEndInvoke(MethodData methodData)
            => methodData.IsSpecialName
                && methodData.DeclaringTypeData.IsDelegate
                && methodData.Name.Equals("EndInvoke", StringComparison.Ordinal);


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
            MethodInfo methodInfo = methodData.GetMethodInfo();
            return !methodInfo.Equals(methodInfo.GetBaseDefinition());
        }

        private static AccessModifier GetAccessModifier(MethodData methodData)
        {
            return methodData.IsPublic ? AccessModifier.Public
              : methodData.IsPrivate ? AccessModifier.Private
              : methodData.IsAssembly ? AccessModifier.Internal
              : methodData.IsFamily ? AccessModifier.Protected
              : methodData.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
              : methodData.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        }

        #region IMethodDataInvoker

        bool IMethodDataInvoker.IsInvocable
            => !this.IsOpenGenericMethodOrGenericMethodDefinition
                && (this._invoker is not null
                || this._asyncTaskInvoker is not null
                || this._asyncGenericTaskInvoker is not null
                || this._asyncValueTaskInvoker is not null
                || this._asyncGenericValueTaskInvoker is not null);

        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, object?>? invocator) => this._invoker = invocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, Task>? asyncTaskInvocator) => this._asyncTaskInvoker = asyncTaskInvocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, Task<object?>>? asyncGenericTaskInvocator) => this._asyncGenericTaskInvoker = asyncGenericTaskInvocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, ValueTask>? asyncValueTaskInvocator) => this._asyncValueTaskInvoker = asyncValueTaskInvocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, ValueTask<object?>>? asyncGenericValueTaskInvocator) => this._asyncGenericValueTaskInvoker = asyncGenericValueTaskInvocator;

        #endregion IMethodDataInvoker

        #region IStrictMethodDataInvoker

        bool IStrictMethodDataInvoker.IsInvocable(MethodDataGenericTypeVariantKey methodDataGenericTypeVariantKey)
            => !this.IsOpenGenericMethodOrGenericMethodDefinition
                && this._invokerTable.ContainsKey(methodDataGenericTypeVariantKey);

        void IStrictMethodDataInvoker.SetInvoker(MethodDataGenericTypeVariantKey methodDataGenericTypeVariantKey, Delegate strictlyTypedInvoker)
        {
            ArgumentNullExceptionAdvanced.ThrowIfDefault(methodDataGenericTypeVariantKey);
            ArgumentNullException.ThrowIfNull(strictlyTypedInvoker);

            // Store the invoker in a concurrent dictionary for later use
            _ = this._invokerTable.TryAdd(methodDataGenericTypeVariantKey, strictlyTypedInvoker);
        }

        #endregion IStrictMethodDataInvoker
    }
}
