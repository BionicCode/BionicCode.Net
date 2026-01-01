namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class PropertyList : IReadOnlyList<PropertyData>, IEquatable<PropertyList>
    {
        public static readonly PropertyList Empty = new PropertyList(Array.Empty<PropertyData>());
        private readonly int _hashCode; // precomputed

        public PropertyList(PropertyData[] items) : this((IEnumerable<PropertyData>)items)
        {
        }

        public PropertyList(IEnumerable<PropertyData> items)
        {
            this.Properties = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Properties, nameof(items));

            this.DeclaringTypeHandle = this.Properties.FirstOrDefault()!.DeclaringTypeHandle;
            if (!this.Properties.All(property => property.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)))
            {
                throw new ArgumentException("All properties must belong to the same declaring type.", nameof(items));
            }

            this._hashCode = ComputeHashCode(this.Properties);
        }

        public int Count => this.Properties.Count;
        public bool IsEmpty => this.Properties.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<PropertyData> Properties { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public PropertyData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Properties.Count, nameof(index));

                return this.Properties[index];
            }
        }

        public IEnumerator<PropertyData> GetEnumerator()
            => ((IEnumerable<PropertyData>)this.Properties).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Properties.GetEnumerator();

        public bool Equals(PropertyList? other)
            => other != null && this.Properties.SequenceEqual(other.Properties)
                && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle);

        public override bool Equals(object? obj)
            => obj is PropertyList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<PropertyData> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + this.DeclaringTypeHandle.GetHashCode();
                foreach (PropertyData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
