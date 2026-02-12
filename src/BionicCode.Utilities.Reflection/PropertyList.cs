namespace BionicCode.Utilities.Net.Reflection;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

/// <summary>
/// Represents a read-only list of <see cref="PropertyData"/> items that belong to the same declaring type.
/// </summary>
internal sealed class PropertyList : IReadOnlyList<PropertyData>, IEquatable<PropertyList>
{
    public static PropertyList Empty { get; } = new PropertyList();
    private readonly int _hashCode; // precomputed
    private readonly SymbolReflectionInfoCacheKeyInternal _declaringTypeCacheKey;
    private readonly Dictionary<string, PropertyData> _propertyNameIndex;

    public PropertyList(PropertyData[] items) : this((IEnumerable<PropertyData>)items)
    {
    }

    public PropertyList(IEnumerable<PropertyData> items)
    {
        Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
        _propertyNameIndex = Properties.ToDictionary(property => property.Name, StringComparer.Ordinal);

        if (HasItems)
        {
            _declaringTypeCacheKey = Properties.First().DeclaringTypeData.CacheKey;

            ArgumentExceptionAdvanced.ThrowIfAny(
                Properties,
                property => property.DeclaringTypeData.CacheKey != DeclaringTypeCacheKey,
                nameof(items),
                $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    internal PropertyList(IEnumerable<PropertyData> items, bool isIntegrityValidationEnabled)
    {
        Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
        _propertyNameIndex = Properties.ToDictionary(property => property.Name);
        _declaringTypeCacheKey = HasItems
            ? Properties.First().DeclaringTypeData.CacheKey
            : default;

        if (isIntegrityValidationEnabled && HasItems)
        {
            ArgumentExceptionAdvanced.ThrowIfAny(
                Properties,
                property => !property.DeclaringTypeHandle.Equals(DeclaringTypeCacheKey),
                nameof(items),
                $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
        }

        _hashCode = ComputeHashCode();
    }

    private PropertyList()
    {
        Properties = ImmutableList<PropertyData>.Empty;
        _propertyNameIndex = new Dictionary<string, PropertyData>(0);
    }

    public bool TryGetPropertyByName(string propertyName, out PropertyData? propertyData)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(propertyName);
        return _propertyNameIndex.TryGetValue(propertyName, out propertyData);
    }

    public int Count => Properties.Count;
    public bool IsEmpty => Properties.IsEmpty;
    public bool HasItems => !IsEmpty;
    public ImmutableList<PropertyData> Properties { get; }
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

    public PropertyData this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Properties.Count, nameof(index));

            return HasItems
                ? Properties[index]
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(PropertyList), ReflectionConstants.IndexerGetMethodName));
        }
    }

    public IEnumerator<PropertyData> GetEnumerator()
        => ((IEnumerable<PropertyData>)Properties).GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        => Properties.GetEnumerator();

    public bool Equals(PropertyList? other)
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
            if (!Properties[index].Equals(other.Properties[index]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object? obj)
        => obj is PropertyList other && Equals(other);

    public override int GetHashCode()
        => _hashCode;

    private int ComputeHashCode()
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(Count);
            hashCode.Add(DeclaringTypeCacheKey);
            for (int index = 0; index < Properties.Count; index++)
            {
                hashCode.Add(Properties[index]);
            }

            return hashCode.ToHashCode();
        }
    }

    public static bool operator ==(PropertyList? left, PropertyList? right)
        => left?.Equals(right) ?? (right is null);
    public static bool operator !=(PropertyList? left, PropertyList? right)
        => !(left == right);
}
