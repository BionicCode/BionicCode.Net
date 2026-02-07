#endregion

namespace BionicCode.Controls.Net.Wpf
{
    #region Info

    // 2020/11/05  10:38
    // Activitytracker

    #endregion

    #region Usings

    using System;

    public class CalendarEvent : BionicCode.Utilities.Net.ViewModel, ICalendarEvent
    {
        #region

        /// <inheritdoc />
        public DateTime Start { get => this._start; set => TrySetValue(value, ref this._start); }

        /// <inheritdoc />
        public DateTime Stop { get => this._stop; set => TrySetValue(value, ref this._stop); }

        /// <inheritdoc />
        public bool IsRecurring { get => this._isRecurring; set => TrySetValue(value, ref this._isRecurring); }

        /// <inheritdoc />
        public DateTime Interval { get => this._interval; set => TrySetValue(value, ref this._interval); }

        /// <inheritdoc />
        public object Id { get => this._id; set => TrySetValue(value, ref this._id); }

        /// <inheritdoc />
        public string Summary { get => this._summary; set => TrySetValue(value, ref this._summary); }

        #endregion

        private readonly object _id;
        private readonly DateTime _interval;
        private readonly bool _isRecurring;
        private readonly DateTime _start;
        private readonly DateTime _stop;
        private readonly string _summary;
    }
}
