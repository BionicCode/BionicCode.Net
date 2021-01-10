#region Info

// 2021/01/09  10:13
// BionicCode.Controls.Net.Core.Wpf

#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BionicCode.Controls.Net.Core.Wpf
{
  [ContentProperty("AnalogClockFace")]
  public class AnalogTimePicker : Control
  {
    #region AnalogClockFace dependency property

    public static readonly DependencyProperty AnalogClockFaceProperty = DependencyProperty.Register(
      "AnalogClockFace",
      typeof(Canvas),
      typeof(AnalogTimePicker),
      new PropertyMetadata(default(Canvas), OnAnalogClockFaceChanged));

    public AnalogClockFace AnalogClockFace { get => (AnalogClockFace) GetValue(AnalogTimePicker.AnalogClockFaceProperty); set => SetValue(AnalogTimePicker.AnalogClockFaceProperty, value); }

    #endregion AnalogClockFace dependency property

    #region AnalogClockFaceStyle dependency property

    public static readonly DependencyProperty AnalogClockFaceStyleProperty = DependencyProperty.Register(
      "AnalogClockFaceStyle",
      typeof(Style),
      typeof(AnalogTimePicker),
      new PropertyMetadata(default));

    public Style AnalogClockFaceStyle { get => (Style) GetValue(AnalogTimePicker.AnalogClockFaceStyleProperty); set => SetValue(AnalogTimePicker.AnalogClockFaceStyleProperty, value); }

    #endregion AnalogClockFaceStyle dependency property

    #region ClockDiameter dependency property

    public static readonly DependencyProperty ClockDiameterProperty = DependencyProperty.Register(
      "ClockDiameter",
      typeof(double),
      typeof(AnalogTimePicker),
      new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    public double ClockDiameter { get => (double) GetValue(AnalogTimePicker.ClockDiameterProperty); set => SetValue(AnalogTimePicker.ClockDiameterProperty, value); }

    #endregion ClockDiameter dependency property
    private bool IsClockPointerSelectionEnabled { get; set; }
    private Line ClockSelectionPointer { get; set; }

    static AnalogTimePicker()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(AnalogTimePicker), new FrameworkPropertyMetadata(typeof(AnalogTimePicker)));
    }

    public AnalogTimePicker()
    {
      //this.AnalogClockFace = new AnalogClockFace() {Background = Brushes.PaleVioletRed};
    }

    #region Overrides of AnalogClockFace

    //protected virtual void OnClockFaceLoaded(object sender, RoutedEventArgs routedEventArgs)
    //{
    //  double radius = this.ClockDiameter / 2;

    //  this.ClockSelectionPointer = new Line() { X1 = radius, Y1 = radius, X2 = radius, Y2 = 0, Stroke = Brushes.Orange, StrokeThickness = 2 };
    //  this.AnalogClockFace.AddElementToClockFace(this.ClockSelectionPointer, new Point());
    //  this.AnalogClockFace.AddElementToClockFace(new Line() { X1 = radius, Y1 = radius, X2 = 0, Y2 = radius, Stroke = Brushes.Orange, StrokeThickness = 2 }, new Point());
    //  //selectPointer.PreviewMouseLeftButtonDown += timePicker.OnSelectPointerLeftMouseButtonDown;
    //}

    #endregion

    #region Overrides of Control

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      constraint = new Size(this.ClockDiameter, this.ClockDiameter);
      this.AnalogClockFace.Diameter = this.ClockDiameter;
      //this.AnalogClockFace.Measure(constraint);
      base.MeasureOverride(constraint);
      return constraint;
    }

    #endregion

    private static void OnAnalogClockFaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      (d as AnalogTimePicker).OnAnalogClockFaceChanged();
    }

    protected virtual void OnAnalogClockFaceChanged()
    {
      // this.AnalogClockFace.ClockFaceLoaded += OnClockFaceLoaded;
    }

    //private void OnSelectPointerLeftMouseButtonDown(object sender, MouseButtonEventArgs e)
    //{
    //  this.IsClockPointerSelectionEnabled = true;
    //  //CaptureMouse();
    //}

    //#region Overrides of UIElement

    ///// <inheritdoc />
    //protected override void OnPreviewMouseMove(MouseEventArgs e)
    //{
    //  base.OnPreviewMouseMove(e);
    //  if (!this.IsClockPointerSelectionEnabled)
    //  {
    //    return;
    //  }

    //  Point mousePosition = e.GetPosition(this.ClockPanel);
    //  var quadrant = mousePosition.X >= 200 && mousePosition.Y <= 200 ? 1 :
    //    mousePosition.X < 200 && mousePosition.Y <= 200 ? 2 :
    //    mousePosition.X < 200 && mousePosition.Y > 200 ? 3 : 4;
    //  bool isRightQuadrantActive = quadrant == 1 || quadrant == 4;
    //  bool isTopQuadrantActive = quadrant == 1 || quadrant == 2;
    //  var intervalPositions = this.ClockPanel.Children
    //    .Cast<UIElement>()
    //    .Where(element => !object.ReferenceEquals(element, this.ClockSelectionPointer))
    //    .Select(element => new Point(Canvas.GetLeft(element), Canvas.GetTop(element)))
    //    .Where(cartesianPoint => isRightQuadrantActive && isTopQuadrantActive
    //      ? cartesianPoint.X >= 200 && cartesianPoint.Y <= 200
    //      : isRightQuadrantActive
    //      ? cartesianPoint.X >= 200 && cartesianPoint.Y > 200 : isTopQuadrantActive ? cartesianPoint.X < 200 && cartesianPoint.Y <= 200 :
    //      cartesianPoint.X < 200 && cartesianPoint.Y > 200);
    //  Point closestIntervalPosition = intervalPositions.Aggregate(intervalPositions.First(), (closestIntervalPosition, intervalPosition) => Math.Abs(intervalPosition.X - mousePosition.X) < Math.Abs(closestIntervalPosition.X - mousePosition.X) && Math.Abs(intervalPosition.Y - mousePosition.Y) < Math.Abs(closestIntervalPosition.Y - mousePosition.Y) ? intervalPosition : closestIntervalPosition);

    //  this.ClockSelectionPointer.X2 = closestIntervalPosition.X;
    //  this.ClockSelectionPointer.Y2 = closestIntervalPosition.Y;
    //}

    ///// <inheritdoc />
    //protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    //{
    //  base.OnPreviewMouseLeftButtonUp(e);
    //  this.IsClockPointerSelectionEnabled = false;
    //  ReleaseMouseCapture();
    //}

    //#endregion
  }
}