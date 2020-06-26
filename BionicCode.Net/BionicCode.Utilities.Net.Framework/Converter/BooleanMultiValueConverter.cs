using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Framework.Converter
{
  public class BooleanMultiValueConverter : IMultiValueConverter
  {
    #region Implementation of IMultiValueConverter

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => values?.OfType<bool>().All(value => value);

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();

    #endregion
  }
}