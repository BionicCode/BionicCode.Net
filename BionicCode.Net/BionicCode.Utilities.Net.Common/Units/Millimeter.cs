namespace BionicCode.Utilities.Net
{
  using System;

  //using iText.Kernel.Geom;
  public readonly struct Millimeter : IEquatable<Millimeter>
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

    public static Millimeter ToMillimeter(double value, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => value,
      LengthUnit.Inch => Millimeter.FromInch(value),
      LengthUnit.Point => Millimeter.FromPoint(value),
      LengthUnit.Pixel => Millimeter.FromPixel(value),
    };

    public static double ToUnit(Millimeter millimeter, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => millimeter,
      LengthUnit.Inch => Inch.FromMillimeter(millimeter),
      LengthUnit.Point => Point.FromMillimeter(millimeter),
      LengthUnit.Pixel => Pixel.FromMillimeter(millimeter),
    };

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

    public override string? ToString() => this.Value.ToString();

    public bool Equals(Millimeter other) => this.Value.Equals(other.Value);
    public override int GetHashCode() => HashCode.Combine(this.Value);
    public override bool Equals(object obj) => obj is Millimeter value && Equals(value);

    public double Value { get; }

    public LengthUnit Unit { get; }

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
    public static implicit operator Millimeter(Point pointValue) => pointValue.ToMillimeter();
    public static implicit operator Millimeter(Inch inchValue) => inchValue.ToMillimeter();
    public static implicit operator Millimeter(Pixel dipValue) => dipValue.ToMillimeter();
    public static implicit operator Millimeter(double doubleValue) => new(doubleValue);
    public static implicit operator double(Millimeter millimeterValue) => millimeterValue.Value;
  }
}