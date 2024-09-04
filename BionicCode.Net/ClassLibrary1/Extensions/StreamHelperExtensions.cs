namespace BionicCode.Utilities.Net
{
  using System.IO;

  /// <summary>
  /// A collection of extension methods for various default types
  /// </summary>
  public static partial class HelperExtensionsCommon
  {

    #region Stream
    /// <summary>
    /// Return whether the end of a <see cref="Stream"/> is reached.
    /// </summary>
    /// <param name="streamToCheck"></param>
    /// <returns></returns>
    public static bool HasReachedEnd(this Stream streamToCheck) =>
      streamToCheck != null && streamToCheck.Position == streamToCheck.Length;

    #endregion
  }
}
