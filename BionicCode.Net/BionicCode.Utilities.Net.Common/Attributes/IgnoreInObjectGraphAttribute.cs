namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  /// <summary>
  /// Attribute to decorate class members that should be ignored when traversing an instance's object graph.
  /// </summary>
  /// <remarks>The <see cref="IgnoreInObjectGraphAttribute"/> can be used in conjunction with the <see cref="HelperExtensionsCommon.ToDictionary(object)"/> and <see cref="HelperExtensionsCommon.ToFlatDictionary(object)"/> extension methods.</remarks>
  [System.AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]  
  public sealed class IgnoreInObjectGraphAttribute : Attribute
  {
    /// <summary>
    /// The default constructor
    /// </summary>
    public IgnoreInObjectGraphAttribute()
    {
    }
  }
}
