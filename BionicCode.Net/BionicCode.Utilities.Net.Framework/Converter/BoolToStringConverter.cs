using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BionicCode.Utilities.Net.Framework.Converter
{
  [ValueConversion(typeof(bool), typeof(string))]
  public class BoolToStringConverter : DependencyObject, IValueConverter
  {
    public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register(
      "TrueValue",
      typeof(string),
      typeof(BoolToStringConverter),
      new PropertyMetadata("True"));

    public string TrueValue
    {
      get => (string) GetValue(BoolToStringConverter.TrueValueProperty);
      set => SetValue(BoolToStringConverter.TrueValueProperty, value);
    }

    public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register(
      "FalseValue",
      typeof(string),
      typeof(BoolToStringConverter),
      new PropertyMetadata("False"));

    public string FalseValue
    {
      get => (string) GetValue(BoolToStringConverter.FalseValueProperty);
      set => SetValue(BoolToStringConverter.FalseValueProperty, value);
    }


    public static readonly DependencyProperty NullValueProperty = DependencyProperty.Register(
      "NullValue",
      typeof(string),
      typeof(BoolToStringConverter),
      new PropertyMetadata("Unset"));

    public string NullValue
    {
      get => (string) GetValue(BoolToStringConverter.NullValueProperty);
      set => SetValue(BoolToStringConverter.NullValueProperty, value);
    }


    #region Implementation of IValueConverter

    public object Convert(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      if (value is bool isTrue)
      {
        return isTrue
          ? this.TrueValue
          : this.FalseValue;
      }

      return this.NullValue;
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture) =>
      (value as string)?.Equals(this.TrueValue, StringComparison.OrdinalIgnoreCase) ?? false;

    #endregion
  }
}