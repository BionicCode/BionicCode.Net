namespace BionicCode.Utilities.Net.Standard.Generic
{
  /// <summary>
  /// Interface that returns a null object for the implementing type. Implements Null-Object Pattern
  /// </summary>
  /// <typeparam name="TObject"></typeparam>
  public interface INullObject<out TObject> : INullObject
  {
    /// <summary>
    /// Instance to return instead of NULL
    /// </summary>
    TObject NullObject { get; }
  }
}
