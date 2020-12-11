#region Info

// 2020/12/05  18:05
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System;
using System.Windows;
using System.Windows.Threading;

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  internal class DayChangeWatcher
  {
    public DayChangeWatcher()
    {
      this.Timer = new DispatcherTimer(
        TimeSpan.Zero,
        DispatcherPriority.Background,
        NotifyDayChangedOnTimerElapsed,
        Application.Current.Dispatcher);
      this.Timer.Stop();
    }

    private void NotifyDayChangedOnTimerElapsed(object sender, EventArgs e)
    {
      OnCalendarDayChanged();
      InitializeTimerInterval();
      this.Timer.Start();
    }

    public void Start()
    {
      InitializeTimerInterval();
      this.Timer.Start();
    }

    public void Stop() => this.Timer.Stop();

    private void InitializeTimerInterval()
    {
      DateTime tomorrow = DateTime.Today.AddDays(1);
      this.Timer.Interval = tomorrow.Subtract(DateTime.Now);
    }

    private DispatcherTimer Timer { get; }
    public event EventHandler CalendarDayChanged;
    protected virtual void OnCalendarDayChanged() => this.CalendarDayChanged?.Invoke(this, EventArgs.Empty);
  }
}