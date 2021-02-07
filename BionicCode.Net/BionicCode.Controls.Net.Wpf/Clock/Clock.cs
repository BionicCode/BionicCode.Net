using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BionicCode.Controls.Net.Wpf
{
  public class Clock : Control
  {
    #region Dependency Properties

    #region ClockFace dependency property

    public static readonly DependencyProperty ClockFaceProperty = DependencyProperty.Register(
      "ClockFace",
      typeof(ClockFace),
      typeof(Clock),
      new PropertyMetadata(default));

    public ClockFace ClockFace { get => (ClockFace) GetValue(Clock.ClockFaceProperty); set => SetValue(Clock.ClockFaceProperty, value); }

    #endregion ClockFace dependency property

    #region CurrentDateTime dependency property

    public static readonly DependencyProperty CurrentDateTimeProperty = DependencyProperty.Register(
      "CurrentDateTime",
      typeof(DateTime),
      typeof(Clock),
      new PropertyMetadata(default(DateTime), Clock.OnCurrentDateTimeChanged));

    public DateTime CurrentDateTime { get => (DateTime) GetValue(Clock.CurrentDateTimeProperty); set => SetValue(Clock.CurrentDateTimeProperty, value); }

    #endregion CurrentDateTime dependency property

    #region TimeZone dependency property

    public static readonly DependencyProperty TimeZoneProperty = DependencyProperty.Register(
      "TimeZone",
      typeof(TimeZoneInfo),
      typeof(Clock),
      new PropertyMetadata(TimeZoneInfo.Local));

    public TimeZoneInfo TimeZone { get => (TimeZoneInfo) GetValue(Clock.TimeZoneProperty); set => SetValue(Clock.TimeZoneProperty, value); }

    #endregion TimeZone dependency property

    #region Resolution dependency property

    public static readonly DependencyProperty ResolutionProperty = DependencyProperty.Register(
      "Resolution",
      typeof(TimeSpan),
      typeof(Clock),
      new PropertyMetadata(TimeSpan.FromSeconds(1)));

    public TimeSpan Resolution { get => (TimeSpan) GetValue(Clock.ResolutionProperty); set => SetValue(Clock.ResolutionProperty, value); }

    #endregion Resolution dependency property

    #region Precision dependency property

    public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register(
      "Precision",
      typeof(TimeSpan),
      typeof(Clock),
      new PropertyMetadata(TimeSpan.FromMilliseconds(50)));

    public TimeSpan Precision { get => (TimeSpan) GetValue(Clock.PrecisionProperty); set => SetValue(Clock.PrecisionProperty, value); }

    #endregion Precision dependency property

    #region DispatcherPriority dependency property

    public static readonly DependencyProperty DispatcherPriorityProperty = DependencyProperty.Register(
      "DispatcherPriority",
      typeof(DispatcherPriority),
      typeof(Clock),
      new PropertyMetadata(System.Windows.Threading.DispatcherPriority.Send));

    public DispatcherPriority DispatcherPriority { get => (DispatcherPriority) GetValue(Clock.DispatcherPriorityProperty); set => SetValue(Clock.DispatcherPriorityProperty, value); }

    #endregion DispatcherPriority dependency property

    #region Meridiem read-only dependency property
    protected static readonly DependencyPropertyKey MeridiemPropertyKey = DependencyProperty.RegisterReadOnly(
      "Meridiem",
      typeof(Meridiem),
      typeof(Clock),
      new PropertyMetadata(default(Meridiem)));

    public static readonly DependencyProperty MeridiemProperty = Clock.MeridiemPropertyKey.DependencyProperty;

    public Meridiem Meridiem
    {
      get => (Meridiem) GetValue(Clock.MeridiemProperty);
      private set => SetValue(Clock.MeridiemPropertyKey, value);
    }

    #endregion Meridiem read-only dependency property

    #endregion Dependency Properties

    public bool IsDaylightSavingTime => this.TimeZone.IsDaylightSavingTime(this.CurrentDateTime);

    private DispatcherTimer Timer { get; }

    static Clock()
    {
      FrameworkElement.DefaultStyleKeyProperty
        .OverrideMetadata(typeof(Clock), new FrameworkPropertyMetadata(typeof(Clock)));
    }

    public Clock()
    {
      this.Timer = new DispatcherTimer(this.Precision, this.DispatcherPriority, OnTimerElapsed, this.Dispatcher);
    }

    #region OnCurrentDateTimeChanged dependency property changed handler

    private static void OnCurrentDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      (d as Clock).OnCurrentDateTimeChanged((DateTime) e.OldValue, (DateTime) e.NewValue);

    protected virtual void OnCurrentDateTimeChanged(DateTime oldValue, DateTime newValue)
    {
      this.Meridiem = newValue.TimeOfDay < new TimeSpan(11, 59, 59)
        ? Meridiem.AM
        : Meridiem.PM;
    }

    #endregion OnCurrentDateTimeChanged dependency property changed handler

    private void OnTimerElapsed(object sender, EventArgs e) => this.CurrentDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.TimeZone);
  }
}
