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
    using System.Windows;
  After:
    using System.Windows;
      */
  using System.Windows;


  public interface ICartesianDataConverter<TDataItem> : ICartesianDataConverter, IDataConverter<TDataItem, Point>
  {
    Point ConvertToCartesianPoint(TDataItem dataItem);
#if NET
    Point ICartesianDataConverter.ConvertToCartesianPoint(object dataItem) => ConvertToCartesianPoint((TDataItem)dataItem);
    Point IDataConverter<TDataItem, Point>.Convert(TDataItem dataItem) => ConvertToCartesianPoint(dataItem);
#endif
  }
}
