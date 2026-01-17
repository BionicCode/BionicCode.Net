namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// A read-only list of <see cref="MethodParameterInfo"/> items sorted by parameter position in ascending order.
    /// </summary>
    /// <remarks>The <see cref="MethodParameterInfo"/> items must belong to the same member of the same declaring type.
    /// This collection is not intended for a loose collection of unrelated parameters.<br/>
    /// Instead the collection is a strict representation of member parameters.</remarks>
    internal sealed class MethodParameterInfoList : IReadOnlyList<MethodParameterInfo>, IEquatable<MethodParameterInfoList>
    {
        public static readonly MethodParameterInfoList Empty = new MethodParameterInfoList();
        private readonly int _hashCode; // precomputed

        public MethodParameterInfoList(MethodParameterInfo[] items) : this((IEnumerable<MethodParameterInfo>)items)
        {
        }

        public MethodParameterInfoList(ReadOnlySpan<MethodParameterInfo> items) : this(items.ToArray())
        {
        }

        public MethodParameterInfoList(IEnumerable<MethodParameterInfo> items)
        {
            this.Parameters = items.OrderBy(parameter => parameter.Position).ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNull(this.Parameters, nameof(items));

            if (this.HasItems)
            {
                MethodParameterInfo methodParameterInfo = this.Parameters.FirstOrDefault();
                this._declaringMemberTypeHandle = methodParameterInfo.DeclaringTypeHandle;
                Type declaringType = Type.GetTypeFromHandle(methodParameterInfo.ParameterTypeHandle)
                    ?? throw new ArgumentException($"The argument '{nameof(items)}' contains an invalid item at position '0'. Reason: Could not resolve type from handle 'ParameterTypeHandle'.");
                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Parameters,
                    parameterData => !parameterData.DeclaringTypeHandle.Equals(this._declaringMemberTypeHandle),
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodParameterInfo)}.{nameof(MethodParameterInfo.DeclaringTypeHandle)}' declaring type handle. All parameters must belong to the same member of the same declaring type.");

            }

            this._hashCode = ComputeHashCode();
        }

        private MethodParameterInfoList()
            => this.Parameters = ImmutableList<MethodParameterInfo>.Empty;

        public int Count => this.Parameters.Count;
        public bool IsEmpty => this.Parameters.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<MethodParameterInfo> Parameters { get; }

        private readonly RuntimeTypeHandle _declaringMemberTypeHandle;
        public RuntimeTypeHandle DeclaringMemberTypeHandle
            => this.IsEmpty
                ? throw new InvalidOperationException($"The collection is empty and has no '{nameof(this.DeclaringMemberTypeHandle)}'.")
                : this._declaringMemberTypeHandle;

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
        {
            if (other is null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            if (!this.DeclaringMemberTypeHandle.Equals(other.DeclaringMemberTypeHandle))
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
            => obj is MethodParameterInfoList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringMemberTypeHandle);
                for (int index = 0; index < this.Parameters.Count; index++)
                {
                    hashCode.Add(this.Parameters[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(MethodParameterInfoList? left, MethodParameterInfoList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(MethodParameterInfoList? left, MethodParameterInfoList? right)
            => !(left == right);
    }
}
