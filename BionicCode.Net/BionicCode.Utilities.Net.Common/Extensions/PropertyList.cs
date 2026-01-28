namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a read-only list of <see cref="PropertyData"/> items that belong to the same declaring type.
    /// </summary>
    internal sealed class PropertyList : IReadOnlyList<PropertyData>, IEquatable<PropertyList>
    {
        public static readonly PropertyList Empty = new PropertyList();
        private readonly int _hashCode; // precomputed
        private readonly SymbolReflectionInfoCacheKey _declaringTypeCacheKey;
        private readonly Dictionary<string, PropertyData> _propertyNameIndex;

        public PropertyList(PropertyData[] items) : this((IEnumerable<PropertyData>)items)
        {
        }

        public PropertyList(IEnumerable<PropertyData> items)
        {
            this.Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
            this._propertyNameIndex = this.Properties.ToDictionary(property => property.Name, StringComparer.Ordinal);

            if (this.HasItems)
            {
                this._declaringTypeCacheKey = this.Properties.First().DeclaringTypeData.CacheKey;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Properties,
                    property => property.DeclaringTypeData.CacheKey != this.DeclaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
        }

        internal PropertyList(IEnumerable<PropertyData> items, bool isIntegrityValidationEnabled)
        {
            this.Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
            this._propertyNameIndex = this.Properties.ToDictionary(property => property.Name);
            this._declaringTypeCacheKey = this.HasItems
                ? this.Properties.First().DeclaringTypeData.CacheKey
                : default;

            if (isIntegrityValidationEnabled && this.HasItems)
            {
                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Properties,
                    property => !property.DeclaringTypeHandle.Equals(this.DeclaringTypeCacheKey),
                    nameof(items),
                    $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
        }

        private PropertyList()
        {
            this.Properties = ImmutableList<PropertyData>.Empty;
            this._propertyNameIndex = new Dictionary<string, PropertyData>(0);
        }

        public bool TryGetPropertyByName(string propertyName, out PropertyData? propertyData)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(propertyName);
            return this._propertyNameIndex.TryGetValue(propertyName, out propertyData);
        }

        public int Count => this.Properties.Count;
        public bool IsEmpty => this.Properties.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<PropertyData> Properties { get; }
        public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
            => this.HasItems
                ? this._declaringTypeCacheKey
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeCacheKey)));

        public TypeData DeclaringTypeData
        {
            get
            {
                SymbolReflectionInfoCacheKey cacheKey = this.DeclaringTypeCacheKey;
                return this.HasItems
                    ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeData)));
            }
        }

        public PropertyData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Properties.Count, nameof(index));

                return this.HasItems
                    ? this.Properties[index]
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(PropertyList), ReflectionConstants.IndexerGetMethodName));
            }
        }

        public IEnumerator<PropertyData> GetEnumerator()
            => ((IEnumerable<PropertyData>)this.Properties).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Properties.GetEnumerator();

        public bool Equals(PropertyList? other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            if (this.DeclaringTypeCacheKey != other.DeclaringTypeCacheKey)
            {
                return false;
            }

            for (int index = 0; index < this.Count; index++)
            {
                if (!this.Properties[index].Equals(other.Properties[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is PropertyList other && Equals(other);

        public override int GetHashCode()
            => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeCacheKey);
                for (int index = 0; index < this.Properties.Count; index++)
                {
                    hashCode.Add(this.Properties[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(PropertyList? left, PropertyList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(PropertyList? left, PropertyList? right)
            => !(left == right);
    }
}
