using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Framework.Converter
{
  [ValueConversion(typeof(double), typeof(bool))]
  public class IsGreaterThanValueConverter : DependencyObject, IValueConverter
  {
    public static readonly DependencyProperty ComparerProperty = DependencyProperty.Register(
      "Comparer",
      typeof(Comparer),
      typeof(IsGreaterThanValueConverter),
      new PropertyMetadata(Comparer.Default));

    public Comparer Comparer
    {
      get => (Comparer) GetValue(IsGreaterThanValueConverter.ComparerProperty);
      set => SetValue(IsGreaterThanValueConverter.ComparerProperty, value);
    }

    public static readonly DependencyProperty CompareValueProperty = DependencyProperty.Register(
      "CompareValue",
      typeof(object),
      typeof(IsGreaterThanValueConverter),
      new PropertyMetadata(default(object)));

    public object CompareValue
    {
      get => GetValue(IsGreaterThanValueConverter.CompareValueProperty);
      set => SetValue(IsGreaterThanValueConverter.CompareValueProperty, value);
    }
    #region Implementation of IValueConverter

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      object compareValue = this.CompareValue ?? parameter;
      return this.Comparer.Compare(value, compareValue) > 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotSupportedException();
    }

    #endregion
  }
}
