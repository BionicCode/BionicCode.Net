#region Info
// //  
// BionicCode.BionicNuGetDeploy.Main
#endregion

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;
using BionicCode.Utilities.Net.Standard.ViewModel;
using BionicCode.Utilities.Net.Wpf.Generic;

namespace BionicCode.Utilities.Net.Wpf.Dialog
{
  /// <summary>
  /// Implementation of <see cref="IDialogViewModel"/>. This abstract class implements the dialog data handling logic and is therefore preferred over a custom implementation of <see cref="IDialogViewModel"/>.
  /// </summary>
  /// <remarks>It is recommended to use the <see cref="Dialog"/> attached behavior as it handles the view logic. Just bind <see cref="Dialog.DialogDataContextProperty"/> to an instance of <see cref="DialogViewModel"/> (or <see cref="IDialogViewModel"/>) and define a DataTemplate for each implementation of <see cref="DialogViewModel"/>.</remarks>
  /// <seealso href="https://github.com/BionicCode/BionicCode.Net#mvvm-dialog-attached-behavior">See advanced example</seealso>
  public abstract class DialogViewModel : ViewModel, IDialogViewModel
  {
    /// <summary>
    /// Creates a nw instance of the <see cref="DialogViewModel"/> using a do-nothing dialog closed callback.
    /// </summary>
    /// <param name="message">The dialog message to sow.</param>
    /// <param name="title">The dialog caption to appear in the title bar of the Window.</param>
    protected DialogViewModel(string message, string title) : this(message, title, (dialogViewModel) => Task.CompletedTask)
    {
    }

    /// <summary>
    /// Creates a nw instance of the <see cref="DialogViewModel"/> registering a dialog closed callback.
    /// </summary>
    /// <param name="message">The dialog message to sow.</param>
    /// <param name="title">The dialog caption to appear in the title bar of the Window.</param>
    /// <param name="sendResponseCallbackAsync">The call back to invoke when the dialog was closed</param>
    protected DialogViewModel(string message, string title, Func<IDialogViewModel, Task> sendResponseCallbackAsync) : this(message, title, null, sendResponseCallbackAsync)
    {
    }

    /// <summary>
    /// Creates a nw instance of the <see cref="DialogViewModel"/> registering a dialog closed callback.
    /// </summary>
    /// <param name="message">The dialog message to sow.</param>
    /// <param name="title">The dialog caption to appear in the title bar of the Window.</param>
    /// <param name="titleBarIcon">The <see cref="ImageSource"/> for the window's title icon.</param>
    /// <param name="sendResponseCallbackAsync">The call back to invoke when the dialog was closed</param>
    protected DialogViewModel(string message, string title, ImageSource titleBarIcon, Func<IDialogViewModel, Task> sendResponseCallbackAsync)
    {
      this.ResponseCallbackAsync = sendResponseCallbackAsync;
      this.Message = message;
      this.Title = title;
      this.TitleBarIcon = titleBarIcon;
    }

    /// <summary>
    /// Asynchronously called when the SendResponseAsyncCommand is executed.
    /// </summary>
    /// <param name="result">A <see cref="Wpf.Dialog.DialogResult"/> value that was received by the ICommand.</param>
    /// <returns>A <c>Task</c> instance to make this method awaitable.</returns>
    protected virtual async Task ExecuteSendResponseCommandAsync(DialogResult result)
    {
      this.DialogResult = result;
      await this.ResponseCallbackAsync.Invoke(this);
      OnInteractionCompleted();
    }

    protected virtual bool CanExecuteSendResponseCommand(DialogResult commandParameter) => true;

    #region Implementation of IDialogViewModel

    private string title;
    /// <inheritdoc />
    public string Title { get => this.title; set => TrySetValue(value, ref this.title); }

    private string message;
    /// <inheritdoc />
    public string Message { get => this.message; set => TrySetValue(value, ref this.message); }

    private ImageSource titleBarIcon;
    /// <inheritdoc />
    public ImageSource TitleBarIcon { get => this.titleBarIcon; set => TrySetValue(value, ref this.titleBarIcon); }

    /// <inheritdoc />
    public IAsyncRelayCommand<DialogResult> SendResponseAsyncCommand => new AsyncRelayCommand<DialogResult>(ExecuteSendResponseCommandAsync, CanExecuteSendResponseCommand);

    private DialogResult dialogResult;

    /// <inheritdoc />
    public DialogResult DialogResult
    {
      get => this.dialogResult;
      set => TrySetValue(value, ref this.dialogResult);
    }

    private Func<IDialogViewModel, Task> responseCallbackAsync;

    /// <inheritdoc />
    public Func<IDialogViewModel, Task> ResponseCallbackAsync
    {
      get => this.responseCallbackAsync;
      set => TrySetValue(value, ref this.responseCallbackAsync);
    }

    /// <inheritdoc />
    public event EventHandler<CancelEventArgs> InteractionCompleting;

    /// <inheritdoc />
    public event EventHandler InteractionCompleted;

    #endregion

    /// <summary>
    /// Event invocator of the <see cref="InteractionCompleting"/> event. Raised before <see cref="InteractionCompleted"/> event. When overriding the member, setting the property <see cref="CancelEventArgs.Cancel"/> of the <paramref name="cancelEventArgs"/> parameter to <c>true</c> will cancel the dialog close process. 
    /// </summary>
    /// <param name="cancelEventArgs">When setting the property <see cref="CancelEventArgs.Cancel"/> of the event's <see cref="CancelEventArgs"/> to <c>true</c> will cancel the dialog close process.</param>
    /// <remarks>When the dialog close process was cancelled e.g., by overriding the <see cref="OnInteractionCompleting(CancelEventArgs)"/> method and by setting the <paramref name="cancelEventArgs"/> to <c>true</c>, a new close process can be triggered either by invoking the <see cref="OnInteractionCompleted"/> method or by invoking the <see cref="SendResponseAsyncCommand"/>.</remarks>
    protected virtual void OnInteractionCompleting(CancelEventArgs cancelEventArgs) 
      => this.InteractionCompleting?.Invoke(this, cancelEventArgs);

    /// <summary>
    /// Event invocator of the <see cref="InteractionCompleted"/> event. Raising this event will request the dialog to be closed.
    /// </summary>
    protected virtual void OnInteractionCompleted()
    {
      var cancelEventArgs = new CancelEventArgs();
      OnInteractionCompleting(cancelEventArgs);

      if (!cancelEventArgs.Cancel)
      {
        this.InteractionCompleted?.Invoke(this, EventArgs.Empty); 
      }
    }
  }
}