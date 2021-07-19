#region Info

// 2021/02/18  13:43
// BionicCode.Controls.Net.Framework.Wpf

#endregion

namespace BionicCode.Controls.Net.Wpf
{
  public delegate void CalendarViewChangedRoutedEventHandler<TData>(object sender, CalendarViewChangedRoutedEventArgs<TData> e) where TData : CalendarView;
}