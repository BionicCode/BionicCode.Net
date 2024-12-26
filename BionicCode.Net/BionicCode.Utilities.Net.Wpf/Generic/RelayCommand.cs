namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that accepts a parameter and encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="RelayCommand"/> supports data binding to properties like <see cref="IRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// </summary>
  /// <remarks>
  /// For an asynchronous version see <see cref="AsyncRelayCommand"/> and <see cref="AsyncRelayCommand{TParam}"/>.
  /// </remarks>
  public class RelayCommand<TParam> : RelayCommandCommon<TParam>, IRelayCommand<TParam>
  {
#if !NETSTANDARD
    private bool isCommandManagerRequerySuggestedEnabled;
    /// <inheritdoc />
    public bool IsCommandManagerRequerySuggestedEnabled
    {
      get => this.isCommandManagerRequerySuggestedEnabled;
      set
      {
        if (value == this.IsCommandManagerRequerySuggestedEnabled)
        {
          return;
        }

        this.isCommandManagerRequerySuggestedEnabled = value;
        if (this.IsCommandManagerRequerySuggestedEnabled)
        {
          // CommandManager internally uses a WeakEventManager to register the event handlers
          CommandManager.RequerySuggested += OnCommandManagerRequerySuggested;
        }
        else
        {
          CommandManager.RequerySuggested -= OnCommandManagerRequerySuggested;
        }

        OnPropertyChanged();
      }
    }
#endif

    #region Constructors

    /// <inheritdoc />
    public RelayCommand(Action<TParam> execute) : base(execute)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

    /// <inheritdoc />
    public RelayCommand(Action<TParam, CancellationToken> execute) : base(execute)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

    /// <inheritdoc />
    public RelayCommand(Action<TParam> execute, Func<TParam, bool> canExecute) : base(execute, canExecute)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

    /// <inheritdoc />
    public RelayCommand(Action<TParam, CancellationToken> executeAsync, Func<TParam, bool> canExecute) : base(executeAsync, canExecute)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

#if !NETSTANDARD

    /// <summary>
    ///   Creates a new synchronous command that supports cancellation and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="CommandManager.RequerySuggested"/> event. 
    /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IAsyncRelayCommandCore.InvalidateCommand"/>.
    /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>
    public RelayCommand(Action<TParam, CancellationToken> execute, Func<TParam, bool> canExecute, bool isCommandManagerRequerySuggestedEnabled) : base(execute, canExecute)
    {
      this.IsCommandManagerRequerySuggestedEnabled = isCommandManagerRequerySuggestedEnabled;
    }

    /// <summary>
    ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="CommandManager.RequerySuggested"/> event. 
    /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IAsyncRelayCommandCore.InvalidateCommand"/>.
    /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>
    public RelayCommand(Action<TParam> execute, Func<TParam, bool> canExecute, bool isCommandManagerRequerySuggestedEnabled) : base(execute, canExecute)
    {
      this.IsCommandManagerRequerySuggestedEnabled = isCommandManagerRequerySuggestedEnabled;
    }
#endif

    #endregion Constructors

    /// <summary>
    /// Event invocator. Called when <see cref="IRelayCommand.IsCommandManagerRequerySuggestedEnabled"/> is <see langword="true"/> and the <see cref="CommandManager.RequerySuggested"/> is raised.
    /// </summary>
    /// <param name="sender"><see langword="null"/> because the source event is the static <see cref="CommandManager.RequerySuggested"/> event.</param>
    /// <param name="e">The event args object.</param>
#if NET
    protected virtual void OnCommandManagerRequerySuggested(object? sender, EventArgs e)
#else
    protected virtual void OnCommandManagerRequerySuggested(object sender, EventArgs e)
#endif
      => OnCanExecuteChanged(sender, e);
  }
}