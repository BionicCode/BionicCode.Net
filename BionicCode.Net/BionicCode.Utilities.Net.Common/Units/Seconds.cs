namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Seconds : IEquatable<Seconds>, IComparable<Seconds>, IComparable, IConvertible, ITimeUnit
  {
    public static Seconds Zero { get; } = 0;
    public static Seconds MinValue { get; } = TimeValueConverter.ToSeconds(Nanoseconds.MinValue);
    public static Seconds MaxValue { get; } = TimeValueConverter.ToSeconds(Nanoseconds.MaxValue);

    public Seconds(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Seconds;
    }

    public Minutes ToMinutes() => TimeValueConverter.ToMinutes(this);
    public Milliseconds ToMilliseconds() => TimeValueConverter.ToMilliseconds(this);
    public Microseconds ToMicroseconds() => TimeValueConverter.ToMicroseconds(this);
    public Nanoseconds ToNanoseconds() => TimeValueConverter.ToNanoseconds(this);

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Seconds other) => this.Value.Equals(other.Value);
    public override bool Equals(object obj) => obj is Seconds seconds && Equals(seconds) || obj is double value && Equals(value);
    
    #region IComparable
    
    public int CompareTo(Seconds other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Minutes other) => CompareTo(other.ToSeconds());
    public int CompareTo(Milliseconds other) => CompareTo(other.ToSeconds());
    public int CompareTo(Microseconds other) => CompareTo(other.ToSeconds());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToSeconds());
    public int CompareTo(TimeSpan other) => CompareTo((Seconds)other);

    int IComparable.CompareTo(object obj) => obj is Seconds seconds
      ? CompareTo(seconds)
      : obj is Minutes minutes
        ? CompareTo(minutes)
        : obj is Milliseconds milliseconds
          ? CompareTo(milliseconds)
          : obj is Microseconds microseconds
            ? CompareTo(microseconds)
            : obj is Nanoseconds nanoseconds
              ? CompareTo(nanoseconds)
              : obj is TimeSpan timeSpan
                ? CompareTo(timeSpan)
                : throw new ArgumentException("Unable to compare the provided type.", nameof(obj));

    #endregion IComparable

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

    public double Value { get; }
    public TimeUnit Unit { get; }
    
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

    #region Comparison operators

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, double right) => left.CompareTo((Seconds)right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, double right) => left.CompareTo((Seconds)right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, double right) => left.CompareTo((Seconds)right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, double right) => left.CompareTo((Seconds)right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, Minutes right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, Minutes right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, Minutes right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, Minutes right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, Seconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, Seconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, Seconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, Seconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, Milliseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, Milliseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, Milliseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, Milliseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, Microseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, Microseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, Microseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, Microseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, Nanoseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, Nanoseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, Nanoseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, Nanoseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Seconds left, TimeSpan right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Seconds left, TimeSpan right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Seconds left, TimeSpan right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Seconds left, TimeSpan right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Arithmetic operators

    public static Seconds operator +(Seconds left, Minutes right) => left.Value + right.ToSeconds();
    public static Seconds operator -(Seconds left, Minutes right) => left.Value - right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Minutes right) => left.Value * right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Minutes right) => left.Value / right.ToSeconds();

    public static Seconds operator +(Seconds left, Seconds right) => left.Value + right.Value;
    public static Seconds operator -(Seconds left, Seconds right) => left.Value - right.Value;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Seconds right) => left.Value * right.Value;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Seconds right) => left.Value / right.Value;

    public static Seconds operator +(Seconds left, Milliseconds right) => left.Value + right.ToSeconds();
    public static Seconds operator -(Seconds left, Milliseconds right) => left.Value - right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Milliseconds right) => left.Value * right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Milliseconds right) => left.Value / right.ToSeconds();

    public static Seconds operator +(Seconds left, Microseconds right) => left.Value + right.ToSeconds();
    public static Seconds operator -(Seconds left, Microseconds right) => left.Value - right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Microseconds right) => left.Value * right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Microseconds right) => left.Value / right.ToSeconds();

    public static Seconds operator +(Seconds left, Nanoseconds right) => left.Value + right.ToSeconds();
    public static Seconds operator -(Seconds left, Nanoseconds right) => left.Value - right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Nanoseconds right) => left.Value * right.ToSeconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Nanoseconds right) => left.Value / right.ToSeconds();

    public static Seconds operator +(Seconds left, double right) => left.Value + right;
    public static Seconds operator -(Seconds left, double right) => left.Value - right;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, double right) => left.Value * right;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, double right) => left.Value / right;

    public static Seconds operator +(Seconds left, TimeSpan right) => left.Value + right.TotalSeconds;
    public static Seconds operator -(Seconds left, TimeSpan right) => left.Value - right.TotalSeconds;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, TimeSpan right) => left.Value * right.TotalSeconds;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, TimeSpan right) => left.Value / right.TotalSeconds;

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Seconds(Minutes minutes) => minutes.ToSeconds();
    public static implicit operator Seconds(Milliseconds milliseconds) => milliseconds.ToSeconds();
    public static implicit operator Seconds(Microseconds micrseconds) => micrseconds.ToSeconds();
    public static implicit operator Seconds(Nanoseconds nanoseconds) => nanoseconds.ToSeconds();
    public static implicit operator Seconds(TimeSpan timeSpan) => timeSpan.TotalSeconds;
    public static implicit operator TimeSpan(Seconds seconds) => TimeSpan.FromSeconds(seconds.Value);
    public static implicit operator Seconds(double seconds) => new Seconds(seconds);
    public static implicit operator double(Seconds seconds) => seconds.Value;

    #endregion

    #region Unary operators

    public static Seconds operator +(Seconds nanoseconds) => +nanoseconds.Value;
    public static Seconds operator -(Seconds nanoseconds) => -nanoseconds.Value;

    #endregion

    #region Equality operators

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, double right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, double right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, Minutes right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, Minutes right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, Seconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, Seconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, Milliseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, Milliseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, Microseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, Microseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, Nanoseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, Nanoseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Seconds left, TimeSpan right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Seconds left, TimeSpan right) => !(left == right);

    #endregion Equality operators

    #region Increment and Decrement operators

    public static Seconds operator ++(Seconds millimeter) => millimeter.Value + 1d;
    public static Seconds operator --(Seconds millimeter) => millimeter.Value - 1d;

    #endregion Increment and Decrement operators

    #region ITimeUnit

    public Seconds ToSiUnit() => this;

    public ITimeUnit ToUnit(TimeUnit unit)
    {
      switch (unit)
      {
        case TimeUnit.Microseconds:
          return ToMicroseconds();
        case TimeUnit.Nanoseconds:
          return ToNanoseconds();
        case TimeUnit.Milliseconds:
          return ToMilliseconds();
        case TimeUnit.Seconds:
        case TimeUnit.None:
          return this;
        case TimeUnit.Minutes:
          return ToMinutes();
        case TimeUnit.Auto:
          TimeUnit newUnit = TimeValueConverter.GetBestDisplayUnit(this);
          return ToUnit(newUnit);
        default:
          throw new NotImplementedException();
      }
    }

    int IComparable<ITimeUnit>.CompareTo(ITimeUnit other)
    {
      switch (other)
      {
        case Minutes minutes:
          return CompareTo(minutes);
        case Seconds seconds:
          return CompareTo(seconds);
        case Milliseconds milliseconds:
          return CompareTo(milliseconds);
        case Microseconds microseconds:
          return CompareTo(microseconds);
        case Nanoseconds nanoseconds:
          return CompareTo(nanoseconds);
        default:
          throw new NotImplementedException();
      }
    }

    bool IEquatable<ITimeUnit>.Equals(ITimeUnit other)
    {
      switch (other)
      {
        case Minutes minutes:
          return Equals(minutes);
        case Seconds seconds:
          return Equals(seconds);
        case Milliseconds milliseconds:
          return Equals(milliseconds);
        case Microseconds microseconds:
          return Equals(microseconds);
        case Nanoseconds nanoseconds:
          return Equals(nanoseconds);
        default:
          throw new NotImplementedException();
      }
    }

    #endregion ITimeUnit
  }
}