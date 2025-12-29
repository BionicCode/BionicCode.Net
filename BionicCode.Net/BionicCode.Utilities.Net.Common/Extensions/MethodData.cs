namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal sealed class MethodData : MemberInfoData, IMethodDataInvoker
    {
        private static readonly Type AsyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);

        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? isAwaitable;
        private bool? isAwaitableTask;
        private bool? isAwaitableValueTask;
        private bool? isAwaitableGenericValueTask;
        private bool? isAsync;
        private bool? isSealed;
        private bool? isExtensionMethod;
        private ParameterList? parameters;
        private TypeList? genericMethodArguments;
        private bool? isOverride;
        private bool? isStatic;
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
        private bool? _isAbstract;
        private bool? _isVirtual;
        private bool? _isPublic;
        private bool? _isPrivate;
        private bool? _isAssembly;
        private bool? _isFamily;
        private bool? _isFamilyOrAssembly;
        private bool? _isFamilyAndAssembly;

        public MethodData(MethodInfo methodInfo, SymbolInfoDataCacheKey symbolInfoDataCacheKey) : base(methodInfo, symbolInfoDataCacheKey)
        {
            ArgumentNullException.ThrowIfNull(methodInfo, nameof(methodInfo));

            this.Handle = methodInfo.MethodHandle;
        }

        public MethodInfo GetMethodInfo()
          => (MethodInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle)!;

        protected override MemberInfo GetMemberInfo()
          => GetMethodInfo();

        public MethodData MakeGenericMethodData(params TypeData[] typeDataArguments)
        {
            Type[] typeArguments = typeDataArguments.Select(t => t.UnwrapType()).ToArray();
            MethodInfo genericMethodInfo = GetMethodInfo().MakeGenericMethod(typeArguments);
            return SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericMethodInfo);
        }

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
        /// you must call the <see cref="InvokeOpenGeneric(object, IEnumerable{TypeData}, object[])"/> overload and provide the generic type parameter arguments.</remarks>
        public object? Invoke(object? target, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(Invoke), nameof(InvokeOpenGeneric));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            MethodData invocatorMethod = GetInvokerInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invoker!.Invoke(target, args);
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
        public object? InvokeOpenGeneric(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGeneric), nameof(Invoke));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invoker!.Invoke(target, args);
        }

        public async Task InvokeTaskAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeTaskAsync), nameof(InvokeOpenGenericTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            MethodData invocatorMethod = GetInvokerInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task InvokeOpenGenericTaskAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericTaskAsync), nameof(InvokeTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object?> InvokeTaskWithResultAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableGenericTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call {nameof(this.IsAwaitableGenericTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task<T>'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeTaskWithResultAsync), nameof(InvokeOpenGenericTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            MethodData invocatorMethod = GetInvokerInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object?> InvokeOpenGenericTaskWithResultAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericTaskWithResultAsync), nameof(InvokeTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask InvokeValueTaskAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeValueTaskAsync), nameof(InvokeOpenGenericValueTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            MethodData invocatorMethod = GetInvokerInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask InvokeOpenGenericValueTaskAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericValueTaskAsync), nameof(InvokeValueTaskAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask<object?> InvokeValueTaskWithResultAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableGenericValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask<T>'.");
            }

            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(InvokeValueTaskWithResultAsync), nameof(InvokeOpenGenericValueTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            MethodData invocatorMethod = GetInvokerInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask<object?> InvokeOpenGenericValueTaskWithResultAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(InvokeOpenGenericValueTaskWithResultAsync), nameof(InvokeValueTaskWithResultAsync));
            ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(target);
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericValueTaskInvoker!.Invoke(target, args).ConfigureAwait(false);
        }

        public MethodData GetInvoker(params object?[]? args)
        {
            ThrowIfAttemptingToInvokeGenericMethodLikeNonGenericMethod(nameof(GetInvoker), nameof(GetOpenGenericInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            MethodData invocatorMethod = GetInvokerInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        public MethodData GetOpenGenericInvoker(IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            ThrowIfAttemptingToInvokeNonGenericMethodLikeGenericMethod(nameof(GetOpenGenericInvoker), nameof(GetInvoker));
            ThrowIfDeclaringTypeIsAnOpenGenericType();
            ThrowIfInvalidMethodArguments(args);

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvokerInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        private void ThrowIfInvalidMethodArguments(object?[]? args)
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
                        ArgumentNullException.ThrowIfNull(args, nameof(args));

                        // Insufficient number of arguments provided for method invocation with 'params' parameter.
                        // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                        ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                    }
                }
                else
                {
                    // NULL is valid for 'args' if there is only a single non-params parameter.
                    // However, NULL is not valid for 'args' if there are more than a single non-params parameters.
                    if (this.Parameters.Count > 1)
                    {
                        ArgumentNullException.ThrowIfNull(args, nameof(args));
                    }

                    if (args is not null)
                    {
                        ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                    }
                }
            }
            else if (args is not null && args.Length > 0) // Method has no parameters but arguments were provided.
            {
                throw new ArgumentException("Method has no parameters but arguments were provided.", nameof(args));
            }
        }

        private void ThrowIfTargetIsNullOrTargetTypeIsNotMatchingDeclaringTypeForInstanceMember(object? target)
        {
            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));

                Type targetType = target.GetType();
                Type declaringType = this.DeclaringTypeData.UnwrapType();
                ArgumentExceptionEx.ThrowIfNotAssignableTo(
                    targetType,
                    declaringType,
                    nameof(target),
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            targetType,
                            "target type",
                            declaringType,
                            "declaring type"));
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
            if (!this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed using {nameOfWrongMethod}. Call {nameOfCorrectMethod} instead to ensure the generic method is properly closed by specifying the required generic type arguments.");
            }
        }

        private MethodData GetInvokerInternal(TypeData[] genericMethodParameters, params object?[]? args)
        {
            MethodData invocatorSource = ((IMethodDataInvoker)this).IsInvocable
                // 'this' is already a closed generic method with constructed invocator.
                // Reason: only closed generic methods can have invocator/are invocable...
                ? this

                // ...otherwise generate or get cached invocator
                : DelegateProvider.GetOrCreateFastMethodInvoker(this, genericMethodParameters);
            return invocatorSource;
        }

        public RuntimeMethodHandle Handle { get; }

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

        public ParameterList Parameters
          => this.parameters ??= new ParameterList(GetMethodInfo().GetParameters().Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray());

        public bool HasParamsParameter
          => this._hasParamsParameter ??= this.Parameters.HasItems && this.Parameters[^1].IsParams;

        public bool IsVoidMethod
          => this._isVoidMethod ??= this.ReturnTypeData.UnwrapType() == typeof(void);

        public TypeList GenericMethodArguments
        {
            get
            {
                if (this.genericMethodArguments is null)
                {
                    Type[] typeArguments = GetMethodInfo().GetGenericArguments();
                    this.genericMethodArguments = TypeListBuilder.Create(typeArguments);
                }

                return this.genericMethodArguments;
            }
        }

        public override AccessModifier AccessModifier => this.accessModifier is AccessModifier.Undefined
          ? (this.accessModifier = MethodData.GetAccessModifier(this))
          : this.accessModifier;

        public bool IsExtensionMethod
          => this.isExtensionMethod ??= MethodData.IsMethodExtensionMethod(this);

        public bool IsAsync
          => this.isAsync ??= IsMarkedAsync(this);

        public bool IsAwaitable
          => this.isAwaitable ??= this.ReturnTypeData.IsAwaitable;

        public bool IsAbstract
          => this._isAbstract ??= GetMethodInfo().IsAbstract;

        public bool IsVirtual
            => this._isVirtual ??= GetMethodInfo().IsVirtual;

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

        public override bool IsStatic
          => this.isStatic ??= GetMethodInfo().IsStatic;

        public bool IsSealed
          => this.isSealed ??= GetMethodInfo().IsFinal;

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

        bool IMethodDataInvoker.IsInvocable
            => !this.IsOpenGenericMethodOrGenericMethodDefinition
                && this._invoker is not null
                && this._asyncTaskInvoker is not null
                && this._asyncGenericTaskInvoker is not null
                && this._asyncValueTaskInvoker is not null
                && this._asyncGenericValueTaskInvoker is not null;

        public override bool IsPublic
            => this._isPublic ??= GetMethodInfo().IsPublic;

        public override bool IsPrivate
            => this._isPrivate ??= GetMethodInfo().IsPrivate;

        public override bool IsAssembly
            => this._isAssembly ??= GetMethodInfo().IsAssembly;

        public override bool IsFamily
            => this._isFamily ??= GetMethodInfo().IsFamily;

        public override bool IsFamilyOrAssembly
            => this._isFamilyOrAssembly ??= GetMethodInfo().IsFamilyOrAssembly;

        public override bool IsFamilyAndAssembly
            => this._isFamilyAndAssembly ??= GetMethodInfo().IsFamilyAndAssembly;

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

        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, object?>? invocator) => this._invoker = invocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, Task>? asyncTaskInvocator) => this._asyncTaskInvoker = asyncTaskInvocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, Task<object?>>? asyncGenericTaskInvocator) => this._asyncGenericTaskInvoker = asyncGenericTaskInvocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, ValueTask>? asyncValueTaskInvocator) => this._asyncValueTaskInvoker = asyncValueTaskInvocator;
        void IMethodDataInvoker.SetInvoker(Func<object?, object?[]?, ValueTask<object?>>? asyncGenericValueTaskInvocator) => this._asyncGenericValueTaskInvoker = asyncGenericValueTaskInvocator;

        # endregion IMethodDataInvoker
    }
}
