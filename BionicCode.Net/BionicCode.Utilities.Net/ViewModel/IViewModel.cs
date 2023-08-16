namespace BionicCode.Utilities.Net
{
  #region Info
  // //  
  // WpfTestRange.Main
  #endregion

  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;

  /// <summary>
  /// Encapsulates implementations of <see cref="INotifyPropertyChanged"/> and <see cref="INotifyDataErrorInfo"/> and adds <see cref="ViewModelCommon.PropertyValueChanged"/> event which is raised in tandem with <see cref="INotifyPropertyChanged.PropertyChanged"/> except it provides addition data like old value and new value.
  /// </summary>
  public interface IViewModel : IViewModelCommon
  {
  }
}