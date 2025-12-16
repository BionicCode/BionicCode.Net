namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class ParameterList : IReadOnlyList<ParameterData>, IEquatable<ParameterList>
    {
        public static readonly ParameterList Empty = new ParameterList(Array.Empty<ParameterData>());
        private readonly ParameterData[] _items;
        private readonly int _hashCode; // precomputed
        private ImmutableList<ParameterData>? genericTypeParameters;

        public ParameterList(ParameterData[] items)
        {
            this._items = items?.ToArray() ?? Array.Empty<ParameterData>();
            this._hashCode = ComputeHashCode(this._items);
        }

        public ParameterList(IEnumerable<ParameterData> items)
        {
            this._items = items?.ToArray() ?? Array.Empty<ParameterData>();
            this._hashCode = ComputeHashCode(this._items);
        }

        public int Count => this._items.Length;
        public int GenericTypeParameterCount => this.GenericMethodParameters.Count;
        public bool IsEmpty => this._items.Length == 0;
        public bool HasItems => this._items.Length > 0;
        public ImmutableList<ParameterData> GenericMethodParameters
            => this.genericTypeParameters ??= ImmutableList.CreateRange(this._items.Where(parameterData => parameterData.IsGenericMethodParameter));

        public ParameterData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._items.Length, nameof(index));

                return this._items[index];
            }
        }

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
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
