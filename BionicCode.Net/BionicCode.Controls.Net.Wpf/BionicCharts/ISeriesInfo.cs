namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Collections;
  using System.Windows.Media;

  public interface ISeriesInfo
  {
    IEnumerable Series { get; }
    Pen Pen { get; }
    IDataConverter PointConverter { get; }
  }
}
