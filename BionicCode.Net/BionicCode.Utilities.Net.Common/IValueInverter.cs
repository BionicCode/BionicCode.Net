#region Info

// //  
// BionicCode.BionicUtilities.Net.Core.Wpf

#endregion

using System;

namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Provide an implementation to invert an object or objects.
  /// </summary>
  public interface IValueInverter
  {
    /// <summary>
    /// Trys to invert a value. Won't throw an exception if operation fails.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="invertedValue"></param>
    /// <returns><c>true</c> when successful, otherwise <c>false</c></returns>
    bool TryInvertValue(object value, out object invertedValue);

    /// <summary>
    /// Throws an exception if operation has failed.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <returns>The inverted value.</returns>
    object InvertValue(object value);
  }
}