#region Info

// 2021/02/18  13:43
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System.Windows;

namespace BionicCode.Controls.Net.Wpf
{
  public class CalendarViewChangedRoutedEventArgs<TData> : RoutedEventArgs where TData : CalendarView
  {
    public CalendarViewChangedRoutedEventArgs(RoutedEvent routedEvent, TData currentView) : this(routedEvent, default, currentView)
    {
    }

    public CalendarViewChangedRoutedEventArgs(RoutedEvent routedEvent, object source, TData currentView) : base(routedEvent, source) => this.CurrentView = currentView;

    public TData CurrentView { get; }
  }
}