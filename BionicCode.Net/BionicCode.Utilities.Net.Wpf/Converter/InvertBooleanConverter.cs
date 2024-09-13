namespace BionicCode.Utilities.Net
{
  using System;
  using System.Globalization;
#if !NETSTANDARD
  using System.Windows.Data;
  /// <summary>
  /// Implementation of <see cref="IValueConverter"/> that inverts a bool value.
  /// </summary>
  [ValueConversion(typeof(bool), typeof(bool))]
  public class InvertBooleanConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is bool boolValue)
      {
        return !boolValue;
      }

      return Binding.DoNothing;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      Convert(value, targetType, parameter, culture);

    #endregion
  }
#endif
}