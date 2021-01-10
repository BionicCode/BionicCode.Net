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

      double divisor = parameter is double doubleValue 
        ? doubleValue.Equals(0) 
          ? 1 
          : doubleValue
        : parameter is string stringValue && double.TryParse(stringValue, out doubleValue) 
          ? doubleValue
          : 1;

      return (double) value / divisor;
    }

    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
      {
        return Binding.DoNothing;
      }

      double divisor = parameter is double doubleValue
        ? doubleValue.Equals(0)
          ? 1
          : doubleValue
        : parameter is string stringValue && double.TryParse(stringValue, out doubleValue)
          ? doubleValue
          : 1;

      return (double)value * divisor;
    }

    #endregion
  }
}
