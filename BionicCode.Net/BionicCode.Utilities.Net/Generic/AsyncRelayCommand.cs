namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommand{TParam}"/> accepts asynchronous command handlers and supports data bindng to properties like <see cref="AsyncRelayCommandCommon.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="IAsyncRelayCommandCommon.ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommand{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="AsyncRelayCommandCommon.ExecuteAsync()"/> or its overloads instead.</remarks>
  public partial class AsyncRelayCommand<TParam> : AsyncRelayCommandCommon<TParam>, IAsyncRelayCommand<TParam>
  {
    #region Constructors

    /// <inheritdoc />
    public AsyncRelayCommand(Action<TParam> execute) : base(execute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<TParam, CancellationToken> execute) : base(execute)
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
    public AsyncRelayCommand(Action<TParam> execute, Predicate<TParam> canExecute) : base(execute, canExecute)
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
    public AsyncRelayCommand(Action<TParam, CancellationToken> executeAsync, Predicate<TParam> canExecute) : base(executeAsync, canExecute)
    {
    }

    #endregion Constructors
  }
}