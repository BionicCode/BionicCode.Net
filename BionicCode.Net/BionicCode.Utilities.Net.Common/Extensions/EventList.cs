namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class EventList : IReadOnlyList<EventData>, IEquatable<EventList>
    {
        public static readonly EventList Empty = new EventList();
        private readonly int _hashCode; // precomputed
        private readonly SymbolInfoDataCacheKey _declaringTypeCacheKey;
        private readonly Dictionary<string, EventData> _eventNameIndex;

        public EventList(EventData[] items) : this((IEnumerable<EventData>)items)
        {
        }

        public EventList(IEnumerable<EventData> items)
        {
            this.Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
            this._eventNameIndex = this.Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

            if (this.HasItems)
            {
                this._declaringTypeCacheKey = this.Events.First().DeclaringTypeData.CacheKey;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Events,
                    eventData => eventData.DeclaringTypeData.CacheKey != this._declaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

            }

            this._hashCode = ComputeHashCode();
        }

        internal EventList(IEnumerable<EventData> items, bool isIntegrityValidationEnabled)
        {
            this.Events = items?.ToImmutableList() ?? ImmutableList<EventData>.Empty;
            this._eventNameIndex = this.Events.ToDictionary(eventData => eventData.Name, StringComparer.Ordinal);

            if (isIntegrityValidationEnabled && this.HasItems)
            {
                this._declaringTypeCacheKey = this.Events.First().DeclaringTypeData.CacheKey;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Events,
                    eventData => eventData.DeclaringTypeData.CacheKey != this._declaringTypeCacheKey,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

            }

            this._hashCode = ComputeHashCode();
        }

        private EventList()
        {
            this.Events = ImmutableList<EventData>.Empty;
            this._eventNameIndex = new Dictionary<string, EventData>(0, StringComparer.Ordinal);
        }

        public bool TryGetEventByName(string eventName, out EventData? eventData)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(eventName);
            return this._eventNameIndex.TryGetValue(eventName, out eventData);
        }

        public int Count => this.Events.Count;
        public bool IsEmpty => this.Events.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<EventData> Events { get; }
        public SymbolInfoDataCacheKey DeclaringTypeCacheKey
            => this.HasItems
                ? this._declaringTypeCacheKey
                : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeCacheKey)));

        public TypeData DeclaringTypeData
        {
            get
            {
                SymbolInfoDataCacheKey cacheKey = this.DeclaringTypeCacheKey;
                return this.HasItems
                    ? SymbolReflectionInfoCache.GetOrCreateTypeDataCacheEntry(ref cacheKey)
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(MethodList), nameof(this.DeclaringTypeData)));
            }
        }

        public EventData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Events.Count, nameof(index));

                return this.HasItems
                    ? this.Events[index]
                    : throw new InvalidOperationException(ExceptionMessages.GetInvalidAccessCollectionEmptyExceptionMessage(nameof(EventList), ReflectionConstants.IndexerGetMethodName));
            }
        }

        public IEnumerator<EventData> GetEnumerator()
            => ((IEnumerable<EventData>)this.Events).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Events.GetEnumerator();

        public bool Equals(EventList? other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            if (!this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle))
            {
                return false;
            }

            for (int index = 0; index < this.Count; index++)
            {
                if (!this.Events[index].Equals(other.Events[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is EventList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeHandle);
                for (int index = 0; index < this.Events.Count; index++)
                {
                    hashCode.Add(this.Events[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(EventList? left, EventList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(EventList? left, EventList? right)
            => !(left == right);
    }
}
