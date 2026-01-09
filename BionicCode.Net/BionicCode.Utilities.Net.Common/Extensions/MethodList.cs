namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class MethodList : IReadOnlyList<MethodData>, IEquatable<MethodList>
    {
        public static readonly MethodList Empty = new MethodList(Array.Empty<MethodData>());
        private readonly int _hashCode; // precomputed

        public MethodList(MethodData[] items) : this((IEnumerable<MethodData>)items)
        {
        }

        public MethodList(IEnumerable<MethodData> items)
        {
            this.Methods = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Methods, nameof(items));

            this.DeclaringTypeHandle = this.Methods.FirstOrDefault()!.DeclaringTypeHandle;

            ArgumentExceptionAdvanced.ThrowIfAny(this.Methods, methodData => !methodData.DeclaringTypeHandle.Equals(this.DeclaringTypeHandle), nameof(items), $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(MethodData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All methods must belong to the same declaring type.");

            this._hashCode = ComputeHashCode(this.Methods);
        }

        public int Count => this.Methods.Count;
        public bool IsEmpty => this.Methods.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<MethodData> Methods { get; }
        public RuntimeTypeHandle DeclaringTypeHandle { get; }

        public MethodData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Methods.Count, nameof(index));

                return this.Methods[index];
            }
        }

        public IEnumerator<MethodData> GetEnumerator()
            => ((IEnumerable<MethodData>)this.Methods).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Methods.GetEnumerator();

        public bool Equals(MethodList? other)
            => other != null && this.Methods.SequenceEqual(other.Methods)
                && this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle);

        public override bool Equals(object? obj)
            => obj is MethodList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode(IEnumerable<MethodData> items)
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + HashCode.Combine(this.Count, this.DeclaringTypeHandle);
                foreach (MethodData item in items)
                {
                    hash = (hash * 31) + item.GetHashCode();
                }

                return hash;
            }
        }
    }
}
