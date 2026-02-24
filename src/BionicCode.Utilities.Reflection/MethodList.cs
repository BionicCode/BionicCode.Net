namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
internal sealed class MethodList : IReadOnlyList<MethodData>, ICollection, IEmptyCollectionProvider<MethodList>, IEquatable<MethodList>
{
    public static MethodList Empty { get; } = new MethodList();
    private readonly int _hashCode; // precomputed
    private readonly ILookup<string, MethodData> _methodNameIndex;
    private readonly TypeData? _declaringTypeData;
    private IMethodListView? _view;

    public MethodList(MethodData[] items, TypeData? declaringType) : this((IEnumerable<MethodData>)items, declaringType)
    {
    }

    public MethodList(IEnumerable<MethodData> items, TypeData? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringTypeData = declaringType;

        Methods = items?.ToImmutableList() ?? ImmutableList<MethodData>.Empty;
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal); // allow duplicate method names (overloads)

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Methods,
                methodData => !ReferenceEquals(methodData.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal MethodList(IEnumerable<MethodData> items, TypeData? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringTypeData = declaringType;

        Methods = items?.ToImmutableList() ?? ImmutableList<MethodData>.Empty;

        // allow duplicate method names (overloads)
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Methods,
                methodData => !ReferenceEquals(methodData.DeclaringTypeData, _declaringTypeData),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private MethodList()
    {
        Methods = ImmutableList<MethodData>.Empty;
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal);
        _declaringTypeData = default;
        _hashCode = ComputeHashCode();
    }

    public bool TryGetMethodsByName(string methodName, out MethodList methodList)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName);
        methodList = _methodNameIndex[methodName].ToMethodList(DeclaringTypeData);

        return methodList.HasItems;
    }

    public bool ContainsMethodWithName(string methodName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName);
        return _methodNameIndex.Contains(methodName);
    }

    public int Count => Methods.Count;
    public bool IsEmpty => Methods.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<MethodData> Methods { get; }
    public IMethodListView View => _view ??= Methods.ToMethodListView(DeclaringTypeData);
    public TypeData DeclaringTypeData => _declaringTypeData ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeData)));

    // Immutable collections are inherently thread-safe for read operations,
    // so we can consider this collection as synchronized for enumeration and access.
    public bool IsSynchronized { get; } = true;

    object ICollection.SyncRoot => this;

    public MethodData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Methods.Count, nameof(index));

            return HasItems
                ? Methods[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    /// <summary>
    /// Gets the method data for the method with the specified name and parameter signature.
    /// </summary>
    /// <remarks>Use this indexer to retrieve method metadata when you know both the method name and
    /// the exact parameter signature. Parameter matching considers type, position, and modifier (such as ref or
    /// out).</remarks>
    /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
    /// <param name="methodParameters">An array of parameter information objects that describe the expected parameter types, positions, and
    /// modifiers for the method signature.</param>
    /// <returns>The method data that matches the specified name and parameter signature.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method index has not been initialized.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no method with the specified name exists, or if no method with the specified name matches the
    /// provided parameter signature.</exception>
    public MethodData this[string? methodName, ParameterDescriptorList? methodParameters]
    {
        get
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), ReflectionConstants.IndexerGetMethodName));
            }

            if (string.IsNullOrWhiteSpace(methodName) && methodParameters is null)
            {
                throw new ArgumentNullException(nameof(methodName), $"Provide at least one valid argument. Argument '{nameof(methodName)}' cannot be null or empty and '{nameof(methodParameters)}' cannot be null.");
            }

            if (_methodNameIndex == null)
            {
                throw new InvalidOperationException("Method index is not initialized.");
            }

            methodParameters = methodParameters.OrEmpty();
            ImmutableList<MethodData> methods = Methods;
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                methods = _methodNameIndex[methodName].ToImmutableList();
                if (methods.IsEmpty)
                {
                    throw new KeyNotFoundException($"Invalid key.No method named '{methodName}' could be found.");
                }

                if (methods.Count == 1 && methodParameters.Count == 0)
                {
                    return methods[0];
                }

                if (methods.Count > 1 && methodParameters.Count == 0)
                {
                    throw new AmbiguousMatchException($"Ambiguous match found for method '{methodName}'. Try to provide the parameter types via the '{nameof(methodParameters)}' argument to disambiguate.");
                }
            }

            var results = new List<MethodData>(methods.Count);
            foreach (MethodData method in methods)
            {
                ParameterList parameters = method.Parameters;

                if (methodParameters.DeclaringMethodParameterCount != ParameterDescriptor.UnknownParameterCountOrPosition
                    && parameters.Count != methodParameters.DeclaringMethodParameterCount)
                {
                    continue;
                }

                // 'methodParameters' could be an incomplete parameter descriptor list that only specifies a subset of the parameters
                // of the method signature (e.g. only the first two parameters of a method with 4 parameters).
                // In this case, we only compare the specified subset of parameters and ignore the rest for matching purposes.
                for (int parameterIndex = 0; parameterIndex < methodParameters.Count; parameterIndex++)
                {
                    ParameterDescriptor parameterDescriptor = methodParameters[parameterIndex];

                    if (parameterDescriptor.HasParameterPosition
                        && parameterDescriptor.ParameterPosition > parameters.Count)
                    {
                        break;
                    }

                    // If ALL parameter descriptors have an explicitly specified parameter position (in this case 'methodParameters.IsSortedByParameterPosition' is true),
                    // we use it to retrieve the corresponding parameter from the method signature for comparison to improve speed.
                    // Otherwise, we must iterate through all parameters of the method signature to find the corresponding parameter for comparison, which is slower.
                    ParameterData parameter = methodParameters.IsSortedByParameterPosition
                        ? parameters[parameterDescriptor.ParameterPosition]
                        : parameters[parameterIndex];

                    // Parameter is valid if it matches position in method signature, parameter type and modifier.
                    // Since parameter collections are always ordered by parameter position in ascending order, we don't have to check the order explicitly (only count - see above).

                    // TODO:: Split algorithm into two separate algorithms:
                    // - one for the case when ALL parameter descriptors have an explicitly specified parameter position
                    // - and one for the case when parameter position information is missing for at least one parameter descriptor
                    // to improve readability and maintainability.

                    // When the source is unsorted we still must anticipate the possibility of parameter position information being
                    // randomly provided in the descriptor to find the corresponding parameter for comparison.
                    // If the position information is provided but does not match the current parameter index, we skip this parameter
                    // and continue searching for the corresponding parameter in the method signature.
                    if (!methodParameters.IsSortedByParameterPosition
                        && parameterDescriptor.HasParameterPosition
                        && parameterDescriptor.ParameterPosition != parameterIndex)
                    {
                        continue;
                    }

                    if (parameterDescriptor.HasParameterTypeHandle
                        && !parameter.ParameterTypeData.Handle.Equals(parameterDescriptor.ParameterTypeHandle))
                    {
                        break;
                    }

                    if (parameterDescriptor.HasParameterModifier
                        && !parameter.ParameterModifier.Equals(parameterDescriptor.ParameterModifier))
                    {
                        break;
                    }
                }

                results.Add(method);
            }

            if (results.Count > 1)
            {
                throw new AmbiguousMatchException($"Ambiguous match found based on the provided information. Try to provide the parameter names and the parameter types via the '{nameof(methodName)}' and '{nameof(methodParameters)}' arguments to disambiguate.");
            }

            if (results.IsEmpty())
            {
                throw new KeyNotFoundException($"Invalid arguments. No method could be found based on the provided information. Either the method doe not exist or the provided information is insufficient.");
            }

            return results[0];
        }
    }

    public IEnumerator<MethodData> GetEnumerator() => ((IEnumerable<MethodData>)Methods).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Methods.GetEnumerator();

    public bool Equals(MethodList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringTypeData, other.DeclaringTypeData))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Methods[index], other.Methods[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is MethodList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeData);
            for (int index = 0; index < Methods.Count; index++)
            {
                hashCode.Add(Methods[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public void CopyTo(Array array, int index) => Methods.CopyTo((MethodData[])array, index);

    public static bool operator ==(MethodList? left, MethodList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(MethodList? left, MethodList? right) => !(left == right);
}

[DebuggerDisplay($"Count = {{{nameof(Count)}}}")]
public sealed class MethodListView : IReadOnlyList<IMethodDataView>, ICollection, IEmptyCollectionProvider<IMethodListView>, IEquatable<IMethodListView>, IMethodListView
{
    public static IMethodListView Empty { get; } = new MethodListView();
    private readonly int _hashCode; // precomputed
    private readonly ILookup<string, IMethodDataView> _methodNameIndex;
    private readonly ITypeDataView? _declaringType;
    private IMethodListView? _view;

    public MethodListView(IMethodDataView[] items, ITypeDataView? declaringType) : this((IEnumerable<IMethodDataView>)items, declaringType)
    {
    }

    public MethodListView(IEnumerable<IMethodDataView> items, ITypeDataView? declaringType)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringType = declaringType;

        Methods = items?.ToImmutableList() ?? ImmutableList<IMethodDataView>.Empty;
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal); // allow duplicate method names (overloads)

        if (HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Methods,
                methodData => !ReferenceEquals(methodData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IMethodDataView)}.{nameof(IMethodDataView.DeclaringType)}' declaring type handle. All methods must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal MethodListView(IEnumerable<IMethodDataView> items, ITypeDataView? declaringType, bool isIntegrityValidationEnabled)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNull(declaringType);
        _declaringType = declaringType;

        Methods = items?.ToImmutableList() ?? ImmutableList<IMethodDataView>.Empty;

        // allow duplicate method names (overloads)
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal);

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Methods,
                methodData => !ReferenceEquals(methodData.DeclaringType, _declaringType),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(IMethodDataView)}.{nameof(IMethodDataView.DeclaringType)}' declaring type handle. All methods must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private MethodListView()
    {
        Methods = ImmutableList<IMethodDataView>.Empty;
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal);
        _declaringType = default;
        _hashCode = ComputeHashCode();
    }

    public bool TryGetMethodsByName(string methodName, out IMethodListView methodList)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName);
        methodList = _methodNameIndex[methodName].ToMethodListView(DeclaringType);

        return methodList.HasItems;
    }

    public bool ContainsMethodWithName(string methodName)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName);
        return _methodNameIndex.Contains(methodName);
    }

    public int Count => Methods.Count;
    public bool IsEmpty => Methods.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<IMethodDataView> Methods { get; }
    public IMethodListView View => _view ??= Methods.ToMethodListView(DeclaringType);
    public ITypeDataView DeclaringType => _declaringType ?? throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringType)));

    // Immutable collections are inherently thread-safe for read operations,
    // so we can consider this collection as synchronized for enumeration and access.
    public bool IsSynchronized { get; } = true;

    object ICollection.SyncRoot => this;

    public IMethodDataView this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Methods.Count, nameof(index));

            return HasItems
                ? Methods[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(IMethodListView), ReflectionConstants.IndexerGetMethodName));
        }
    }

    /// <summary>
    /// Gets the method data for the method with the specified name and parameter signature.
    /// </summary>
    /// <remarks>Use this indexer to retrieve method metadata when you know both the method name and
    /// the exact parameter signature. Parameter matching considers type, position, and modifier (such as ref or
    /// out).</remarks>
    /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
    /// <param name="methodParameters">An array of parameter information objects that describe the expected parameter types, positions, and
    /// modifiers for the method signature.</param>
    /// <returns>The method data that matches the specified name and parameter signature.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the method index has not been initialized.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if no method with the specified name exists, or if no method with the specified name matches the
    /// provided parameter signature.</exception>
    public IMethodDataView this[string? methodName, ParameterDescriptorList? methodParameters]
    {
        get
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), ReflectionConstants.IndexerGetMethodName));
            }

            if (string.IsNullOrWhiteSpace(methodName) && methodParameters is null)
            {
                throw new ArgumentNullException(nameof(methodName), $"Provide at least one valid argument. Argument '{nameof(methodName)}' cannot be null or empty and '{nameof(methodParameters)}' cannot be null.");
            }

            if (_methodNameIndex == null)
            {
                throw new InvalidOperationException("Method index is not initialized.");
            }

            methodParameters = methodParameters.OrEmpty();
            ImmutableList<IMethodDataView> methods = Methods;
            if (!string.IsNullOrWhiteSpace(methodName))
            {
                methods = _methodNameIndex[methodName].ToImmutableList();
                if (methods.IsEmpty)
                {
                    throw new KeyNotFoundException($"Invalid key.No method named '{methodName}' could be found.");
                }

                if (methods.Count == 1 && methodParameters.Count == 0)
                {
                    return methods[0];
                }

                if (methods.Count > 1 && methodParameters.Count == 0)
                {
                    throw new AmbiguousMatchException($"Ambiguous match found for method '{methodName}'. Try to provide the parameter types via the '{nameof(methodParameters)}' argument to disambiguate.");
                }
            }

            var results = new List<IMethodDataView>(methods.Count);
            foreach (IMethodDataView method in methods)
            {
                IParameterListView parameters = method.Parameters;
                if (parameters.Count != methodParameters.Count)
                {
                    continue;
                }

                for (int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++)
                {
                    IParameterDataView parameter = parameters[parameterIndex];
                    ParameterDescriptor parameterInfo = methodParameters[parameterIndex];
                    ParameterDescriptor parameterDescriptor = parameterInfo.ParameterDescriptor;

                    // Parameter is valid if it matches position in method signature, parameter type and modifier.
                    // Since parameter collections are always ordered by parameter position in ascending order, we don't have to check the order explicitly (only count - see above).
                    if (parameterDescriptor.HasParameterTypeHandle
                        && !parameter.ParameterTypeData.Handle.Equals(parameterDescriptor.ParameterTypeHandle))
                    {
                        break;
                    }

                    if (parameterDescriptor.HasParameterModifier
                        && !parameter.ParameterModifier.Equals(parameterDescriptor.ParameterModifier))
                    {
                        break;
                    }
                }

                results.Add(method);
            }

            if (results.Count > 1)
            {
                throw new AmbiguousMatchException($"Ambiguous match found based on the provided information. Try to provide the parameter names and the parameter types via the '{nameof(methodName)}' and '{nameof(methodParameters)}' arguments to disambiguate.");
            }

            if (results.IsEmpty())
            {
                throw new KeyNotFoundException($"Invalid arguments. No method could be found based on the provided information. Either the method doe not exist or the provided information is insufficient.");
            }

            return results[0];
        }
    }

    public IEnumerator<IMethodDataView> GetEnumerator() => ((IEnumerable<IMethodDataView>)Methods).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => Methods.GetEnumerator();

    public bool Equals(IMethodListView? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!ReferenceEquals(DeclaringType, other.DeclaringType))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!ReferenceEquals(Methods[index], other.Methods[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj) => obj is IMethodListView other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringType);
            for (int index = 0; index < Methods.Count; index++)
            {
                hashCode.Add(Methods[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public void CopyTo(Array array, int index) => Methods.CopyTo((IMethodDataView[])array, index);

    public static bool operator ==(MethodListView? left, IMethodListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(MethodListView? left, IMethodListView? right) => !(left == right);

    public static bool operator ==(IMethodListView? left, MethodListView? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(IMethodListView? left, MethodListView? right) => !(left == right);
}
