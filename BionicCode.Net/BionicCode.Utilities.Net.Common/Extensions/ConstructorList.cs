namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class ConstructorList : IReadOnlyList<ConstructorData>, IEquatable<ConstructorList>
    {
        public static readonly ConstructorList Empty = new ConstructorList();
        private readonly int _hashCode; // precomputed
        private readonly SymbolReflectionInfoCacheKey _declaringTypeCacheKey;

        public ConstructorList(ConstructorData[] items) : this((IEnumerable<ConstructorData>)items)
        {
        }

        public ConstructorList(IEnumerable<ConstructorData> items)
        {
            this.Constructors = items?.ToImmutableList() ?? ImmutableList<ConstructorData>.Empty;

            if (this.HasItems)
            {
                this._declaringTypeCacheKey = this.Constructors.First().DeclaringTypeData.CacheKey;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Constructors,
                    constructorData => constructorData.DeclaringTypeData.CacheKey != this.DeclaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ConstructorData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All constructors must belong to the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
        }

        internal ConstructorList(IEnumerable<ConstructorData> items, bool isIntegrityValidationEnabled)
        {
            this.Constructors = items?.ToImmutableList() ?? ImmutableList<ConstructorData>.Empty;
            this._declaringTypeCacheKey = this.HasItems
                ? this.Constructors.First().DeclaringTypeData.CacheKey
                : default;

            if (isIntegrityValidationEnabled && this.HasItems)
            {
                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Constructors,
                    constructorData => constructorData.DeclaringTypeData.CacheKey != this.DeclaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ConstructorData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All constructors must belong to the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
        }

        private ConstructorList()
            => this.Constructors = ImmutableList<ConstructorData>.Empty;

        public int Count => this.Constructors.Count;
        public bool IsEmpty => this.Constructors.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<ConstructorData> Constructors { get; }
        public SymbolReflectionInfoCacheKey DeclaringTypeCacheKey
            => this.HasItems
                ? this._declaringTypeCacheKey
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(this.DeclaringTypeCacheKey)));

        public TypeData DeclaringTypeData
        {
            get
            {
                SymbolReflectionInfoCacheKey cacheKey = this.DeclaringTypeCacheKey;
                return this.HasItems
                    ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(GetType().Name, nameof(this.DeclaringTypeData)));
            }
        }

        public ConstructorData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Constructors.Count, nameof(index));

                return this.HasItems
                    ? this.Constructors[index]
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(ConstructorList), ReflectionConstants.IndexerGetMethodName));
            }
        }

        public IEnumerator<ConstructorData> GetEnumerator()
            => ((IEnumerable<ConstructorData>)this.Constructors).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Constructors.GetEnumerator();

        public bool Equals(ConstructorList? other)
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
                if (!this.Constructors[index].Equals(other.Constructors[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is ConstructorList other && Equals(other);

        public override int GetHashCode()
            => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeCacheKey);
                for (int index = 0; index < this.Constructors.Count; index++)
                {
                    hashCode.Add(this.Constructors[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(ConstructorList? left, ConstructorList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(ConstructorList? left, ConstructorList? right)
            => !(left == right);
    }
}
