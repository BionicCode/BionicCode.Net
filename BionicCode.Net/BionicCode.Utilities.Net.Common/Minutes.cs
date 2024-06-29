namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Minutes : IEquatable<Minutes>, IComparable<Minutes>
  {
    public static Minutes Zero { get; } = new Minutes(0);
    public static Minutes MinValue { get; } = new Minutes(TimeValueConverter.ToMinutes(Nanoseconds.MinValue));
    public static Minutes MaxValue { get; } = new Minutes(TimeValueConverter.ToMinutes(Nanoseconds.MaxValue));

    public Minutes(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Minutes;
    }

    public Seconds ToSeconds() => new Seconds(TimeValueConverter.ToSeconds(this));
    public Milliseconds ToMilliseconds() => new Milliseconds(TimeValueConverter.ToMilliseconds(this));
    public Microseconds ToMicroseconds() => new Microseconds(TimeValueConverter.ToMicroseconds(this));
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Minutes other) => this.Value.Equals(other.Value);
    public int CompareTo(Minutes other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Seconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(Milliseconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(Microseconds other) => CompareTo(other.ToMinutes());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToMinutes());

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

    public override bool Equals(object obj) => obj is Minutes minutes && Equals(minutes);

    public double Value { get; }
    public TimeUnit Unit { get; }

    #region Comparison operators

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

    #endregion Comparison operators

    #region Arithmetic operators

    public static Minutes operator +(Minutes left, Minutes right) => new Minutes(left.Value + right.Value);
    public static Minutes operator -(Minutes left, Minutes right) => new Minutes(left.Value - right.Value);

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Minutes right) => new Minutes(left.Value * right.Value);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Minutes right) => new Minutes(left.Value / right.Value);

    public static Minutes operator +(Minutes left, Seconds right) => new Minutes(left.Value + TimeValueConverter.ToMinutes(right));
    public static Minutes operator -(Minutes left, Seconds right) => new Minutes(left.Value - TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Seconds right) => new Minutes(left.Value * TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Seconds right) => new Minutes(left.Value / TimeValueConverter.ToMinutes(right));

    public static Minutes operator +(Minutes left, Milliseconds right) => new Minutes(left.Value + TimeValueConverter.ToMinutes(right));
    public static Minutes operator -(Minutes left, Milliseconds right) => new Minutes(left.Value - TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Milliseconds right) => new Minutes(left.Value * TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Milliseconds right) => new Minutes(left.Value / TimeValueConverter.ToMinutes(right));

    public static Minutes operator +(Minutes left, Microseconds right) => new Minutes(left.Value + TimeValueConverter.ToMinutes(right));
    public static Minutes operator -(Minutes left, Microseconds right) => new Minutes(left.Value - TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Microseconds right) => new Minutes(left.Value * TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Microseconds right) => new Minutes(left.Value / TimeValueConverter.ToMinutes(right));

    public static Minutes operator +(Minutes left, Nanoseconds right) => new Minutes(left.Value + TimeValueConverter.ToMinutes(right));
    public static Minutes operator -(Minutes left, Nanoseconds right) => new Minutes(left.Value - TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, Nanoseconds right) => new Minutes(left.Value * TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, Nanoseconds right) => new Minutes(left.Value / TimeValueConverter.ToMinutes(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Minutes operator *(Minutes left, double right) => new Minutes(left.Value * right);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Minutes operator /(Minutes left, double right) => new Minutes(left.Value / right);

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

    #endregion Equality operators
  }
}