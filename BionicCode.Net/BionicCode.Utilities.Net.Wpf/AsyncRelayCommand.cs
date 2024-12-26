namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// A reusable parameterless asynchronous command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommand"/> accepts asynchronous command handlers and supports data binding to properties like <see cref="IAsyncRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="IAsyncRelayCommandCommon.ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><see cref="AsyncRelayCommand"/> implements <see cref="System.Windows.Input.ICommand" />. 
  /// In case the <see cref="AsyncRelayCommand"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="AsyncRelayCommandCommon.ExecuteAsync()"/> or its overloads instead. 
  /// Otherwise, the call is not properly awaited and exceptions will not propagate to the caller's context as expected.
  /// <para>
  /// See <see cref="AsyncRelayCommand{TParam}"/> to enable command parameters.
  /// </para>
  /// </remarks>
  public class AsyncRelayCommand : AsyncRelayCommandCommon, IAsyncRelayCommand
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
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam) : base(executeAsyncNoParam)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsyncNoParam) : base(executeAsyncNoParam)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : base(executeAsyncNoParam, canExecuteNoParam)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute) : base(executeAsync, canExecute)
    {
#if !NETSTANDARD
      this.IsCommandManagerRequerySuggestedEnabled = true;
#endif
    }

#if !NETSTANDARD

    /// <summary>
    ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
    /// </summary>
    /// <param name="executeAsync">The awaitable execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="CommandManager.RequerySuggested"/> event. 
    /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IAsyncRelayCommandCore.InvalidateCommand"/>.
    /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute, bool isCommandManagerRequerySuggestedEnabled) : base(executeAsync, canExecute)
      => this.IsCommandManagerRequerySuggestedEnabled = isCommandManagerRequerySuggestedEnabled;

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execute handler.</param>
    /// <param name="canExecuteNoParam">The can execute handler.</param>
    /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="CommandManager.RequerySuggested"/> event. 
    /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IAsyncRelayCommandCore.InvalidateCommand"/>.
    /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam, bool isCommandManagerRequerySuggestedEnabled) : base(executeAsyncNoParam, canExecuteNoParam)
      => this.IsCommandManagerRequerySuggestedEnabled = isCommandManagerRequerySuggestedEnabled;

#endif
    #endregion Constructors

    /// <summary>
    /// Event invocator. Called when <see cref="IAsyncRelayCommand.IsCommandManagerRequerySuggestedEnabled"/> is <see langword="true"/> and the <see cref="CommandManager.RequerySuggested"/> is raised.
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