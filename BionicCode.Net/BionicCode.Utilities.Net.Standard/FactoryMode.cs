#region Info

// 2020/09/19  17:17
// BionicCode.Utilities.Net.Standard

#endregion

namespace BionicCode.Utilities.Net.Standard
{
  /// <summary>
  /// Describes lifetime scope of object instances
  /// </summary>
  public enum FactoryMode
  {
    /// <summary>
    /// Unset
    /// </summary>
    Default = 0, 
    /// <summary>
    /// Create a shared instance
    /// </summary>
    Singleton, 
    /// <summary>
    /// A new instance is created for each request
    /// </summary>
    Transient
  }
}