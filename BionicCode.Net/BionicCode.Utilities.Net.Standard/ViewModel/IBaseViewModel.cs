#region Info
// //  
// BionicUtilities.NetStandard
#endregion

using System;

namespace BionicCode.Utilities.Net.Standard.ViewModel
{
  [Obsolete("This interface is deprecated it will be removed. Use the renamed 'IViewModel' and abstract implementation 'ViewModel' instead (same API).")]
  public interface IBaseViewModel : IViewModel
  {
    /// <summary>
    /// PropertyChanged implementation that sends old value and new value of the change and raises the INotifyPropertyChanged event.
    /// </summary>
    new event PropertyValueChangedEventHandler<object> PropertyValueChanged;
  }
}