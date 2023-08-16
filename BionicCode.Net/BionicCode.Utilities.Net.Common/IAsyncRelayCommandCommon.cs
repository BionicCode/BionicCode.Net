namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// An interface to define a reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates and data binding (implements <see cref="INotifyPropertyChanged"/>). 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// </summary>
  public interface IAsyncRelayCommandCommon : ICommand, INotifyPropertyChanged
  {
    /// <summary>
    /// A flag to signal if the asynchronous operation has completed.
    /// </summary>
    bool IsExecuting { get; }
    /// <summary>
    /// Returns whether the command can be cancelled.
    /// </summary>
    /// <value><c>true</c> if cancellation is allowed. Otherwise <c>false</c>.</value>
    bool CanBeCanceled { get; }
    /// <summary>
    /// Returns whether the command's executing operation was cancelled.
    /// </summary>
    /// <value><c>true</c> if the last command execution was cancelled. Otherwise <c>false</c>.</value>
    bool IsCancelled { get; }
    /// <summary>
    /// Returns whether the command's pending operations were cancelled.
    /// </summary>
    /// <value><c>true</c> if the last command execution was cancelled. Otherwise <c>false</c>.</value>
    bool IsPendingCancelled { get; }

    /// <summary>
    /// Return whether the command has pending executions.
    /// </summary>
    /// <remarks>Only one command is executed at time. Sucessive command invocations are enqueued and run after the currently executing delegate has completed
    /// <br/> or was cancelled.</remarks>
    bool HasPending { get; }

    /// <summary>
    /// Returns the number of pending command delegate executions.
    /// </summary>
    int PendingCount { get; }

    /// <summary>
    /// The currently used <see cref="CancellationToken"/>.
    /// </summary>
    /// <value>A <see cref="CancellationToken"/> instance currently in use during the command execution.</value>
    /// <remarks>This property can be used to query the state of the cancellation or to register a cancellation callback by calling <see cref="CancellationToken.Register(Action)"/>.
    /// <br/>The instance will change for each command execution.</remarks>
    CancellationToken CurrentCancellationToken { get; }

    /// <summary>
    /// Cancels all pending and executing commands.
    /// </summary>
    /// <remarks>See <see cref="CancellationTokenSource.Cancel()"/> for the exception behavior of this overload.</remarks>
    void CancelAll();

    /// <summary>
    /// Cancels all pending and executing commands.
    /// </summary>
    /// <param name="throwOnFirstException">See <see cref="CancellationTokenSource.Cancel(bool)"/> for the effects of the parameter.</param>
    /// <remarks>See <see cref="CancellationTokenSource.Cancel(bool)"/> for the exception behavior of this overload.</remarks>
    void CancelAll(bool throwOnFirstException);

    /// <summary>
    /// Cancels the currently executing command.
    /// </summary>
    /// <remarks>See <see cref="CancellationTokenSource.Cancel()"/> for the exception behavior of this overload.</remarks>
    void CancelExecuting();

    /// <summary>
    /// Cancels the currently executing command.
    /// </summary>
    /// <param name="throwOnFirstException">See <see cref="CancellationTokenSource.Cancel(bool)"/> for the effects of the parameter.</param>
    /// <remarks>See <see cref="CancellationTokenSource.Cancel(bool)"/> for the exception behavior of this overload.</remarks>
    void CancelExecuting(bool throwOnFirstException);

    /// <summary>
    /// Cancels all pending command executions.
    /// </summary>    
    /// <remarks>See <see cref="CancellationTokenSource.Cancel()"/> for the exception behavior of this overload.</remarks>
    void CancelPending();

    /// <summary>
    /// Cancels all pending command executions.
    /// </summary>    
    /// <param name="throwOnFirstException">See <see cref="CancellationTokenSource.Cancel(bool)"/> for the effects of the parameter.</param>
    /// <remarks>See <see cref="CancellationTokenSource.Cancel(bool)"/> for the exception behavior of this overload.</remarks>
    void CancelPending(bool throwOnFirstException);

    /// <summary>
    /// Checks if the <see cref="ICommand"/> can execute.
    /// </summary>
    /// <returns><c>true</c> when the <see cref="ICommand"/> can execute, otherwise <c>false</c>.</returns>
    bool CanExecute();

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.</remarks>
    Task ExecuteAsync();

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.
    /// <para>Only one command is executed at time. Sucessive command invocations are enqueued and run after the currently executing delegate has completed
    /// <br/> or was cancelled. Query <see cref="HasPending"/> to know if the command has pending delegate executions and <see cref="PendingCount"/> to knwo the number of pending delegate executions.</para>
    /// </remarks>
    /// <exception cref="OperationCanceledException">If executing command delegate was cancelled.</exception>
    Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="timeout">A <seealso cref="TimeSpan"/> to specify the timeout of the operation. 
    /// <br/>A value of <see cref="Timeout.InfiniteTimeSpan"/> (or a <see cref="TimeSpan"/> that represents -1) will specifiy an infinite time out. 
    /// <br/>A value of <see cref="TimeSpan.Zero"/> will cancel the operation immediately.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// <para>Only one command is executed at time. Sucessive command invocations are enqueued and run after the currently executing delegate has completed
    /// <br/> or was cancelled. Query <see cref="HasPending"/> to know if the command has pending delegate executions and <see cref="PendingCount"/> to knwo the number of pending delegate executions.</para>
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    Task ExecuteAsync(TimeSpan timeout);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="timeout">A <seealso cref="TimeSpan"/> to specify the timeout of the operation. 
    /// <br/>A value of <see cref="Timeout.InfiniteTimeSpan"/> (or a <see cref="TimeSpan"/> that represents -1) will specifiy an infinite time out. 
    /// <br/>A value of <see cref="TimeSpan.Zero"/> will cancel the operation immediately.</param>
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout>"/>.TotalMilliseconds is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
    Task ExecuteAsync(TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Raises the <seealso cref="ICommand.CanExecuteChanged"/> event of this particular command only.
    /// </summary>
    void InvalidateCommand();
  }
}