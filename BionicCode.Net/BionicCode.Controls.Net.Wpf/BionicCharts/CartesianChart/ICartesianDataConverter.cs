namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows;

  public interface ICartesianDataConverter : IDataConverter
  {
    Point ConvertToCartesianPoint(object dataItem);
    object IDataConverter.Convert(object dataItem) => ConvertToCartesianPoint(dataItem);
  }
}
