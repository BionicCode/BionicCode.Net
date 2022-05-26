namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Windows;

  public interface ICartesianDataConverter : IDataConverter
  {
    Point ConvertToCartesianPoint(object dataItem);
    object IDataConverter.Convert(object dataItem) => ConvertToCartesianPoint(dataItem);
  }
}
