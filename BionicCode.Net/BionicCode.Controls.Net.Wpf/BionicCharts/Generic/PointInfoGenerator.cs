/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
namespace BionicCode.Controls.Net.Wpf
After:
namespace BionicCode.Controls.Net.Wpf
*/

namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Linq;

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
    using System.Windows.Data;
  After:
    using System.Windows.Data;
      */

  public abstract class PointInfoGenerator<TSeriesInfo> : PointInfoGenerator where TSeriesInfo : ISeriesInfo
  {
    protected PointInfoGenerator(Chart owner) : base(owner)
    {
    }

    public override void Generate()
    {
      GenerateSeriesInfo();
      this.SeriesInfoToDrawingInfoDataMap.Clear();
      foreach (TSeriesInfo seriesInfo in this.SeriesInfos)
      {
        Dictionary<IChartPointInfo, object> drawingInfoToDataMap = GeneratePointInfos(seriesInfo);
        this.SeriesInfoToDrawingInfoDataMap.Add(seriesInfo, drawingInfoToDataMap);
      }

      IEnumerable<IChartPointInfo> plotInfos = this.SeriesInfoToDrawingInfoDataMap.Values
        .SelectMany(drawingInfoToDataMap => drawingInfoToDataMap.Keys);
      foreach (ChartPanel panel in this.ChartPanelTable.Values)
      {
        panel.ClearPlotInfos();
        panel.AddPlotInfoRange(plotInfos);
        //panel.InvalidatePlot();
      }
      OnGenerated();
    }

    protected virtual void OnGenerated()
    {
    }

    private void GenerateSeriesInfo()
    {
      SeriesSelector seriesSelector = this.Owner.SeriesSelector ?? this.Owner.GetDefaultSeriesSelector();
      ResourceLocator resourceLocator = this.Owner.ResourceLocator;
      try
      {
        IEnumerable<TSeriesInfo> seriesInfos = seriesSelector.ProvideSeriesCollection(this.Owner.DataContext, resourceLocator, this.InternalItems)
          .Cast<TSeriesInfo>();

        this.SeriesInfos = new List<TSeriesInfo>(seriesInfos);
      }
      catch (InvalidCastException e)
      {
        throw new InvalidCastException($"Incompatible {typeof(ISeriesInfo)} implementation used in {typeof(SeriesSelector)}. {nameof(SeriesSelector.ProvideSeriesCollection)} must return a collection of {typeof(TSeriesInfo)}.", e);
      }
    }

    protected abstract Dictionary<IChartPointInfo, object> GeneratePointInfos(TSeriesInfo seriesInfo);
    protected List<TSeriesInfo> SeriesInfos { get; set; }
  }
}
