namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Collections.Generic;
  using System.Windows;

  public interface ICartesianSeriesInfo : ISeriesInfo
  {
    Point MinY { get; }
    Point MaxY { get; }
    Point MaxX { get; }
    Point MinX { get; }
  }
}
