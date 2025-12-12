namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class MethodParameterInfoList : IReadOnlyList<MethodParameterInfo>, IEquatable<MethodParameterInfoList>
    {
        public static readonly MethodParameterInfoList Empty = new MethodParameterInfoList(Array.Empty<MethodParameterInfo>());
        private readonly MethodParameterInfo[] _items;
        private readonly int _hashCode; // precomputed
        private ImmutableList<MethodParameterInfo>? genericTypeParameters;

        public MethodParameterInfoList(MethodParameterInfo[] items)
        {
            this._items = items ?? Array.Empty<MethodParameterInfo>();
            this._hashCode = ComputeHashCode(this._items);
        }

        public MethodParameterInfoList(IEnumerable<MethodParameterInfo> items)
        {
            this._items = items?.ToArray() ?? Array.Empty<MethodParameterInfo>();
            this._hashCode = ComputeHashCode(this._items);
        }

        public int Count => this._items.Length;
        public int GenericTypeParameterCount => this.GenericTypeParameters.Count;
        public bool IsEmpty => this._items.Length == 0;
        public bool HasItems => this._items.Length > 0;
        public ImmutableList<MethodParameterInfo> GenericTypeParameters
            => this.genericTypeParameters ??= ImmutableList.CreateRange(this._items.Where(methodParameterInfo => methodParameterInfo.IsGenericTypeParameter));

        public MethodParameterInfo this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this._items.Length, nameof(index));

                return this._items[index];
            }
        }

        public IEnumerator<MethodParameterInfo> GetEnumerator()
            => ((IEnumerable<MethodParameterInfo>)this._items).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this._items.GetEnumerator();

        public bool Equals(MethodParameterInfoList? other)
            => other != null && this._items.SequenceEqual(other._items);

        public override bool Equals(object? obj)
            => obj is MethodParameterInfoList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private static int ComputeHashCode(MethodParameterInfo[] items)
        {
            unchecked
            {
                int hash = 17;
                foreach (MethodParameterInfo item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
