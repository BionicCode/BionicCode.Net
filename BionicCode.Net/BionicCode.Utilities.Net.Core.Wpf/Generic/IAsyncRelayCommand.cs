#region Info
// //  
// Library
#endregion

using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Core.Wpf.Generic
{
  public interface IAsyncRelayCommand<TParam> : IAsyncRelayCommand, ICommand
  {
    bool CanExecute(TParam parameter);
    /// <summary>
    /// Executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    void Execute(TParam parameter);
    /// <summary>
    /// Asynchronously executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// Data used by the command. If the command does not require data to be passed, 
    /// this object can be set to null.
    /// </param>
    Task ExecuteAsync(TParam parameter);
  }
}