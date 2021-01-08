using System;
using System.Globalization;
using System.Windows.Data;

namespace BionicCode.Controls.Net.Core.Wpf.Converters
{
  class SwitchWidthToTotalWidthConverter : IMultiValueConverter
  {
    #region Implementation of IMultiValueConverter

    /// <summary>
    /// Konvertiert einen Wert. 
    /// </summary>
    /// <returns>
    /// Ein konvertierter Wert.Wenn die Methode null zurückgibt, wird der gültige NULL-Wert verwendet.
    /// </returns>
    /// <param name="widthParameters">Der von der Bindungsquelle erzeugte Wert.</param><param name="targetType">Der Typ der Bindungsziel-Eigenschaft.</param><param name="labelWidthParameter">Der zu verwendende Konverterparameter.</param><param name="culture">Die im Konverter zu verwendende Kultur.</param>
    public object Convert(object[] widthParameters,
      Type targetType,
      object labelWidthParameter,
      CultureInfo culture)
    {
      double desiredSwitchWidth = (double) widthParameters[0];
      double switchLabelWidth = (double) widthParameters[1];
      var totalControlWidth = desiredSwitchWidth + switchLabelWidth;
      return totalControlWidth > -1 ? totalControlWidth : 0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
