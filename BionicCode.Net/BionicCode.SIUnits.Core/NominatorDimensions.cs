namespace BionicCode.SIUnits.Core
{
    using System.Diagnostics.CodeAnalysis;

    public class NominatorDimensions : List<DimensionInfo>
    {
    }

    public readonly struct DimensionInfo : IEquatable<DimensionInfo>
    {
        public DimensionInfo(SIBaseDimension siBaseDimension, int exponent)
        {
            this.SIBaseDimension = siBaseDimension;
            this.Exponent = exponent;
        }

        public SIBaseDimension SIBaseDimension { get; }
        public int Exponent { get; }

        public bool Equals(DimensionInfo other)
            => this.SIBaseDimension == other.SIBaseDimension && this.Exponent == other.Exponent;

        public override bool Equals([NotNullWhen(true)] object? obj)
            => obj is DimensionInfo other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(this.SIBaseDimension, this.Exponent);

        public override string? ToString() => base.ToString();

        public static bool operator ==(DimensionInfo left, DimensionInfo right)
            => left.Equals(right);

        public static bool operator !=(DimensionInfo left, DimensionInfo right)
            => !(left == right);
    }
}