using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace BionicCode.Utilities.Net.Uwp.Converter
{
  public class DividerValueConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      if (parameter == null)
      {
        return value;
      }

      double divisor = ((double) parameter).Equals(0) ? 1 : (double) parameter;
      return (double)value / divisor;
    }

    
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
      if (parameter == null)
      {
        return value;
      }

      double multiplier = ((double)parameter).Equals(0) ? 1 : (double)parameter;
      return (double)value * multiplier;
    }

    #endregion
  }
}
