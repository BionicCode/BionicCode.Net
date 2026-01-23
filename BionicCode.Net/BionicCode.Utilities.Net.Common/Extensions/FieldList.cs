namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal sealed class FieldList : IReadOnlyList<FieldData>, IEquatable<FieldList>
    {
        public static readonly FieldList Empty = new FieldList();
        private readonly int _hashCode; // precomputed
        private readonly Dictionary<string, FieldData> _fieldNameIndex;

        public FieldList(FieldData[] items) : this((IEnumerable<FieldData>)items)
        {
        }

        public FieldList(IEnumerable<FieldData> items)
        {
            this.Fields = items.ToImmutableList();
            ArgumentNullExceptionAdvanced.ThrowIfNull(this.Fields, nameof(items));

            if (this.HasItems)
            {
                this._declaringTypeHandle = this.Fields.FirstOrDefault()!.DeclaringTypeHandle;
                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Fields,
                    fieldData => !fieldData.DeclaringTypeHandle.Equals(this._declaringTypeHandle),
                    nameof(items),
                    $"At least one item in the argument sequence '{nameof(items)}' has a different value for the '{nameof(FieldData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All fields must belong to the same declaring type.");

            }

            this._hashCode = ComputeHashCode();
        }

        private FieldList()
            => this.Fields = ImmutableList<FieldData>.Empty;

        public bool TryGetFieldByName(string fieldName, out FieldData? fieldData)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(fieldName);
            return this._fieldNameIndex.TryGetValue(fieldName, out fieldData!);
        }

        public int Count => this.Fields.Count;
        public bool IsEmpty => this.Fields.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<FieldData> Fields { get; }

        private readonly RuntimeTypeHandle _declaringTypeHandle;
        public RuntimeTypeHandle DeclaringTypeHandle
            => this.IsEmpty
                ? throw new InvalidOperationException($"The '{nameof(FieldList)}' is empty. Therefore the '{nameof(this.DeclaringTypeHandle)}' property is not accessible.")
                : this._declaringTypeHandle;

        public FieldData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Fields.Count, nameof(index));

                return this.Fields[index];
            }
        }

        public IEnumerator<FieldData> GetEnumerator()
            => ((IEnumerable<FieldData>)this.Fields).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Fields.GetEnumerator();

        public bool Equals(FieldList? other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.Count != other.Count)
            {
                return false;
            }

            if (!this.DeclaringTypeHandle.Equals(other.DeclaringTypeHandle))
            {
                return false;
            }

            bool isEqual = false;
            for (int index = 0; index < this.Count && !isEqual; index++)
            {
                if (!this.Fields[index].Equals(other.Fields[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is FieldList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeHandle);
                for (int index = 0; index < this.Fields.Count; index++)
                {
                    hashCode.Add(this.Fields[index]);
                }

                return hashCode.ToHashCode();
            }
        }
    }
}
