namespace BionicCode.Utilities.Net.Common
{
  #region Info
  // //  
  // WpfTestRange.Main
  #endregion

  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net;
  using System;
  using System.ComponentModel;
  using ProgressChangedEventArgs = ProgressChangedEventArgs;

  /// <summary>
  /// Interface to provide progress properties to be exposed by a view model for data binding a progress reporter GUI control.
  /// </summary>
  public interface IProgressReporterCommon : INotifyPropertyChanged
  {
    /// <summary>
    /// Creates a <see cref="IProgress{T}"/> instance that is associated with the caller's thread.
    /// The registered progress callback is the virtual <c><see cref="ViewModel"/>.OnProgress(ProgressData)</c> member.
    /// </summary>
    /// <remarks>To create a <see cref="IProgress{T}"/> instance that is associated with the application's primary dispatcher thread, for example to update proerties that bind to a <c>DispatcherObject</c>, call <see cref="CreateProgressReporterFromUiThread"/>.</remarks>
    /// <returns>A <see cref="IProgress{ProgressData}"/> instance that posts progress to the thread <see cref="CreateProgressReporterFromCurrentThread"/> was called from.</returns>
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