namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal sealed class ConstructorList : IReadOnlyList<ConstructorData>, IEquatable<ConstructorList>
{
    public static ConstructorList Empty { get; } = new ConstructorList();
    private readonly int _hashCode; // precomputed
    private readonly SymbolReflectionInfoCacheKey _declaringTypeCacheKey;

    public ConstructorList(ConstructorData[] items) : this((IEnumerable<ConstructorData>)items)
    {
    }

    public ConstructorList(IEnumerable<ConstructorData> items)
    {
        Constructors = items?.ToImmutableList() ?? ImmutableList<ConstructorData>.Empty;

        if (HasItems)
        {
            _declaringTypeCacheKey = Constructors.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Constructors,
                constructorData => constructorData.DeclaringTypeData.CacheKey != DeclaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ConstructorData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All constructors must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal ConstructorList(IEnumerable<ConstructorData> items, bool isIntegrityValidationEnabled)
    {
        Constructors = items?.ToImmutableList() ?? ImmutableList<ConstructorData>.Empty;
        _declaringTypeCacheKey = HasItems
            ? Constructors.First().DeclaringTypeData.CacheKey
            : default;

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Constructors,
                constructorData => constructorData.DeclaringTypeData.CacheKey != DeclaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ConstructorData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All constructors must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    private ConstructorList()
        => Constructors = ImmutableList<ConstructorData>.Empty;

    public int Count => Constructors.Count;
    public bool IsEmpty => Constructors.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<ConstructorData> Constructors { get; }
    public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
        => HasItems
            ? _declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKey cacheKey = DeclaringTypeCacheKey;
            return HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(DeclaringTypeData)));
        }
    }

    public ConstructorData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Constructors.Count, nameof(index));

            return HasItems
                ? Constructors[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ConstructorList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<ConstructorData> GetEnumerator()
        => ((IEnumerable<ConstructorData>)Constructors).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Constructors.GetEnumerator();

    public bool Equals(ConstructorList? other)
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
            if (!Constructors[index].Equals(other.Constructors[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is ConstructorList other && Equals(other);

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Constructors.Count; index++)
            {
                hashCode.Add(Constructors[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(ConstructorList? left, ConstructorList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ConstructorList? left, ConstructorList? right)
        => !(left == right);
}
