#region Info

// 2021/02/18  13:43
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System;
using System.Windows;

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  public class CalendarViewChangedEventArgs<TData> : RoutedEventArgs where TData : CalendarView
  {
    public CalendarViewChangedEventArgs(RoutedEvent routedEvent, TData currentView) : this(routedEvent, default, currentView)
    {
    }

    public CalendarViewChangedEventArgs(RoutedEvent routedEvent, object source, TData currentView) : base(routedEvent, source)
    {
      this.CurrentView = currentView;
    }

    public TData CurrentView { get; }
  }
}