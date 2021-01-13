#region Info

// //  
// BionicCode.BionicUtilities.Net.Core.Wpf

#endregion

namespace BionicCode.Utilities.Net.Wpf.Markup
{
  /// <summary>
  /// Enumeration to set the inversion direction of the <see cref="InvertExtension"/>.
  /// </summary>
  public enum InversionMode
  {
    /// <summary>
    /// Default/Unset value
    /// </summary>
    Default = 0, 
    /// <summary>
    /// Only invert value from binding source to target.
    /// </summary>
    OneWay, 
    /// <summary>
    /// Only invert from binding target to source.
    /// </summary>
    OneWayToSource, 
    /// <summary>
    /// Invert bi-directional.
    /// </summary>
    TwoWay, 
    /// <summary>
    /// Only invert on initialization (from source to target).
    /// </summary>
    OneTime
  }
}