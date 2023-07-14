namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using BionicCode.Utilities.Net.Common;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommand{TParam}"/> accepts asynchronous command handlers and supports data bindng to properties like <see cref="IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="IAsyncRelayCommandCommon.ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommand{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="ExecuteAsync()"/> or its overloads instead.</remarks>
  public class AsyncRelayCommand<TParam> : AsyncRelayCommandCommon<TParam>, IAsyncRelayCommand, ICommand, IAsyncRelayCommand<TParam>, INotifyPropertyChanged
  {
    #region Constructors
    /// <inheritdoc />
    protected AsyncRelayCommand()
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<TParam> execute) : base(execute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<TParam, CancellationToken> execute) : base(execute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action executeNoParam) : base(executeNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<CancellationToken> executeNoParam) : base(executeNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<TParam, CancellationToken, Task> executeAsync) : base(executeAsync)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<TParam, Task> executeAsync) : base(executeAsync)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam) : base(executeAsyncNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsyncNoParam) : base(executeAsyncNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : base(executeNoParam, canExecuteNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<TParam> execute, Predicate<TParam> canExecute) : base(execute, canExecute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : base(executeAsyncNoParam, canExecuteNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<TParam, Task> executeAsync, Predicate<TParam> canExecute) : base(executeAsync, canExecute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<TParam, CancellationToken, Task> executeAsync, Predicate<TParam> canExecute) : base(executeAsync, canExecute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute) : base(executeAsync, canExecute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<CancellationToken> executeAsync, Func<bool> canExecute) : base(executeAsync, canExecute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<TParam, CancellationToken> executeAsync, Predicate<TParam> canExecute) : base(executeAsync, canExecute)
    {
    } 

    #endregion Constructors
  }
}