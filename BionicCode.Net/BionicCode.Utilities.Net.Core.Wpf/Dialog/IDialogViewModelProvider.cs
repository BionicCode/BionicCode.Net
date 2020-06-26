#region Info
// //  
// BionicUtilities.Net
#endregion

namespace BionicCode.Utilities.Net.Core.Wpf.Dialog
{
  /// <summary>
  /// The binding source for the current dialog view and the attached property <see cref="Dialog.DialogDataContextProperty"/>.
  /// </summary>
  /// <remarks>The <see cref="IDialogViewModel"/> can be received from instances that implement <see cref="IDialogViewModelProviderSource"/> by subscribing to their <see cref="IDialogViewModelProviderSource.DialogRequested"/> event.
  /// When used together with the attached <see cref="Dialog.DialogDataContextProperty"/> the setting of the <see cref="DialogViewModel"/> property automatically triggers the showing of a dialog. The <see cref="IDialogViewModel"/> is rendered by a custom defined <see cref="DataTemplate"/> that targets the concrete type of a <see cref="IDialogViewModel"/> implementation.</remarks>
  public interface IDialogViewModelProvider
  {
    /// <summary>
    /// The binding source for the current dialog view.
    /// </summary>
    /// <remarks>The <see cref="IDialogViewModel"/> can be received from instances that implement <see cref="IDialogViewModelProviderSource"/> by subscribing to their <see cref="IDialogViewModelProviderSource.DialogRequested"/> event.
    /// When used together with the attached <see cref="Dialog.DialogDataContextProperty"/> the setting of the <see cref="DialogViewModel"/> property automatically triggers the showing of a dialog. The <see cref="IDialogViewModel"/> is rendered by a custom defined <see cref="DataTemplate"/> that targets the concrete type of a <see cref="IDialogViewModel"/> implementation.</remarks>
    IDialogViewModel DialogViewModel { get; }
  }
}