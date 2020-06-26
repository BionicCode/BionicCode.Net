using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BionicCode.Utilities.Net.Standard.IO
{
  public class FilePathFilter : IFilePathFilter
  {
    public IEnumerable<string> FilterFilePathsFromFolder(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories)
    {
      if (!Directory.Exists(folderPath))
      {
        var folderNotFoundException =
          new DirectoryNotFoundException("The following directory couldn't be found: " + folderPath);
        folderNotFoundException.Data.Add("folder", folderPath);
        throw folderNotFoundException;
      }

      SearchOption currentDirectorySearchOption =
        isIncludingSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
      if (fileExtensionsToCollect.HasFlag(FileExtensions.Any))
      {
        return CollectAllFilePathsIgnoreFileExtension(folderPath, currentDirectorySearchOption);
      }

      return CollectFilePathsFromFolderByFileExtension(
        folderPath,
        fileExtensionsToCollect,
        currentDirectorySearchOption);
    }

    protected IEnumerable<string> CollectFilePathsFromFolderByFileExtension(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      SearchOption directorySearchOption)
    {
      if (!Directory.Exists(folderPath))
      {
        return new List<string>();
      }

      var directoryInfo = new DirectoryInfo(folderPath);
      var fileInfoCollection = new List<FileInfo>();

      List<string> extensionsToCollect = fileExtensionsToCollect.ToString().Split(',').ToList();
      foreach (string fileExtension in extensionsToCollect)
      {
        fileInfoCollection.AddRange(directoryInfo.GetFiles("*." + fileExtension, directorySearchOption));
      }

      return (from fileInfo in fileInfoCollection select fileInfo.FullName).ToList();
    }

    protected IEnumerable<string> CollectAllFilePathsIgnoreFileExtension(
      string folderPath,
      SearchOption directorySearchOption)
    {
      if (!Directory.Exists(folderPath))
      {
        return new List<string>();
      }

      var directoryInfo = new DirectoryInfo(folderPath);
      List<FileInfo> fileInfoCollection =
        directoryInfo.GetFiles("*", directorySearchOption).Where(fileInfo => fileInfo.Exists).ToList();
      return (from fileInfo in fileInfoCollection select fileInfo.FullName).ToList();
    }

    /// <summary>
    ///   Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the extension filter
    ///   will be applied to all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="fileExtensionsToCollect">
    ///   A flagged Enum type that defines one or more extensions to filter from the
    ///   collection. <see cref="FileExtensions" />
    /// </param>
    /// <param name="isIncludingSubdirectories">
    ///   Sets the filter whether to apply to sub directories or not.
    ///   <c>True</c> includes subdirectories and <c>False</c> ignores them.
    ///   If value is passed the parameter defaults to <c>Tue</c>.
    /// </param>
    /// <returns>IEnumerable</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found specify the <c>any</c> file extension.
    ///   <see cref="FileExtensions" />
    /// </remarks>
    public IEnumerable<string> FilterFilePathsFromMixedPathsIncludingFolders(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories)
    {
      var filePathCollection = new List<string>();
      foreach (string path in pathEntries)
      {
        if (Directory.Exists(path))
        {
          filePathCollection.AddRange(
            CollectFilePathsFromFolderByFileExtension(
              path,
              fileExtensionsToCollect,
              isIncludingSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
        }
        else if (File.Exists(path))
        {
          var fileInfo = new FileInfo(path);
          FileExtensions foundFileExtension;
          if (Enum.TryParse(fileInfo.Extension.Substring(1), true, out foundFileExtension) &&
              fileExtensionsToCollect.HasFlag(foundFileExtension))
          {
            filePathCollection.Add(path);
          }
        }
      }

      return filePathCollection;
    }

    /// <summary>
    ///   Extracts valid paths or paths with a specified extension from a collection of paths.
    ///   The path collection can be a mix-up of files and folders. In case the path describes a folder, the filter will ignore
    ///   it including all containing files.
    /// </summary>
    /// <param name="pathEntries">A string collection holding folder and/ or file paths filter.</param>
    /// <param name="fileExtensionsToCollect">
    ///   A flagged Enum type that defines one or more extensions to filter from the
    ///   collection. <see cref="FileExtensions" />
    /// </param>
    /// <returns>IEnumerable</returns>
    /// <remarks>
    ///   To ignore file extensions and collect all files found specify the <c>any</c> file extension.
    ///   <see cref="FileExtensions" />
    /// </remarks>
    public IEnumerable<string> FilterFilePathsFromMixedPathsIgnoringFolders(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect)
    {
      var filePathCollection = new List<string>();
      foreach (string path in pathEntries)
      {
        if (Directory.Exists(path))
        {
          continue;
        }

        if (File.Exists(path))
        {
          FilePathFilter.CollectFilePaths(fileExtensionsToCollect, path, filePathCollection);
        }
      }

      return filePathCollection;
    }

    private static void CollectFilePaths(
      FileExtensions fileExtensionsToCollect,
      string path,
      List<string> filePathCollection)
    {
      var fileInfo = new FileInfo(path);
      FileExtensions currentFileExtension;
      if (Enum.TryParse(fileInfo.Extension, true, out currentFileExtension) &&
          fileExtensionsToCollect.HasFlag(currentFileExtension))
      {
        filePathCollection.Add(path);
      }
    }
  }
}