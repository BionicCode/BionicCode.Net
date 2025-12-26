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
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    internal static class DelegateProvider
    {
        private static readonly ConcurrentDictionary<InvocatorKeyMapKey, SymbolInfoDataCacheKey> InvocatorKeyMap = new ConcurrentDictionary<InvocatorKeyMapKey, SymbolInfoDataCacheKey>();

        public static MethodData GetOrCreateFastMethodInvocator(MethodData targetMethodData, TypeData[] genericMethodParameters)
        {
            MethodData methodData = targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? DelegateProvider.GetOrConstructGenericMethod(targetMethodData, genericMethodParameters)
                : targetMethodData;

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]), "args");

            Expression? instance = null;
            if (!methodData.IsStatic)
            {
                instance = Expression.Convert(targetParam, methodData.DeclaringTypeData.UnwrapType()!);
            }

            UnaryExpression[] callArgs = methodData.Parameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                    parameter.ParameterTypeData.UnwrapType())).ToArray();

            MethodInfo methodInfo = methodData.GetMethodInfo();
            Expression call = methodData.IsStatic
                ? Expression.Call(methodInfo, callArgs)
                : Expression.Call(instance!, methodInfo, callArgs); // instance required for non-static :contentReference[oaicite:7]{index=7}

            Expression body = methodData.IsVoidMethod
                ? Expression.Block(call, Expression.Constant(null, typeof(object)))
                : methodData.IsAwaitableGenericTask
                    ? Expression.Convert(call, typeof(Task<object>))
                    : methodData.IsAwaitableGenericValueTask
                        ? Expression.Convert(call, typeof(ValueTask<object>))
                        : methodData.IsAwaitableValueTask
                            ? Expression.Convert(call, typeof(ValueTask))
                            : methodData.IsAwaitableTask
                                ? Expression.Convert(call, typeof(Task))
                                : Expression.Convert(call, typeof(object));

            Func<object?, object?[]?, object?>? invocator = null;
            Func<object?, object?[]?, Task<object?>>? awaitableGenericTaskInvocator = null;
            Func<object?, object?[]?, ValueTask<object?>>? awaitableGenericValueTaskInvocator = null;
            Func<object?, object?[]?, ValueTask>? awaitableValueTaskInvocator = null;
            Func<object?, object?[]?, Task>? awaitableTaskInvocator = null;
            if (methodData.IsAwaitableGenericTask)
            {
                awaitableGenericTaskInvocator = Expression.Lambda<Func<object?, object?[]?, Task<object?>>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableGenericValueTask)
            {
                awaitableGenericValueTaskInvocator = Expression.Lambda<Func<object?, object?[]?, ValueTask<object?>>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableValueTask)
            {
                awaitableValueTaskInvocator = Expression.Lambda<Func<object?, object?[]?, ValueTask>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableTask)
            {
                awaitableTaskInvocator = Expression.Lambda<Func<object?, object?[]?, Task>>(body, targetParam, argsParam).Compile();
            }
            else
            {
                invocator = Expression.Lambda<Func<object?, object?[]?, object?>>(body, targetParam, argsParam).Compile();
            }

            IMethodDataInvoker methodInvoker = targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? (IMethodDataInvoker)methodData // only the closed generic method data holds the constructed invocator
                : (IMethodDataInvoker)targetMethodData; // Since targetMethodData is already a closed generic method, we can set the generated invocator directly on it.

            methodInvoker.SetInvocator(invocator);
            methodInvoker.SetInvocator(awaitableTaskInvocator);
            methodInvoker.SetInvocator(awaitableGenericTaskInvocator);
            methodInvoker.SetInvocator(awaitableGenericValueTaskInvocator);
            methodInvoker.SetInvocator(awaitableValueTaskInvocator);

            return methodData;
        }

        private static MethodData GetOrConstructGenericMethod(MethodData targetMethodData, TypeData[] genericMethodParameters)
        {
            MethodData methodData;

            // Try get cached constructed invocator for the specified generic method parameters.
            var invocatorKeyMapKey = InvocatorKeyMapKey.Create(genericMethodParameters);
            if (DelegateProvider.InvocatorKeyMap.TryGetValue(invocatorKeyMapKey, out SymbolInfoDataCacheKey symbolInfoCachekey)
                && SymbolReflectionInfoCache.TryGetSymbolInfoDataCacheEntry(symbolInfoCachekey, out MethodData? cachedMethodData))
            {
                methodData = cachedMethodData!;
            }
            else // Create closed generic method data for the specified generic method parameters.
            {
                MethodData closedGenericMethodData = targetMethodData.MakeGenericMethodData(genericMethodParameters);
                _ = DelegateProvider.InvocatorKeyMap.TryAdd(invocatorKeyMapKey, closedGenericMethodData.CacheKey);
                methodData = closedGenericMethodData;
            }

            return methodData;
        }

        private readonly struct InvocatorKeyMapKey : IEquatable<InvocatorKeyMapKey>
        {
            public ImmutableList<RuntimeTypeHandle> MethodParameterTypeHandles { get; }
            private readonly int? _hashCode;

            public InvocatorKeyMapKey(ImmutableList<RuntimeTypeHandle> methodParameterTypeHandles) : this()
            {
                ArgumentNullExceptionEx.ThrowIfNullOrEmpty(methodParameterTypeHandles, nameof(methodParameterTypeHandles), "Method parameter type handles must be provided to create an invocator map key.");

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
