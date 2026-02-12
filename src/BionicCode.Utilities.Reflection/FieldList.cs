namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal sealed class FieldList : IReadOnlyList<FieldData>, IEquatable<FieldList>
{
    public static FieldList Empty { get; } = new FieldList();
    private readonly int _hashCode; // precomputed
    private readonly Dictionary<string, FieldData> _fieldNameIndex;
    private readonly SymbolReflectionInfoCacheKeyInternal _declaringTypeCacheKey;

    public FieldList(FieldData[] items) : this((IEnumerable<FieldData>)items)
    {
    }

    public FieldList(IEnumerable<FieldData> items)
    {
        Fields = items?.ToImmutableList() ?? ImmutableList<FieldData>.Empty;
        _fieldNameIndex = Fields.ToDictionary(fieldData => fieldData.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            _declaringTypeCacheKey = Fields.First()!.DeclaringTypeData.CacheKey;
            ArgumentExceptionAdvanced.ThrowIfAny(
                Fields,
                fieldData => fieldData.DeclaringTypeData.CacheKey != _declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(FieldData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All fields must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    internal FieldList(IEnumerable<FieldData> items, bool isIntegrityValidationEnabled)
    {
        Fields = items?.ToImmutableList() ?? ImmutableList<FieldData>.Empty;
        _fieldNameIndex = Fields.ToDictionary(fieldData => fieldData.Name, StringComparer.Ordinal);
        _declaringTypeCacheKey = Fields.FirstOrDefault()?.DeclaringTypeData.CacheKey ?? default;

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Fields,
                fieldData => fieldData.DeclaringTypeData.CacheKey != _declaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(FieldData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All fields must belong to the same declaring type.");

        }

        _hashCode = ComputeHashCode();
    }

    private FieldList()
    {
        Fields = ImmutableList<FieldData>.Empty;
        _fieldNameIndex = new Dictionary<string, FieldData>(0, StringComparer.Ordinal);
    }

    public bool TryGetFieldByName(string fieldName, out FieldData? fieldData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);
        return _fieldNameIndex.TryGetValue(fieldName, out fieldData!);
    }

    public int Count => Fields.Count;
    public bool IsEmpty => Fields.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<FieldData> Fields { get; }
    public SymbolReflectionInfoCacheKeyInternal DeclaringTypeCacheKey
        => HasItems
            ? _declaringTypeCacheKey
            : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(DeclaringTypeCacheKey)));

    public TypeData DeclaringTypeData
    {
        get
        {
            SymbolReflectionInfoCacheKeyInternal cacheKey = DeclaringTypeCacheKey;
            return HasItems
                ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(DeclaringTypeData)));
        }
    }

    public FieldData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Fields.Count, nameof(index));

            return HasItems
                ? Fields[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(FieldList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<FieldData> GetEnumerator()
        => ((IEnumerable<FieldData>)Fields).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Fields.GetEnumerator();

    public bool Equals(FieldList? other)
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

        bool isEqual = false;
        for (int index = 0; index < Count && !isEqual; index++)
        {
            if (!Fields[index].Equals(other.Fields[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is FieldList other && Equals(other);

    public override int GetHashCode() => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Fields.Count; index++)
            {
                hashCode.Add(Fields[index]);
            }

            return hashCode.ToHashCode();
        }
    }
}
