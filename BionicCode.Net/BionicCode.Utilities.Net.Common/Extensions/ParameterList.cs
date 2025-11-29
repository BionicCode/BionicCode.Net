namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParameterList : IReadOnlyList<ParameterData>, IEquatable<ParameterList>
    {
        public static readonly ParameterList Empty = new ParameterList(Array.Empty<ParameterData>());
        private readonly ParameterData[] _items;
        private readonly int _hashCode; // precomputed

        public ParameterList(ParameterData[] items)
        {
            this._items = items ?? Array.Empty<ParameterData>();
            this._hashCode = ComputeHashCode(this._items);
        }

        public int Count => this._items.Length;

        public ParameterData this[int index] => this._items[index];

        public IEnumerator<ParameterData> GetEnumerator()
            => ((IEnumerable<ParameterData>)this._items).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this._items.GetEnumerator();

        public bool Equals(ParameterList? other)
            => other != null && this._items.SequenceEqual(other._items);

        public override bool Equals(object? obj)
            => obj is ParameterList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private static int ComputeHashCode(ParameterData[] items)
        {
            unchecked
            {
                int hash = 17;
                foreach (ParameterData item in items)
                {
                    hash *= 31 + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
