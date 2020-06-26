using System;
using System.Globalization;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Core.Wpf.Converter
{
  [ValueConversion(typeof(double), typeof(double))]
  public class DividerValueConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return Binding.DoNothing;
      }

      double divisor = 1;

      if (parameter != null)
      {
        divisor = ((double) parameter).Equals(0) ? 1 : (double) parameter;
      }

      return (double) value / divisor;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return Binding.DoNothing;
      }

      double multiplier = 1;

      if (parameter != null)
      {
        multiplier = ((double)parameter).Equals(0) ? 1 : (double)parameter;
      }

      return (double)value * multiplier;
    }

    #endregion
  }
}
