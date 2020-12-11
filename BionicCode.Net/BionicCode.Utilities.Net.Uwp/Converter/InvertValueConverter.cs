using System;
using System.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace BionicCode.Utilities.Net.Uwp.Converter
{
  /// <summary>
  /// Implementation of <see cref="IValueConverter"/> that inverts <see cref="bool"/>, <see cref="Visibility"/>, <see cref="int"/>, <see cref="double"/>, <see cref="decimal"/> and <see cref="float"/>
  /// </summary>
  public class InvertValueConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
      switch (value)
      {
        case double doubleValue:
          return -doubleValue;
        case decimal decimalValue:
          return -decimalValue;
        case float floatValue:
          return -floatValue;
        case int intValue:
          return -intValue;
        case bool boolValue:
          return !boolValue;
        case Visibility visibilityValue:
          return visibilityValue.Equals(Visibility.Collapsed) 
            ? Visibility.Visible
            : Visibility.Collapsed;
        default:
          return value;
      }
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
      Convert(value, targetType, parameter, language);

    #endregion
  }
}