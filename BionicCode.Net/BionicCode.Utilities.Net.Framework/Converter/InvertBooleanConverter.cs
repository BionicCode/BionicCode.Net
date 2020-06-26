using System;
using System.Globalization;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Framework.Converter
{
  [ValueConversion(typeof(bool), typeof(bool))]
  public class InvertBooleanConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return !boolValue;
      }

      return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      Convert(value, targetType, parameter, culture);

    #endregion
  }
}