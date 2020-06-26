using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Framework.Converter
{
  [ValueConversion(typeof(object), typeof(object))]
  public class InvertValueConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      switch (value)
      {
        case double doubleValue:
          return doubleValue * -1;
        case int intValue:
          return intValue * -1;
        case bool boolValue:
          return !boolValue;
        case Visibility visibilityValue:
          return visibilityValue.Equals(Visibility.Hidden) || visibilityValue.Equals(Visibility.Collapsed)
            ? Visibility.Visible
            : Visibility.Collapsed;
        default:
          return Binding.DoNothing;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      Convert(value, targetType, parameter, culture);

    #endregion
  }
}