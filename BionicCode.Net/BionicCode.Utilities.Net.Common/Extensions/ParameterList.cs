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
        public static readonly ParameterList Empty = new ParameterList();
        private readonly int _hashCode; // precomputed

        private ParameterList()
            => this.Parameters = ImmutableList<ParameterData>.Empty;

        public ParameterList(ParameterData[] items) : this((IEnumerable<ParameterData>)items)
        {
        }

        public ParameterList(IEnumerable<ParameterData> items)
        {
            this.Parameters = items.OrderBy(parameter => parameter.Position).ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNull(this.Parameters);

            if (this.HasItems)
            {
                this._declaringMember = this.Parameters.First().MemberData;
                ArgumentNullExceptionAdvanced.ThrowIfNull(
                    this._declaringMember,
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has no value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring method handle. All parameters must belong to the same member of the same declaring type.");

                RuntimeMethodHandle declaringMemberHandle = this._declaringMember.Handle;
                RuntimeTypeHandle declaringTypeHandle = this._declaringMember.DeclaringTypeHandle;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Parameters,
                    parameterData => parameterData.MemberData.Handle != declaringMemberHandle || !parameterData.MemberData.DeclaringTypeHandle.Equals(declaringTypeHandle),
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(ParameterData)}.{nameof(ParameterData.MemberData)}' declaring method handle. All parameters must belong to the same member of the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
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

        private readonly ParameterizedMemberData? _declaringMember;
        public ParameterizedMemberData DeclaringMember
            => this.IsEmpty
                ? throw new InvalidOperationException($"The '{nameof(ParameterList)}' is empty and has no declaring member.")
                : this._declaringMember!;

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
                if (!this.Parameters[index].Equals(other.Parameters[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is ParameterList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                for (int index = 0; index < this.Parameters.Count; index++)
                {
                    hashCode.Add(this.Parameters[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(ParameterList? left, ParameterList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(ParameterList? left, ParameterList? right)
            => !(left == right);
    }
}
