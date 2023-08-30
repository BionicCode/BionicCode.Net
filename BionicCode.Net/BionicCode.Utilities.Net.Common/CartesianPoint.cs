namespace BionicCode.Utilities.Net
{
  using System;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;

  /// <summary>
  /// Point - Defaults to 0,0
  /// </summary>
  public struct CartesianPoint : IEquatable<CartesianPoint>, IComparable<CartesianPoint>, IFormattable
  {
    /// <summary>
    /// Constructor which accepts the X and Y values
    /// </summary>
    /// <param name="x">The value for the X coordinate of the new Point</param>
    /// <param name="y">The value for the Y coordinate of the new Point</param>
    public CartesianPoint(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    /// <summary>
    /// Offset - update the location by adding offsetX to X and offsetY to Y
    /// </summary>
    /// <param name="xOffset"> The offset in the x dimension </param>
    /// <param name="yOffset"> The offset in the y dimension </param>
    public void Offset(double xOffset, double yOffset)
    {
      this.X += xOffset;
      this.Y += yOffset;
    }

    /// <summary>
    /// The X value of the point. The default value is '0'.
    /// </summary>
    public double X { get; private set; }

    /// <summary>
    /// The Y value of the point. The default value is '0'.
    /// </summary>
    public double Y { get; private set; }

    /// <inheritdoc/>
    public bool Equals(CartesianPoint other) => other.X == this.X && other.Y == this.Y;

    /// <inheritdoc/>
    public override bool Equals(object obj) => Equals((CartesianPoint)obj);

    /// <inheritdoc/>
    public override int GetHashCode()
#if NET || NETSTANDARD2_1_OR_GREATER
       => HashCode.Combine(this.X, this.Y);
#else
    {
      int hashCode = 1861411795;
      hashCode = hashCode * -1521134295 + this.X.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Y.GetHashCode();
      return hashCode;
    }
#endif

    /// <inheritdoc/>
    public override string ToString() => ConvertToString(null, null);
    /// <inheritdoc/>
    public string ToString(string format, IFormatProvider formatProvider) => ConvertToString(format, formatProvider);

    private string ConvertToString(string formatString, IFormatProvider formatProvider) 
      => string.Format(formatProvider, $"{{1:{formatString}}}{{0}}{{2:{formatString}}}", ",", this.X, this.Y);

    /// <inheritdoc/>
    /// <remarks>This method compares the <see cref="X"/> value. When the <see cref="X"/> value of the current instance is smaller the value of <paramref name="other"/> then the current instance precedes <paramref name="other"/>.</remarks>
    public int CompareTo(CartesianPoint other) => this.X.CompareTo(other.X);

    /// <summary>
    /// Compares two Point instances for exact equality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which are logically equal may fail.
    /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
    /// </summary>
    /// <returns>
    /// bool - true if the two Point instances are exactly equal, false otherwise
    /// </returns>
    /// <param name='left'>The first Point to compare</param>
    /// <param name='right'>The second Point to compare</param>
    public static bool operator ==(CartesianPoint left, CartesianPoint right) => left.Equals(right);
    /// <summary>
    /// Compares two Point instances for exact equality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which are logically equal may fail.
    /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
    /// </summary>
    /// <returns>
    /// bool - true if the two Point instances are exactly equal, false otherwise
    /// </returns>
    /// <param name='left'>The first Point to compare</param>
    /// <param name='right'>The second Point to compare</param>
    public static bool operator !=(CartesianPoint left, CartesianPoint right) => !(left == right);

    public static bool operator <(CartesianPoint left, CartesianPoint right) => left.CompareTo(right) < 0;
    public static bool operator <=(CartesianPoint left, CartesianPoint right) => left.CompareTo(right) <= 0;
    public static bool operator >(CartesianPoint left, CartesianPoint right) => left.CompareTo(right) > 0;
    public static bool operator >=(CartesianPoint left, CartesianPoint right) => left.CompareTo(right) >= 0;
  }
}
