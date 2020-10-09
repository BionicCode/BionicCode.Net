#region Info
// //  
// Library
#endregion

using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Core.Wpf.Generic
{
  /// <summary>
  /// Extends <see cref="ICommand"/> to allow asynchronous command execution, where the accepted parameter of the <see cref="Execute"/> and <see cref="CanExecute"/> is strongly typed to eliminate type casting inside the registered callbacks.
  /// </summary>
  /// <typeparam name="TParam">The type of the <see cref="ICommandSource.CommandParameter"/>.</typeparam>
  public interface IAsyncRelayCommand<TParam> : IAsyncRelayCommand, ICommand
  {
    /// <summary>
    /// Checks if the <see cref="ICommand"/> can execute.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    /// <returns><c>true</c> when the <see cref="ICommand"/> can execute, otherwise <c>false</c>.</returns>
    bool CanExecute(TParam parameter);
    /// <summary>
    /// Executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    void Execute(TParam parameter);
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    /// <returns>An awaitable <see cref="Task"/> instance.</returns>
    Task ExecuteAsync(TParam parameter);
  }
}