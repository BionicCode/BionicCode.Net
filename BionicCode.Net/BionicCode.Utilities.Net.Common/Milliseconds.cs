namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Milliseconds : IEquatable<Milliseconds>, IComparable<Milliseconds>
  {
    public static Milliseconds Zero { get; } = new Milliseconds(0);
    public static Milliseconds MinValue { get; } = new Milliseconds(TimeValueConverter.ToMilliseconds(Nanoseconds.MinValue));
    public static Milliseconds MaxValue { get; } = new Milliseconds(TimeValueConverter.ToMilliseconds(Nanoseconds.MaxValue));

    public Milliseconds(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Milliseconds;
    }

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
    public Seconds ToSeconds() => new Seconds(TimeValueConverter.ToSeconds(this));
    public Microseconds ToMicroseconds() => new Microseconds(TimeValueConverter.ToMicroseconds(this));

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard21)'
Before:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));
    
    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
After:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net50)'
Before:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));
    
    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
After:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net80)'
Before:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));
    
    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
After:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net60)'
Before:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));
    
    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
After:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net70)'
Before:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));
    
    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
After:
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
*/
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Milliseconds other) => this.Value.Equals(other.Value);
    public int CompareTo(Milliseconds other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Minutes other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(Seconds other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(Microseconds other) => CompareTo(other.ToMilliseconds());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToMilliseconds());

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

    public override bool Equals(object obj) => obj is Milliseconds milliseconds && Equals(milliseconds);

    public double Value { get; }
    public TimeUnit Unit { get; }

    #region Comparison operators

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

    #endregion Comparison operators

    #region Arithmetic operators

    public static Milliseconds operator +(Milliseconds left, Minutes right) => new Milliseconds(left.Value + TimeValueConverter.ToMilliseconds(right));
    public static Milliseconds operator -(Milliseconds left, Minutes right) => new Milliseconds(left.Value - TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Minutes right) => new Milliseconds(left.Value * TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Minutes right) => new Milliseconds(left.Value / TimeValueConverter.ToMilliseconds(right));

    public static Milliseconds operator +(Milliseconds left, Seconds right) => new Milliseconds(left.Value + TimeValueConverter.ToMilliseconds(right));
    public static Milliseconds operator -(Milliseconds left, Seconds right) => new Milliseconds(left.Value - TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Seconds right) => new Milliseconds(left.Value * TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Seconds right) => new Milliseconds(left.Value / TimeValueConverter.ToMilliseconds(right));

    public static Milliseconds operator +(Milliseconds left, Milliseconds right) => new Milliseconds(left.Value + right.Value);
    public static Milliseconds operator -(Milliseconds left, Milliseconds right) => new Milliseconds(left.Value - right.Value);

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Milliseconds right) => new Milliseconds(left.Value * right.Value);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Milliseconds right) => new Milliseconds(left.Value / right.Value);

    public static Milliseconds operator +(Milliseconds left, Microseconds right) => new Milliseconds(left.Value + TimeValueConverter.ToMilliseconds(right));
    public static Milliseconds operator -(Milliseconds left, Microseconds right) => new Milliseconds(left.Value - TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Microseconds right) => new Milliseconds(left.Value * TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Microseconds right) => new Milliseconds(left.Value / TimeValueConverter.ToMilliseconds(right));

    public static Milliseconds operator +(Milliseconds left, Nanoseconds right) => new Milliseconds(left.Value + TimeValueConverter.ToMilliseconds(right));
    public static Milliseconds operator -(Milliseconds left, Nanoseconds right) => new Milliseconds(left.Value - TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, Nanoseconds right) => new Milliseconds(left.Value * TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, Nanoseconds right) => new Milliseconds(left.Value / TimeValueConverter.ToMilliseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Milliseconds operator *(Milliseconds left, double right) => new Milliseconds(left.Value * right);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Milliseconds operator /(Milliseconds left, double right) => new Milliseconds(left.Value / right);

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

    #endregion Equality operators
  }
}