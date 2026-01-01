namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class ConstructorList : IReadOnlyList<ConstructorData>, IEquatable<ConstructorList>
    {
        public static readonly ConstructorList Empty = new ConstructorList(Array.Empty<ConstructorData>());
        private readonly int _hashCode; // precomputed

        public ConstructorList(ConstructorData[] items) : this((IEnumerable<ConstructorData>)items)
        {
        }

        public ConstructorList(IEnumerable<ConstructorData> items)
        {
            this.Constructors = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Constructors, nameof(items));

            this.DeclaringTypeHandle = this.Constructors.FirstOrDefault()!.DeclaringTypeHandle;
            if (!this.Constructors.All(method => method.DeclaringTypeData.Equals(this.DeclaringTypeHandle)))
            {
                throw new ArgumentException("All constructors must belong to the same declaring type.", nameof(items));
            }

            this._hashCode = ComputeHashCode(this.Constructors);
        }

        public int Count => this.Constructors.Count;
        public bool IsEmpty => this.Constructors.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<ConstructorData> Constructors { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public ConstructorData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Constructors.Count, nameof(index));

                return this.Constructors[index];
            }
        }

        public IEnumerator<ConstructorData> GetEnumerator()
            => ((IEnumerable<ConstructorData>)this.Constructors).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Constructors.GetEnumerator();

        public bool Equals(ConstructorList? other)
            => other != null && this.Constructors.SequenceEqual(other.Constructors)
                && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle);

        public override bool Equals(object? obj)
            => obj is ConstructorList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<ConstructorData> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + HashCode.Combine(this.Count, this.DeclaringTypeHandle);
                foreach (ConstructorData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
