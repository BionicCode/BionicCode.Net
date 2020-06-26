#region Info

// //  
// BionicCode.BionicUtilities.Net.Core.Wpf

#endregion

namespace BionicCode.Utilities.Net.Standard
{
  public interface IValueInverter
  {
    bool TryInvertValue(object value, out object invertedValue);
    object InvertValue(object value);
  }
}