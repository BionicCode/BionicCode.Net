using System;
using BionicCode.Utilities.Net.Standard.Generic;

namespace BionicCode.Utilities.Net.Framework.Dialog
{
  /// <summary>
  /// Interface that supports notification of observers to request display of a dialog.
  /// The event args is the view model of <see cref="IDialogViewModel"/> which serves as the DataContext and binding source of the dialog.
  /// </summary>
  public interface IDialogViewModelProviderSource
  {
    /// <summary>
    /// Event that can be raised to notify a listening view model (or view) that the displaying of dialog is requested. The event args is the view model of <see cref="IDialogViewModel"/> which serves as the DataContext and binding source of the dialog.
    /// </summary>
    event EventHandler<ValueEventArgs<IDialogViewModel>> DialogRequested;
  }
}