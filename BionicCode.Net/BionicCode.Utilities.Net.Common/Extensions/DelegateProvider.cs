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
    /// Represents a method that sets the returnType of a field on a returnType declaringType instance.
    /// </summary>
    /// <remarks>This delegate is typically used to update fields on returnType declaringType instances, such as structs,
    /// where direct assignment is required. The propertyType parameter is passed by reference to allow modification of the
    /// original instance.</remarks>
    /// <typeparam name="TTarget">The returnType declaringType whose field will be set.</typeparam>
    /// <typeparam name="TValue">The declaringType of the returnType to assign to the field.</typeparam>
    /// <param name="target">A reference to the returnType declaringType instance whose field will be set.</param>
    /// <param name="value">The returnType to assign to the field. May be null if the field declaringType allows null values.</param>
    public delegate void ValueTypeMemberSetter<TTarget, TValue>(ref TTarget target, TValue? value) where TTarget : struct;

    /// <summary>
    /// Represents a method that sets the returnType of an indexer property on a returnType type instance using the specified
    /// indices.
    /// </summary>
    /// <remarks>This delegate is typically used to abstract the process of setting indexer properties on
    /// returnType types, such as structs, where direct assignment is required. The propertyType parameter is passed by reference
    /// to allow modification of the underlying returnType type instance.</remarks>
    /// <typeparam name="TTarget">The returnType type that contains the indexer property to be set.</typeparam>
    /// <typeparam name="TValue">The type of the returnType to assign to the indexer property.</typeparam>
    /// <param name="target">A reference to the returnType type instance whose indexer property will be set.</param>
    /// <param name="indices">An array of objects representing the indices used to access the indexer property. Can be null if the indexer
    /// does not require indices.</param>
    /// <param name="value">The returnType to assign to the indexer property. Can be null for reference types or nullable returnType types.</param>
    public delegate void ValueTypeIndexerPropertySetter<TTarget, TValue>(ref TTarget target, object?[]? indices, TValue? value) where TTarget : struct;

    public delegate TValue IndexerPropertyGetter<TTarget, TIndex, TValue>(TTarget target, TIndex index);
    public delegate TValue IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TValue>(TTarget target, TIndex1 index1, TIndex2 index2);
    public delegate TValue IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3);

    internal static class DelegateProvider
    {
        private static readonly ConcurrentDictionary<InvokerKeyMapKey, SymbolInfoDataCacheKey> InvocatorKeyMap = new ConcurrentDictionary<InvokerKeyMapKey, SymbolInfoDataCacheKey>();

        /// <summary>
        /// Gets an invocable MethodData instance for the specified method, generating a fast delegate-based invoker if
        /// necessary. Supports both generic and non-generic methods.
        /// </summary>
        /// <remarks>If the specified method already has an invoker, it is returned as-is. For generic
        /// method definitions or open generic methods, the method is first constructed with the provided generic type
        /// arguments before generating the invoker. The returned MethodData can be used for efficient runtime
        /// invocation without reflection overhead.</remarks>
        /// <param name="targetMethodData">The MethodData representing the propertyType method. This can be a generic method definition, an open generic
        /// method, or a closed method.</param>
        /// <param name="genericMethodParameters">An array of TypeData objects specifying the generic type arguments to use if the propertyType method is a generic
        /// method definition or open generic method. This parameter is ignored for non-generic methods.</param>
        /// <returns>A MethodData instance that is guaranteed to have an invoker delegate attached, suitable for fast invocation.
        /// If the method is generic, the returned MethodData corresponds to the constructed closed generic method.</returns>
        public static MethodData GetOrCreateFastMethodInvoker(MethodData targetMethodData, TypeData[] genericMethodParameters)
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

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "propertyType");
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

        /// <summary>
        /// Retrieves an existing fast event invoker for the specified event or creates one if it does not already
        /// exist.
        /// </summary>
        /// <param name="eventData">The metadata describing the event for which to obtain or create a fast invoker. Cannot be null.</param>
        /// <returns>A MethodData instance representing a fast invoker for the specified event.</returns>
        public static MethodData GetOrCreateFastEventInvoker(EventData eventData)
        {
            MethodData targetMethodData = eventData.EventInvokerMethodData;

            return GetOrCreateFastMethodInvoker(targetMethodData!, Array.Empty<TypeData>());
        }

        /// <summary>
        /// Creates a delegate that retrieves the returnType of the specified field from a given object instance.
        /// </summary>
        /// <remarks>The returned delegate expects the propertyType object to be of the field's declaring type
        /// or compatible with it. For returnType type fields, the result is boxed. Passing a propertyType of an incompatible type
        /// may result in a runtime exception.</remarks>
        /// <param name="fieldData">The metadata describing the field for which to create a getter delegate. Must not be null and must have a
        /// non-null declaring type.</param>
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
        /// Creates a delegate that sets the returnType of the specified field on a given object instance or type.
        /// </summary>
        /// <remarks>The returned delegate uses object-based parameters. For instance fields declared on
        /// reference types, the propertyType parameter must be an instance of the declaring type. For static fields, the
        /// propertyType parameter is ignored. This method does not support creating setters for instance fields on returnType
        /// types (structs); use a ref-based setter in such cases.</remarks>
        /// <param name="fieldData">The metadata describing the field for which to create a setter. Must not represent a const or readonly
        /// field.</param>
        /// <returns>An <see cref="Action{Object, Object}"/> delegate that sets the returnType of the specified field. For static
        /// fields, the propertyType parameter is ignored.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="fieldData"/> represents a const or readonly field.</exception>
        /// <exception cref="NotSupportedException">Thrown if <paramref name="fieldData"/> represents an instance field declared on a returnType type. Use a
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
                    "Cannot create an object-based setter for an instance field declared on a value type. " +
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
        /// This method validates type compatibility and field mutability before creating the setter.</remarks>
        /// <typeparam name="TTarget">The type of the struct that contains the field to set.</typeparam>
        /// <typeparam name="TValue">The type of the returnType to assign to the field.</typeparam>
        /// <param name="fieldData">Metadata describing the field to be set, including its declaring type and field type information. Cannot be
        /// null.</param>
        /// <returns>A delegate that sets the specified field on a struct of type TTarget to a returnType of type TValue.</returns>
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
                        "declaring type"));

            Type valueType = typeof(TValue);
            Type fieldType = fieldData.FieldTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                valueType,
                fieldType,
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                        valueType,
                        nameof(TValue),
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
        /// Creates a delegate that retrieves the returnType of the specified readable property.
        /// </summary>
        /// <remarks>The returned delegate expects the propertyType object to be of the declaring type of the
        /// property. For returnType type properties, the result is boxed as an object. Attempting to use the delegate with
        /// an incompatible propertyType type will result in a runtime exception.</remarks>
        /// <param name="propertyData">The metadata describing the property for which to create a getter delegate. Must represent a readable
        /// property and cannot be null.</param>
        /// <returns>A delegate that takes an object instance and returns the returnType of the specified property. For static
        /// properties, the instance parameter is ignored.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a getter delegate has already been generated for the specified property.</exception>
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
        /// Creates a strongly typed getter delegate for a single-parameter indexer property on the specified target
        /// type.
        /// </summary>
        /// <remarks>The created delegate provides efficient, strongly typed access to the indexer
        /// property. The property described by propertyData must be an indexer with exactly one index parameter, and
        /// the types specified by TTarget, TIndex, and TValue must be compatible with the declaring type, index
        /// parameter type, and property type, respectively.</remarks>
        /// <typeparam name="TTarget">The type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TIndex">The type of the index parameter accepted by the indexer.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create the getter. Must represent a readable
        /// indexer with exactly one index parameter.</param>
        /// <returns>A delegate that gets the value of the specified indexer property for a given target object and index.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the specified property is not a single-parameter indexer, or if the types specified by TTarget, TIndex, or TValue are not compatible with the declaring type, index parameter type, or property type.</exception>
        public static IndexerPropertyGetter<TTarget, TIndex, TValue> CreateIndexerGetter<TTarget, TIndex, TValue>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

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
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                1,
                propertyData.IndexerParameters.Count,
                nameof(propertyData),
                "The provided indexer property must have exactly a single index parameter to create a 1D indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            if (!propertyData.PropertyTypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                propertyType,
                returnType,
                nameof(TValue),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    propertyType,
                    "property type",
                    returnType,
                    nameof(TValue)));
            }

            Type indexType = typeof(TIndex);
            TypeData indexerParameterTypeData = propertyData.IndexerParameters[0].ParameterTypeData;
            Type indexParameterType = indexerParameterTypeData.UnwrapType();
            if (!indexerParameterTypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        indexType,
                        indexParameterType,
                        nameof(TIndex),
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                            indexType,
                            nameof(TIndex),
                            indexParameterType,
                            "indexer parameter type"));
            }

            // (object? propertyType, object?[]? indices) => (object?)((TDeclaring)propertyType)[convertedIndices...]
            ParameterExpression targetParam = Expression.Parameter(targetType, "target");
            ParameterExpression indexParam = Expression.Parameter(indexType, "index");

            Expression castedIndexParam;
            try
            {
                // Use provided argument type and cast to indexer parameter type if needed.
                castedIndexParam = indexType != indexParameterType
                    ? Expression.Convert(indexParam, indexParameterType)
                    : indexParam;
            }
            catch (InvalidOperationException e)
            {
                throw new ArgumentException(
                    $"The provided argument {nameof(TIndex)} is incompatible with the property's index parameter type. Cast from the provided {indexType.FullName} to {indexerParameterTypeData.FullyQualifiedSignature} is not natively supported.",
                    nameof(TIndex),
                    e);
            }

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : targetType != declaringType
                    ? Expression.Convert(targetParam, declaringType)
                    : targetParam;

            // Access the indexer: propertyType[index]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, [castedIndexParam]);

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
                    $"The provided argument {nameof(TValue)} is incompatible with the property type. Cast from {propertyData.PropertyTypeData.FullyQualifiedSignature} to the provided {returnType.FullName} is not natively supported.",
                    nameof(TValue),
                    e);
            }

            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TIndex, TValue>>(body, targetParam, indexParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed delegate that gets the value of a two-parameter indexer property for a specified
        /// target object.
        /// </summary>
        /// <remarks>The created delegate performs type checking and conversion as needed to match the
        /// indexer property signature. This method is intended for advanced scenarios such as dynamic property access
        /// or code generation.</remarks>
        /// <typeparam name="TTarget">The type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TIndex1">The type of the first index parameter of the indexer property.</typeparam>
        /// <typeparam name="TIndex2">The type of the second index parameter of the indexer property.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer property.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a getter delegate. Must represent a
        /// readable indexer property with exactly two index parameters.</param>
        /// <returns>A delegate that gets the value of the specified two-parameter indexer property for a given target object and
        /// index values.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a getter invoker generated.</exception>
        /// <exception cref="ArgumentException">Thrown if the property is not an indexer, does not have exactly two index parameters, is write-only, or if
        /// the provided type arguments are incompatible with the property or its index parameters.</exception>
        public static IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TValue> CreateIndexerGetter<TTarget, TIndex1, TIndex2, TValue>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

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
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                2,
                propertyData.IndexerParameters.Count,
                nameof(propertyData),
                "The provided indexer property must have exactly two index parameters to create a 2D indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            if (!propertyData.PropertyTypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                propertyType,
                returnType,
                nameof(TValue),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    propertyType,
                    "property type",
                    returnType,
                    nameof(TValue)));
            }

            Type index1Type = typeof(TIndex1);
            TypeData indexerParameter1TypeData = propertyData.IndexerParameters[0].ParameterTypeData;
            Type indexParameter1Type = indexerParameter1TypeData.UnwrapType();
            if (!indexerParameter1TypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        index1Type,
                        indexParameter1Type,
                        nameof(TIndex1),
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                            index1Type,
                            nameof(TIndex1),
                            indexParameter1Type,
                            "indexer parameter 1 type"));
            }

            Type index2Type = typeof(TIndex2);
            TypeData indexerParameter2TypeData = propertyData.IndexerParameters[1].ParameterTypeData;
            Type indexParameter2Type = indexerParameter2TypeData.UnwrapType();
            if (!indexerParameter2TypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        index2Type,
                        indexParameter2Type,
                        nameof(TIndex2),
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                            index2Type,
                            nameof(TIndex2),
                            indexParameter2Type,
                            "indexer parameter 2 type"));
            }

            // (object? propertyType, object?[]? indices) => (object?)((TDeclaring)propertyType)[convertedIndices...]
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
                    $"The provided argument {nameof(TIndex1)} is incompatible with the property's index parameter type. Cast from the provided {index1Type.FullName} to {indexerParameter1TypeData.FullyQualifiedSignature} is not natively supported.",
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
                    $"The provided argument {nameof(TIndex2)} is incompatible with the property's index parameter type. Cast from the provided {index2Type.FullName} to {indexerParameter2TypeData.FullyQualifiedSignature} is not natively supported.",
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
                    $"The provided argument {nameof(TValue)} is incompatible with the property type. Cast from {propertyData.PropertyTypeData.FullyQualifiedSignature} to the provided {returnType.FullName} is not natively supported.",
                    nameof(TValue),
                    e);
            }

            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TValue>>(body, targetParam, indexParam1, indexParam2)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed delegate that retrieves the value of a three-parameter indexer property for a
        /// specified target object.
        /// </summary>
        /// <remarks>The created delegate performs type checking and conversion as needed to match the
        /// indexer property signature. This method is typically used in advanced scenarios such as dynamic property
        /// access or code generation.</remarks>
        /// <typeparam name="TTarget">The type of the object that declares the indexer property.</typeparam>
        /// <typeparam name="TIndex1">The type of the first index parameter of the indexer.</typeparam>
        /// <typeparam name="TIndex2">The type of the second index parameter of the indexer.</typeparam>
        /// <typeparam name="TIndex3">The type of the third index parameter of the indexer.</typeparam>
        /// <typeparam name="TValue">The type of the value returned by the indexer.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a getter. Must represent a readable indexer
        /// with exactly three index parameters.</param>
        /// <returns>A delegate that takes a target object and three index values and returns the value of the specified indexer
        /// property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if a getter invoker has already been generated for the specified property.</exception>
        /// <exception cref="ArgumentException">Thrown if the type arguments are not compatible with the indexer property, if the property is not an
        /// indexer, if it does not have exactly three index parameters, or if the property is write-only.</exception>
        public static IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue> CreateIndexerGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue>(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

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
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
                3,
                propertyData.IndexerParameters.Count,
                nameof(propertyData),
                "The provided indexer property must have exactly three index parameters to create a 3D indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");
            Type returnType = typeof(TValue);
            Type propertyType = propertyData.PropertyTypeData.UnwrapType();
            if (!propertyData.PropertyTypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                propertyType,
                returnType,
                nameof(TValue),
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    propertyType,
                    "property type",
                    returnType,
                    nameof(TValue)));
            }

            Type index1Type = typeof(TIndex1);
            TypeData indexerParameter1TypeData = propertyData.IndexerParameters[0].ParameterTypeData;
            Type indexParameter1Type = indexerParameter1TypeData.UnwrapType();
            if (!indexerParameter1TypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        index1Type,
                        indexParameter1Type,
                        nameof(TIndex1),
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                            index1Type,
                            nameof(TIndex1),
                            indexParameter1Type,
                            "indexer parameter 1 type"));
            }

            Type index2Type = typeof(TIndex2);
            TypeData indexerParameter2TypeData = propertyData.IndexerParameters[1].ParameterTypeData;
            Type indexParameter2Type = indexerParameter2TypeData.UnwrapType();
            if (!indexerParameter2TypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        index2Type,
                        indexParameter2Type,
                        nameof(TIndex2),
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                            index2Type,
                            nameof(TIndex2),
                            indexParameter2Type,
                            "indexer parameter 2 type"));
            }

            Type index3Type = typeof(TIndex3);
            TypeData indexerParameter3TypeData = propertyData.IndexerParameters[2].ParameterTypeData;
            Type indexParameter3Type = indexerParameter3TypeData.UnwrapType();
            if (!indexerParameter3TypeData.IsValueType)
            {
                ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                        index3Type,
                        indexParameter3Type,
                        nameof(TIndex3),
                        ExceptionMessages.GetTypeMismatchExceptionMessage(
                            index3Type,
                            nameof(TIndex3),
                            indexParameter3Type,
                            "indexer parameter 3 type"));
            }

            // (object? propertyType, object?[]? indices) => (object?)((TDeclaring)propertyType)[convertedIndices...]
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
                    $"The provided argument {nameof(TIndex1)} is incompatible with the property's index parameter type. Cast from the provided {index1Type.FullName} to {indexerParameter1TypeData.FullyQualifiedSignature} is not natively supported.",
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
                    $"The provided argument {nameof(TIndex2)} is incompatible with the property's index parameter type. Cast from the provided {index2Type.FullName} to {indexerParameter2TypeData.FullyQualifiedSignature} is not natively supported.",
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
                    $"The provided argument {nameof(TIndex3)} is incompatible with the property's index parameter type. Cast from the provided {index3Type.FullName} to {indexerParameter3TypeData.FullyQualifiedSignature} is not natively supported.",
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
                    $"The provided argument {nameof(TValue)} is incompatible with the property type. Cast from {propertyData.PropertyTypeData.FullyQualifiedSignature} to the provided {returnType.FullName} is not natively supported.",
                    nameof(TValue),
                    e);
            }

            return Expression
                .Lambda<IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue>>(body, targetParam, indexParam1, indexParam2, indexParam3)
                .Compile();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyData"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Func<object?, object[], object?> CreateIndexerGetter(PropertyData propertyData)
        {
            if (propertyData is IPropertyDataInvoker propertyDataInvoker && propertyDataInvoker.HasGetter)
            {
                throw new InvalidOperationException("The 'PropertyData' has already the get invoker generated.");
            }

            ArgumentNullException.ThrowIfNull(propertyData, nameof(propertyData));
            TypeData declaringTypeData = propertyData.DeclaringTypeData;
            ArgumentNullException.ThrowIfNull(declaringTypeData, nameof(propertyData));
            Type targetType = typeof(object);
            Type declaringType = declaringTypeData.UnwrapType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                declaringType,
                "target",
                ExceptionMessages.GetTypeMismatchExceptionMessage(
                    targetType,
                    "target",
                    declaringType,
                    "declaring type"));
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.IsIndexer,
                nameof(propertyData),
                "The provided property must be an indexer to create an indexer getter.");
            ArgumentExceptionAdvanced.ThrowIfFalse(
                propertyData.CanRead,
                nameof(propertyData),
                "Cannot create a getter for a write-only property.");

            // (object? propertyType, object?[]? indices) => (object?)((TDeclaring)propertyType)[convertedIndices...]
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "target");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");

            ImmutableArray<ParameterInfo> indexParameters = propertyData.IndexerParameters.AsParameterInfoArray();

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

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : Expression.Convert(targetParam, declaringType);

            // Access the indexer: propertyType[index0, index1, ...]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, indexExpressions);

            // Box the result
            UnaryExpression body = Expression.Convert(propertyAccess, typeof(object));

            // Build the delegate: Func<object?, object?[]?, object?>
            return Expression
                .Lambda<Func<object?, object[], object?>>(body, targetParam, indicesParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the returnType of the specified property on a given object instance.
        /// </summary>
        /// <remarks>The returned delegate uses object-based parameters. For instance properties declared
        /// on returnType types (structs), use a ref-based setter to avoid modifying a boxed copy. The property must be
        /// writable and not read-only.</remarks>
        /// <param name="propertyData">The metadata describing the property for which to create a setter. Must represent a writable property.
        /// Cannot be null.</param>
        /// <returns>An <see cref="Action{Object, Object}"/> delegate that sets the returnType of the specified property on a propertyType
        /// object. The first parameter is the propertyType object instance (or null for static properties); the second
        /// parameter is the returnType to assign to the property.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
        /// <exception cref="NotSupportedException">Thrown if the property is an instance property declared on a returnType type. Use a ref-based setter instead.</exception>
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
                    $"You need a ref-based setter: call {nameof(CreateStructSetter)} instead.");
            }

            ArgumentExceptionAdvanced.ThrowIfTrue(
                propertyData.IsReadOnly,
                nameof(propertyData),
                "Cannot create a setter for an read-only property.");
            ArgumentNullException.ThrowIfNull(propertyData.DeclaringTypeData, nameof(propertyData));

            ParameterExpression targetParam = Expression.Parameter(typeof(object), "propertyType");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "returnType");

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

        /// <summary>
        /// Creates a delegate that sets the returnType of an indexer property on a specified object using the provided
        /// indices and returnType.
        /// </summary>
        /// <remarks>The returned delegate expects the propertyType object, an array of index values, and the
        /// returnType to set. For static indexers, the propertyType parameter is ignored. The method validates that the number of
        /// indices matches the indexer signature at runtime. Attempting to use this setter on a boxed struct will not
        /// modify the original returnType; use a ref-based setter for returnType types.</remarks>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a setter delegate. Must represent a
        /// writable indexer property.</param>
        /// <returns>An <see cref="Action{Object, Object[], Object}"/> delegate that sets the returnType of the specified indexer
        /// property on a propertyType object using the given indices and returnType.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified <paramref name="propertyData"/> already has a set invoker generated.</exception>
        /// <exception cref="NotSupportedException">Thrown if the indexer property is an instance property declared on a returnType type. Use a ref-based setter
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
                    $"You need a ref-based setter: call {nameof(CreateStructIndexerSetter)} instead.");
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

            // (object? propertyType, object?[]? indices) => (object?)((TDeclaring)propertyType)[convertedIndices...]
            ParameterExpression targetParam = Expression.Parameter(typeof(object), "propertyType");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "returnType");

            Type declaringType = propertyData.DeclaringTypeData.UnwrapType();

            ImmutableArray<ParameterInfo> indexParameters = propertyData.IndexerParameters.AsParameterInfoArray();

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

            // Access the indexer: propertyType[index0, index1, ...]
            PropertyInfo propertyInfo = propertyData.GetPropertyInfo();

            Expression? instanceExpression = propertyData.IsStatic
                ? null
                : Expression.Convert(targetParam, declaringType);
            IndexExpression propertyAccess = Expression.MakeIndex(instanceExpression, propertyInfo, indexExpressions);
            BinaryExpression assign = Expression.Assign(
                propertyAccess,
                Expression.Convert(valueParam, propertyData.PropertyTypeData.UnwrapType())); // Expression.Assign 

            // Action<...> requires a void body -> wrap assignment in a void block.
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            // Build the delegate: Func<object?, object?[]?, object?>
            return Expression
                .Lambda<Action<object?, object?[]?, object?>>(body, targetParam, indicesParam, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a strongly-typed setter delegate for an instance property of a returnType type (struct).
        /// </summary>
        /// <remarks>Use this method to generate a setter for struct instance properties. For static or
        /// reference type properties, use CreateSetter instead. The returned delegate operates on a struct passed by
        /// reference, allowing direct property assignment.</remarks>
        /// <typeparam name="TTarget">The returnType type (struct) that declares the property to set.</typeparam>
        /// <typeparam name="TValue">The type of the returnType to assign to the property.</typeparam>
        /// <param name="propertyData">The metadata describing the property for which to create a setter. Must represent a non-static,
        /// non-read-only property of the specified returnType type.</param>
        /// <returns>A delegate that sets the returnType of the specified property on a given struct instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the specified property already has a set invoker generated.</exception>
        public static ValueTypeMemberSetter<TTarget, TValue> CreateStructSetter<TTarget, TValue>(PropertyData propertyData)
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
                $"For reference type instance properties or class properties (static) call {nameof(CreateSetter)} instead.");
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
                        "property type"));

            // (ref TTarget propertyType, TValue returnType) => propertyType.Property = returnType;
            ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "propertyType");
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "returnType");

            PropertyInfo property = propertyData.GetPropertyInfo();
            MemberExpression propertyAccess = Expression.Property(targetByRef, property);
            BinaryExpression assign = Expression.Assign(propertyAccess, Expression.Convert(valueParam, propertyType));
            BlockExpression body = Expression.Block(assign, Expression.Empty());

            return Expression
                .Lambda<ValueTypeMemberSetter<TTarget, TValue>>(body, targetByRef, valueParam)
                .Compile();
        }

        /// <summary>
        /// Creates a delegate that sets the returnType of an indexer property on a returnType type instance.
        /// </summary>
        /// <remarks>Use this method to generate a performant setter for struct indexer properties when
        /// reflection-based property access is required. The returned delegate expects the propertyType struct to be passed
        /// by reference, along with the index values and the returnType to set.</remarks>
        /// <typeparam name="TTarget">The returnType type that declares the indexer property. Must be a struct.</typeparam>
        /// <typeparam name="TValue">The type of the returnType to set on the indexer property.</typeparam>
        /// <param name="propertyData">The metadata describing the indexer property for which to create a setter. Must represent a non-static,
        /// non-read-only indexer property and cannot be null.</param>
        /// <returns>A delegate that sets the returnType of the specified indexer property on a returnType type instance using the provided
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
                $"For reference type instance properties or class properties (static) call {nameof(CreateSetter)} instead.");
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
                        "property type"));

            // (ref TTarget propertyType, TValue returnType) => propertyType.Property = returnType;
            ParameterExpression targetByRef = Expression.Parameter(typeof(TTarget).MakeByRefType(), "propertyType");
            ParameterExpression indicesParam = Expression.Parameter(typeof(object[]), "indices");
            ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "returnType");

            ImmutableArray<ParameterInfo> indexParameters = propertyData.IndexerParameters.AsParameterInfoArray();

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

        #region InvokerKeyMapKey

        private readonly struct InvokerKeyMapKey : IEquatable<InvokerKeyMapKey>
        {
            public ImmutableList<RuntimeTypeHandle> MethodParameterTypeHandles { get; }
            private readonly int? _hashCode;

            public InvokerKeyMapKey(ImmutableList<RuntimeTypeHandle> methodParameterTypeHandles) : this()
            {
                ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(methodParameterTypeHandles, nameof(methodParameterTypeHandles), "Method parameter declaringType handles must be provided to create an invocator map key.");

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
