#region Info
// //  
// BionicCode.BionicNuGetDeploy.Main
#endregion

using System;
using System.Threading.Tasks;
using System.Windows.Media;
using BionicCode.Utilities.Net.Framework.Utility.Generic;
using BionicCode.Utilities.Net.Standard.ViewModel;

namespace BionicCode.Utilities.Net.Framework.Dialog
{
  /// <summary>
  /// Implementation of <see cref="IDialogViewModel"/>. This abstract class implements the dialog data handling logic and is therefore preferred over a custom implementation of <see cref="IDialogViewModel"/>.
  /// </summary>
  /// <remarks>It is recommended to use the <see cref="Dialog"/> attached behavior as it handles the view logic. Just bind <see cref="Dialog.DialogDataContextProperty"/> to an instance of <see cref="DialogViewModel"/> (or <see cref="IDialogViewModel"/>) and define a DataTemplate for each implementation of <see cref="DialogViewModel"/>.</remarks>
  public abstract class DialogViewModel : ViewModel, IDialogViewModel
  {
    protected DialogViewModel(string message, string title) : this(message, title, (dialogViewModel) => Task.CompletedTask)
    {
    }
    protected DialogViewModel(string message, string title, Func<IDialogViewModel, Task> sendResponseCallbackAsync) : this(message, title, null, sendResponseCallbackAsync)
    {
    }

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
    /// <param name="result">A <see cref="Framework.Dialog.DialogResult"/> value that was received by the ICommand.</param>
    /// <returns>A <c>Task</c> instance to make this method awaitable.</returns>
    protected virtual async Task OnSendResponseAsyncCommandExecuted(DialogResult result)
    {
      this.DialogResult = result;
      await this.ResponseCallbackAsync.Invoke(this);
      OnInteractionCompleted();
    }

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
    public IAsyncRelayCommand<DialogResult> SendResponseAsyncCommand => new AsyncRelayCommand<DialogResult>(OnSendResponseAsyncCommandExecuted);
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
    public event EventHandler InteractionCompleted;

    #endregion

    protected virtual void OnInteractionCompleted()
    {
      this.InteractionCompleted?.Invoke(this, EventArgs.Empty);
    }
  }
}