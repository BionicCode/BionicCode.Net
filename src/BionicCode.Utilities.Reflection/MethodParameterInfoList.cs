namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// A read-only list of <see cref="MethodParameterInfo"/> items sorted by parameter position in ascending order.
/// </summary>
/// <remarks>The <see cref="MethodParameterInfo"/> items must belong to the same member of the same declaring type.
/// This collection is not intended for a loose collection of unrelated parameters.<br/>
/// Instead the collection is a strict representation of member parameters.</remarks>
internal sealed class MethodParameterInfoList : IReadOnlyList<MethodParameterInfo>, IEquatable<MethodParameterInfoList>
{
    public static MethodParameterInfoList Empty { get; } = new MethodParameterInfoList();
    private readonly int _hashCode; // precomputed
    private readonly SymbolReflectionInfoCacheKeyInternal _declaringTypeCacheKey;
    private readonly Dictionary<string, MethodParameterInfo> _parameterNameIndex;

    public MethodParameterInfoList(MethodParameterInfo[] items) : this((IEnumerable<MethodParameterInfo>)items)
    {
    }

    public MethodParameterInfoList(ReadOnlySpan<MethodParameterInfo> items) : this(items.ToArray())
    {
    }

    public MethodParameterInfoList(IEnumerable<MethodParameterInfo> items)
    {
        Parameters = items.OrderBy(parameter => parameter.Position).ToImmutableList();
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.MethodName, StringComparer.Ordinal);
        ArgumentNullExceptionAdvanced.ThrowIfNull(Parameters, nameof(items));

        if (HasItems)
        {
            MethodParameterInfo methodParameterInfo = Parameters.FirstOrDefault();
            RuntimeTypeHandle declaringMemberTypeHandle = methodParameterInfo.DeclaringTypeHandle;
            Type declaringType = Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle)
                ?? throw new ArgumentException($"The argument '{nameof(items)}' contains an invalid item at position '0'. Reason: Could not resolve type from handle 'ParameterTypeHandle'.");
            _declaringTypeCacheKey = declaringType.ToTypeData().CacheKey;
            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => !parameterData.DeclaringTypeHandle.Equals(declaringMemberTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal MethodParameterInfoList(IEnumerable<MethodParameterInfo> items, bool isIntegrityValidationEnabled)
    {
        Parameters = items.OrderBy(parameter => parameter.Position).ToImmutableList();
        _parameterNameIndex = Parameters.ToDictionary(parameter => parameter.MethodName, StringComparer.Ordinal);
        ArgumentNullExceptionAdvanced.ThrowIfNull(Parameters, nameof(items));

        if (HasItems)
        {
            MethodParameterInfo methodParameterInfo = Parameters.FirstOrDefault();
            RuntimeTypeHandle declaringMemberTypeHandle = methodParameterInfo.DeclaringTypeHandle;
            Type declaringType = Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle)
                ?? throw new ArgumentException($"The argument '{nameof(items)}' contains an invalid item at position '0'. Reason: Could not resolve type from handle 'ParameterTypeHandle'.");
            _declaringTypeCacheKey = declaringType.ToTypeData().CacheKey;
            ArgumentExceptionAdvanced.ThrowIfAny(
                Parameters,
                parameterData => !parameterData.DeclaringTypeHandle.Equals(declaringMemberTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private MethodParameterInfoList()
    {
        Parameters = ImmutableList<MethodParameterInfo>.Empty;
        _parameterNameIndex = new Dictionary<string, MethodParameterInfo>(0, StringComparer.Ordinal);
    }

    public bool TryGetParameterByName(string parameterName, out MethodParameterInfo parameterData)
    {
        ArgumentNullExceptionAdvanced.ThrowIfNullOrWhiteSpace(parameterName);
        return _parameterNameIndex.TryGetValue(parameterName, out parameterData);
    }

    public int Count => Parameters.Count;
    public bool IsEmpty => Parameters.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<MethodParameterInfo> Parameters { get; }
    public SymbolReflectionInfoCacheKeyInternal DeclaringTypeCacheKey
        => HasItems
            ? _declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeData)));
            }

            SymbolReflectionInfoCacheKeyInternal cacheKey = DeclaringTypeCacheKey;
            return SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey);
        }
    }

    public MethodParameterInfo this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Parameters.Count, nameof(index));

            return HasItems
                ? Parameters[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodParameterInfoList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<MethodParameterInfo> GetEnumerator()
        => ((IEnumerable<MethodParameterInfo>)Parameters).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Parameters.GetEnumerator();

    public bool Equals(MethodParameterInfoList? other)
    {
        if (other is null)
        {
            return false;
        }

        if (Count != other.Count)
        {
            return false;
        }

        if (!DeclaringTypeCacheKey.Equals(other.DeclaringTypeCacheKey))
        {
            return false;
        }

        for (int index = 0; index < Count; index++)
        {
            if (!Parameters[index].Equals(other.Parameters[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is MethodParameterInfoList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Parameters.Count; index++)
            {
                hashCode.Add(Parameters[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(MethodParameterInfoList? left, MethodParameterInfoList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(MethodParameterInfoList? left, MethodParameterInfoList? right)
        => !(left == right);
}
