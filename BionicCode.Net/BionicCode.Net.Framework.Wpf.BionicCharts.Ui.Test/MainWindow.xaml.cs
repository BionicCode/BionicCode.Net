using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BionicCode.Net.Framework.Wpf.BionicCharts.Ui.Test
{
  enum MyEnum
  {
    None = 0,
    Selected
  }
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
      "Text",
      typeof(string),
      typeof(MainWindow),
      new PropertyMetadata(default(string)));

    public string Text { get => (string) GetValue(MainWindow.TextProperty); set => SetValue(MainWindow.TextProperty, value); }
    public MainWindow()
    {
      InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
      foreach (var point in (this.DataContext as CartesianChartViewModel).ChartPoints)
      {
        point.Y += 50;
      }
    }
  }
}
