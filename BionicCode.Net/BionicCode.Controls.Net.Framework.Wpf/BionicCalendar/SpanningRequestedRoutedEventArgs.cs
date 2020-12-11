#region Info

// 2020/12/05  20:11
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System.Windows;
using System.Windows.Controls;

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  public class SpanningRequestedRoutedEventArgs : RoutedEventArgs
  {
    public SpanningRequestedRoutedEventArgs()
    {
    }

    public SpanningRequestedRoutedEventArgs(RoutedEvent routedEvent) : base(routedEvent)
    {
    }

    public SpanningRequestedRoutedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }

    public SpanningRequestedRoutedEventArgs(RoutedEvent routedEvent, object source, ExpandDirection spanDirection) :
      base(routedEvent, source) => this.SpanDirection = spanDirection;

    public ExpandDirection SpanDirection { get; set; }
  }
}