namespace BionicCode.Utilities.Net
{
  #region Info

  // 2021/01/13  12:51
  // Clock

  #endregion

  using System;
  using System.Globalization;
  using System.Windows;

#if !NETSTANDARD
  using System.Windows.Data;
  public class PercentageToThicknessConverter : IValueConverter
  {
    public Thickness ThicknessPercentage { get; set; }

    #region Implementation of IValueConverter

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      double baseValue = value is IConvertible convertibleValue
        ? System.Convert.ToDouble(convertibleValue)
        : 0;
      double left = this.ThicknessPercentage.Left / 100 * baseValue;
      double top = this.ThicknessPercentage.Top / 100 * baseValue;
      double right = this.ThicknessPercentage.Right / 100 * baseValue;
      double bottom = this.ThicknessPercentage.Bottom / 100 * baseValue;

      return new Thickness(left, top, right, bottom);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
      throw new NotSupportedException();

    #endregion
  }
#endif
}