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
  public interface IAsyncRelayCommandCommon<TParam> : ICommand, IAsyncRelayCommandCommon, INotifyPropertyChanged
  {
    /// <summary>
    /// Checks if the <see cref="ICommand"/> can execute based on the command parameter.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> when the <see cref="ICommand"/> can execute, otherwise <c>false</c>.</returns>
    bool CanExecute(TParam parameter);

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
    Task ExecuteAsync(TParam parameter);

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
    Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken);

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
    Task ExecuteAsync(TParam parameter, TimeSpan timeout);

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
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.  
    /// </remarks>
    /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout>"/>.TotalMilliseconds is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
    Task ExecuteAsync(TParam parameter, TimeSpan timeout, CancellationToken cancellationToken);
  }
}