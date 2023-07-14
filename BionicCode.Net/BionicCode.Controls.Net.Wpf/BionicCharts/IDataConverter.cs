namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows;

  public interface IDataConverter
  {
    object Convert(object dataItem);
  }
}