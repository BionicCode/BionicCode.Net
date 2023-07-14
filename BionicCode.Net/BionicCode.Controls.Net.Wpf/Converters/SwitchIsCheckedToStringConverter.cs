namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Globalization;
  using System.Windows.Data;

  [ValueConversion(typeof(bool), typeof(string))]
  class SwitchIsCheckedToStringConverter : IValueConverter
  {
    #region Implementation of IValueConverter

    /// <summary>
    /// Konvertiert einen Wert. 
    /// </summary>
    /// <returns>
    /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
    /// </returns>
    /// <param name="value">Der von der Bindungsquelle erzeugte Wert.</param><param name="targetType">Der Typ der Bindungsziel-Eigenschaft.</param><param name="parameter">Der zu verwendende Konverterparameter.</param><param name="culture">Die im Konverter zu verwendende Kultur.</param>
    public object Convert(object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      if (!(value is bool))
      {
        throw new ArgumentException("Wrong conversion type. Only conversion from bool to srting supported.", nameof(value));
      }

      return (bool) value 
        ? Resources.SwitchStateToggled 
        : Resources.SwitchStateUntoggled;
    }

    /// <summary>
    /// Konvertiert einen Wert. 
    /// </summary>
    /// <returns>
    /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
    /// </returns>
    /// <param name="value">Der Wert, der vom Bindungsziel erzeugt wird.</param><param name="targetType">Der Typ, in den konvertiert werden soll.</param><param name="parameter">Der zu verwendende Konverterparameter.</param><param name="culture">Die im Konverter zu verwendende Kultur.</param>
    public object ConvertBack(object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
