namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
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

public delegate void PropertySetter<TTarget, TValue>(TTarget? target, TValue value) where TTarget : class?;
public delegate void ValueTypePropertySetter<TTarget, TValue>(ref TTarget target, TValue value) where TTarget : struct;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate void IndexerPropertySetter<TTarget, TValue, TIndex>(TTarget? target, TValue value, params TIndex[] index) where TTarget : class?;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex>(ref TTarget target, TValue value, params TIndex[] index) where TTarget : struct;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate void IndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>(TTarget? target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : class?;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : struct;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate void IndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget? target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : class?;
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : struct;

public delegate TValue PropertyGetter<TTarget, TValue>(TTarget target);
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate TValue IndexerPropertyGetter<TTarget, TValue, TIndex>(TTarget target, params TIndex[] index);
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate TValue IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TIndex1 index1, TIndex2 index2);
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", Justification = "<Pending>")]
public delegate TValue IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3);

internal static class DelegateProvider
{
    private static readonly ConcurrentDictionary<MethodDataGenericTypeVariantKey, SymbolReflectionInfoCacheKey> s_invocatorKeyMap = new();

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
            ? targetMethodData.ReturnTypeData.Type
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
            instance = Expression.Convert(targetParam, methodData.DeclaringTypeData.Type!);
        }

        UnaryExpression[] callArgs = methodData.Parameters.Select((parameter, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                parameter.ParameterTypeData.Type)).ToArray();

        MethodInfo methodInfo = methodData.MethodInfo;
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
        Type declaringType = targetMethodData.DeclaringTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            targetType,
            targetMethodData.DeclaringTypeData.Type!,
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
            : declaringType != targetType
                ? Expression.Convert(targetParam, declaringType)
                : targetParam;

        UnaryExpression[] callArgs = methodData.Parameters.Select((parameter, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                parameter.ParameterTypeData.Type)).ToArray();

        MethodInfo methodInfo = methodData.MethodInfo;
        Expression call = methodData.IsStatic
            ? Expression.Call(methodInfo, callArgs)
            : Expression.Call(instanceExpression, methodInfo, callArgs); // instance required for non-static :contentReference[oaicite:7]{index=7}

        Type methodReturnType = targetMethodData.ReturnTypeData.Type;
        Delegate? invocator = null;
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
        Type declaringType = targetMethodData.DeclaringTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            targetType,
            targetMethodData.DeclaringTypeData.Type!,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    nameof(TTarget),
                    declaringType!,
                    "declaring type"));

        Type resultType = typeof(void);

        var genericTypedMethodVariantKey = new MethodDataGenericTypeVariantKey(
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
            : declaringType != targetType
                ? Expression.Convert(targetParam, declaringType)
                : targetParam;

        UnaryExpression[] callArgs = methodData.Parameters.Select((parameter, index) =>
            Expression.Convert(
                Expression.ArrayIndex(argsParam, Expression.Constant(index)),
                parameter.ParameterTypeData.Type)).ToArray();

        MethodInfo methodInfo = methodData.MethodInfo;
        Expression call = methodData.IsStatic
            ? Expression.Call(methodInfo, callArgs)
            : Expression.Call(instanceExpression, methodInfo, callArgs); // instance required for non-static :contentReference[oaicite:7]{index=7}

        Type methodReturnType = targetMethodData.ReturnTypeData.Type;
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

        Type methodReturnType = targetMethodData.ReturnTypeData.Type;

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
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo<Task>(resultType, ExceptionMessages.GetTypeMismatchExceptionMessage(
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
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo<Task>(resultType, ExceptionMessages.GetTypeMismatchExceptionMessage(
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
        if (DelegateProvider.s_invocatorKeyMap.TryGetValue(invocatorKeyMapKey, out SymbolReflectionInfoCacheKeyInternal symbolInfoCacheKey)
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
        _ = DelegateProvider.s_invocatorKeyMap.TryAdd(invocatorKeyMapKey, closedGenericMethodData.View.CacheKey);
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
        ArgumentNullException.ThrowIfNull(eventData);

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

        FieldInfo field = fieldData.FieldInfo;

        // (object? propertyType) => (object?)((TDeclaring)propertyType).Field
        ParameterExpression targetParam = Expression.Parameter(typeof(object), "propertyType");

        Expression fieldAccess =
            field.IsStatic
                ? Expression.Field(expression: null, field) // static: no instance
                : Expression.Field(
                    Expression.Convert(targetParam, fieldData.DeclaringTypeData.Type), // cast/unbox
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

        FieldInfo field = fieldData.FieldInfo;
        Expression fieldAccess =
            fieldData.IsStatic
                ? Expression.Field(expression: null, field)
                : Expression.Field(
                    Expression.Convert(targetParam, fieldData.DeclaringTypeData.Type),
                    field);

        BinaryExpression assign = Expression.Assign(
            fieldAccess,
            Expression.Convert(valueParam, fieldData.FieldTypeData.Type)); // Expression.Assign 

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

        Type declaringType = fieldData.DeclaringTypeData.Type;
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
        Type fieldType = fieldData.FieldTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            fieldType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    fieldType,
                    "field propertyType"));

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

        FieldInfo field = fieldData.FieldInfo;
        MemberExpression fieldAccess = Expression.Field(targetByRef, field);
        BinaryExpression assign = Expression.Assign(fieldAccess, Expression.Convert(valueParam, fieldType));
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<ValueTypeMemberSetter<TTarget, TValue>>(body, targetByRef, valueParam)
            .Compile();
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
    /// <exception cref="ArgumentException">Thrown if the property is an indexer, is static, is write-only, if the generic method arguments are incompatible with the
    /// property or declaring type, or if the property type cannot be cast to the specified return type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the provided property data is <see langword="null"/>.</exception>
    public static PropertyGetter<TTarget, TValue> CreateGetter<TTarget, TValue>(PropertyData propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            !propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is an indexer or static. Use the appropriate getter creation method instead.");
        ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

        Type targetType = typeof(TTarget);
        Type declaringType = propertyData.DeclaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;

        // (TTarget target) => (TValue)target.Property
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");

        Expression? instanceExpression = declaringType != targetType
            ? Expression.Convert(targetParam, declaringType)
            : targetParam;

        PropertyInfo property = propertyData.PropertyInfo;
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
    /// Creates a strongly-typed delegate that retrieves the value of a specified static property.
    /// </summary>
    /// <remarks>The created delegate provides efficient access to the property value and supports
    /// both static and instance properties. For indexer properties, use the appropriate indexer getter creation
    /// method instead.</remarks>
    /// <typeparam name="TValue">The property type of the value returned by the static property getter.</typeparam>
    /// <param name="propertyData">The metadata describing the property for which to create a getter. Must represent a readable, non-indexer
    /// property.</param>
    /// <returns>A delegate that returns the value of the specified static property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is an indexer, not static, is write-only, if the generic method arguments are incompatible with the
    /// property, or if the property type cannot be cast to the specified return type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyData"/> or its declaring type data is <see langword="null">.</exception>"
    public static PropertyGetter<object?, TValue> CreateStaticGetter<TValue>(PropertyData propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            !propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is an indexer or not static. Use the appropriate indexer getter creation method instead.");
        ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

        Type targetType = typeof(object);
        Type declaringType = propertyData.DeclaringTypeData.Type;
        Type returnType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;

        // (TTarget target) => (TValue)target.Property
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");

        Expression? instanceExpression = null;

        PropertyInfo property = propertyData.PropertyInfo;
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
            .Lambda<PropertyGetter<object?, TValue>>(body, targetParam)
            .Compile(); // compiles to a delegate 
    }

    /// <summary>
    /// Creates a strongly typed getter delegate for an indexer property on the specified target instance.
    /// </summary>
    /// <remarks>The created delegate provides efficient, strongly typed access to the indexer
    /// property. The property described by <paramref name="propertyData"/> must be an indexer with at least one index parameter, and
    /// the types specified by <typeparamref name="TTarget"/>, <typeparamref name="TIndex"/>, and <typeparamref name="TValue"/> must be compatible with the declaring type, index
    /// parameter type, and property type, respectively.</remarks>
    /// <typeparam name="TTarget">The target type of the object that declares the indexer property.</typeparam>
    /// <typeparam name="TIndex">The type of the index parameter accepted by the indexer.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create the getter. Must represent a readable
    /// indexer with at least one index parameter.</param>
    /// <returns>A delegate that gets the value of the specified indexer property for a given declaringType object and index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the specified property is not an indexer, or is static, or if the types specified by <typeparamref name="TTarget"/>, <typeparamref name="TIndex"/>, or <typeparamref name="TValue"/> are not compatible with the declaring type, index parameter type, or property type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyData"/> or its declaring type data is <see langword="null">.</exception>"
    public static IndexerPropertyGetter<TTarget, TValue, TIndex> CreateIndexerGetter<TTarget, TValue, TIndex>(PropertyData propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is not an instance indexer. Use the appropriate indexer getter creation method instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
        TypeData declaringTypeData = propertyData.DeclaringTypeData;
        ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
        Type targetType = typeof(TTarget);
        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;

        // (TTarget declaringType, TIndex[] indices)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression indicesParam = Expression.Parameter(typeof(TIndex[]), "indices");

        // Validate indices length
        Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: true);

        Type indexType = typeof(TIndex);
        ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;

        var indexExpressions = new Expression[propertyGetMethodParameters.Count];
        for (int i = 0; i < propertyGetMethodParameters.Count; i++)
        {
            // indices[i]
            BinaryExpression indexAccess = Expression.ArrayIndex(
                indicesParam,
                Expression.Constant(i));

            TypeData propertyIndexTypeData = propertyGetMethodParameters[i].ParameterTypeData;
            Type propertyIndexType = propertyIndexTypeData.Type;

            // (TIndexType)indices[i]
            Expression castedIndexParam;
            try
            {
                // Use provided argument type  and cast to indexer parameter type if needed.
                castedIndexParam = indexType != propertyIndexType
                    ? Expression.Convert(indexAccess, propertyIndexType)
                    : indexAccess;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{indexType.FullName}' to '{propertyIndexTypeData.FullyQualifiedSignature}' is not natively supported.",
                    $"index {i}",
                    e);
            }

            indexExpressions[i] = castedIndexParam;
        }

        Expression? instanceExpression = declaringType != targetType
            ? Expression.Convert(targetParam, declaringType)
            : targetParam;

        // Access the indexer: propertyType[index]
        PropertyInfo propertyInfo = propertyData.PropertyInfo;
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
    /// Creates a strongly typed getter delegate for a static indexer property on the specified target instance.
    /// </summary>
    /// <remarks>The created delegate provides efficient, strongly typed access to the indexer
    /// property. The property described by <paramref name="propertyData"/> must be an indexer with exactly one index parameter, and
    /// the types specified by <typeparamref name="TIndex"/>, and <typeparamref name="TValue"/> must be compatible with the index
    /// parameter type, and property type, respectively.</remarks>
    /// <typeparam name="TIndex">The type of the index parameter accepted by the indexer.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
    /// <param name="propertyData">The metadata describing the static indexer property for which to create the getter. Must represent a readable
    /// indexer with exactly one index parameter.</param>
    /// <returns>A delegate that gets the value of the specified static indexer property for a given declaringType object and index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the specified property is not an static indexer, or not static, or if the types specified by <typeparamref name="TIndex"/>, or <typeparamref name="TValue"/> are not compatible with the index parameter type, or property type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertyData"/> or its declaring type data is <see langword="null">.</exception>"
    public static IndexerPropertyGetter<object?, TValue, TIndex> CreateStaticIndexerGetter<TValue, TIndex>(PropertyData propertyData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is not a static indexer. Use the appropriate indexer getter creation method instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
        TypeData declaringTypeData = propertyData.DeclaringTypeData;
        ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
        Type targetType = typeof(object);
        Type declaringType = declaringTypeData.Type;
        Type returnType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;

        // (TTarget declaringType, TIndex[] indices)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression indicesParam = Expression.Parameter(typeof(TIndex[]), "indices");

        // Validate indices length
        Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: true);

        Type indexType = typeof(TIndex);
        ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;

        var indexExpressions = new Expression[propertyGetMethodParameters.Count];
        for (int i = 0; i < propertyGetMethodParameters.Count; i++)
        {
            // indices[i]
            BinaryExpression indexAccess = Expression.ArrayIndex(
                indicesParam,
                Expression.Constant(i));

            TypeData propertyIndexTypeData = propertyGetMethodParameters[i].ParameterTypeData;
            Type propertyIndexType = propertyIndexTypeData.Type;

            // (TIndexType)indices[i]
            Expression castedIndexParam;
            try
            {
                // Use provided argument type  and cast to indexer parameter type if needed.
                castedIndexParam = indexType != propertyIndexType
                    ? Expression.Convert(indexAccess, propertyIndexType)
                    : indexAccess;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion to '{propertyIndexTypeData.FullyQualifiedSignature}' is not natively supported.",
                    $"index {i}",
                    e);
            }

            indexExpressions[i] = castedIndexParam;
        }

        Expression? instanceExpression = null;

        // Access the indexer: propertyType[index]
        PropertyInfo propertyInfo = propertyData.PropertyInfo;
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
            .Lambda<IndexerPropertyGetter<object?, TValue, TIndex>>(guardedBody, targetParam, indicesParam)
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
    /// <exception cref="ArgumentException">Thrown if the property is not an indexer, or is static, does not have exactly two index parameters, is write-only, or if
    /// the provided generic method arguments are incompatible with the property or its index parameters.</exception>
    public static IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2> Create2DIndexerGetter<TTarget, TValue, TIndex1, TIndex2>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is not an indexer or is static. Use the appropriate indexer getter creation method instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        TypeData declaringTypeData = propertyData.DeclaringTypeData;
        ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
        Type targetType = typeof(TTarget);
        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertyGetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertyGetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        // (TTarget target, TIndex1 index1, TIndex2 index2)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");

        Expression castedIndex1Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided generic method argument '{nameof(TIndex1)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index1Type.ToFullyQualifiedSignatureName()}' to '{propertyIndex1TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex1),
                e);
        }

        Expression castedIndex2Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex2)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index2Type.FullName}' to '{propertyIndex2TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex2),
                e);
        }

        Expression? instanceExpression = declaringType != targetType
            ? Expression.Convert(targetParam, declaringType)
            : targetParam;

        // Access the indexer: propertyType[index1, index2]
        PropertyInfo propertyInfo = propertyData.PropertyInfo;
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
            .Lambda<IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2>>(body, targetParam, index1Param, index2Param)
            .Compile();
    }

    /// <summary>
    /// Creates a strongly-typed delegate that gets the value of a static 2D indexer property.
    /// </summary>
    /// <remarks>The created delegate performs type checking and conversions as needed to match the
    /// indexer property signature. This method is intended for advanced scenarios such as dynamic property access
    /// or code generation.</remarks>
    /// <typeparam name="TIndex1">The type of the first index parameter of the indexer property.</typeparam>
    /// <typeparam name="TIndex2">The type of the second index parameter of the indexer property.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the static indexer property for which to create a getter delegate. Must represent a
    /// readable indexer property with exactly two index parameters.</param>
    /// <returns>A delegate that gets the value of the specified static two-parameter indexer property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is not an indexer, or is not static, does not have exactly two index parameters, is write-only, or if
    /// the provided generic method arguments are incompatible with the property or its index parameters.</exception>
    public static IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2> CreateStatic2DIndexerGetter<TValue, TIndex1, TIndex2>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is not a static indexer. Use the appropriate indexer getter creation method instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        TypeData declaringTypeData = propertyData.DeclaringTypeData;
        ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
        Type targetType = typeof(object);
        Type declaringType = declaringTypeData.Type;

        ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            propertyGetMethodParameters.Count,
            nameof(propertyData),
            "The provided indexer property must have exactly two index parameters to create a 2D indexer getter.");
        Type returnType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertyGetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertyGetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        // (TTarget target, TIndex1 index1, TIndex2 index2)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");

        Expression castedIndex1Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided generic method argument '{nameof(TIndex1)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index1Type.ToFullyQualifiedSignatureName()}' to '{propertyIndex1TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex1),
                e);
        }

        Expression castedIndex2Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex2)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index2Type.FullName}' to '{propertyIndex2TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex2),
                e);
        }

        Expression? instanceExpression = null;

        // Access the indexer: propertyType[index1, index2]
        PropertyInfo propertyInfo = propertyData.PropertyInfo;
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
            .Lambda<IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2>>(body, targetParam, index1Param, index2Param)
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
    /// indexer or is static, if it does not have exactly three index parameters, or if the property is write-only.</exception>
    public static IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> Create3DIndexerGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is not an indexer or is static. Use the appropriate indexer getter creation method instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        TypeData declaringTypeData = propertyData.DeclaringTypeData;
        ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
        Type targetType = typeof(TTarget);
        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertyGetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertyGetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        Type index3Type = typeof(TIndex3);
        TypeData propertyIndex3TypeData = propertyGetMethodParameters[2].ParameterTypeData;
        Type propertyIndex3Type = propertyIndex3TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression index3Param = Expression.Parameter(index3Type, "index3");

        Expression castedIndex1Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex1)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index1Type.FullName}' to '{propertyIndex1TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex1),
                e);
        }

        Expression castedIndex2Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex2)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index2Type.FullName}' to '{propertyIndex2TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex2),
                e);
        }

        Expression castedIndex3Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex3Param = index3Type != propertyIndex3Type
                ? Expression.Convert(index3Param, propertyIndex3Type)
                : index3Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex3)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index3Type.FullName}' to '{propertyIndex3TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex3),
                e);
        }

        Expression? instanceExpression = declaringType != targetType
            ? Expression.Convert(targetParam, declaringType)
            : targetParam;

        // Access the indexer: propertyType[index1, index2, index3]
        PropertyInfo propertyInfo = propertyData.PropertyInfo;
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
            .Lambda<IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>>(body, targetParam, index1Param, index2Param, index3Param)
            .Compile();
    }

    /// <summary>
    /// Creates a strongly-typed delegate that retrieves the value of a static 3D indexer.
    /// </summary>
    /// <remarks>The created delegate performs type checking and conversion as needed to match the
    /// indexer property signature. This method is typically used in advanced scenarios such as dynamic property
    /// access or code generation.</remarks>
    /// <typeparam name="TIndex1">The type of the first index parameter of the indexer.</typeparam>
    /// <typeparam name="TIndex2">The type of the second index parameter of the indexer.</typeparam>
    /// <typeparam name="TIndex3">The type of the third index parameter of the indexer.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
    /// <param name="propertyData">The metadata describing the static 3D indexer property for which to create a getter. Must represent a readable indexer
    /// with exactly three index parameters.</param>
    /// <returns>A delegate that takes three index values and returns the value of the specified indexer
    /// property. The first parameter will be ignored.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a getter invoker has already been generated for the specified property.</exception>
    /// <exception cref="ArgumentException">Thrown if the provided generic method arguments are not compatible with the indexer property, if the property is not an
    /// indexer or is not static, if it does not have exactly three index parameters, or if the property is write-only.</exception>
    public static IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2, TIndex3> CreateStatic3DIndexerGetter<TValue, TIndex1, TIndex2, TIndex3>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property is not a static indexer. Use the appropriate indexer getter creation method instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.CanRead,
            nameof(propertyData),
            "Cannot create a getter for a write-only property.");
        TypeData declaringTypeData = propertyData.DeclaringTypeData;
        ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
        Type targetType = typeof(object);
        Type declaringType = declaringTypeData.Type;

        ParameterList propertyGetMethodParameters = propertyData.PropertyGetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            propertyGetMethodParameters.Count,
            nameof(propertyData),
            "The provided indexer property must have exactly three index parameters to create a 3D indexer getter.");
        Type returnType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertyGetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertyGetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        Type index3Type = typeof(TIndex3);
        TypeData propertyIndex3TypeData = propertyGetMethodParameters[2].ParameterTypeData;
        Type propertyIndex3Type = propertyIndex3TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression index3Param = Expression.Parameter(index3Type, "index3");

        Expression castedIndex1Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex1)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index1Type.FullName}' to '{propertyIndex1TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex1),
                e);
        }

        Expression castedIndex2Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex2)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index2Type.FullName}' to '{propertyIndex2TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex2),
                e);
        }

        Expression castedIndex3Param;
        try
        {
            // Use provided argument type and cast to indexer parameter type if needed.
            castedIndex3Param = index3Type != propertyIndex3Type
                ? Expression.Convert(index3Param, propertyIndex3Type)
                : index3Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided argument '{nameof(TIndex3)}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{index3Type.FullName}' to '{propertyIndex3TypeData.FullyQualifiedSignature}' is not natively supported.",
                nameof(TIndex3),
                e);
        }

        Expression? instanceExpression = null;

        // Access the indexer: propertyType[index1, index2, index3]
        PropertyInfo propertyInfo = propertyData.PropertyInfo;
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
            .Lambda<IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2, TIndex3>>(body, targetParam, index1Param, index2Param, index3Param)
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
    /// cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified property on a target instance of type <typeparamref
    /// name="TTarget"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
    /// <exception cref="NotSupportedException">Thrown if attempting to create a setter for an instance property declared on a value type. Use a ref-based
    /// setter instead.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is read-only or if the provided generic method arguments are incompatible or if the property is an indexer or static.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyData"/> is <see langword="null"/> or <see cref="MemberData.DeclaringTypeData"/> property of the <paramref name="propertyData"/> is <see langword="null"/>.</exception>
    public static PropertySetter<TTarget?, TValue> CreateSetter<TTarget, TValue>(PropertyData propertyData) where TTarget : class?
    {
        ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfTrue(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            "Cannot create a non-ref-based setter for a property declared on a value type. Use a ref-based setter instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            !propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must not be an indexer or static to create a non-indexer setter.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        Type declaringType = declaringTypeData.Type;
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

        PropertyInfo property = propertyData.PropertyInfo;
        Expression propertyAccess = Expression.Property(
            Expression.Convert(targetParam, declaringType),
            property);

        Type propertyType = propertyData.PropertyTypeData.Type;
        Expression value;
        try
        {
            value = propertyType != valueType
                ? Expression.Convert(valueParam, propertyType)
                : valueParam;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"Type mismatch. Unable to convert the provided '{nameof(TValue)}' type '{valueType.ToFullyQualifiedSignatureName()}' to the property's type '{propertyData.PropertyTypeData.FullyQualifiedSignature}'.",
                nameof(TValue),
                e);
        }

        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            value); // Expression.Assign

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<PropertySetter<TTarget?, TValue>>(body, targetParam, valueParam)
            .Compile();
    }

    /// <summary>
    /// Creates a strongly-typed delegate that sets the value of a static property.
    /// </summary>
    /// <remarks>This method validates that the property is writable and that the provided type
    /// arguments are compatible with the property's declaring type and value type.</remarks>
    /// <typeparam name="TValue">The type of the value to assign to the property.</typeparam>
    /// <param name="propertyData">The metadata describing the static property for which to create a setter. Must represent a writable property and
    /// cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified static property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
    /// <exception cref="NotSupportedException">Thrown if attempting to create a setter for an instance property declared on a value type. Use a ref-based
    /// setter instead.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is read-only or if the provided generic method arguments are incompatible or if the property is an indexer or not static.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="propertyData"/> is <see langword="null"/> or <see cref="MemberData.DeclaringTypeData"/> property of the <paramref name="propertyData"/> is <see langword="null"/>.</exception>
    public static PropertySetter<object?, TValue> CreateStaticSetter<TValue>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        // Important: setting instance properties on a boxed struct would modify only a copy.
        bool isInstanceValueType = !propertyData.IsStatic && declaringTypeData.IsValueType;
        ArgumentExceptionAdvanced.ThrowIfTrue(
            isInstanceValueType,
            nameof(propertyData),
            "Cannot create an object-based setter for an instance property declared on a value type. Use a ref-based setter instead.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            !propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must not be an indexer to create a non-indexer setter.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        Type declaringType = declaringTypeData.Type;
        Type targetType = typeof(object);
        Type valueType = typeof(TValue);

        // (TTarget target, TValue value)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression valueParam = Expression.Parameter(valueType, "value");

        PropertyInfo property = propertyData.PropertyInfo;
        Expression? instanceExpression = null; // Static property has no instance
        Expression propertyAccess = Expression.Property(instanceExpression, property);

        Type propertyType = propertyData.PropertyTypeData.Type;
        Expression value;
        try
        {
            value = propertyType != valueType
                ? Expression.Convert(valueParam, propertyType)
                : valueParam;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"Type mismatch. Unable to convert the provided '{nameof(TValue)}' type '{valueType.ToFullyQualifiedSignatureName()}' to the property's type '{propertyData.PropertyTypeData.FullyQualifiedSignature}'.",
                nameof(TValue),
                e);
        }

        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            value); // Expression.Assign

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<PropertySetter<object?, TValue>>(body, targetParam, valueParam)
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
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfFalse(
            declaringTypeData.IsValueType && !propertyData.IsIndexer,
            nameof(propertyData),
            $"The provided non-indexer property must be declared on a value type to create a struct setter. For reference type properties call '{nameof(CreateSetter)}' and for indexers call '{nameof(CreateStructIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsStatic,
            nameof(propertyData),
            $"For reference type instance properties or class properties (static) call '{nameof(CreateStaticSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;

        // (ref TTarget target, TValue value) => propertyType.Property = value;
        Type refTargetType = targetType.MakeByRefType();
        ParameterExpression targetByRef = Expression.Parameter(refTargetType, "target");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        PropertyInfo property = propertyData.PropertyInfo;
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
    /// Creates a delegate that sets the value of a non-static indexer property on a reference type instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for non-static indexer properties when
    /// reflection-based property access is required.</remarks>
    /// <typeparam name="TTarget">The reference type that declares the indexer property. Must be a class.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified 3D indexer property on a reference type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is declared on a value type,or is read-only, static, or if the provided generic method arguments are incompatible or if the property is not a indexer.</exception>"
    public static IndexerPropertySetter<TTarget?, TValue, TIndex> CreateIndexerSetter<TTarget, TValue, TIndex>(PropertyData propertyData)
        where TTarget : class?
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must be an non-static indexer to create an indexer getter.");

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfTrue(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            $"Cannot create an indexer setter for an instance property declared on a value type. For struct instance properties use '{nameof(CreateStruct3DIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        // (TTarget target, TValue value, params TIndex[] indices)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");
        ParameterExpression indicesParam = Expression.Parameter(typeof(TIndex[]), "indices");

        // Validate indices length
        Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: false);

        Type indexType = typeof(TIndex);
        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;

        var indexExpressions = new Expression[propertySetMethodParameters.Count - 1];
        for (int i = 0; i < propertySetMethodParameters.Count - 1; i++)
        {
            // indices[i]
            BinaryExpression indexAccess = Expression.ArrayIndex(
                indicesParam,
                Expression.Constant(i));

            TypeData propertyIndexTypeData = propertySetMethodParameters[i].ParameterTypeData;
            Type propertyIndexType = propertyIndexTypeData.Type;

            // (TIndexType)indices[i]
            Expression castedIndexParam;
            try
            {
                // Use provided argument type  and cast to indexer parameter type if needed.
                castedIndexParam = indexType != propertyIndexType
                    ? Expression.Convert(indexAccess, propertyIndexType)
                    : indexAccess;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{indexType.FullName}' to '{propertyIndexTypeData.FullyQualifiedSignature}' is not natively supported.",
                    $"index {i}",
                    e);
            }

            indexExpressions[i] = castedIndexParam;
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetParam, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, indexExpressions);
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());
        BlockExpression guardedBody = Expression.Block(validationExpression, body);

        return Expression
            .Lambda<IndexerPropertySetter<TTarget?, TValue, TIndex>>(guardedBody, targetParam, valueParam, indicesParam)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static indexer property on a reference type instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for non-static indexer properties when
    /// reflection-based property access is required.</remarks>
    /// <typeparam name="TTarget">The reference type that declares the indexer property. Must be a class.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified 3D indexer property on a reference type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is read-only, static, or if the provided generic method arguments are incompatible or if the property is not an indexer.</exception>"
    public static IndexerPropertySetter<object?, TValue, TIndex> CreateStaticIndexerSetter<TValue, TIndex>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        // Important: setting instance properties on a boxed struct would modify only a copy.
        bool isInstanceValueType = !propertyData.IsStatic && declaringTypeData.IsValueType;
        ArgumentExceptionAdvanced.ThrowIfTrue(
            isInstanceValueType,
            nameof(propertyData),
            "Cannot create an object-based setter for an instance property declared on a value type. Use a ref-based setter instead.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must be a static indexer to create an indexer getter.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        Type declaringType = declaringTypeData.Type;
        Type targetType = typeof(object);
        Type valueType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        // (TTarget target, TValue value, params TIndex[] indices)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");
        ParameterExpression indicesParam = Expression.Parameter(typeof(TIndex[]), "indices");

        // Validate indices length
        Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: false);

        Type indexType = typeof(TIndex);
        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;

        var indexExpressions = new Expression[propertySetMethodParameters.Count - 1];
        for (int i = 0; i < propertySetMethodParameters.Count - 1; i++)
        {
            // indices[i]
            BinaryExpression indexAccess = Expression.ArrayIndex(
                indicesParam,
                Expression.Constant(i));

            TypeData propertyIndexTypeData = propertySetMethodParameters[i].ParameterTypeData;
            Type propertyIndexType = propertyIndexTypeData.Type;

            // (TIndexType)indices[i]
            Expression castedIndexParam;
            try
            {
                // Use provided argument type  and cast to indexer parameter type if needed.
                castedIndexParam = indexType != propertyIndexType
                    ? Expression.Convert(indexAccess, propertyIndexType)
                    : indexAccess;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{indexType.FullName}' to '{propertyIndexTypeData.FullyQualifiedSignature}' is not natively supported.",
                    $"index {i}",
                    e);
            }

            indexExpressions[i] = castedIndexParam;
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetParam, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, indexExpressions);
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());
        BlockExpression guardedBody = Expression.Block(validationExpression, body);

        return Expression
            .Lambda<IndexerPropertySetter<object?, TValue, TIndex>>(guardedBody, targetParam, valueParam, indicesParam)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static indexer property on a struct instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for struct indexer properties when
    /// reflection-based property access is required. The returned delegate expects the value type/struct to be passed
    /// by reference, along with the index parameters and the type of the value to set.</remarks>
    /// <typeparam name="TTarget">The value type that declares the indexer property. Must be a struct.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the returnType of the specified indexer property on a value type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    public static ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex> CreateStructIndexerSetter<TTarget, TValue, TIndex>(PropertyData propertyData)
        where TTarget : struct
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfFalse(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            $"The provided property must be declared on a value type to create a struct indexer setter. For reference type properties call '{nameof(Create3DIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer & !propertyData.IsStatic,
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

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        // (TTarget target, TValue value, TIndex1 index1)
        ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "propertyType");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");
        ParameterExpression indicesParam = Expression.Parameter(typeof(TIndex[]), "indices");

        // Validate indices length
        Expression validationExpression = CreateIndexParameterArrayLengthMismatchExceptionExpression(propertyData, indicesParam, isGetter: false);

        Type indexType = typeof(TIndex);
        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;

        var indexExpressions = new Expression[propertySetMethodParameters.Count - 1];
        for (int i = 0; i < propertySetMethodParameters.Count - 1; i++)
        {
            // indices[i]
            BinaryExpression indexAccess = Expression.ArrayIndex(
                indicesParam,
                Expression.Constant(i));

            TypeData propertyIndexTypeData = propertySetMethodParameters[i].ParameterTypeData;
            Type propertyIndexType = propertyIndexTypeData.Type;

            // (TIndexType)indices[i]
            Expression castedIndexParam;
            try
            {
                // Use provided argument type  and cast to indexer parameter type if needed.
                castedIndexParam = indexType != propertyIndexType
                    ? Expression.Convert(indexAccess, propertyIndexType)
                    : indexAccess;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided indexer argument at position '{i}' is incompatible with the property's index parameter type. Reason: A conversion from the provided '{indexType.FullName}' to '{propertyIndexTypeData.FullyQualifiedSignature}' is not natively supported.",
                    $"index {i}",
                    e);
            }

            indexExpressions[i] = castedIndexParam;
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetByRef, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, indexExpressions);
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());
        BlockExpression guardedBody = Expression.Block(validationExpression, body);

        return Expression
            .Lambda<ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex>>(guardedBody, targetByRef, valueParam, indicesParam)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static 2D indexer property on a reference type instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for non-static 2D indexer properties when
    /// reflection-based property access is required.</remarks>
    /// <typeparam name="TTarget">The reference type that declares the indexer property. Must be a class.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified 3D indexer property on a reference type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is declared on a value type,or is read-only, static, or if the provided generic method arguments are incompatible or if the property is not a 2D indexer.</exception>"
    public static IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2> Create2DIndexerSetter<TTarget, TValue, TIndex1, TIndex2>(PropertyData propertyData)
        where TTarget : class?
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must be an non-static indexer to create an indexer getter.");

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfTrue(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            $"Cannot create an indexer setter for an instance property declared on a value type. For struct instance properties use '{nameof(CreateStruct3DIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            propertySetMethodParameters.Count - 1,
            nameof(propertyData),
            "The provided indexer property must have exactly two index parameters to create a 2D indexer setter.");

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertySetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertySetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        Expression convertedIndex1Param;
        try
        {
            // TIndex1
            convertedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{1}' is incompatible with the property's index parameter type. Reason: A conversion to '{index1Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{1}",
                e);
        }

        Expression convertedIndex2Param;
        try
        {
            // TIndex2
            convertedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{2}' is incompatible with the property's index parameter type. Reason: A conversion to '{index2Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{2}",
                e);
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetParam, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, new[] { convertedIndex1Param, convertedIndex2Param });
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2>>(body, targetParam, valueParam, index1Param, index2Param)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static 2D indexer property on a reference type instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for non-static 2D indexer properties when
    /// reflection-based property access is required.</remarks>
    /// <typeparam name="TTarget">The reference type that declares the indexer property. Must be a class.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified 3D indexer property on a reference type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is read-only, static, or if the provided generic method arguments are incompatible or if the property is not a 3D indexer.</exception>"
    public static IndexerPropertySetter<object?, TValue, TIndex1, TIndex2> CreateStatic2DIndexerSetter<TValue, TIndex1, TIndex2>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        // Important: setting instance properties on a boxed struct would modify only a copy.
        bool isInstanceValueType = !propertyData.IsStatic && declaringTypeData.IsValueType;
        ArgumentExceptionAdvanced.ThrowIfTrue(
            isInstanceValueType,
            nameof(propertyData),
            "Cannot create an object-based setter for an instance property declared on a value type. Use a ref-based setter instead.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must be a static indexer to create an indexer getter.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            propertySetMethodParameters.Count - 1,
            nameof(propertyData),
            "The provided indexer property must have exactly two index parameters to create a 2D indexer setter.");

        Type declaringType = declaringTypeData.Type;
        Type targetType = typeof(object);
        Type valueType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertySetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertySetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        Expression convertedIndex1Param;
        try
        {
            // TIndex1
            convertedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{1}' is incompatible with the property's index parameter type. Reason: A conversion to '{index1Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{1}",
                e);
        }

        Expression convertedIndex2Param;
        try
        {
            // TIndex2
            convertedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{2}' is incompatible with the property's index parameter type. Reason: A conversion to '{index2Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{2}",
                e);
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = null; // Static property has no instance
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, new[] { convertedIndex1Param, convertedIndex2Param });
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<IndexerPropertySetter<object?, TValue, TIndex1, TIndex2>>(body, targetParam, valueParam, index1Param, index2Param)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static 2D indexer property on a struct instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for struct indexer properties when
    /// reflection-based property access is required. The returned delegate expects the value type/struct to be passed
    /// by reference, along with the index parameters and the type of the value to set.</remarks>
    /// <typeparam name="TTarget">The value type that declares the indexer property. Must be a struct.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the returnType of the specified indexer property on a value type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    public static ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2> CreateStruct2DIndexerSetter<TTarget, TValue, TIndex1, TIndex2>(PropertyData propertyData)
        where TTarget : struct
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfFalse(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            $"The provided property must be declared on a value type to create a struct indexer setter. For reference type properties call '{nameof(Create3DIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer & !propertyData.IsStatic,
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

        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            propertySetMethodParameters.Count - 1,
            nameof(propertyData),
            "The provided indexer property must have exactly two index parameters to create a 2D indexer getter.");

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertySetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertySetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2)
        ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "propertyType");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        Expression convertedIndex1Param;
        try
        {
            // TIndex1
            convertedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{1}' is incompatible with the property's index parameter type. Reason: A conversion to '{index1Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{1}",
                e);
        }

        Expression convertedIndex2Param;
        try
        {
            // TIndex2
            convertedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{2}' is incompatible with the property's index parameter type. Reason: A conversion to '{index2Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{2}",
                e);
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetByRef, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, new[] { convertedIndex1Param, convertedIndex2Param });
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>>(body, targetByRef, valueParam, index1Param, index2Param)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static 3D indexer property on a reference type instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for non-static 3D indexer properties when
    /// reflection-based property access is required.</remarks>
    /// <typeparam name="TTarget">The reference type that declares the indexer property. Must be a class.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified 3D indexer property on a reference type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is declared on a value type,or is read-only, static, or if the provided generic method arguments are incompatible or if the property is not a 3D indexer.</exception>"
    public static IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3> Create3DIndexerSetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(PropertyData propertyData)
        where TTarget : class?
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && !propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must be an non-static indexer to create an indexer getter.");

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfTrue(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            $"Cannot create an indexer setter for an instance property declared on a value type. For struct instance properties use '{nameof(CreateStruct3DIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            propertySetMethodParameters.Count - 1,
            nameof(propertyData),
            "The provided indexer property must have exactly three index parameters to create a 3D indexer getter.");

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertySetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertySetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        Type index3Type = typeof(TIndex3);
        TypeData propertyIndex3TypeData = propertySetMethodParameters[2].ParameterTypeData;
        Type propertyIndex3Type = propertyIndex3TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression index3Param = Expression.Parameter(index3Type, "index3");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        Expression convertedIndex1Param;
        try
        {
            // TIndex1
            convertedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{1}' is incompatible with the property's index parameter type. Reason: A conversion to '{index1Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{1}",
                e);
        }

        Expression convertedIndex2Param;
        try
        {
            // TIndex2
            convertedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{2}' is incompatible with the property's index parameter type. Reason: A conversion to '{index2Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{2}",
                e);
        }

        Expression convertedIndex3Param;
        try
        {
            // TIndex3
            convertedIndex3Param = index3Type != propertyIndex3Type
                ? Expression.Convert(index3Param, propertyIndex3Type)
                : index3Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{3}' is incompatible with the property's index parameter type. Reason: A conversion to '{index3Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{3}",
                e);
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetParam, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, new[] { convertedIndex1Param, convertedIndex2Param, convertedIndex3Param });
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3>>(body, targetParam, valueParam, index1Param, index2Param, index3Param)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static 3D indexer property on a reference type instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for non-static 3D indexer properties when
    /// reflection-based property access is required.</remarks>
    /// <typeparam name="TTarget">The reference type that declares the indexer property. Must be a class.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the value of the specified 3D indexer property on a reference type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    /// <exception cref="ArgumentException">Thrown if the property is read-only, static, or if the provided generic method arguments are incompatible or if the property is not a 3D indexer.</exception>"
    public static IndexerPropertySetter<object?, TValue, TIndex1, TIndex2, TIndex3> CreateStatic3DIndexerSetter<TValue, TIndex1, TIndex2, TIndex3>(PropertyData propertyData)
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        // Important: setting instance properties on a boxed struct would modify only a copy.
        bool isInstanceValueType = !propertyData.IsStatic && declaringTypeData.IsValueType;
        ArgumentExceptionAdvanced.ThrowIfTrue(
            isInstanceValueType,
            nameof(propertyData),
            "Cannot create an object-based setter for an instance property declared on a value type. Use a ref-based setter instead.");

        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer && propertyData.IsStatic,
            nameof(propertyData),
            "The provided property must be a static indexer to create an indexer getter.");
        ArgumentExceptionAdvanced.ThrowIfTrue(
            propertyData.IsReadOnly,
            nameof(propertyData),
            "Cannot create a setter for an read-only property.");

        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            propertySetMethodParameters.Count - 1,
            nameof(propertyData),
            "The provided indexer property must have exactly three index parameters to create a 3D indexer getter.");

        Type declaringType = declaringTypeData.Type;
        Type targetType = typeof(object);
        Type valueType = typeof(TValue);
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertySetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertySetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        Type index3Type = typeof(TIndex3);
        TypeData propertyIndex3TypeData = propertySetMethodParameters[2].ParameterTypeData;
        Type propertyIndex3Type = propertyIndex3TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3)
        ParameterExpression targetParam = Expression.Parameter(targetType, "target");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression index3Param = Expression.Parameter(index3Type, "index3");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        Expression convertedIndex1Param;
        try
        {
            // TIndex1
            convertedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{1}' is incompatible with the property's index parameter type. Reason: A conversion to '{index1Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{1}",
                e);
        }

        Expression convertedIndex2Param;
        try
        {
            // TIndex2
            convertedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{2}' is incompatible with the property's index parameter type. Reason: A conversion to '{index2Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{2}",
                e);
        }

        Expression convertedIndex3Param;
        try
        {
            // TIndex3
            convertedIndex3Param = index3Type != propertyIndex3Type
                ? Expression.Convert(index3Param, propertyIndex3Type)
                : index3Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{3}' is incompatible with the property's index parameter type. Reason: A conversion to '{index3Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{3}",
                e);
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = null; // Static property has no instance
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, new[] { convertedIndex1Param, convertedIndex2Param, convertedIndex3Param });
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<IndexerPropertySetter<object?, TValue, TIndex1, TIndex2, TIndex3>>(body, targetParam, valueParam, index1Param, index2Param, index3Param)
            .Compile();
    }

    /// <summary>
    /// Creates a delegate that sets the value of a non-static 3D indexer property on a struct instance.
    /// </summary>
    /// <remarks>Use this method to generate a performant setter for struct indexer properties when
    /// reflection-based property access is required. The returned delegate expects the value type/struct to be passed
    /// by reference, along with the index parameters and the type of the value to set.</remarks>
    /// <typeparam name="TTarget">The value type that declares the indexer property. Must be a struct.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
    /// non-read-only indexer property and cannot be <see langword="null"/>.</param>
    /// <returns>A delegate that sets the returnType of the specified indexer property on a value type instance using the provided
    /// indices and value type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
    public static ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> CreateStruct3DIndexerSetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(PropertyData propertyData)
        where TTarget : struct
    {
        ArgumentNullException.ThrowIfNull(propertyData);

        TypeData declaringTypeData = propertyData.DeclaringTypeData;

        ArgumentExceptionAdvanced.ThrowIfFalse(
            declaringTypeData.IsValueType,
            nameof(propertyData),
            $"The provided property must be declared on a value type to create a struct indexer setter. For reference type properties call '{nameof(Create3DIndexerSetter)}' instead.");
        ArgumentExceptionAdvanced.ThrowIfFalse(
            propertyData.IsIndexer & !propertyData.IsStatic,
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

        ParameterList propertySetMethodParameters = propertyData.PropertySetMethodParameters;
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            propertySetMethodParameters.Count - 1,
            nameof(propertyData),
            "The provided indexer property must have exactly three index parameters to create a 3D indexer getter.");

        Type declaringType = declaringTypeData.Type;
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
        Type propertyType = propertyData.PropertyTypeData.Type;
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            valueType,
            propertyType,
            ExceptionMessages.GetTypeMismatchExceptionMessage(
                    valueType,
                    nameof(TValue),
                    propertyType,
                    "property propertyType"));

        Type index1Type = typeof(TIndex1);
        TypeData propertyIndex1TypeData = propertySetMethodParameters[0].ParameterTypeData;
        Type propertyIndex1Type = propertyIndex1TypeData.Type;

        Type index2Type = typeof(TIndex2);
        TypeData propertyIndex2TypeData = propertySetMethodParameters[1].ParameterTypeData;
        Type propertyIndex2Type = propertyIndex2TypeData.Type;

        Type index3Type = typeof(TIndex3);
        TypeData propertyIndex3TypeData = propertySetMethodParameters[2].ParameterTypeData;
        Type propertyIndex3Type = propertyIndex3TypeData.Type;

        // (TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3)
        ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "propertyType");
        ParameterExpression index1Param = Expression.Parameter(index1Type, "index1");
        ParameterExpression index2Param = Expression.Parameter(index2Type, "index2");
        ParameterExpression index3Param = Expression.Parameter(index3Type, "index3");
        ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

        Expression convertedIndex1Param;
        try
        {
            // TIndex1
            convertedIndex1Param = index1Type != propertyIndex1Type
                ? Expression.Convert(index1Param, propertyIndex1Type)
                : index1Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{1}' is incompatible with the property's index parameter type. Reason: A conversion to '{index1Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{1}",
                e);
        }

        Expression convertedIndex2Param;
        try
        {
            // TIndex2
            convertedIndex2Param = index2Type != propertyIndex2Type
                ? Expression.Convert(index2Param, propertyIndex2Type)
                : index2Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{2}' is incompatible with the property's index parameter type. Reason: A conversion to '{index2Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{2}",
                e);
        }

        Expression convertedIndex3Param;
        try
        {
            // TIndex3
            convertedIndex3Param = index3Type != propertyIndex3Type
                ? Expression.Convert(index3Param, propertyIndex3Type)
                : index3Param;
        }
        catch (InvalidOperationException e)
        {
            throw new ArgumentException(
                $"The provided indexer argument at position '{3}' is incompatible with the property's index parameter type. Reason: A conversion to '{index3Type.ToTypeDataView().FullyQualifiedSignature}' is not natively supported.",
                $"TIndex{3}",
                e);
        }

        PropertyInfo property = propertyData.PropertyInfo;

        Expression? instanceExpression = Expression.Convert(targetByRef, declaringType);
        IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, property, new[] { convertedIndex1Param, convertedIndex2Param, convertedIndex3Param });
        BinaryExpression assign = Expression.Assign(
            propertyAccess,
            Expression.Convert(valueParam, propertyData.PropertyTypeData.Type)); // Expression.Assign 

        // Action<...> requires a void body -> wrap assignment in a void block.
        BlockExpression body = Expression.Block(assign, Expression.Empty());

        return Expression
            .Lambda<ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>>(body, targetByRef, valueParam, index1Param, index2Param, index3Param)
            .Compile();
    }

    private static Expression CreateIndexParameterArrayLengthMismatchExceptionExpression(PropertyData propertyData, ParameterExpression indicesParam, bool isGetter)
    {
        ParameterList indexParameters = isGetter
            ? propertyData.PropertyGetMethodParameters
            : propertyData.PropertySetMethodParameters;
        int indexParameterCount = isGetter
            ? indexParameters.Count
            : indexParameters.Count - 1;
        Expression lengthMismatch = Expression.NotEqual(
                        Expression.ArrayLength(indicesParam),
                        Expression.Constant(indexParameterCount));
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
            Expression.Constant(indexParameterCount.ToString(System.Globalization.CultureInfo.InvariantCulture)),
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
        Type declaringType = propertyData.DeclaringTypeData.Type;
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

        TypeData helperExtensionsCommonTypeData = SymbolReflectionInfoCache.GetOrCreateEntryInternal(typeof(ReflectionHelperExtensions));
        const string extensionMethodName = nameof(ReflectionHelperExtensions.ToFullyQualifiedSignatureName);
        var typeCacheKey = SymbolReflectionInfoCacheKeyInternal.CreateForType(typeof(Type));
        MethodData toFullyQualifiedSignatureNameExtensionMethodData = helperExtensionsCommonTypeData.Methods[extensionMethodName, new MethodParameterInfo(typeCacheKey)];

        // BUG::Call GetType on target and pass to extension method
        MethodCallExpression extensionMethodCall = Expression.Call(toFullyQualifiedSignatureNameExtensionMethodData.MethodInfo, target);

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
    //            Expression.NotEqual(value, Expression.Constant(null, typeof(object))), Expression.TypeIs(value, propertyData.DeclaringTypeData.Type));

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
