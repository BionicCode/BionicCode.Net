namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A read-only list of <see cref="ParameterData"/> items sorted by parameter position in ascending order.
    /// </summary>
    /// <remarks>The <see cref="ParameterData"/> items must belong to the same member of the same declaring type.
    /// This collection is not intended for a loose collection of unrelated parameters.<br/>
    /// Instead the collection is a strict representation of member parameters.</remarks>
    internal sealed class ParameterList : IReadOnlyList<ParameterData>, IEquatable<ParameterList>
    {
        public static readonly ParameterList Empty = new ParameterList(Array.Empty<ParameterData>());
        private readonly int _hashCode; // precomputed

        public ParameterList(ParameterData[] items) : this((IEnumerable<ParameterData>)items)
        {
        }

        public ParameterList(IEnumerable<ParameterData> items)
        {
            this.Parameters = items.OrderBy(parameter => parameter.Position).ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Parameters, nameof(items));

            this.DeclaringMember = this.Parameters.FirstOrDefault()?.MemberData;
            RuntimeTypeHandle declaringTypeHandle = this.DeclaringMember is MemberData memberData
                ? memberData.DeclaringTypeHandle
                : this.DeclaringMember is TypeData type
                    ? type.Handle
                    : this.DeclaringMember is ParameterData parameterData
                        ? parameterData.DeclaringTypeHandle
                        : throw new NotImplementedException($"The support for the declaring member '{this.DeclaringMember}' is currently not implemented.");

            ArgumentExceptionAdvanced.ThrowIfAny(this.Parameters, parameterData => !parameterData.DeclaringTypeHandle.Equals(declaringTypeHandle), nameof(items), $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ParameterData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type.");

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
        public SymbolInfoData? DeclaringMember { get; }

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
