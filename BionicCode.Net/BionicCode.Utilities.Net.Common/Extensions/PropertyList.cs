namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    /// <summary>
    /// Represents a read-only list of <see cref="PropertyData"/> items that belong to the same declaring type.
    /// </summary>
    internal sealed class PropertyList : IReadOnlyList<PropertyData>, IEquatable<PropertyList>
    {
        public static readonly PropertyList Empty = new PropertyList();
        private readonly int _hashCode; // precomputed

        public PropertyList(PropertyData[] items) : this((IEnumerable<PropertyData>)items)
        {
        }

        public PropertyList(IEnumerable<PropertyData> items)
        {
            this.Properties = items.ToImmutableList();

            ArgumentNullExceptionAdvanced.ThrowIfNullOrEmpty(this.Properties, nameof(items));

            if (this.HasItems)
            {
                this._declaringTypeHandle = this.Properties.FirstOrDefault()!.DeclaringTypeHandle;

                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Properties,
                    property => !property.DeclaringTypeHandle.Equals(this._declaringTypeHandle),
                    nameof(items),
                    $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
        }

        internal PropertyList(IEnumerable<PropertyData> items, bool isIntegrityValidationEnabled)
        {
            this.Properties = items?.ToImmutableList() ?? ImmutableList<PropertyData>.Empty;
            this._declaringTypeHandle = this.Properties.FirstOrDefault()?.DeclaringTypeHandle ?? default;

            if (isIntegrityValidationEnabled && this.HasItems)
            {
                ArgumentExceptionAdvanced.ThrowIfAny(
                    this.Properties,
                    property => !property.DeclaringTypeHandle.Equals(this._declaringTypeHandle),
                    nameof(items),
                    $"At least one item in the argument '{nameof(items)}' has a different value for the '{nameof(PropertyData)}.{nameof(MemberData.DeclaringTypeHandle)}' declaring type handle. All properties must belong to the same declaring type.");
            }

            this._hashCode = ComputeHashCode();
        }

        private PropertyList()
            => this.Properties = ImmutableList<PropertyData>.Empty;

        public int Count => this.Properties.Count;
        public bool IsEmpty => this.Properties.IsEmpty;
        public bool HasItems => !this.IsEmpty;
        public ImmutableList<PropertyData> Properties { get; }

        private readonly RuntimeTypeHandle _declaringTypeHandle;
        public RuntimeTypeHandle DeclaringTypeHandle
            => this.IsEmpty
                ? throw new InvalidOperationException($"The '{nameof(PropertyList)}' is empty and has no '{nameof(this.DeclaringTypeHandle)}'.")
                : this._declaringTypeHandle;

        public PropertyData this[int index]
        {
            get
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, this.Properties.Count, nameof(index));

                return this.Properties[index];
            }
        }

        public IEnumerator<PropertyData> GetEnumerator()
            => ((IEnumerable<PropertyData>)this.Properties).GetEnumerator();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            => this.Properties.GetEnumerator();

        public bool Equals(PropertyList? other)
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

            for (int index = 0; index < this.Count; index++)
            {
                if (!this.Properties[index].Equals(other.Properties[index]))
                {
                    return false;
                }
            }

            return true;
        }

        public override bool Equals(object? obj)
            => obj is PropertyList other && Equals(other);

        public override int GetHashCode() => this._hashCode;

        private int ComputeHashCode()
        {
            unchecked
            {
                var hashCode = new HashCode();
                hashCode.Add(this.Count);
                hashCode.Add(this.DeclaringTypeHandle);
                for (int index = 0; index < this.Properties.Count; index++)
                {
                    hashCode.Add(this.Properties[index]);
                }

                return hashCode.ToHashCode();
            }
        }

        public static bool operator ==(PropertyList? left, PropertyList? right)
            => left?.Equals(right) ?? (right is null);
        public static bool operator !=(PropertyList? left, PropertyList? right)
            => !(left == right);
    }
}
