/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
namespace BionicCode.Controls.Net.Wpf.BionicCharts
After:
namespace BionicCode.Controls.Net.Wpf.BionicCharts
*/

namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System;
  using System.Windows;
    
  internal class CartesianDataConverter<TDataItem> : ICartesianDataConverter<TDataItem>
  {
    private static Lazy<CartesianDataConverter<TDataItem>> Instance = new Lazy<CartesianDataConverter<TDataItem>>(() => new CartesianDataConverter<TDataItem>());
    public static CartesianDataConverter<TDataItem> Default => Instance.Value;

    public Point ConvertToCartesianPoint(TDataItem dataItem) => ConvertItemToPointInfo(dataItem);

    internal Point ConvertItemToPointInfo(TDataItem item)
    {
      if (item == null)
      {
        throw new ArgumentNullException(nameof(item), "Item is not initialized.");
      }

      return item switch
      {
        ICartesianChartPoint cartesianChartPoint => cartesianChartPoint.ToPoint(),
        Point point => point,
        ValueTuple<double, double> tuple => new Point(tuple.Item1, tuple.Item2),
        ValueTuple<int, int> tuple => new Point(tuple.Item1, tuple.Item2),
        Tuple<double, double> tuple => new Point(tuple.Item1, tuple.Item2),
        Tuple<int, int> tuple => new Point(tuple.Item1, tuple.Item2),
        CartesianChartItem itemContainer => itemContainer.ToPoint(),
        _ => throw new InvalidCastException($"No default conversion available for data type {item.GetType()}. Please extend SeriesSelector and provide a custom ICartesianDataConverter<T> implementation. Alternatively, ensure that the item type is of type {typeof(Point)} or implements {typeof(ICartesianChartPoint)}.")
      };
    }
  }
}
