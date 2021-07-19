using System;
using System.Windows;
using System.Windows.Threading;

namespace BionicCode.Net.Wpf.Ui.Test
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
        TimeSpan.FromMilliseconds(250),
        DispatcherPriority.Render,
        OnTimerIntervalElapsed,
        this.Dispatcher);
    }

    private void OnTimerIntervalElapsed(object? sender, EventArgs e) => this.CurrentDateTime = DateTime.Now;
  }
}
