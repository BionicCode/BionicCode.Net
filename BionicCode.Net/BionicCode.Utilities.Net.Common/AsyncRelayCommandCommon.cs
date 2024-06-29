namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Concurrent;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using System.Linq;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommandCommon{TParam}"/> accepts asynchronous command handlers and supports data bindng to properties like <see cref="IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommandCommon</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommandCommon{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="ExecuteAsync()"/> or its overloads instead.</remarks>
  public abstract partial class AsyncRelayCommandCommon : AsyncRelayCommandCore, IAsyncRelayCommandCommon
  {
    /// <summary>
    /// The registered parameterless async execute delegate that supports cancellation.
    /// </summary>
    /// <value>
    /// A delegate that supports cancellation, but takes no command parameter and returns a <see cref="Task"/>.</value>
    private readonly Func<CancellationToken, Task> executeCancellableAsyncNoParamDelegate;

    /// <summary>
    /// The registered parameterless synchronous execute delegate that supports cancellation.
    /// </summary>
    /// <value>
    /// A delegate that suppoprts cancellation, but takes no command parameter and returns void.</value>
    private readonly Action<CancellationToken> executeCancellableNoParamDelegate;

    /// <summary>
    /// The registered parameterless CanExecute delegate.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
    private readonly Func<bool> canExecuteNoParamDelegate;

    #region Constructors

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action<CancellationToken> executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command.
    /// </summary>
    /// <param name="executeNoParam">The execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Action executeNoParam, Func<bool> canExecuteNoParam)
    {
      if (executeNoParam is null)
      {
        throw new ArgumentNullException(nameof(executeNoParam));
      }

      this.executeCancellableNoParamDelegate = cancellationToken => executeNoParam.Invoke();
      this.canExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam)
    {
      if (executeAsyncNoParam is null)
      {
        throw new ArgumentNullException(nameof(executeAsyncNoParam));
      }

      this.executeCancellableAsyncNoParamDelegate = cancellationToken => executeAsyncNoParam.Invoke();
      this.canExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute)
    {
      this.executeCancellableAsyncNoParamDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.canExecuteNoParamDelegate = canExecute ?? (() => true);
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that supports cancellation.
    /// </summary>
    /// <param name="executeCancellable">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Action<CancellationToken> executeCancellable, Func<bool> canExecute)
    {
      this.executeCancellableNoParamDelegate = executeCancellable ?? throw new ArgumentNullException(nameof(executeCancellable));
      this.canExecuteNoParamDelegate = canExecute ?? (() => true);
    }

    #endregion Constructors

    /// <summary>
    ///   Determines whether this AsyncRelayCommandCommon can execute.
    /// </summary>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute() => this.canExecuteNoParamDelegate?.Invoke() ?? true;

    /// <inheritdoc />
    public async Task ExecuteAsync() => await ExecuteAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken) => await ExecuteAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TimeSpan timeout) => await ExecuteAsync(timeout, CancellationToken.None);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="timeout">A <seealso cref="TimeSpan"/> to specify the timeout of the operation. 
    /// <br/>A value of <see cref="Timeout.InfiniteTimeSpan"/> (or a <see cref="TimeSpan"/> that represents -1) will specifiy an infinite time out. 
    /// <br/>A value of <see cref="TimeSpan.Zero"/> will cancel the operation immediately.</param>
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously. 
    /// <br/><br/>Repeated or concurrent calls are synchronized.
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout>"/>.TotalMilliseconds is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
    public virtual async Task ExecuteAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
      await BeginExecuteAsyncCoreAsync(timeout, cancellationToken);

      try
      {
        if (this.executeCancellableAsyncNoParamDelegate != null)
        {
          this.CurrentCancellationToken.ThrowIfCancellationRequested();
          await this.executeCancellableAsyncNoParamDelegate.Invoke(this.CurrentCancellationToken);
        }
        else if (this.executeCancellableNoParamDelegate != null)
        {
          this.CurrentCancellationToken.ThrowIfCancellationRequested();
          this.executeCancellableNoParamDelegate.Invoke(this.CurrentCancellationToken);
        }
      }
      finally
      {
        EndExecuteAyncCore();
      }
    }

    #region ICommand implementation
#if NET
    bool ICommand.CanExecute(object? parameter) => CanExecute();
#else
    bool ICommand.CanExecute(object parameter) => CanExecute();
#endif

    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync();

    #endregion ICommand implementation

  }
}