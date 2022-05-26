namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Collections;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Media;
  
  public interface ISeriesInfo<TData, TResult> : ISeriesInfo
  {
    new IEnumerable<TData> Series { get; }
    new IDataConverter<TData, TResult> PointConverter { get; }
  }
}
