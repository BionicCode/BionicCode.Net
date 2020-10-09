using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Core.Wpf
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
    /// Executes the AsyncRelayCommand.
    /// </summary>
    void Execute();
    /// <summary>
    /// Executes the AsyncRelayCommand asynchronously.
    /// </summary>
    Task ExecuteAsync();
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    Task ExecuteAsync(object parameter);
  }
}