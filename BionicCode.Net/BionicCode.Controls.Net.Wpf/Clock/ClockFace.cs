#region Info

// 2021/01/31  14:29
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BionicCode.Utilities.Net;

namespace BionicCode.Controls.Net.Wpf
{
  [TemplatePart(Name = "PART_HostPanel", Type = typeof(Panel))]
  public abstract class ClockFace : Control
  {
    #region SelectedTimeChangedRoutedEvent

    public static readonly RoutedEvent SelectedTimeChangedRoutedEvent = EventManager.RegisterRoutedEvent(
      "SelectedTimeChanged",
      RoutingStrategy.Bubble,
      typeof(RoutedEventHandler),
      typeof(ClockFace));

    public event RoutedEventHandler SelectedTimeChanged
    {
      add => AddHandler(ClockFace.SelectedTimeChangedRoutedEvent, value);
      remove => RemoveHandler(ClockFace.SelectedTimeChangedRoutedEvent, value);
    }

    #endregion

    #region ClockFaceLoadedRoutedEvent

    public static readonly RoutedEvent ClockFaceLoadedRoutedEvent = EventManager.RegisterRoutedEvent(
      "ClockFaceLoaded",
      RoutingStrategy.Bubble,
      typeof(RoutedEventHandler),
      typeof(ClockFace));

    public event RoutedEventHandler ClockFaceLoaded
    {
      add => AddHandler(ClockFace.ClockFaceLoadedRoutedEvent, value);
      remove => RemoveHandler(ClockFace.ClockFaceLoadedRoutedEvent, value);
    }

    #endregion

    #region DateElement dependency property

    public static readonly DependencyProperty DateElementProperty = DependencyProperty.Register(
      "DateElement",
      typeof(FrameworkElement),
      typeof(ClockFace),
      new FrameworkPropertyMetadata(
        default(FrameworkElement),
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
        ClockFace.OnDateElementChanged));

    public FrameworkElement DateElement
    {
      get => (FrameworkElement)GetValue(ClockFace.DateElementProperty);
      set => SetValue(ClockFace.DateElementProperty, value);
    }

    #endregion DateElement dependency property

    #region SelectedHour dependency property

    public static readonly DependencyProperty SelectedHourProperty = DependencyProperty.Register(
      "SelectedHour",
      typeof(double),
      typeof(ClockFace),
      new PropertyMetadata(default(double), ClockFace.OnSelectedHourChanged, ClockFace.CoerceHours));

    public double SelectedHour
    {
      get => (double)GetValue(ClockFace.SelectedHourProperty);
      set => SetValue(ClockFace.SelectedHourProperty, value);
    }

    #endregion SelectedHour dependency property

    #region SelectedMinute dependency property

    public static readonly DependencyProperty SelectedMinuteProperty = DependencyProperty.Register(
      "SelectedMinute",
      typeof(double),
      typeof(ClockFace),
      new PropertyMetadata(default(double), ClockFace.OnSelectedMinuteChanged, ClockFace.CoerceMinutes));

    public double SelectedMinute
    {
      get => (double)GetValue(ClockFace.SelectedMinuteProperty);
      set => SetValue(ClockFace.SelectedMinuteProperty, value);
    }

    #endregion SelectedMinute dependency property

    #region SelectedSecond dependency property

    public static readonly DependencyProperty SelectedSecondProperty = DependencyProperty.Register(
      "SelectedSecond",
      typeof(double),
      typeof(ClockFace),
      new PropertyMetadata(default(double), ClockFace.OnSelectedSecondChanged, ClockFace.CoerceSeconds));

    public double SelectedSecond
    {
      get => (double)GetValue(ClockFace.SelectedSecondProperty);
      set => SetValue(ClockFace.SelectedSecondProperty, value);
    }

    #endregion SelectedSecond dependency property

    #region SelectedTime dependency property

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
      "SelectedTime",
      typeof(DateTime),
      typeof(ClockFace),
      new FrameworkPropertyMetadata(
        DateTime.Now,
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        ClockFace.OnSelectedTimeChanged));

    public DateTime SelectedTime
    {
      get => (DateTime)GetValue(ClockFace.SelectedTimeProperty);
      set => SetValue(ClockFace.SelectedTimeProperty, value);
    }

    #endregion SelectedTime dependency property

    #region Is24HModeEnabled dependency property

    public static readonly DependencyProperty Is24HModeEnabledProperty = DependencyProperty.Register(
      "Is24HModeEnabled",
      typeof(bool),
      typeof(ClockFace),
      new PropertyMetadata(default));

    public bool Is24HModeEnabled
    {
      get => (bool)GetValue(ClockFace.Is24HModeEnabledProperty);
      set => SetValue(ClockFace.Is24HModeEnabledProperty, value);
    }

    #endregion Is24HModeEnabled dependency property

    #region IsDisplayDateEnabled dependency property

    public static readonly DependencyProperty IsDisplayDateEnabledProperty = DependencyProperty.Register(
      "IsDisplayDateEnabled",
      typeof(bool),
      typeof(ClockFace),
      new FrameworkPropertyMetadata(
        default(bool),
        FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public bool IsDisplayDateEnabled
    {
      get => (bool)GetValue(ClockFace.IsDisplayDateEnabledProperty);
      set => SetValue(ClockFace.IsDisplayDateEnabledProperty, value);
    }

    #endregion IsDisplayDateEnabled dependency property

    #region Stretch dependency property

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
      "Stretch",
      typeof(Stretch),
      typeof(ClockFace),
      new PropertyMetadata(System.Windows.Media.Stretch.Uniform));

    public Stretch Stretch { get => (Stretch)GetValue(ClockFace.StretchProperty); set => SetValue(ClockFace.StretchProperty, value); }

    #endregion Stretch dependency property

    protected Canvas ClockFaceCanvas { get; }
    protected ScaleTransform ClockFaceScaleTransform { get; }
    protected bool IsUpdatingSelectedTimeComponent { get; set; }
    private bool IsUpdatingSelectedTime { get; set; }

    static ClockFace() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ClockFace), new FrameworkPropertyMetadata(typeof(ClockFace)));

    protected ClockFace()
    {
      this.ClockFaceScaleTransform = new ScaleTransform(1, 1);
      this.ClockFaceCanvas = new Canvas
      {
        RenderTransform = this.ClockFaceScaleTransform,
        Background = Brushes.Transparent,
        Width = 0,
        Height = 0,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top
      };
    }

    protected virtual Point ScaleClockFaceOverride(Size arrangeBounds)
    {
      Size requiredSize = GetNaturalSize();
      if (requiredSize.Equals(new Size()) || requiredSize.Equals(arrangeBounds) || !IsValidArrangeSize(arrangeBounds) || !IsValidArrangeSize(requiredSize))
      {
        return new Point(1, 1);
      }
      double horizontalScaleFactor;
      double verticalScaleFactor;
      switch (this.Stretch)
      {
        case Stretch.UniformToFill:
          {
            horizontalScaleFactor = arrangeBounds.Width / requiredSize.Width;
            verticalScaleFactor = arrangeBounds.Height / requiredSize.Height;
            double scaleFactor = horizontalScaleFactor >= verticalScaleFactor
              ? horizontalScaleFactor
              : verticalScaleFactor;
            horizontalScaleFactor = scaleFactor;
            verticalScaleFactor = scaleFactor;
            break;
          }
        case Stretch.Uniform:
          {
            horizontalScaleFactor = arrangeBounds.Width / requiredSize.Width;
            verticalScaleFactor = arrangeBounds.Height / requiredSize.Height;
            double scaleFactor = horizontalScaleFactor <= verticalScaleFactor
              ? horizontalScaleFactor
              : verticalScaleFactor;
            horizontalScaleFactor = scaleFactor;
            verticalScaleFactor = scaleFactor;
            break;
          }
        case Stretch.Fill:
          horizontalScaleFactor = arrangeBounds.Width / requiredSize.Width;
          verticalScaleFactor = arrangeBounds.Height / requiredSize.Height;
          break;
        case Stretch.None:
        default:
          horizontalScaleFactor = 1;
          verticalScaleFactor = 1;
          break;
      }

      return new Point(horizontalScaleFactor, verticalScaleFactor);
    }

    protected bool IsValidArrangeSize(Size arrangeBounds) => !arrangeBounds.IsEmpty && double.IsNormal(arrangeBounds.Width) && double.IsNormal(arrangeBounds.Height);

    protected virtual Size GetNaturalSize() => new Size(this.ClockFaceCanvas.DesiredSize.Width, this.ClockFaceCanvas.DesiredSize.Height);

    private static object CoerceHours(DependencyObject d, object basevalue)
    {
      var this_ = d as ClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }

      this_.IsUpdatingSelectedTimeComponent = true;

      double hourValue = (double)basevalue;
      double hours = this_.Is24HModeEnabled
        ? Math.Truncate(hourValue) % 24
        : Math.Truncate(hourValue) % 12 == 0
          ? 12
          : Math.Truncate(hourValue) % 12;
      double decimalPart = hourValue - Math.Truncate(hourValue);
      double decimalMinutes = decimalPart * 60;
      this_.SelectedMinute = Math.Truncate(decimalMinutes);

      decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      var decimalSeconds = Math.Truncate(decimalPart * 60);
      this_.SelectedSecond = decimalSeconds;

      this_.IsUpdatingSelectedTimeComponent = false;
      return hours;
    }

    // Accept time in minutes (convert to h:m:s)
    private static object CoerceMinutes(DependencyObject d, object basevalue)
    {
      var this_ = d as ClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }

      this_.IsUpdatingSelectedTimeComponent = true;
      double minuteValue = (double)basevalue;
      double decimalHours = minuteValue / 60;
      this_.SelectedHour = this_.Is24HModeEnabled
        ? Math.Truncate(decimalHours) % 24
        : Math.Truncate(decimalHours) % 12 == 0
          ? 12
          : Math.Truncate(decimalHours) % 12;

      double decimalMinutes = minuteValue % 60;
      var minutes = Math.Truncate(decimalMinutes);

      var decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      double decimalSeconds = Math.Truncate(decimalPart * 60);
      this_.SelectedSecond = decimalSeconds;

      this_.IsUpdatingSelectedTimeComponent = false;
      return minutes;
    }

    // Accept time in seconds (convert to h:m:s)
    private static object CoerceSeconds(DependencyObject d, object basevalue)
    {
      var this_ = d as ClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }

      this_.IsUpdatingSelectedTimeComponent = true;
      double secondsValue = (double)basevalue;
      double decimalHours = secondsValue / 3600;
      this_.SelectedHour = this_.Is24HModeEnabled
        ? Math.Truncate(decimalHours) % 24
        : Math.Truncate(decimalHours) % 12 == 0
          ? 12
          : Math.Truncate(decimalHours) % 12;

      double minutePart = secondsValue % 3600;
      double decimalMinutes = minutePart / 60;
      this_.SelectedMinute = Math.Truncate(decimalMinutes);

      double decimalSeconds = minutePart % 60;
      var seconds = Math.Truncate(decimalSeconds);

      this_.IsUpdatingSelectedTimeComponent = false;
      return seconds;
    }

    private static void OnSelectedHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as ClockFace).OnSelectedHourChanged((double)e.OldValue, (double)e.NewValue);

    private static void OnSelectedMinuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as ClockFace).OnSelectedMinuteChanged((double)e.OldValue, (double)e.NewValue);

    private static void OnSelectedSecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as ClockFace).OnSelectedSecondChanged((double)e.OldValue, (double)e.NewValue);

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as ClockFace;

      this_.OnSelectedTimeChanged((DateTime)e.OldValue, (DateTime)e.NewValue);
    }

    private static void OnDateElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as ClockFace;
      this_.OnDateElementChanged(e.OldValue as FrameworkElement, e.NewValue as FrameworkElement);
    }

    protected virtual void OnSelectedTimeChanged(DateTime oldValue, DateTime newValue)
    {
      if (this.IsUpdatingSelectedTimeComponent)
      {
        RaiseEvent(new RoutedEventArgs(ClockFace.SelectedTimeChangedRoutedEvent, this));
        return;
      }

      this.IsUpdatingSelectedTime = true;
      this.SelectedSecond = newValue.TimeOfDay.TotalSeconds;
      this.IsUpdatingSelectedTime = false;
      RaiseEvent(new RoutedEventArgs(ClockFace.SelectedTimeChangedRoutedEvent, this));
    }

    protected virtual void OnSelectedHourChanged(double oldValue, double newValue)
    {
      if (this.IsUpdatingSelectedTime)
      {
        return;
      }

      this.SelectedTime = new DateTime(
        this.SelectedTime.Year,
        this.SelectedTime.Month,
        this.SelectedTime.Day,
        (int)newValue,
        (int)this.SelectedMinute,
        (int)this.SelectedSecond);
    }

    protected virtual void OnSelectedMinuteChanged(double oldValue, double newValue)
    {
      if (this.IsUpdatingSelectedTime)
      {
        return;
      }

      this.SelectedTime = new DateTime(
        this.SelectedTime.Year,
        this.SelectedTime.Month,
        this.SelectedTime.Day,
        (int)this.SelectedHour,
        (int)newValue,
        (int)this.SelectedSecond);
    }

    protected virtual void OnSelectedSecondChanged(double oldValue, double newValue)
    {
      if (this.IsUpdatingSelectedTime)
      {
        return;
      }

      this.SelectedTime = new DateTime(
        this.SelectedTime.Year,
        this.SelectedTime.Month,
        this.SelectedTime.Day,
        (int)this.SelectedHour,
        (int)this.SelectedMinute,
        (int)newValue);
    }

    protected virtual void OnDateElementChanged(FrameworkElement oldDateElement, FrameworkElement newDateElement)
    {
    }

    protected virtual void OnClockFaceLoaded() =>
      RaiseEvent(new RoutedEventArgs(ClockFace.ClockFaceLoadedRoutedEvent, this));

    public void AddCartesianElementToClockFace(
      FrameworkElement clockElement,
      Point cartesianPoint,
      double yMax,
      int zIndex = 1)
    {
      Point screenPoint = cartesianPoint.ToScreenPoint(yMax);
      AddElementToClockFace(clockElement, screenPoint, zIndex);
    }

    public void AddElementToClockFace(FrameworkElement clockElement, Point screenPoint, int zIndex = 1)
    {
      Canvas.SetLeft(clockElement, screenPoint.X);
      Canvas.SetTop(clockElement, screenPoint.Y);
      Panel.SetZIndex(clockElement, zIndex);
      this.ClockFaceCanvas.Children.Add(clockElement);
    }

    public void RemoveElementFromClockFace(FrameworkElement clockElement) => this.ClockFaceCanvas.Children.Remove(clockElement);

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      var hostPanel = GetTemplateChild("PART_HostPanel") as Panel;
      if (hostPanel == null)
      {
        throw new InvalidOperationException("Template part 'PART_HostPanel' of type 'Panel' not found");
      }
      hostPanel.Children.Add(this.ClockFaceCanvas);
    }

    #endregion

    #region Overrides of Control

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      if (!IsValidArrangeSize(constraint))
      {
        constraint = GetNaturalSize();
      }
      if (!IsValidArrangeSize(constraint))
      {
        constraint = base.MeasureOverride(constraint);
      }

      return constraint;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      Point scaleFactors = ScaleClockFaceOverride(arrangeBounds);
      this.ClockFaceScaleTransform.ScaleX = scaleFactors.X;
      this.ClockFaceScaleTransform.ScaleY = scaleFactors.Y;
      return base.ArrangeOverride(arrangeBounds);
    }

    #endregion
  }
}