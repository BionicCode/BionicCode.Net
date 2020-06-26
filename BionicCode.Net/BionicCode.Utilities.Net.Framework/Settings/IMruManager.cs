#region Info
// //  
// BionicCode.BionicNuGetDeploy.Main
#endregion

using System;
using System.Collections.ObjectModel;
using BionicCode.Utilities.Net.Standard.Generic;
using BionicCode.Utilities.Net.Standard.ViewModel;

namespace BionicCode.Utilities.Net.Framework.Settings
{
  /// <summary>
  /// Interface that describes an API that manages a MRU (Most Recently Used files) table which is stored in the AppSettings file.
  /// </summary>
  public interface IMruManager : IViewModel
  {
    /// <summary>
    /// Adds a file with the specified path to the MRU table.
    /// </summary>
    /// <param name="filePath">The path to the file which is to add to the MRU table.</param>
    /// <remarks>Checks if the file exists. Does nothing if file doesn't exist. When the number of files in the MRU table exceeds the limit set by <see cref="MaxMostRecentlyUsedCount"/> the entry with the least recent access is removed from the table.</remarks>
    void AddMostRecentlyUsedFile(string filePath);
    /// <summary>
    /// Clears the MRU list.
    /// </summary>
    void Clear();
    /// <summary>
    /// A <see cref="ReadOnlyObservableCollection{T}"/> collection of <see cref="MostRecentlyUsedFileItem"/> which contains the MRU files.
    /// </summary>
    ReadOnlyObservableCollection<MostRecentlyUsedFileItem> MostRecentlyUsedFiles { get; }
    /// <summary>
    /// Gets the MRU file which is the last file added to the MRU table.
    /// </summary>
    MostRecentlyUsedFileItem MostRecentlyUsedFile { get; }
    /// <summary>
    /// The maximum number of files that are kept in the MRU table.<br/>The default value is 10.
    /// </summary>
    /// <remarks>When the limit is exceeded, the least recent used file will be removed from the MRU table every time a new file is added. <br/>The maximum allowed value is 100. The minimum allowed value is 1.</remarks>
    int MaxMostRecentlyUsedCount { get; set; }
    /// <summary>
    /// Raised when a new file was added to the MRU list. The event args contains the old and the new MostRecentlyUsedFileItem. <br/>Once the max <see cref="MostRecentlyUsedFileItem"/> limit is reached, the least used file will be removed from the list to make space for the new item. <br/>In this case the <see cref="ValueChangedEventArgs{TValue}.OldValue"/> is the removed oldest item in the list. <br/>Otherwise <see cref="ValueChangedEventArgs{TValue}.OldValue"/> will be <c>null</c>.
    /// </summary>
    event EventHandler<ValueChangedEventArgs<MostRecentlyUsedFileItem>> FileAdded;
  }
}