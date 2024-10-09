namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Pixel : IEquatable<Pixel>
  {
    public Pixel(double value)
    {
      this.Value = value;
      this.Unit = LengthUnit.Pixel;
    }

    public static Pixel ToPixel(double value, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => Pixel.FromMillimeter(value),
      LengthUnit.Inch => Pixel.FromInch(value),
      LengthUnit.Point => Pixel.FromPoint(value),
      LengthUnit.Pixel => value,
    };

    public static double ToUnit(Pixel pixel, LengthUnit unit) => unit switch
    {
      LengthUnit.Default or LengthUnit.Millimeter => Millimeter.FromPixel(pixel),
      LengthUnit.Inch => Inch.FromPixel(pixel),
      LengthUnit.Point => Point.FromPixel(pixel),
      LengthUnit.Pixel => pixel,
    };

    public Inch ToInch()
      => this.Value * Inch.InchesPerPixel;

    public Millimeter ToMillimeter()
      => this.Value * Millimeter.MillimetersPerPixel;

    public Point ToPoint()
      => this.Value * (Inch.InchesPerPixel / Inch.InchesPerPoint);

    public static Pixel FromInch(double inch)
      => new Inch(inch);

    public static Pixel FromMillimeter(double millimeter)
      => new Millimeter(millimeter);

    public static Pixel FromPoint(double point)
      => new Point(point);

    public double Value { get; }

    public LengthUnit Unit { get; }

    public bool Equals(Pixel other) => this.Value.Equals(other.Value);
    public override int GetHashCode() => HashCode.Combine(this.Value);

    public override bool Equals(object obj) => obj is Pixel value && Equals(value);
    public override string? ToString() => this.Value.ToString();

    public static bool operator ==(Pixel left, Pixel right) => left.Equals(right);
    public static bool operator !=(Pixel left, Pixel right) => !(left == right);
    public static bool operator ==(Pixel left, Inch right) => left.Equals(right);
    public static bool operator !=(Pixel left, Inch right) => !(left == right);
    public static bool operator ==(Pixel left, double right) => left.Equals(right);
    public static bool operator !=(Pixel left, double right) => !(left == right);
    public static bool operator ==(Pixel left, Millimeter right) => left.Equals(right);
    public static bool operator !=(Pixel left, Millimeter right) => !(left == right);
    public static bool operator ==(Pixel left, Point right) => left.Equals(right);
    public static bool operator !=(Pixel left, Point right) => !(left == right);
    public static Inch operator +(Pixel left, Pixel right) => left.Value + right.Value;
    public static Inch operator -(Pixel left, Pixel right) => left.Value - right.Value;
    public static Inch operator *(Pixel left, Pixel right) => left.Value * right.Value;
    public static Inch operator /(Pixel left, Pixel right) => left.Value / right.Value;
    public static Inch operator +(Pixel left, Inch right) => left.Value + right.ToPixel().Value;
    public static Inch operator -(Pixel left, Inch right) => left.Value - right.ToPixel().Value;
    public static Inch operator *(Pixel left, Inch right) => left.Value * right.ToPixel().Value;
    public static Inch operator /(Pixel left, Inch right) => left.Value / right.ToPixel().Value;
    public static Inch operator +(Pixel left, Point right) => left.Value + right.ToPixel().Value;
    public static Inch operator -(Pixel left, Point right) => left.Value - right.ToPixel().Value;
    public static Inch operator *(Pixel left, Point right) => left.Value * right.ToPixel().Value;
    public static Inch operator /(Pixel left, Point right) => left.Value / right.ToPixel().Value;
    public static Inch operator +(Pixel left, Millimeter right) => left.Value + right.ToPixel().Value;
    public static Inch operator -(Pixel left, Millimeter right) => left.Value - right.ToPixel().Value;
    public static Inch operator *(Pixel left, Millimeter right) => left.Value * right.ToPixel().Value;
    public static Inch operator /(Pixel left, Millimeter right) => left.Value / right.ToPixel().Value;
    public static Inch operator +(Pixel left, double right) => left.Value + right;
    public static Inch operator -(Pixel left, double right) => left.Value - right;
    public static Inch operator *(Pixel left, double right) => left.Value * right;
    public static Inch operator /(Pixel left, double right) => left.Value / right;
    public static implicit operator Pixel(Millimeter millimeterValue) => millimeterValue.ToPixel();
    public static implicit operator Pixel(Point pointValue) => pointValue.ToPixel();
    public static implicit operator Pixel(Inch inchValue) => inchValue.ToPixel();
    public static implicit operator Pixel(double doubleValue) => new(doubleValue);
    public static implicit operator double(Pixel pixelValue) => pixelValue.Value;
  }
}