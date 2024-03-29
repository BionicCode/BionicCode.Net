﻿#region Info

// 2020/09/19  17:17
// BionicCode.Utilities.Net

#endregion

namespace BionicCode.Utilities.Net
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
    Transient,
    /// <summary>
    /// Create an instance that is shared iside a particular scope
    /// </summary>
    Scoped
  }
}