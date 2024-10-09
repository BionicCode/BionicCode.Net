namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Inch : IEquatable<Inch>
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

    public static Inch ToInch(double value, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => Inch.FromMillimeter(value),
      LengthUnit.Inch => value,
      LengthUnit.Point => Inch.FromPoint(value),
      LengthUnit.Pixel => Inch.FromPixel(value),
      _ => throw new NotImplementedException()
    };

    public static double ToUnit(Inch inch, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => Millimeter.FromInch(inch),
      LengthUnit.Inch => inch,
      LengthUnit.Point => Point.FromInch(inch),
      LengthUnit.Pixel => Pixel.FromInch(inch),
      _ => throw new NotImplementedException()
    };

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
    public override int GetHashCode() => HashCode.Combine(this.Value);

    public override bool Equals(object? obj) => obj is Inch value && Equals(value);
    public override string? ToString() => this.Value.ToString();

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
    public static implicit operator Inch(Millimeter millimeterValue) => millimeterValue.ToInch();
    public static implicit operator Inch(Point pointValue) => pointValue.ToInch();
    public static implicit operator Inch(Pixel pixelValue) => pixelValue.ToInch();
    public static implicit operator Inch(double doubleValue) => new(doubleValue);
    public static implicit operator double(Inch inchValue) => inchValue.Value;
  }
}