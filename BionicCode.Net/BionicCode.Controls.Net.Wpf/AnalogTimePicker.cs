#region Info

// 2021/01/09  10:13
// BionicCode.Controls.Net.Core.Wpf

#endregion

using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace BionicCode.Controls.Net.Wpf
{
  [ContentProperty("AnalogClockFace")]
  [TemplateVisualState(Name = AnalogTimePicker.VisualStatePickerOpen, GroupName = "PanelStates")]
  [TemplateVisualState(Name = AnalogTimePicker.VisualStatePickerClosed, GroupName = "PanelStates")]
  public class AnalogTimePicker : Control
  {
    public const string VisualStatePickerOpen = "PickerOpen";
    public const string VisualStatePickerClosed = "PickerClosed";
    #region AnalogClockFace dependency property

    public static readonly DependencyProperty AnalogClockFaceProperty = DependencyProperty.Register(
      "AnalogClockFace",
      typeof(AnalogClockFace),
      typeof(AnalogTimePicker),
      new FrameworkPropertyMetadata(default(AnalogClockFace), AnalogTimePicker.OnAnalogClockFaceChanged));

    public AnalogClockFace AnalogClockFace { get => (AnalogClockFace)GetValue(AnalogTimePicker.AnalogClockFaceProperty); set => SetValue(AnalogTimePicker.AnalogClockFaceProperty, value); }

    #endregion AnalogClockFace dependency property

    #region SelectedTime dependency property

    public static readonly DependencyProperty SelectedTimeProperty = DependencyProperty.Register(
      "SelectedTime",
      typeof(DateTime),
      typeof(AnalogTimePicker),
      new FrameworkPropertyMetadata(default(DateTime), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, AnalogTimePicker.OnSelectedTimeChanged));

    public DateTime SelectedTime { get => (DateTime)GetValue(AnalogTimePicker.SelectedTimeProperty); set => SetValue(AnalogTimePicker.SelectedTimeProperty, value); }

    #endregion SelectedTime dependency property

    #region SelectedTimeText dependency property

    public static readonly DependencyProperty SelectedTimeTextProperty = DependencyProperty.Register(
      "SelectedTimeText",
      typeof(string),
      typeof(AnalogTimePicker),
      new PropertyMetadata(TimeSpan.Zero.ToString()));

    public string SelectedTimeText
    {
      get => (string)GetValue(AnalogTimePicker.SelectedTimeTextProperty);
      set => SetValue(AnalogTimePicker.SelectedTimeTextProperty, value);
    }

    #endregion SelectedTimeText dependency property

    #region propertyName dependency property

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
      "IsOpen",
      typeof(bool),
      typeof(AnalogTimePicker),
      new FrameworkPropertyMetadata(
        default(bool),
        FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        AnalogTimePicker.OnIsOpenChanged));

    public bool IsOpen { get => (bool)GetValue(AnalogTimePicker.IsOpenProperty); set => SetValue(AnalogTimePicker.IsOpenProperty, value); }

    #endregion propertyName dependency property

    #region ClockDiameter dependency property

    public static readonly DependencyProperty ClockDiameterProperty = DependencyProperty.Register(
      "ClockDiameter",
      typeof(double),
      typeof(AnalogTimePicker),
      new PropertyMetadata(default));

    public double ClockDiameter { get => (double)GetValue(AnalogTimePicker.ClockDiameterProperty); set => SetValue(AnalogTimePicker.ClockDiameterProperty, value); }

    #endregion ClockDiameter dependency property

    #region IsClockHandVisible dependency property

    public static readonly DependencyProperty IsClockHandVisibleProperty = DependencyProperty.Register(
      "IsClockHandVisible",
      typeof(bool),
      typeof(AnalogTimePicker),
      new PropertyMetadata(default(bool), AnalogTimePicker.OnIsClockHandVisibleChanged));

    public bool IsClockHandVisible { get => (bool)GetValue(AnalogTimePicker.IsClockHandVisibleProperty); set => SetValue(AnalogTimePicker.IsClockHandVisibleProperty, value); }

    #endregion IsClockHandVisible dependency property
    private string TimeStringFormatPattern { get; set; }

    static AnalogTimePicker()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnalogTimePicker), new FrameworkPropertyMetadata(typeof(AnalogTimePicker)));
    }

    public AnalogTimePicker()
    {
      CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
      CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern = "hh.mm.ss";
      var timePattern = new StringBuilder();
      //if (this.IsHoursEnabled)
      {
        timePattern.Append("hh");
      }

      timePattern.Append(@"\:mm");
      //if (this.IsSecondsEnabled)
      {
        timePattern.Append(@"\:ss");
      }

      this.TimeStringFormatPattern = timePattern.ToString();
      //this.AnalogClockFace = new AnalogClockFace() {Background = Brushes.PaleVioletRed};
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogTimePicker;
      this_.SelectedTimeText = this_.FormatTime((DateTime)e.NewValue);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var isOpen = (bool)e.NewValue;
      (d as AnalogTimePicker).OnIsOpenChanged(isOpen);
    }

    private string FormatTime(DateTime time) => time.TimeOfDay.ToString(this.TimeStringFormatPattern, CultureInfo.CurrentCulture);

    private static void OnAnalogClockFaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogTimePicker).OnAnalogClockFaceChanged(e.OldValue as AnalogClockFace, e.NewValue as AnalogClockFace);
    }

    private static void OnIsClockHandVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var this_ = d as AnalogTimePicker;
      if (this_.AnalogClockFace == null)
      {
        return;
      }
      bool isClockHandVisible = (bool)e.NewValue;
      this_.AnalogClockFace.IsTimePickerClockHandVisible = isClockHandVisible;
    }

    protected virtual void OnAnalogClockFaceChanged(AnalogClockFace oldValue, AnalogClockFace newValue)
    {
      if (oldValue != null)
      {
        oldValue.SelectedTimeChanged -= OnSelectedAnalogClockFaceTimeChanged;
      }

      if (newValue != null)
      {
        newValue.SelectedTimeChanged += OnSelectedAnalogClockFaceTimeChanged;
        newValue.IsTimePickerClockHandVisible = this.IsClockHandVisible;
      }
    }

    protected virtual void OnIsOpenChanged(bool isOpen)
    {
      VisualStateManager.GoToState(
        this,
        isOpen
          ? AnalogTimePicker.VisualStatePickerOpen
          : AnalogTimePicker.VisualStatePickerClosed,
        true);
    }

    private void OnSelectedAnalogClockFaceTimeChanged(object sender, RoutedEventArgs e)
    {
      this.SelectedTime = this.AnalogClockFace.SelectedTime;
    }
  }
}