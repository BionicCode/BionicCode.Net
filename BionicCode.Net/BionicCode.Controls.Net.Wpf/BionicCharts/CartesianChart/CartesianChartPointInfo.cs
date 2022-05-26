namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Windows;

  internal readonly struct CartesianChartPointInfo : IChartPointInfo
  {
    public CartesianChartPointInfo(Point chartPoint, ICartesianSeriesInfo seriesInfo)
    {
      this.CartesianChartPoint = chartPoint;
      this.SeriesInfo = seriesInfo;
    }

    public Point CartesianChartPoint { get; }
    public ICartesianSeriesInfo SeriesInfo { get; }
  }
}
