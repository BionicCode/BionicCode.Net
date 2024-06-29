namespace BionicCode.Controls.Net.Wpf
{
  using System.Collections.Generic;
/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
  using System.Windows.Media;
  
  public interface ISeriesInfo<TData, TResult> : ISeriesInfo
After:
  using System.Windows.Media;

  public interface ISeriesInfo<TData, TResult> : ISeriesInfo
*/


  public interface ISeriesInfo<TData, TResult> : ISeriesInfo
  {
    new IEnumerable<TData> Series { get; }
    new IDataConverter<TData, TResult> PointConverter { get; }
  }
}
