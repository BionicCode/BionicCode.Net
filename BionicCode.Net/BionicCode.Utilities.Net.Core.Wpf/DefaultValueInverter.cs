﻿#region Info

// //  
// BionicCode.BionicUtilities.Net.Core.Wpf

#endregion

using System;
using System.Globalization;
using System.Windows;
using BionicCode.Utilities.Net.Standard;

namespace BionicCode.Utilities.Net.Core.Wpf
{
  /// <summary>
  /// Class that inverts a value.
  /// Supports inversion of <see cref="byte"/>, <see cref="int"/>, <see cref="double"/>, <see cref="decimal"/>, <see cref="float"/>, <see cref="bool"/> and <see cref="Visibility"/>. <para>This class can be used with the <see cref="BionicCode.Utilities.Net.Core.Wpf.Markup.InvertExtension"/>.</para>
  /// </summary>
  /// <remarks>The <see cref="DefaultValueInverter"/> will check if the value is of type string. The string representation is then converted to the native type, inverted and then converted back to string using the provided <see cref="object.ToString"/> implementation provided by the native type.</remarks>
  /// <seealso cref="BionicCode.Utilities.Net.Core.Wpf.Markup.InvertExtension"/>
  public class DefaultValueInverter : IValueInverter
  {
    #region Implementation of IValueInverter

    /// <inheritdoc />
    public object InvertValue(object value)
    {
      if (value == null ||
          value is string stringValue && (string.IsNullOrWhiteSpace(stringValue) || stringValue.Length == 1 && (stringValue.StartsWith("-") || stringValue.StartsWith("+"))))
      {
        return value;
      }
      if (TryInvertValue(value, out object invertedValue))
      {
        return invertedValue;
      }
      throw new InvalidOperationException($"The value {value} could not be inverted.");
    }

    /// <inheritdoc />
    public bool TryInvertValue(object value, out object invertedValue)
    {
      invertedValue = null;
      bool valueIsString = false;
      if (TryConvertStringToNumber(value, out object number))
      {
        valueIsString = true;
        value = number;
      }

      switch (value)
      {
        case bool boolValue:
          invertedValue = !boolValue;
          break;
        case int intValue:
          invertedValue = intValue * -1;
          break;
        case decimal decimalValue:
          invertedValue = decimalValue * decimal.MinusOne;
          break;
        case double doubleValue:
          invertedValue = double.IsNaN(doubleValue)
            ? doubleValue
            : double.IsNegativeInfinity(doubleValue)
              ? double.PositiveInfinity
              : double.IsPositiveInfinity(doubleValue)
                ? double.NegativeInfinity
                : doubleValue * -1.0;
          break;
        case float floatValue:
          invertedValue = float.IsNaN(floatValue)
            ? floatValue
            : float.IsNegativeInfinity(floatValue)
              ? float.PositiveInfinity
              : float.IsPositiveInfinity(floatValue)
                ? float.NegativeInfinity
                : floatValue * -1.0;
          break;
        case byte byteValue:
          invertedValue = ~byteValue;
          break;
        case Visibility visibilityValue:
          invertedValue = visibilityValue.Equals(Visibility.Hidden) || visibilityValue.Equals(Visibility.Collapsed)
            ? Visibility.Visible
            : Visibility.Collapsed;
          break;
      }

      if (valueIsString)
      {
        invertedValue = invertedValue?.ToString();
      }
      return invertedValue != null;
    }

    #endregion Implementation of IValueInverter

    private bool TryConvertStringToNumber(object value, out object numericValue)
    {
      numericValue = null;
      if (!(value is string stringValue))
      {
        return false;
      }

      if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.CurrentUICulture, out double doubleValue))
      {
        numericValue = doubleValue;
      }
      else if (int.TryParse(stringValue, out int integer))
      {
        numericValue = integer;
      }
      else if (bool.TryParse(stringValue, out bool boolValue))
      {
        numericValue = boolValue;
      }

      return numericValue != null;
    }
  }
}