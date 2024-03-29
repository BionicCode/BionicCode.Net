﻿namespace BionicCode.Utilities.Net
{
  using BionicCode.Utilities.Net.Common;
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
  }
}