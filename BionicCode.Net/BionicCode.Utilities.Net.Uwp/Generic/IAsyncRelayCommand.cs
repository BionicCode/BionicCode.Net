#region Info
// //  
// Library
#endregion

using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Uwp.Generic
{
  /// <summary>
  /// Extends <see cref="ICommand"/> to allow asynchronous command execution, where the accepted parameter of the <see cref="ExecuteAsync(TParam)"/> and <see cref="CanExecute()"/> is strongly typed to eliminate type casting inside the registered callbacks.
  /// </summary>
  /// <typeparam name="TParam">The type of the command parameter.</typeparam>
  public interface IAsyncRelayCommand<in TParam> : ICommand
  {
    /// <summary>
    /// Checks if the <see cref="ICommand"/> can execute.
    /// </summary>
    /// <returns><c>true</c> when the <see cref="ICommand"/> can execute, otherwise <c>false</c>.</returns>
    bool CanExecute();
    /// <summary>
    /// Executes the AsyncRelayCommand asynchronously.
    /// </summary>
    Task ExecuteAsync();
    /// <summary>
    /// Executes the AsyncRelayCommand asynchronously.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
    /// <summary>
    /// Checks if the <see cref="ICommand"/> can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> when the <see cref="ICommand"/> can execute, otherwise <c>false</c>.</returns>
    bool CanExecute(TParam parameter);
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    /// <returns>An awaitable <see cref="Task"/> instance.</returns>
    Task ExecuteAsync(TParam parameter);
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the execution.</param>
    /// <returns>An awaitable <see cref="Task"/> instance.</returns>
    Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken);

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