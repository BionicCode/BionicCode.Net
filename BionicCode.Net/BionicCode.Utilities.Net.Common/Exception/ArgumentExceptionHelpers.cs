namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.Numerics;
  using System.Runtime.CompilerServices;
  using System.Runtime.Serialization;

#if NET5_0_OR_GREATER || NETCOREAPP
  using NotNullAttribute = System.Diagnostics.CodeAnalysis.NotNullAttribute;
#endif

  public class ArgumentNullExceptionEx : System.ArgumentNullException
  {
    public ArgumentNullExceptionEx()
    {
    }

    public ArgumentNullExceptionEx(string paramName) : base(paramName)
    {
    }

    public ArgumentNullExceptionEx(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentNullExceptionEx(string paramName, string message) : base(paramName, message)
    {
    }

#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNull(object argument, string paramName = null)
#endif
    {
#if NET6_0_OR_GREATER
      ArgumentNullException.ThrowIfNull(argument, paramName);
#else
      if (argument is null)
      {
        Throw(paramName);
      }
#endif
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNullOrEmpty(string argument, string paramName = null)
#endif
    {
      ArgumentExceptionEx.ThrowIfNullOrEmpty(argument, paramName);
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNullOrWhiteSpace(string argument, string paramName = null)
#endif
    {
      ArgumentExceptionEx.ThrowIfNullOrWhiteSpace(argument, paramName);
    }

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void Throw(string paramName)
      => throw new ArgumentNullException(paramName);
  }

  public class ArgumentExceptionEx : ArgumentException
  {
    public ArgumentExceptionEx()
    {
    }

    public ArgumentExceptionEx(string message) : base(message)
    {
    }

    public ArgumentExceptionEx(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentExceptionEx(string message, string paramName) : base(message, paramName)
    {
    }

    public ArgumentExceptionEx(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
    {
    }

#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNull(object argument, string paramName = null)
#endif
    {
      ArgumentNullExceptionEx.ThrowIfNull(argument, paramName);
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNullOrEmpty(string argument, string paramName = null)
#endif
    {
#if NET7_0_OR_GREATER
      ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
#else
      if (string.IsNullOrEmpty(argument))
      {
        ThrowNullOrEmptyException(argument, paramName);
      }
#endif
    }

    /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNullOrWhiteSpace(string argument, string paramName = null)
#endif
    {
#if NET8_0_OR_GREATER
      ArgumentException.ThrowIfNullOrWhiteSpace(argument, paramName);
#else
      if (string.IsNullOrWhiteSpace(argument))
      {
        ThrowNullOrWhiteSpaceException(argument, paramName);
      }
#endif
    }

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNullOrEmptyException(string argument, string paramName)
      => throw new ArgumentException("Argument is NULL or empty", paramName);

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNullOrWhiteSpaceException(string argument, string paramName)
      => throw new ArgumentException("Argument is NULL or empty or consists of white spaces", paramName);
  }

  public class ArgumentOutOfRangeExceptionEx : System.ArgumentOutOfRangeException
  {
    public ArgumentOutOfRangeExceptionEx()
    {
    }

    public ArgumentOutOfRangeExceptionEx(string paramName) : base(paramName)
    {
    }

    public ArgumentOutOfRangeExceptionEx(string paramName, string message) : base(paramName, message)
    {
    }

    public ArgumentOutOfRangeExceptionEx(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentOutOfRangeExceptionEx(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
    {
    }

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowZero<T>(T value, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, "Value must be non-zero.");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNegative<T>(T value, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, "Value must be non-negative.");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNegativeOrZero<T>(T value, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, "Value must be non-negative and non-zero.");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowGreater<T>(T value, T other, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, $"Value must be less or equal to {other}");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowGreaterEqual<T>(T value, T other, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, $"Value must be less than {other}");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowLess<T>(T value, T other, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, $"Value must be greater or equal to {other}");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowLessEqual<T>(T value, T other, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, $"Value must be greater than {other}");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowEqual<T>(T value, T other, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, $"Value must not be equal to {(other?.ToString() ?? "NULL")}");

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNotEqual<T>(T value, T other, string paramName)
      => throw new System.ArgumentOutOfRangeException(paramName, value, $"Value must be equal to {(other?.ToString() ?? "NULL")}");

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is zero.</summary>
    /// <param name="value">The argument to validate as non-zero.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfZero<T>(T value, string paramName = null)
#endif
#if NET7_0_OR_GREATER
        where T : INumberBase<T>
#else
        where T : IConvertible
#endif
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfZero(value, nameof(paramName));
#elif NET7_0_OR_GREATER
      if (T.IsZero(value))
      {
        ThrowZero(value, paramName);
      }
#else
      if (Convert.ToDouble(value) == 0)
      {
        ThrowZero(value, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is negative.</summary>
    /// <param name="value">The argument to validate as non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfNegative<T>(T value, string paramName = null)
#endif
#if NET7_0_OR_GREATER
        where T : INumberBase<T>
#else
        where T : IConvertible
#endif
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfNegative(value, nameof(paramName));
#elif NET7_0_OR_GREATER
      if (T.IsNegative(value))
      {
        ThrowNegative(value, paramName);
      }
#else
      if (Convert.ToDouble(value) < 0)
      {
        ThrowNegative(value, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is negative or zero.</summary>
    /// <param name="value">The argument to validate as non-zero or non-negative.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfNegativeOrZero<T>(T value, string paramName = null)
#endif
#if NET7_0_OR_GREATER
        where T : INumberBase<T>
#else
        where T : IConvertible
#endif
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, nameof(paramName));
#elif NET7_0_OR_GREATER
      if (T.IsNegative(value) || T.IsZero(value))
      {
        ThrowNegativeOrZero(value, paramName);
      }
#else
      if (Convert.ToDouble(value) <= 0)
      {
        ThrowNegativeOrZero(value, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEquatable<T>?
#else
    public static void ThrowIfEqual<T>(T value, T other, string paramName = null) where T : IEquatable<T>
#endif
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfEqual(value, other, nameof(paramName));
#else
      if (EqualityComparer<T>.Default.Equals(value, other))
      {
        ThrowEqual(value, other, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEquatable<T>?
#else
    public static void ThrowIfNotEqual<T>(T value, T other, string paramName = null) where T : IEquatable<T>
#endif
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfNotEqual(value, other, nameof(paramName));
#else
      if (!EqualityComparer<T>.Default.Equals(value, other))
      {
        ThrowNotEqual(value, other, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as less or equal than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfGreaterThan<T>(T value, T other, string paramName = null)
#endif
        where T : IComparable<T>
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfGreaterThan(value, other, nameof(paramName));
#else
      if (value.CompareTo(other) > 0)
      {
        ThrowGreater(value, other, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is greater than or equal <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, string paramName = null)
#endif
        where T : IComparable<T>
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(value, other, nameof(paramName));
#else
      if (value.CompareTo(other) >= 0)
      {
        ThrowGreaterEqual(value, other, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is less than <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as greater than or equal than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfLessThan<T>(T value, T other, string paramName = null)
#endif
        where T : IComparable<T>
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfLessThan(value, other, nameof(paramName));
#else
      if (value.CompareTo(other) < 0)
      {
        ThrowLess(value, other, paramName);
      }
#endif
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeExceptionEx"/> if <paramref name="value"/> is less than or equal <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as greater than <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
#else
    public static void ThrowIfLessThanOrEqual<T>(T value, T other, string paramName = null)
#endif
        where T : IComparable<T>
    {
#if NET8_0_OR_GREATER
      ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(value, other, nameof(paramName));
#else
      if (value.CompareTo(other) <= 0)
      {
        ThrowLessEqual(value, other, paramName);
      }
#endif
    }
  }
}
