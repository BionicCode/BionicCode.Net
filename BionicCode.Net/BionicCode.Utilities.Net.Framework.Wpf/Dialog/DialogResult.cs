namespace BionicCode.Utilities.Net.Framework.Wpf.Dialog
{
  /// <summary>
  /// An enumeration to represent the dialog' status after it was closed.
  /// </summary>
  public enum DialogResult
  {
    /// <summary>
    /// Undefined
    /// </summary>
    None = 0, 
    /// <summary>
    /// Accepted e.g., "OK" button was clicked.
    /// </summary>
    Accepted, 
    /// <summary>
    /// Denied e.g. "No" button was clicked
    /// </summary>
    Denied,
    /// <summary>
    /// Aborted e.g., "Cancel" button was clicked
    /// </summary>
    Aborted
  }
}