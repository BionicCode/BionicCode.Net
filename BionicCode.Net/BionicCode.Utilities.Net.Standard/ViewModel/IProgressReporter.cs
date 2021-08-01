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
    bool IsReportingProgress { get; }
    bool IsIndeterminate { get; set; }
    string ProgressText { get; set; }
    double ProgressValue { get; set; }
    event EventHandler<ProgressChangedEventArgs> ProgressChanged;
  }
}