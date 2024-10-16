namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Minutes : IEquatable<Minutes>, IComparable<Minutes>, IComparable, IConvertible, ITimeUnit
  {
    public static Minutes Zero { get; } = 0;
    public static Minutes MinValue { get; } = TimeValueConverter.ToMinutes(Nanoseconds.MinValue);
    public static Minutes MaxValue { get; } = TimeValueConverter.ToMinutes(Nanoseconds.MaxValue);

    public Minutes(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Minutes;
    }

    public Seconds ToSeconds() => TimeValueConverter.ToSeconds(this);
    public Milliseconds ToMilliseconds() => TimeValueConverter.ToMilliseconds(this);
    public Microseconds ToMicroseconds() => TimeValueConverter.ToMicroseconds(this);
    public Nanoseconds ToNanoseconds() => TimeValueConverter.ToNanoseconds(this);

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Minutes other) => this.Value.Equals(other.Value);
    public override bool Equals(object obj) => obj is Minutes minutes && Equals(minutes) || obj is double value && Equals(value);

    #region IComparable

    public int CompareTo(Minutes other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Seconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(Milliseconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(Microseconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(TimeSpan other) => CompareTo((Minutes)other);

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
    public TimeUnit Unit { get; }

    #region Comparison operators

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, double right) => left.CompareTo((Minutes)right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, double right) => left.CompareTo((Minutes)right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, double right) => left.CompareTo((Minutes)right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, double right) => left.CompareTo((Minutes)right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, Minutes right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, Minutes right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, Minutes right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, Minutes right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, Seconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, Seconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, Seconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, Seconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, Milliseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, Milliseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, Milliseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, Milliseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, Microseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, Microseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, Microseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, Microseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, Nanoseconds right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, Nanoseconds right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, Nanoseconds right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, Nanoseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Minutes left, TimeSpan right) => left.CompareTo(right) < 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Minutes left, TimeSpan right) => left.CompareTo(right) <= 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Minutes left, TimeSpan right) => left.CompareTo(right) > 0;
    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Minutes left, TimeSpan right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Arithmetic operators

    public static Minutes operator +(Minutes left, Minutes right) => left.Value + right.Value;
    public static Minutes operator -(Minutes left, Minutes right) => left.Value - right.Value;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Minutes right) => left.Value * right.Value;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Minutes right) => left.Value / right.Value;

    public static Minutes operator +(Minutes left, Seconds right) => left.Value + right.ToMinutes();
    public static Minutes operator -(Minutes left, Seconds right) => left.Value - right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Seconds right) => left.Value * right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Seconds right) => left.Value / right.ToMinutes();

    public static Minutes operator +(Minutes left, Milliseconds right) => left.Value + right.ToMinutes();
    public static Minutes operator -(Minutes left, Milliseconds right) => left.Value - right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Milliseconds right) => left.Value * right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Milliseconds right) => left.Value / right.ToMinutes();

    public static Minutes operator +(Minutes left, Microseconds right) => left.Value + right.ToMinutes();
    public static Minutes operator -(Minutes left, Microseconds right) => left.Value - right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Microseconds right) => left.Value * right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Microseconds right) => left.Value / right.ToMinutes();

    public static Minutes operator +(Minutes left, Nanoseconds right) => left.Value + right.ToMinutes();
    public static Minutes operator -(Minutes left, Nanoseconds right) => left.Value - right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Nanoseconds right) => left.Value * right.ToMinutes();
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Nanoseconds right) => left.Value / right.ToMinutes();

    public static Minutes operator +(Minutes left, double right) => left.Value + right;
    public static Minutes operator -(Minutes left, double right) => left.Value - right;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, double right) => left.Value * right;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, double right) => left.Value / right;

    public static Minutes operator +(Minutes left, TimeSpan right) => left.Value + right.TotalMinutes;
    public static Minutes operator -(Minutes left, TimeSpan right) => left.Value - right.TotalMinutes;
    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, TimeSpan right) => left.Value * right.TotalMinutes;
    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, TimeSpan right) => left.Value / right.TotalMinutes;

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Minutes(Seconds seconds) => seconds.ToMinutes();
    public static implicit operator Minutes(Milliseconds milliseconds) => milliseconds.ToMinutes();
    public static implicit operator Minutes(Microseconds micrseconds) => micrseconds.ToMinutes();
    public static implicit operator Minutes(Nanoseconds nanoseconds) => nanoseconds.ToMinutes();
    public static implicit operator Minutes(TimeSpan timeSpan) => timeSpan.TotalMinutes;
    public static implicit operator TimeSpan(Minutes minutes) => TimeSpan.FromMinutes(minutes.Value);
    public static implicit operator Minutes(double minutes) => new Minutes(minutes);
    public static implicit operator double(Minutes minutes) => minutes.Value;

    #endregion

    #region Unary operators

    public static Minutes operator +(Minutes nanoseconds) => +nanoseconds.Value;
    public static Minutes operator -(Minutes nanoseconds) => -nanoseconds.Value;

    #endregion

    #region Equality operators

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, double right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, double right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, Minutes right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, Minutes right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, Seconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, Seconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, Milliseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, Milliseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, Microseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, Microseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, Nanoseconds right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, Nanoseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Minutes left, TimeSpan right) => left.Equals(right);
    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Minutes left, TimeSpan right) => !(left == right);

    #endregion Equality operators

    #region Increment and Decrement operators

    public static Minutes operator ++(Minutes minutes) => minutes.Value + 1d;
    public static Minutes operator --(Minutes minutes) => minutes.Value - 1d;

    #endregion Increment and Decrement operators

    #region ITimeUnit

    public Seconds ToSiUnit() => ToSeconds();

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
          return ToSeconds();
        case TimeUnit.Minutes:
        case TimeUnit.None:
          return this;
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
        case Minutes minutes: return CompareTo(minutes);
        case Seconds seconds: return CompareTo(seconds);
        case Milliseconds milliseconds: return CompareTo(milliseconds);
        case Microseconds microseconds: return CompareTo(microseconds);
        case Nanoseconds nanoseconds: return CompareTo(nanoseconds);
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