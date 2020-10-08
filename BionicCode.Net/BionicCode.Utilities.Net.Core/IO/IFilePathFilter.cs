using System;
using System.Collections.Generic;
using System.IO;
using BionicCode.Utilities.Net.Standard.IO;

namespace BionicCode.Utilities.Net.Core.IO
{
  /// <summary>
  /// API to enumerate the filesystem e.g., by extension
  /// </summary>
  public interface IFilePathFilter
  {
    #region .NET Standard 2.1

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="folderPath">The path to the folder to filter.</param>
    /// <param name="fileExtensionsToCollect">
    ///   The flagged Enum type <see cref="FileExtensions" /> that defines one or more extensions to filter from the folder.
    /// </param>
    /// <param name="enumerationOptions">
    ///   An instance of <see cref="EnumerationOptions"/> to configures the enumeration behavior.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFiles(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      EnumerationOptions enumerationOptions);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="folderPath">The path to the folder to filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileInfo"/> items.
    /// </param>
    /// <param name="enumerationOptions">
    ///   An instance of <see cref="EnumerationOptions"/> to configures the enumeration behavior.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFiles(
      string folderPath,
      Predicate<FileInfo> filterPredicate,
      EnumerationOptions enumerationOptions);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileInfo"/> items.
    /// </param>
    /// <param name="enumerationOptions">
    ///   An instance of <see cref="EnumerationOptions"/> to configures the enumeration behavior.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFiles(
      IEnumerable<string> pathEntries,
      Predicate<FileInfo> filterPredicate,
      EnumerationOptions enumerationOptions);

    /// <summary>
    ///   Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="fileExtensionsToCollect">
    /// The flagged Enum type <see cref="FileExtensions" /> that defines one or more extensions to filter from the folder.
    /// </param>
    /// <param name="enumerationOptions">
    ///   An instance of <see cref="EnumerationOptions"/> to configures the enumeration behavior.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileInfo> EnumerateFiles(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect,
      EnumerationOptions enumerationOptions);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. 
    /// </summary>
    /// <param name="folderPath">The path to the folder to filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileSystemInfo"/> items.
    /// </param>
    /// <param name="enumerationOptions">
    ///   An instance of <see cref="EnumerationOptions"/> to configures the enumeration behavior.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileSystemInfo> EnumerateFileSystem(
      string folderPath,
      Predicate<FileSystemInfo> filterPredicate,
      EnumerationOptions enumerationOptions);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. 
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileSystemInfo"/> items.
    /// </param>
    /// <param name="enumerationOptions">
    ///   An instance of <see cref="EnumerationOptions"/> to configures the enumeration behavior.
    /// </param>
    /// <returns>A enumerable collection of <see cref="FileInfo"/>.</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found pass the <see cref="FileExtensions.Any" /> value.
    ///   
    /// </remarks>
    IEnumerable<FileSystemInfo> EnumerateFileSystem(
      IEnumerable<string> pathEntries,
      Predicate<FileSystemInfo> filterPredicate,
      EnumerationOptions enumerationOptions);

    #endregion

    #region .NET Standard 2.0


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
    IEnumerable<FileInfo> EnumerateFiles(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="folderPath">The path to the folder to filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileInfo"/> items.
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
    IEnumerable<FileInfo> EnumerateFiles(
      string folderPath,
      Predicate<FileInfo> filterPredicate,
      bool isIncludingSubdirectories);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileInfo"/> items.
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
    IEnumerable<FileInfo> EnumerateFiles(
      IEnumerable<string> pathEntries,
      Predicate<FileInfo> filterPredicate,
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
    IEnumerable<FileInfo> EnumerateFiles(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. 
    /// </summary>
    /// <param name="folderPath">The path to the folder to filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileSystemInfo"/> items.
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
    IEnumerable<FileSystemInfo> EnumerateFileSystem(
      string folderPath,
      Predicate<FileSystemInfo> filterPredicate,
      bool isIncludingSubdirectories);

    /// <summary>
    /// Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. 
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="filterPredicate">
    ///   A delegate used to filter the <see cref="FileSystemInfo"/> items.
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
    IEnumerable<FileSystemInfo> EnumerateFileSystem(
      IEnumerable<string> pathEntries,
      Predicate<FileSystemInfo> filterPredicate,
      bool isIncludingSubdirectories);

    #endregion

  }
}
