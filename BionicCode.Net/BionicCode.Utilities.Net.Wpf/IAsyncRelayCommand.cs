namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// Extends <see cref="ICommand"/> to allow asynchronous command execution.
  /// </summary>
  public interface IAsyncRelayCommand : ICommand, INotifyPropertyChanged
  {
#if !NETSTANDARD
    /// <summary>
    /// Controls whether the command's <see cref="ICommand.CanExecuteChanged"/> is attached to the <see cref="CommandManager.RequerySuggested"/> event. 
    /// </summary>
    /// <value><c>true</c> to attach the <see cref="ICommand.CanExecuteChanged"/> is attached to the <see cref="CommandManager.RequerySuggested"/> event. 
    /// <br/>The default is <c>true</c>.</value>
    /// <remarks>If the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property is set to <c>true</c>, calling the <see cref="CommandManager.InvalidateRequerySuggested"/> method will also raise the <see cref="ICommand.CanExecuteChanged"/> event.
    /// <br/>This is the default behavior. <see cref="CommandManager.InvalidateRequerySuggested"/> is regularly invoked by the framework to keep UI elemnts, that implement <see cref="ICommandSource"/> (like the Button control) and listen to the <see cref="ICommand.CanExecuteChanged"/> event, updated. 
    /// <br/>When the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property ios set to <c>false</c>, the <see cref="ICommand.CanExecuteChanged"/> must be raised explicitly by calling the <see cref="InvalidateCommand"/> method.</remarks>
    bool IsCommandManagerRequerySuggestedEnabled { get; set; }
#endif

    /// <summary>
    /// Returns whetther the command can be cancelled.
    /// </summary>
    /// <value><c>true</c> if cancellation is allowed. Otherwise <c>false</c>.</value>
    bool CanBeCanceled { get; }

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
    /// <remarks>See <see cref="CancellationTokenSource.Cancel"/> for the exception behavior of this overload.</remarks>
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
    /// <remarks>See <see cref="CancellationTokenSource.Cancel"/> for the exception behavior of this overload.</remarks>
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
    /// <remarks>See <see cref="CancellationTokenSource.Cancel"/> for the exception behavior of this overload.</remarks>
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
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    Task ExecuteAsync(TimeSpan timeout);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    Task ExecuteAsync(object parameter);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout>"/>.TotalMilliseconds is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). 
    /// <br/>Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
    Task ExecuteAsync(object parameter, CancellationToken cancellationToken);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <param name="timeout">A <seealso cref="TimeSpan"/> to specify the timeout of the operation. 
    /// <br/>A value of <see cref="Timeout.InfiniteTimeSpan"/> (or a <see cref="TimeSpan"/> that represents -1) will specifiy an infinite time out. 
    /// <br/>A value of <see cref="TimeSpan.Zero"/> will cancel the operation immediately.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout>"/>.<see cref="TimeSpan.TotalMilliseconds"/> is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). 
    /// <br/>Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
    Task ExecuteAsync(object parameter, TimeSpan timeout);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <param name="cancellationToken">A <seealso cref="TimeSpan"/> to specify the timeout of the operation. 
    /// <br/>A value of <see cref="Timeout.InfiniteTimeSpan"/> (or a <see cref="TimeSpan"/> that represents -1) will specifiy an infinite time out. 
    /// <br/>A value of <see cref="TimeSpan.Zero"/> will cancel the operation immediately.</param>
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout>"/>.TotalMilliseconds is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
    Task ExecuteAsync(object parameter, TimeSpan timeout, CancellationToken cancellationToken);

    /// <summary>
    /// Raises the <seealso cref="ICommand.CanExecuteChanged"/> event of this particular command only.
    /// </summary>
    void InvalidateCommand();

    /// <summary>
    /// A flag to signal if the asynchronous operation has completed.
    /// </summary>
    bool IsExecuting { get; }
  }
}