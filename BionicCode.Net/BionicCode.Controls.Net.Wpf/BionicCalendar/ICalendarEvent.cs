#region Info

// 2020/11/05  10:39
// Activitytracker

#endregion

#region Usings

using System;
using System.ComponentModel;

#endregion

namespace BionicCode.Controls.Net.Wpf
{
  public interface ICalendarEvent : INotifyPropertyChanged
  {
    DateTime Start { get; set; }
    DateTime Stop { get; set; }
    bool IsRecurring { get; set; }
    DateTime Interval { get; set; }
    object Id { get; set; }
    string Summary { get; set; }
  }
}