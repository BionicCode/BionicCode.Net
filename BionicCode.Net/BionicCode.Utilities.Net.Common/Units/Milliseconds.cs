namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Milliseconds : IEquatable<Milliseconds>, IComparable<Milliseconds>, IComparable, IConvertible, ITimeUnit
  {
    public static Milliseconds Zero { get; } = 0;
    public static Milliseconds MinValue { get; } = TimeValueConverter.ToMilliseconds(Nanoseconds.MinValue);
    public static Milliseconds MaxValue { get; } = TimeValueConverter.ToMilliseconds(Nanoseconds.MaxValue);

    public Milliseconds(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Milliseconds;
    }

    public Minutes ToMinutes() => TimeValueConverter.ToMinutes(this);
    public Seconds ToSeconds() => TimeValueConverter.ToSeconds(this);
    public Microseconds ToMicroseconds() => TimeValueConverter.ToMicroseconds(this);
    public Nanoseconds ToNanoseconds() => TimeValueConverter.ToNanoseconds(this);
    public Seconds ToSiUnit() => ToSeconds();

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Milliseconds other) => this.Value.Equals(other.Value);
    public override bool Equals(object obj) => obj is Milliseconds milliseconds && Equals(milliseconds) || obj is double value && Equals(value);

    #region IComparable
    
    public int CompareTo(Milliseconds other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Minutes other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(Seconds other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(Microseconds other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(TimeSpan other) => CompareTo((Milliseconds)other);

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

    #endregion IComaparable

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
    public static bool operator <(Milliseconds left, double right) => left.CompareTo((Milliseconds)right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, double right) => left.CompareTo((Milliseconds)right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, double right) => left.CompareTo((Milliseconds)right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, double right) => left.CompareTo((Milliseconds)right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Milliseconds left, Minutes right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, Minutes right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, Minutes right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, Minutes right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Milliseconds left, Seconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, Seconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, Seconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, Seconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Milliseconds left, Milliseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, Milliseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, Milliseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, Milliseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Milliseconds left, Microseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, Microseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, Microseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, Microseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Milliseconds left, Nanoseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, Nanoseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, Nanoseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, Nanoseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Milliseconds left, TimeSpan right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Milliseconds left, TimeSpan right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Milliseconds left, TimeSpan right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Milliseconds left, TimeSpan right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Arithmetic operators

    public static Milliseconds operator +(Milliseconds left, Minutes right) => left.Value + right.ToMilliseconds();
    public static Milliseconds operator -(Milliseconds left, Minutes right) => left.Value - right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Minutes right) => left.Value * right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Minutes right) => left.Value / right.ToMilliseconds();

    public static Milliseconds operator +(Milliseconds left, Seconds right) => left.Value + right.ToMilliseconds();
    public static Milliseconds operator -(Milliseconds left, Seconds right) => left.Value - right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Seconds right) => left.Value * right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Seconds right) => left.Value / right.ToMilliseconds();

    public static Milliseconds operator +(Milliseconds left, Milliseconds right) => left.Value + right.Value;
    public static Milliseconds operator -(Milliseconds left, Milliseconds right) => left.Value - right.Value;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Milliseconds right) => left.Value * right.Value;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Milliseconds right) => left.Value / right.Value;

    public static Milliseconds operator +(Milliseconds left, Microseconds right) => left.Value + right.ToMilliseconds();
    public static Milliseconds operator -(Milliseconds left, Microseconds right) => left.Value - right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Microseconds right) => left.Value * right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Microseconds right) => left.Value / right.ToMilliseconds();

    public static Milliseconds operator +(Milliseconds left, Nanoseconds right) => left.Value + right.ToMilliseconds();
    public static Milliseconds operator -(Milliseconds left, Nanoseconds right) => left.Value - right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Nanoseconds right) => left.Value * right.ToMilliseconds();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Nanoseconds right) => left.Value / right.ToMilliseconds();

    public static Milliseconds operator +(Milliseconds left, double right) => left.Value + right;
    public static Milliseconds operator -(Milliseconds left, double right) => left.Value - right;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, double right) => left.Value * right;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, double right) => left.Value / right;

    public static Milliseconds operator +(Milliseconds left, TimeSpan right) => left.Value + right.TotalMilliseconds;
    public static Milliseconds operator -(Milliseconds left, TimeSpan right) => left.Value - right.TotalMilliseconds;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, TimeSpan right) => left.Value * right.TotalMilliseconds;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, TimeSpan right) => left.Value / right.TotalMilliseconds;

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Milliseconds(Minutes minutes) => minutes.ToMilliseconds();
    public static implicit operator Milliseconds(Seconds seconds) => seconds.ToMilliseconds();
    public static implicit operator Milliseconds(Microseconds micrseconds) => micrseconds.ToMilliseconds();
    public static implicit operator Milliseconds(Nanoseconds nanoseconds) => nanoseconds.ToMilliseconds();
    public static implicit operator Milliseconds(TimeSpan timeSpan) => timeSpan.TotalMilliseconds;
    public static implicit operator TimeSpan(Milliseconds milliseconds) => TimeSpan.FromMilliseconds(milliseconds.Value);
    public static implicit operator Milliseconds(double milliseconds) => new Milliseconds(milliseconds);
    public static implicit operator double(Milliseconds milliseconds) => milliseconds.Value;

    #endregion

    #region Unary operators

    public static Milliseconds operator +(Milliseconds nanoseconds) => +nanoseconds.Value;
    public static Milliseconds operator -(Milliseconds nanoseconds) => -nanoseconds.Value;

    #endregion

    #region Equality operators

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, double right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, double right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, Minutes right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, Minutes right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, Seconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, Seconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, Milliseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, Milliseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, Microseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, Microseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, Nanoseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, Nanoseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Milliseconds left, TimeSpan right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Milliseconds left, TimeSpan right) => !(left == right);

    #endregion Equality operators

    #region Increment and Decrement operators

    public static Milliseconds operator ++(Milliseconds milliseconds) => milliseconds.Value + 1d;
    public static Milliseconds operator --(Milliseconds milliseconds) => milliseconds.Value - 1d;

    #endregion Increment and Decrement operators

    #region ITimeUnit

    public ITimeUnit ToUnit(TimeUnit unit)
    {
      switch (unit)
      {
        case TimeUnit.Microseconds:
          return ToMicroseconds();
        case TimeUnit.Nanoseconds:
          return ToNanoseconds();
        case TimeUnit.Milliseconds:
          return this;
        case TimeUnit.Seconds:
          return ToSeconds();
        case TimeUnit.Minutes:
          return ToMinutes();
        case TimeUnit.None:
        case TimeUnit.Auto:
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