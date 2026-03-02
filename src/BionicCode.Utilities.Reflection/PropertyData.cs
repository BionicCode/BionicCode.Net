//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

internal sealed class PropertyData : MemberData
{
    private string? _displayName;
    private string? _shortDisplayName;
    private string? _fullyQualifiedDisplayName;
    private string? _signature;
    private string? _shortSignature;
    private string? _shortCompactSignature;
    private string? _fullyQualifiedSignature;
    private string? _fullyQualifiedRuntimeSignature;
    private string? _runtimeSignature;
    private string? _runtimeShortSignature;
    private string? _runtimeShortCompactSignature;
    private SymbolAttributes _symbolAttributes;
    private AccessModifier _propertyAccessModifier;
    private AccessModifier _setAccessorAccessModifier;
    private AccessModifier _getAccessorAccessModifier;
    private ParameterList? _getMethodParameters;
    private ParameterList? _setMethodParameters;
    private TypeData? _propertyTypeData;
    private MethodData? _getMethodData;
    private MethodData? _setMethodData;
    private bool? _isStatic;
    private bool? _isOverride;
    private bool? _isSealed;
    private bool? _canWrite;
    private bool? _canRead;
    private bool? _isInit;
    private bool? _isPublic;
    private bool? _isPrivate;
    private bool? _isAssembly;
    private bool? _isFamily;
    private bool? _isFamilyOrAssembly;
    private bool? _isFamilyAndAssembly;
    private readonly ConcurrentDictionary<RuntimeTypeHandle, Delegate> _invokerTable;
    private string? _assemblyName;
    private SymbolComponentInfo? _symbolComponentInfo;
    private readonly WellKnownPropertyDescriptor _descriptor;
    private RuntimeTypeHandle? _declaringTypeHandle;
    private RuntimeTypeHandle? _implementingTypeHandle;
    private bool? _isExplicitInterfaceImplementation;
    private IPropertyDataView? _propertyDataView;

    internal PropertyData(WellKnownPropertyDescriptor descriptor)
        : base(descriptor.PropertyName, SymbolKind.MemberProperty)
    {
        ArgumentNullExceptionAdvanced.ThrowIfDefault(descriptor);

        _descriptor = descriptor;
        _invokerTable = new ConcurrentDictionary<RuntimeTypeHandle, Delegate>();
        PropertyInfo = _descriptor.PropertyInfo;
    }

    internal new IPropertyDataView View => _propertyDataView ??= new PropertyDataView(GetPublicCacheKey());

    protected override MemberInfo MemberInfo => PropertyInfo;

    /// <summary>
    /// Gets the value of the property represented by this instance for the specified target object.
    /// </summary>
    /// <remarks>For indexer properties, call <see cref="GetIndexerValue(object?, object[])"/> or an overload. <para/>
    /// If the types are known at compile time use the strictly typed overload  <see cref="GetValue{TTarget, TValue}(TTarget)"/> instead to boost performance (e.g. avoid boxing).<para/>
    /// For static properties, the target parameter is ignored.</remarks>
    /// <param name="target">The instance whose property value is to be retrieved. For static properties, this parameter is ignored.<brb/>
    /// For instance properties, this must be an instance of a type assignable to the declaring type; cannot be <see langword="null"/> for instance
    /// properties.</param>
    /// <returns>The value of the property for the specified target object and index parameters, or <see langword="null"/> if the property
    /// value is <see langword="null"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a getter or if the property is an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target is <see langword="null"/> for an instance property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal object? GetValue(object? target) => GetValue<object?, object?>(target);

    /// <summary>
    /// Gets the value of the property represented by this instance for the specified target object and optional
    /// index parameters.
    /// </summary>
    /// <remarks>For indexer properties, the number and types of elements in indexerPropertyParameters must match the indexer parameters defined in the property.</remarks>
    /// <typeparam name="TTarget">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the property getter.</typeparam>
    /// <param name="target">The instance to invoke the property getter on.</param>
    /// <returns>The property value of type <typeparamref name="TValue"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a getter or if the property is an indexer.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target is <see langword="null"/> for an instance property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal TValue GetValue<TTarget, TValue>(TTarget target)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use one of the overloads that accepts indexer parameters.");
        }

        if (IsStatic)
        {
            PropertyGetter<object?, TValue> staticPropertySetInvoker = GetStaticPropertyGetterInternal<TValue>();
            return staticPropertySetInvoker.Invoke(null);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            PropertyGetter<TTarget, TValue> propertySetInvoker = GetPropertyGetterInternal<TTarget, TValue>();
            return propertySetInvoker.Invoke(target);
        }
    }

    /// <summary>
    /// Gets the value of the indexer property for the specified target object and index parameters.
    /// </summary>
    /// <remarks>For instance indexer properties, the target object must be assignable to the
    /// declaring type of the property. The length and types of the indexerPropertyParameters array must match the
    /// indexer parameters defined by the property.</remarks>
    /// <param name="target">The object instance on which to access the indexer property. Must be non-null for instance properties;
    /// ignored for static properties.</param>
    /// <param name="indices">An array of values representing the index parameters required by the indexer property. The number and types
    /// of elements must match the indexer definition. Cannot be null.</param>
    /// <returns>The value returned by the indexer property for the specified target and index parameters.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a getter or is not an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target is null for an instance property, or if indexerPropertyParameters is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of elements in indexerPropertyParameters does not match the indexer parameter count of the property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal object? GetIndexerValue(object? target, params object?[] indices) => GetIndexerValue<object?, object?, object?>(target, indices);

    /// <summary>
    /// Gets the value of the indexer property represented by this instance for the specified target object and
    /// indexer parameter.
    /// </summary>
    /// <remarks>Use this method for 1D indexer properties that accept a single index parameter.</remarks>
    /// <typeparam name="TTarget">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the property getter.</typeparam>
    /// <param name="target">The target instance the property is invoked on.</param>
    /// <param name="indices"></param>
    /// <returns>The value of the indexer property of type <typeparamref name="TValue"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the property is not readable or if the property is not an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the indexer parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the indexer parameter count does not match the indexer parameter count of the property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object?[] indices) => GetIndexerValue<TTarget, TValue, object?>(target, indices);

    /// <summary>
    /// Gets the value of the indexer property represented by this instance for the specified target object and
    /// indexer parameter.
    /// </summary>
    /// <remarks>Use this method for 1D indexer properties that accept a single index parameter.</remarks>
    /// <typeparam name="TTarget">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the property getter.</typeparam>
    /// <typeparam name="TIndex">The type of the indexer parameter.</typeparam>
    /// <param name="target">The target instance the property is invoked on.</param>
    /// <param name="indices"></param>
    /// <returns>The value of the indexer property of type <typeparamref name="TValue"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the property is not readable or if the property is not an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the indexer parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the indexer parameter count does not match the indexer parameter count of the property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indices)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use one of the overloads without indexer parameters instead.");
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(indices, nameof(indices), $"Indexer index parameter {nameof(indices)} cannot be null for indexer properties.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            indices.Length,
            PropertyGetMethodParameters.Count,
            nameof(indices),
            $"Provided number of indexer parameters does not match the indexer parameter count of this property '{FullyQualifiedSignature}'. Expected: {PropertyGetMethodParameters.Count} index parameters; Found: {indices.Length}.");

        if (IsStatic)
        {
            IndexerPropertyGetter<object?, TValue, TIndex> staticPropertyGetInvoker = GetStaticIndexerGetterInternal<TValue, TIndex>();
            return staticPropertyGetInvoker.Invoke(null, indices!);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            IndexerPropertyGetter<TTarget, TValue, TIndex> propertyGetInvoker = GetIndexerGetterInternal<TTarget, TValue, TIndex>();
            return propertyGetInvoker.Invoke(target, indices!);
        }
    }

    internal TValue GetIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget target, TIndex1 index1, TIndex2 index2)
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetValue)} instead.");
        }

        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            PropertyGetMethodParameters.Count,
            nameof(TIndex2),
            $"Indexer property parameter count mismatch. Provided 2 indexer parameters for an indexer that requires {PropertyGetMethodParameters.Count} parameters. Please use the appropriate overload that matches the number of indexer parameters.");

        if (IsStatic)
        {
            IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2> staticInvoker = GetStatic2DIndexerGetterInternal<TValue, TIndex1, TIndex2>();
            return staticInvoker.Invoke(null, index1, index2);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2> invoker = Get2DIndexerGetterInternal<TTarget, TValue, TIndex1, TIndex2>();
            return invoker.Invoke(target, index1, index2);
        }
    }

    internal TValue GetIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget target, TIndex1 index1, TIndex2 index2, TIndex3 index3)
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetValue)} instead.");
        }

        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            PropertyGetMethodParameters.Count,
            nameof(TIndex3),
            $"Indexer property parameter count mismatch. Provided 3 indexer parameters for an indexer that requires {PropertyGetMethodParameters.Count} parameters. Please use the appropriate overload that matches the number of indexer parameters.");

        if (IsStatic)
        {

            IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2, TIndex3> staticInvoker = GetStatic3DIndexerGetterInternal<TValue, TIndex1, TIndex2, TIndex3>();
            return staticInvoker.Invoke(null, index1, index2, index3);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> invoker = Get3DIndexerGetterInternal<TTarget, TValue, TIndex1, TIndex2, TIndex3>();
            return invoker.Invoke(target, index1, index2, index3);
        }
    }

    /// <summary>
    /// Sets the value of the property on the specified target object.
    /// </summary>
    /// <remarks>To set the value of an indexer property, use an overload that accepts indexer
    /// parameters. The method validates that the target object is compatible with the declaring type of the
    /// property.<para/>
    /// If the types are known at compile time use the <see cref="SetValue{TTarget, TValue}(TTarget, TValue)"/> overload to significantly improve performance (e.g. avoid boxing).</remarks>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance
    /// properties, this must be a non-null object assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or is an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the property is an instance property and the target object is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal void SetValue(object? target, object? value) => SetValue<object?, object?>(target, value);

    /// <summary>
    /// Sets the value of the property on the specified target object.
    /// </summary>
    /// <typeparam name="TTarget">The type of the target object.</typeparam>
    /// <typeparam name="TValue">The type of the value to assign to the property.</typeparam>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance properties, this must be a non-null object assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or is an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the property is an instance property and the target object is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal void SetValue<TTarget, TValue>(TTarget? target, TValue value) where TTarget : class?
    {
        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetStructValue)));
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use one of the overloads that accepts indexer parameters.");
        }

        if (IsStatic)
        {
            PropertySetter<object?, TValue> propertySetInvoker = GetStaticSetInvokerInternal<TValue>();
            propertySetInvoker(null, value);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            PropertySetter<TTarget?, TValue> propertySetInvoker = GetSetInvokerInternal<TTarget, TValue>();
            propertySetInvoker(target, value);
        }
    }

    /// <summary>
    /// Sets the value of a specified property or field on a struct instance, ensuring that the member is writable and
    /// the types are compatible.
    /// </summary>
    /// <remarks>This method is intended for use with value types only. Ensure that the target struct is
    /// mutable and that the value being set is compatible with the member's type.</remarks>
    /// <typeparam name="TTarget">The type of the struct whose member value is to be set. Must be a value type.</typeparam>
    /// <typeparam name="TValue">The type of the value to assign to the struct member.</typeparam>
    /// <param name="target">A reference to the struct instance whose member value will be set. The type must be assignable to the declaring
    /// type of the member.</param>
    /// <param name="value">The value to assign to the specified member of the struct.</param>
    /// <exception cref="InvalidOperationException">Thrown if the declaring type is a reference type, if the property is read-only, if the property is static, or if
    /// the property is an indexer.</exception>
    internal void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value) where TTarget : struct
    {
        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetValue)));
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (IsStatic)
        {
            throw new InvalidOperationException($"Setting static property values on struct types is not supported by this method. Call '{nameof(SetValue)}' instead.");
        }

        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use '{nameof(SetStructIndexerValue)}' instead.");
        }

        Type targetType = typeof(TTarget);
        ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
            targetType,
            DeclaringTypeData.Type,
            nameof(target),
            $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

        ValueTypeMemberSetter<TTarget, TValue> propertySetInvoker = GetStructSetInvokerInternal<TTarget, TValue>();
        propertySetInvoker.Invoke(ref target, value);
    }

    /// <summary>
    /// Gets a delegate that sets the value of an indexer property on a struct type for the specified target and
    /// value types.
    /// </summary>
    /// <remarks>Use this method when you need to set the value of an indexer property on a value type
    /// (struct) using a strongly typed delegate. For non-indexer properties or properties declared on reference
    /// types, use the appropriate alternative methods.</remarks>
    /// <typeparam name="TTarget">The struct type that declares the indexer property.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <returns>A delegate that sets the value of the indexer property on the specified struct type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not an indexer property, or if the property is not declared on a struct type.</exception>
    internal void SetStructIndexerValue<TTarget, TValue>(ref TTarget target, TValue value, params object?[] indices) where TTarget : struct => SetStructIndexerValue<TTarget, TValue, object?>(ref target, value, indices);

    /// <summary>
    /// Sets the value of the property on the specified target object, optionally using index parameters for indexer
    /// properties.
    /// </summary>
    /// <remarks>For indexer properties, the number and types of elements in <paramref name="indices"/> must
    /// match the indexer parameters defined by the property. 
    /// <para/>
    /// If types are known at compile time use a strictly typed overload instead to boost performance (e.g. avoid boxing).</remarks>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance
    /// properties, this cannot be <see langword="null"/> and must be assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="indices">An array of index values to use if the property is an indexer. The number of elements must match the number
    /// of indexer parameters. This parameter is required for indexer properties and ignored for non-indexer
    /// properties.</param>
    /// <typeparam name="TIndex">The type of the indexer parameters.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or if the declaring type is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target object is <see langword="null"/> for an instance property, or if <paramref name="indices"/> is <see langword="null"/> for an
    /// indexer property.</exception>
    internal void SetIndexerValue<TTarget, TValue>(TTarget? target, TValue value, params object?[] indices) where TTarget : class? => SetIndexerValue<TTarget?, TValue, object?>(target, value, indices);

    /// <summary>
    /// Sets the value of the property on the specified target object, optionally using index parameters for indexer
    /// properties.
    /// </summary>
    /// <remarks>For indexer properties, the number and types of elements in <paramref name="indices"/> must
    /// match the indexer parameters defined by the property. 
    /// <para/>
    /// If types are known at compile time use a strictly typed overload instead to boost performance (e.g. avoid boxing).</remarks>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance
    /// properties, this cannot be <see langword="null"/> and must be assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="indices">An array of index values to use if the property is an indexer. The number of elements must match the number
    /// of indexer parameters. This parameter is required for indexer properties and ignored for non-indexer
    /// properties.</param>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or if the declaring type is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target object is <see langword="null"/> for an instance property, or if <paramref name="indices"/> is <see langword="null"/> for an
    /// indexer property.</exception>
    internal void SetIndexerValue(object? target, object? value, params object?[] indices) => SetIndexerValue<object?, object?, object?>(target, value, indices);

    /// <summary>
    /// Gets a delegate that sets the value of an indexer property on a struct type for the specified target and
    /// value types.
    /// </summary>
    /// <remarks>Use this method when you need to set the value of an indexer property on a value type
    /// (struct) using a strongly typed delegate. For non-indexer properties or properties declared on reference
    /// types, use the appropriate alternative methods.</remarks>
    /// <typeparam name="TTarget">The struct type that declares the indexer property.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <typeparam name="TIndex">The type of the indexer parameter(s) of the <see langword="params"/> array <paramref name="indices"/>.</typeparam>
    /// <returns>A delegate that sets the value of the indexer property on the specified struct type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not an indexer property, or if the property is not declared on a struct type.</exception>
    internal void SetStructIndexerValue<TTarget, TValue, TIndex>(ref TTarget target, TValue value, params TIndex[] indices) where TTarget : struct
    {
        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetIndexerValue)));
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{Signature}' is not an indexer property. Use {nameof(SetStructValue)} instead.");
        }

        if (IsStatic)
        {
            throw new InvalidOperationException($"Setting static property values on struct types is not supported by this method. Call '{nameof(SetValue)}<TTarget, TValue>(ref TTarget target, TValue value)' instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{Signature}' does not have a setter.");
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(indices, nameof(indices), "Indexer property index cannot be null for indexer properties.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            indices.Length,
            PropertySetMethodParameters.Count - 1, // Subtract 1 to exclude the value parameter
            nameof(indices),
            $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");

        ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex> invoker = GetStructIndexerSetInvokerInternal<TTarget, TValue, TIndex>();
        invoker.Invoke(ref target, value, indices);
    }

    /// <summary>
    /// Sets the value of the property on the specified target object, optionally using index parameters for indexer
    /// properties.
    /// </summary>
    /// <remarks>For indexer properties, the number and types of elements in <paramref name="indices"/> must
    /// match the indexer parameters defined by the property. 
    /// <para/>
    /// If types are known at compile time use a strictly typed overload instead to boost performance (e.g. avoid boxing).</remarks>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance
    /// properties, this cannot be null and must be assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="indices">An array of index values to use if the property is an indexer. The number of elements must match the number
    /// of indexer parameters. This parameter is required for indexer properties and ignored for non-indexer
    /// properties.</param>
    /// <typeparam name="TTarget">The type of the target object. Must be a reference type.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <typeparam name="TIndex">The type of the indexer parameters.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or if the declaring type is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target object is null for an instance property, or if <paramref name="indices"/> is null for an
    /// indexer property.</exception>
    internal void SetIndexerValue<TTarget, TValue, TIndex>(TTarget? target, TValue value, params TIndex[] indices) where TTarget : class?
    {
        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetIndexerValue)));
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use '{nameof(SetValue)}' instead.");
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(indices, nameof(indices), "Indexer property index cannot be null for indexer properties.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            indices.Length,
            PropertySetMethodParameters.Count - 1, // Subtract 1 to exclude the value parameter
            nameof(indices),
            $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");

        if (IsStatic)
        {
            IndexerPropertySetter<object?, TValue, TIndex> staticPropertySetInvoker = GetStaticIndexerSetInvokerInternal<TValue, TIndex>();
            staticPropertySetInvoker(null, value, indices!);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            IndexerPropertySetter<TTarget?, TValue, TIndex> staticPropertySetInvoker = GetIndexerSetInvokerInternal<TTarget, TValue, TIndex>();
            staticPropertySetInvoker(target, value, indices!);
        }
    }

    /// <summary>
    /// Gets a delegate that sets the value of an indexer property on a struct type for the specified target and
    /// value types.
    /// </summary>
    /// <remarks>Use this method when you need to set the value of an indexer property on a value type
    /// (struct) using a strongly typed delegate. For non-indexer properties or properties declared on reference
    /// types, use the appropriate alternative methods.</remarks>
    /// <param name="target">The struct instance the property is invoked on.</param>
    /// <param name="value">The value to set on the indexer property.</param>
    /// <param name="index1">The first indexer parameter value.</param>
    /// <param name="index2">The second indexer parameter value.</param>
    /// <typeparam name="TTarget">The struct type that declares the indexer property.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <typeparam name="TIndex1">The type of the first indexer parameter.</typeparam>
    /// <typeparam name="TIndex2">The type of the second indexer parameter.</typeparam>
    /// <returns>A delegate that sets the value of the indexer property on the specified struct type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not an indexer property, or if the property is not declared on a struct type.</exception>
    internal void SetStructIndexerValue<TTarget, TValue, TIndex1, TIndex2>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : struct
    {
        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetIndexerValue)));
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{Signature}' is not an indexer property. Use {nameof(SetStructValue)} instead.");
        }

        if (IsStatic)
        {
            throw new InvalidOperationException($"Setting static property values on struct types is not supported by this method. Call '{nameof(SetValue)}<TTarget, TValue>(ref TTarget target, TValue value)' instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{Signature}' does not have a setter.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            PropertySetMethodParameters.Count - 1, // Subtract 1 to exclude the value parameter
            nameof(TIndex2),
            $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");

        ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2> invoker = GetStruct2DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2>();
        invoker.Invoke(ref target, value, index1, index2);
    }

    /// <summary>
    /// Sets the value of the property on the specified target object, optionally using index parameters for indexer
    /// properties.
    /// </summary>
    /// <remarks>For indexer properties, the number and types of elements in indexerPropertyParameters must
    /// match the indexer parameters defined by the property. 
    /// <para/>
    /// If types are known at compile time use a strictly typed overload instead to boost performance (e.g. avoid boxing).</remarks>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance
    /// properties, this cannot be null and must be assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="index1">The first indexer parameter value.</param>
    /// <param name="index2">The second indexer parameter value.</param>
    /// <typeparam name="TTarget">The type of the target object. Must be a reference type.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <typeparam name="TIndex1">The type of the first indexer parameter.</typeparam>
    /// <typeparam name="TIndex2">The type of the second indexer parameter.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or if the declaring type is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target object is null for an instance property, or if indexerPropertyParameters is null for an
    /// indexer property.</exception>
    internal void SetIndexerValue<TTarget, TValue, TIndex1, TIndex2>(TTarget? target, TValue value, TIndex1 index1, TIndex2 index2) where TTarget : class?
    {
        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetIndexerValue)));
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use '{nameof(SetValue)}' instead.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            2,
            PropertySetMethodParameters.Count - 1, // Subtract 1 to exclude the value parameter
            nameof(TIndex2),
            $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");

        if (IsStatic)
        {
            IndexerPropertySetter<object?, TValue, TIndex1, TIndex2> staticPropertySetInvoker = GetStatic2DIndexerSetInvokerInternal<TValue, TIndex1, TIndex2>();
            staticPropertySetInvoker(null, value, index1, index2);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2> staticPropertySetInvoker = Get2DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2>();
            staticPropertySetInvoker(target, value, index1, index2);
        }
    }

    /// <summary>
    /// Gets a delegate that sets the value of an indexer property on a struct type for the specified target and
    /// value types.
    /// </summary>
    /// <remarks>Use this method when you need to set the value of an indexer property on a value type
    /// (struct) using a strongly typed delegate. For non-indexer properties or properties declared on reference
    /// types, use the appropriate alternative methods.</remarks>
    /// <typeparam name="TTarget">The struct type that declares the indexer property.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <typeparam name="TIndex1">The type of the first indexer parameter.</typeparam>
    /// <typeparam name="TIndex2">The type of the second indexer parameter.</typeparam>
    /// <typeparam name="TIndex3">The type of the third indexer parameter.</typeparam>
    /// <returns>A delegate that sets the value of the indexer property on the specified struct type.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not an indexer property, or if the property is not declared on a struct type.</exception>
    internal void SetStructIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(ref TTarget target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : struct
    {
        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetIndexerValue)));
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{Signature}' is not an indexer property. Use {nameof(SetStructValue)} instead.");
        }

        if (IsStatic)
        {
            throw new InvalidOperationException($"Setting static property values on struct types is not supported by this method. Call '{nameof(SetValue)}<TTarget, TValue>(ref TTarget target, TValue value)' instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{Signature}' does not have a setter.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            PropertySetMethodParameters.Count - 1, // Subtract 1 to exclude the value parameter
            nameof(TIndex3),
            $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");

        ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> invoker = GetStruct3DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2, TIndex3>();
        invoker.Invoke(ref target, value, index1, index2, index3);
    }

    /// <summary>
    /// Sets the value of the property on the specified target object, optionally using index parameters for indexer
    /// properties.
    /// </summary>
    /// <remarks>For indexer properties, the number and types of elements in indexerPropertyParameters must
    /// match the indexer parameters defined by the property. 
    /// <para/>
    /// If types are known at compile time use a strictly typed overload instead to boost performance (e.g. avoid boxing).</remarks>
    /// <param name="target">The object whose property value will be set. For static properties, this parameter is ignored. For instance
    /// properties, this cannot be null and must be assignable to the declaring type of the property.</param>
    /// <param name="value">The value to assign to the property.</param>
    /// <param name="index1">The first indexer parameter value.</param>
    /// <param name="index2">The second indexer parameter value.</param>
    /// <param name="index3">The third indexer parameter value.</param>
    /// <typeparam name="TTarget">The type of the target object. Must be a reference type.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the indexer property.</typeparam>
    /// <typeparam name="TIndex1">The type of the first indexer parameter.</typeparam>
    /// <typeparam name="TIndex2">The type of the second indexer parameter.</typeparam>
    /// <typeparam name="TIndex3">The type of the third indexer parameter.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or if the declaring type is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target object is null for an instance property, or if indexerPropertyParameters is null for an
    /// indexer property.</exception>
    internal void SetIndexerValue<TTarget, TValue, TIndex1, TIndex2, TIndex3>(TTarget? target, TValue value, TIndex1 index1, TIndex2 index2, TIndex3 index3) where TTarget : class?
    {
        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetIndexerValue)));
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use '{nameof(SetValue)}' instead.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            PropertySetMethodParameters.Count - 1, // Subtract 1 to exclude the value parameter
            nameof(TIndex3),
            $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");

        if (IsStatic)
        {
            IndexerPropertySetter<object?, TValue, TIndex1, TIndex2, TIndex3> staticPropertySetInvoker = GetStatic3DIndexerSetInvokerInternal<TValue, TIndex1, TIndex2, TIndex3>();
            staticPropertySetInvoker(null, value, index1, index2, index3);
        }
        else
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target), "Target object cannot be null for instance properties.");
            }

            Type targetType = target.GetType();
            ArgumentExceptionAdvanced.ThrowIfNotAssignableTo(
                targetType,
                DeclaringTypeData.Type,
                nameof(target),
                $"Type mismatch. Reason: The instance type {targetType.ToFullyQualifiedSignatureName()} is not assignable to {DeclaringTypeData.FullyQualifiedSignature}");

            IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3> staticPropertySetInvoker = Get3DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2, TIndex3>();
            staticPropertySetInvoker(target, value, index1, index2, index3);
        }
    }

    private IndexerPropertyGetter<TTarget, TValue, TIndex> GetIndexerGetterInternal<TTarget, TValue, TIndex>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertyGetter<TTarget, TValue, TIndex>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateIndexerGetter<TTarget, TValue, TIndex>(this));

        return (IndexerPropertyGetter<TTarget, TValue, TIndex>)invoker;
    }

    private IndexerPropertyGetter<object?, TValue, TIndex> GetStaticIndexerGetterInternal<TValue, TIndex>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertyGetter<object?, TValue, TIndex>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateStaticIndexerGetter<TValue, TIndex>(this));

        return (IndexerPropertyGetter<object?, TValue, TIndex>)invoker;
    }

    private IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2> Get2DIndexerGetterInternal<TTarget, TValue, TIndex1, TIndex2>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.Create2DIndexerGetter<TTarget, TValue, TIndex1, TIndex2>(this));

        return (IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2>)invoker;
    }

    private IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2> GetStatic2DIndexerGetterInternal<TValue, TIndex1, TIndex2>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateStatic2DIndexerGetter<TValue, TIndex1, TIndex2>(this));

        return (IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2>)invoker;
    }

    private IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> Get3DIndexerGetterInternal<TTarget, TValue, TIndex1, TIndex2, TIndex3>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.Create3DIndexerGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(this));

        return (IndexerPropertyGetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>)invoker;
    }

    private IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2, TIndex3> GetStatic3DIndexerGetterInternal<TValue, TIndex1, TIndex2, TIndex3>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2, TIndex3>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateStatic3DIndexerGetter<TValue, TIndex1, TIndex2, TIndex3>(this));

        return (IndexerPropertyGetter<object?, TValue, TIndex1, TIndex2, TIndex3>)invoker;
    }

    private ValueTypeMemberSetter<TTarget, TValue> GetStructSetInvokerInternal<TTarget, TValue>() where TTarget : struct
    {
        RuntimeTypeHandle targetTypeHandle = typeof(ValueTypeMemberSetter<TTarget, TValue>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, _ => DelegateProvider.CreateStructSetter<TTarget, TValue>(this));
        var invoker = (ValueTypeMemberSetter<TTarget, TValue>)cachedInvoker;

        return invoker;
    }

    private PropertyGetter<TTarget, TValue> GetPropertyGetterInternal<TTarget, TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(PropertyGetter<TTarget, TValue>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateGetter<TTarget, TValue>(this));

        return (PropertyGetter<TTarget, TValue>)invoker;
    }

    private PropertyGetter<object?, TValue> GetStaticPropertyGetterInternal<TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(PropertyGetter<object?, TValue>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateStaticGetter<TValue>(this));

        return (PropertyGetter<object?, TValue>)invoker;
    }

    private PropertySetter<TTarget?, TValue> GetSetInvokerInternal<TTarget, TValue>() where TTarget : class?
    {
        RuntimeTypeHandle targetTypeHandle = typeof(PropertySetter<TTarget?, TValue>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateSetter<TTarget?, TValue>(this));

        return (PropertySetter<TTarget?, TValue>)invoker;
    }

    private PropertySetter<object?, TValue> GetStaticSetInvokerInternal<TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(PropertySetter<object?, TValue>).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateStaticSetter<TValue>(this));

        return (PropertySetter<object?, TValue>)invoker;
    }

    private ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex> GetStructIndexerSetInvokerInternal<TTarget, TValue, TIndex>() where TTarget : struct
    {
        RuntimeTypeHandle targetTypeHandle = typeof(ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStructIndexerSetter<TTarget, TValue, TIndex>(this));
        var invoker = (ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex>)cachedInvoker;
        return invoker;
    }

    private IndexerPropertySetter<TTarget?, TValue, TIndex> GetIndexerSetInvokerInternal<TTarget, TValue, TIndex>() where TTarget : class?
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertySetter<TTarget?, TValue, TIndex>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateIndexerSetter<TTarget?, TValue, TIndex>(this));
        var invoker = (IndexerPropertySetter<TTarget?, TValue, TIndex>)cachedInvoker;
        return invoker;
    }

    private IndexerPropertySetter<object?, TValue, TIndex> GetStaticIndexerSetInvokerInternal<TValue, TIndex>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertySetter<object?, TValue, TIndex>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStaticIndexerSetter<TValue, TIndex>(this));
        var invoker = (IndexerPropertySetter<object?, TValue, TIndex>)cachedInvoker;
        return invoker;
    }

    private ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2> GetStruct2DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2>() where TTarget : struct
    {
        RuntimeTypeHandle targetTypeHandle = typeof(ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStruct2DIndexerSetter<TTarget, TValue, TIndex1, TIndex2>(this));
        var invoker = (ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2>)cachedInvoker;
        return invoker;
    }

    private IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2> Get2DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2>() where TTarget : class?
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.Create2DIndexerSetter<TTarget?, TValue, TIndex1, TIndex2>(this));
        var invoker = (IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2>)cachedInvoker;
        return invoker;
    }

    private IndexerPropertySetter<object?, TValue, TIndex1, TIndex2> GetStatic2DIndexerSetInvokerInternal<TValue, TIndex1, TIndex2>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertySetter<object?, TValue, TIndex1, TIndex2>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStatic2DIndexerSetter<TValue, TIndex1, TIndex2>(this));
        var invoker = (IndexerPropertySetter<object?, TValue, TIndex1, TIndex2>)cachedInvoker;
        return invoker;
    }

    private ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3> GetStruct3DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2, TIndex3>() where TTarget : struct
    {
        RuntimeTypeHandle targetTypeHandle = typeof(ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStruct3DIndexerSetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>(this));
        var invoker = (ValueTypeIndexerPropertySetter<TTarget, TValue, TIndex1, TIndex2, TIndex3>)cachedInvoker;
        return invoker;
    }

    private IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3> Get3DIndexerSetInvokerInternal<TTarget, TValue, TIndex1, TIndex2, TIndex3>() where TTarget : class?
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.Create3DIndexerSetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3>(this));
        var invoker = (IndexerPropertySetter<TTarget?, TValue, TIndex1, TIndex2, TIndex3>)cachedInvoker;
        return invoker;
    }

    private IndexerPropertySetter<object?, TValue, TIndex1, TIndex2, TIndex3> GetStatic3DIndexerSetInvokerInternal<TValue, TIndex1, TIndex2, TIndex3>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(IndexerPropertySetter<object?, TValue, TIndex1, TIndex2, TIndex3>).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStatic3DIndexerSetter<TValue, TIndex1, TIndex2, TIndex3>(this));
        var invoker = (IndexerPropertySetter<object?, TValue, TIndex1, TIndex2, TIndex3>)cachedInvoker;
        return invoker;
    }

    private void GetAccessors()
    {
        (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = PropertyData.GetPropertyAccessModifier(PropertyGetMethodData, PropertySetMethodData);
        _propertyAccessModifier = propertyModifier;
        _setAccessorAccessModifier = setMethodModifier;
        _getAccessorAccessModifier = getMethodModifier;
    }

    internal bool IsIndexer => CanRead
        ? PropertyGetMethodParameters.HasItems
        : CanWrite && PropertySetMethodParameters.Count > 1;

    /// <summary>
    /// Gets the parameters of the property getter method. For non-indexer properties, this will be empty.
    /// </summary>
    internal ParameterList PropertyGetMethodParameters => _getMethodParameters ??= PropertyGetMethodData.Parameters;

    /// <summary>
    /// Gets the parameters of the property setter method (indexer parameters plus the implicit <c>value</c> parameter).
    /// </summary>
    internal ParameterList PropertySetMethodParameters => _setMethodParameters ??= PropertySetMethodData.Parameters;

    internal override AccessModifier AccessModifier
    {
        get
        {
            if (_propertyAccessModifier is AccessModifier.Undefined)
            {
                GetAccessors();
            }

            return _propertyAccessModifier;
        }
    }

    internal AccessModifier SetAccessorAccessModifier
    {
        get
        {
            if (_setAccessorAccessModifier is AccessModifier.Undefined)
            {
                GetAccessors();
            }

            return _setAccessorAccessModifier;
        }
    }

    internal AccessModifier GetAccessorAccessModifier
    {
        get
        {
            if (_getAccessorAccessModifier is AccessModifier.Undefined)
            {
                GetAccessors();
            }

            return _getAccessorAccessModifier;
        }
    }

    internal TypeData PropertyTypeData => _propertyTypeData ??= GetOrCreateCacheEntry(PropertyInfo.PropertyType);

    internal PropertyInfo PropertyInfo { get; }

    //internal static PropertyData TaskResultPropertyData
    //  => PropertyData._TaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateMethodDataCacheEntry(typeof(Task<>).GetProperty(nameof(Task<object>.Result)));

    //internal static PropertyData ValueTaskResultPropertyData
    //  => PropertyData._ValueTaskResultPropertyData ??= SymbolReflectionInfoCache.GetOrCreateMethodDataCacheEntry(typeof(ValueTask<>).GetProperty(nameof(ValueTask<object>.Result)));

    internal bool IsSealed => _isSealed ??= (CanRead && PropertyGetMethodData!.IsSealed)
        || (CanWrite && PropertySetMethodData!.IsSealed);

    internal bool CanWrite => _canWrite ??= PropertyInfo.CanWrite;

    internal bool CanRead => _canRead ??= PropertyInfo.CanRead;

    /// <summary>
    /// Gets a value indicating whether the current property has a setter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="IsReadOnly"/> is an alias for the inverted <see cref="CanWrite"/>:
    /// <c>IsReadOnly == !CanWrite</c>.
    /// </para>
    /// <para>
    /// Note: <see cref="CanWrite"/> is <see langword="true"/> if a set accessor exists (even if non-public).
    /// Init-only properties still have a setter, so they are not considered read-only by this flag.
    /// </para>
    /// </remarks>
    /// <value>
    /// <see langword="true"/> if the property does not have a set accessor; otherwise, <see langword="false"/>.
    /// </value>
    internal bool IsReadOnly => !CanWrite;

    /// <summary>
    /// Returns whether the property is an init-only property.
    /// </summary>
    /// <remarks>This is determined by checking if the property has a setter method and if that setter method is marked with the <see cref="IsExternalInit"/> modifier, 
    /// which is used by the C# compiler to indicate init-only properties. 
    /// If the property does not have a setter or if the setter is not marked as init-only, this property returns <see langword="false"/>.</remarks>
    /// <value><see langword="true"/> if the property is an init-only property; otherwise, <see langword="false"/>.</value>
    internal bool IsInit => _isInit ??= CanWrite && PropertyData.IsPropertyInit(this);

    internal MethodData PropertyGetMethodData => _getMethodData ??= PropertyInfo is PropertyInfo propertyInfo && propertyInfo.CanRead
        ? propertyInfo.GetGetMethod(true) is MethodInfo propertyGetter
            ? GetOrCreateCacheEntry(propertyGetter)
            : throw new NotSupportedException($"The underlying '{typeof(PropertyInfo).FullName}' for property '{PropertyInfo.Name}' does not have a get method.")
        : throw new NotSupportedException($"The underlying '{typeof(PropertyInfo).FullName}' for property '{PropertyInfo.Name}' does not have a get method. Check '{nameof(PropertyData)}.{nameof(PropertyData.CanRead)}' before access.");

    internal MethodData PropertySetMethodData => _setMethodData ??= PropertyInfo is PropertyInfo propertyInfo && propertyInfo.CanWrite
        ? propertyInfo.GetSetMethod(true) is MethodInfo propertySetter
            ? GetOrCreateCacheEntry(propertySetter)
            : throw new NotSupportedException($"The underlying '{typeof(PropertyInfo).FullName}' for property '{PropertyInfo.Name}' does not have a set method.")
        : throw new NotSupportedException($"The underlying '{typeof(PropertyInfo).FullName}' for property '{PropertyInfo.Name}' does not have a set method. Check '{nameof(PropertyData)}.{nameof(PropertyData.CanWrite)}' before access.");

    internal override SymbolAttributes SymbolAttributes => _symbolAttributes is SymbolAttributes.Undefined
        ? (_symbolAttributes = PropertyData.GetAttributesInternal(this))
        : _symbolAttributes;

    internal override SymbolComponentInfo SymbolComponentInfo => _symbolComponentInfo ??= SymbolSignatureGenerator.ToSignatureComponentsInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false);

    internal override string Signature => _signature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortSignature => _shortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: false);

    internal override string ShortCompactSignature => _shortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: false);

    internal override string FullyQualifiedSignature => _fullyQualifiedSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: false);

    internal override string FullyQualifiedRuntimeSignature => _fullyQualifiedRuntimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: true, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeSignature => _runtimeSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: true, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortSignature => _runtimeShortSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: false, isRuntimeSymbol: true);

    internal override string RuntimeShortCompactSignature => _runtimeShortCompactSignature ??= SymbolSignatureGenerator.ToSignatureNameInternal(this, isFullyQualifiedName: false, isDeclaringTypeIncluded: false, isCompact: true, isRuntimeSymbol: true);

    internal override string DisplayName => _displayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string ShortDisplayName => _shortDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: false, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: false);

    internal override string FullyQualifiedDisplayName => _fullyQualifiedDisplayName ??= SymbolSignatureGenerator.ToDisplayNameInternal(this, isFullyQualifiedName: true, isGenericTypeParameterIncluded: true, isDeclaringTypeIncluded: true);

    internal override string AssemblyName => _assemblyName ??= DeclaringTypeData.AssemblyName;

    internal override bool IsStatic => _isStatic ??= (CanRead && PropertyGetMethodData!.IsStatic)
        || (CanWrite && PropertySetMethodData!.IsStatic);

    internal bool IsOverride => _isOverride ??= (CanRead && PropertyGetMethodData!.IsOverride)
        || (CanWrite && PropertySetMethodData!.IsOverride);

    internal override bool IsPublic => _isPublic ??= AccessModifier == AccessModifier.Public;

    internal override bool IsPrivate => _isPrivate ??= AccessModifier == AccessModifier.Private;

    /// <summary>
    /// Gets a value indicating whether the member has internal accessibility within its assembly.
    /// </summary>
    internal override bool IsAssembly => _isAssembly ??= AccessModifier == AccessModifier.Internal;

    /// <summary>
    /// Gets a value indicating whether the member is protected and thus accessible only within its own class or by
    /// derived class instances.
    /// </summary>
    internal override bool IsFamily => _isFamily ??= AccessModifier == AccessModifier.Protected;

    internal override bool IsFamilyOrAssembly => _isFamilyOrAssembly ??= AccessModifier == AccessModifier.ProtectedInternal;

    internal override bool IsFamilyAndAssembly => _isFamilyAndAssembly ??= AccessModifier == AccessModifier.PrivateProtected;

    /// <inheritdoc/>
    internal override bool IsExplicitInterfaceImplementation => _isExplicitInterfaceImplementation ??= IsExplicitImplementation(this);

    /// <inheritdoc/>
    internal override RuntimeTypeHandle DeclaringTypeHandle => _declaringTypeHandle ??= CanWrite
        ? PropertySetMethodData.DeclaringTypeHandle
        : PropertyGetMethodData.DeclaringTypeHandle;

    /// <inheritdoc/>
    internal override RuntimeTypeHandle ImplementingTypeHandle => _implementingTypeHandle ??= CanWrite
        ? PropertySetMethodData.ImplementingTypeHandle
        : PropertyGetMethodData.ImplementingTypeHandle;

    private static bool IsExplicitImplementation(PropertyData propertyData) => propertyData.CanWrite
        ? propertyData.PropertySetMethodData.IsExplicitInterfaceImplementation
        : propertyData.PropertyGetMethodData.IsExplicitInterfaceImplementation;

    /// <summary>
    /// Determines the set of symbol attributes for the specified property based on its metadata and accessor
    /// methods.
    /// </summary>
    /// <remarks>For performance reasons avoid querying the attributes and prefer reading the particular property or properties.</remarks>
    /// <param name="propertyData">The metadata describing the property for which to retrieve symbol attributes. Cannot be null.</param>
    /// <returns>A bitwise combination of SymbolAttributes values that represent the characteristics of the property, such as
    /// whether it is static, abstract, virtual, an indexer, or has other modifiers.</returns>
    private static SymbolAttributes GetAttributesInternal(PropertyData propertyData)
    {
        SymbolAttributes propertyAttributes = propertyData.IsIndexer
          ? SymbolAttributes.IndexerProperty
          : SymbolAttributes.Property;

        MethodData? accessorData = propertyData.PropertyGetMethodData ?? propertyData.PropertySetMethodData;
        if (accessorData is null)
        {
            return SymbolAttributes.Undefined;
        }

        if (!propertyData.CanWrite)
        {
            propertyAttributes |= SymbolAttributes.Final;
        }

        if (propertyData.IsInit)
        {
            propertyAttributes |= SymbolAttributes.Init;
        }

        if (accessorData.IsAbstract)
        {
            propertyAttributes |= SymbolAttributes.Abstract;
        }

        if (propertyData.IsStatic)
        {
            propertyAttributes |= SymbolAttributes.Static;
        }

        if (accessorData.IsVirtual)
        {
            propertyAttributes |= SymbolAttributes.Virtual;
        }

        if (propertyData.IsOverride)
        {
            propertyAttributes |= SymbolAttributes.Override;
        }

        return propertyAttributes;
    }

    private static bool IsPropertyInit(PropertyData propertyData) => propertyData.CanWrite
        && propertyData.PropertySetMethodData.ReturnParameterData.HasRequiredCustomModifier(ReflectionConstants.IsExternalInitFullName);

    private static (AccessModifier PropertyModifier, AccessModifier GetMethodModifier, AccessModifier SetMethodModifier) GetPropertyAccessModifier(MethodData? getMethodData, MethodData? setMethodData)
    {
        AccessModifier getMethodModifier = getMethodData?.AccessModifier ?? AccessModifier.Undefined;
        AccessModifier setMethodModifier = setMethodData?.AccessModifier ?? AccessModifier.Undefined;

        // Property accessors with the least restriction provides the access modifier for the property.
        AccessModifier propertyAccessModifier = (getMethodModifier, setMethodModifier) switch
        {
            (AccessModifier.Undefined, AccessModifier.Undefined) => AccessModifier.Undefined,
            (AccessModifier.Undefined, _) => setMethodModifier,
            (_, AccessModifier.Undefined) => getMethodModifier,
            _ => (AccessModifier)System.Math.Min((int)getMethodModifier, (int)setMethodModifier)
        };

        return (propertyAccessModifier, getMethodModifier, setMethodModifier);
    }
}
