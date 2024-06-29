namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Windows.Input;

  /// <summary>
  /// Defines the shared core functionality of the different implementations.
  /// </summary>
  public interface IAsyncRelayCommandCore : INotifyPropertyChanged
  { 
    /// <summary>
    /// Event is raised after a pending command has been cancelled.
    /// </summary>
    event EventHandler PendingCommandCancelled;
    /// <summary>
    /// Event is raised if the executing command handler has been cancelled.
    /// </summary>
    event EventHandler ExecutingCommandCancelled;
    ///// <summary>
    ///// Sets the number of concurrent execute delegate calls.
    ///// </summary>
    ///// <value>
    ///// A positive property value limits the number of concurrent operations to the set value.<br/>
    ///// If it is -1, there is no limit on the number of concurrently running operations.<br/>
    ///// If it is 1, then all calls will be executed sequentially.
    ///// </value>
    ///// <remarks>
    ///// The MaxDegreeOfParallelism property affects the number of concurrent operations run by Execute method calls. <br/>
    ///// A positive property value limits the number of concurrent operations to the set value.<br/>
    ///// If it is -1, there is no limit on the number of concurrently running operations.<br/>
    ///// If it is 1, then all calls will be executed sequentially.<br/>
    ///// The default is -1.
    ///// </remarks>
    //int MaxDegreeOfParallelism { get; }
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
    /// Raises the <seealso cref="ICommand.CanExecuteChanged"/> event of this particular command only.
    /// </summary>
    void InvalidateCommand();
  }
}