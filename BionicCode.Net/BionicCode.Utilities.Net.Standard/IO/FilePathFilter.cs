using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BionicCode.Utilities.Net.Standard.IO
{
  public class FilePathFilter : IFilePathFilter
  {
    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFilesInFolder(
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

    protected IEnumerable<FileInfo> CollectFilePathsFromFolderByFileExtension(
      string folderPath,
      FileExtensions fileExtensionsToCollect,
      SearchOption directorySearchOption)
    {
      if (!Directory.Exists(folderPath))
      {
        yield break;
      }

      var directoryInfo = new DirectoryInfo(folderPath);

      IEnumerable<string> extensionsToCollect = fileExtensionsToCollect.ToString().Split(',');
      foreach (string fileExtension in extensionsToCollect)
      {
        foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles("*." + fileExtension, directorySearchOption))
        {
          yield return fileInfo;
        }
      }
    }

    protected IEnumerable<FileInfo> CollectAllFilePathsIgnoreFileExtension(
      string folderPath,
      SearchOption directorySearchOption)
    {
      if (!Directory.Exists(folderPath))
      {
        return new List<FileInfo>();
      }

      var directoryInfo = new DirectoryInfo(folderPath);
      return 
        directoryInfo.EnumerateFiles("*", directorySearchOption);
    }

    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFilesIncludingFolders(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect,
      bool isIncludingSubdirectories = false)
    {
      foreach (string path in pathEntries)
      {
        if (Directory.Exists(path))
        {
          foreach (FileInfo fileInfo in CollectFilePathsFromFolderByFileExtension(
            path,
            fileExtensionsToCollect,
            isIncludingSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly))
          {
            yield return fileInfo;
          }
        }
        else if (File.Exists(path))
        {
          var fileInfo = new FileInfo(path);
          if (Enum.TryParse(fileInfo.Extension.Substring(1), true, out FileExtensions foundFileExtension) &&
              fileExtensionsToCollect.HasFlag(foundFileExtension))
          {
            yield return fileInfo;
          }
        }
      }
    }

    /// <inheritdoc />
    public IEnumerable<FileInfo> EnumerateFilesIgnoringFolders(
      IEnumerable<string> pathEntries,
      FileExtensions fileExtensionsToCollect)
    {
      foreach (string path in pathEntries)
      {
        if (Directory.Exists(path))
        {
          continue;
        }

        if (!File.Exists(path))
        {
          continue;
        }

        var fileInfo = new FileInfo(path);
        if (Enum.TryParse(fileInfo.Extension, true, out FileExtensions currentFileExtension) &&
            fileExtensionsToCollect.HasFlag(currentFileExtension))
        {
          yield return fileInfo;
        }
      }
    }
  }
}