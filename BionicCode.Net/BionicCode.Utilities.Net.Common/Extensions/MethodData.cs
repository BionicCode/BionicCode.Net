namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
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
        private volatile Func<object?, object?[]?, object?>? _invocator;
        private volatile Func<object?, object?[]?, Task>? _asyncTaskInvocator;
        private volatile Func<object?, object?[]?, Task<object?>>? _asyncGenericTaskInvocator;
        private volatile Func<object?, object?[]?, ValueTask<object?>>? _asyncGenericValueTaskInvocator;
        private volatile Func<object?, object?[]?, ValueTask>? _asyncValueTaskInvocator;
        private string? assemblyName;
        private bool? isReturnValueReadOnly;
        private bool? containsGenericParameters;
        private bool? _hasParamsParameter;
        private bool? _isVoidMethod;
        private bool? _isAwaitableGenericTask;
        private bool? _isAbstract;
        private bool? _isVirtual;

        public MethodData(MethodInfo methodInfo) : base(methodInfo)
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
            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed. Call {nameof(InvokeOpenGeneric)} instead to ensure the generic method is properly closed by specifying generic type arguments.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            MethodData invocatorMethod = GetInvocatorInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invocator!.Invoke(target, args);
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
            if (!this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                throw new InvalidOperationException($"Cannot invoke non-generic methods using {nameof(InvokeOpenGeneric)}. Call '{nameof(Invoke)}' instead.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvocatorInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod._invocator!.Invoke(target, args);
        }

        public async Task InvokeTaskAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.IsGenericMethodDefinition || this.ContainsGenericParameters)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed. Call {nameof(InvokeOpenGenericTaskAsync)}' instaed to ensure the generic method is properly closed by specifying generic type arguments.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            MethodData invocatorMethod = GetInvocatorInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task InvokeOpenGenericTaskAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            if (!this.IsAwaitableTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task' object. Call {nameof(this.IsAwaitableTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvocatorInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object?> InvokeTaskWithResultAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableGenericTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call {nameof(this.IsAwaitableGenericTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task<T>'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.IsGenericMethodDefinition || this.ContainsGenericParameters)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed. Call {nameof(InvokeOpenGenericTaskWithResultAsync)}' to ensure the generic method is properly closed by specifying generic type arguments.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            MethodData invocatorMethod = GetInvocatorInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async Task<object?> InvokeOpenGenericTaskWithResultAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            if (!this.IsAwaitableGenericTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'Task<T>' object. Call {nameof(this.IsAwaitableGenericTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'Task<T>'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvocatorInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask InvokeValueTaskAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.IsGenericMethodDefinition || this.ContainsGenericParameters)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed. Call {nameof(InvokeOpenGenericValueTaskAsync)} Ensure the generic method is properly closed by specifying generic type arguments.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            MethodData invocatorMethod = GetInvocatorInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncValueTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask InvokeOpenGenericValueTaskAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            if (!this.IsAwaitableValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask' object. Call {nameof(this.IsAwaitableValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvocatorInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            await invocatorMethod._asyncValueTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask<object?> InvokeValueTaskWithResultAsync(object? target, params object?[]? args)
        {
            if (!this.IsAwaitableGenericValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask<T>'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.IsGenericMethodDefinition || this.ContainsGenericParameters)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed. Call {nameof(InvokeOpenGenericValueTaskWithResultAsync)} Ensure the generic method is properly closed by specifying generic type arguments.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            MethodData invocatorMethod = GetInvocatorInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericValueTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public async ValueTask<object?> InvokeOpenGenericValueTaskWithResultAsync(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            if (!this.IsAwaitableGenericValueTask)
            {
                throw new InvalidOperationException($"Method does not return an awaitable 'ValueTask<T>' object. Call {nameof(this.IsAwaitableGenericValueTask)} to ensure the closedGenericMethoMethodInfo is awaitable and returns a 'ValueTask<T>'.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.Parameters.HasItems)
            {
                ArgumentNullException.ThrowIfNull(args, nameof(args));

                if (this.HasParamsParameter)
                {
                    // Insufficient number of arguments provided for method invocation with 'params' parameter.
                    // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                    ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
                }
                else
                {
                    ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
                }
            }

            ArgumentNullException.ThrowIfNull(genericMethodParameters, nameof(genericMethodParameters));

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvocatorInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return await invocatorMethod._asyncGenericValueTaskInvocator!.Invoke(target, args).ConfigureAwait(false);
        }

        public MethodData GetInvocator(params object?[]? args)
        {
            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                throw new InvalidOperationException($"Cannot invoke generic methods that are not closed. Call {nameof(InvokeOpenGeneric)} instead to ensure the generic method is properly closed by specifying generic type arguments.");
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.HasParamsParameter)
            {
                // Insufficient number of arguments provided for method invocation with 'params' parameter.
                // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
            }

            MethodData invocatorMethod = GetInvocatorInternal(Array.Empty<TypeData>(), args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        public MethodData GetOpenGenericInvocator(object? target, IEnumerable<TypeData> genericMethodParameters, params object?[]? args)
        {
            if (!this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                throw new InvalidOperationException($"Cannot invoke non-generic methods using {nameof(InvokeOpenGeneric)}. Call '{nameof(Invoke)}' instead.");
            }

            if (!this.IsStatic)
            {
                ArgumentNullException.ThrowIfNull(target, nameof(target));
            }

            if (this.DeclaringTypeData.IsGenericTypeDefinition || this.DeclaringTypeData.ContainsGenericParameters)
            {
                throw new InvalidOperationException("Cannot invoke methods declared on generic type definitions or types that contain generic parameters. Ensure the declaring generic type is properly closed.");
            }

            if (this.HasParamsParameter)
            {
                // Insufficient number of arguments provided for method invocation with 'params' parameter.
                // For a params method parameter, providing no arguments for it is valid, hence the -1 check.
                ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, this.Parameters.Count - 1, nameof(args));
            }
            else
            {
                ArgumentOutOfRangeException.ThrowIfNotEqual(args.Length, this.Parameters.Count, nameof(args));
            }

            TypeData[] genericMethodTypeDataParameters = genericMethodParameters.ToArray();
            ArgumentOutOfRangeException.ThrowIfNotEqual(genericMethodTypeDataParameters.Length, this.GenericMethodArguments.Count, nameof(genericMethodParameters));

            MethodData invocatorMethod = GetInvocatorInternal(genericMethodTypeDataParameters, args);
            Debug.Assert(invocatorMethod is not null);

            return invocatorMethod;
        }

        private MethodData GetInvocatorInternal(TypeData[] genericMethodParameters, params object?[]? args)
        {
            MethodData invocatorSource = this.HasInvocatorGenerated
                // 'this' is already a closed generic method with constructed invocator.
                // Reason: only closed generic methods can have invocator/are invocable...
                ? this

                // ...otherwise generate or get cached invocator
                : GetOrCreateFastInvocator(genericMethodParameters, args);
            return invocatorSource;
        }

        private MethodData GetOrCreateFastInvocator(TypeData[] genericMethodParameters, params object?[]? args)
        {
            MethodData methodData = GetOrConstructGenericMethod(genericMethodParameters);
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

            Func<object?, object[]?, object?>? invocator = null;
            Func<object?, object[]?, Task<object?>>? awaitableGenericTaskInvocator = null;
            Func<object?, object[]?, ValueTask<object?>>? awaitableGenericValueTaskInvocator = null;
            Func<object?, object[]?, ValueTask>? awaitableValueTaskInvocator = null;
            Func<object?, object[]?, Task>? awaitableTaskInvocator = null;
            if (methodData.IsAwaitableGenericTask)
            {
                awaitableGenericTaskInvocator = Expression.Lambda<Func<object?, object[]?, Task<object?>>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableGenericValueTask)
            {
                awaitableGenericValueTaskInvocator = Expression.Lambda<Func<object?, object[]?, ValueTask<object?>>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableValueTask)
            {
                awaitableValueTaskInvocator = Expression.Lambda<Func<object?, object[]?, ValueTask>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableTask)
            {
                awaitableTaskInvocator = Expression.Lambda<Func<object?, object[]?, Task>>(body, targetParam, argsParam).Compile();
            }
            else
            {
                invocator = Expression.Lambda<Func<object?, object[]?, object?>>(body, targetParam, argsParam).Compile();
            }

            // only the closed generic method data holds the constructed invocator
            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                methodData._invocator = invocator;
                methodData._asyncTaskInvocator = awaitableTaskInvocator;
                methodData._asyncGenericTaskInvocator = awaitableGenericTaskInvocator;
                methodData._asyncGenericValueTaskInvocator = awaitableGenericValueTaskInvocator;
                methodData._asyncValueTaskInvocator = awaitableValueTaskInvocator;
            }
            else // Since 'this' is already a closed generic method, we can set the generated invocator directly on it.
            {
                this._invocator = invocator;
                this._asyncTaskInvocator = awaitableTaskInvocator;
                this._asyncGenericTaskInvocator = awaitableGenericTaskInvocator;
                this._asyncGenericValueTaskInvocator = awaitableGenericValueTaskInvocator;
                this._asyncValueTaskInvocator = awaitableValueTaskInvocator;
            }

            return methodData;
        }

        private MethodData GetOrConstructGenericMethod(TypeData[] genericMethodParameters)
        {
            MethodData methodData = this;
            if (this.IsOpenGenericMethodOrGenericMethodDefinition)
            {
                // Try get cached constructed invocator for the specified generic method parameters.
                var invocatorKeyMapKey = InvocatorKeyMapKey.Create(genericMethodParameters);
                if (MethodData.InvocatorKeyMap.TryGetValue(invocatorKeyMapKey, out SymbolInfoDataCacheKey symbolInfoCachekey)
                    && SymbolReflectionInfoCache.TryGetSymbolInfoDataCacheEntry(symbolInfoCachekey, out MethodData? cachedMethodData))
                {
                    methodData = cachedMethodData!;
                }
                else // Create closed generic method data for the specified generic method parameters.
                {
                    MethodData closedGenericMethodData = GetOrMakeClosedGenericMethodData(genericMethodParameters);
                    MethodInfo closedGenericMethodMethodInfo = closedGenericMethodData.GetMethodInfo();
                    _ = MethodData.InvocatorKeyMap.TryAdd(invocatorKeyMapKey, SymbolInfoDataCacheKey.CreateForMethod(closedGenericMethodMethodInfo));
                    methodData = closedGenericMethodData;
                }
            }

            return methodData;
        }

        private MethodData GetOrMakeClosedGenericMethodData(TypeData[] genericMethodParameters)
        {
            MethodData? closedGenericMethodData = this;
            if (this.IsGenericMethodDefinition || this.ContainsGenericParameters)
            {
                closedGenericMethodData = MakeGenericMethodData(genericMethodParameters);
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

        internal bool HasInvocatorGenerated
            => this._invocator is not null
                && this._asyncTaskInvocator is not null
                && this._asyncGenericTaskInvocator is not null
                && this._asyncValueTaskInvocator is not null
                && this._asyncGenericValueTaskInvocator is not null;

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
