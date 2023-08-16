namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <inheritdoc/>
  public interface IAsyncRelayCommand : IAsyncRelayCommandCommon, ICommand, INotifyPropertyChanged
  {
#if !NETSTANDARD
    /// <summary>
    /// Controls whether the command's <see cref="ICommand.CanExecuteChanged"/> is attached to the <see cref="CommandManager.RequerySuggested"/> event. 
    /// </summary>
    /// <value><c>true</c> to attach the <see cref="ICommand.CanExecuteChanged"/> is attached to the <see cref="CommandManager.RequerySuggested"/> event. 
    /// <br/>The default is <c>true</c>.</value>
    /// <remarks>If the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property is set to <c>true</c>, calling the <see cref="CommandManager.InvalidateRequerySuggested"/> method will also raise the <see cref="ICommand.CanExecuteChanged"/> event.
    /// <br/>This is the default behavior. <see cref="CommandManager.InvalidateRequerySuggested"/> is regularly invoked by the framework to keep UI elemnts, that implement <see cref="ICommandSource"/> (like the Button control) and listen to the <see cref="ICommand.CanExecuteChanged"/> event, updated. 
    /// <br/>When the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property ios set to <c>false</c>, the <see cref="ICommand.CanExecuteChanged"/> must be raised explicitly by calling the <see cref="AsyncRelayCommandCommon.InvalidateCommand"/> method.</remarks>
    bool IsCommandManagerRequerySuggestedEnabled { get; set; }
#endif
  }
}