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
  public interface IAsyncRelayCommandCommon : IAsyncRelayCommandCore, ICommand
  {
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
    /// <br/> or was cancelled. Query <see cref="IAsyncRelayCommandCore.HasPending"/> to know if the command has pending delegate executions and <see cref="PendingCount"/> to knwo the number of pending delegate executions.</para>
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
    /// <br/> or was cancelled. Query <see cref="IAsyncRelayCommandCore.HasPending"/> to know if the command has pending delegate executions and <see cref="PendingCount"/> to knwo the number of pending delegate executions.</para>
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
  }
}