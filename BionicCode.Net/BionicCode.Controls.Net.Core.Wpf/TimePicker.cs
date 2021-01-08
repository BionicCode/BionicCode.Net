using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BionicCode.Controls.Net.Core.Wpf
{
  /// <summary>
  /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
  ///
  /// Step 1a) Using this custom control in a XAML file that exists in the current project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:BionicCode.Controls.Net.Core.Wpf"
  ///
  ///
  /// Step 1b) Using this custom control in a XAML file that exists in a different project.
  /// Add this XmlNamespace attribute to the root element of the markup file where it is 
  /// to be used:
  ///
  ///     xmlns:MyNamespace="clr-namespace:BionicCode.Controls.Net.Core.Wpf;assembly=BionicCode.Controls.Net.Core.Wpf"
  ///
  /// You will also need to add a project reference from the project where the XAML file lives
  /// to this project and Rebuild to avoid compilation errors:
  ///
  ///     Right click on the target project in the Solution Explorer and
  ///     "Add Reference"->"Projects"->[Browse to and select this project]
  ///
  ///
  /// Step 2)
  /// Go ahead and use your control in the XAML file.
  ///
  ///     <MyNamespace:TimePicker/>
  ///
  /// </summary>
  [TemplateVisualState(Name = TimePicker.VisualStatePickerOpen, GroupName = "PanelStates")]
  [TemplateVisualState(Name = TimePicker.VisualStatePickerClosed, GroupName = "PanelStates")]
  public class TimePicker : System.Windows.Controls.Control
  {
    #region SelectedTime dependency property

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
      "SelectedTime",
      typeof(TimeSpan),
      typeof(TimePicker),
      new FrameworkPropertyMetadata(TimeSpan.Zero, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimePicker.OnSelectedTimeChanged));

    public TimeSpan SelectedTime { get => (TimeSpan)GetValue(TimePicker.SelectedTimeProperty); set => SetValue(TimePicker.SelectedTimeProperty, value); }

    #endregion SelectedTime dependency property

    #region SelectedTimeText dependency property

    public static readonly DependencyProperty SelectedTimeTextProperty = DependencyProperty.Register(
      "SelectedTimeText",
      typeof(string),
      typeof(TimePicker),
      new PropertyMetadata(TimeSpan.Zero.ToString()));

    public string SelectedTimeText { get => (string)GetValue(TimePicker.SelectedTimeTextProperty); set => SetValue(TimePicker.SelectedTimeTextProperty, value); }

    #endregion SelectedTimeText dependency property

    #region SelectedHours dependency property

    public static readonly DependencyProperty SelectedHoursProperty = DependencyProperty.Register(
      "SelectedHours",
      typeof(int),
      typeof(TimePicker),
      new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimePicker.OnSelectedHoursChanged, TimePicker.CoerceHours));

    public int SelectedHours { get => (int)GetValue(TimePicker.SelectedHoursProperty); set => SetValue(TimePicker.SelectedHoursProperty, value); }

    #endregion SelectedHours dependency property

    #region SelectedMinutes dependency property

    public static readonly DependencyProperty SelectedMinutesProperty = DependencyProperty.Register(
      "SelectedMinutes",
      typeof(int),
      typeof(TimePicker),
      new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimePicker.OnSelectedMinutesChanged, TimePicker.Coerce60BasedTimeValue));

    public int SelectedMinutes { get => (int)GetValue(TimePicker.SelectedMinutesProperty); set => SetValue(TimePicker.SelectedMinutesProperty, value); }

    #endregion SelectedMinutes dependency property

    #region SelectedSeconds dependency property

    public static readonly DependencyProperty SelectedSecondsProperty = DependencyProperty.Register(
      "SelectedSeconds",
      typeof(int),
      typeof(TimePicker),
      new FrameworkPropertyMetadata(default(int), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimePicker.OnSelectedSecondsChanged, TimePicker.Coerce60BasedTimeValue));

    public int SelectedSeconds { get => (int)GetValue(TimePicker.SelectedSecondsProperty); set => SetValue(TimePicker.SelectedSecondsProperty, value); }

    #endregion SelectedSeconds dependency property

    #region IsHoursEnabled dependency property

    public static readonly DependencyProperty IsHoursEnabledProperty = DependencyProperty.Register(
      "IsHoursEnabled",
      typeof(bool),
      typeof(TimePicker),
      new PropertyMetadata(true, TimePicker.OnTimeColumnVisibilityChanged));

    public bool IsHoursEnabled { get => (bool)GetValue(TimePicker.IsHoursEnabledProperty); set => SetValue(TimePicker.IsHoursEnabledProperty, value); }

    #endregion IsHoursEnabled dependency property

    #region IsMinutesEnabled dependency property

    public static readonly DependencyProperty IsMinutesEnabledProperty = DependencyProperty.Register(
      "IsMinutesEnabled",
      typeof(bool),
      typeof(TimePicker),
      new PropertyMetadata(true, TimePicker.OnTimeColumnVisibilityChanged));

    public bool IsMinutesEnabled { get => (bool)GetValue(TimePicker.IsMinutesEnabledProperty); set => SetValue(TimePicker.IsMinutesEnabledProperty, value); }

    #endregion IsMinutesEnabled dependency property

    #region IsSecondsEnabled dependency property

    public static readonly DependencyProperty IsSecondsEnabledProperty = DependencyProperty.Register(
      "IsSecondsEnabled",
      typeof(bool),
      typeof(TimePicker),
      new PropertyMetadata(true, TimePicker.OnTimeColumnVisibilityChanged));

    public bool IsSecondsEnabled { get => (bool)GetValue(TimePicker.IsSecondsEnabledProperty); set => SetValue(TimePicker.IsSecondsEnabledProperty, value); }

    #endregion IsSecondsEnabled dependency property

    #region Is24HourModeEnabled dependency property

    public static readonly DependencyProperty Is24HourModeEnabledProperty = DependencyProperty.Register(
      "Is24HourModeEnabled",
      typeof(bool),
      typeof(TimePicker),
      new PropertyMetadata(true, TimePicker.OnIs24HourModeEnabledChanged));

    public bool Is24HourModeEnabled { get => (bool)GetValue(TimePicker.Is24HourModeEnabledProperty); set => SetValue(TimePicker.Is24HourModeEnabledProperty, value); }

    #endregion Is24HourModeEnabled dependency property

    #region SelectedMeridiem dependency property

    public static readonly DependencyProperty SelectedMeridiemProperty = DependencyProperty.Register(
      "SelectedMeridiem",
      typeof(Meridiem),
      typeof(TimePicker),
      new PropertyMetadata(default(Meridiem), TimePicker.OnSelectedMeridiemChanged));

    public Meridiem SelectedMeridiem { get => (Meridiem)GetValue(TimePicker.SelectedMeridiemProperty); set => SetValue(TimePicker.SelectedMeridiemProperty, value); }

    #endregion SelectedMeridiem dependency property

    #region HoursSource dependency property

    public static readonly DependencyProperty HoursSourceProperty = DependencyProperty.Register(
      "HoursSource",
      typeof(ObservableCollection<int>),
      typeof(TimePicker),
      new PropertyMetadata(default));

    public ObservableCollection<int> HoursSource { get => (ObservableCollection<int>)GetValue(TimePicker.HoursSourceProperty); set => SetValue(TimePicker.HoursSourceProperty, value); }

    #endregion HoursSource dependency property

    protected static readonly DependencyPropertyKey MeridiemSourcePropertyKey = DependencyProperty.RegisterReadOnly(
      "MeridiemSource",
      typeof(IEnumerable<Meridiem>),
      typeof(TimePicker),
      new PropertyMetadata(default(IEnumerable<Meridiem>)));

    public static readonly DependencyProperty MeridiemSourceProperty = TimePicker.MeridiemSourcePropertyKey.DependencyProperty;

    public IEnumerable<Meridiem> MeridiemSource
    {
      get => (IEnumerable<Meridiem>)GetValue(TimePicker.MeridiemSourceProperty);
      private set => SetValue(TimePicker.MeridiemSourcePropertyKey, value);
    }

    protected static readonly DependencyPropertyKey MinutesSecondsSourcePropertyKey = DependencyProperty.RegisterReadOnly(
      "MinutesSecondsSource",
      typeof(IEnumerable<int>),
      typeof(TimePicker),
      new PropertyMetadata(default(IEnumerable<int>)));

    public static readonly DependencyProperty MinutesSecondsSourceProperty = TimePicker.MinutesSecondsSourcePropertyKey.DependencyProperty;

    public IEnumerable<int> MinutesSecondsSource
    {
      get => (IEnumerable<int>)GetValue(TimePicker.MinutesSecondsSourceProperty);
      private set => SetValue(TimePicker.MinutesSecondsSourcePropertyKey, value);
    }

    #region IsOpen dependency property

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
      "IsOpen",
      typeof(bool),
      typeof(TimePicker),
      new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimePicker.OnIsOpenChanged));

    public bool IsOpen { get => (bool)GetValue(TimePicker.IsOpenProperty); set => SetValue(TimePicker.IsOpenProperty, value); }

    #endregion IsOpen dependency property

    #region ClockPanel dependency property

    public static readonly DependencyProperty ClockPanelProperty = DependencyProperty.Register(
      "ClockPanel",
      typeof(Panel),
      typeof(TimePicker),
      new PropertyMetadata(default));

    public Panel ClockPanel { get => (Panel) GetValue(TimePicker.ClockPanelProperty); set => SetValue(TimePicker.ClockPanelProperty, value); }

    #endregion ClockPanel dependency property

    public const string VisualStatePickerOpen = "PickerOpen";
    public const string VisualStatePickerClosed = "PickerClosed";

    private bool IsClockPointerSelectionEnabled { get; set; }
    private Line ClockSelectionPointer { get; set; }
    private string TimeStringFormatPattern { get; set; }
    private bool IsInternalUpdate { get; set; }

    static TimePicker()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TimePicker), new FrameworkPropertyMetadata(typeof(TimePicker)));
    }

    public TimePicker()
    {
      CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
      CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern = "hh.mm.ss";
      var timePattern = new StringBuilder();
      if (this.IsHoursEnabled)
      {
        timePattern.Append("hh");
      }
      timePattern.Append(@"\:mm");
      if (this.IsSecondsEnabled)
      {
        timePattern.Append(@"\:ss");
      }

      this.TimeStringFormatPattern = timePattern.ToString();
      this.SelectedTimeText = this.SelectedTime.ToString(this.TimeStringFormatPattern, CultureInfo.CurrentCulture);
      this.MeridiemSource = new List<Meridiem>() { Meridiem.AM, Meridiem.PM };
      this.HoursSource = new ObservableCollection<int>(this.Is24HourModeEnabled ? Enumerable.Range(0, 24) : Enumerable.Range(1, 12));
      this.MinutesSecondsSource = new List<int>(Enumerable.Range(0, 60));
    }

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.ClockPanel = GetTemplateChild("PART_ClockCanvas") as Panel;

      DrawAnalogClock();
    }

    private void DrawAnalogClock()
    {
      double diameter = 480.0;
      double radius = diameter / 2;

      // Move circle off center (to the right)
      double axisOffset = radius;

      this.ClockPanel.Width = diameter;
      this.ClockPanel.Height = diameter;
      double steps = 60.0;
      double degreeOfStep = 360.0 / steps;
      for (int step = 0; step < steps; step++)
      {
        Shape intervalMarker = step switch
        {
          // 15 minutes marker
          { } is15MinutesStep when is15MinutesStep % 15 == 0 => new Ellipse()
          {
            Width = 8, Height = 8, Fill = Brushes.Red
          },
          { } is5MinutesStep when is5MinutesStep % 5 == 0 =>
            new Ellipse() {Width = 4, Height = 4, Fill = Brushes.Black},
          _ => new Ellipse() {Width = 2, Height = 2, Fill = Brushes.Black}
        };

        Point cartesianPoint = GetCartesianPointOfStep(step, degreeOfStep, radius, axisOffset);
        AddCartesianElementToCanvas(intervalMarker, cartesianPoint , true);
      }

      var selectPointer = new Line() { X1 = radius, Y1 = radius, X2 = radius, Y2 = 0, Stroke = Brushes.Orange, StrokeThickness = 2};
      // AddCartesianElementToCanvas(selectPointer, new Point(0, 0));
      this.ClockPanel.Children.Add(selectPointer);
      this.ClockSelectionPointer = selectPointer;

      selectPointer.PreviewMouseLeftButtonDown += OnSelectPointerLeftMouseButtonDown;
      //selectPointer.PreviewMouseMove += OnSelectPointerLeftMouseMove;
    }

    private void OnSelectPointerLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    {
      this.IsClockPointerSelectionEnabled = true;
      //CaptureMouse();
    }

    #region Overrides of UIElement

    /// <inheritdoc />
    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
      base.OnPreviewMouseMove(e);
      if (!this.IsClockPointerSelectionEnabled)
      {
        return;
      }

      Point mousePosition = e.GetPosition(this.ClockPanel);
      var quadrant = mousePosition.X >= 200 && mousePosition.Y <= 200 ? 1 :
        mousePosition.X < 200 && mousePosition.Y <= 200 ? 2 :
        mousePosition.X < 200 && mousePosition.Y > 200 ? 3 : 4;
      bool isRightQuadrantActive = quadrant == 1 || quadrant == 4;
      bool isTopQuadrantActive = quadrant == 1 || quadrant == 2;
      var intervalPositions = this.ClockPanel.Children
        .Cast<UIElement>()
        .Where(element => !object.ReferenceEquals(element, this.ClockSelectionPointer))
        .Select(element => new Point(Canvas.GetLeft(element), Canvas.GetTop(element)))
        .Where(position => isRightQuadrantActive && isTopQuadrantActive 
          ? position.X >= 200 && position.Y <= 200 
          : isRightQuadrantActive 
          ? position.X >= 200 && position.Y > 200 : isTopQuadrantActive ? position.X < 200 && position.Y <= 200 :
          position.X < 200 && position.Y > 200);
      Point closestIntervalPosition = intervalPositions.Aggregate(intervalPositions.First(), (closestIntervalPosition, intervalPosition) => Math.Abs(intervalPosition.X - mousePosition.X) < Math.Abs(closestIntervalPosition.X - mousePosition.X) && Math.Abs(intervalPosition.Y - mousePosition.Y) < Math.Abs(closestIntervalPosition.Y - mousePosition.Y) ? intervalPosition : closestIntervalPosition);

      this.ClockSelectionPointer.X2 = closestIntervalPosition.X;
      this.ClockSelectionPointer.Y2 = closestIntervalPosition.Y;
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonUp(e);
      this.IsClockPointerSelectionEnabled = false;
      ReleaseMouseCapture();
    }

    #endregion

    private Point GetCartesianPointOfStep(int step, double degreeOfStep, double radius, double axisOffset)
    {
      var arcRadiantDegree = step * degreeOfStep * Math.PI / 180;
      double x = Math.Cos(arcRadiantDegree) * radius + axisOffset;
      double y = Math.Sin(arcRadiantDegree) * radius + axisOffset;
      var cartesianPoint = new Point(x, y);
      return cartesianPoint;
    }

    // Adjust/flip y-axis
    private Point ConvertCartesianPointToScreenPoint(Point point, double yMax) => new Point(point.X, yMax - point.Y);
    private void AddCartesianElementToCanvas(FrameworkElement clockElement, Point position, bool isRelativelyCentered = false)
    {
      Point screenPoint = ConvertCartesianPointToScreenPoint(position, this.ClockPanel.Height);
      if (isRelativelyCentered)
      {
        screenPoint.Offset(-clockElement.Width / 2, -clockElement.Height / 2);
      }

      Canvas.SetLeft(clockElement, screenPoint.X);
      Canvas.SetTop(clockElement, screenPoint.Y);
      this.ClockPanel.Children.Add(clockElement);
    }

    #endregion

    private static void OnTimeColumnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
    {
      var this_ = d as TimePicker;
      this_.UpdateTimeFormatPattern();
      this_.FormatTime(this_.SelectedTime);
    }

    private static void OnSelectedHoursChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as TimePicker;
      this_.OnTimeComponentChanged();
    }

    private static void OnSelectedMinutesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as TimePicker;
      this_.OnTimeComponentChanged();
    }

    private static void OnSelectedSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as TimePicker;
      this_.OnTimeComponentChanged();
    }

    private static void OnIs24HourModeEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as TimePicker).OnIs24HourModeEnabledChanged((bool)e.OldValue, (bool)e.NewValue);

    private static void OnSelectedMeridiemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as TimePicker;
      this_.SelectedTimeText = this_.FormatTime(this_.SelectedTime);
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as TimePicker).OnSelectedTimeChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var isOpen = (bool)e.NewValue;
      (d as TimePicker).OnIsOpenChanged(isOpen);
    }

    private static object CoerceHours(DependencyObject d, object basevalue)
    {
      var this_ = d as TimePicker;
      var hourValue = (int)basevalue;
      return this_.Is24HourModeEnabled
        ? hourValue % 24
        : hourValue % 12 == 0
          ? 12
          : hourValue % 12;
    }

    private static object Coerce60BasedTimeValue(DependencyObject d, object basevalue)
    {
      var hourValue = (int)basevalue;
      return Math.Min(59, Math.Max(0, hourValue));
    }

    protected virtual void OnIsOpenChanged(bool isOpen)
    {
      VisualStateManager.GoToState(
        this,
        isOpen
          ? TimePicker.VisualStatePickerOpen
          : TimePicker.VisualStatePickerClosed,
        true);
    }

    protected virtual void OnTimeComponentChanged()
    {
      this.SelectedTime = new TimeSpan(this.SelectedHours, this.SelectedMinutes, this.SelectedSeconds);
    }

    protected virtual void OnSelectedTimeChanged(TimeSpan oldValue, TimeSpan newValue)
    {
      this.SelectedTimeText = FormatTime(newValue);

      if (!this.IsInternalUpdate)
      {
        this.SelectedHours = newValue.Hours;
        this.SelectedMinutes = newValue.Minutes;
        this.SelectedSeconds = newValue.Seconds;
      }
    }

    protected virtual void OnIs24HourModeEnabledChanged(bool oldValue, bool newValue)
    {
      this.IsInternalUpdate = true;

      bool is24HourModeEnabled = newValue;

      this.HoursSource = new ObservableCollection<int>(newValue ? Enumerable.Range(0, 24) : Enumerable.Range(1, 12));

      if (is24HourModeEnabled)
      {
        switch (this.SelectedMeridiem)
        {
          case Meridiem.PM when this.SelectedHours != 12:
            this.SelectedHours += 12;
            break;
          case Meridiem.AM when this.SelectedHours == 12:
            this.SelectedHours = 0;
            break;
          default:
            GetBindingExpression(TimePicker.SelectedHoursProperty)?.UpdateSource();
            this.SelectedTimeText = FormatTime(this.SelectedTime);
            break;
        }

        this.SelectedMeridiem = Meridiem.None;
        CollectionViewSource.GetDefaultView(this.HoursSource).MoveCurrentToPosition(this.SelectedHours);
      }
      else
      {
        this.SelectedMeridiem = this.SelectedHours >= 12 ? Meridiem.PM : Meridiem.AM;

        if (this.SelectedHours == 0)
        {
          this.SelectedHours = 12;
        }
        else if (this.SelectedHours != 12)
        {
          this.SelectedHours %= 12;
        }
        else
        {
          GetBindingExpression(TimePicker.SelectedHoursProperty)?.UpdateSource();
        }
        CollectionViewSource.GetDefaultView(this.HoursSource).MoveCurrentToPosition(this.SelectedHours - 1);
      }
      this.IsInternalUpdate = false;
    }

    private void UpdateTimeFormatPattern()
    {
      var timePattern = new StringBuilder();
      if (this.IsHoursEnabled)
      {
        timePattern.Append("hh");
      }

      timePattern.Append(@"\:mm");
      if (this.IsSecondsEnabled)
      {
        timePattern.Append(@"\:ss");
      }

      this.TimeStringFormatPattern = timePattern.ToString();
    }

    private string FormatTime(TimeSpan time) => this.Is24HourModeEnabled ? time.ToString(this.TimeStringFormatPattern, CultureInfo.CurrentCulture) : $"{time.ToString(this.TimeStringFormatPattern, CultureInfo.CurrentCulture)} {this.SelectedMeridiem}";
  }
}

