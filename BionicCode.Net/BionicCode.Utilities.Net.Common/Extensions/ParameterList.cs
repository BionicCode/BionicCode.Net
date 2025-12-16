namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class ParameterList : IReadOnlyList<ParameterData>, IEquatable<ParameterList>
    {
        public static readonly ParameterList Empty = new ParameterList(Array.Empty<ParameterData>());
        private readonly int _hashCode; // precomputed

        public ParameterList(ParameterData[] items) : this((IEnumerable<ParameterData>)items)
        {
        }

        public ParameterList(IEnumerable<ParameterData> items)
        {
            this.Parameters = items.ToImmutableList();
            this.GenericMethodParameters = this.Parameters
                .Where(parameterData => parameterData.IsGenericMethodParameter)
                .ToImmutableList();
            this._hashCode = ComputeHashCode(this.Parameters);
        }

        public int Count => this.Parameters.Count;
        public int GenericMethodParameterCount => this.GenericMethodParameters.Count;
        public bool IsEmpty => this.Parameters.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<ParameterData> Parameters { get; }
        public ImmutableList<ParameterData> GenericMethodParameters { get; }

        public ParameterData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Parameters.Count, nameof(index));

                return this.Parameters[index];
            }
        }

        public IEnumerator<ParameterData> GetEnumerator()
            => ((IEnumerable<ParameterData>)this.Parameters).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Parameters.GetEnumerator();

        public bool Equals(ParameterList? other)
            => other != null && this.Parameters.SequenceEqual(other.Parameters);

        public override bool Equals(object? obj)
            => obj is ParameterList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private static int ComputeHashCode(IEnumerable<ParameterData> items)
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
