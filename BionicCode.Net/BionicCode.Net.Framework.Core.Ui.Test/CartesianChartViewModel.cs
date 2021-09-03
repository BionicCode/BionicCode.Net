using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BionicCode.Controls.Net.Wpf.BionicCharts.CartesianChart;
using BionicCode.Utilities.Net.Standard.ViewModel;
using BionicCode.Utilities.Net.Wpf;
using BionicCode.Utilities.Net.Wpf.Generic;

namespace BionicCode.Net.Wpf.Ui.Test
{
  class CartesianChartViewModel : ViewModel
  {
    private ObservableCollection<ICartesianChartPoint> chartPoints;
    public ObservableCollection<ICartesianChartPoint> ChartPoints
    {
      get => chartPoints;
      set => TrySetValue(value, ref chartPoints);
    }

    public IAsyncRelayCommand AsyncRelayTestCommand => new AsyncRelayCommand<string>(ExecuteCommandTest);
    public IAsyncRelayCommand<string> SynchronousRelayTestCommand => new AsyncRelayCommand<string>(ExecuteCommandTestSynchronously);

    private void ExecuteCommandTestSynchronously(string obj)
    {
      ;
      //throw new InvalidCastException("SynchronousRelayTestCommandHandler");
    }


    private async Task ExecuteCommandTest(string arg)
    {
      await SynchronousRelayTestCommand.ExecuteAsync();
      await SynchronousRelayTestCommand.ExecuteAsync("123");
      throw new InvalidCastException("AsyncRelayTestCommandHandler");
    }

    public CartesianChartViewModel()
    {
      ChartPoints = new ObservableCollection<ICartesianChartPoint>();
      for (int x = 0; x < 361 * 100; x++)
      //for (double x = 0; x < 10; x+=0.1)
      {
        var point = new CartesianChartPoint() { X = x, Y = Math.Sin(x * Math.PI / 180) * 2800 }; // Since you want 2*PI to be at 1};
        //var point = new CartesianChartPoint() { X = x, Y =(x * x) }; // Since you want 2*PI to be at 1};
        ChartPoints.Add(point);
      }
    }
  }
}
