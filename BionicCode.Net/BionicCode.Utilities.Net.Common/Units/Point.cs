namespace BionicCode.Utilities.Net
{
  using System;

  /// <summary>
  /// The unit that the PDF system uses (or typographical unit in general). 
  /// <br/>
  /// In the PDF coordinate system 1 pt is: 1 in for 72 pt (1 pt = 1/72 in)
  /// which is equal to 72 pt for 1 in (1 in = 72 pt).
  /// <para/>
  /// In the PDF coordinate system 1 PDF unit is: 25,4 mm for 72 pt (1 PDF unit = 25.4/72 mm)
  /// which is equal to 72 pt for 25.4 mm (1 mm = 72/25.4 = 2.835 pt).
  /// </summary>
  public readonly struct Point : IEquatable<Point>, IConvertible, IComparable<Point>, IComparable
  {
    public Point(double value)
    {
      this.Value = value;
      this.Unit = LengthUnit.Point;
    }

    public static Point ToPoint(double value, LengthUnit unit)
    {
      switch(unit)
      {
        case LengthUnit.Default:
        case LengthUnit.Millimeter:
          return Point.FromMillimeter(value);
        case LengthUnit.Inch:
          return Point.FromInch(value);
        case LengthUnit.Point: 
          return value;
        case LengthUnit.Pixel: return Point.FromPixel(value);
        default:
          throw new NotImplementedException();
      }
    }

    public double ToUnit(LengthUnit unit)
    {
      switch (unit)
      {
        case LengthUnit.Default:
        case LengthUnit.Millimeter:
          return Millimeter.FromPoint(this.Value);
        case LengthUnit.Inch:
          return Inch.FromPoint(this.Value);
        case LengthUnit.Point:
          return this.Value;
        case LengthUnit.Pixel:
          return Pixel.FromPoint(this.Value);
        default:
          throw new NotImplementedException();
      }
    }

    public Millimeter ToMillimeter()
      => this.Value * Millimeter.MillimetersPerPoint;

    public Inch ToInch()
      => this.Value * Inch.InchesPerPoint;

    public Pixel ToPixel()
      => this.Value * (Inch.InchesPerPoint / Inch.InchesPerPixel);

    public static Point FromMillimeter(double millimeter)
      => new Millimeter(millimeter);

    public static Point FromInch(double inch)
      => new Inch(inch);

    public static Point FromPixel(double pixel)
      => new Pixel(pixel);

    public override string ToString() => this.Value.ToString();

    public double Value { get; }

    public LengthUnit Unit { get; }

    public bool Equals(Point other) => this.Value.Equals(other.Value);

    /// <inheritdoc/>
#if NET || NETSTANDARD2_1_OR_GREATER
    public override int GetHashCode() => HashCode.Combine(this.Value, this.Unit);
#else
    public override int GetHashCode()
    {
      int hashCode = -177567199;
      hashCode = (hashCode * -1521134295) + this.Value.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.Unit.GetHashCode();
      return hashCode;
    }
#endif

    public override bool Equals(object obj) => obj is Point value && Equals(value);
    
    #region IComparable
    
    public int CompareTo(Point other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Pixel other) => CompareTo(other.ToPoint());
    public int CompareTo(Inch other) => CompareTo(other.ToPoint());
    public int CompareTo(Millimeter other) => CompareTo(other.ToPoint());

    int IComparable.CompareTo(object obj) => obj is Point point 
      ? CompareTo(point) 
      : obj is Pixel pixel
        ? CompareTo(pixel)
        : obj is Inch inch
          ? CompareTo(inch)
          : obj is Millimeter millimeter
            ? CompareTo(millimeter)
            : throw new ArgumentException("Unable to compare the provided type.", nameof(obj)); 
    
    #endregion IComparable

    #region IConvertible

    TypeCode IConvertible.GetTypeCode() => throw new NotImplementedException();
    bool IConvertible.ToBoolean(IFormatProvider provider) => throw new InvalidCastException();
    char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException();
    sbyte IConvertible.ToSByte(IFormatProvider provider) => Convert.ToSByte(this.Value);
    byte IConvertible.ToByte(IFormatProvider provider) => Convert.ToByte(this.Value);
    short IConvertible.ToInt16(IFormatProvider provider) => Convert.ToInt16(this.Value);
    ushort IConvertible.ToUInt16(IFormatProvider provider) => Convert.ToUInt16(this.Value);
    int IConvertible.ToInt32(IFormatProvider provider) => Convert.ToInt32(this.Value);
    uint IConvertible.ToUInt32(IFormatProvider provider) => Convert.ToUInt32(this.Value);
    long IConvertible.ToInt64(IFormatProvider provider) => Convert.ToInt64(this.Value);
    ulong IConvertible.ToUInt64(IFormatProvider provider) => Convert.ToUInt64(this.Value);
    float IConvertible.ToSingle(IFormatProvider provider) => Convert.ToSingle(this.Value);
    double IConvertible.ToDouble(IFormatProvider provider) => this.Value;
    decimal IConvertible.ToDecimal(IFormatProvider provider) => Convert.ToDecimal(this.Value);
    DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException();
    string IConvertible.ToString(IFormatProvider provider) => ToString();
    object IConvertible.ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(this.Value, conversionType, provider);

    #endregion IConvertible

    #region Arithmetic operators

    public static bool operator ==(Point left, Point right) => left.Equals(right);
    public static bool operator !=(Point left, Point right) => !(left == right);
    public static bool operator ==(Point left, Pixel right) => left.Equals(right);
    public static bool operator !=(Point left, Pixel right) => !(left == right);
    public static bool operator ==(Point left, double right) => left.Equals(right);
    public static bool operator !=(Point left, double right) => !(left == right);
    public static bool operator ==(Point left, Millimeter right) => left.Equals(right);
    public static bool operator !=(Point left, Millimeter right) => !(left == right);
    public static bool operator ==(Point left, Inch right) => left.Equals(right);
    public static bool operator !=(Point left, Inch right) => !(left == right);
    public static Inch operator +(Point left, Pixel right) => left.Value + right.ToPoint().Value;
    public static Inch operator -(Point left, Pixel right) => left.Value - right.ToPoint().Value;
    public static Inch operator *(Point left, Pixel right) => left.Value * right.ToPoint().Value;
    public static Inch operator /(Point left, Pixel right) => left.Value / right.ToPoint().Value;
    public static Inch operator +(Point left, Point right) => left.Value + right.Value;
    public static Inch operator -(Point left, Point right) => left.Value - right.Value;
    public static Inch operator *(Point left, Point right) => left.Value * right.Value;
    public static Inch operator /(Point left, Point right) => left.Value / right.Value;
    public static Inch operator +(Point left, Inch right) => left.Value + right.ToPoint().Value;
    public static Inch operator -(Point left, Inch right) => left.Value - right.ToPoint().Value;
    public static Inch operator *(Point left, Inch right) => left.Value * right.ToPoint().Value;
    public static Inch operator /(Point left, Inch right) => left.Value / right.ToPoint().Value;
    public static Inch operator +(Point left, Millimeter right) => left.Value + right.ToPoint().Value;
    public static Inch operator -(Point left, Millimeter right) => left.Value - right.ToPoint().Value;
    public static Inch operator *(Point left, Millimeter right) => left.Value * right.ToPoint().Value;
    public static Inch operator /(Point left, Millimeter right) => left.Value / right.ToPoint().Value;
    public static Inch operator +(Point left, double right) => left.Value + right;
    public static Inch operator -(Point left, double right) => left.Value - right;
    public static Inch operator *(Point left, double right) => left.Value * right;
    public static Inch operator /(Point left, double right) => left.Value / right;

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Point(Millimeter value) => value.ToPoint();
    public static implicit operator Point(Inch value) => value.ToPoint();
    public static implicit operator Point(Pixel dipValue) => dipValue.ToPoint();
    public static implicit operator Point(double value) => new Point(value);
    public static implicit operator double(Point pointValue) => pointValue.Value;

    #endregion Cast operators

    #region Increment and Decrement operators

    public static Point operator ++(Point point) => point.Value + 1d;
    public static Point operator --(Point point) => point.Value - 1d;

    #endregion Increment and Decrement operators

    #region Comparison operators

    public static bool operator <(Point left, Point right) => left.CompareTo(right) < 0;

    public static bool operator <=(Point left, Point right) => left.CompareTo(right) <= 0;

    public static bool operator >(Point left, Point right) => left.CompareTo(right) > 0;

    public static bool operator >=(Point left, Point right) => left.CompareTo(right) >= 0;

    public static bool operator <(Point left, Pixel right) => left.CompareTo(right) < 0;

    public static bool operator <=(Point left, Pixel right) => left.CompareTo(right) <= 0;

    public static bool operator >(Point left, Pixel right) => left.CompareTo(right) > 0;

    public static bool operator >=(Point left, Pixel right) => left.CompareTo(right) >= 0;

    public static bool operator <(Point left, Millimeter right) => left.CompareTo(right) < 0;

    public static bool operator <=(Point left, Millimeter right) => left.CompareTo(right) <= 0;

    public static bool operator >(Point left, Millimeter right) => left.CompareTo(right) > 0;

    public static bool operator >=(Point left, Millimeter right) => left.CompareTo(right) >= 0;

    public static bool operator <(Point left, Inch right) => left.CompareTo(right) < 0;

    public static bool operator <=(Point left, Inch right) => left.CompareTo(right) <= 0;

    public static bool operator >(Point left, Inch right) => left.CompareTo(right) > 0;

    public static bool operator >=(Point left, Inch right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Unary operators

    public static Point operator +(Point point) => +point.Value;
    public static Point operator -(Point point) => -point.Value;

    #endregion
  }
}