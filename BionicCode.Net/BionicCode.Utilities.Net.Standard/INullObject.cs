namespace BionicCode.Utilities.Net.Standard
{
  /// <summary>
  /// Null-Object Pattern implementation
  /// </summary>
  public interface INullObject
  {
    /// <summary>
    /// Property to indicate whether the current type is NULL
    /// </summary>
    bool IsNull { get; }
  }
}
