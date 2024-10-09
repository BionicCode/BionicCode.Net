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
  public readonly struct Point : IEquatable<Point>
  {
    public Point(double value)
    {
      this.Value = value;
      this.Unit = LengthUnit.Point;
    }

    public static Point ToPoint(double value, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => Point.FromMillimeter(value),
      LengthUnit.Inch => Point.FromInch(value),
      LengthUnit.Point => value,
      LengthUnit.Pixel => Point.FromPixel(value),
    };

    public static double ToUnit(Point point, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => Millimeter.FromPoint(point),
      LengthUnit.Inch => Inch.FromPoint(point),
      LengthUnit.Point => point,
      LengthUnit.Pixel => Pixel.FromPoint(point),
    };

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

    public override string? ToString() => this.Value.ToString();

    public double Value { get; }

    public LengthUnit Unit { get; }

    public bool Equals(Point other) => this.Value.Equals(other.Value);
    public override int GetHashCode() => HashCode.Combine(this.Value);
    public override bool Equals(object obj) => obj is Point value && Equals(value);

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

    public static implicit operator Point(Millimeter value) => value.ToPoint();
    public static implicit operator Point(Inch value) => value.ToPoint();
    public static implicit operator Point(Pixel dipValue) => dipValue.ToPoint();
    public static implicit operator Point(double value) => new(value);
    public static implicit operator double(Point pointValue) => pointValue.Value;
  }
}