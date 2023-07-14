namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Collections.Specialized;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;

  public interface IObservableCartesianSeries : INotifyCollectionChanged, ICollection
  {
    ICartesianDataConverter DataConverter { get; }
  }
}
