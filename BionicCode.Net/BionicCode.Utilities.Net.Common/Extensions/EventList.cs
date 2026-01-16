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

            ArgumentExceptionAdvanced.ThrowIfAny(
                this.Events,
                eventData => !eventData.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle),
                nameof(items),
                $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(EventData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All events must belong to the same declaring type.");

            this._hashCode = ComputeHashCode();
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

            bool isEqual = false;
            for (int index = 0; index < this.Count && !isEqual; index++)
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
