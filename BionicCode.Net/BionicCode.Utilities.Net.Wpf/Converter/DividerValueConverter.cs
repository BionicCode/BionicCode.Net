namespace BionicCode.Utilities.Net
{
#if !NETSTANDARD
  using System;
  using System.Globalization;
  using System.Windows.Data;
  /// <summary>
  /// An <see cref="IValueConverter"/> implementation that enables to devide a bound numeric value by a divisor value provided via the <see cref="Binding.ConverterParameter"/> property.
  /// </summary>
  [ValueConversion(typeof(double), typeof(double))]
  public class DividerValueConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return Binding.DoNothing;
      }

      double divident = value is double doubleValue
        ? doubleValue
        : value is IConvertible convertibleValue
          ? System.Convert.ToDouble(convertibleValue)
          : parameter is string stringValue && double.TryParse(stringValue, out doubleValue)
            ? doubleValue
            : 1;

      double divisor = parameter is double doubleParameter
        ? doubleParameter
        : parameter is int intParameter
        ? System.Convert.ToDouble(intParameter)
        : parameter is string stringParameter && double.TryParse(stringParameter, out doubleParameter)
          ? doubleParameter
          : 1;
      if (divisor.Equals(0))
      {
        divisor = 1;
      }

      return divident / divisor;
    }

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return Binding.DoNothing;
      }

      double multiplier = value is double doubleValue
        ? doubleValue
        : parameter is int intValue
          ? System.Convert.ToDouble(intValue)
          : parameter is string stringValue && double.TryParse(stringValue, out doubleValue)
            ? doubleValue
            : 1;

      double multiplicant = parameter is double doubleParameter
        ? doubleParameter
        : parameter is int intParameter
          ? System.Convert.ToDouble(intParameter)
          : parameter is string stringParameter && double.TryParse(stringParameter, out doubleParameter)
            ? doubleParameter
            : 1;
      return multiplier * multiplicant;
    }

    #endregion
  }
#endif
}
