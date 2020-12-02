using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BionicCode.Controls.Net.Framework.Wpf.BionicCharts;
using BionicCode.Utilities.Net.Framework.Wpf;
using BionicCode.Utilities.Net.Framework.Wpf.Generic;
using BionicCode.Utilities.Net.Standard.ViewModel;

namespace BionicCode.Net.Framework.Wpf.BionicCharts.Ui.Test
{
  class MyClass :ICommand
  {
    #region Implementation of ICommand

    /// <inheritdoc />
    public bool CanExecute(object parameter) => throw new NotImplementedException();

    /// <inheritdoc />
    public void Execute(object parameter)
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public event EventHandler CanExecuteChanged;

    #endregion
  }
  class CartesianChartViewModel : ViewModel
  {
    private ObservableCollection<ICartesianChartPoint> chartPoints;   
    public ObservableCollection<ICartesianChartPoint> ChartPoints
    {
      get => this.chartPoints;
      set => TrySetValue(value, ref this.chartPoints);
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
      await this.SynchronousRelayTestCommand.ExecuteAsync();
      await this.SynchronousRelayTestCommand.ExecuteAsync("123");
      throw new InvalidCastException("AsyncRelayTestCommandHandler");
    }

    public CartesianChartViewModel()
    {
      this.ChartPoints = new ObservableCollection<ICartesianChartPoint>();
      for (int x = 0; x < 361 * 100; x++)
      //for (double x = 0; x < 10; x+=0.1)
      {
        var point = new CartesianChartPoint() { X = x, Y = Math.Sin(x * Math.PI / 180) * 2800 }; // Since you want 2*PI to be at 1};
        //var point = new CartesianChartPoint() { X = x, Y =(x * x) }; // Since you want 2*PI to be at 1};
        this.ChartPoints.Add(point);
      }
    }
  }
}
