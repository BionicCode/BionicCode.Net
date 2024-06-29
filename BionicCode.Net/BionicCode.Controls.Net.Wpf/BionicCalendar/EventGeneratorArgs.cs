#region Info

// 2020/11/15  20:55
// Activitytracker

#endregion

#region Usings

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace BionicCode.Controls.Net.Wpf
{
  public class EventGeneratorArgs : EventArgs
  {
    #region

    public EventGeneratorArgs() : this(null, null, null, -1, -1, DateTime.MinValue)
    {
    }

    public EventGeneratorArgs(UIElement itemContainer, Panel itemsHost, IEnumerable<object> itemsOfDates, int columnIndex, int rowIndex, DateTime itemDate)
    {
      this.ItemContainer = itemContainer;
      this.ItemsHost = itemsHost;
      this.ItemsOfDate = itemsOfDates;
      this.ColumnIndex = columnIndex;
      this.RowIndex = rowIndex;
      this.ItemDate = itemDate;
    }

    #endregion

    public bool IsCanceled { get; set; }
    public UIElement ItemContainer { get; set; }
    public Panel ItemsHost { get; set; }
    public IEnumerable<object> ItemsOfDate { get; set; }
    public int ColumnIndex { get; set; }
    public int RowIndex { get; set; }
    public DateTime ItemDate { get; set; }
  }
}