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
  public class TimePicker : ContentControl
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

    

    #region IsOpen dependency property

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
      "IsOpen",
      typeof(bool),
      typeof(TimePicker),
      new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TimePicker.OnIsOpenChanged));

    public bool IsOpen { get => (bool)GetValue(TimePicker.IsOpenProperty); set => SetValue(TimePicker.IsOpenProperty, value); }

    #endregion IsOpen dependency property

    #region AnalogTimePicker dependency property

    public static readonly DependencyProperty AnalogTimePickerProperty = DependencyProperty.Register(
      "AnalogTimePicker",
      typeof(Control),
      typeof(TimePicker),
      new PropertyMetadata(default));

    public Control AnalogTimePicker { get => (Control) GetValue(TimePicker.AnalogTimePickerProperty); set => SetValue(TimePicker.AnalogTimePickerProperty, value); }

    #endregion AnalogTimePicker dependency property

    #region LinearTimePicker dependency property

    public static readonly DependencyProperty LinearTimePickerProperty = DependencyProperty.Register(
      "LinearTimePicker",
      typeof(Control),
      typeof(TimePicker),
      new PropertyMetadata(default));

    public Control LinearTimePicker { get => (Control) GetValue(TimePicker.LinearTimePickerProperty); set => SetValue(TimePicker.LinearTimePickerProperty, value); }

    #endregion LinearTimePicker dependency property

    #region Read-only dependency properties

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

    #endregion Read-only dependency properties

    public const string VisualStatePickerOpen = "PickerOpen";
    public const string VisualStatePickerClosed = "PickerClosed";

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
      this.AnalogTimePicker = new AnalogTimePicker();
      this.Content = this.AnalogTimePicker;
    }

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

