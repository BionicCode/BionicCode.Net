namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // WpfTestRange.Main
  #endregion
  using System;
  using System.ComponentModel;

  /// <summary>
  /// Interface to provide progress properties to be exposed by a view model for data binding a progress reporter GUI control.
  /// </summary>
  public interface IProgressReporter : IProgressReporterCommon, INotifyPropertyChanged
  {
#if NET || NET472_OR_GREATER
    /// <summary>
    /// Creates a <see cref="IProgress{T}"/> instance that is associated with the application's primary dispatcher thread.
    /// The registered progress callback is the virtual <c><see cref="ViewModel"/>.OnProgress(ProgressData)</c> member.
    /// </summary>
    /// <remarks>To create a <see cref="IProgress{T}"/> instance that is associated with the application's primary dispatcher thread, call </remarks>
    /// <returns>A <see cref="IProgress{ProgressData}"/> instance that posts progress to the thread <see cref="IProgressReporterCommon.CreateProgressReporterFromCurrentThread"/> was called from.</returns>
    IProgress<ProgressData> CreateProgressReporterFromUiThread();
#endif
  }
}