namespace BionicCode.Utilities.Net
{
  using System;
  using System.Numerics;
  using Microsoft.CodeAnalysis.CSharp.Syntax;

  //using iText.Kernel.Geom;
  public readonly struct Millimeter : IEquatable<Millimeter>, IComparable<Millimeter>, IComparable, IConvertible
  {
    // In the PDF coordinate system 1 pt is: 25,4 mm for 72 pt (1 pt = 25.4/72 mm)
    // which is equal to 72 pt for 25.4 mm (1 mm = 72/25.4 = 2.835 pt).
    public const double MillimetersPerPoint = 25.4 / 72d;

    public const double MillimetersPerInch = 25.4 / 1d;
    public const double MillimetersPerPixel = 25.4 / 96d;

    public Millimeter(double value)
    {
      this.Value = value;
      this.Unit = LengthUnit.Millimeter;
    }

    public static Millimeter ToMillimeter(double value, LengthUnit unit)
    {
      switch(unit)
      {
        case LengthUnit.Default: 
        case LengthUnit.Millimeter: return value;
        case LengthUnit.Inch: return Millimeter.FromInch(value);
        case LengthUnit.Point: return Millimeter.FromPoint(value);
        case LengthUnit.Pixel: return Millimeter.FromPixel(value);
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
          return this.Value;
        case LengthUnit.Inch:
          return Inch.FromMillimeter(this.Value);
        case LengthUnit.Point:
          return Point.FromMillimeter(this.Value);
        case LengthUnit.Pixel:
          return Pixel.FromMillimeter(this.Value);
        default:
          throw new NotImplementedException();
      }
    }

    public Inch ToInch()
      => this.Value / Millimeter.MillimetersPerInch;

    public Point ToPoint()
      => this.Value / Millimeter.MillimetersPerPoint;

    public Pixel ToPixel()
      => this.Value / Millimeter.MillimetersPerPixel;

    public static Millimeter FromInch(double inch)
      => new Inch(inch);

    public static Millimeter FromPoint(double point)
      => new Point(point);

    public static Millimeter FromPixel(double pixel)
      => new Pixel(pixel);

    public override string ToString() => this.Value.ToString();

    public bool Equals(Millimeter other) => this.Value.Equals(other.Value);

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

    public override bool Equals(object obj) => obj is Millimeter value && Equals(value);

    #region IComparable

    public int CompareTo(Millimeter other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Inch other) => CompareTo(other.ToMillimeter());
    public int CompareTo(Pixel other) => CompareTo(other.ToMillimeter());
    public int CompareTo(Point other) => CompareTo(other.ToMillimeter());

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

    public double Value { get; }

    public LengthUnit Unit { get; }

    #region Arithmetic operators

    public static bool operator ==(Millimeter left, Millimeter right) => left.Equals(right);
    public static bool operator !=(Millimeter left, Millimeter right) => !(left == right);
    public static bool operator ==(Millimeter left, Pixel right) => left.Equals(right);
    public static bool operator !=(Millimeter left, Pixel right) => !(left == right);
    public static bool operator ==(Millimeter left, double right) => left.Equals(right);
    public static bool operator !=(Millimeter left, double right) => !(left == right);
    public static bool operator ==(Millimeter left, Inch right) => left.Equals(right);
    public static bool operator !=(Millimeter left, Inch right) => !(left == right);
    public static bool operator ==(Millimeter left, Point right) => left.Equals(right);
    public static bool operator !=(Millimeter left, Point right) => !(left == right);
    public static Millimeter operator +(Millimeter left, Millimeter right) => left.Value + right.Value;
    public static Millimeter operator -(Millimeter left, Millimeter right) => left.Value - right.Value;
    public static Millimeter operator *(Millimeter left, Millimeter right) => left.Value * right.Value;
    public static Millimeter operator /(Millimeter left, Millimeter right) => left.Value / right.Value;
    public static Millimeter operator +(Millimeter left, Pixel right) => left.Value + right.ToMillimeter().Value;
    public static Millimeter operator -(Millimeter left, Pixel right) => left.Value - right.ToMillimeter().Value;
    public static Millimeter operator *(Millimeter left, Pixel right) => left.Value * right.ToMillimeter().Value;
    public static Millimeter operator /(Millimeter left, Pixel right) => left.Value / right.ToMillimeter().Value;
    public static Millimeter operator +(Millimeter left, Point right) => left.Value + right.ToMillimeter().Value;
    public static Millimeter operator -(Millimeter left, Point right) => left.Value - right.ToMillimeter().Value;
    public static Millimeter operator *(Millimeter left, Point right) => left.Value * right.ToMillimeter().Value;
    public static Millimeter operator /(Millimeter left, Point right) => left.Value / right.ToMillimeter().Value;
    public static Millimeter operator +(Millimeter left, Inch right) => left.Value + right.ToMillimeter().Value;
    public static Millimeter operator -(Millimeter left, Inch right) => left.Value - right.ToMillimeter().Value;
    public static Millimeter operator *(Millimeter left, Inch right) => left.Value * right.ToMillimeter().Value;
    public static Millimeter operator /(Millimeter left, Inch right) => left.Value / right.ToMillimeter().Value;
    public static Millimeter operator +(Millimeter left, double right) => left.Value + right;
    public static Millimeter operator -(Millimeter left, double right) => left.Value - right;
    public static Millimeter operator *(Millimeter left, double right) => left.Value * right;
    public static Millimeter operator /(Millimeter left, double right) => left.Value / right;

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Millimeter(Point pointValue) => pointValue.ToMillimeter();
    public static implicit operator Millimeter(Inch inchValue) => inchValue.ToMillimeter();
    public static implicit operator Millimeter(Pixel dipValue) => dipValue.ToMillimeter();
    public static implicit operator Millimeter(double doubleValue) => new Millimeter(doubleValue);
    public static implicit operator double(Millimeter millimeterValue) => millimeterValue.Value;

    #endregion Cast operators

    #region Comparison operators

    public static bool operator <(Millimeter left, Millimeter right) => left.CompareTo(right) < 0;

    public static bool operator <=(Millimeter left, Millimeter right) => left.CompareTo(right) <= 0;

    public static bool operator >(Millimeter left, Millimeter right) => left.CompareTo(right) > 0;

    public static bool operator >=(Millimeter left, Millimeter right) => left.CompareTo(right) >= 0;

    public static bool operator <(Millimeter left, Inch right) => left.CompareTo(right) < 0;

    public static bool operator <=(Millimeter left, Inch right) => left.CompareTo(right) <= 0;

    public static bool operator >(Millimeter left, Inch right) => left.CompareTo(right) > 0;

    public static bool operator >=(Millimeter left, Inch right) => left.CompareTo(right) >= 0;

    public static bool operator <(Millimeter left, Pixel right) => left.CompareTo(right) < 0;

    public static bool operator <=(Millimeter left, Pixel right) => left.CompareTo(right) <= 0;

    public static bool operator >(Millimeter left, Pixel right) => left.CompareTo(right) > 0;

    public static bool operator >=(Millimeter left, Pixel right) => left.CompareTo(right) >= 0;

    public static bool operator <(Millimeter left, Point right) => left.CompareTo(right) < 0;

    public static bool operator <=(Millimeter left, Point right) => left.CompareTo(right) <= 0;

    public static bool operator >(Millimeter left, Point right) => left.CompareTo(right) > 0;

    public static bool operator >=(Millimeter left, Point right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Increment and Decrement operators

    public static Millimeter operator ++(Millimeter millimeter) => millimeter.Value + 1d;
    public static Millimeter operator --(Millimeter millimeter) => millimeter.Value - 1d;
    
    #endregion Increment and Decrement operators

    #region Unary operators

    public static Millimeter operator +(Millimeter millimeter) => +millimeter.Value;
    public static Millimeter operator -(Millimeter millimeter) => -millimeter.Value;

    #endregion
  }
}