using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Core.Wpf
{
  public interface IAsyncRelayCommand : ICommand
  {
    /// <summary>
    /// Executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    bool CanExecute();
    /// <summary>
    /// Executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    void Execute();
    /// <summary>
    /// Executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    Task ExecuteAsync();
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    Task ExecuteAsync(object parameter);
  }
}