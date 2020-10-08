using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BionicCode.Utilities.Net.Standard.IO;

namespace BionicCode.Utilities.Net.Framework.IO
{
  /// <inheritdoc />
  public class FilePathFilter : IFilePathFilter
  {
    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFiles(
      string folderPath,
      Predicate<FileInfo> filterPredicate,
      bool isIncludingSubdirectories)
    {
      if (!Directory.Exists(folderPath))
      {
        var folderNotFoundException =
          new DirectoryNotFoundException("The following directory couldn't be found: " + folderPath);
        folderNotFoundException.Data.Add("folder", folderPath);
        throw folderNotFoundException;
      }

      SearchOption currentDirectorySearchOption = isIncludingSubdirectories 
        ? SearchOption.AllDirectories 
        : SearchOption.TopDirectoryOnly;

      var directoryInfo = new DirectoryInfo(folderPath);
      return CollectFileItemsByFilter(directoryInfo, filterPredicate, currentDirectorySearchOption);
    }

    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFiles(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories)
    {
      IList<string> extensions = fileExtensionsToCollect.ToString().Split(',').Select(extension => $".{extension}").ToList();
      bool FilterExpression(FileInfo fileInfo) => fileExtensionsToCollect.HasFlag(FileExtensions.Any) || extensions.Contains(fileInfo.Extension);
      SearchOption currentDirectorySearchOption = isIncludingSubdirectories 
        ? SearchOption.AllDirectories 
        : SearchOption.TopDirectoryOnly;
      var directoryInfo = new DirectoryInfo(folderPath);
      return CollectFileItemsByFilter(directoryInfo, FilterExpression, currentDirectorySearchOption);
    }

    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFiles(
      IEnumerable<string> folderPath,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories) =>
      folderPath.SelectMany(filePath => EnumerateFiles(filePath, fileExtensionsToCollect, isIncludingSubdirectories));

    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFiles(
      IEnumerable<string> folderPath,
      Predicate<FileInfo> filterPredicate,
      bool isIncludingSubdirectories) =>
      folderPath.SelectMany(filePath => EnumerateFiles(filePath, filterPredicate, isIncludingSubdirectories));

    /// <inheritdoc />
    public IEnumerable<FileSystemInfo> EnumerateFileSystem(
      IEnumerable<string> pathEntries,
      Predicate<FileSystemInfo> filterPredicate,
      bool isIncludingSubdirectories = false) =>
      pathEntries.SelectMany(path => EnumerateFileSystem(path, filterPredicate, isIncludingSubdirectories));

    /// <inheritdoc />
    public IEnumerable<FileSystemInfo> EnumerateFileSystem(
      string path,
      Predicate<FileSystemInfo> filterPredicate,
      bool isIncludingSubdirectories = false)
    {
      if (Directory.Exists(path))
      {
        var directoryInfo = new DirectoryInfo(path);
        foreach (FileSystemInfo fileSystemInfo in CollectFileSystemItemsByFilter(
          directoryInfo,
          filterPredicate,
          isIncludingSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
        {
          yield return fileSystemInfo;
        }
      }
      else if (File.Exists(path))
      {
        var fileInfo = new FileInfo(path);
        if (filterPredicate.Invoke(fileInfo))
        {
          yield return fileInfo;
        }
      }
    }

    private IEnumerable<FileSystemInfo> CollectFileSystemItemsByFilter(
      DirectoryInfo directoryInfo,
      Predicate<FileSystemInfo> filterPredicate,
      SearchOption directorySearchOption) =>
      directoryInfo.EnumerateFileSystemInfos("*", directorySearchOption)
        .Where(filterPredicate.Invoke);

    private IEnumerable<FileInfo> CollectFileItemsByFilter(
      DirectoryInfo directoryInfo,
      Predicate<FileInfo> filterPredicate,
      SearchOption directorySearchOption) =>
      directoryInfo.EnumerateFiles("*", directorySearchOption)
        .Where(filterPredicate.Invoke);
  }
}