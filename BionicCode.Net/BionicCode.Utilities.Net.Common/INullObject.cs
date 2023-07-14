namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Null-Object Pattern implementation
  /// </summary>
  public interface INullObject
  {
    /// <summary>
    /// Property to indicate whether the current NULL type is a shared instance or a new instance should be created for each request.
    /// </summary>
    bool IsNull { get; set; }
  }
}
