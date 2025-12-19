namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class TypeList : IReadOnlyList<TypeData>, IEquatable<TypeList>
    {
        public static readonly TypeList Empty = new TypeList(Array.Empty<TypeData>());
        private readonly int _hashCode; // precomputed

        public TypeList(TypeData[] items) : this((IEnumerable<TypeData>)items)
        {
        }

        public TypeList(IEnumerable<TypeData> items)
        {
            this.Types = items.ToImmutableList();
            ArgumentNullExceptionEx.ThrowIfNullOrEmpty(this.Types, nameof(items));
            this._hashCode = ComputeHashCode(this.Types);
        }

        public int Count => this.Types.Count;
        public bool IsEmpty => this.Types.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<TypeData> Types { get; }

        public TypeData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Types.Count, nameof(index));

                return this.Types[index];
            }
        }

        public IEnumerator<TypeData> GetEnumerator()
            => ((IEnumerable<TypeData>)this.Types).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Types.GetEnumerator();

        public bool Equals(TypeList? other)
            => other != null && this.Types.SequenceEqual(other.Types);

        public override bool Equals(object? obj)
            => obj is TypeList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private static int ComputeHashCode(IEnumerable<TypeData> items)
        {
            unchecked
            {
                int hash = 17;
                foreach (TypeData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
