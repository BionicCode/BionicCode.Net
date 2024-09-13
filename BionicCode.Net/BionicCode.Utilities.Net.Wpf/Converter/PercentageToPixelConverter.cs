namespace BionicCode.Utilities.Net
{
  using System;
  using System.Globalization;

#if !NETSTANDARD
  using System.Windows.Data;
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
#endif
}
