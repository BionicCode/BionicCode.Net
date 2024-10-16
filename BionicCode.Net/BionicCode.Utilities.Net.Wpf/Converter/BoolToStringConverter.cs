namespace BionicCode.Utilities.Net
{
#if !NETSTANDARD
  using System;
  using System.Globalization;
  using System.Windows;
  using System.Windows.Data;
  /// <summary>
  /// Implementation of <see cref="IValueConverter"/> that converts a <see cref="bool"/> to a custom string representation e.g., convert <c>true</c> to <c>"Enabled"</c>.
  /// </summary>
  /// <example>
  /// <code>
  /// &lt;ToggleButton IsChecked="{Binding IsEnabled}"&gt;
  ///   &lt;ToggleButton.Content&gt;
  ///     &lt;Binding Path="IsEnabled"&gt;
  ///       &lt;Binding.Converter&gt;
  ///         &lt;BoolToStringConverter TrueValue="On" FalseValue="{Binding DisabledText}" NullValue="Undefined" /&gt;
  ///       &lt;/Binding.Converter&gt;
  ///     &lt;/Binding&gt;
  ///   &lt;/ToggleButton.Content&gt;
  /// &lt;/ToggleButton&gt;
  /// </code>
  /// </example>
  [ValueConversion(typeof(bool), typeof(string))]
  public class BoolToStringConverter : DependencyObject, IValueConverter
  {
    /// <summary>
    /// The <see cref="DependencyProperty"/> of the <see cref="TrueValue"/> property.
    /// </summary>
    public static readonly DependencyProperty TrueValueProperty = DependencyProperty.Register(
      "TrueValue",
      typeof(string),
      typeof(BoolToStringConverter),
      new PropertyMetadata("True"));

    /// <summary>
    /// The value to show in case the converter input evaluates to <c>true</c>.
    /// </summary>
    /// <value>A string representation of <see cref="bool"/> that evaluates to <c>true</c>.</value>
    public string TrueValue
    {
      get => (string)GetValue(BoolToStringConverter.TrueValueProperty);
      set => SetValue(BoolToStringConverter.TrueValueProperty, value);
    }

    /// <summary>
    /// The <see cref="DependencyProperty"/> of the <see cref="FalseValue"/> property.
    /// </summary>
    public static readonly DependencyProperty FalseValueProperty = DependencyProperty.Register(
      "FalseValue",
      typeof(string),
      typeof(BoolToStringConverter),
      new PropertyMetadata("False"));

    /// <summary>
    /// The value to show in case the converter input evaluates to <c>false</c>.
    /// </summary>
    /// <value>A string representation of <see cref="bool"/> that evaluates to <c>false</c>.</value>
    public string FalseValue
    {
      get => (string)GetValue(BoolToStringConverter.FalseValueProperty);
      set => SetValue(BoolToStringConverter.FalseValueProperty, value);
    }

    /// <summary>
    /// The <see cref="DependencyProperty"/> of the <see cref="NullValue"/> property.
    /// </summary>
    public static readonly DependencyProperty NullValueProperty = DependencyProperty.Register(
      "NullValue",
      typeof(string),
      typeof(BoolToStringConverter),
      new PropertyMetadata("Unset"));

    /// <summary>
    /// The value to show in case the converter input evaluates to <c>true</c>.
    /// </summary>
    /// <value>A string representation of <see cref="Nullable"/> that evaluates to <c>null</c>.</value>
    public string NullValue
    {
      get => (string)GetValue(BoolToStringConverter.NullValueProperty);
      set => SetValue(BoolToStringConverter.NullValueProperty, value);
    }

    #region Implementation of IValueConverter

    /// <inheritdoc />
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

    /// <inheritdoc />
    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture) =>
      (value as string)?.Equals(this.TrueValue, StringComparison.OrdinalIgnoreCase) ?? false;

    #endregion
  }
#endif
}