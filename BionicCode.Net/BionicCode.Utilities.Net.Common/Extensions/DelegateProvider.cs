namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;

    public delegate void MethodVoidInvoker<TTarget>(TTarget? target, params object?[] args);
    //public delegate TResult MethodInvoker<TTarget, TResult>(TTarget? target, params object?[] args);
    public delegate TResult MethodInvoker<TTarget, TResult>(TTarget? target, ReadOnlySpan<object?> args);
    public delegate Task<TResult> MethodAwaitableGenericTaskInvoker<TTarget, TResult>(TTarget? target, params object?[] args);
    public delegate Task MethodAwaitableTaskInvoker<TTarget, TResult>(TTarget? target, params object?[] args);
    public delegate ValueTask<TResult> MethodAwaitableGenericValueTaskInvoker<TTarget, TResult>(TTarget? target, params object?[] args);
    public delegate ValueTask MethodAwaitableValueTaskInvoker<TTarget, TResult>(TTarget? target, params object?[] args);
    public delegate Task MethodAwaitableTaskDiscardInvoker<TTarget>(TTarget target, params object?[] args);
    public delegate dynamic MethodAwaitableValueTaskDiscardInvoker<TTarget>(TTarget target, params object?[] args);


    /// <summary>
    /// Represents a method that sets the field on a value prpertyType instance.
    /// </summary>
    /// <remarks>This delegate is typically used to update fields on value prpertyType instances, such as structs,
    /// where direct assignment is required. The propertyType parameter is passed by reference to allow modification of the
    /// original instance.</remarks>
    /// <typeparam name="TTarget">The value prpertyType whose field will be set.</typeparam>
    /// <typeparam name="TValue">The prpertyType of the value to assign to the field.</typeparam>
    /// <param name="target">A reference to the value prpertyType instance whose field will be set.</param>
    /// <param name="value">The value to assign to the field. May be null if the field allows null values.</param>
    public delegate void ValueTypeMemberSetter<TTarget, TValue>(ref TTarget target, TValue? value) where TTarget : struct;

    /// <summary>
    /// Represents a method that sets an indexer property on a value prpertyType instance using the specified
    /// indices.
    /// </summary>
    /// <remarks>This delegate is typically used to abstract the process of setting indexer properties on
    /// value types, such as structs, where direct assignment is required. The propertyType parameter is passed by reference
    /// to allow modification of the underlying value prpertyType instance.</remarks>
    /// <typeparam name="TTarget">The value prpertyType that contains the indexer property to be set.</typeparam>
    /// <typeparam name="TValue">The prpertyType of the value to assign to the indexer property.</typeparam>
    /// <param name="target">A reference to the value prpertyType instance whose indexer property will be set.</param>
    /// <param name="indices">An array of objects representing the indices used to access the indexer property. Can be null if the indexer
    /// does not require indices.</param>
    /// <param name="value">The value to assign to the indexer property. Can be null for reference types or nullable value types.</param>
    public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue>(ref TTarget target, TValue value, params object[] indices) where TTarget : struct;

    public delegate void PropertySetter<TTarget, TValue>(TTarget? target, TValue value) where TTarget : class;
    public delegate void ValueTypePropertySetter<TTarget, TValue>(ref TTarget target, TValue value) where TTarget : struct;
    public delegate object ValueTypePropertySetter(ref object target, object? value);
    public delegate void IndexerPropertySetter<TTarget, TValue, TIndex>(TTarget? target, TValue value, params TIndex[] index) where TTarget : class;
    public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex>(ref TTarget target, TValue value, TIndex index) where TTarget : struct;
    public delegate void IndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>(TTarget? target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : class;
    public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : struct;
    public delegate void IndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget? target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : class;
    public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : struct;


    public delegate TValue PropertyGetter<TTarget, TValue>(TTarget target);
    public delegate TValue IndexerPropertyGetter<TTarget, TValue>(TTarget target, params object[] index);
    public delegate TValue IndexerPropertyGetter<TTarget, TValue, TIndex>(TTarget target, params TIndex[] index);
    public delegate TValue IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TIndex1 index1, TIndex2 index2);
    public delegate TValue IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3);

    internal static class DelegateProvider
    {
        private static readonly ConcurrentDictionary<MethodDataGenericTypeVariantKey, SymbolInfoDataCacheKey> InvocatorKeyMap = new ConcurrentDictionary<MethodDataGenericTypeVariantKey, SymbolInfoDataCacheKey>();

        /// <summary>
        /// Gets an invocable MethodData instance for the specified method, generating a fast delegate-based invoker if
        /// necessary. Supports both generic and non-generic methods.
        /// </summary>
        /// <remarks>If the specified method already has an invoker, it is returned as-is. For generic
        /// method definitions or open generic methods, the method is first constructed with the provided generic prpertyType
        /// arguments before generating the invoker. The returned MethodData can be used for efficient runtime
        /// invocation without reflection overhead.</remarks>
        /// <param name="targetMethodData">The MethodData representing the propertyType method. This can be a generic method definition, an open generic
        /// method, or a closed method.</param>
        /// <param name="genericMethodArguments">An array of TypeData objects specifying the generic prpertyType arguments to use if the propertyType method is a generic
        /// method definition or open generic method. This parameter is ignored for non-generic methods.</param>
        /// <returns>A MethodData instance that is guaranteed to have an invoker delegate attached, suitable for fast invocation.
        /// If the method is generic, the returned MethodData corresponds to the constructed closed generic method.</returns>
        public static MethodData GetOrCreateFastMethodInvoker(MethodData targetMethodData, TypeList genericMethodArguments)
        {
            // If the method is not a generic method definition or an open generic method and already has an invocator, return it directly.
            if (!targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition && ((IMethodDataInvoker)targetMethodData).IsInvocable)
            {
                return targetMethodData;
            }

            Type desiredReturnType = !targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? targetMethodData.ReturnTypeData.UnwrapType()
                : targetMethodData.IsAwaitableGenericTask
                    ? typeof(Task<object>)
                    : targetMethodData.IsAwaitableTask
                        ? typeof(Task)
                        : targetMethodData.IsAwaitableGenericValueTask
                            ? typeof(ValueTask<object>)
                            : targetMethodData.IsAwaitableValueTask
                                ? typeof(ValueTask)
                                : typeof(object);

            // Get or construct the (closed) generic method data if required.
            MethodData methodData = targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? DelegateProvider.GetOrConstructGenericMethod(typeof(object).TypeHandle, desiredReturnType.TypeHandle, genericMethodArguments, targetMethodData)
                : targetMethodData;

            // If the closed method has already a generated invoker, it will be returned directly.
            if (((IMethodDataInvoker)methodData).IsInvocable)
            {
                return methodData;
            }

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

            Func<object?, object?[]?, object?>? invocator = null;
            Func<object?, object?[]?, Task<object?>>? awaitableGenericTaskInvocator = null;
            Func<object?, object?[]?, ValueTask<object?>>? awaitableGenericValueTaskInvocator = null;
            Func<object?, object?[]?, ValueTask>? awaitableValueTaskInvocator = null;
            Func<object?, object?[]?, Task>? awaitableTaskInvocator = null;
            Expression body;
            if (methodData.IsVoidMethod)
            {
                body = Expression.Block(call, Expression.Constant(null, typeof(object)));
                invocator = Expression.Lambda<Func<object?, object?[]?, object?>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableGenericTask)
            {
                body = Expression.Convert(call, typeof(Task<object>));
                awaitableGenericTaskInvocator = Expression.Lambda<Func<object?, object?[]?, Task<object?>>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableGenericValueTask)
            {
                body = Expression.Convert(call, typeof(ValueTask<object>));
                awaitableGenericValueTaskInvocator = Expression.Lambda<Func<object?, object?[]?, ValueTask<object?>>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableValueTask)
            {
                body = Expression.Convert(call, typeof(ValueTask));
                awaitableValueTaskInvocator = Expression.Lambda<Func<object?, object?[]?, ValueTask>>(body, targetParam, argsParam).Compile();
            }
            else if (methodData.IsAwaitableTask)
            {
                body = Expression.Convert(call, typeof(Task));
                awaitableTaskInvocator = Expression.Lambda<Func<object?, object?[]?, Task>>(body, targetParam, argsParam).Compile();
            }
            else
            {
                body = Expression.Convert(call, typeof(object));
                invocator = Expression.Lambda<Func<object?, object?[]?, object?>>(body, targetParam, argsParam).Compile();
            }

            // only the closed generic method data holds the constructed invocator
            IMethodDataInvoker methodInvoker = methodData;

            methodInvoker.SetInvoker(invocator);
            methodInvoker.SetInvoker(awaitableTaskInvocator);
            methodInvoker.SetInvoker(awaitableGenericTaskInvocator);
            methodInvoker.SetInvoker(awaitableGenericValueTaskInvocator);
            methodInvoker.SetInvoker(awaitableValueTaskInvocator);

            return methodData;
        }

        public static MethodData GetOrCreateFastMethodInvoker<TTarget, TResult>(MethodData targetMethodData, TypeList genericMethodArguments, bool isDiscardDelegate)
        {
            Type targetType = typeof(TTarget);
            Type declaringType = targetMethodData.DeclaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                targetMethodData.DeclaringTypeData.UnwrapType()!,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(TTarget),
                        declaringType!,
                        "declaring type"));

            Type desiredReturnType = typeof(TResult);

            // Validate method return type compatibility
            ThrowIfReturnTypeIsInvalid(targetMethodData, desiredReturnType, nameof(TResult), isDiscardDelegate);

            var genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
                genericMethodArguments,
                desiredReturnType.TypeHandle,
                targetType.TypeHandle,
                targetMethodData.BasicMethodFingerprint);

            // If the method is not a generic method definition or an open generic method and already has the correct invocator, return it directly.
            if (((IStrictMethodDataInvoker)targetMethodData).IsInvocable(genericTypedMethodVariantKey))
            {
                return targetMethodData;
            }

            // Get or construct the (closed) generic method data if required.
            MethodData methodData = targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? DelegateProvider.GetOrConstructGenericMethod(targetType.TypeHandle, desiredReturnType.TypeHandle, genericMethodArguments, targetMethodData)
                : targetMethodData;

            genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
                genericMethodArguments,
                desiredReturnType.TypeHandle,
                targetType.TypeHandle,
                targetMethodData.BasicMethodFingerprint);

            // If the closed method has already a generated invoker, it will be returned directly.
            if (((IStrictMethodDataInvoker)methodData).IsInvocable(genericTypedMethodVariantKey))
            {
                return methodData;
            }

            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]), "args");

            Expression? instanceExpression = methodData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            UnaryExpression[] callArgs = methodData.Parameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                    parameter.ParameterTypeData.UnwrapType())).ToArray();

            MethodInfo methodInfo = methodData.GetMethodInfo();
            Expression call = methodData.IsStatic
                ? Expression.Call(methodInfo, callArgs)
                : Expression.Call(instanceExpression, methodInfo, callArgs); // instance required for non-static :contentReference[oaicite:7]{index=7}

            Type methodReturnType = targetMethodData.ReturnTypeData.UnwrapType();
            Delegate invocator = null;
            Expression body;
            try
            {
                if (methodData.IsAwaitableGenericTask)
                {
                    if (isDiscardDelegate)
                    {
                        // Ignore the generic return type argument TResult and force cast to Task
                        body = Expression.Convert(call, typeof(Task));
                        invocator = Expression.Lambda<MethodAwaitableTaskDiscardInvoker<TTarget>>(body, targetParam, argsParam).Compile();
                    }
                    else
                    {
                        body = Expression.Convert(call, desiredReturnType);
                        invocator = Expression.Lambda<MethodAwaitableGenericTaskInvoker<TTarget, TResult>>(body, targetParam, argsParam).Compile();
                    }
                }
                else if (methodData.IsAwaitableGenericValueTask)
                {
                    if (isDiscardDelegate)
                    {
                        // Ignore the generic return type argument TResult and force declared method return type
                        body = Expression.Convert(call, methodReturnType);
                        invocator = Expression.Lambda<MethodAwaitableValueTaskDiscardInvoker<TTarget>>(body, targetParam, argsParam).Compile();
                    }
                    else
                    {
                        body = Expression.Convert(call, desiredReturnType);
                        invocator = Expression.Lambda<MethodAwaitableGenericValueTaskInvoker<TTarget, TResult>>(body, targetParam, argsParam).Compile();
                    }
                }
                else if (methodData.IsAwaitableValueTask)
                {
                    body = Expression.Convert(call, typeof(ValueTask));
                    if (isDiscardDelegate)
                    {
                        invocator = Expression.Lambda<MethodAwaitableValueTaskDiscardInvoker<TTarget>>(body, targetParam, argsParam).Compile();
                    }
                    else
                    {
                        invocator = Expression.Lambda<MethodAwaitableValueTaskInvoker<TTarget, TResult>>(body, targetParam, argsParam).Compile();
                    }
                }
                else if (methodData.IsAwaitableTask)
                {
                    body = Expression.Convert(call, typeof(Task));
                    if (isDiscardDelegate)
                    {
                        invocator = Expression.Lambda<MethodAwaitableTaskDiscardInvoker<TTarget>>(body, targetParam, argsParam).Compile();
                    }
                    else
                    {
                        invocator = Expression.Lambda<MethodAwaitableTaskInvoker<TTarget, TResult>>(body, targetParam, argsParam).Compile();
                    }
                }
                else
                {
                    // Use provided argument type  and cast to indexer parameter type if needed.
                    body = Expression.Convert(call, desiredReturnType);

                    invocator = Expression.Lambda<MethodInvoker<TTarget, TResult>>(body, targetParam, argsParam).Compile();
                }
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TResult)}' is incompatible with the method's return type. Reason: A conversion from the provided '{methodReturnType.ToFullyQualifiedSignatureName()}' to '{desiredReturnType.ToFullyQualifiedSignatureName()}' is not natively supported.",
                    nameof(TResult),
                    e);
            }

            // Only the closed generic method data holds the constructed invocator
            IStrictMethodDataInvoker methodInvoker = methodData;
            methodInvoker.SetInvoker(genericTypedMethodVariantKey, invocator);

            return methodData;
        }

        public static MethodData GetOrCreateFastVoidMethodInvoker<TTarget>(MethodData targetMethodData, TypeList genericMethodArguments)
        {
            Type targetType = typeof(TTarget);
            Type declaringType = targetMethodData.DeclaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                targetMethodData.DeclaringTypeData.UnwrapType()!,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(TTarget),
                        declaringType!,
                        "declaring type"));

            Type resultType = typeof(void);

            MethodDataGenericTypeVariantKey genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
                genericMethodArguments,
                resultType.TypeHandle,
                targetType.TypeHandle,
                targetMethodData.BasicMethodFingerprint);

            // If the method is not a generic method definition or an open generic method and already has an invocator, return it directly.
            if (((IStrictMethodDataInvoker)targetMethodData).IsInvocable(genericTypedMethodVariantKey))
            {
                return targetMethodData;
            }

            // Get or construct the (closed) generic method data if required.
            MethodData methodData = targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? DelegateProvider.GetOrConstructGenericMethod(targetType.TypeHandle, resultType.TypeHandle, genericMethodArguments, targetMethodData)
                : targetMethodData;

            genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
                genericMethodArguments,
                resultType.TypeHandle,
                targetType.TypeHandle,
                targetMethodData.BasicMethodFingerprint);
            // If the closed method has already a generated invoker, it will be returned directly.
            if (((IStrictMethodDataInvoker)methodData).IsInvocable(genericTypedMethodVariantKey))
            {
                return methodData;
            }

            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression argsParam = Expression.Parameter(typeof(object[]), "args");

            Expression? instanceExpression = methodData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            UnaryExpression[] callArgs = methodData.Parameters.Select((parameter, index) =>
                Expression.Convert(
                    Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                    parameter.ParameterTypeData.UnwrapType())).ToArray();

            MethodInfo methodInfo = methodData.GetMethodInfo();
            Expression call = methodData.IsStatic
                ? Expression.Call(methodInfo, callArgs)
                : Expression.Call(instanceExpression, methodInfo, callArgs); // instance required for non-static :contentReference[oaicite:7]{index=7}

            Type methodReturnType = targetMethodData.ReturnTypeData.UnwrapType();
            Expression body = Expression.Block(call, Expression.Empty());
            Delegate invocator = Expression.Lambda<MethodVoidInvoker<TTarget>>(body, targetParam, argsParam).Compile();

            // Only the closed generic method data holds the constructed invocator
            IStrictMethodDataInvoker methodInvoker = methodData;
            methodInvoker.SetInvoker(genericTypedMethodVariantKey, invocator);

            return methodData;
        }

        private static void ThrowIfReturnTypeIsInvalid(MethodData targetMethodData, Type resultType, string paramName, bool isDiscardDelegate)
        {
            // void-returning methods: TResult must be void (for strict void invoker) or you need a different API.
            if (targetMethodData.IsVoidMethod)
            {
                ArgumentExceptionAdvanced.ThrowIfFalse(resultType == typeof(void),
                    paramName,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                        resultType,
                        paramName,
                        typeof(void),
                        "method return type"));

                return;
            }

            Type methodReturnType = targetMethodData.ReturnTypeData.UnwrapType();

            if (resultType.IsGenericType && !resultType.IsConstructedGenericType) // Disallow open generic types
            {
                throw new ArgumentException(
                    $"The provided argument '{paramName}' is an open generic type. Please provide a constructed generic type.",
                    paramName);
            }
            else if (!targetMethodData.IsAwaitable) // Non-awaitable synchronous methods
            {
                // Let compiler decide whether a conversion is possible and catch exception if not.
                return;
            }
            else if (targetMethodData.IsAwaitable && isDiscardDelegate)
            {
                if (targetMethodData.IsAwaitableTask || targetMethodData.IsAwaitableGenericTask)
                {
                    ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        typeof(Task),
                        resultType,
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                                resultType,
                                paramName,
                                typeof(Task),
                                "method return type"));

                }
                else if (targetMethodData.IsAwaitableValueTask || targetMethodData.IsAwaitableGenericValueTask)
                {
                    // ValueTask and ValueTask<T> are not assignable to each other.
                    // The generated delegate will handle that particular case.
                    return;
                }
            }
            else if (targetMethodData.IsAwaitableTask)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    typeof(Task),
                    resultType,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            resultType,
                            paramName,
                            typeof(Task),
                            "method return type"));
            }
            else if (targetMethodData.IsAwaitableGenericTask)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    typeof(Task<>).MakeGenericType(methodReturnType),
                    resultType,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            resultType,
                            paramName,
                            typeof(Task<>).MakeGenericType(methodReturnType),
                            "method return type"));
            }
            else if (targetMethodData.IsAwaitableGenericValueTask)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                    typeof(ValueTask<>).MakeGenericType(methodReturnType),
                    resultType,
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            resultType,
                            paramName,
                            typeof(ValueTask<>).MakeGenericType(methodReturnType),
                            "method return type"));
            }
            else if (targetMethodData.IsAwaitableValueTask)
            {
                ArgumentExceptionAdvanced.ThrowIfNotEqualsType(
                    resultType,
                    typeof(ValueTask),
                    ExceptionMessages.GetTypeMismatchExceptionMessage(
                            resultType,
                            paramName,
                            typeof(ValueTask),
                            "method return type"));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static MethodData GetOrConstructGenericMethod(RuntimeTypeHandle targetTypeHandle, RuntimeTypeHandle desiredReturnTypeHandle, TypeList genericMethodArguments, MethodData targetMethodData)
        {
            MethodData methodData;

            // Try get cached constructed invocator for the specified generic method parameters.
            var invocatorKeyMapKey = new MethodDataGenericTypeVariantKey(genericMethodArguments, desiredReturnTypeHandle, targetTypeHandle, targetMethodData.BasicMethodFingerprint);
            if (DelegateProvider.InvocatorKeyMap.TryGetValue(invocatorKeyMapKey, out SymbolInfoDataCacheKey symbolInfoCacheKey)
                && SymbolReflectionInfoCache.TryGetSymbolInfoDataCacheEntry(symbolInfoCacheKey, out MethodData? cachedMethodData))
            {
                methodData = cachedMethodData!;
            }
            else // Create closed generic method data for the specified generic method parameters.
            {
                methodData = CloseOpenGenericMethodAndAddToCache(targetMethodData, genericMethodArguments, invocatorKeyMapKey);
            }

            return methodData;
        }

        private static MethodData CloseOpenGenericMethodAndAddToCache(MethodData targetMethodData, TypeList genericMethodParameters, MethodDataGenericTypeVariantKey invocatorKeyMapKey)
        {
            MethodData methodData;
            MethodData closedGenericMethodData = targetMethodData.MakeGenericMethodData(genericMethodParameters);
            _ = DelegateProvider.InvocatorKeyMap.TryAdd(invocatorKeyMapKey, closedGenericMethodData.CacheKey);
            methodData = closedGenericMethodData;
            return methodData;
        }

        /// <summary>
        /// Retrieves an existing fast event invoker for the specified event or creates one if it does not already
        /// exist.
        /// </summary>
        /// <param name="eventData">The metadata describing the event for which to obtain or create a fast invoker. Cannot be null.</param>
        /// <returns>A MethodData instance representing a fast invoker for the specified event.</returns>
        public static MethodData GetOrCreateFastEventInvoker(EventData eventData)
        {
            MethodData targetMethodData = eventData.EventInvokerMethodData;

            return GetOrCreateFastMethodInvoker(targetMethodData!, TypeList.Empty);
        }

        /// <summary>
        /// Creates a delegate that retrieves the returnType of the specified field from a given object instance.
        /// </summary>
        /// <remarks>The returned delegate expects the propertyType object to be of the field's declaring prpertyType
        /// or compatible with it. For returnType prpertyType fields, the result is boxed. Passing a propertyType of an incompatible prpertyType
        /// may result in a runtime exception.</remarks>
        /// <param name="fieldData">The metadata describing the field for which to create a getter delegate. Must not be null and must have a
        /// non-null declaring prpertyType.</param>
        /// <returns>A delegate that takes an object instance and returns the returnType of the specified field as an object. For
        /// static fields, the instance parameter is ignored.</returns>
        public static Func<object?, object?> CreateGetter(FieldData fieldData)
        {
            ArgumentNullException.ThrowIfNull(fieldData, nameof(fieldData));
            ArgumentNullException.ThrowIfNull(fieldData.DeclaringTypeData, nameof(fieldData));

            FieldInfo field = fieldData.GetFieldInfo();

            // (object? propertyType) => (object?)((TDeclaring)propertyType).Field
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "propertyType");

            Expression fieldAccess =
                field.IsStatic
                    ? Expression.Field(expression: null, field) // static: no instance
                    : Expression.Field(
                        Expression.Convert(targetParam, fieldData.DeclaringTypeData.UnwrapType()), // cast/unbox
                        field);

            // Box returnType types
            UnaryExpression body = Expression.Convert(fieldAccess, typeof(object));

            return Expression
                .Lambda<Func<object?, object?>>(body, targetParam)
                .Compile(); // compiles to a delegate 
        }

        /// <summary>
        /// Creates a delegate that sets the returnType of the specified field on a given object instance or prpertyType.
        /// </summary>
        /// <remarks>The returned delegate uses object-based parameters. For instance fields declared on
        /// reference types, the propertyType parameter must be an instance of the declaring prpertyType. For static fields, the
        /// propertyType parameter is ignored. This method does not support creating setters for instance fields on returnType
        /// types (structs); use a ref-based setter in such cases.</remarks>
        /// <param name="fieldData">The metadata describing the field for which to create a setter. Must not represent a const or readonly
        /// field.</param>
        /// <returns>An <see cref="Action{Object, Object}"/> delegate that sets the returnType of the specified field. For static
        /// fields, the propertyType parameter is ignored.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="fieldData"/> represents a const or readonly field.</exception>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="fieldData"/> represents an instance field declared on a returnType prpertyType. Use a
        /// ref-based setter instead.</exception>
        public static Action<object?, object?> CreateSetter(FieldData fieldData)
        {
            ArgumentNullException.ThrowIfNull(fieldData, nameof(fieldData));
            ArgumentNullException.ThrowIfNull(fieldData.DeclaringTypeData, nameof(fieldData));

            // Reject const / readonly up front
            if (fieldData.IsConst) // const 
            {
                throw new InvalidOperationException("Cannot create a setter for a 'const' field.");
            }

            if (fieldData.IsReadonly) // readonly 
            {
                throw new InvalidOperationException("Cannot create a setter for a 'readonly' field.");
            }

            // Important: setting instance fields on a boxed struct would modify only a copy.
            if (!fieldData.IsStatic && fieldData.DeclaringTypeData.IsValueType)
            {
                throw new NotSupportedException(
                    "Cannot create an object-based setter for an instance field declared on a value prpertyType. " +
                    $"You need a ref-based setter: call {nameof(CreateStructSetter)} instead.");
            }

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "propertyType");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "returnType");

            FieldInfo field = fieldData.GetFieldInfo();
            Expression fieldAccess =
                fieldData.IsStatic
                    ? Expression.Field(expression: null, field)
                    : Expression.Field(
                        Expression.Convert(targetParam, fieldData.DeclaringTypeData.UnwrapType()),
                        field);

            BinaryExpression assign = Expression.Assign(
                fieldAccess,
                Expression.Convert(valueParam, fieldData.FieldTypeData.UnwrapType())); // Expression.Assign 

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<Action<object?, object?>>(body, targetParam, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the returnType of a specified field on a struct propertyType instance.
        /// </summary>
        /// <remarks>The returned delegate operates on struct instances by reference, allowing direct
        /// assignment to the field. The field and returnType types must be compatible with TTarget and TValue, respectively.
        /// This method validates prpertyType compatibility and field mutability before creating the setter.</remarks>
        /// <typeparam name="TTarget">The prpertyType of the struct that contains the field to set.</typeparam>
        /// <typeparam name="TValue">The prpertyType of the returnType to assign to the field.</typeparam>
        /// <param name="fieldData">Metadata describing the field to be set, including its declaring prpertyType and field prpertyType information. Cannot be
        /// null.</param>
        /// <returns>A delegate that sets the specified field on a struct of prpertyType TTarget to a returnType of prpertyType TValue.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified field is declared as const or readonly.</exception>
        public static ValueTypeMemberSetter<TTarget, TValue> CreateStructSetter<TTarget, TValue>(FieldData fieldData)
            where TTarget : struct
        {
            ArgumentNullException.ThrowIfNull(fieldData, nameof(fieldData));
            ArgumentNullException.ThrowIfNull(fieldData.DeclaringTypeData, nameof(fieldData));

            Type declaringType = fieldData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(TTarget),
                        declaringType,
                        "declaring prpertyType"));

            Type valueType = typeof(TValue);
            Type fieldType = fieldData.FieldTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                valueType,
                fieldType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        valueType,
                        nameof(TValue),
                        fieldType,
                        "field prpertyType"));

            if (fieldData.IsConst)
            {
                throw new InvalidOperationException("Cannot create a setter for a 'const' field.");
            }

            if (fieldData.IsReadonly)
            {
                throw new InvalidOperationException("Cannot create a setter for a 'readonly' field.");
            }

            // (ref TTarget propertyType, TValue returnType) => propertyType.Field = returnType;
            ParameterExpression targetByRef = Expression.Parameter(targetType.MakeByRefType(), "propertyType");
            ParameterExpression valueParam = Expression.Parameter(valueType, "returnType");

            FieldInfo field = fieldData.GetFieldInfo();
            MemberExpression fieldAccess = Expression.Field(targetByRef, field);
            BinaryExpression assign = Expression.Assign(fieldAccess, Expression.Convert(valueParam, fieldType));
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypeMemberSetter<TTarget, TValue>>(body, targetByRef, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that retrieves the value of the specified readable property.
        /// </summary>
        /// <remarks>The returned delegate expects the target instance to be compatible with the declaring type of the
        /// property. For value type properties, the result is boxed.</remarks>
        /// <param name="propertyData">The metadata describing the property for which to create a getter delegate. Must represent a readable
        /// property and cannot be null.</param>
        /// <returns>A delegate that takes an object instance and returns the returnType of the specified property. For static
        /// properties, the instance parameter is ignored.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a getter delegate has already been generated for the specified property.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified property is an indexer or is write-only.</exception>
        public static Func<object?, object?> CreateGetter(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property is an indexer. Use the appropriate indexer getter creation method instead.");
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            // (object? target) => (object?)((TDeclaring)target).Property
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");

            PropertyInfo property = propertyData.GetPropertyInfo();
            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Expression propertyAccess =
                propertyData.IsStatic
                    ? Expression.Property(expression: null, property) // static: no instance
                    : Expression.Property(
                        Expression.Convert(targetParam, declaringType), // cast/unbox
                        property);

            // Box returnType types
            UnaryExpression body = Expression.Convert(propertyAccess, typeof(object));

            return Expression
                .Lambda<Func<object?, object?>>(body, targetParam)
                .Compile(); // compiles to a delegate 
        }

        /// <summary>
        /// Creates a strongly-typed delegate that retrieves the value of a specified property from a specified target instance.
        /// </summary>
        /// <remarks>The created delegate provides efficient access to the property value and supports
        /// both static and instance properties. For indexer properties, use the appropriate indexer getter creation
        /// method instead.</remarks>
        /// <typeparam name="TTarget">The target type of the instance that declares the property.</typeparam>
        /// <typeparam name="TValue">The property type of the value returned by the property getter.</typeparam>
        /// <param name="propertyData">The metadata describing the property for which to create a getter. Must represent a readable, non-indexer
        /// property.</param>
        /// <returns>A delegate that, when invoked with a declaringType object, returns the value of the specified property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the property is an indexer, is write-only, if the generic method arguments are incompatible with the
        /// property or declaring type, or if the property type cannot be cast to the specified return type.</exception>
        public static PropertyGetter<TTarget, TValue> CreateGetter<TTarget, TValue>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property is an indexer. Use the appropriate indexer getter creation method instead.");
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            Type targetType = typeof(TTarget);
            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(TTarget),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType,
                    "declaring type"));

            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();

            // (TTarget target) => (TValue)target.Property
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            PropertyInfo property = propertyData.GetPropertyInfo();
            Expression propertyAccess = Expression.Property(instanceExpression, property);

            Expression body;
            try
            {
                body = returnType == propertyType
                        ? propertyAccess
                        : Expression.Convert(propertyAccess, returnType);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TValue)}' is incompatible with the property's type. Reason: A conversion from the provided '{propertyData.PropertyTypeData.FullyQualifiedSignature}' to the provided '{returnType.FullName}' is not natively supported.",
                    nameof(TValue),
                    e);
            }

            return Expression
                .Lambda<PropertyGetter<TTarget, TValue>>(body, targetParam)
                .Compile(); // compiles to a delegate 
        }

        /// <summary>
        /// Creates a strongly typed getter delegate for an indexer property on the specified target instance.
        /// </summary>
        /// <remarks>The created delegate provides efficient, strongly typed access to the indexer
        /// property. The property described by propertyData must be an indexer with exactly one index parameter, and
        /// the types specified by TTarget, TIndex, and TValue must be compatible with the declaring type, index
        /// parameter type, and property type, respectively.</remarks>
        /// <typeparam name="TTarget">The target type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TIndex">The type of the index parameter accepted by the indexer.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create the getter. Must represent a readable
        /// indexer with exactly one index parameter.</param>
        /// <returns>A delegate that gets the value of the specified indexer property for a given declaringType object and index.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified property is not a 1D indexer, or if the types specified by TTarget, TIndex, or TValue are not compatible with the declaring type, index parameter type, or property type.</exception>
        public static IndexerPropertyGetter<TTarget, TValue, TIndex> CreateIndexerGetter<TTarget, TValue, TIndex>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property is not an indexer. Use the appropriate indexer getter creation method instead.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            TypeData declaringTypeData = propertyData.DeclaringTypeData;
            ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
            Type targetType = typeof(TTarget);
            Type declaringType = declaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(TTarget),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType,
                    "declaring type"));
            //ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            //    1,
            //    propertyData.IndexerParameters.Count,
            //    nameof(propertyData),
            //    "The provided indexer property must have exactly a single index parameter to create a 1D indexer getter.");
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            Type indexType = typeof(TIndex);
            ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
            TypeData indexerParameterTypeData = propertyGetMethodParameters[0].ParameterTypeData;
            Type indexParameterType = indexerParameterTypeData.UnwrapType();

            // (TTarget declaringType, TIndex[] indices)
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression indicesParam = Expression.Parameter(typeof(TIndex[]), "indices");

            // Validate indices length
            Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: true);

            Expression[] indexExpressions = new Expression[propertyGetMethodParameters.Count];
            for (int i = 0; i < propertyGetMethodParameters.Count; i++)
            {
                ParameterData indexParameterData = propertyGetMethodParameters[i];

                // indices[i]
                BinaryExpression indexAccess = Expression.ArrayIndex(
                    indicesParam,
                    Expression.Constant(i));

                // (TIndexType)indices[i]
                Expression castedIndexParam;
                try
                {
                    // Use provided argument type  and cast to indexer parameter type if needed.
                    castedIndexParam = indexType != indexParameterType
                        ? Expression.Convert(indexAccess, indexParameterType)
                        : indicesParam;
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException(
                        $"The provided argument '{nameof(TIndex)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{indexType.FullName}' to '{indexerParameterTypeData.FullyQualifiedSignature}' is not natively supported.",
                        nameof(TIndex),
                        e);
                }

                indexExpressions[i] = castedIndexParam;
            }


            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            // Access the indexer: propertyType[index]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, indexExpressions);

            Expression body;
            try
            {
                body = returnType == propertyType
                        ? propertyAccess
                        : Expression.Convert(propertyAccess, returnType);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TValue)}' is incompatible with the property tpe. Reason: A conversion from '{propertyData.PropertyTypeData.FullyQualifiedSignature}' to the provided '{returnType.FullName}' is not natively supported.",
                    nameof(TValue),
                    e);
            }

            Expression guardedBody = Expression.Block(validationExpression, body);
            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TValue, TIndex>>(guardedBody, targetParam, indicesParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed delegate that retrieves the value of an indexer property for a specified target
        /// instance and value type.
        /// </summary>
        /// <remarks>The returned delegate can be used to efficiently access the value of an indexer
        /// property at runtime, given a target object and the appropriate index arguments. The property described by
        /// propertyData must be an indexer and must support reading. The types specified by TTarget and TValue must be
        /// compatible with the declaring type and the property type, respectively.</remarks>
        /// <typeparam name="TTarget">The target type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer property.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a getter. Must represent an indexer
        /// property that can be read.</param>
        /// <returns>A delegate that takes a target object and an array of index values, and returns the value of the specified
        /// indexer property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the target type is not assignable to the declaring type of the property, if the property is not an
        /// indexer, if the property is write-only, or if the value type is incompatible with the property's type.</exception>
        public static IndexerPropertyGetter<TTarget, TValue> CreateIndexerGetter<TTarget, TValue>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property is not an indexer. Use the appropriate indexer getter creation method instead.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            TypeData declaringTypeData = propertyData.DeclaringTypeData;
            ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
            Type targetType = typeof(TTarget);
            Type declaringType = declaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(TTarget),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType,
                    "declaring type"));
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();

            // (TTarget target, object[] indices)
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");

            Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: true);

            ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
            Expression[] indexExpressions = new Expression[propertyGetMethodParameters.Count];
            for (int i = 0; i < propertyGetMethodParameters.Count; i++)
            {
                ParameterData indexParameterData = propertyGetMethodParameters[i];
                Type indexParameterType = indexParameterData.ParameterTypeData.UnwrapType();

                // indices[i]
                BinaryExpression indexAccess = Expression.ArrayIndex(
                    indicesParam,
                    Expression.Constant(i));

                // (TIndexType)indices[i]
                Expression castedIndexParam;
                try
                {
                    // Use provided argument type and cast to indexer parameter type if needed.
                    castedIndexParam = Expression.Convert(indexAccess, indexParameterType);
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException(
                        $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion to '{indexParameterData.FullyQualifiedSignature}' is not natively supported.",
                        $"index {i}",
                        e);
                }

                indexExpressions[i] = castedIndexParam;
            }

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            // Access the indexer: propertyType[index]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, indexExpressions);

            Expression body;
            try
            {
                body = returnType == propertyType
                        ? propertyAccess
                        : Expression.Convert(propertyAccess, returnType);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided generic method argument '{nameof(TValue)}' is incompatible with the property's type. Reason: A conversion from '{propertyData.PropertyTypeData.FullyQualifiedSignature}' to the provided '{returnType.FullName}' is not natively supported.",
                    nameof(TValue),
                    e);
            }

            Expression guardedBody = Expression.Block(validationExpression, body);
            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TValue>>(guardedBody, targetParam, indicesParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed delegate that gets the value of a 2D indexer property for a specified
        /// target instance.
        /// </summary>
        /// <remarks>The created delegate performs type checking and conversions as needed to match the
        /// indexer property signature. This method is intended for advanced scenarios such as dynamic property access
        /// or code generation.</remarks>
        /// <typeparam name="TTarget">The target type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TIndex1">The type of the first index parameter of the indexer property.</typeparam>
        /// <typeparam name="TIndex2">The type of the second index parameter of the indexer property.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer property.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a getter delegate. Must represent a
        /// readable indexer property with exactly two index parameters.</param>
        /// <returns>A delegate that gets the value of the specified two-parameter indexer property for a given declaringType object and
        /// index values.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the property is not an indexer, does not have exactly two index parameters, is write-only, or if
        /// the provided generic method arguments are incompatible with the property or its index parameters.</exception>
        public static IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2> CreateIndexerGetter<TTarget, TValue, TIndex1, TIndex2>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property is not an indexer. Use the appropriate indexer getter creation method instead.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            TypeData declaringTypeData = propertyData.DeclaringTypeData;
            ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
            Type targetType = typeof(TTarget);
            Type declaringType = declaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(TTarget),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType,
                    "declaring type"));

            ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                2,
                propertyGetMethodParameters.Count,
                nameof(propertyData),
                "The provided indexer property must have exactly two index parameters to create a 2D indexer getter.");
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            Type index1Type = typeof(TIndex1);
            TypeData indexerParameter1TypeData = propertyGetMethodParameters[0].ParameterTypeData;
            Type indexParameter1Type = indexerParameter1TypeData.UnwrapType();
            Type index2Type = typeof(TIndex2);
            TypeData indexerParameter2TypeData = propertyGetMethodParameters[1].ParameterTypeData;
            Type indexParameter2Type = indexerParameter2TypeData.UnwrapType();

            // (TTarget target, TIndex1 index1, TIndex2 index2)
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression indexParam1 = Expression.Parameter(index1Type, "index1");
            ParameterExpression indexParam2 = Expression.Parameter(index2Type, "index2");

            Expression castedIndex1Param;
            try
            {
                // Use provided argument type and cast to indexer parameter type if needed.
                castedIndex1Param = index1Type != indexParameter1Type
                    ? Expression.Convert(indexParam1, indexParameter1Type)
                    : indexParam1;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided generic method argument '{nameof(TIndex1)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index1Type.ToFullyQualifiedSignatureName()}' to '{indexerParameter1TypeData.FullyQualifiedSignature}' is not natively supported.",
                    nameof(TIndex1),
                    e);
            }

            Expression castedIndex2Param;
            try
            {
                // Use provided argument type and cast to indexer parameter type if needed.
                castedIndex2Param = index2Type != indexParameter2Type
                    ? Expression.Convert(indexParam2, indexParameter2Type)
                    : indexParam2;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TIndex2)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index2Type.FullName}' to '{indexerParameter2TypeData.FullyQualifiedSignature}' is not natively supported.",
                    nameof(TIndex2),
                    e);
            }

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            // Access the indexer: propertyType[index1, index2]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, [castedIndex1Param, castedIndex2Param]);

            Expression body;
            try
            {
                body = returnType == propertyType
                        ? propertyAccess
                        : Expression.Convert(propertyAccess, returnType);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TValue)}' is incompatible with the property type. Reason: A conversion from '{propertyData.PropertyTypeData.FullyQualifiedSignature}' to the provided {returnType.FullName} is not natively supported.",
                    nameof(TValue),
                    e);
            }

            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2>>(body, targetParam, indexParam1, indexParam2)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed delegate that retrieves the value of a 3D indexer property for a
        /// specified target instance.
        /// </summary>
        /// <remarks>The created delegate performs type checking and conversion as needed to match the
        /// indexer property signature. This method is typically used in advanced scenarios such as dynamic property
        /// access or code generation.</remarks>
        /// <typeparam name="TTarget">The target type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TIndex1">The type of the first index parameter of the indexer.</typeparam>
        /// <typeparam name="TIndex2">The type of the second index parameter of the indexer.</typeparam>
        /// <typeparam name="TIndex3">The type of the third index parameter of the indexer.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a getter. Must represent a readable indexer
        /// with exactly three index parameters.</param>
        /// <returns>A delegate that takes a declaringType object and three index values and returns the value of the specified indexer
        /// property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a getter invoker has already been generated for the specified property.</exception>
        /// <exception cref="ArgumentException">Thrown if the provided generic method arguments are not compatible with the indexer property, if the property is not an
        /// indexer, if it does not have exactly three index parameters, or if the property is write-only.</exception>
        public static IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> CreateIndexerGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property is not an indexer. Use the appropriate indexer getter creation method instead.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            TypeData declaringTypeData = propertyData.DeclaringTypeData;
            ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
            Type targetType = typeof(TTarget);
            Type declaringType = declaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(TTarget),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType,
                    "declaring type"));

            ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                3,
                propertyGetMethodParameters.Count,
                nameof(propertyData),
                "The provided indexer property must have exactly three index parameters to create a 3D indexer getter.");
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();

            Type index1Type = typeof(TIndex1);
            TypeData indexerParameter1TypeData = propertyGetMethodParameters[0].ParameterTypeData;
            Type indexParameter1Type = indexerParameter1TypeData.UnwrapType();

            Type index2Type = typeof(TIndex2);
            TypeData indexerParameter2TypeData = propertyGetMethodParameters[1].ParameterTypeData;
            Type indexParameter2Type = indexerParameter2TypeData.UnwrapType();

            Type index3Type = typeof(TIndex3);
            TypeData indexerParameter3TypeData = propertyGetMethodParameters[2].ParameterTypeData;
            Type indexParameter3Type = indexerParameter3TypeData.UnwrapType();

            // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3)
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression indexParam1 = Expression.Parameter(index1Type, "index1");
            ParameterExpression indexParam2 = Expression.Parameter(index2Type, "index2");
            ParameterExpression indexParam3 = Expression.Parameter(index3Type, "index3");

            Expression castedIndex1Param;
            try
            {
                // Use provided argument type and cast to indexer parameter type if needed.
                castedIndex1Param = index1Type != indexParameter1Type
                    ? Expression.Convert(indexParam1, indexParameter1Type)
                    : indexParam1;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TIndex1)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index1Type.FullName}' to '{indexerParameter1TypeData.FullyQualifiedSignature}' is not natively supported.",
                    nameof(TIndex1),
                    e);
            }

            Expression castedIndex2Param;
            try
            {
                // Use provided argument type and cast to indexer parameter type if needed.
                castedIndex2Param = index2Type != indexParameter2Type
                    ? Expression.Convert(indexParam2, indexParameter2Type)
                    : indexParam2;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TIndex2)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index2Type.FullName}' to '{indexerParameter2TypeData.FullyQualifiedSignature}' is not natively supported.",
                    nameof(TIndex2),
                    e);
            }

            Expression castedIndex3Param;
            try
            {
                // Use provided argument type and cast to indexer parameter type if needed.
                castedIndex3Param = index3Type != indexParameter3Type
                    ? Expression.Convert(indexParam3, indexParameter3Type)
                    : indexParam3;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TIndex3)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index3Type.FullName}' to '{indexerParameter3TypeData.FullyQualifiedSignature}' is not natively supported.",
                    nameof(TIndex3),
                    e);
            }

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            // Access the indexer: propertyType[index1, index2, index3]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, [castedIndex1Param, castedIndex2Param, castedIndex3Param]);

            Expression body;
            try
            {
                body = returnType == propertyType
                        ? propertyAccess
                        : Expression.Convert(propertyAccess, returnType);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument '{nameof(TValue)}' is incompatible with the property type. Reason: A conversion from '{propertyData.PropertyTypeData.FullyQualifiedSignature}' to the provided '{returnType.FullName}' is not natively supported.",
                    nameof(TValue),
                    e);
            }

            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>>(body, targetParam, indexParam1, indexParam2, indexParam3)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that retrieves the value of an indexer property for a specified target instance and index
        /// parameters.
        /// </summary>
        /// <remarks>The returned delegate expects the target object to be assignable to the declaring
        /// type of the indexer property. The indices array must match the number and types of the indexer parameters.
        /// This method only supports indexer properties that can be read; attempting to use it with write-only or
        /// non-indexer properties will result in an exception.</remarks>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a getter delegate. Must represent an
        /// indexer property that supports reading.</param>
        /// <returns>A delegate that takes a target object and an array of index values, and returns the value of the specified
        /// indexer property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyData"/> is <see langword="null"/> or <see cref="MemberData.DeclaringTypeData"/> property of <paramref name="propertyData"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified property is not an indexer.</exception>
        public static Func<object?, object[], object?> CreateIndexerGetter(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            TypeData declaringTypeData = propertyData.DeclaringTypeData;
            ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");


            // (object? target, object[] indices)
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");

            // Validate that indices length matches the number of index parameters when not null.
            // We do this inside the expression so the check happens at runtime.
            Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: true);

            ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
            Expression[] indexExpressions = new Expression[propertyGetMethodParameters.Count];
            for (int i = 0; i < propertyGetMethodParameters.Count; i++)
            {
                ParameterData indexParameterData = propertyGetMethodParameters[i];
                Type indexParameterType = indexParameterData.ParameterTypeData.UnwrapType();

                // indices[i]
                BinaryExpression indexAccess = Expression.ArrayIndex(
                    indicesParam,
                    Expression.Constant(i));

                // (TIndexType)indices[i]
                Expression castedIndexParam;
                try
                {
                    // Use provided argument type and cast to indexer parameter type if needed.
                    castedIndexParam = Expression.Convert(indexAccess, indexParameterType);
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException(
                        $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion to '{indexParameterData.FullyQualifiedSignature}' is not natively supported.",
                        $"index {i}",
                        e);
                }
            }

            Type declaringType = declaringTypeData.UnwrapType();
            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : Expression.Convert(targetParam, declaringType);

            // Access the indexer: propertyType[index0, index1, ...]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, indexExpressions);

            // Box the result
            UnaryExpression body = Expression.Convert(propertyAccess, typeof(object));
            Expression guardedBody = Expression.Block(validationExpression, body);

            // Build the delegate: Func<object?, object?[]?, object?>
            return Expression
                .Lambda<Func<object?, object[], object?>>(guardedBody, targetParam, indicesParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the value of the specified property on a given target instance.
        /// </summary>
        /// <remarks>The returned delegate uses object-based parameters. For instance properties declared
        /// on value types (structs), use a ref-based setter to avoid modifying a boxed copy. The property must be
        /// writable and not read-only.</remarks>
        /// <param name="propertyData">The metadata describing the property for which to create a setter. Must represent a writable property.
        /// Cannot be null.</param>
        /// <returns>An <see cref="Action{Object, Object}"/> delegate that sets the value of the specified property on a propertyType
        /// object. The first parameter is the propertyType object instance (or null for static properties); the second
        /// parameter is the value to assign to the property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
        /// <exception cref="NotSupportedException">Thrown if the property is an instance property declared on a value type. Use a ref-based setter instead.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyData"/> is <see langword="null"/>.</exception>
        public static Action<object?, object?> CreateSetter(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasSetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the set invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));

            // Important: setting instance properties on a boxed struct would modify only a copy.
            if (!propertyData.IsStatic && propertyData.DeclaringTypeData.IsValueType)
            {
                throw new NotSupportedException(
                    "Cannot create an object-based setter for an instance property declared on a value type. " +
                    $"You need a ref-based setter: call '{nameof(CreateStructSetter)}' instead.");
            }

            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            // (object? target, object? value)
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");

            PropertyInfo property = propertyData.GetPropertyInfo();
            Expression propertyAccess =
                propertyData.IsStatic
                    ? Expression.Property(expression: null, property)
                    : Expression.Property(
                        Expression.Convert(targetParam, propertyData.DeclaringTypeData.UnwrapType()),
                        property);

            Type type = propertyData.PropertyTypeData.UnwrapType();
            BinaryExpression assign = Expression.Assign(
                propertyAccess,
                Expression.Convert(valueParam, type)); // Expression.Assign 

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<Action<object?, object?>>(body, targetParam, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed delegate that sets the value of a property on a specified target instance.
        /// </summary>
        /// <remarks>This method validates that the property is writable and that the provided type
        /// arguments are compatible with the property's declaring type and value type. For instance properties on value
        /// types, use a ref-based setter to avoid modifying a copy of the struct.</remarks>
        /// <typeparam name="TTarget">The target type of the object that declares the property to be set.</typeparam>
        /// <typeparam name="TValue">The type of the value to assign to the property.</typeparam>
        /// <param name="propertyData">The metadata describing the property for which to create a setter. Must represent a writable property and
        /// cannot be null.</param>
        /// <returns>A delegate that sets the value of the specified property on a target instance of type <typeparamref
        /// name="TTarget"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
        /// <exception cref="NotSupportedException">Thrown if attempting to create a setter for an instance property declared on a value type. Use a ref-based
        /// setter instead.</exception>
        /// <exception cref="ArgumentException">Thrown if the property is read-only or if the provided generic method arguments are incompatible or if the property is an indexer.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyData"/> is <see langword="null"/> or <see cref="MemberData.DeclaringTypeData"/> property of the <paramref name="propertyData"/> is <see langword="null"/>.</exception>
        public static PropertySetter<TTarget, TValue> CreateSetter<TTarget, TValue>(PropertyData propertyData) where TTarget : class
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasSetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the set invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));

            // Important: setting instance properties on a boxed struct would modify only a copy.
            if (!propertyData.IsStatic && propertyData.DeclaringTypeData.IsValueType)
            {
                throw new NotSupportedException(
                    "Cannot create an object-based setter for an instance property declared on a value type. " +
                    $"You need a ref-based setter: call '{nameof(CreateStructSetter)}' instead.");
            }

            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must not be an indexer to create a non-indexer setter.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                nameof(TTarget),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType,
                    "declaring type"));

            Type valueType = typeof(TValue);

            // (TTarget target, TValue value)
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression valueParam = Expression.Parameter(valueType, "value");

            PropertyInfo property = propertyData.GetPropertyInfo();
            Expression propertyAccess =
                propertyData.IsStatic
                    ? Expression.Property(expression: null, property)
                    : Expression.Property(
                        Expression.Convert(targetParam, declaringType),
                        property);

            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            Expression value;
            try
            {
                value = valueType != propertyType
                    ? Expression.Convert(valueParam, propertyType)
                    : valueParam;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"Type mismatch. Unable to convert the provided '{nameof(TValue)}' type '{valueType.ToFullyQualifiedSignatureName()}' to the property's type '{propertyData.PropertyTypeData.FullyQualifiedSignature} is incompatible with the property's index parameter type. Reason: A conversion from the provided ",
                    nameof(TValue),
                    e);
            }

            BinaryExpression assign = Expression.Assign(
                propertyAccess,
                value); // Expression.Assign

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<PropertySetter<TTarget, TValue>>(body, targetParam, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the value of an indexer property on a specified target instance using the provided
        /// indices.
        /// </summary>
        /// <remarks>
        /// For static indexers, the target parameter is ignored. The method validates that the number of
        /// indices matches the indexer signature at runtime. Attempting to use this setter on a boxed struct will not
        /// modify the original instance - hence this method will not support value types; use a ref-based overload for value types.</remarks>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a setter delegate. Must represent a
        /// writable indexer property.</param>
        /// <returns>An <see cref="Action{Object, Object[], Object}"/> delegate that sets the value of the specified indexer
        /// property on a reference type using the given indices and value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
        /// <exception cref="NotSupportedException">Thrown if the indexer property is an instance property declared on a value type. Use a ref-based setter
        /// instead.</exception>
        public static Action<object?, object[], object?> CreateIndexerSetter(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasSetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the set invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));

            // Important: setting instance properties on a boxed struct would modify only a copy.
            if (!propertyData.IsStatic && propertyData.DeclaringTypeData.IsValueType)
            {
                throw new NotSupportedException(
                    "Cannot create an object-based setter for an instance property declared on a value type. " +
                    $"You need a ref-based setter: call '{nameof(CreateStructIndexerSetter)}' instead.");
            }

            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            // (object? target, object?[]? indices)
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "returnType");


            // Validate that indices length matches the number of index parameters when not null.
            // We do this inside the expression so the check happens at runtime.
            Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: false);

            ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;
            Expression[] indexExpressions = new Expression[propertySetMethodParameters.Count];
            for (int i = 0; i < propertySetMethodParameters.Count; i++)
            {
                ParameterData indexParameterData = propertySetMethodParameters[i];
                Type indexParameterType = indexParameterData.ParameterTypeData.UnwrapType();

                // indices[i]
                BinaryExpression indexAccess = Expression.ArrayIndex(
                    indicesParam,
                    Expression.Constant(i));

                // (TIndexType)indices[i]
                Expression castedIndexParam;
                try
                {
                    // Use provided argument type and cast to indexer parameter type if needed.
                    castedIndexParam = Expression.Convert(indexAccess, indexParameterType);
                }
                catch (InvalidOperationException e)
                {
                    throw new ArgumentException(
                        $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion to '{indexParameterData.FullyQualifiedSignature}' is not natively supported.",
                        $"Index {i}",
                        e);
                }
            }

            // Access the indexer: propertyType[index0, index1, ...]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : Expression.Convert(targetParam, declaringType);
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, indexExpressions);
            BinaryExpression assign = Expression.Assign(
                propertyAccess,
                Expression.Convert(valueParam, propertyData.PropertyTypeData.UnwrapType())); // Expression.Assign 

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());
            Expression guardedBody = Expression.Block(validationExpression, body);

            // Build the delegate: Func<object?, object?[]?, object?>
            return Expression
                .Lambda<Action<object?, object?[]?, object?>>(guardedBody, targetParam, indicesParam, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed setter delegate for an instance property of a value type (struct).
        /// </summary>
        /// <remarks>Use this method to generate a setter for struct instance properties. For static or
        /// reference type properties, use a <code>CreateSetter</code> overload instead. The returned delegate operates on a struct passed by
        /// reference, allowing direct property assignment.</remarks>
        /// <typeparam name="TTarget">The target type of the value t ype/struct instance that declares the property to set.</typeparam>
        /// <typeparam name="TValue">The type of the value to assign to the property.</typeparam>
        /// <param name="propertyData">The metadata describing the property for which to create a setter. Must represent a non-static,
        /// non-read-only property of the specified value type.</param>
        /// <returns>A delegate that sets the returnType of the specified property on a given struct instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the property is static, read-only, or if the provided generic method arguments are incompatible.</exception>"
        public static ValueTypePropertySetter<TTarget, TValue> CreateStructSetter<TTarget, TValue>(PropertyData propertyData)
            where TTarget : struct
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasSetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the set invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsStatic,
                nameof(propertyData),
                $"For reference type instance properties or class properties (static) call '{nameof(CreateSetter)}' instead.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(TTarget),
                        declaringType,
                        "declaring type"));

            Type valueType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();

            // (ref TTarget target, TValue value) => propertyType.Property = value;
            Type refTargetType = targetType.MakeByRefType();
            ParameterExpression targetByRef = Expression.Parameter(refTargetType, "target");
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

            PropertyInfo property = propertyData.GetPropertyInfo();
            MemberExpression propertyAccess = Expression.Property(targetByRef, property);

            UnaryExpression castedValue;
            try
            {
                castedValue = Expression.Convert(valueParam, propertyType);
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"Type mismatch. The generic method parameter '{nameof(TValue)}' type {valueType.ToFullyQualifiedSignatureName()} is not assignable to the property '{propertyData.FullyQualifiedSignature}'.",
                    nameof(TValue),
                    e);
            }

            BinaryExpression assign = Expression.Assign(propertyAccess, castedValue);
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypePropertySetter<TTarget, TValue>>(body, targetByRef, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed setter delegate for an instance property of a value type (struct).
        /// </summary>
        /// <remarks>Use this method to generate a setter for struct instance properties. For static or
        /// reference type properties, use a <code>CreateSetter</code> overload instead. The returned delegate operates on a struct passed by
        /// reference, allowing direct property assignment.</remarks>
        /// <typeparam name="TTarget">The target type of the value t ype/struct instance that declares the property to set.</typeparam>
        /// <typeparam name="TValue">The type of the value to assign to the property.</typeparam>
        /// <param name="propertyData">The metadata describing the property for which to create a setter. Must represent a non-static,
        /// non-read-only property of the specified value type.</param>
        /// <returns>A delegate that sets the returnType of the specified property on a given struct instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the property is static, read-only, or if the provided generic method arguments are incompatible.</exception>"
        public static ValueTypePropertySetter CreateStructSetter(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasSetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the set invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsStatic,
                nameof(propertyData),
                $"For reference type instance properties or class properties (static) call '{nameof(CreateSetter)}' instead.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(object);
            Type valueType = typeof(object);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();

            // (object target, object? value) => propertyType.Property = value;
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression valueParam = Expression.Parameter(valueType, "value");

            PropertyInfo property = propertyData.GetPropertyInfo();
            MemberExpression propertyAccess = Expression.Property(targetParam, property);

            UnaryExpression castedValue = null;
            try
            {
                castedValue = Expression.Convert(valueParam, propertyType);
            }
            catch (InvalidOperationException e)
            {
                //throw new ArgumentException(
                //    $"Type mismatch. The type of the provided value is not assignable to the property '{propertyData.FullyQualifiedSignature}'.",
                //    nameof(TValue),
                //    e);
            }

            BinaryExpression assign = Expression.Assign(propertyAccess, castedValue);
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypePropertySetter>(body, targetParam, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the returnType of an indexer property on a value type instance.
        /// </summary>
        /// <remarks>Use this method to generate a performant setter for struct indexer properties when
        /// reflection-based property access is required. The returned delegate expects the value type/struct to be passed
        /// by reference, along with the index parameters and the type of the value to set.</remarks>
        /// <typeparam name="TTarget">The value type that declares the indexer property. Must be a struct.</typeparam>
        /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
        /// non-read-only indexer property and cannot be null.</param>
        /// <returns>A delegate that sets the returnType of the specified indexer property on a value type instance using the provided
        /// indices and returnType.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
        public static ValueTypeIndexerPropertySetter<TTarget, TValue> CreateStructIndexerSetter<TTarget, TValue>(PropertyData propertyData)
            where TTarget : struct
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasSetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the set invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsStatic,
                nameof(propertyData),
                $"For reference instance properties or class properties (static) call {nameof(CreateSetter)} instead.");
            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        nameof(TTarget),
                        declaringType,
                        "declaring type"));

            Type valueType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                valueType,
                propertyType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        valueType,
                        nameof(TValue),
                        propertyType,
                        "property prpertyType"));

            // (ref TTarget propertyType, TValue returnType) => propertyType.Property = returnType;
            ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "propertyType");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "returnType");

            ImmutableArray<ParameterInfo> indexParameters = propertyData.PropertySetMethodParameters.AsParameterInfoArray();

            // Validate that indices length matches the number of index parameters when not null.
            // We do this inside the expression so the check happens at runtime.
            Expression[] indexExpressions = new Expression[indexParameters.Length];
            for (int i = 0; i < indexParameters.Length; i++)
            {
                ParameterInfo indexParameterInfo = indexParameters[i];
                Type parameterType = indexParameterInfo.ParameterType;

                // indices[i]
                BinaryExpression indexAccess = Expression.ArrayIndex(
                    indicesParam,
                    Expression.Constant(i));

                // (TIndexType)indices[i]
                UnaryExpression convertedIndex = Expression.Convert(indexAccess, parameterType);
                indexExpressions[i] = convertedIndex;
            }

            PropertyInfo property = propertyData.GetPropertyInfo();

            Expression? instanceExpression = Expression.Convert(targetByRef, declaringType);
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, indexExpressions);
            BinaryExpression assign = Expression.Assign(
                propertyAccess,
                Expression.Convert(valueParam, propertyData.PropertyTypeData.UnwrapType())); // Expression.Assign 

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypeIndexerPropertySetter<TTarget, TValue>>(body, targetByRef, indicesParam, valueParam)
                .Compile();
        }

        private static Expression CreateIndexParameterArrayLengthMismatchExceptionExpression(PropertyData propertyData, ParameterExpression indicesParam, bool isGetter)
        {
            ParameterList indexParameters = isGetter
                ? propertyData.PropertyGetMethodParameters
                : propertyData.PropertySetMethodParameters;
            Expression lengthMismatch = Expression.NotEqual(
                            Expression.ArrayLength(indicesParam),
                            Expression.Constant(indexParameters.Count));
            Expression foundIndexCount = Expression.ArrayLength(indicesParam);
            MethodInfo stringConcat5 = typeof(string).GetMethod(
                nameof(string.Concat),
                [
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string)
                    ])!;

            Expression message = Expression.Call(
                stringConcat5,
                Expression.Constant($"The provided number of indexer parameters does not match the indexer parameter count of the property '{propertyData.FullyQualifiedSignature}'. Expected: "),
                Expression.Constant(indexParameters.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                Expression.Constant(", Found: "),
                Expression.Call(foundIndexCount, nameof(int.ToString), Type.EmptyTypes),
                Expression.Constant("."));

            Expression throwLengthMismatch = Expression.Throw(
                Expression.New(
                    typeof(ArgumentOutOfRangeException).GetConstructor([typeof(string), typeof(string)])!,
                    Expression.Constant("indices"),
                    message),
                typeof(void));
            Expression validateLength = Expression.IfThen(lengthMismatch, throwLengthMismatch);
            return validateLength;
        }

        private static Expression CreateTargetTypeMismatchExceptionExpression(PropertyData propertyData, ParameterExpression target)
        {
            BinaryExpression isTargetNull = Expression.Equal(target, Expression.Constant(null, typeof(object)));
            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Expression isTargetInvalid = !propertyData.IsStatic
                ? Expression.OrElse(
                    isTargetNull,
                     Expression.Not(Expression.TypeIs(target, declaringType)))
                : Expression.Constant(false);

            MethodInfo stringConcat5 = typeof(string).GetMethod(
                nameof(string.Concat),
                [
                        typeof(string),
                        typeof(string),
                        typeof(string),
                        typeof(string),
                    ])!;

            TypeData helperExtensionsCommonTypeData = SymbolReflectionInfoCache.GetOrCreateSymbolInfoDataCacheEntry(typeof(HelperExtensionsCommon));
            const string extensionMethodName = nameof(HelperExtensionsCommon.ToFullyQualifiedSignatureName);
            var typeCacheKey = SymbolInfoDataCacheKey.CreateForType(typeof(Type));
            MethodData toFullyQualifiedSignatureNameExtensionMethodData = helperExtensionsCommonTypeData.Methods[extensionMethodName, new MethodParameterInfo(typeCacheKey)];

            // BUG::Call GetType on target and pass to extension method
            MethodCallExpression extensionMethodCall = Expression.Call(toFullyQualifiedSignatureNameExtensionMethodData.GetMethodInfo(), target);

            Expression message = Expression.Call(
                stringConcat5,
                Expression.Constant($"The type of the provided target instance is not assignable to the declaring type of the property. Expected: '{propertyData.DeclaringTypeData.FullyQualifiedSignature}' "),
                Expression.Constant(", Found: "),
                Expression.Condition(isTargetNull, Expression.Constant("null", typeof(string)), extensionMethodCall),
                Expression.Constant("."));

            Expression throwLengthMismatch = Expression.Throw(
                Expression.New(
                    typeof(ArgumentException).GetConstructor([typeof(string), typeof(string)])!,
                    message,
                    Expression.Constant("target")),
                typeof(void));
            Expression validateTargetType = Expression.IfThen(isTargetInvalid, throwLengthMismatch);
            return validateTargetType;
        }

        //private static Expression CreateValueTypeMismatchExceptionExpression(PropertyData propertyData, ParameterExpression value)
        //{
        //    Expression isTargetValid = propertyData.IsStatic
        //        ? Expression.Constant(true)
        //        : Expression.AndAlso(
        //            Expression.NotEqual(value, Expression.Constant(null, typeof(object))), Expression.TypeIs(value, propertyData.DeclaringTypeData.UnwrapType()));

        //    MethodInfo stringConcat5 = typeof(string).GetMethod(
        //        nameof(string.Concat),
        //        [
        //                typeof(string),
        //                typeof(string),
        //                typeof(string),
        //                typeof(string),
        //                typeof(string)
        //            ])!;

        //    Expression message = Expression.Call(
        //        stringConcat5,
        //        Expression.Constant($"The type of the provided {(forTarget ? "target instance" : "value")} is not assignable to the {(forTarget ? "declaring type" : "property type")}. Expected: '{(forTarget ? propertyData.DeclaringTypeData.FullyQualifiedSignature : propertyData.PropertyTypeData.FullyQualifiedSignature)}' "),
        //        Expression.Constant(propertyData.IndexerParameters.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        //        Expression.Constant(", Found: "),
        //        Expression.Call(value, nameof(int.ToString), Type.EmptyTypes),
        //        Expression.Constant("."));

        //    Expression throwLengthMismatch = Expression.Throw(
        //        Expression.New(
        //            typeof(ArgumentException).GetConstructor([typeof(string), typeof(string)])!,
        //            Expression.Constant(forTarget ? "target" : "value"),
        //            message),
        //        typeof(void));
        //    Expression validateLength = Expression.IfThen(lengthMismatch, throwLengthMismatch);
        //    return validateLength;
        //}
    }
}
