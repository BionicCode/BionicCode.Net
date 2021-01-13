using System;
using System.Globalization;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Wpf.Converter
{
  public class PercentageToPixelConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double baseValue = value is IConvertible convertibleValue
        ? System.Convert.ToDouble(convertibleValue)
        : 0;
      double percentage = parameter is IConvertible convertibleParameter
        ? System.Convert.ToDouble(convertibleParameter)
        : 0;

      return percentage / 100 * baseValue;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double baseValue = value is IConvertible convertibleValue
        ? System.Convert.ToDouble(convertibleValue)
        : 0;
      double percentage = parameter is IConvertible convertibleParameter
        ? System.Convert.ToDouble(convertibleParameter)
        : 0;

      double pixelValue = baseValue * 100 / percentage;
      return pixelValue;
    }

    #endregion
  }
}
