#region Info

// 2021/01/09  15:11
// BionicCode.Controls.Net.Core.Wpf

#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BionicCode.Controls.Net.Core.Wpf
{
  public class AnalogClockFace : Canvas
  {
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

    #region HourHandElement dependency property

    public static readonly DependencyProperty HourHandElementProperty = DependencyProperty.Register(
      "HourHandElement",
      typeof(UIElement ),
      typeof(AnalogClockFace),
      new FrameworkPropertyMetadata(default(UIElement), FrameworkPropertyMetadataOptions.AffectsRender, AnalogClockFace.OnHourHandElementChanged));

    public UIElement HourHandElement { get => (UIElement ) GetValue(AnalogClockFace.HourHandElementProperty); set => SetValue(AnalogClockFace.HourHandElementProperty, value); }

    #endregion HourHandElement dependency property

    #region MinuteHandElement dependency property

    public static readonly DependencyProperty MinuteHandElementProperty = DependencyProperty.Register(
      "MinuteHandElement",
      typeof(UIElement ),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(UIElement), AnalogClockFace.OnMinuteHandElementChanged));

    public UIElement MinuteHandElement { get => (UIElement ) GetValue(AnalogClockFace.MinuteHandElementProperty); set => SetValue(AnalogClockFace.MinuteHandElementProperty, value); }

    #endregion MinuteHandElement dependency property

    #region SecondHandElement dependency property

    public static readonly DependencyProperty SecondHandElementProperty = DependencyProperty.Register(
      "SecondHandElement",
      typeof(UIElement),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(UIElement), AnalogClockFace.OnSecondHandElementChanged));

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

    #region Diameter dependency property

    public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
      "Diameter",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double), AnalogClockFace.OnDiameterChanged));

    public double Diameter { get => (double) GetValue(AnalogClockFace.DiameterProperty); set => SetValue(AnalogClockFace.DiameterProperty, value); }

    #endregion Diameter dependency property

    #region ClockFaceLoadedRoutedEvent

    public static readonly RoutedEvent ClockFaceLoadedRoutedEvent = EventManager.RegisterRoutedEvent("ClockFaceLoaded",
      RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(AnalogClockFace));

    public event RoutedEventHandler ClockFaceLoaded
    {
      add => AddHandler(AnalogClockFace.ClockFaceLoadedRoutedEvent, value);
      remove => RemoveHandler(AnalogClockFace.ClockFaceLoadedRoutedEvent, value);
    }

    #endregion

    #region Radius read-only dependecy property

    protected static readonly DependencyPropertyKey RadiusPropertyKey = DependencyProperty.RegisterReadOnly(
      "Radius",
      typeof(double),
      typeof(AnalogClockFace),
      new PropertyMetadata(default(double)));

    public static readonly DependencyProperty RadiusProperty = AnalogClockFace.RadiusPropertyKey.DependencyProperty;

    public double Radius
    {
      get => (double) GetValue(AnalogClockFace.RadiusProperty);
      private set => SetValue(AnalogClockFace.RadiusPropertyKey, value);
    }

    #endregion Radius read-only dependecy property

    private RotateTransform HourHandTransform { get; set; }
    private RotateTransform MinuteHandTransform { get; set; }
    private RotateTransform SecondHandTransform { get; set; }

    static AnalogClockFace()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnalogClockFace), new FrameworkPropertyMetadata(typeof(AnalogClockFace)));
    }
    public AnalogClockFace()
    {
      InitializeClockHandRotateTransforms();
    }

    private void InitializeClockHandRotateTransforms()
    {
      this.HourHandTransform = new RotateTransform(0, this.Radius, this.Radius);
      this.MinuteHandTransform = new RotateTransform(0, this.Radius, this.Radius);
      this.SecondHandTransform = new RotateTransform(0, this.Radius, this.Radius);
      var radiusBinding = new Binding("Radius") {Source = this};
      BindingOperations.SetBinding(this.HourHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.HourHandTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.MinuteHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.MinuteHandTransform, RotateTransform.CenterYProperty, radiusBinding);
      BindingOperations.SetBinding(this.SecondHandTransform, RotateTransform.CenterXProperty, radiusBinding);
      BindingOperations.SetBinding(this.SecondHandTransform, RotateTransform.CenterYProperty, radiusBinding);
    }

    #region Overrides of Canvas

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      DrawAnalogClock();
      base.MeasureOverride(constraint);
      return constraint;
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

    private static void OnDiameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogClockFace).OnDiameterChanged((double)e.OldValue, (double)e.NewValue);
    }

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

    protected virtual void OnDiameterChanged(double oldValue, double newValue)
    {
      this.Radius = newValue / 2;
    }

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

      this.Children.Remove(oldClockHand);

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

      this.Children.Remove(oldClockHand);

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

      this.Children.Remove(oldClockHand);

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

    protected virtual UIElement Create15MinuteIntervalVisual() =>
      this.FifteenMinuteIntervalElement != null
        ? CloneElement(this.FifteenMinuteIntervalElement)
        : new Ellipse()
        {
          Width = 8, Height = 8, Fill = Brushes.Red
        };

    protected virtual UIElement Create5MinuteIntervalVisual() =>
      this.FiveMinuteIntervalElement != null
        ? CloneElement(this.FiveMinuteIntervalElement)
        : new Ellipse()
        {
          Width = 4, Height = 4, Fill = Brushes.AntiqueWhite
        };

    protected virtual UIElement CreateMinuteIntervalVisual() =>
      this.MinuteIntervalElement != null
        ? CloneElement(this.MinuteIntervalElement)
        : new Ellipse()
        {
          Width = 2, Height = 2, Fill = Brushes.AntiqueWhite
        };

    protected UIElement CloneElement(UIElement elementToClone)
    {
      string serializedElement = XamlWriter.Save(elementToClone);
      var cloneElement = XamlReader.Parse(serializedElement) as UIElement;
      return cloneElement;
    }

    protected virtual void OnClockFaceLoaded() =>
      RaiseEvent(new RoutedEventArgs(AnalogClockFace.ClockFaceLoadedRoutedEvent, this));

    private double GetClockAngle(double reading, int steps) => reading * 360 / steps;

    protected virtual void DrawAnalogClock()
    {
      if (double.IsInfinity(this.Diameter))
      {
        return;
      }

      this.Children.Clear();

      double radius = this.Diameter / 2;

      // Move circle center off cartesian origin (to the top right)
      double axisOffset = radius;

      double steps = 60.0;
      double degreeOfStep = 360.0 / steps;
      double intervalElementSizeDelta = -1;
      double intervalMarkerCenterPositionRadius = -1;
      for (int step = 0; step < steps; step++)
      {
        UIElement intervalMarker;
        switch (step)
        {
          case { } is15MinutesStep when is15MinutesStep % 15 == 0:
            intervalMarker = this.Is15MinuteIntervalEnabled 
              ? Create15MinuteIntervalVisual() 
              : this.Is5MinuteIntervalEnabled 
                ? Create5MinuteIntervalVisual() 
                : CreateMinuteIntervalVisual();
            if (intervalElementSizeDelta.Equals(-1))
            {
              double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
              intervalMarkerCenterPositionRadius = radius + radiusOffset;
            }
            break;
          case { } is5MinutesStep when is5MinutesStep % 5 == 0:
            intervalMarker = Create5MinuteIntervalVisual();
            if (!this.Is15MinuteIntervalEnabled && intervalElementSizeDelta.Equals(-1))
            {
              double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
              intervalMarkerCenterPositionRadius = radius + radiusOffset;
            }
            break;
          default:
            intervalMarker = CreateMinuteIntervalVisual();
            if (!this.Is15MinuteIntervalEnabled && !this.Is5MinuteIntervalEnabled && intervalElementSizeDelta.Equals(-1))
            {
              double radiusOffset = CalculateIntervalMarkerCenterRadiusOffset(intervalMarker);
              intervalMarkerCenterPositionRadius = radius + radiusOffset;
            }
            break;
        }

        intervalMarker.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        double intervalElementCenterPointOffset = intervalMarker.DesiredSize.Height / 2;
        Point cartesianPoint = GetCartesianPointOfStep(step, degreeOfStep, intervalMarkerCenterPositionRadius, new Point(axisOffset - intervalElementCenterPointOffset, axisOffset + intervalElementCenterPointOffset));
        AddCartesianElementToClockFace(intervalMarker, cartesianPoint);
      }


      //this.ClockSelectionPointer = new Line() { X1 = radius, Y1 = radius, X2 = radius, Y2 = 0, Stroke = Brushes.Orange, StrokeThickness = 2 };
      //this.AnalogClockFace.AddElementToClockFace(this.ClockSelectionPointer, new Point());
      //this.AnalogClockFace.AddElementToClockFace(new Line() { X1 = radius, Y1 = radius, X2 = 0, Y2 = radius, Stroke = Brushes.Orange, StrokeThickness = 2 }, new Point());
      OnClockFaceLoaded();
      AddClockHands();
      //selectPointer.PreviewMouseMove += OnSelectPointerLeftMouseMove;
    }

    private void AddClockHands()
    {
      if (this.HourHandElement != null)
      {
        AddElementToClockFace(this.HourHandElement, new Point(Canvas.GetLeft(this.HourHandElement), Canvas.GetTop(this.HourHandElement)));
      }
      if (this.MinuteHandElement != null)
      {
        AddElementToClockFace(this.MinuteHandElement, new Point(Canvas.GetLeft(this.MinuteHandElement), Canvas.GetTop(this.MinuteHandElement)));
      }

      if (this.SecondHandElement != null)
      {
        AddElementToClockFace(this.SecondHandElement, new Point(Canvas.GetLeft(this.SecondHandElement), Canvas.GetTop(this.SecondHandElement)));
      }
    }

    private double CalculateIntervalMarkerCenterRadiusOffset(UIElement intervalMarker)
    {
      intervalMarker.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
      double intervalElementCenterPointOffset = intervalMarker.DesiredSize.Height / 2;
      return -intervalElementCenterPointOffset;
    }

    private Point GetCartesianPointOfStep(int step, double degreeOfStep, double radius, Point axisOffset)
    {
      var arcRadiantDegree = step * degreeOfStep * Math.PI / 180;
      double x = Math.Cos(arcRadiantDegree) * radius;
      double y = Math.Sin(arcRadiantDegree) * radius;
      var cartesianPoint = new Point(x, y);
      cartesianPoint.Offset(axisOffset.X, axisOffset.Y);
      return cartesianPoint;
    }

    // Adjust/flip y-axis
    protected Point ConvertCartesianPointToScreenPoint(Point point, double yMax) => new Point(point.X, yMax - 0 - point.Y);

    public void AddCartesianElementToClockFace(UIElement clockElement, Point cartesianPoint)
    {
      Point screenPoint = ConvertCartesianPointToScreenPoint(cartesianPoint, this.Diameter);
      AddElementToClockFace(clockElement, screenPoint);
    }

    public void AddElementToClockFace(UIElement clockElement, Point screenPoint)
    {
      Canvas.SetLeft(clockElement, screenPoint.X);
      Canvas.SetTop(clockElement, screenPoint.Y);
      this.Children.Add(clockElement);
    }
  }
}