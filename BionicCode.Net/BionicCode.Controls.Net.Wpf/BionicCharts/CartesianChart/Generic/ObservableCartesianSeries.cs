/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
namespace BionicCode.Controls.Net.Wpf
After:
namespace BionicCode.Controls.Net.Wpf
*/

namespace BionicCode.Controls.Net.Wpf
{

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
    using System.Collections.ObjectModel;
  After:
    using System.Collections.ObjectModel;
    ;
  */
  using System.Collections.ObjectModel;

  public class ObservableCartesianSeries<TItem> : ObservableCollection<TItem>, IObservableCartesianSeries
  {
    public ICartesianDataConverter<TItem> DataConverter { get; init; }
    ICartesianDataConverter IObservableCartesianSeries.DataConverter => this.DataConverter;

    public ObservableCartesianSeries(ICartesianDataConverter<TItem> dataConverter) => this.DataConverter = dataConverter;
  }
}
