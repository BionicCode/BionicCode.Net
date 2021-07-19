#region Info

// 2020/11/04  13:25
// Activitytracker

#endregion

#region Usings

#endregion

#region Usings

using System;

#endregion

namespace BionicCode.Controls.Net.Wpf
{
    public class CalendarDate : BionicCode.Utilities.Net.Standard.ViewModel.ViewModel, ICalendarDate
    {
        #region

        public DateTime Day { get => this.day; set => TrySetValue(value, ref this.day); }

        public string Annotation { get => this.annotation; set => TrySetValue(value, ref this.annotation); }

        public bool IsHoliday { get => this.isHoliday; set => TrySetValue(value, ref this.isHoliday); }
        public DayOfWeek DayOfWeek { get => this.dayOfWeek; set => TrySetValue(value, ref this.dayOfWeek); }
        public int WeekOfYear { get => this.weekOfYear; set => TrySetValue(value, ref this.weekOfYear); }

        private bool isToday;   
        public bool IsToday
        {
          get => this.isToday;
          set => TrySetValue(value, ref this.isToday);
        }

        private bool isSelected;   
        public bool IsSelected
        {
          get => this.isSelected;
          set => TrySetValue(value, ref this.isSelected);
        }
        #endregion


        private string annotation;
        private DateTime day;

        private DayOfWeek dayOfWeek;
        private bool isHoliday;
        private int weekOfYear;
    }
}