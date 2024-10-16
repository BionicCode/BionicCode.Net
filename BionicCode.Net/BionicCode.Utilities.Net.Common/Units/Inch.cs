namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Inch : IEquatable<Inch>, IComparable<Inch>, IComparable, IConvertible
  {
    // In the PDF coordinate system 1 point is: 1 in for 72 pt (1 pt = 1/72 in)
    // which is equal to 72 pt for 1 in (1 in = 72 pt).
    public const double InchesPerPoint = 1d / 72d;

    public const double InchesPerPixel = 1d / 96d;
    public const double InchesPerMillimeter = 1d / Millimeter.MillimetersPerInch;

    public Inch(double value)
    {
      this.Value = value;
      this.Unit = LengthUnit.Inch;
    }

    public static Inch ToInch(double value, LengthUnit unit)
    {
      switch(unit)
      {
        case LengthUnit.Default:
        case LengthUnit.Millimeter:
          return Inch.FromMillimeter(value);
        case LengthUnit.Inch: 
          return value;
        case LengthUnit.Point: 
          return Inch.FromPoint(value);
        case LengthUnit.Pixel: 
          return Inch.FromPixel(value);
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
          return Millimeter.FromInch(this.Value);
        case LengthUnit.Inch:
          return this.Value;
        case LengthUnit.Point:
          return Point.FromInch(this.Value);
        case LengthUnit.Pixel:
          return Pixel.FromInch(this.Value);
        default:
          throw new NotImplementedException();
      }
    }

    public Millimeter ToMillimeter()
      => this.Value / Inch.InchesPerMillimeter;

    public Point ToPoint()
      => this.Value / Inch.InchesPerPoint;

    public Pixel ToPixel()
      => this.Value / Inch.InchesPerPixel;

    public static Inch FromMillimeter(double millimeter)
      => new Millimeter(millimeter);

    public static Inch FromPoint(double point)
      => new Point(point);

    public static Inch FromPixel(double pixel)
      => new Pixel(pixel);

    public double Value { get; }

    public LengthUnit Unit { get; }

    public bool Equals(Inch other) => this.Value.Equals(other.Value);

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

    #region IComparable

    public int CompareTo(Inch other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Point other) => CompareTo(other.ToInch());
    public int CompareTo(Pixel other) => CompareTo(other.ToInch());
    public int CompareTo(Millimeter other) => CompareTo(other.ToInch());

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

    public override bool Equals(object obj) => obj is Inch value && Equals(value);
    public override string ToString() => this.Value.ToString();

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

    public static bool operator ==(Inch left, Inch right) => left.Equals(right);
    public static bool operator !=(Inch left, Inch right) => !(left == right);
    public static bool operator ==(Inch left, double right) => left.Equals(right);
    public static bool operator !=(Inch left, double right) => !(left == right);
    public static bool operator ==(Inch left, Millimeter right) => left.Equals(right);
    public static bool operator !=(Inch left, Millimeter right) => !(left == right);
    public static bool operator ==(Inch left, Point right) => left.Equals(right);
    public static bool operator !=(Inch left, Point right) => !(left == right);
    public static Inch operator +(Inch left, Inch right) => left.Value + right.Value;
    public static Inch operator -(Inch left, Inch right) => left.Value - right.Value;
    public static Inch operator *(Inch left, Inch right) => left.Value * right.Value;
    public static Inch operator /(Inch left, Inch right) => left.Value / right.Value;
    public static Inch operator +(Inch left, Point right) => left.Value + right.ToInch().Value;
    public static Inch operator -(Inch left, Point right) => left.Value - right.ToInch().Value;
    public static Inch operator *(Inch left, Point right) => left.Value * right.ToInch().Value;
    public static Inch operator /(Inch left, Point right) => left.Value / right.ToInch().Value;
    public static Inch operator +(Inch left, Millimeter right) => left.Value + right.ToInch().Value;
    public static Inch operator -(Inch left, Millimeter right) => left.Value - right.ToInch().Value;
    public static Inch operator *(Inch left, Millimeter right) => left.Value * right.ToInch().Value;
    public static Inch operator /(Inch left, Millimeter right) => left.Value / right.ToInch().Value;
    public static Inch operator +(Inch left, Pixel right) => left.Value + right.ToInch().Value;
    public static Inch operator -(Inch left, Pixel right) => left.Value - right.ToInch().Value;
    public static Inch operator *(Inch left, Pixel right) => left.Value * right.ToInch().Value;
    public static Inch operator /(Inch left, Pixel right) => left.Value / right.ToInch().Value;
    public static Inch operator +(Inch left, double right) => left.Value + right;
    public static Inch operator -(Inch left, double right) => left.Value - right;
    public static Inch operator *(Inch left, double right) => left.Value * right;
    public static Inch operator /(Inch left, double right) => left.Value / right;

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Inch(Millimeter millimeterValue) => millimeterValue.ToInch();
    public static implicit operator Inch(Point pointValue) => pointValue.ToInch();
    public static implicit operator Inch(Pixel pixelValue) => pixelValue.ToInch();
    public static implicit operator Inch(double doubleValue) => new Inch(doubleValue);
    public static implicit operator double(Inch inchValue) => inchValue.Value;

    #endregion Cast operators

    #region Increment and Decrement operators

    public static Inch operator ++(Inch inch) => inch.Value + 1d;
    public static Inch operator --(Inch inch) => inch.Value - 1d;

    #endregion Increment and Decrement operators

    #region Comparison operators

    public static bool operator <(Inch left, Inch right) => left.CompareTo(right) < 0;

    public static bool operator <=(Inch left, Inch right) => left.CompareTo(right) <= 0;

    public static bool operator >(Inch left, Inch right) => left.CompareTo(right) > 0;

    public static bool operator >=(Inch left, Inch right) => left.CompareTo(right) >= 0;

    #endregion

    #region Unary operators

    public static Inch operator +(Inch inch) => +inch.Value;
    public static Inch operator -(Inch inch) => -inch.Value;

    #endregion
  }
}