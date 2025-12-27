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

    /// <summary>
    /// Represents a method that sets the value of a field on a value declaringType instance.
    /// </summary>
    /// <remarks>This delegate is typically used to update fields on value declaringType instances, such as structs,
    /// where direct assignment is required. The target parameter is passed by reference to allow modification of the
    /// original instance.</remarks>
    /// <typeparam name="TTarget">The value declaringType whose field will be set.</typeparam>
    /// <typeparam name="TValue">The declaringType of the value to assign to the field.</typeparam>
    /// <param name="target">A reference to the value declaringType instance whose field will be set.</param>
    /// <param name="value">The value to assign to the field. May be null if the field declaringType allows null values.</param>
    public delegate void ValueTypeMemberSetter<TTarget, TValue>(ref TTarget target, TValue? value) where TTarget : struct;

    internal static class DelegateProvider
    {
        private static readonly ConcurrentDictionary<InvokerKeyMapKey, SymbolInfoDataCacheKey> InvocatorKeyMap = new ConcurrentDictionary<InvokerKeyMapKey, SymbolInfoDataCacheKey>();

        public static MethodData GetOrCreateFastMethodInvocator(MethodData targetMethodData, TypeData[] genericMethodParameters)
        {
            // If the method is not a generic method definition or an open generic method and already has an invocator, return it directly.
            if (!targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition && ((IMethodDataInvoker)targetMethodData).IsInvocable)
            {
                return targetMethodData;
            }

            // Get or construct the (closed) generic method data if required.
            MethodData methodData = targetMethodData.IsOpenGenericMethodOrGenericMethodDefinition
                ? DelegateProvider.GetOrConstructGenericMethod(targetMethodData, genericMethodParameters)
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

            methodInvoker.SetInvoker(invocator);
            methodInvoker.SetInvoker(awaitableTaskInvocator);
            methodInvoker.SetInvoker(awaitableGenericTaskInvocator);
            methodInvoker.SetInvoker(awaitableGenericValueTaskInvocator);
            methodInvoker.SetInvoker(awaitableValueTaskInvocator);

            return methodData;
        }

        private static MethodData GetOrConstructGenericMethod(MethodData targetMethodData, TypeData[] genericMethodParameters)
        {
            MethodData methodData;

            // Try get cached constructed invocator for the specified generic method parameters.
            var invocatorKeyMapKey = InvokerKeyMapKey.Create(genericMethodParameters);
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

        public static Func<object?, object?> CreateGetter(FieldData fieldData)
        {
            ArgumentNullException.ThrowIfNull(fieldData, nameof(fieldData));
            ArgumentNullException.ThrowIfNull(fieldData.DeclaringTypeData, nameof(fieldData));

            FieldInfo field = fieldData.GetFieldInfo();

            // (object? target) => (object?)((TDeclaring)target).Field
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");

            Expression fieldAccess =
                field.IsStatic
                    ? Expression.Field(expression: null, field) // static: no instance
                    : Expression.Field(
                        Expression.Convert(targetParam, fieldData.DeclaringTypeData.UnwrapType()), // cast/unbox
                        field);

            // Box value types
            UnaryExpression body = Expression.Convert(fieldAccess, typeof(object));

            return Expression
                .Lambda<Func<object?, object?>>(body, targetParam)
                .Compile(); // compiles to a delegate 
        }

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
            if (!fieldData.IsStatic && fieldData.DeclaringTypeData.IsStruct)
            {
                throw new NotSupportedException(
                    "Cannot create an object-based setter for an instance field declared on a value type. " +
                    $"You need a ref-based setter: call {nameof(CreateStructSetter)} instead.");
            }

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");

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

        public static ValueTypeMemberSetter<TTarget, TValue> CreateStructSetter<TTarget, TValue>(FieldData fieldData)
            where TTarget : struct
        {
            ArgumentNullException.ThrowIfNull(fieldData, nameof(fieldData));
            ArgumentNullException.ThrowIfNull(fieldData.DeclaringTypeData, nameof(fieldData));

            Type declaringType = fieldData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionEx.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        "target type",
                        declaringType,
                        "declaring type"));

            Type valueType = typeof(TValue);
            Type fieldType = fieldData.FieldTypeData.UnwrapType();
            ArgumentExceptionEx.ThrowIfNotAssignableTo(
                valueType,
                fieldType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        valueType,
                        "value type",
                        fieldType,
                        "field type"));

            if (fieldData.IsConst)
            {
                throw new InvalidOperationException("Cannot create a setter for a 'const' field.");
            }

            if (fieldData.IsReadonly)
            {
                throw new InvalidOperationException("Cannot create a setter for a 'readonly' field.");
            }

            // (ref TTarget target, TValue value) => target.Field = value;
            ParameterExpression targetByRef = Expression.Parameter(targetType.MakeByRefType(), "target");
            ParameterExpression valueParam = Expression.Parameter(valueType, "value");

            FieldInfo field = fieldData.GetFieldInfo();
            MemberExpression fieldAccess = Expression.Field(targetByRef, field);
            BinaryExpression assign = Expression.Assign(fieldAccess, Expression.Convert(valueParam, fieldType));
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypeMemberSetter<TTarget, TValue>>(body, targetByRef, valueParam)
                .Compile();
        }

        public static Func<object?, object?> CreateGetter(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");

            PropertyInfo property = propertyData.GetPropertyInfo();
            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Expression propertyAccess =
                propertyData.IsStatic
                    ? Expression.Property(expression: null, property) // static: no instance
                    : Expression.Property(
                        Expression.Convert(targetParam, declaringType), // cast/unbox
                        property);

            // Box value types
            UnaryExpression body = Expression.Convert(propertyAccess, typeof(object));

            return Expression
                .Lambda<Func<object?, object?>>(body, targetParam)
                .Compile(); // compiles to a delegate 
        }

        public static Action<object?, object?> CreateSetter(PropertyData propertyData)
        {
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            // Reject readonly up front
            if (propertyData.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot create a setter for an read-only property.");
            }

            // Important: setting instance properties on a boxed struct would modify only a copy.
            if (!propertyData.IsStatic && propertyData.DeclaringTypeData.IsStruct)
            {
                throw new NotSupportedException(
                    "Cannot create an object-based setter for an instance property declared on a value type. " +
                    $"You need a ref-based setter: call {nameof(CreateStructSetter)} instead.");
            }

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");

            PropertyInfo property = propertyData.GetPropertyInfo();
            Expression propertyAccess =
                propertyData.IsStatic
                    ? Expression.Property(expression: null, property)
                    : Expression.Property(
                        Expression.Convert(targetParam, propertyData.DeclaringTypeData.UnwrapType()),
                        property);

            BinaryExpression assign = Expression.Assign(
                propertyAccess,
                Expression.Convert(valueParam, propertyData.PropertyTypeData.UnwrapType())); // Expression.Assign 

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<Action<object?, object?>>(body, targetParam, valueParam)
                .Compile();
        }

        public static ValueTypeMemberSetter<TTarget, TValue> CreateStructSetter<TTarget, TValue>(PropertyData propertyData)
            where TTarget : struct
        {
            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();
            Type targetType = typeof(TTarget);
            ArgumentExceptionEx.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        targetType,
                        "target type",
                        declaringType,
                        "declaring type"));

            Type valueType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            ArgumentExceptionEx.ThrowIfNotAssignableTo(
                valueType,
                propertyType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        valueType,
                        "value type",
                        propertyType,
                        "property type"));

            if (propertyData.IsReadOnly)
            {
                throw new InvalidOperationException("Cannot create a setter for an read-only property.");
            }

            // (ref TTarget target, TValue value) => target.Property = value;
            ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "target");
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

            PropertyInfo property = propertyData.GetPropertyInfo();
            MemberExpression propertyAccess = Expression.Property(targetByRef, property);
            BinaryExpression assign = Expression.Assign(propertyAccess, Expression.Convert(valueParam, propertyType));
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypeMemberSetter<TTarget, TValue>>(body, targetByRef, valueParam)
                .Compile();
        }

        #region InvokerKeyMapKey

        private readonly struct InvokerKeyMapKey : IEquatable<InvokerKeyMapKey>
        {
            public ImmutableList<RuntimeTypeHandle> MethodParameterTypeHandles { get; }
            private readonly int? _hashCode;

            public InvokerKeyMapKey(ImmutableList<RuntimeTypeHandle> methodParameterTypeHandles) : this()
            {
                ArgumentNullExceptionEx.ThrowIfNullOrEmpty(methodParameterTypeHandles, nameof(methodParameterTypeHandles), "Method parameter declaringType handles must be provided to create an invocator map key.");

                this.MethodParameterTypeHandles = methodParameterTypeHandles;
                this._hashCode = ComputeHashCode();
            }

            public static InvokerKeyMapKey Create(IEnumerable<RuntimeTypeHandle> methodParameterTypeHandles)
                => new InvokerKeyMapKey(methodParameterTypeHandles.ToImmutableList());

            public static InvokerKeyMapKey Create(IEnumerable<TypeData> methodParameterTypeDatas)
                => new InvokerKeyMapKey(methodParameterTypeDatas.Select(typeData => typeData.Handle).ToImmutableList());

            public bool Equals(InvokerKeyMapKey other)
                => this.MethodParameterTypeHandles.SequenceEqual(other.MethodParameterTypeHandles);

            public override bool Equals([NotNullWhen(true)] object obj)
                => obj is InvokerKeyMapKey invocatorKey && base.Equals(invocatorKey);

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

            public static bool operator ==(InvokerKeyMapKey left, InvokerKeyMapKey right) => left.Equals(right);
            public static bool operator !=(InvokerKeyMapKey left, InvokerKeyMapKey right) => !(left == right);
        }

        #endregion InvokerKeyMapKey
    }
}
