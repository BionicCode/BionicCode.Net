namespace BionicCode.Utilities.Net
{
  using System;
  using System.Globalization;
  using System.Windows;

#if NET || NET461_OR_GREATER
using System.Windows.Data;
  /// <summary>
  /// Implementation of <see cref="IValueConverter"/> that inverts <see cref="bool"/>, <see cref="Visibility"/>, <see cref="int"/>, <see cref="double"/>, <see cref="decimal"/> and <see cref="float"/>
  /// </summary>
  [ValueConversion(typeof(object), typeof(object))]
  public class InvertValueConverter : IValueConverter
  {
  #region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
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
          return visibilityValue.Equals(Visibility.Hidden) || visibilityValue.Equals(Visibility.Collapsed)
            ? Visibility.Visible
            : Visibility.Collapsed;
        default:
          return Binding.DoNothing;
      }
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      Convert(value, targetType, parameter, culture);

  #endregion
  }
#endif
}