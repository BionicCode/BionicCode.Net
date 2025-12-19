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
            ArgumentNullExceptionEx.ThrowIfNullOrEmpty(this.Parameters, nameof(items));

            this.DeclaringMemberTypeHandle = this.Parameters.FirstOrDefault().DeclaringTypeHandle;
            if (!this.Parameters.All(parameter => parameter.DeclaringTypeHandle.Equals(this.DeclaringMemberTypeHandle)))
            {
                throw new ArgumentException("All parameters must belong to the same member.", nameof(items));
            }

            this._hashCode = ComputeHashCode(this.Parameters);
        }

        public int Count => this.Parameters.Count;
        public bool IsEmpty => this.Parameters.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<MethodParameterInfo> Parameters { get; }
        public RuntimeTypeHandle DeclaringMemberTypeHandle { get; }

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
            => other != null && this.Parameters.SequenceEqual(other.Parameters)
                && this.DeclaringMemberTypeHandle.Equals(other.DeclaringMemberTypeHandle);

        public override bool Equals(object? obj)
            => obj is MethodParameterInfoList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<MethodParameterInfo> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + this.DeclaringMemberTypeHandle.GetHashCode();
                foreach (MethodParameterInfo item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
