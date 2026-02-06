namespace BionicCode.Utilities.Net
{
    using System;

    public readonly struct Pixel : IEquatable<Pixel>, IComparable<Pixel>, IComparable, IConvertible
    {
        public Pixel(double value)
        {
            Value = value;
            Unit = LengthUnit.Pixel;
        }

        public static Pixel ToPixel(double value, LengthUnit unit)
        {
            switch (unit)
            {
                case LengthUnit.Default:
                case LengthUnit.Millimeter:
                    return Pixel.FromMillimeter(value);
                case LengthUnit.Inch:
                    return Pixel.FromInch(value);
                case LengthUnit.Point:
                    return Pixel.FromPoint(value);
                case LengthUnit.Pixel:
                    return value;
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
                    return Millimeter.FromPixel(Value);
                case LengthUnit.Inch:
                    return Inch.FromPixel(Value);
                case LengthUnit.Point:
                    return Point.FromPixel(Value);
                case LengthUnit.Pixel:
                    return Value;
                default:
                    throw new NotImplementedException();
            }
        }

        public Inch ToInch()
          => Value * Inch.InchesPerPixel;

        public Millimeter ToMillimeter()
          => Value * Millimeter.MillimetersPerPixel;

        public Point ToPoint()
          => Value * (Inch.InchesPerPixel / Inch.InchesPerPoint);

        public static Pixel FromInch(double inch)
          => new Inch(inch);

        public static Pixel FromMillimeter(double millimeter)
          => new Millimeter(millimeter);

        public static Pixel FromPoint(double point)
          => new Point(point);

        public double Value { get; }

        public LengthUnit Unit { get; }

        public bool Equals(Pixel other) => Value.Equals(other.Value);

        /// <inheritdoc/>
#if NET || NETSTANDARD2_1_OR_GREATER
        public override int GetHashCode() => HashCode.Combine(Value, Unit);
#else
    public override int GetHashCode()
    {
      int hashCode = -177567199;
      hashCode = (hashCode * -1521134295) + Value.GetHashCode();
      hashCode = (hashCode * -1521134295) + Unit.GetHashCode();
      return hashCode;
    }
#endif

        public override bool Equals(object obj) => obj is Pixel value && Equals(value);
        public override string ToString() => Value.ToString();

        #region IComparable

        public int CompareTo(Pixel other) => Value.CompareTo(other.Value);
        public int CompareTo(Point other) => CompareTo(other.ToPixel());
        public int CompareTo(Inch other) => CompareTo(other.ToPixel());
        public int CompareTo(Millimeter other) => CompareTo(other.ToPixel());

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
        sbyte IConvertible.ToSByte(IFormatProvider provider) => Convert.ToSByte(Value);
        byte IConvertible.ToByte(IFormatProvider provider) => Convert.ToByte(Value);
        short IConvertible.ToInt16(IFormatProvider provider) => Convert.ToInt16(Value);
        ushort IConvertible.ToUInt16(IFormatProvider provider) => Convert.ToUInt16(Value);
        int IConvertible.ToInt32(IFormatProvider provider) => Convert.ToInt32(Value);
        uint IConvertible.ToUInt32(IFormatProvider provider) => Convert.ToUInt32(Value);
        long IConvertible.ToInt64(IFormatProvider provider) => Convert.ToInt64(Value);
        ulong IConvertible.ToUInt64(IFormatProvider provider) => Convert.ToUInt64(Value);
        float IConvertible.ToSingle(IFormatProvider provider) => Convert.ToSingle(Value);
        double IConvertible.ToDouble(IFormatProvider provider) => Value;
        decimal IConvertible.ToDecimal(IFormatProvider provider) => Convert.ToDecimal(Value);
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException();
        string IConvertible.ToString(IFormatProvider provider) => ToString();
        object IConvertible.ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType(Value, conversionType, provider);

        #endregion IConvertible

        #region Arithmetic operators

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

        #endregion Arithmetic operators

        #region Cast operators

        public static implicit operator Pixel(Millimeter millimeterValue) => millimeterValue.ToPixel();
        public static implicit operator Pixel(Point pointValue) => pointValue.ToPixel();
        public static implicit operator Pixel(Inch inchValue) => inchValue.ToPixel();
        public static implicit operator Pixel(double doubleValue) => new Pixel(doubleValue);
        public static implicit operator double(Pixel pixelValue) => pixelValue.Value;

        #endregion Cast operators

        #region Increment and Decrement operators

        public static Pixel operator ++(Pixel pixel) => pixel.Value + 1d;
        public static Pixel operator --(Pixel pixel) => pixel.Value - 1d;

        #endregion Increment and Decrement operators

        #region Comparison operators

        public static bool operator <(Pixel left, Pixel right) => left.CompareTo(right) < 0;

        public static bool operator <=(Pixel left, Pixel right) => left.CompareTo(right) <= 0;

        public static bool operator >(Pixel left, Pixel right) => left.CompareTo(right) > 0;

        public static bool operator >=(Pixel left, Pixel right) => left.CompareTo(right) >= 0;

        #endregion Comparison operators

        #region Unary operators

        public static Pixel operator +(Pixel pixel) => +pixel.Value;
        public static Pixel operator -(Pixel pixel) => -pixel.Value;

        public static Inch Add(Pixel left, Pixel right) => throw new NotImplementedException();

        public static Inch Subtract(Pixel left, Pixel right) => throw new NotImplementedException();
        public static Inch Multiply(Pixel left, Pixel right) => throw new NotImplementedException();

        public static Inch Divide(Pixel left, Pixel right)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
