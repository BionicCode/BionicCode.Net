namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Nanoseconds : IEquatable<Nanoseconds>, IComparable<Nanoseconds>
  {
    public static Nanoseconds Zero { get; } = new Nanoseconds(0);
    public static Nanoseconds MinValue { get; } = new Nanoseconds(double.MinValue);
    public static Nanoseconds MaxValue { get; } = new Nanoseconds(double.MaxValue);

    public Nanoseconds(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Nanoseconds;
    }

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
    public Seconds ToSeconds() => new Seconds(TimeValueConverter.ToSeconds(this));
    public Milliseconds ToMilliseconds() => new Milliseconds(TimeValueConverter.ToMilliseconds(this));
    public Microseconds ToMicroseconds() => new Microseconds(TimeValueConverter.ToMicroseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Nanoseconds other) => this.Value.Equals(other.Value);
    public int CompareTo(Nanoseconds other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Microseconds other) => CompareTo(other.ToNanoseconds());
    public int CompareTo(Milliseconds other) => CompareTo(other.ToNanoseconds());
    public int CompareTo(Seconds other) => CompareTo(other.ToNanoseconds());
    public int CompareTo(Minutes other) => CompareTo(other.ToNanoseconds());

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

    public override bool Equals(object obj) => obj is Nanoseconds nanoseconds && Equals(nanoseconds);

    public double Value { get; }
    public TimeUnit Unit { get; }

    #region Comparison operators

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Nanoseconds left, Nanoseconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Nanoseconds left, Nanoseconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Nanoseconds left, Nanoseconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Nanoseconds left, Nanoseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Nanoseconds left, Microseconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Nanoseconds left, Microseconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Nanoseconds left, Microseconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Nanoseconds left, Microseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Nanoseconds left, Milliseconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Nanoseconds left, Milliseconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Nanoseconds left, Milliseconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Nanoseconds left, Milliseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Nanoseconds left, Seconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Nanoseconds left, Seconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Nanoseconds left, Seconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Nanoseconds left, Seconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Nanoseconds left, Minutes right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Nanoseconds left, Minutes right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Nanoseconds left, Minutes right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Nanoseconds left, Minutes right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Arithmetic operators

    public static Nanoseconds operator +(Nanoseconds left, Minutes right) => new Nanoseconds(left.Value + TimeValueConverter.ToNanoseconds(right));
    public static Nanoseconds operator -(Nanoseconds left, Minutes right) => new Nanoseconds(left.Value - TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Nanoseconds operator *(Nanoseconds left, Minutes right) => new Nanoseconds(left.Value * TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Nanoseconds operator /(Nanoseconds left, Minutes right) => new Nanoseconds(left.Value / TimeValueConverter.ToNanoseconds(right));

    public static Nanoseconds operator +(Nanoseconds left, Seconds right) => new Nanoseconds(left.Value + TimeValueConverter.ToNanoseconds(right));
    public static Nanoseconds operator -(Nanoseconds left, Seconds right) => new Nanoseconds(left.Value - TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Nanoseconds operator *(Nanoseconds left, Seconds right) => new Nanoseconds(left.Value * TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Nanoseconds operator /(Nanoseconds left, Seconds right) => new Nanoseconds(left.Value / TimeValueConverter.ToNanoseconds(right));

    public static Nanoseconds operator +(Nanoseconds left, Milliseconds right) => new Nanoseconds(left.Value + TimeValueConverter.ToNanoseconds(right));
    public static Nanoseconds operator -(Nanoseconds left, Milliseconds right) => new Nanoseconds(left.Value - TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Nanoseconds operator *(Nanoseconds left, Milliseconds right) => new Nanoseconds(left.Value * TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Nanoseconds operator /(Nanoseconds left, Milliseconds right) => new Nanoseconds(left.Value / TimeValueConverter.ToNanoseconds(right));

    public static Nanoseconds operator +(Nanoseconds left, Microseconds right) => new Nanoseconds(left.Value + TimeValueConverter.ToNanoseconds(right));
    public static Nanoseconds operator -(Nanoseconds left, Microseconds right) => new Nanoseconds(left.Value - TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Nanoseconds operator *(Nanoseconds left, Microseconds right) => new Nanoseconds(left.Value * TimeValueConverter.ToNanoseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Nanoseconds operator /(Nanoseconds left, Microseconds right) => new Nanoseconds(left.Value / TimeValueConverter.ToNanoseconds(right));

    public static Nanoseconds operator +(Nanoseconds left, Nanoseconds right) => new Nanoseconds(left.Value + right.Value);
    public static Nanoseconds operator -(Nanoseconds left, Nanoseconds right) => new Nanoseconds(left.Value - right.Value);

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Nanoseconds operator *(Nanoseconds left, Nanoseconds right) => new Nanoseconds(left.Value * right.Value);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Nanoseconds operator /(Nanoseconds left, Nanoseconds right) => new Nanoseconds(left.Value / right.Value);

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Nanoseconds operator *(Nanoseconds left, double right) => new Nanoseconds(left.Value * right);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Nanoseconds operator /(Nanoseconds left, double right) => new Nanoseconds(left.Value / right);

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Nanoseconds(Minutes minutes) => minutes.ToNanoseconds();
    public static implicit operator Nanoseconds(Seconds seconds) => seconds.ToNanoseconds();
    public static implicit operator Nanoseconds(Milliseconds milliseconds) => milliseconds.ToNanoseconds();
    public static implicit operator Nanoseconds(Microseconds microseconds) => microseconds.ToNanoseconds();
#if !NET7_0_OR_GREATER
    public static implicit operator Nanoseconds(TimeSpan timeSpan) => timeSpan.TotalNanoseconds();
    public static implicit operator TimeSpan(Nanoseconds nanoseconds) => TimeSpan.FromMilliseconds(TimeValueConverter.ToMilliseconds(nanoseconds));
#else
    public static implicit operator Nanoseconds(TimeSpan timeSpan) => timeSpan.TotalMicroseconds;
    public static implicit operator TimeSpan(Nanoseconds nanoseconds) => TimeSpan.FromMicroseconds(TimeValueConverter.ToMicroseconds(nanoseconds));
#endif
    public static implicit operator Nanoseconds(double nanoseconds) => new Nanoseconds(nanoseconds);
    public static implicit operator double(Nanoseconds nanoseconds) => nanoseconds.Value;

    #endregion

    #region Unary operators

    public static Nanoseconds operator +(Nanoseconds nanoseconds) => +nanoseconds.Value;
    public static Nanoseconds operator -(Nanoseconds nanoseconds) => -nanoseconds.Value;

    #endregion

    #region Equality operators

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Nanoseconds left, Minutes right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Nanoseconds left, Minutes right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Nanoseconds left, Seconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Nanoseconds left, Seconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Nanoseconds left, Milliseconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Nanoseconds left, Milliseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Nanoseconds left, Microseconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Nanoseconds left, Microseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Nanoseconds left, Nanoseconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Nanoseconds left, Nanoseconds right) => !(left == right);

    #endregion Equality operators
  }
}