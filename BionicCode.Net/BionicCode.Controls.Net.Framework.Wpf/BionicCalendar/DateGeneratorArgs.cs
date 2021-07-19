#region Info

// 2020/11/15  20:55
// Activitytracker

#endregion

#region Usings

using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
    public class DateGeneratorArgs : EventArgs
    {
        #region

        public DateGeneratorArgs() : this(null, null, null, -1, -1, 0)
        {
        }

        public DateGeneratorArgs(
            UIElement itemContainer,
            Panel itemsHost,
            object item,
            int columnIndex,
            int rowIndex,
            int weekNumber)
        {
            this.ItemContainer = itemContainer;
            this.ItemsHost = itemsHost;
            this.Item = item;
            this.ColumnIndex = columnIndex;
            this.RowIndex = rowIndex;
            this.WeekNumber = weekNumber;
        }

        #endregion

        public bool IsCanceled { get; set; }
        public UIElement ItemContainer { get; set; }
        public Panel ItemsHost { get; set; }
        public object Item { get; set; }
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
        public int WeekNumber { get; set; }
    }
}