using System;
using System.IO;
using BionicCode.Utilities.Net.Standard.ViewModel;

namespace BionicCode.Utilities.Net.Framework.Settings
{
  /// <summary>
  /// An immutable item that represents a Most Recently Used file (MRU) table entry.
  /// </summary>
  public class MostRecentlyUsedFileItem : ViewModel, IEquatable<MostRecentlyUsedFileItem>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fileInfo">The underlying <see cref="FileInfo"/> of the item.</param>
    public MostRecentlyUsedFileItem(FileInfo fileInfo)
    {
      this.FileInfo = fileInfo;
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override bool Equals(object obj) => Equals(obj as MostRecentlyUsedFileItem);

    #region Equality members

    /// <inheritdoc />
    public bool Equals(MostRecentlyUsedFileItem other)
    {
      if (object.ReferenceEquals(null, other))
      {
        return false;
      }

      if (object.ReferenceEquals(this, other))
      {
        return true;
      }

      if (this.FileInfo.FullName.Equals(other.FileInfo.FullName, StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }

      return object.Equals(this.FileInfo, other.FileInfo);
    }

    /// <inheritdoc />
    public override int GetHashCode() => (this.FileInfo != null ? this.FileInfo.GetHashCode() : 0);

    public static bool operator ==(MostRecentlyUsedFileItem left, MostRecentlyUsedFileItem right) => left?.Equals(right) ?? false;

    public static bool operator !=(MostRecentlyUsedFileItem left, MostRecentlyUsedFileItem right) => !left?.Equals(right) ?? false;

    #endregion

    #endregion

    /// <summary>
    /// Return the underlying <see cref="FileInfo"/> of this instance.
    /// </summary>
    public FileInfo FileInfo { get; }
    /// <summary>
    /// Returns the file name including the extension.
    /// </summary>
    public string Name => this.FileInfo.Name;
    /// <summary>
    /// Returns the full file path of the file.
    /// </summary>
    public string FullName => this.FileInfo.FullName;
  }
}
