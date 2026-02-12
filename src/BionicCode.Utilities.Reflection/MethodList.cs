namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal sealed class MethodList : IReadOnlyList<MethodData>, IEquatable<MethodList>
{
    public static MethodList Empty { get; } = new MethodList();
    private readonly int _hashCode; // precomputed
    private readonly ILookup<string, MethodData> _methodNameIndex;
    private readonly SymbolReflectionInfoCacheKeyInternal _declaringTypeCacheKey;

    public MethodList(MethodData[] items) : this((IEnumerable<MethodData>)items)
    {
    }

    public MethodList(IEnumerable<MethodData> items)
    {
        Methods = items?.ToImmutableList() ?? ImmutableList<MethodData>.Empty;
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal); // allow duplicate method names (overloads)

        if (HasItems)
        {
            _declaringTypeCacheKey = Methods.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Methods,
                methodData => methodData.DeclaringTypeData.CacheKey != DeclaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal MethodList(IEnumerable<MethodData> items, bool isIntegrityValidationEnabled)
    {
        Methods = items?.ToImmutableList() ?? ImmutableList<MethodData>.Empty;

        // allow duplicate method names (overloads)
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal);

        _declaringTypeCacheKey = HasItems
            ? Methods.First().DeclaringTypeData.CacheKey
            : default;

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Methods,
                methodData => methodData.DeclaringTypeData.CacheKey != DeclaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private MethodList()
    {
        Methods = ImmutableList<MethodData>.Empty;
        _methodNameIndex = Methods.ToLookup(method => method.Name, StringComparer.Ordinal);
    }

    public bool TryGetMethodsByName(string methodName, out MethodList methodList)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(methodName);
        methodList = _methodNameIndex[methodName]
            .ToMethodList();

        return methodList.HasItems;
    }

    public int Count => Methods.Count;
    public bool IsEmpty => Methods.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<MethodData> Methods { get; }
    public SymbolReflectionInfoCacheKeyInternal DeclaringTypeCacheKey => HasItems
        ? _declaringTypeCacheKey
        : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKeyInternal cacheKey = DeclaringTypeCacheKey;
            return HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeData)));
        }
    }

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
    public MethodData this[string methodName, params MethodParameterInfo[] methodParameters]
    {
        get
        {
            if (_methodNameIndex == null)
            {
                throw new InvalidOperationException("Method index is not initialized.");
            }

            var methods = _methodNameIndex[methodName].ToMethodList();
            if (methods.IsEmpty)
            {
                throw new KeyNotFoundException($"Invalid key.No method named '{methodName}' could be found.");
            }

            foreach (MethodData method in methods)
            {
                ParameterList parameters = method.Parameters;
                bool isMethodInvalidCandidate = false;
                int parameterIndex = 0;
                foreach (ParameterData parameter in parameters)
                {
                    if (parameters.Count != methodParameters.Length)
                    {
                        isMethodInvalidCandidate = true;
                        break;
                    }

                    // Parameter is valid if it matches position in method signature, parameter type and modifier.
                    foreach (MethodParameterInfo parameterInfo in methodParameters)
                    {
                        if (parameterIndex == parameterInfo.Position)
                        {
                            if (!parameter.ParameterTypeData.Handle.Equals(parameterInfo.ParameterTypeHandle)
                                || !parameter.ParameterKind.Equals(parameterInfo.Kind))
                            {
                                isMethodInvalidCandidate = true;
                                break;
                            }
                        }
                    }

                    if (isMethodInvalidCandidate)
                    {
                        break;
                    }
                }

                if (!isMethodInvalidCandidate)
                {
                    return method;
                }
            }

            throw new KeyNotFoundException($"Invalid arguments. A method named '{methodName}' could be found but its signature does not match the provided parameter list of the '{nameof(methodParameters)}' argument. Ensure '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.ParameterTypeHandle)}' is referencing the correct type and the parameter is in the correct position expressed by collection index and the '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.Kind)}' describes the correct parameter modifier.");
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

        if (DeclaringTypeCacheKey != other.DeclaringTypeCacheKey)
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Methods[index].Equals(other.Methods[index]))
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
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Methods.Count; index++)
            {
                hashCode.Add(Methods[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(MethodList? left, MethodList? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(MethodList? left, MethodList? right) => !(left == right);
}
