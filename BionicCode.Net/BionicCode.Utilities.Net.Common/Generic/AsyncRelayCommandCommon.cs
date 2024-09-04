namespace BionicCode.Utilities.Net
{
  using System;
  using System.CodeDom.Compiler;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommandCommon{TParam}"/> accepts asynchronous command handlers and supports data binding to properties like <see cref="AsyncRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="ExecuteAsync(TParam)"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommandCommon</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommandCommon{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="AsyncRelayCommandCommon.ExecuteAsync()"/> or its overloads instead.</remarks>
  public abstract class AsyncRelayCommandCommon<TParam> : AsyncRelayCommandCore, IAsyncRelayCommandCommon<TParam>
  {
    /// <summary>
    /// The registered async execute delegate that supports cancellation and accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that supports cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
    private readonly Func<TParam, CancellationToken, Task> executeCancellableAsyncDelegate;

    /// <summary>
    /// The registered execute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that supports cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
    private readonly Action<TParam, CancellationToken> executeCancellableDelegate;

    /// <summary>
    /// The registered CanExecute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
    private readonly Func<TParam, bool> canExecuteDelegate;

    #region Constructors

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>)
    ///   <br/> and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>) 
    ///   <br/>and accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam, CancellationToken> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>) 
    ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsync">The awaitable execute handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, CancellationToken, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>) 
    ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execute handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam> execute, Predicate<TParam> canExecute)
    {
      if (execute is null)
      { 
        throw new ArgumentNullException(nameof(execute));
      }

      this.executeCancellableDelegate = (commandParameter, cancellationToken) => execute.Invoke(commandParameter);
      this.canExecuteDelegate = param => canExecute?.Invoke(param) ?? true;
    }

    /// <summary>
    ///   Creates a new asynchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, Task> executeAsync, Predicate<TParam> canExecute)
    {
      if (executeAsync is null)
      {
        throw new ArgumentNullException(nameof(executeAsync));
      }

      this.executeCancellableAsyncDelegate = (commandParameter, cancellationToken) => executeAsync.Invoke(commandParameter);
      this.canExecuteDelegate = canExecute?.ToFunc();
    }

    /// <summary>
    ///   Creates a new asynchronous command that supports cancellation and accepts a command parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, CancellationToken, Task> executeAsync, Predicate<TParam> canExecute)
    {
      if (executeAsync is null)
      {
        throw new ArgumentNullException(nameof(executeAsync));
      }

      this.executeCancellableAsyncDelegate = executeAsync;
      this.canExecuteDelegate = canExecute?.ToFunc();
    }

    /// <summary>
    ///   Creates a new synchronous command that supports cancellation and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam, CancellationToken> execute, Predicate<TParam> canExecute)
    {
      if (execute is null)
      {
        throw new ArgumentNullException(nameof(execute));
      }

      this.executeCancellableDelegate = execute;
      this.canExecuteDelegate = canExecute?.ToFunc();
    }

    #endregion Constructors

    /// <summary>
    ///   Determines whether this AsyncRelayCommandCommon can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute(TParam parameter) => this.canExecuteDelegate?.Invoke(parameter) ?? true;

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, TimeSpan timeout) => await ExecuteAsync(parameter, timeout, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, TimeSpan timeout, CancellationToken cancellationToken)
    {
      await BeginExecuteAsyncCoreAsync(timeout, cancellationToken);

      try
      {
        if (this.executeCancellableAsyncDelegate != null)
        {
          this.CurrentCancellationToken.ThrowIfCancellationRequested();
          await this.executeCancellableAsyncDelegate.Invoke(parameter, this.CurrentCancellationToken);
        }
        else if (this.executeCancellableDelegate != null)
        {
          this.CurrentCancellationToken.ThrowIfCancellationRequested();
          this.executeCancellableDelegate.Invoke(parameter, this.CurrentCancellationToken);
        }
      }
      finally
      {
        EndExecuteAyncCore();
      }
    }

    #region ICommand implementation
#if NET
    bool ICommand.CanExecute(object? parameter) => parameter == null
        ? CanExecute(default)
        : CanExecute((TParam)parameter);
#else
    bool ICommand.CanExecute(object parameter) => parameter == null
        ? CanExecute(default)
        : CanExecute((TParam)parameter);
#endif
    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);

    #endregion ICommand implementation
  }
}