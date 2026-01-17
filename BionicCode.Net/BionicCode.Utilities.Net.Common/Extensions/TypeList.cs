namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    internal sealed class TypeList : IReadOnlyList<TypeData>, IEquatable<TypeList>
    {
        public static readonly TypeList Empty = new TypeList();
        private readonly int _hashCode; // precomputed

        public TypeList(TypeData[] items) : this((IEnumerable<TypeData>)items)
        {
        }

        public TypeList(IEnumerable<TypeData> items)
        {
            this.Types = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNull(this.Types, nameof(items));
            this._hashCode = ComputeHashCode();
        }

        private TypeList()
            => this.Types = ImmutableList<TypeData>.Empty;

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
        {
            if (other is null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            for (int index = 0; index < this.Count; index++)
            {
                if (!this.Types[index].Equals(other.Types[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is TypeList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                for (int index = 0; index < this.Types.Count; index++)
                {
                    hashCode.Add(this.Types[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(TypeList? left, TypeList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(TypeList? left, TypeList? right)
            => !(left == right);
    }
}
