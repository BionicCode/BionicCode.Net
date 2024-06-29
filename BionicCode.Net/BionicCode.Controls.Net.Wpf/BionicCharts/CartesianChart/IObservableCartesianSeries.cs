namespace BionicCode.Controls.Net.Wpf
{
  using System.Collections;
  using System.Collections.Specialized;

  public interface IObservableCartesianSeries : INotifyCollectionChanged, ICollection
  {
    ICartesianDataConverter DataConverter { get; }
  }
}
