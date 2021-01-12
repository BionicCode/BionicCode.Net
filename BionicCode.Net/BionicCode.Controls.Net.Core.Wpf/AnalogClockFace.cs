#region Info

// 2021/01/09  15:11
// BionicCode.Controls.Net.Core.Wpf

#endregion

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using BionicCode.Controls.Net.Core.Wpf.Converters;
using BionicCode.Utilities.Net.Core.Wpf.Converter;
using BionicCode.Utilities.Net.Core.Wpf.Extensions;

namespace BionicCode.Controls.Net.Core.Wpf
{
  [TemplatePart(Name = "PART_ElementHost", Type = typeof(FrameworkElement))]
  public class AnalogClockFace : ContentControl
  {
    #region IsCenterElementOnCircumferenceEnabled attached property

    public static readonly DependencyProperty IsCenterElementOnCircumferenceEnabledProperty = DependencyProperty.RegisterAttached(
      "IsCenterElementOnCircumferenceEnabled", 
      typeof(bool), 
      typeof(AnalogClockFace), 
      new PropertyMetadata(true));

    public static void SetIsCenterElementOnCircumferenceEnabled(DependencyObject attachingElement, bool value) => attachingElement.SetValue(AnalogClockFace.IsCenterElementOnCircumferenceEnabledProperty, value);

    public static bool GetIsCenterElementOnCircumferenceEnabled(DependencyObject attachingElement) => (bool) attachingElement.GetValue(AnalogClockFace.IsCenterElementOnCircumferenceEnabledProperty);


    #endregion IsCenterElementOnCircumferenceEnabled attached property
    #region Is15MinuteIntervalEnabled dependency property

    public static readonly DependencyProperty Is15MinuteIntervalEnabledProperty = DependencyProperty.Register(
      "Is15MinuteIntervalEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(true));

    public bool Is15MinuteIntervalEnabled { get => (bool) GetValue(AnalogClockFace.Is15MinuteIntervalEnabledProperty); set => SetValue(AnalogClockFace.Is15MinuteIntervalEnabledProperty, value); }

    #endregion Is15MinuteIntervalEnabled dependency property

    #region Is5MinuteIntervalEnabled dependency property

    public static readonly DependencyProperty Is5MinuteIntervalEnabledProperty = DependencyProperty.Register(
      "Is5MinuteIntervalEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(true));

    public bool Is5MinuteIntervalEnabled { get => (bool) GetValue(AnalogClockFace.Is5MinuteIntervalEnabledProperty); set => SetValue(AnalogClockFace.Is5MinuteIntervalEnabledProperty, value); }

    #endregion Is5MinuteIntervalEnabled dependency property

    #region IsShowingBig12Interval dependency property

    public static readonly DependencyProperty IsShowingBig12IntervalProperty = DependencyProperty.Register(
      "IsShowingBig12Interval",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public bool IsShowingBig12Interval { get => (bool) GetValue(AnalogClockFace.IsShowingBig12IntervalProperty); set => SetValue(AnalogClockFace.IsShowingBig12IntervalProperty, value); }

    #endregion IsShowingBig12Interval dependency property

    #region FifteenMinuteIntervalElement dependency property

    public static readonly DependencyProperty FifteenMinuteIntervalElementProperty = DependencyProperty.Register(
      "FifteenMinuteIntervalElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public UIElement FifteenMinuteIntervalElement { get => (UIElement) GetValue(AnalogClockFace.FifteenMinuteIntervalElementProperty); set => SetValue(AnalogClockFace.FifteenMinuteIntervalElementProperty, value); }

    #endregion FifteenMinuteIntervalElement dependency property

    #region FiveMinuteIntervalElement dependency property

    public static readonly DependencyProperty FiveMinuteIntervalElementProperty = DependencyProperty.Register(
      "FiveMinuteIntervalElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public UIElement FiveMinuteIntervalElement { get => (UIElement) GetValue(AnalogClockFace.FiveMinuteIntervalElementProperty); set => SetValue(AnalogClockFace.FiveMinuteIntervalElementProperty, value); }

    #endregion FiveMinuteIntervalElement dependency property

    #region MinuteIntervalElement dependency property

    public static readonly DependencyProperty MinuteIntervalElementProperty = DependencyProperty.Register(
      "MinuteIntervalElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public UIElement MinuteIntervalElement { get => (UIElement) GetValue(AnalogClockFace.MinuteIntervalElementProperty); set => SetValue(AnalogClockFace.MinuteIntervalElementProperty, value); }

    #endregion MinuteIntervalElement dependency property

    #region FifteenMinuteIntervalLabel dependency property

    public static readonly DependencyProperty FifteenMinuteIntervalLabelProperty = DependencyProperty.Register(
      "FifteenMinuteIntervalLabel",
      typeof(ContentControl),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public ContentControl FifteenMinuteIntervalLabel { get => (ContentControl) GetValue(AnalogClockFace.FifteenMinuteIntervalLabelProperty); set => SetValue(AnalogClockFace.FifteenMinuteIntervalLabelProperty, value); }

    #endregion FifteenMinuteIntervalLabel dependency property

    #region FiveMinuteIntervalLabel dependency property

    public static readonly DependencyProperty FiveMinuteIntervalLabelProperty = DependencyProperty.Register(
      "FiveMinuteIntervalLabel",
      typeof(ContentControl),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public ContentControl FiveMinuteIntervalLabel { get => (ContentControl) GetValue(AnalogClockFace.FiveMinuteIntervalLabelProperty); set => SetValue(AnalogClockFace.FiveMinuteIntervalLabelProperty, value); }

    #endregion FiveMinuteIntervalLabel dependency property

    #region MinuteIntervalLabel dependency property

 public static readonly DependencyProperty MinuteIntervalLabelProperty = DependencyProperty.Register(
   "MinuteIntervalLabel",
   typeof(ContentControl),
   typeof(AnalogClockFace),
   new FrameworkPropertyMetadata(
     default,
     FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public ContentControl MinuteIntervalLabel { get => (ContentControl) GetValue(AnalogClockFace.MinuteIntervalLabelProperty); set => SetValue(AnalogClockFace.MinuteIntervalLabelProperty, value); }

    #endregion MinuteIntervalLabel dependency property

    #region IntervalLabelFormatter dependency property

    public static readonly DependencyProperty IntervalLabelFormatterProperty = DependencyProperty.Register(
      "IntervalLabelFormatter",
      typeof(Func<int, object>),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public Func<int, object> IntervalLabelFormatter { get => (Func<int, object>) GetValue(AnalogClockFace.IntervalLabelFormatterProperty); set => SetValue(AnalogClockFace.IntervalLabelFormatterProperty, value); }

    #endregion IntervalLabelFormatter dependency property

    #region HourHandElement dependency property

    public static readonly DependencyProperty HourHandElementProperty = DependencyProperty.Register(
      "HourHandElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(UIElement), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnHourHandElementChanged));

    public UIElement HourHandElement { get => (UIElement) GetValue(AnalogClockFace.HourHandElementProperty); set => SetValue(AnalogClockFace.HourHandElementProperty, value); }

    #endregion HourHandElement dependency property

    #region MinuteHandElement dependency property

    public static readonly DependencyProperty MinuteHandElementProperty = DependencyProperty.Register(
      "MinuteHandElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(UIElement), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnMinuteHandElementChanged));

    public UIElement MinuteHandElement { get => (UIElement) GetValue(AnalogClockFace.MinuteHandElementProperty); set => SetValue(AnalogClockFace.MinuteHandElementProperty, value); }

    #endregion MinuteHandElement dependency property

    #region SecondHandElement dependency property

    public static readonly DependencyProperty SecondHandElementProperty = DependencyProperty.Register(
      "SecondHandElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(UIElement3D), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, AnalogClockFace.OnSecondHandElementChanged));

    public UIElement SecondHandElement { get => (UIElement) GetValue(AnalogClockFace.SecondHandElementProperty); set => SetValue(AnalogClockFace.SecondHandElementProperty, value); }

    #endregion SecondHandElement dependency property

    #region SelectedHour dependency property

    public static readonly DependencyProperty SelectedHourProperty = DependencyProperty.Register(
      "SelectedHour",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSelectedHourChanged, AnalogClockFace.CoerceHours));

    public double SelectedHour { get => (double) GetValue(AnalogClockFace.SelectedHourProperty); set => SetValue(AnalogClockFace.SelectedHourProperty, value); }

    #endregion SelectedHour dependency property

    #region SelectedMinute dependency property

    public static readonly DependencyProperty SelectedMinuteProperty = DependencyProperty.Register(
      "SelectedMinute",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSelectedMinuteChanged, AnalogClockFace.CoerceMinutes));

    public double SelectedMinute { get => (double) GetValue(AnalogClockFace.SelectedMinuteProperty); set => SetValue(AnalogClockFace.SelectedMinuteProperty, value); }

    #endregion SelectedMinute dependency property

    #region SelectedSecond dependency property

    public static readonly DependencyProperty SelectedSecondProperty = DependencyProperty.Register(
      "SelectedSecond",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnSelectedSecondChanged, AnalogClockFace.CoerceSeconds));

    public double SelectedSecond { get => (double) GetValue(AnalogClockFace.SelectedSecondProperty); set => SetValue(AnalogClockFace.SelectedSecondProperty, value); }

    #endregion SelectedSecond dependency property
    
    #region ClockFaceLoadedRoutedEvent

    public static readonly RoutedEvent ClockFaceLoadedRoutedEvent = EventManager.RegisterRoutedEvent("ClockFaceLoaded",
      RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AnalogClockFace));

    public event RoutedEventHandler ClockFaceLoaded
    {
      add => AddHandler(AnalogClockFace.ClockFaceLoadedRoutedEvent, value);
      remove => RemoveHandler(AnalogClockFace.ClockFaceLoadedRoutedEvent, value);
    }

    #endregion

    #region Diameter read-only dependecy property

    protected static readonly DependencyPropertyKey DiameterPropertyKey = DependencyProperty.RegisterReadOnly(
      "Diameter",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty DiameterProperty = AnalogClockFace.DiameterPropertyKey.DependencyProperty;

    public double Diameter
    {
      get => (double)GetValue(AnalogClockFace.DiameterProperty);
      private set => SetValue(AnalogClockFace.DiameterPropertyKey, value);
    }

    #endregion Diameter read-only dependecy property

    #region Radius read-only dependecy property

    protected static readonly DependencyPropertyKey RadiusPropertyKey = DependencyProperty.RegisterReadOnly(
      "Radius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty RadiusProperty = AnalogClockFace.RadiusPropertyKey.DependencyProperty;

    public double Radius
    {
      get => (double)GetValue(AnalogClockFace.RadiusProperty);
      private set => SetValue(AnalogClockFace.RadiusPropertyKey, value);
    }

    #endregion Radius read-only dependecy property

    #region Is24HModeEnabled dependency property

    public static readonly DependencyProperty Is24HModeEnabledProperty = DependencyProperty.Register(
      "Is24HModeEnabled",
      typeof(bool),
      typeof(AnalogClockFace),
      new PropertyMetadata(default));

    public bool Is24HModeEnabled { get => (bool) GetValue(AnalogClockFace.Is24HModeEnabledProperty); set => SetValue(AnalogClockFace.Is24HModeEnabledProperty, value); }

    #endregion Is24HModeEnabled dependency property

    private Canvas ClockFaceCanvas { get; set; }
    private RotateTransform HourHandTransform { get; set; }
    private RotateTransform MinuteHandTransform { get; set; }
    private RotateTransform SecondHandTransform { get; set; }
    private RotateTransform IntervalElementTransform { get; set; }

    static AnalogClockFace()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnalogClockFace), new FrameworkPropertyMetadata(typeof(AnalogClockFace)));
    }
    public AnalogClockFace()
    {
      InitializeClockRotateTransforms();
      InitializeClockFaceCanvas();
      this.IntervalLabelFormatter = value => value.ToString();
    }
    
    private void InitializeClockFaceCanvas()
    {
      this.ClockFaceCanvas = new Canvas();
      this.Content = this.ClockFaceCanvas;

      var widthBinding = new Binding(nameof(this.Diameter)) {Source = this};
      this.ClockFaceCanvas.SetBinding(FrameworkElement.WidthProperty, widthBinding);
      var heightBinding = new Binding(nameof(this.Height)) {Source = this};
      this.ClockFaceCanvas.SetBinding(FrameworkElement.HeightProperty, heightBinding);
    }

    private void InitializeClockRotateTransforms()
    {
      this.IntervalElementTransform = new RotateTransform(0, this.Radius, this.Radius);
      this.HourHandTransform = new RotateTransform();
      this.MinuteHandTransform = new RotateTransform(0, this.Radius, this.Radius);
      this.SecondHandTransform = new RotateTransform(0, this.Radius, this.Radius);
      var radiusBinding = new Binding(nameof(this.Radius)) { Source = this };
      BindingOperations.SetBinding(this.IntervalElementTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.IntervalElementTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.HourHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.HourHandTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.MinuteHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.MinuteHandTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.SecondHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.SecondHandTransform, RotateTransform.CenterYProperty, radiusBinding);
    }
    

    #region Overrides of FrameworkElement
    
    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      constraint = base.MeasureOverride(constraint);
      DrawAnalogClock();
      return constraint;
    }

    /// <inheritdoc />
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
      base.OnRenderSizeChanged(sizeInfo);
      this.Diameter = this.ActualWidth;
      this.Radius = this.Diameter / 2;
    }

    #endregion


    
    private static void OnHourHandElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnHourHandElementChanged(e.OldValue as UIElement, e.NewValue as UIElement);
    }

    private static void OnMinuteHandElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnMinuteHandElementChanged(e.OldValue as UIElement, e.NewValue as UIElement);
    }

    private static void OnSecondHandElementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSecondHandElementChanged(e.OldValue as UIElement, e.NewValue as UIElement);
    }

    private static void OnSelectedHourChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSelectedHourChanged((double) e.OldValue, (double) e.NewValue);
    }

    private static void OnSelectedMinuteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSelectedMinuteChanged((double)e.OldValue, (double)e.NewValue);
    }

    private static void OnSelectedSecondChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnSelectedSecondChanged((double)e.OldValue, (double)e.NewValue);
    }

    //private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  (d as AnalogClockFace).OnDiameterChanged((double)e.OldValue, (double)e.NewValue);
    //}

    private bool IsUpdatingSelectedTimeComponent { get; set; }
    private static object CoerceHours(DependencyObject d, object basevalue)
    {
      var this_ = d as AnalogClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }
      this_.IsUpdatingSelectedTimeComponent = true;

      var hourValue = (double)basevalue;
      double decimalPart = hourValue - Math.Truncate(hourValue);
      var decimalMinutes = decimalPart * 60;
      this_.SelectedMinute = Math.Truncate(decimalPart * 60);
      decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      this_.SelectedSecond = Math.Round(decimalPart * 60, MidpointRounding.AwayFromZero);
      
      this_.IsUpdatingSelectedTimeComponent = false;
      return hourValue % 12 == 0
        ? 12
        : hourValue % 12;
    }

    // Accept time in minutes (convert to h:m:s)
    private static object CoerceMinutes(DependencyObject d, object basevalue)
    {
      var this_ = d as AnalogClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }
      this_.IsUpdatingSelectedTimeComponent = true;
      var minuteValue = (double) basevalue;
      this_.SelectedHour = (int) Math.Truncate(minuteValue / 60) % 12;
      var decimalMinutes = minuteValue % 60;
      var minutes = Math.Truncate(decimalMinutes);
      double decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      this_.SelectedSecond = (int) Math.Round(decimalPart * 60, MidpointRounding.AwayFromZero);

      this_.IsUpdatingSelectedTimeComponent = false;
      return minutes;
    }

    // Accept time in seconds (convert to h:m:s)
    private static object CoerceSeconds(DependencyObject d, object basevalue)
    {
      var this_ = d as AnalogClockFace;
      if (this_.IsUpdatingSelectedTimeComponent)
      {
        return basevalue;
      }
      this_.IsUpdatingSelectedTimeComponent = true;
      var secondsValue = (double) basevalue;
      this_.SelectedHour = (int) Math.Truncate(secondsValue / 3600) % 12;
      double decimalMinutes = secondsValue % 60;
      this_.SelectedMinute = (int) Math.Truncate(decimalMinutes);
      double decimalPart = decimalMinutes - Math.Truncate(decimalMinutes);
      var seconds = decimalPart * 60;

      this_.IsUpdatingSelectedTimeComponent = false;
      return seconds;
    }

    //protected virtual void OnDiameterChanged(double oldValue, double newValue)
    //{
    //  this.Radius = newValue / 2;
    //}

    protected virtual void OnSelectedHourChanged(double oldValue, double newValue)
    {
      double angle = GetClockAngle(newValue, 12);
      this.HourHandTransform.Angle = angle;
    }

    protected virtual void OnSelectedMinuteChanged(double oldValue, double newValue)
    {
      double angle = GetClockAngle(newValue, 60);
      this.MinuteHandTransform.Angle = angle;
    }

    protected virtual void OnSelectedSecondChanged(double oldValue, double newValue)
    {
      double angle = GetClockAngle(newValue, 60);
      this.SecondHandTransform.Angle = angle;
    }

    protected virtual void OnHourHandElementChanged(UIElement oldClockHand, UIElement newClockHand)
    {
      if (oldClockHand != null)
      {
        if (oldClockHand.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.HourHandTransform);
        }
        else
        {
          oldClockHand.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldClockHand);

      if (newClockHand == null)
      {
        return;
      }

      if (newClockHand.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.HourHandTransform);
      }
      else
      {
        newClockHand.RenderTransform = this.HourHandTransform;
      }
      AddElementToClockFace(newClockHand, new Point(Canvas.GetLeft(newClockHand), Canvas.GetTop(newClockHand)));
    }

    protected virtual void OnMinuteHandElementChanged(UIElement oldClockHand, UIElement newClockHand)
    {
      if (oldClockHand != null)
      {
        if (oldClockHand.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.MinuteHandTransform);
        }
        else
        {
          oldClockHand.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldClockHand);

      if (newClockHand == null)
      {
        return;
      }

      if (newClockHand.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.MinuteHandTransform);
      }
      else
      {
        newClockHand.RenderTransform = this.MinuteHandTransform;
      }
      AddElementToClockFace(newClockHand, new Point(Canvas.GetLeft(newClockHand), Canvas.GetTop(newClockHand)));
    }

    protected virtual void OnSecondHandElementChanged(UIElement oldClockHand, UIElement newClockHand)
    {
      if (oldClockHand != null)
      {
        if (oldClockHand.RenderTransform is TransformGroup oldTransformGroup)
        {
          oldTransformGroup.Children.Remove(this.SecondHandTransform);
        }
        else
        {
          oldClockHand.RenderTransform = null;
        }
      }

      RemoveElementFromClockFace(oldClockHand);

      if (newClockHand == null)
      {
        return;
      }

      if (newClockHand.RenderTransform is TransformGroup newTransformGroup)
      {
        newTransformGroup.Children.Add(this.SecondHandTransform);
      }
      else
      {
        newClockHand.RenderTransform = this.SecondHandTransform;
      }
      AddElementToClockFace(newClockHand, new Point(Canvas.GetLeft(newClockHand), Canvas.GetTop(newClockHand)));
    }

    protected virtual UIElement Create15MinuteIntervalVisual()
    {
      UIElement intervalElement = this.FifteenMinuteIntervalElement != null
        ? CloneElement(this.FifteenMinuteIntervalElement)
        : new Ellipse
        {
          Width = 8,
          Height = 8,
          Fill = Brushes.Red
        };
        AddIntervalElementTransformToElement(intervalElement);
        return intervalElement;
    }

    protected virtual UIElement Create5MinuteIntervalVisual()
    {
      UIElement intervalElement = this.FiveMinuteIntervalElement != null
        ? CloneElement(this.FiveMinuteIntervalElement)
        : new Ellipse
        {
          Width = 4, Height = 4, Fill = Brushes.AntiqueWhite
        };
      AddIntervalElementTransformToElement(intervalElement);
      return intervalElement;
    }

    protected virtual UIElement CreateMinuteIntervalVisual()
    {
      UIElement intervalElement = this.MinuteIntervalElement != null
        ? CloneElement(this.MinuteIntervalElement)
        : new Ellipse
        {
          Width = 2, Height = 2, Fill = Brushes.AntiqueWhite
        };
      AddIntervalElementTransformToElement(intervalElement);
      return intervalElement;
    }

    protected virtual UIElement Create15MinuteIntervalLabel(int labelValue)
    {
      object formattedLabelValue = this.IntervalLabelFormatter?.Invoke(labelValue) ?? labelValue;
      if (this.FifteenMinuteIntervalLabel != null
          && CloneElement(this.FifteenMinuteIntervalLabel) is FrameworkElement label && label.TryAssignValueToUnknownElement(formattedLabelValue))
      {
        return label;
      }

      return new TextBlock { Text = formattedLabelValue.ToString(), Padding = new Thickness(0) };
    }

    protected virtual UIElement Create5MinuteIntervalLabel(int labelValue)
    {
      object formattedLabelValue = this.IntervalLabelFormatter?.Invoke(labelValue) ?? labelValue;
      if (this.FiveMinuteIntervalLabel != null
          && CloneElement(this.FiveMinuteIntervalLabel) is FrameworkElement label && label.TryAssignValueToUnknownElement(formattedLabelValue))
      {
        return label;
      }

      return new TextBlock {Text = formattedLabelValue.ToString(), Padding = new Thickness(0) };
    }

    protected virtual UIElement CreateMinuteIntervalLabel(int labelValue)
    {
      object formattedLabelValue = this.IntervalLabelFormatter?.Invoke(labelValue) ?? labelValue;
      if (this.MinuteIntervalLabel != null 
          && CloneElement(this.MinuteIntervalLabel) is FrameworkElement label && label.TryAssignValueToUnknownElement(formattedLabelValue))
      {
        return label;
      }

      return new TextBlock {Text = formattedLabelValue.ToString(), Padding = new Thickness(0) };
    }

    protected virtual UIElement CloneElement(UIElement elementToClone) => elementToClone.CloneElement();

    protected virtual void OnClockFaceLoaded() =>
      RaiseEvent(new RoutedEventArgs(AnalogClockFace.ClockFaceLoadedRoutedEvent, this));

    private double GetClockAngle(double reading, int steps) => reading * 360 / steps;

    private int GetStepLabel(int step) => this.Is24HModeEnabled 
      ? step 
      : step == 0 ? 12 : step / 5;

    private void AddIntervalElementTransformToElement(UIElement intervalElement)
    {
      if (!(intervalElement is FrameworkElement frameworkElement))
      {
        return;
      }

      RotateTransform renderTransform = this.IntervalElementTransform.Clone();
      renderTransform.CenterX = AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(frameworkElement) ? frameworkElement.Width / 2 : 0;
      renderTransform.CenterY = AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(frameworkElement) ? frameworkElement.Height / 2 : 0;

      if (intervalElement.RenderTransform is TransformGroup transformGroup)
      {
        transformGroup.Children.Add(renderTransform);
      }
      else
      {
        intervalElement.RenderTransform = renderTransform;
      }
    }

    protected virtual void DrawAnalogClock()
    {
      this.ClockFaceCanvas.Children.Clear();

      if (this.Diameter == 0)
      {
        return;
      }
      double steps = 60.0;
      double degreeOfStep = 360.0 / steps;
      double intervalMarkerCenterPositionRadius = -1;
      for (int step = 0; step < steps; step++)
      {
        UIElement intervalMarker;
        UIElement intervalMarkerLabel = null;
        double degreesOfCurrentStep = step * degreeOfStep;

        switch (step)
        {
          case { } is15MinutesStep when is15MinutesStep % 15 == 0:
          {
            intervalMarker = this.Is15MinuteIntervalEnabled 
              ? Create15MinuteIntervalVisual() 
              : this.Is5MinuteIntervalEnabled 
                ? Create5MinuteIntervalVisual() 
                : CreateMinuteIntervalVisual();
            if (intervalMarkerCenterPositionRadius.Equals(-1))
            {
              double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
              intervalMarkerCenterPositionRadius = this.Radius + radiusOffset;
            }

            var stepLabel = GetStepLabel(step);
            intervalMarkerLabel = this.Is15MinuteIntervalEnabled
              ? Create15MinuteIntervalLabel(stepLabel)
              : this.Is5MinuteIntervalEnabled
                ? Create5MinuteIntervalLabel(stepLabel)
                : CreateMinuteIntervalLabel(stepLabel);
            break;
          }
          case { } is5MinutesStep when is5MinutesStep % 5 == 0:
          {
            intervalMarker = Create5MinuteIntervalVisual();
            if (!this.Is15MinuteIntervalEnabled && intervalMarkerCenterPositionRadius.Equals(-1))
            {
              double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
              intervalMarkerCenterPositionRadius = this.Radius + radiusOffset;
            }

            var stepLabel = GetStepLabel(step);
            intervalMarkerLabel = Create5MinuteIntervalLabel(stepLabel);
            break;
          }
          default:
          {
            intervalMarker = CreateMinuteIntervalVisual();
            if (!this.Is15MinuteIntervalEnabled && !this.Is5MinuteIntervalEnabled && intervalMarkerCenterPositionRadius.Equals(-1))
            {
              double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
              intervalMarkerCenterPositionRadius = this.Radius + radiusOffset;
            }

            if (this.Is24HModeEnabled)
            {
              var stepLabel = GetStepLabel(step);
              intervalMarkerLabel = CreateMinuteIntervalLabel(stepLabel);
            }
            break;
          }
        }

        RotateTransform rotateTransform;
        if (intervalMarker.RenderTransform is TransformGroup transformGroup)
        {
          rotateTransform = transformGroup.Children.OfType<RotateTransform>().FirstOrDefault();
        }
        else
        {
          rotateTransform = intervalMarker.RenderTransform as RotateTransform;
        }

        if (rotateTransform != null)
        {
          rotateTransform.Angle = degreesOfCurrentStep;
        }

        Point cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, intervalMarkerCenterPositionRadius);
        if (AnalogClockFace.GetIsCenterElementOnCircumferenceEnabled(intervalMarker))
        {
          AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarker);
        }

        AddCartesianElementToClockFace(intervalMarker, cartesianPoint);

        if (intervalMarkerLabel == null)
        {
          continue;
        }
          
        double labelRadiusOffset = -24;
        double intervalMarkerLabelCenterPositionRadius = intervalMarkerCenterPositionRadius + labelRadiusOffset;
        cartesianPoint = GetCartesianPointOfStep(degreesOfCurrentStep, intervalMarkerLabelCenterPositionRadius);
        AlignElementCenterPointToRadius(ref cartesianPoint, intervalMarkerLabel);
        cartesianPoint.Offset(0, 4);
        AddCartesianElementToClockFace(intervalMarkerLabel, cartesianPoint);
      }

      double deltaToMiddleRadius = (this.Radius - intervalMarkerCenterPositionRadius) * 2;
      var clockFaceBackgroundPosition = new Point();
      clockFaceBackgroundPosition.Offset(deltaToMiddleRadius / 2, deltaToMiddleRadius / 2);
      AddElementToClockFace(new Ellipse() { Height = this.Diameter - deltaToMiddleRadius, Width = this.Diameter - deltaToMiddleRadius, Fill = Brushes.DarkRed }, clockFaceBackgroundPosition, 0);
      
      AddClockHands();
      OnClockFaceLoaded();
    }

    private void AlignElementCenterPointToRadius(ref Point cartesianPoint, UIElement element)
    {
      if (!element.IsMeasureValid)
      {
        element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      }

      double elementVerticalCenterPointOffset = element.DesiredSize.Height / 2;
      double elementHorizontalCenterPointOffset = element.DesiredSize.Width / 2;

      cartesianPoint.Offset(-elementHorizontalCenterPointOffset, elementVerticalCenterPointOffset);
    }

    private void AddClockHands()
    {
      if (this.HourHandElement != null)
      {
        AddElementToClockFace(this.HourHandElement, new Point(Canvas.GetLeft(this.HourHandElement), Canvas.GetTop(this.HourHandElement)), 2);
      }
      if (this.MinuteHandElement != null)
      {
        AddElementToClockFace(this.MinuteHandElement, new Point(Canvas.GetLeft(this.MinuteHandElement), Canvas.GetTop(this.MinuteHandElement)), 2);
      }

      if (this.SecondHandElement != null)
      {
        AddElementToClockFace(this.SecondHandElement, new Point(Canvas.GetLeft(this.SecondHandElement), Canvas.GetTop(this.SecondHandElement)), 2);
      }
    }

    private double CalculateIntervalMarkerCenterRadiusOffset(UIElement intervalMarker)
    {
      if (!intervalMarker.IsMeasureValid)
      {
        intervalMarker.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      }
      double intervalElementCenterPointOffset = intervalMarker.DesiredSize.Height / 2;
      return -intervalElementCenterPointOffset;
    }

    private Point GetCartesianPointOfStep(double degreesOfCurrentStep, double radius)
    {
      double axisOffset = this.Radius;
      // Rotate and invert degrees in order to move the 0 clock value to the top
      // (instead of the original right -> cartesian based circle)
      double rotatedDegrees = degreesOfCurrentStep - 90;
      double invertedDegrees = 360 - rotatedDegrees;
      var arcRadiantDegree = invertedDegrees * Math.PI / 180;

      double x = Math.Cos(arcRadiantDegree) * radius;
      double y = Math.Sin(arcRadiantDegree) * radius;
      var cartesianPoint = new Point(x, y);

      // Move origin from circle center to bottom left edge by its radius,
      // so that full circle is inside first quadrant
      cartesianPoint.Offset(axisOffset, axisOffset);
      return cartesianPoint;
    }

    // Adjust/flip y-axis
    protected Point ConvertCartesianPointToScreenPoint(Point point, double yMax) => new Point(point.X, yMax - 0 - point.Y);

    public void AddCartesianElementToClockFace(UIElement clockElement, Point cartesianPoint)
    {
      Point screenPoint = ConvertCartesianPointToScreenPoint(cartesianPoint, this.Diameter);
      AddElementToClockFace(clockElement, screenPoint);
    }

    public void AddElementToClockFace(UIElement clockElement, Point screenPoint, int zIndex = 1)
    {
      Canvas.SetLeft(clockElement, screenPoint.X);
      Canvas.SetTop(clockElement, screenPoint.Y);
      Panel.SetZIndex(clockElement, zIndex);
      this.ClockFaceCanvas.Children.Add(clockElement);
    }

    public void RemoveElementFromClockFace(UIElement clockElement) => this.ClockFaceCanvas.Children.Remove(clockElement);
  }
}