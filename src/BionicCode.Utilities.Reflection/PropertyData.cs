//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.Profiling")]
namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

internal sealed class PropertyData : MemberData, IPropertyDataInvoker
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
    private Func<object?, object[], object?>? _indexerPropertyGetInvoker;
    private Action<object?, object[], object?>? _indexerPropertySetInvoker;
    private Func<object?, object?>? _propertyGetInvoker;
    private Action<object?, object?>? _propertySetInvoker;
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
    internal object? GetValue(object? target)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use one of the overloads that accepts indexer parameters.");
        }

        if (!IsStatic)
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
        }

        object? invocationTarget = IsStatic
            ? null
            : target;

        Func<object?, object?> propertySetInvoker = GetPropertyGetterInternal();
        return propertySetInvoker.Invoke(target);
    }

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

        if (!IsStatic)
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
        }

        object? invocationTarget = IsStatic
            ? null
            : target;

        PropertyGetter<TTarget, TValue> propertySetInvoker = GetPropertyGetterInternal<TTarget, TValue>();
        return propertySetInvoker.Invoke(target);
    }

    /// <summary>
    /// Gets the value of the indexer property for the specified target object and index parameters.
    /// </summary>
    /// <remarks>For instance indexer properties, the target object must be assignable to the
    /// declaring type of the property. The length and types of the indexerPropertyParameters array must match the
    /// indexer parameters defined by the property.</remarks>
    /// <param name="target">The object instance on which to access the indexer property. Must be non-null for instance properties;
    /// ignored for static properties.</param>
    /// <param name="indexerPropertyParameters">An array of values representing the index parameters required by the indexer property. The number and types
    /// of elements must match the indexer definition. Cannot be null.</param>
    /// <returns>The value returned by the indexer property for the specified target and index parameters.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property does not have a getter or is not an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target is null for an instance property, or if indexerPropertyParameters is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the number of elements in indexerPropertyParameters does not match the indexer parameter count of the property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal object? GetIndexerValue(object? target, object?[] indexerPropertyParameters)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use one of the overloads without indexer parameters instead.");
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyParameters, nameof(indexerPropertyParameters), "Indexer property index cannot be null for indexer properties.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
            indexerPropertyParameters.Length,
            PropertyGetMethodParameters.Count,
            nameof(indexerPropertyParameters),
            $"Provided number of indexer parameters does not match the indexer parameter count of property '{FullyQualifiedSignature}'. Expected: {PropertyGetMethodParameters.Count}; Found: {indexerPropertyParameters.Length}.");

        if (!IsStatic)
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
        }

        object? invocationTarget = IsStatic
            ? null
            : target;
        Func<object?, object[], object?> propertyGetInvoker = GetIndexerGetterInternal();
        return propertyGetInvoker.Invoke(invocationTarget, indexerPropertyParameters!);
    }

    /// <summary>
    /// Gets the value of the indexer property represented by this instance for the specified target object and
    /// indexer parameter.
    /// </summary>
    /// <remarks>Use this method for 1D indexer properties that accept a single index parameter.</remarks>
    /// <typeparam name="TTarget">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the property getter.</typeparam>
    /// <typeparam name="TIndex">The type of the indexer parameter.</typeparam>
    /// <param name="target">The target instance the property is invoked on.</param>
    /// <param name="indexerPropertyParameters"></param>
    /// <returns>The value of the indexer property of type <typeparamref name="TValue"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the property is not readable or if the property is not an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the indexer parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the indexer parameter count does not match the indexer parameter count of the property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal TValue GetIndexerValue<TTarget, TValue, TIndex>(TTarget target, params TIndex[] indexerPropertyParameters)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use one of the overloads without indexer parameters instead.");
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyParameters, nameof(indexerPropertyParameters), $"Indexer index parameter {nameof(indexerPropertyParameters)} cannot be null for indexer properties.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
            indexerPropertyParameters.Length,
            PropertyGetMethodParameters.Count,
            nameof(indexerPropertyParameters),
            $"Provided number of indexer parameters does not match the indexer parameter count of this property '{FullyQualifiedSignature}'. Expected: {PropertyGetMethodParameters.Count} index parameters; Found: {indexerPropertyParameters.Length}.");

        if (!IsStatic)
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
        }

        TTarget? invocationTarget = IsStatic
            ? default
            : target;
        IndexerPropertyGetter<TTarget, TValue, TIndex> propertyGetInvoker = GetIndexerGetterInternal<TTarget, TValue, TIndex>();
        return propertyGetInvoker.Invoke(invocationTarget, indexerPropertyParameters!);
    }

    /// <summary>
    /// Gets the value of the indexer property represented by this instance for the specified target object and
    /// indexer parameter.
    /// </summary>
    /// <remarks>Use this method for 1D indexer properties that accept a single index parameter.</remarks>
    /// <typeparam name="TTarget">The type of the object that declares the property.</typeparam>
    /// <typeparam name="TValue">The type of the value returned by the property getter.</typeparam>
    /// <param name="target">The target instance the property is invoked on.</param>
    /// <param name="indexerPropertyParameters"></param>
    /// <returns>The value of the indexer property of type <typeparamref name="TValue"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the property is not readable or if the property is not an indexer property.</exception>
    /// <exception cref="ArgumentNullException">Thrown when the indexer parameter is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the indexer parameter count does not match the indexer parameter count of the property.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="target"/> instance is not of the correct type.</exception>
    internal TValue GetIndexerValue<TTarget, TValue>(TTarget target, params object[] indexerPropertyParameters)
    {
        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use one of the overloads without indexer parameters instead.");
        }

        ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyParameters, nameof(indexerPropertyParameters), $"Indexer index parameter {nameof(indexerPropertyParameters)} cannot be null for indexer properties.");
        ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
            indexerPropertyParameters.Length,
            PropertyGetMethodParameters.Count,
            nameof(indexerPropertyParameters),
            $"Provided number of indexer parameters does not match the indexer parameter count of this property '{FullyQualifiedSignature}'. Expected: {PropertyGetMethodParameters.Count} index parameters; Found: {indexerPropertyParameters.Length}.");

        if (!IsStatic)
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
        }

        TTarget? invocationTarget = IsStatic
            ? default
            : target;
        IndexerPropertyGetter<TTarget, TValue> propertyGetInvoker = GetIndexerGetterInternal<TTarget, TValue>();
        return propertyGetInvoker.Invoke(invocationTarget, indexerPropertyParameters!);
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
    internal void SetValue(object? target, object? value)
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

        if (!IsStatic)
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
        }

        object? invocationTarget = IsStatic
            ? null
            : target;
        Action<object?, object?> propertySetInvoker = GetSetInvokerInternal();
        propertySetInvoker(invocationTarget, value);
    }

    internal void SetValue<TTarget, TValue>(TTarget target, TValue value) where TTarget : class
    {
        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetStructValue)));
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(SetStructValue)));
        }

        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use one of the overloads that accepts indexer parameters.");
        }

        if (!IsStatic)
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
        }

        TTarget? invocationTarget = IsStatic
            ? null
            : target;
        PropertySetter<TTarget, TValue> propertySetInvoker = GetSetInvokerInternal<TTarget, TValue>();
        propertySetInvoker(invocationTarget, value);
    }

    internal void SetStructValue<TTarget, TValue>(ref TTarget target, TValue value, object[]? indexerPropertyIndex = null) where TTarget : struct
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(SetValue)));
        }

        if (IsStatic)
        {
            throw new InvalidOperationException($"Setting static property values on struct types is not supported by this method. Call '{nameof(SetValue)}' instead.");
        }

        if (IsIndexer)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyIndex, nameof(indexerPropertyIndex), "Indexer property index cannot be null for indexer properties.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                indexerPropertyIndex!.Length,
                PropertySetMethodParameters.Count,
                nameof(indexerPropertyIndex),
                $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");
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
    /// <param name="indexerPropertyIndex">An array of index values to use if the property is an indexer. The number of elements must match the number
    /// of indexer parameters. This parameter is required for indexer properties and ignored for non-indexer
    /// properties.</param>
    /// <exception cref="InvalidOperationException">Thrown if the property is read-only or if the declaring type is a value type.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the target object is null for an instance property, or if indexerPropertyParameters is null for an
    /// indexer property.</exception>
    internal void SetIndexerValue(object? target, object? value, object?[]? indexerPropertyIndex = null)
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (IsIndexer)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(indexerPropertyIndex, nameof(indexerPropertyIndex), "Indexer property index cannot be null for indexer properties.");
            ArgumentOutOfRangeExceptionAdvanced.ThrowIfLessThan(
                indexerPropertyIndex!.Length,
                PropertySetMethodParameters.Count,
                nameof(indexerPropertyIndex),
                $"Indexer property index count does not match the indexer parameter count of property '{FullyQualifiedSignature}'.");
        }

        if (!IsStatic)
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
        }

        object? invocationTarget = IsStatic
            ? null
            : target;
        if (IsIndexer)
        {
            Action<object?, object[], object?> propertySetInvoker = GetIndexerSetInvokerInternal();
            propertySetInvoker(invocationTarget, indexerPropertyIndex!, value);
        }
        else
        {
            Action<object?, object?> propertySetInvoker = GetSetInvokerInternal();
            propertySetInvoker(invocationTarget, value);
        }
    }

    /// <summary>
    /// Gets a delegate that sets the value of the indexer property represented by this instance.
    /// </summary>
    /// <remarks>Use this method only for properties that are indexers. For non-indexer properties,
    /// use <see cref="GetSetInvoker"/> instead.</remarks>
    /// <returns>An <code>Action&lt;object?, object?[], object?&gt;</code> delegate that sets the value of the indexer property. The
    /// first parameter is the target object, the second parameter is an array of index values, and the third
    /// parameter is the value to set.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property represented by this instance is not an indexer property.</exception>
    internal Action<object?, object[], object?> GetIndexerSetInvoker()
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{Signature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{Signature}' does not have a setter.");
        }

        return GetIndexerSetInvokerInternal();
    }

    private Action<object?, object[], object?> GetIndexerSetInvokerInternal()
        => _indexerPropertySetInvoker ??= DelegateProvider.CreateIndexerSetter(this);

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
    internal ValueTypeIndexerPropertySetter<TTarget, TValue> GetStructIndexerSetInvoker<TTarget, TValue>() where TTarget : struct
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{Signature}' is not an indexer property. Use {nameof(GetStructSetInvoker)} instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{Signature}' does not have a setter.");
        }

        if (!DeclaringTypeData.IsValueType)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsValueTypeWrongInvokerExceptionMessage(this, nameof(GetIndexerSetInvoker)));
        }

        return GetStructIndexerSetInvokerInternal<TTarget, TValue>();
    }

    private ValueTypeIndexerPropertySetter<TTarget, TValue> GetStructIndexerSetInvokerInternal<TTarget, TValue>() where TTarget : struct
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, targetTypeHandle => DelegateProvider.CreateStructIndexerSetter<TTarget, TValue>(this));
        var invoker = (ValueTypeIndexerPropertySetter<TTarget, TValue>)cachedInvoker;

        return invoker;
    }

    /// <summary>
    /// Gets a delegate that sets the value of this property on a struct of the specified type.
    /// </summary>
    /// <typeparam name="TTarget">The struct type that declares the property.</typeparam>
    /// <typeparam name="TValue">The type of the value to set on the property.</typeparam>
    /// <returns>A delegate that sets the value of the property on a struct of type TTarget.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is an indexer or is not declared on a struct type.</exception>
    internal ValueTypeMemberSetter<TTarget, TValue> GetStructSetInvoker<TTarget, TValue>() where TTarget : struct
    {
        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use {nameof(GetIndexerSetInvoker)} instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        if (!DeclaringTypeData.IsStruct)
        {
            throw new InvalidOperationException(ExceptionMessages.GetDeclaringTypeOfMemberIsReferenceTypeWrongInvokerExceptionMessage(this, nameof(GetSetInvoker)));
        }

        return GetStructSetInvokerInternal<TTarget, TValue>();
    }

    private ValueTypeMemberSetter<TTarget, TValue> GetStructSetInvokerInternal<TTarget, TValue>() where TTarget : struct
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate? cachedInvoker = _invokerTable.GetOrAdd(targetTypeHandle, _ => DelegateProvider.CreateStructSetter<TTarget, TValue>(this));
        var invoker = (ValueTypeMemberSetter<TTarget, TValue>)cachedInvoker;

        return invoker;
    }

    /// <summary>
    /// Creates a delegate that sets the value of the property represented by this instance.
    /// </summary>
    /// <remarks>The returned delegate can be used to set the value of the property on a specified
    /// object instance. This method does not support indexer properties.</remarks>
    /// <returns>An <see cref="Action{Object, Object}"/> delegate that sets the property value. The first parameter is the
    /// target object; the second parameter is the value to assign to the property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is an indexer. Use <see cref="GetIndexerSetInvoker"/> to obtain a setter for indexer
    /// properties.</exception>
    internal Action<object?, object?> GetSetInvoker()
    {
        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use '{nameof(GetIndexerSetInvoker)}' instead.");
        }

        if (IsReadOnly)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a setter.");
        }

        return GetSetInvokerInternal();
    }

    /// <summary>
    /// Creates a delegate that retrieves the value of the property represented by this instance.
    /// </summary>
    /// <remarks>The returned delegate can be used to efficiently access the property value without
    /// using reflection for each invocation. The instance parameter must be of the declaring type or a compatible
    /// type; otherwise, an exception may be thrown at invocation time.</remarks>
    /// <returns>A delegate that takes an object instance and returns the value of the property. The delegate returns null if
    /// the property value is null.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is an indexer. Use GetIndexerGetInvoker instead for indexer properties.</exception>
    internal Func<object?, object?> GetGetInvoker()
    {
        if (IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is an indexer property. Use '{nameof(GetIndexerGetInvoker)}' instead.");
        }

        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        return GetPropertyGetterInternal();
    }

    /// <summary>
    /// Retrieves a delegate that gets the value of the indexer property represented by this instance.
    /// </summary>
    /// <remarks>Use this method to obtain a strongly-typed accessor for indexer properties. For
    /// non-indexer properties, use GetGetInvoker instead.<para/>
    /// If types are known at compile time use a strictly typed generic overload to significantly improve performance.</remarks>
    /// <returns>A delegate that takes a target object and an array of index values, and returns the value of the indexer
    /// property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property represented by this instance is not an indexer property or does not have a getter.</exception>
    internal Func<object?, object[], object?> GetIndexerGetInvoker()
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
        }

        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        return GetIndexerGetterInternal();
    }

    internal IndexerPropertyGetter<TTarget, TIndex, TValue> GetIndexerGetInvoker<TTarget, TIndex, TValue>()
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
        }

        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            1,
            PropertyGetMethodParameters.Count,
            nameof(TIndex),
            $"Indexer property parameter count mismatch. Provided 1 indexer parameter for an indexer that requires {PropertyGetMethodParameters.Count} parameters. Please use the appropriate overload that matches the number of indexer parameters.");

        return GetIndexerGetterInternal<TTarget, TIndex, TValue>();
    }

    internal IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TValue> GetIndexerGetInvoker<TTarget, TIndex1, TIndex2, TValue>()
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
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

        return GetIndexerGetterInternal<TTarget, TIndex1, TIndex2, TValue>();
    }

    internal IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue> GetIndexerGetInvoker<TTarget, TIndex1, TIndex2, TIndex3, TValue>()
    {
        if (!IsIndexer)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' is not an indexer property. Use {nameof(GetGetInvoker)} instead.");
        }

        if (!CanRead)
        {
            throw new InvalidOperationException($"The property '{FullyQualifiedSignature}' does not have a getter.");
        }

        ArgumentOutOfRangeExceptionAdvanced.ThrowIfNotEqual(
            3,
            PropertyGetMethodParameters.Count,
            nameof(TIndex2),
            $"Indexer property parameter count mismatch. Provided 3 indexer parameters for an indexer that requires {PropertyGetMethodParameters.Count} parameters. Please use the appropriate overload that matches the number of indexer parameters.");

        return GetIndexerGetterInternal<TTarget, TIndex1, TIndex2, TIndex3, TValue>();
    }

    private Func<object?, object?> GetPropertyGetterInternal() => _propertyGetInvoker ??= DelegateProvider.CreateGetter(this);

    private PropertyGetter<TTarget, TValue> GetPropertyGetterInternal<TTarget, TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateGetter<TTarget, TValue>(this));

        return (PropertyGetter<TTarget, TValue>)invoker;
    }

    private Func<object?, object[], object?> GetIndexerGetterInternal() => _indexerPropertyGetInvoker ??= DelegateProvider.CreateIndexerGetter(this);

    private IndexerPropertyGetter<TTarget, TIndex, TValue> GetIndexerGetterInternal<TTarget, TIndex, TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateIndexerGetter<TTarget, TIndex, TValue>(this));

        return (IndexerPropertyGetter<TTarget, TIndex, TValue>)invoker;
    }

    private IndexerPropertyGetter<TTarget, TValue> GetIndexerGetterInternal<TTarget, TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateIndexerGetter<TTarget, TValue>(this));

        return (IndexerPropertyGetter<TTarget, TValue>)invoker;
    }

    private IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TValue> GetIndexerGetterInternal<TTarget, TIndex1, TIndex2, TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateIndexerGetter<TTarget, TIndex1, TIndex2, TValue>(this));

        return (IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TValue>)invoker;
    }

    private IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue> GetIndexerGetterInternal<TTarget, TIndex1, TIndex2, TIndex3, TValue>()
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateIndexerGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue>(this));

        return (IndexerPropertyGetter<TTarget, TIndex1, TIndex2, TIndex3, TValue>)invoker;
    }

    private Action<object?, object?> GetSetInvokerInternal() => _propertySetInvoker ??= DelegateProvider.CreateSetter(this);

    private PropertySetter<TTarget, TValue> GetSetInvokerInternal<TTarget, TValue>() where TTarget : class
    {
        RuntimeTypeHandle targetTypeHandle = typeof(TTarget).TypeHandle;
        Delegate invoker = _invokerTable.GetOrAdd(
            targetTypeHandle,
            _ => DelegateProvider.CreateSetter<TTarget, TValue>(this));

        return (PropertySetter<TTarget, TValue>)invoker;
    }

    private void GetAccessors()
    {
        (AccessModifier propertyModifier, AccessModifier getMethodModifier, AccessModifier setMethodModifier) = PropertyData.GetPropertyAccessModifier(PropertyGetMethodData, PropertySetMethodData);
        _propertyAccessModifier = propertyModifier;
        _setAccessorAccessModifier = setMethodModifier;
        _getAccessorAccessModifier = getMethodModifier;
    }

    internal new IPropertyDataView View => _propertyDataView
        ??= new PropertyDataView(GetPublicCacheKey());

    internal bool IsIndexer => CanRead
        ? PropertyGetMethodParameters.HasItems
        : CanWrite && PropertySetMethodParameters.Count > 1;

    /// <summary>
    /// Gets the parameters of the property getter method (including indexer parameters only).
    /// </summary>
    internal ParameterList PropertyGetMethodParameters => _getMethodParameters ??= ParameterListBuilder.CreateForPropertyGet(this);

    /// <summary>
    /// Gets the parameters of the property setter method (indexer parameters plus the implicit <c>value</c> parameter).
    /// </summary>
    internal ParameterList PropertySetMethodParameters => _setMethodParameters ??= ParameterListBuilder.CreateForPropertySet(this);

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

    #region IPropertyDataInvoker

    bool IPropertyDataInvoker.IsInvocable => (CanRead && _propertyGetInvoker is not null)
        || (CanWrite && _propertySetInvoker is not null)
        || (IsIndexer && ((CanRead && _indexerPropertyGetInvoker is not null)
        || (CanWrite && _indexerPropertySetInvoker is not null)));

    bool IPropertyDataInvoker.HasGetter => _indexerPropertyGetInvoker is not null || _propertyGetInvoker is not null;

    bool IPropertyDataInvoker.HasSetter => _indexerPropertySetInvoker is not null || _propertySetInvoker is not null;

    bool IPropertyDataInvoker.IsIndexer => IsIndexer;

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

    #endregion IPropertyDataInvoker

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
