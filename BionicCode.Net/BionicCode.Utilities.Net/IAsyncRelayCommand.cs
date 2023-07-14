namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using BionicCode.Utilities.Net.Common;

  /// <inheritdoc/>
  public interface IAsyncRelayCommand : IAsyncRelayCommandCommon, ICommand, INotifyPropertyChanged
  {
  }
}