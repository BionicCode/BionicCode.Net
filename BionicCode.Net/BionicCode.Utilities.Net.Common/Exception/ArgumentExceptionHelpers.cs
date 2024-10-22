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

#if !NET6_0_OR_GREATER
  public class ArgumentNullException : System.ArgumentNullException
  {
    public ArgumentNullException()
    {
    }

    public ArgumentNullException(string paramName) : base(paramName)
    {
    }

    public ArgumentNullException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentNullException(string paramName, string message) : base(paramName, message)
    {
    }

#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNull(object argument, string paramName = null)
#endif
    {
      if (argument is null)
      {
        Throw(paramName);
      }
    }

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void Throw(string paramName) => throw new System.ArgumentNullException(paramName);
  }

    public class ArgumentException : System.ArgumentException
  {
    public ArgumentException()
    {
    }

    public ArgumentException(string message) : base(message)
    {
    }

    public ArgumentException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentException(string message, string paramName) : base(message, paramName)
    {
    }

    public ArgumentException(string message, string paramName, Exception innerException) : base(message, paramName, innerException)
    {
    }

#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
#else
    public static void ThrowIfNull(object argument, string paramName = null)
#endif
    {
      if (argument is null)
      {
        Throw(paramName);
      }
    }

#if !NET7_0_OR_GREATER
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
      if (string.IsNullOrEmpty(argument))
      {
        ThrowNullOrEmptyException(argument, paramName);
      }
    }
#endif

#if !NET8_0_OR_GREATER
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
      if (string.IsNullOrWhiteSpace(argument))
      {
        ThrowNullOrWhiteSpaceException(argument, paramName);
      }
    }
#endif

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void Throw(string paramName) => throw new System.ArgumentNullException(paramName);

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNullOrEmptyException(string argument, string paramName)
    {
      ArgumentNullException.ThrowIfNull(argument, paramName);
      throw new System.ArgumentException("Argument is NULL or empty", paramName);
    }

#if NET5_0_OR_GREATER || NETCOREAPP || NETSTANDARD2_1_OR_GREATER
    [DoesNotReturn]
#endif
    private static void ThrowNullOrWhiteSpaceException(string argument, string paramName)
    {
      ArgumentNullException.ThrowIfNull(argument, paramName);
      throw new System.ArgumentException("Argument is NULL or empty or consists of white spaces", paramName);
    }
  }

#endif

#if !NET8_0_OR_GREATER
  public class ArgumentOutOfRangeException : System.ArgumentOutOfRangeException
  {
    public ArgumentOutOfRangeException()
    {
    }

    public ArgumentOutOfRangeException(string paramName) : base(paramName)
    {
    }

    public ArgumentOutOfRangeException(string paramName, string message) : base(paramName, message)
    {
    }

    public ArgumentOutOfRangeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ArgumentOutOfRangeException(string paramName, object actualValue, string message) : base(paramName, actualValue, message)
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

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.</summary>
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
#if NET7_0_OR_GREATER
      if (T.IsZero(value))
#else
      if (Convert.ToDouble(value) == 0)
#endif
      {
        ThrowZero(value, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative.</summary>
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
#if NET7_0_OR_GREATER
      if (T.IsNegative(value))
#else
      if (Convert.ToDouble(value) < 0)
#endif
      {
        ThrowNegative(value, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is negative or zero.</summary>
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
#if NET7_0_OR_GREATER
      if (T.IsNegative(value) || T.IsZero(value))
#else
      if (Convert.ToDouble(value) <= 0)
#endif
      {
        ThrowNegativeOrZero(value, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEquatable<T>?
#else
    public static void ThrowIfEqual<T>(T value, T other, string paramName = null) where T : IEquatable<T>
#endif
    {
      if (EqualityComparer<T>.Default.Equals(value, other))
      {
        ThrowEqual(value, other, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
    /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
    /// <param name="other">The value to compare with <paramref name="value"/>.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
#if NET5_0_OR_GREATER || NETCOREAPP
    public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IEquatable<T>?
#else
    public static void ThrowIfNotEqual<T>(T value, T other, string paramName = null) where T : IEquatable<T>
#endif
    {
      if (!EqualityComparer<T>.Default.Equals(value, other))
      {
        ThrowNotEqual(value, other, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
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
      if (value.CompareTo(other) > 0)
      {
        ThrowGreater(value, other, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal <paramref name="other"/>.</summary>
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
      if (value.CompareTo(other) >= 0)
      {
        ThrowGreaterEqual(value, other, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.</summary>
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
      if (value.CompareTo(other) < 0)
      {
        ThrowLess(value, other, paramName);
      }
    }

    /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal <paramref name="other"/>.</summary>
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
      if (value.CompareTo(other) <= 0)
      {
        ThrowLessEqual(value, other, paramName);
      }
    }
  }
#endif
  }
