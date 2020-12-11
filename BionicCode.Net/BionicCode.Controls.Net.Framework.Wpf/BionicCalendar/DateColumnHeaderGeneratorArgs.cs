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
    public class DateColumnHeaderGeneratorArgs : EventArgs
    {
        #region

        public DateColumnHeaderGeneratorArgs(
            UIElement itemContainer,
            Panel itemsHost,
            object item,
            int columnIndex,
            int rowIndex)
        {
            this.ItemContainer = itemContainer;
            this.ItemsHost = itemsHost;
            this.Item = item;
            this.ColumnIndex = columnIndex;
            this.RowIndex = rowIndex;
        }

        #endregion

        public bool IsCanceled { get; set; }
        public UIElement ItemContainer { get; }
        public Panel ItemsHost { get; }
        public object Item { get; }
        public int ColumnIndex { get; set; }
        public int RowIndex { get; set; }
    }
}