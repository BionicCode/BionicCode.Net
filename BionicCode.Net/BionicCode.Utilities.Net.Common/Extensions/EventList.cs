namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class EventList : IReadOnlyList<EventData>, IEquatable<EventList>
    {
        public static readonly EventList Empty = new EventList(Array.Empty<EventData>());
        private readonly int _hashCode; // precomputed

        public EventList(EventData[] items) : this((IEnumerable<EventData>)items)
        {
        }

        public EventList(IEnumerable<EventData> items)
        {
            this.Events = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Events, nameof(items));

            this.DeclaringTypeHandle = this.Events.FirstOrDefault()!.DeclaringTypeHandle;
            if (!this.Events.All(property => property.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)))
            {
                throw new ArgumentException("All events must belong to the same declaring type.", nameof(items));
            }

            this._hashCode = ComputeHashCode(this.Events);
        }

        public int Count => this.Events.Count;
        public bool IsEmpty => this.Events.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<EventData> Events { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public EventData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Events.Count, nameof(index));

                return this.Events[index];
            }
        }

        public IEnumerator<EventData> GetEnumerator()
            => ((IEnumerable<EventData>)this.Events).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Events.GetEnumerator();

        public bool Equals(EventList? other)
            => other != null && this.Events.SequenceEqual(other.Events)
                && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle);

        public override bool Equals(object? obj)
            => obj is EventList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<EventData> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + this.DeclaringTypeHandle.GetHashCode();
                foreach (EventData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
