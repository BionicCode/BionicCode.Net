using System.Collections.Generic;
using System.IO;

namespace BionicCode.Utilities.Net.Standard.IO
{
  public interface IFilePathFilter
  {
    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="folderPath">The path to the folder to filter.</param>
    /// <param name="fileExtensionsToCollect">
    ///   The flagged Enum type <see cref="FileExtensions" /> that defines one or more extensions to filter from the folder.
    /// </param>
    /// <param name="isIncludingSubdirectories">
    ///   Sets the filter whether to apply to sub directories or not.
    ///   <c>True</c> includes subdirectories and <c>False</c> ignores them.
    ///   Is <c>false</c> by default.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFilesInFolder(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories);

    /// <summary>
    ///   Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="fileExtensionsToCollect">
    /// The flagged Enum type <see cref="FileExtensions" /> that defines one or more extensions to filter from the folder.
    /// </param>
    /// <param name="isIncludingSubdirectories">
    ///   Sets the filter whether to apply to sub directories or not.
    ///   <c>True</c> includes subdirectories and <c>False</c> ignores them.
    ///   Is <c>false</c> by default.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFilesIncludingFolders(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories);

    /// <summary>
    ///   Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the filter will ignore
    ///   it including all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="fileExtensionsToCollect">
    /// The flagged Enum type <see cref="FileExtensions" /> that defines one or more extensions to filter from the folder.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any"/> value.
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFilesIgnoringFolders(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect);
  }
}
