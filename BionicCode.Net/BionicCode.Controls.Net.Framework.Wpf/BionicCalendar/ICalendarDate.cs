#region Info

// 2020/11/04  13:38
// Activitytracker

#endregion

#region Usings

using System;
using System.ComponentModel;

#endregion

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
    public interface ICalendarDate : INotifyPropertyChanged
    {
        string Annotation { get; set; }
        DateTime Day { get; set; }
        bool IsHoliday { get; set; }
        DayOfWeek DayOfWeek { get; set; }
        int WeekOfYear { get; set; }
    }
}