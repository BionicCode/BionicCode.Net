namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

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

    /// <summary>
    /// Gets the method data for the method with the specified name and parameter signature.
    /// </summary>
    /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
    /// <param name="genericMethodParameters">Optional. A <see cref="TypeList"/> of generic method parameter types that describe the expected generic parameters for the method. Should be <see langword="null"/> or empty if the method is not generic. If the method is a generic method, consider providing the expected generic parameters to disambiguate the method.</param>
    /// <param name="methodParameters">Optional. A <see cref="ParameterDescriptorList"/> of parameter information objects that describe the expected parameter types, positions, and modifiers for the method signature. Should be <see langword="null"/> or empty if method is parameterless. If the method has parameters, consider providing the expected parameters to disambiguate the method.</param>
    /// <param name="methodData">When this method returns, contains the method data that matches the specified name and parameter signature, if found; otherwise, null.</param>
    /// <returns>A <see cref="MemberLookupState"/> value indicating the result of the method lookup.</returns>
    public MemberLookupState TryGetMethod(string? methodName, TypeList? genericMethodParameters, ParameterDescriptorList? methodParameters, out MethodData? methodData)
    {
        methodData = null;

        if (IsEmpty)
        {
            return MemberLookupState.SourceEmpty;
        }

        if (string.IsNullOrWhiteSpace(methodName) && methodParameters is null && genericMethodParameters is null)
        {
            return MemberLookupState.Ambiguous;
        }

        if (_methodNameIndex == null)
        {
            throw new InvalidOperationException("Internal method index is not initialized.");
        }

        ImmutableList<MethodData> methods = Methods;
        if (!string.IsNullOrWhiteSpace(methodName))
        {
            methods = _methodNameIndex[methodName].ToImmutableList();
            if ((methodParameters is null || methodParameters.IsEmpty)
                && (genericMethodParameters is null || genericMethodParameters.IsEmpty))
            {
                if (methods.Count == 1)
                {
                    methodData = methods[0];
                    return MemberLookupState.Found;
                }

                if (methods.IsEmpty)
                {
                    return MemberLookupState.NotFound;
                }

                if (methods.Count > 1)
                {
                    return MemberLookupState.Ambiguous;
                }
            }
        }

        var results = new List<MethodData>(methods.Count);
        foreach (MethodData method in methods)
        {
            bool isTargetMethodGeneric = genericMethodParameters is not null
                && genericMethodParameters.HasItems;
            if (isTargetMethodGeneric)
            {
                if (!method.IsGenericMethod)
                {
                    continue;
                }

                if (!method.GenericMethodParameters.Equals(genericMethodParameters))
                {
                    continue;
                }
            }

            if (methodParameters is null
                || ParameterListEqualityComparer.Equals(methodParameters, method.Parameters) is EqualityComparisonResult.True or EqualityComparisonResult.TrueButAmbiguous)
            {
                results.Add(method);
            }
        }

        methodData = results.FirstOrDefault();
        return results.Count switch
        {
            0 => MemberLookupState.NotFound,
            1 => MemberLookupState.Found,
            _ => MemberLookupState.Ambiguous
        };
    }

    /// <summary>
    /// Gets the method data for the method with the specified name and parameter signature.
    /// </summary>
    /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
    /// <param name="genericMethodParameters">Optional. A <see cref="TypeList"/> of generic method parameter types that describe the expected generic parameters for the method. Should be <see langword="null"/> or empty if the method is not generic. If the method is a generic method, consider providing the expected generic parameters to disambiguate the method.</param>
    /// <param name="methodParameters">Optional. A <see cref="ParameterDescriptorList"/> of parameter information objects that describe the expected parameter types, positions, and modifiers for the method signature. Should be <see langword="null"/> or empty if method is parameterless. If the method has parameters, consider providing the expected parameters to disambiguate the method.</param>
    /// <param name="methodData">When this method returns, contains the method data that matches the specified name and parameter signature, if found; otherwise, null.</param>
    /// <returns>A <see cref="MemberLookupState"/> value indicating the result of the method lookup.</returns>
    public MemberLookupState TryGetMethod(string? methodName, TypeList? genericMethodParameters, ParameterList? methodParameters, out MethodData? methodData)
    {
        methodData = null;

        if (IsEmpty)
        {
            return MemberLookupState.SourceEmpty;
        }

        if (string.IsNullOrWhiteSpace(methodName) && methodParameters is null && genericMethodParameters is null)
        {
            return MemberLookupState.Ambiguous;
        }

        if (_methodNameIndex == null)
        {
            throw new InvalidOperationException("Internal method index is not initialized.");
        }

        ImmutableList<MethodData> methods = Methods;
        if (!string.IsNullOrWhiteSpace(methodName))
        {
            methods = _methodNameIndex[methodName].ToImmutableList();
            if ((methodParameters is null || methodParameters.IsEmpty)
                && (genericMethodParameters is null || genericMethodParameters.IsEmpty))
            {
                if (methods.Count == 1)
                {
                    methodData = methods[0];
                    return MemberLookupState.Found;
                }

                if (methods.IsEmpty)
                {
                    return MemberLookupState.NotFound;
                }

                if (methods.Count > 1)
                {
                    return MemberLookupState.Ambiguous;
                }
            }
        }

        var results = new List<MethodData>(methods.Count);
        foreach (MethodData method in methods)
        {
            bool isTargetMethodGeneric = genericMethodParameters is not null
                && genericMethodParameters.HasItems;
            if (isTargetMethodGeneric)
            {
                if (!method.IsGenericMethod)
                {
                    continue;
                }

                if (!method.GenericMethodParameters.Equals(genericMethodParameters))
                {
                    continue;
                }
            }
            
            if (methodParameters is null
                || methodParameters.Equals(method.Parameters))
            {
                results.Add(method);
            }
        }

        methodData = results.FirstOrDefault();
        return results.Count switch
        {
            0 => MemberLookupState.NotFound,
            1 => MemberLookupState.Found,
            _ => MemberLookupState.Ambiguous
        };
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

    /// <summary>
    /// Gets the method data for the method with the specified name and parameter signature.
    /// </summary>
    /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
    /// <param name="genericMethodParameters">Optional. A <see cref="TypeList"/> of generic method parameter types that describe the expected generic parameters for the method. Should be <see langword="null"/> or empty if the method is not generic. If the method is a generic method, consider providing the expected generic parameters to disambiguate the method.</param>
    /// <param name="methodParameters">Optional. A <see cref="ParameterDescriptorList"/> of parameter information objects that describe the expected parameter types, positions, and modifiers for the method signature. Should be <see langword="null"/> or empty if method is parameterless. If the method has parameters, consider providing the expected parameters to disambiguate the method.</param>
    /// <param name="methodDataView">When this method returns, contains the method data that matches the specified name and parameter signature, if found; otherwise, null.</param>
    /// <returns>A <see cref="MemberLookupState"/> value indicating the result of the method lookup.</returns>
    public MemberLookupState TryGetMethod(string? methodName, ITypeListView? genericMethodParameters, ParameterDescriptorList? methodParameters, out IMethodDataView? methodDataView)
    {
        methodDataView = null;

        if (IsEmpty)
        {
            return MemberLookupState.SourceEmpty;
        }

        if (string.IsNullOrWhiteSpace(methodName) && methodParameters is null && genericMethodParameters is null)
        {
            return MemberLookupState.Ambiguous;
        }

        if (_methodNameIndex == null)
        {
            throw new InvalidOperationException("Internal method index is not initialized.");
        }

        ImmutableList<IMethodDataView> methods = Methods;
        if (!string.IsNullOrWhiteSpace(methodName))
        {
            methods = _methodNameIndex[methodName].ToImmutableList();
            if ((methodParameters is null || methodParameters.IsEmpty)
                && (genericMethodParameters is null || genericMethodParameters.IsEmpty))
            {
                if (methods.Count == 1)
                {
                    methodDataView = methods[0];
                    return MemberLookupState.Found;
                }

                if (methods.IsEmpty)
                {
                    return MemberLookupState.NotFound;
                }

                if (methods.Count > 1)
                {
                    return MemberLookupState.Ambiguous;
                }
            }
        }

        var results = new List<IMethodDataView>(methods.Count);
        foreach (IMethodDataView method in methods)
        {
            bool isTargetMethodGeneric = genericMethodParameters is not null
                && genericMethodParameters.HasItems;
            if (isTargetMethodGeneric)
            {
                if (!method.IsGenericMethod)
                {
                    continue;
                }

                if (!method.GenericMethodParameters.Equals(genericMethodParameters))
                {
                    continue;
                }
            }

            if (methodParameters is null
                || ParameterListEqualityComparer.Equals(methodParameters, method.Parameters) is EqualityComparisonResult.True or EqualityComparisonResult.TrueButAmbiguous)
            {
                results.Add(method);
            }
        }

        methodDataView = results.FirstOrDefault();
        return results.Count switch
        {
            0 => MemberLookupState.NotFound,
            1 => MemberLookupState.Found,
            _ => MemberLookupState.Ambiguous
        };
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
