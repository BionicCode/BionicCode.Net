
namespace BionicCode.Utilities.Net
{
  using System;

#if NET || NET472_OR_GREATER
  /// <summary>
  /// Interface that supports notification of observers to request display of a dialog.
  /// The event args is the view model of <see cref="IDialogViewModel"/> which serves as the DataContext and binding source of the dialog.
  /// </summary>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#mvvm-dialog-attached-behavior">See advanced example</seealso>
  public interface IDialogViewModelProviderSource
  {
    /// <summary>
    /// Event that can be raised to notify a listening view model (or view) that the displaying of dialog is requested. The event args is the view model of <see cref="IDialogViewModel"/> which serves as the DataContext and binding source of the dialog.
    /// </summary>
    event EventHandler<ValueEventArgs<IDialogViewModel>> DialogRequested;
  }
#endif
}