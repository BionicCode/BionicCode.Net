namespace BionicCode.Net.Wpf.Ui.Test
{
  using System;
  using System.Collections.ObjectModel;
  using System.Threading.Tasks;
  using System.Windows;
  using BionicCode.Utilities.Net;
  using Math = System.Math;

  class CartesianChartViewModel : ViewModel
  {
    //private ObservableCollection<ICartesianChartPoint> chartPoints;
    //public ObservableCollection<ICartesianChartPoint> ChartPoints
    //{
    //  get => chartPoints;
    //  set => TrySetValue(value, ref chartPoints);
    //}

    private ObservableCollection<Point> chartPoints;
    public ObservableCollection<Point> ChartPoints
    {
      get => this.chartPoints;
      set => TrySetValue(value, ref this.chartPoints);
    }

    public IAsyncRelayCommand<string> AsyncRelayTestCommand => new AsyncRelayCommand<string>(ExecuteCommandTest);
    public IAsyncRelayCommand<string> SynchronousRelayTestCommand => new AsyncRelayCommand<string>(ExecuteCommandTestSynchronously);

    private void ExecuteCommandTestSynchronously(string obj)
    {
      ;
      //throw new InvalidCastException("SynchronousRelayTestCommandHandler");
    }

    private async Task ExecuteCommandTest(string arg)
    {
      await this.SynchronousRelayTestCommand.ExecuteAsync(string.Empty);
      await this.SynchronousRelayTestCommand.ExecuteAsync("123");
      throw new InvalidCastException("AsyncRelayTestCommandHandler");
    }

    public CartesianChartViewModel()
    {
      this.ChartPoints = new ObservableCollection<Point>();
      for (double x = 0; x < 361 * 100; x += 0.1)
      //for (double x = 0; x < 10; x += 0.1)
      {
        var point = new Point(x, Math.Sin(x * Math.PI / 180) * 2800); // Since you want 2*PI to be at 1};
        //var point = new CartesianChartPoint() { X = x, Y =(x * x) }; // Since you want 2*PI to be at 1};
        this.ChartPoints.Add(point);
      }
    }
  }
}
