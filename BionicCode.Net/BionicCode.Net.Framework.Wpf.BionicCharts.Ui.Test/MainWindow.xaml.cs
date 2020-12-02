using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BionicCode.Utilities.Net.Framework.Wpf.Extensions;

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
      this.Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
      //this.CartesianChart.ItemContainerGenerator.StatusChanged += GeneratorStatusChanged;
      this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      //var cl = VirtualizingPanel.GetCacheLength(this.CartesianChart);
      //var clu = VirtualizingPanel.GetCacheLengthUnit(this.CartesianChart);
      //if (this.CartesianChart.TryFindVisualChildElement(out VirtualizingStackPanel panel))
      //{
      //  ;
      //}
    }

    private void GeneratorStatusChanged(object sender, EventArgs e)
    {
      GeneratorStatus status = (sender as ItemContainerGenerator).Status;
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
