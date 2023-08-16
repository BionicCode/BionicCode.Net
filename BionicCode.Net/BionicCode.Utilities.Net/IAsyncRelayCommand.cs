namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <inheritdoc/>
  public interface IAsyncRelayCommand : IAsyncRelayCommandCommon, ICommand, INotifyPropertyChanged
  {
  }
}