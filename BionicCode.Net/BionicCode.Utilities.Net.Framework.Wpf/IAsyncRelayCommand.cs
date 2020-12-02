using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Framework.Wpf
{
  /// <summary>
  /// Extends <see cref="ICommand"/> to allow asynchronous command execution.
  /// </summary>
  public interface IAsyncRelayCommand : ICommand
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
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    /// <returns>An awaitable <see cref="Task"/> instance.</returns>
    Task ExecuteAsync(object parameter);
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the execution.</param>
    /// <returns>An awaitable <see cref="Task"/> instance.</returns>
    Task ExecuteAsync(object parameter, CancellationToken cancellationToken);

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