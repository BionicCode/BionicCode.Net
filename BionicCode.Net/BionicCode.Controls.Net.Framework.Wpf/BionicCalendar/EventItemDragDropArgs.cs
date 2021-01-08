#region Info

// 2020/12/14  17:00
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System;
using System.Windows;

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  public class EventItemDragDropArgs : RoutedEventArgs
  {
    public DateTime OriginalDay { get; }
    public CalendarEventItem ItemContainer { get; }
    public DragDropEffects DragDropEffects { get; }

    public EventItemDragDropArgs(DateTime originalDay, CalendarEventItem itemContainer, DragDropEffects dragDropEffects) : base(CalendarEventItem.EventItemDroppedRoutedEvent, itemContainer)
    {
      this.OriginalDay = originalDay;
      this.ItemContainer = itemContainer;
      this.DragDropEffects = dragDropEffects;
    }

    public EventItemDragDropArgs(EventItemDragDropArgs args, RoutedEvent routedEvent,UIElement source) : base(routedEvent, source)
    {
      this.OriginalDay = args.OriginalDay;
      this.ItemContainer = args.ItemContainer;
      this.DragDropEffects = args.DragDropEffects;
    }

    public EventItemDragDropArgs()
    {
    }

    public EventItemDragDropArgs(RoutedEvent routedEvent) : base(routedEvent)
    {
    }

    public EventItemDragDropArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }
  }
}