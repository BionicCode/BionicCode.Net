#region Info
// //  
// WpfTestRange.Main
#endregion


using BionicCode.Utilities.Net.Standard.Generic;
using System;
using System.ComponentModel;

namespace BionicCode.Utilities.Net.Standard.ViewModel
{
  /// <summary>
  /// Interface to provide progress properties to be exposed by a view model for data binding a progress reporter GUI control.
  /// </summary>
  public interface IProgressReporter : INotifyPropertyChanged
  {
    /// <summary>
    /// Creates an instance of <see cref="CreateProgressReporterFromCurrentThread"/> that is bound to the SynchronizationContext of the calling thread.
    /// </summary>
    /// <remarks>When handling the progress involves a <see cref="DispatcherObject"/>, the <see cref="CreateProgressReporterFromCurrentThread"/> should be called on the thread that the DispatcherObject is associated with. This will be the UI thread in most cases. </remarks>
    /// <returns>An instance of <see cref="IProgress{T}"/> that is associated with the calling thread.</returns>
    IProgress<ProgressData> CreateProgressReporterFromCurrentThread();

    /// <summary>
    /// Indicates ongoing progress reporting 
    /// </summary>
    /// <remarks>Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</remarks>
    bool IsReportingProgress { get; }

    /// <summary>
    /// Indicates that the progress reporting is indeterminate.
    /// </summary>
    bool IsIndeterminate { get; set; }

    /// <summary>
    /// The progress message.
    /// </summary>
    /// <remarks>Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</remarks>
    string ProgressText { get; set; }

    /// <summary>
    /// The progress value.
    /// </summary>
    /// <remarks>Raises <see cref="INotifyPropertyChanged.PropertyChanged"/> event.</remarks>
    double ProgressValue { get; set; }

    /// <summary>
    /// Raised when <see cref="ProgressValue"/>, <see cref="ProgressText"/> or <see cref="IsIndeterminate"/> has changed.
    /// </summary>
    event EventHandler<ProgressChangedEventArgs> ProgressChanged;
  }
}