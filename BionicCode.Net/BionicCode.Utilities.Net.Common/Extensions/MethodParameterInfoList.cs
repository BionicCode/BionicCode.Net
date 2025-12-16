namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class MethodParameterInfoList : IReadOnlyList<MethodParameterInfo>, IEquatable<MethodParameterInfoList>
    {
        public static readonly MethodParameterInfoList Empty = new MethodParameterInfoList(Array.Empty<MethodParameterInfo>());
        private readonly int _hashCode; // precomputed

        public MethodParameterInfoList(MethodParameterInfo[] items) : this((IEnumerable<MethodParameterInfo>)items)
        {
        }

        public MethodParameterInfoList(IEnumerable<MethodParameterInfo> items)
        {
            this.Parameters = items.ToImmutableList();
            this.GenericMethodParameters = this.Parameters
                .Where(parameter => parameter.IsGenericMethodParameter)
                .ToImmutableList();
            this._hashCode = ComputeHashCode(this.Parameters);
        }

        public int Count => this.Parameters.Count;
        public int GenericTypeParameterCount => this.GenericMethodParameters.Count;
        public bool IsEmpty => this.Parameters.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<MethodParameterInfo> Parameters { get; }
        public ImmutableList<MethodParameterInfo> GenericMethodParameters { get; }

        public MethodParameterInfo this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Parameters.Count, nameof(index));

                return this.Parameters[index];
            }
        }

        public IEnumerator<MethodParameterInfo> GetEnumerator()
            => ((IEnumerable<MethodParameterInfo>)this.Parameters).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Parameters.GetEnumerator();

        public bool Equals(MethodParameterInfoList? other)
            => other != null && this.Parameters.SequenceEqual(other.Parameters);

        public override bool Equals(object? obj)
            => obj is MethodParameterInfoList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private static int ComputeHashCode(IEnumerable<MethodParameterInfo> items)
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
