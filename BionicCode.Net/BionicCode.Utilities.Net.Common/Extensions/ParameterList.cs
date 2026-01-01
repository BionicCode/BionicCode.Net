namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

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
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Parameters, nameof(items));

            this.DeclaringMember = this.Parameters.FirstOrDefault()?.MemberData;
            if (!this.Parameters.All(parameter => ReferenceEquals(parameter.MemberData, this.DeclaringMember)))
            {
                throw new ArgumentException("All parameters must belong to the same member.", nameof(items));
            }

            this._hashCode = ComputeHashCode(this.Parameters);
        }

        public ImmutableList<ParameterInfo> AsParameterInfoList()
            => this.Parameters
                .Select(parameterData => parameterData.GetParameterInfo())
                .ToImmutableList();

        public ImmutableArray<ParameterInfo> AsParameterInfoArray()
            => this.Parameters
                .Select(parameterData => parameterData.GetParameterInfo())
                .ToImmutableArray();

        public int Count => this.Parameters.Count;
        public bool IsEmpty => this.Parameters.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<ParameterData> Parameters { get; }
        public MemberInfoData? DeclaringMember { get; }

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
            => other != null && this.Parameters.SequenceEqual(other.Parameters)
                && this.DeclaringMember == other.DeclaringMember;

        public override bool Equals(object? obj)
            => obj is ParameterList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<ParameterData> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + HashCode.Combine(this.Count, this.DeclaringMember);
                foreach (ParameterData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
