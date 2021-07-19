#region Info

// 2020/11/15  20:55
// Activitytracker

#endregion

#region Usings

using System.Windows;
using System.Windows.Controls;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
    public class EventGeneratorArgs : DateGeneratorArgs
    {
        #region

        public EventGeneratorArgs() : this(null, null, null, -1, -1)
        {
        }

        public EventGeneratorArgs(UIElement itemContainer, Panel itemsHost, object item, int columnIndex, int rowIndex)
            : base(itemContainer, itemsHost, item, columnIndex, rowIndex, 0)
        {
        }

        #endregion
    }
}