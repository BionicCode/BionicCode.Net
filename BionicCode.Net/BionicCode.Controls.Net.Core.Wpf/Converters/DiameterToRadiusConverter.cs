#region Info

// 2021/01/10  19:28
// BionicCode.Controls.Net.Core.Wpf

#endregion

using System;
using System.Globalization;
using System.Windows.Data;

namespace BionicCode.Controls.Net.Core.Wpf.Converters
{
  public class DiameterToRadiusConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is double doubleValue ? doubleValue / 2 : value is string stringValue && double.TryParse(stringValue,  out doubleValue) ? doubleValue / 2 : Binding.DoNothing;

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is double doubleValue ? doubleValue * 2 : value is string stringValue && double.TryParse(stringValue, out doubleValue) ? doubleValue * 2 : Binding.DoNothing;

    #endregion
  }
}