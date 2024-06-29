namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct Microseconds : IEquatable<Microseconds>, IComparable<Microseconds>
  {
    public static Microseconds Zero { get; } = new Microseconds(0);
    public static Microseconds MinValue { get; } = new Microseconds(TimeValueConverter.ToMicroseconds(Nanoseconds.MinValue));
    public static Microseconds MaxValue { get; } = new Microseconds(TimeValueConverter.ToMicroseconds(Nanoseconds.MaxValue));

    public Microseconds(double value)
    {
      this.Value = value;
      this.Unit = TimeUnit.Microseconds;

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (netstandard21)'
Before:
    }
    

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
After:
    }


    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net50)'
Before:
    }
    

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
After:
    }


    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net80)'
Before:
    }
    

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
After:
    }


    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net60)'
Before:
    }
    

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
After:
    }


    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
*/

/* Unmerged change from project 'BionicCode.Utilities.Net.Common (net70)'
Before:
    }
    

    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
After:
    }


    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
*/
    }


    public Minutes ToMinutes() => new Minutes(TimeValueConverter.ToMinutes(this));
    public Seconds ToSeconds() => new Seconds(TimeValueConverter.ToSeconds(this));
    public Milliseconds ToMilliseconds() => new Milliseconds(TimeValueConverter.ToMilliseconds(this));
    public Nanoseconds ToNanoseconds() => new Nanoseconds(TimeValueConverter.ToNanoseconds(this));

    public override string ToString() => $"{this.Value} {this.Unit.ToDisplayStringValue()}";
    public bool Equals(Microseconds other) => this.Value.Equals(other.Value);
    public int CompareTo(Microseconds other) => this.Value.CompareTo(other.Value);
    public int CompareTo(Minutes other) => CompareTo(other.ToMicroseconds());
    public int CompareTo(Seconds other) => CompareTo(other.ToMicroseconds());
    public int CompareTo(Milliseconds other) => CompareTo(other.ToMicroseconds());
    public int CompareTo(Nanoseconds other) => CompareTo(other.ToMicroseconds());

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

    public override bool Equals(object obj) => obj is Microseconds microseconds && Equals(microseconds);

    public double Value { get; }
    public TimeUnit Unit { get; }

    #region Comparison operators

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Microseconds left, Minutes right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Microseconds left, Minutes right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Microseconds left, Minutes right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Microseconds left, Minutes right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Microseconds left, Seconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Microseconds left, Seconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Microseconds left, Seconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Microseconds left, Seconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Microseconds left, Milliseconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Microseconds left, Milliseconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Microseconds left, Milliseconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Microseconds left, Milliseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Microseconds left, Microseconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Microseconds left, Microseconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Microseconds left, Microseconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Microseconds left, Microseconds right) => left.CompareTo(right) >= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)" />
    public static bool operator <(Microseconds left, Nanoseconds right) => left.CompareTo(right) < 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)" />
    public static bool operator <=(Microseconds left, Nanoseconds right) => left.CompareTo(right) <= 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)" />
    public static bool operator >(Microseconds left, Nanoseconds right) => left.CompareTo(right) > 0;

    /// <inheritdoc cref="System.Numerics.IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)" />
    public static bool operator >=(Microseconds left, Nanoseconds right) => left.CompareTo(right) >= 0;

    #endregion Comparison operators

    #region Arithmetic operators

    public static Microseconds operator +(Microseconds left, Minutes right) => new Microseconds(left.Value + TimeValueConverter.ToMicroseconds(right));
    public static Microseconds operator -(Microseconds left, Minutes right) => new Microseconds(left.Value - TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Microseconds operator *(Microseconds left, Minutes right) => new Microseconds(left.Value * TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Microseconds operator /(Microseconds left, Minutes right) => new Microseconds(left.Value / TimeValueConverter.ToMicroseconds(right));

    public static Microseconds operator +(Microseconds left, Seconds right) => new Microseconds(left.Value + TimeValueConverter.ToMicroseconds(right));
    public static Microseconds operator -(Microseconds left, Seconds right) => new Microseconds(left.Value - TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Microseconds operator *(Microseconds left, Seconds right) => new Microseconds(left.Value * TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Microseconds operator /(Microseconds left, Seconds right) => new Microseconds(left.Value / TimeValueConverter.ToMicroseconds(right));

    public static Microseconds operator +(Microseconds left, Milliseconds right) => new Microseconds(left.Value + TimeValueConverter.ToMicroseconds(right));
    public static Microseconds operator -(Microseconds left, Milliseconds right) => new Microseconds(left.Value - TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Microseconds operator *(Microseconds left, Milliseconds right) => new Microseconds(left.Value * TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Microseconds operator /(Microseconds left, Milliseconds right) => new Microseconds(left.Value / TimeValueConverter.ToMicroseconds(right));

    public static Microseconds operator +(Microseconds left, Microseconds right) => new Microseconds(left.Value + right.Value);
    public static Microseconds operator -(Microseconds left, Microseconds right) => new Microseconds(left.Value - right.Value);

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Microseconds operator *(Microseconds left, Microseconds right) => new Microseconds(left.Value * right.Value);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Microseconds operator /(Microseconds left, Microseconds right) => new Microseconds(left.Value / right.Value);

    public static Microseconds operator +(Microseconds left, Nanoseconds right) => new Microseconds(left.Value + TimeValueConverter.ToMicroseconds(right));
    public static Microseconds operator -(Microseconds left, Nanoseconds right) => new Microseconds(left.Value - TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Microseconds operator *(Microseconds left, Nanoseconds right) => new Microseconds(left.Value * TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Microseconds operator /(Microseconds left, Nanoseconds right) => new Microseconds(left.Value / TimeValueConverter.ToMicroseconds(right));

    /// <inheritdoc cref="System.Numerics.IMultiplyOperators{TSelf, TOther, TResult}.op_Multiply(TSelf, TOther)" />
    public static Microseconds operator *(Microseconds left, double right) => new Microseconds(left.Value * right);

    /// <inheritdoc cref="System.Numerics.IDivisionOperators{TSelf, TOther, TResult}.op_Division(TSelf, TOther)" />
    public static Microseconds operator /(Microseconds left, double right) => new Microseconds(left.Value / right);

    #endregion Arithmetic operators

    #region Cast operators

    public static implicit operator Microseconds(Minutes minutes) => minutes.ToMicroseconds();
    public static implicit operator Microseconds(Seconds seconds) => seconds.ToMicroseconds();
    public static implicit operator Microseconds(Milliseconds milliseconds) => milliseconds.ToMicroseconds();
    public static implicit operator Microseconds(Nanoseconds nanoseconds) => nanoseconds.ToMicroseconds();
#if !NET7_0_OR_GREATER
    public static implicit operator Microseconds(TimeSpan timeSpan) => timeSpan.TotalMicroseconds();
    public static implicit operator TimeSpan(Microseconds microseconds) => TimeSpan.FromMilliseconds(TimeValueConverter.ToMilliseconds(microseconds));
#else
    public static implicit operator Microseconds(TimeSpan timeSpan) => timeSpan.TotalMicroseconds;
    public static implicit operator TimeSpan(Microseconds microseconds) => TimeSpan.FromMicroseconds(microseconds.Value);
#endif
    public static implicit operator Microseconds(double microseconds) => new Microseconds(microseconds);
    public static implicit operator double(Microseconds microseconds) => microseconds.Value;

    #endregion

    #region Unary operators

    public static Microseconds operator +(Microseconds nanoseconds) => +nanoseconds.Value;
    public static Microseconds operator -(Microseconds nanoseconds) => -nanoseconds.Value;

    #endregion

    #region Equality operators

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Microseconds left, Minutes right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Microseconds left, Minutes right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Microseconds left, Seconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Microseconds left, Seconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Microseconds left, Milliseconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Microseconds left, Milliseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Microseconds left, Microseconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Microseconds left, Microseconds right) => !(left == right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)" />
    public static bool operator ==(Microseconds left, Nanoseconds right) => left.Equals(right);

    /// <inheritdoc cref="System.Numerics.IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)" />
    public static bool operator !=(Microseconds left, Nanoseconds right) => !(left == right);

    #endregion Equality operators
  }
}