namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
        private static readonly ConcurrentDictionary<InvocatorKey, Delegate> InvocatorCache = new ConcurrentDictionary<InvocatorKey, Delegate>();

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
        private Func<object, object[], object>? invocator;
        private Func<object, object[], Task> awaitableTaskInvocator;
        private Func<object, object[], dynamic> awaitableGenericValueTaskInvocator;
        private Func<object, object[], ValueTask> awaitableValueTaskInvocator;
        private string assemblyName;
        private bool? isReturnValueReadOnly;

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
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (this.invocator is null)
            {
                InitializeInvocator(args);
            }

            // REVIEW::Will this work with void methods?
            return this.invocator.Invoke(target, args);
        }

        public async Task InvokeAwaitableTaskAsync(object target, params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' or 'Task<T>' object. Call {nameof(this.IsAwaitableTask)} to ensure the method is awaitable returns a 'Task' or 'Task<T>'.");
            }

            if (this.awaitableTaskInvocator is null)
            {
                InitializeAwaitableTaskInvocator(args);
            }

            await this.awaitableTaskInvocator.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object> InvokeAwaitableTaskWithResultAsync(object target, params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' or 'Task<T>' object. Call {nameof(this.IsAwaitableTask)} to ensure the method is awaitable returns a 'Task' or 'Task<T>'.");
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
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, arguments.Length, nameof(arguments));

            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' or 'ValueTask<T>' object. Call {nameof(this.IsAwaitableValueTask)} or {nameof(this.IsAwaitableGenericValueTask)} to ensure the method is awaitable and returns a 'ValueTask' or 'ValueTask<T>'.");
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
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' or 'ValueTask<T>' object. Call {nameof(this.IsAwaitableValueTask)} or {nameof(this.IsAwaitableGenericValueTask)} to ensure the method is awaitable and returns a 'ValueTask' or 'ValueTask<T>'.");
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

                return null;
            }
        }

        public Func<object, object[], object> GetInvocator(params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (this.invocator is null)
            {
                InitializeInvocator(args);
            }

            return this.invocator;
        }

        public Func<object, object[], object> GetAwaitableGenericValueTaskInvocator(params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableGenericValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the method is awaitable and returns a 'ValueTask<T>'.");
            }

            if (this.awaitableGenericValueTaskInvocator is null)
            {
                InitializeAwaitableGenericValueTaskInvocator();
            }

            return this.awaitableGenericValueTaskInvocator;
        }

        public Func<object, object[], ValueTask> GetAwaitableValueTaskInvocator(params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the method is awaitable and returns a 'ValueTask'.");
            }

            if (this.awaitableValueTaskInvocator is null)
            {
                InitializeAwaitableValueTaskInvocator(args);
            }

            return this.awaitableValueTaskInvocator;
        }

        public Func<object, object[], Task> GetAwaitableTaskInvocator(params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the method is awaitable and returns a 'Task'.");
            }

            if (this.awaitableTaskInvocator is null)
            {
                InitializeAwaitableTaskInvocator(args);
            }

            return this.awaitableTaskInvocator;
        }

        public Func<object, object[], Task> CreateFastInvocator<TResult>(params object[] args)
        {
            ArgumentOutOfRangeException.ThrowIfNotEqual(this.Parameters.Count, args.Length, nameof(args));

            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the method is awaitable and returns a 'Task'.");
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

        private void InitializeInvocator(object[] args)
        {
            MethodData invocatorData = MakeClosedGenericMethodData(args);

            // REVIEW::Will this work when method (current MethodData) is declared on an interface?
            // Probably requries instance for CreateDelegate() call for interface methods.
            this.invocator = invocatorData.GetMethodInfo().CreateDelegate<Func<object, object[], object>>();
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

        private MethodData MakeClosedGenericMethodData(object[] args)
        {
            MethodData invocatorData = this;
            if (this.IsGenericMethodDefinition || this.IsGenericMethod)
            {
                int genericTypeParameterCount = this.GenericTypeArguments.Length;
                TypeData[] concretegGenericTypeArguments = new TypeData[genericTypeParameterCount];
                int genericTypeParameterIndex = 0;
                foreach (ParameterData parameterData in this.Parameters)
                {
                    if (parameterData.IsGenericTypeParameter)
                    {
                        int typeArgumentIndex = parameterData.Position;
                        Type genericParameterArgument = args[typeArgumentIndex].GetType();
                        TypeData genericParameterArgumentData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(genericParameterArgument);
                        concretegGenericTypeArguments[genericTypeParameterIndex++] = genericParameterArgumentData;
                    }

                    if (genericTypeParameterIndex == genericTypeParameterCount)
                    {
                        break;
                    }
                }

                invocatorData = MakeGenericMethodData(concretegGenericTypeArguments);
            }

            return invocatorData;
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

        private static bool IsMethodExtensionMethod(MethodData methodData)
        {
            // Check if the declaring class satisfies the constraints to declare extension methods
            TypeData declaringTypeData = methodData.DeclaringTypeData;
            if (!declaringTypeData.CanDeclareExtensionMethod)
            {
                return false;
            }

            /* Check if the method satisfies the constraints to act as an extension methods */

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
        /// Determines the set of symbol attributes for the specified method based on its metadata and characteristics.
        /// </summary>
        /// <param name="methodData">The method metadata used to evaluate and determine the applicable symbol attributes.</param>
        /// <returns>A bitwise combination of SymbolAttributes values that describe the method's characteristics, such as whether
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

        private readonly struct InvocatorKey : IEquatable<InvocatorKey>
        {
            public InvocatorKey(RuntimeTypeHandle targetTypeHandle, RuntimeTypeHandle argumentTypeHandle, RuntimeTypeHandle returnTypeHandle) : this()
            {
                this.TargetTypeHandle = targetTypeHandle;
                this.ArgumentTypeHandle = argumentTypeHandle;
                this.ReturnTypeHandle = returnTypeHandle;
            }

            public RuntimeTypeHandle TargetTypeHandle { get; }
            public RuntimeTypeHandle ArgumentTypeHandle { get; }
            public RuntimeTypeHandle ReturnTypeHandle { get; }

            public bool Equals(InvocatorKey other) => this.TargetTypeHandle.Equals(other.TargetTypeHandle)
                && this.ArgumentTypeHandle.Equals(other.ArgumentTypeHandle)
                && this.ReturnTypeHandle.Equals(other.ReturnTypeHandle);

            public override bool Equals([NotNullWhen(true)] object obj)
                => obj is InvocatorKey invocatorKey && base.Equals(invocatorKey);

            public override int GetHashCode()
                => HashCode.Combine(this.TargetTypeHandle, this.ArgumentTypeHandle, this.ReturnTypeHandle);

            public static bool operator ==(InvocatorKey left, InvocatorKey right) => left.Equals(right);
            public static bool operator !=(InvocatorKey left, InvocatorKey right) => !(left == right);
        }
    }
}
