namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class FieldList : IReadOnlyList<FieldData>, IEquatable<FieldList>
    {
        public static readonly FieldList Empty = new FieldList(Array.Empty<FieldData>());
        private readonly int _hashCode; // precomputed

        public FieldList(FieldData[] items) : this((IEnumerable<FieldData>)items)
        {
        }

        public FieldList(IEnumerable<FieldData> items)
        {
            this.Fields = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Fields, nameof(items));
            this.DeclaringTypeHandle = this.Fields.FirstOrDefault()!.DeclaringTypeHandle;
            if (!this.Fields.All(field => field.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle)))
            {
                throw new ArgumentException("All fields must belong to the same declaring type.", nameof(items));
            }

            this._hashCode = ComputeHashCode(this.Fields);
        }

        public int Count => this.Fields.Count;
        public bool IsEmpty => this.Fields.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<FieldData> Fields { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public FieldData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Fields.Count, nameof(index));

                return this.Fields[index];
            }
        }

        public IEnumerator<FieldData> GetEnumerator()
            => ((IEnumerable<FieldData>)this.Fields).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Fields.GetEnumerator();

        public bool Equals(FieldList? other)
            => other != null && this.Fields.SequenceEqual(other.Fields)
                && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle);

        public override bool Equals(object? obj)
            => obj is FieldList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<FieldData> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + this.DeclaringTypeHandle.GetHashCode();
                foreach (FieldData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
