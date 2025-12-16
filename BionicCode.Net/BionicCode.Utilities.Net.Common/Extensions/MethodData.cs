namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal sealed class MethodData : MemberInfoData
    {
        private static readonly Type AsyncStateMachineAttributeType = typeof(AsyncStateMachineAttribute);
        private static readonly ConcurrentDictionary<InvocatorKeyMapKey, SymbolInfoDataCacheKey> InvocatorKeyMap = new ConcurrentDictionary<InvocatorKeyMapKey, SymbolInfoDataCacheKey>();

        private SymbolAttributes symbolAttributes;
        private AccessModifier accessModifier;
        private bool? isAwaitable;
        private bool? isAwaitableTask;
        private bool? isAwaitableValueTask;
        private bool? isAwaitableGenericValueTask;
        private bool? isAsync;
        private bool? isSealed;
        private bool? isExtensionMethod;
        private ParameterList parameters;
        private TypeData[] genericTypeArguments;
        private bool? isOverride;
        private bool? isStatic;
        private SymbolComponentInfo symbolComponentInfo;
        private string displayName;
        private string shortDisplayName;
        private string fullyQualifiedDisplayName;
        private string signature;
        private string shortSignature;
        private string fullyQualifiedRuntimeSignature;
        private string runtimeSignature;
        private string runtimeShortSignature;
        private string runtimeShortCompactSignature;
        private string shortCompactSignature;
        private string fullyQualifiedSignature;
        private TypeData returnTypeData;
        private bool? isGenericMethod;
        private bool? isGenericTypeMethod;
        private MethodData genericMethodDefinitionData;
        private bool? isReturnValueByRef;
        private Func<object?, object?[], object?>? _invocator;
        private Func<object, object[], Task>? awaitableTaskInvocator;
        private Func<object, object[], dynamic>? awaitableGenericValueTaskInvocator;
        private Func<object, object[], ValueTask>? awaitableValueTaskInvocator;
        private string assemblyName;
        private bool? isReturnValueReadOnly;
        private bool? containsGenericParameters;
        private bool? _hasParamsParameter;

        public MethodData(MethodInfo methodInfo) : base(methodInfo) => this.Handle = methodInfo.MethodHandle;

        public MethodInfo GetMethodInfo()
          => (MethodInfo)MethodInfo.GetMethodFromHandle(this.Handle, this.DeclaringTypeHandle);

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

        public object Invoke(object target, params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems && !this.Parameters[^1].IsParams)
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(this.Parameters.Count, args.Length, nameof(args));

            MethodData invocator = GetOrCreateFastInvocator(Array.Empty<TypeData>(), args);

            // REVIEW::Will this work with void methods?
            return invocator.Invoke(target, args);
        }

        public object InvokeGeneric(object target, IEnumerable<TypeData> genericMethodParameters, params object[] args)
        {
            if (!this.IsGenericMethod)
            {
                return Invoke(target, args);
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems && !this.Parameters[^1].IsParams)
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(this.Parameters.Count, args.Length, nameof(args));

            if (this._invocator is null)
            {
                GetOrCreateFastInvocator(genericMethodParameters.ToArray(), args);
            }

            // REVIEW::Will this work with void methods?
            return this._invocator!.Invoke(target, args);
        }

        public async Task InvokeAwaitableTaskAsync(object target, params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' or 'Task<T>' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable returns a 'Task' or 'Task<T>'.");
            }

            if (this.awaitableTaskInvocator is null)
            {
                InitializeAwaitableTaskInvocator(args);
            }

            await this.awaitableTaskInvocator.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object> InvokeAwaitableTaskWithResultAsync(object target, params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' or 'Task<T>' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable returns a 'Task' or 'Task<T>'.");
            }

            if (this.awaitableTaskInvocator is null)
            {
                InitializeAwaitableTaskInvocator(args);
            }

            Task task = this.awaitableTaskInvocator.Invoke(target, args);
            await task.ConfigureAwait(false);
            object result = null;
            if (this.ReturnTypeData.IsGenericType)
            {
                result = PropertyData.TaskResultPropertyData.Get(task);
            }

            return result;
        }

        public async Task InvokeAwaitableValueTaskAsync(object target, params object[] arguments)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, arguments.Length, nameof(arguments));

            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' or 'ValueTask<T>' object. Call {nameof(this.IsAwaitableValueTask)} or {nameof(this.IsAwaitableGenericValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask' or 'ValueTask<T>'.");
            }

            if (this.ReturnTypeData.IsGenericType)
            {
                if (this.awaitableGenericValueTaskInvocator is null)
                {
                    InitializeAwaitableGenericValueTaskInvocator(arguments);
                }

                await this.awaitableGenericValueTaskInvocator.Invoke(target, arguments);
            }
            else
            {
                if (this.awaitableValueTaskInvocator is null)
                {
                    InitializeAwaitableValueTaskInvocator(arguments);
                }

                await this.awaitableValueTaskInvocator.Invoke(target, arguments).ConfigureAwait(false);
            }
        }

        public async Task<object> InvokeAwaitableValueTaskWithResultAsync(object target, params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' or 'ValueTask<T>' object. Call {nameof(this.IsAwaitableValueTask)} or {nameof(this.IsAwaitableGenericValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask' or 'ValueTask<T>'.");
            }

            if (this.ReturnTypeData.IsGenericType)
            {
                if (this.awaitableGenericValueTaskInvocator is null)
                {
                    InitializeAwaitableGenericValueTaskInvocator();
                }

                dynamic task = this.awaitableGenericValueTaskInvocator.Invoke(target, args);
                await task;
                object result = PropertyData.ValueTaskResultPropertyData.Get(task);

                return result;
            }
            else
            {
                if (this.awaitableValueTaskInvocator is null)
                {
                    InitializeAwaitableValueTaskInvocator(args);
                }

                await this.awaitableValueTaskInvocator.Invoke(target, args).ConfigureAwait(false);
            }
        }

        public Func<object, object[], object> GetInvocator(params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (this._invocator is null)
            {
                InitializeInvocator(args);
            }

            return this._invocator;
        }

        public Func<object, object[], object> GetAwaitableGenericValueTaskInvocator(params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableGenericValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask<T>'.");
            }

            if (this.awaitableGenericValueTaskInvocator is null)
            {
                InitializeAwaitableGenericValueTaskInvocator();
            }

            return this.awaitableGenericValueTaskInvocator;
        }

        public Func<object, object[], ValueTask> GetAwaitableValueTaskInvocator(params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask'.");
            }

            if (this.awaitableValueTaskInvocator is null)
            {
                InitializeAwaitableValueTaskInvocator(args);
            }

            return this.awaitableValueTaskInvocator;
        }

        public Func<object, object[], Task> GetAwaitableTaskInvocator(params object[] args)
        {
            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task'.");
            }

            if (this.awaitableTaskInvocator is null)
            {
                InitializeAwaitableTaskInvocator(args);
            }

            return this.awaitableTaskInvocator;
        }

        public Func<object, object[], Task> CreateFastAsyncInvocator<TResult>(params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task'.");
            }

            if (this.awaitableTaskInvocator is null)
            {
                List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
                foreach (ParameterData parameter in this.Parameters)
                {
                    ParameterExpression parameterExpression = Expression.Parameter(parameter.ParameterTypeData.UnwrapType(), parameter.Name);
                    parameterExpressions.Add(parameterExpression);
                }

                MethodCallExpression expressionBody = Expression.Call(Expression.Parameter(typeof(object), "invocationTarget"), GetMethodInfo(), parameterExpressions);
                Expression<Func<object, object[], Task>> lambdaExpression = Expression.Lambda<Func<object, object[], Task>>(expressionBody, parameterExpressions);
                this.awaitableTaskInvocator = lambdaExpression.Compile();
            }

            return this.awaitableTaskInvocator;
        }

        private MethodData GetOrCreateFastInvocator(TypeData[] genericMethodParameters, params object[] args)
        {
            if (this._invocator is not null)
            {
                return this;
            }

            MethodData methodData = this;
            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                ArgumentExceptionEx.ThrowIfEnumerableIsNullOrEmpty(genericMethodParameters, nameof(genericMethodParameters), "Generic closedGenericMethoMethodInfo parameters must be provided to create a closed generic closedGenericMethoMethodInfo from a generic closedGenericMethoMethodInfo definition or a closedGenericMethoMethodInfo that contains generic parameters.");
                methodData = GetOrMakeClosedGenericMethodData(genericMethodParameters);
                if (methodData.HasConstructedInvocator)
                {
                    return methodData;
                }
            }

            List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]), "args");

            Expression? instance = null;
            if (!this.IsStatic)
            {
                instance = Expression.Convert(targetParam, this.DeclaringTypeData.UnwrapType()!);
            }

            UnaryExpression[] callArgs = this.Parameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                    parameter.ParameterTypeData.UnwrapType())).ToArray();

            MethodInfo methodInfo = methodData.GetMethodInfo();
            Expression call = this.IsStatic
                ? Expression.Call(methodInfo, callArgs)
                : Expression.Call(instance!, methodInfo, callArgs); // instance required for non-static :contentReference[oaicite:7]{index=7}

            Expression body = methodData.ReturnTypeData.UnwrapType() == typeof(void)
                ? Expression.Block(call, Expression.Constant(null, typeof(object)))
                : Expression.Convert(call, typeof(object));

            Func<object?, object?[], object?> invocator = Expression.Lambda<Func<object?, object?[], object?>>(body, targetParam, argsParam).Compile();
            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                methodData._invocator = invocator;
            }
            else
            {
                this._invocator = invocator;
            }

            return methodData;
        }

        private void InitializeInvocator(object[] args)
        {
            MethodData invocatorData = MakeClosedGenericMethodData(args);

            // REVIEW::Will this work when closedGenericMethoMethodInfo (current MethodData) is declared on an interface?
            // Probably requries instance for CreateDelegate() call for interface methods.
            this._invocator = invocatorData.GetMethodInfo().CreateDelegate<Func<object, object[], object>>();
        }

        private void InitializeAwaitableTaskInvocator(object[] args)
        {
            MethodData invocatorData = MakeClosedGenericMethodData(args);
            this.awaitableTaskInvocator = invocatorData.GetMethodInfo().CreateDelegate<Func<object, object[], Task>>();
        }

        private void InitializeAwaitableValueTaskInvocator(object[] args)
        {
            MethodData invocatorData = MakeClosedGenericMethodData(args);
            this.awaitableValueTaskInvocator = invocatorData.GetMethodInfo().CreateDelegate<Func<object, object[], ValueTask>>();
        }

        private void InitializeAwaitableGenericValueTaskInvocator()
          => this.awaitableGenericValueTaskInvocator = (invocationTarget, invocationArguments) => GetMethodInfo().Invoke(invocationTarget, invocationArguments);

        private MethodData GetOrMakeClosedGenericMethodData(TypeData[] genericMethodParameters)
        {
            MethodData? closedGenericMethodData = this;
            if (this.IsGenericMethodDefinition || this.ContainsGenericParameters)
            {
                var invocatorKey = InvocatorKeyMapKey.Create(genericMethodParameters);
                if (!MethodData.InvocatorKeyMap.TryGetValue(invocatorKey, out SymbolInfoDataCacheKey symbolInfoCachekey)
                    || !SymbolReflectionInfoCache.TryGetSymbolInfoDataCacheEntry(symbolInfoCachekey, out closedGenericMethodData))
                {
                    closedGenericMethodData = MakeGenericMethodData(genericMethodParameters);
                    MethodInfo closedGenericMethodMethodInfo = closedGenericMethodData.GetMethodInfo();
                    _ = MethodData.InvocatorKeyMap.TryAdd(invocatorKey, SymbolInfoDataCacheKey.CreateForMethod(closedGenericMethodMethodInfo));
                }
            }

            return closedGenericMethodData!;
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

        public TypeData[] GenericTypeArguments
        {
            get
            {
                if (this.genericTypeArguments is null)
                {
                    Type[] typeArguments = GetMethodInfo().GetGenericArguments();
                    this.genericTypeArguments = typeArguments.Select(SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry).ToArray();
                }

                return this.genericTypeArguments;
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

                return this.isAwaitableValueTask.Value;
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

        private bool HasConstructedInvocator
            => this._invocator is not null;

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
        /// <param name="methodData">The closedGenericMethoMethodInfo metadata used to evaluate and determine the applicable symbol attributes.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that describe the closedGenericMethoMethodInfo's characteristics, such as whether
        /// it is static, abstract, virtual, final, override, or generic.</returns>
        private static SymbolAttributes GetAttributes(MethodData methodData)
        {
            MethodInfo methodInfo = methodData.GetMethodInfo();
            SymbolAttributes methodAttributes = SymbolAttributes.Method;
            if (methodInfo.IsFinal)
            {
                methodAttributes |= SymbolAttributes.Final;
            }

            if (methodInfo.IsAbstract)
            {
                methodAttributes |= SymbolAttributes.Abstract;
            }

            if (methodData.IsStatic)
            {
                methodAttributes |= SymbolAttributes.Static;
            }

            if (methodInfo.IsVirtual)
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
            MethodInfo methodInfo = methodData.GetMethodInfo();
            return methodInfo.IsPublic ? AccessModifier.Public
              : methodInfo.IsPrivate ? AccessModifier.Private
              : methodInfo.IsAssembly ? AccessModifier.Internal
              : methodInfo.IsFamily ? AccessModifier.Protected
              : methodInfo.IsFamilyOrAssembly ? AccessModifier.ProtectedInternal
              : methodInfo.IsFamilyAndAssembly ? AccessModifier.PrivateProtected
              : throw new InvalidOperationException("Unable to identify the accessibility of the Types.");
        }

        private readonly struct InvocatorKeyMapKey : IEquatable<InvocatorKeyMapKey>
        {
            public ImmutableList<RuntimeTypeHandle> MethodParameterTypeHandles { get; }
            private readonly int? _hashCode;

            public InvocatorKeyMapKey(ImmutableList<RuntimeTypeHandle> methodParameterTypeHandles) : this()
            {
                ArgumentExceptionEx.ThrowIfEnumerableIsNullOrEmpty(methodParameterTypeHandles, nameof(methodParameterTypeHandles), "Method parameter type handles must be provided to create an invocator map key.");

                this.MethodParameterTypeHandles = methodParameterTypeHandles;
                this._hashCode = ComputeHashCode();
            }

            public static InvocatorKeyMapKey Create(IEnumerable<RuntimeTypeHandle> methodParameterTypeHandles)
                => new InvocatorKeyMapKey(methodParameterTypeHandles.ToImmutableList());

            public static InvocatorKeyMapKey Create(IEnumerable<TypeData> methodParameterTypeDatas)
                => new InvocatorKeyMapKey(methodParameterTypeDatas.Select(typeData => typeData.Handle).ToImmutableList());

            public bool Equals(InvocatorKeyMapKey other)
                => this.MethodParameterTypeHandles.SequenceEqual(other.MethodParameterTypeHandles);

            public override bool Equals([NotNullWhen(true)] object obj)
                => obj is InvocatorKeyMapKey invocatorKey && base.Equals(invocatorKey);

            public override int GetHashCode()
                => this._hashCode ?? ComputeHashCode();

            private int ComputeHashCode()
            {
                int hashCode = 1248511333;
                foreach (RuntimeTypeHandle typeHandle in this.MethodParameterTypeHandles)
                {
                    unchecked
                    {
                        int currentHash = typeHandle.GetHashCode();
                        hashCode = (hashCode * 397) ^ currentHash;
                    }
                }

                return hashCode;
            }

            public static bool operator ==(InvocatorKeyMapKey left, InvocatorKeyMapKey right) => left.Equals(right);
            public static bool operator !=(InvocatorKeyMapKey left, InvocatorKeyMapKey right) => !(left == right);
        }
    }
}
