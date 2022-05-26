namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  using System.Windows;

  public interface IDataConverter
  {
    object Convert(object dataItem);
  }
}