using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
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
using System.Windows.Threading;

namespace BionicCode.Net.Framework.Core.Ui.Test
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    #region CurrentDateTime dependency property

    public static readonly DependencyProperty CurrentDateTimeProperty = DependencyProperty.Register(
      "CurrentDateTime",
      typeof(DateTime),
      typeof(MainWindow),
      new PropertyMetadata(default));

    public DateTime CurrentDateTime { get => (DateTime)GetValue(MainWindow.CurrentDateTimeProperty); set => SetValue(MainWindow.CurrentDateTimeProperty, value); }

    #endregion CurrentDateTime dependency property

    private DispatcherTimer ClockTimer { get; }

    public MainWindow()
    {
      InitializeComponent();

      this.ClockTimer = new DispatcherTimer(
        TimeSpan.FromMilliseconds(500),
        DispatcherPriority.Render,
        OnTimerIntervalElapsed,
        this.Dispatcher);
    }

    private void OnTimerIntervalElapsed(object? sender, EventArgs e) => this.CurrentDateTime = DateTime.Now;
  }
}
