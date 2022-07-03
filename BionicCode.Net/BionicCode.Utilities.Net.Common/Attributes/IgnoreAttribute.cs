namespace BionicCode.Utilities.Net
{
  using System;

  /// <summary>
  /// Attribute to decorate types or members.<br/>
  /// For example, the attribute is used by the <see cref="HelperExtensionsCommon.ToDataTable"/> extension method.
  /// </summary>
  [AttributeUsage(AttributeTargets.All, Inherited = false)]
  public class IgnoreAttribute : Attribute
  {
  }
}