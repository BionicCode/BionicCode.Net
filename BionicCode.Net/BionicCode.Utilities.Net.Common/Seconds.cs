namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Seconds : IEquatable<Seconds>, IComparable<Seconds>
  {
    public static Seconds Zero { get; } = new Seconds(0);
    public static Seconds MinValue { get; } = new Seconds(TimeValueConverter.ToSeconds(Nanoseconds.MinValue));
    public static Seconds MaxValue { get; } = new Seconds(TimeValueConverter.ToSeconds(Nanoseconds.MaxValue));

    public Seconds(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Seconds;
    }    

    public Minutes ToMinutes() => new Minutes(this.Value / 60);
    public Milliseconds ToMilliseconds() => new Milliseconds(TimeValueConverter.ToMilliseconds(this));
    public Microseconds ToMicroseconds() => new Microseconds(TimeValueConverter.ToMicroseconds(this));
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Seconds other) => this.Value.Equals(other.Value);
    public int CompareTo(Seconds other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Minutes other) => CompareTo(other.ToSeconds());
    public int CompareTo(Milliseconds other) => CompareTo(other.ToSeconds());
    public int CompareTo(Microseconds other) => CompareTo(other.ToSeconds());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToSeconds());

    /// <inheritdoc/>
#if NET || NETSTANDARD2_1_OR_GREATER
    public override int GetHashCode() => HashCode.Combine(this.Value, this.Unit);
#else
    public override int GetHashCode()
    {
      int hashCode = -177567199;
      hashCode = hashCode * -1521134295 + this.Value.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Unit.GetHashCode();
      return hashCode;
    }
#endif

    public override bool Equals(object obj) => obj is Seconds seconds && this.Equals(seconds);

    public double Value { get; }
    public TimeUnit Unit { get; }

    #region Comparison operators

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

    #endregion Comparison operators

    #region Arithmetic operators

    public static Seconds operator +(Seconds left, Minutes right) => new Seconds(left.Value + TimeValueConverter.ToSeconds(right));
    public static Seconds operator -(Seconds left, Minutes right) => new Seconds(left.Value - TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Minutes right) => new Seconds(left.Value * TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Minutes right) => new Seconds(left.Value / TimeValueConverter.ToSeconds(right));

    public static Seconds operator +(Seconds left, Seconds right) => new Seconds(left.Value + right.Value);
    public static Seconds operator -(Seconds left, Seconds right) => new Seconds(left.Value - right.Value);

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Seconds right) => new Seconds(left.Value * right.Value);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Seconds right) => new Seconds(left.Value / right.Value);

    public static Seconds operator +(Seconds left, Milliseconds right) => new Seconds(left.Value + TimeValueConverter.ToSeconds(right));
    public static Seconds operator -(Seconds left, Milliseconds right) => new Seconds(left.Value - TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Milliseconds right) => new Seconds(left.Value * TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Milliseconds right) => new Seconds(left.Value / TimeValueConverter.ToSeconds(right));

    public static Seconds operator +(Seconds left, Microseconds right) => new Seconds(left.Value + TimeValueConverter.ToSeconds(right));
    public static Seconds operator -(Seconds left, Microseconds right) => new Seconds(left.Value - TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Microseconds right) => new Seconds(left.Value * TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Microseconds right) => new Seconds(left.Value / TimeValueConverter.ToSeconds(right));

    public static Seconds operator +(Seconds left, Nanoseconds right) => new Seconds(left.Value + TimeValueConverter.ToSeconds(right));
    public static Seconds operator -(Seconds left, Nanoseconds right) => new Seconds(left.Value - TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, Nanoseconds right) => new Seconds(left.Value * TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, Nanoseconds right) => new Seconds(left.Value / TimeValueConverter.ToSeconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Seconds operator *(Seconds left, double right) => new Seconds(left.Value * right);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Seconds operator /(Seconds left, double right) => new Seconds(left.Value / right);

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

    #endregion Equality operators
  }
}