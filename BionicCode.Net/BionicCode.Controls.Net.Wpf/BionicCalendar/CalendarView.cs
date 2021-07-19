#region Info

// 2021/02/12  11:52
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BionicCode.Controls.Net.Wpf
{
  public abstract class CalendarView
  {
    protected CalendarView(System.Globalization.Calendar calendar, DayOfWeek firstDayOfWeek)
    {
      this.Calendar = calendar;
      this.FirstDayOfWeek = firstDayOfWeek;
    }

    public bool ContainsDate(DateTime date) => this.Dates.Contains(date);
    public virtual DateTime GetLastDate() => this.Dates.Last();
    public virtual DateTime GetFirstDate() => this.Dates.First();

    public List<DateTime> Dates { get; protected set; }
    public DateTime Index { get; set; }
    public System.Globalization.Calendar Calendar { get; }
    public DayOfWeek FirstDayOfWeek { get; }
  }

  public class CalendarWeekView : CalendarView
  {
    /// <inheritdoc />
    public CalendarWeekView(DateTime firstDateOfWeek, System.Globalization.Calendar calendar, DayOfWeek firstDayOfWeek) : base(calendar, firstDayOfWeek)
    {
      this.Dates = new List<DateTime>();
      this.WeekNumber = calendar.GetWeekOfYear(firstDateOfWeek, CalendarWeekRule.FirstFourDayWeek, firstDayOfWeek);
    }

    public int WeekNumber { get; }
  }

  public class CalendarMonthView : CalendarView, IEnumerator<CalendarWeekView>, IEnumerable<CalendarWeekView>
  {
    /// <inheritdoc />
    public CalendarMonthView(DateTime monthStart, System.Globalization.Calendar calendar, DayOfWeek firstDayOfWeek) 
      : base(calendar, firstDayOfWeek)
    {
      this.Index = monthStart;
      this.Dates = CreateCalendarViewOfMonthDates(monthStart);
      this.CurrentIndex = -1;
    }

    public CalendarMonthView GetPrevious() =>
      this.Index != DateTime.MinValue 
        ? CreateCalendarViewOfMonth(this.Index.AddMonths(-1)) 
        : this;
    public CalendarMonthView GetNext() => this.Index != DateTime.MaxValue 
      ? CreateCalendarViewOfMonth(this.Index.AddMonths(1))
      : this;

    private CalendarMonthView CreateCalendarViewOfMonth(DateTime monthStart)
    {
      List<DateTime> datesOfCurrentView = CreateCalendarViewOfMonthDates(monthStart);
      var calendarView = new CalendarMonthView(monthStart, this.Calendar, this.FirstDayOfWeek) {Dates = datesOfCurrentView};

      return calendarView;
    }

    private List<DateTime> CreateCalendarViewOfMonthDates(DateTime monthStart)
    {
      var datesOfCurrentView = new List<DateTime>();
      DateTime firstDateInView = monthStart;
      int daysInMonth = this.Calendar.GetDaysInMonth(monthStart.Year, monthStart.Month);
      var lastDateInView = new DateTime(monthStart.Year, monthStart.Month, daysInMonth);

      while (firstDateInView.DayOfWeek != this.FirstDayOfWeek)
      {
        firstDateInView = firstDateInView.Subtract(TimeSpan.FromDays(1));
      }

      while (lastDateInView.AddDays(1).DayOfWeek != this.FirstDayOfWeek)
      {
        lastDateInView = lastDateInView.AddDays(1);
      }

      int daysInCalendarView = lastDateInView.Subtract(firstDateInView).Days + 1;
      int requiredRowCount = (int) Math.Max(5, Math.Ceiling(daysInCalendarView / 7.0));
      //int requiredDateCount = requiredRowCount * 7;
      int requiredDateCount = daysInCalendarView;

      DateTime currentCalendarDate = firstDateInView;
      for (int dayIndex = 0;
        dayIndex < requiredDateCount;
        dayIndex++)
      {
        datesOfCurrentView.Add(currentCalendarDate.AddDays(dayIndex));
      }

      return datesOfCurrentView;
    }

    #region Implementation of IEnumerator

    /// <inheritdoc />
    public bool MoveNext()
    {
      bool hasNext = ++this.CurrentIndex < Math.Ceiling(this.Dates.Count / 7.0);
      if (hasNext)
      {
        IEnumerable<DateTime> daysOfWeek = this.Dates.Skip(this.CurrentIndex * 7).Take(7).ToList();
        this.Current = new CalendarWeekView(daysOfWeek.FirstOrDefault(), this.Calendar, this.FirstDayOfWeek) {Index = daysOfWeek.FirstOrDefault()};
        this.Current.Dates.AddRange(daysOfWeek);
      }

      return hasNext;
    }

    /// <inheritdoc />
    public void Reset()
    {
      this.CurrentIndex = -1;
      this.Current = null;
    }

    /// <inheritdoc />
    public CalendarWeekView Current { get; private set; }

    /// <inheritdoc />
    object IEnumerator.Current => this.Current;

    #endregion

    private int CurrentIndex { get; set; }
    
    public string Name => this.Index.ToString("MMMM", CultureInfo.CurrentCulture);
    public string DisplayName => ToString();

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString() => this.Index.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

    #endregion

    #region IDisposable

    /// <inheritdoc />
    public void Dispose()
    {
    }

    #endregion

    #region Implementation of IEnumerable

    /// <inheritdoc />
    public IEnumerator<CalendarWeekView> GetEnumerator()
    {
      Reset();
      return this;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public int WeekCount => (int) Math.Ceiling(this.Dates.Count / 7.0);
  }
}