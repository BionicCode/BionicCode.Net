namespace BionicCode.Utilities.Net
{
#if NET || NET461_OR_GREATER
  using System;
  using System.Globalization;
  using System.Linq;
using System.Windows.Data;
  /// <summary>
  /// An <see cref="IMultiValueConverter"/> implementation that checks if all values are <c>true</c>. Supports <see cref="BindingMode.OneWay"/> or <see cref="BindingMode.OneTime"/> only.
  /// </summary>
  public class BooleanMultiValueConverter : IMultiValueConverter
  {
  #region Implementation of IMultiValueConverter

    /// <inheritdoc />
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) => values?.OfType<bool>().All(value => value);

    /// <summary>
    /// Not supported.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Back conversion is not supported.</exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();

  #endregion
  }
#endif
}