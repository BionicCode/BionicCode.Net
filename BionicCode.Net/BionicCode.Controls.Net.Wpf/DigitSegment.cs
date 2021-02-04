#region Info

// 2021/02/04  00:53
// BionicCode.Controls.Net.Wpf

#endregion

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BionicCode.Controls.Net.Wpf
{
  public abstract class DigitSegment : Shape
  {
    #region Index dependency property

    public static readonly DependencyProperty IndexProperty = DependencyProperty.Register(
      "Index",
      typeof(int),
      typeof(DigitSegment),
      new PropertyMetadata(default(int)));

    public int Index { get => (int)GetValue(DigitSegment.IndexProperty); set => SetValue(DigitSegment.IndexProperty, value); }

    #endregion Index dependency property

    #region IsOn dependency property

    public static readonly DependencyProperty IsOnProperty = DependencyProperty.Register(
      "IsOn",
      typeof(bool),
      typeof(DigitSegment),
      new PropertyMetadata(default(bool)));

    public bool IsOn { get => (bool)GetValue(DigitSegment.IsOnProperty); set => SetValue(DigitSegment.IsOnProperty, value); }

    #endregion IsOn dependency property

    #region OnColor dependency property

    public static readonly DependencyProperty OnColorProperty = DependencyProperty.Register(
      "OnColor",
      typeof(Brush),
      typeof(DigitSegment),
      new PropertyMetadata(default(Brush)));

    public Brush OnColor { get => (Brush)GetValue(DigitSegment.OnColorProperty); set => SetValue(DigitSegment.OnColorProperty, value); }

    #endregion OnColor dependency property

    #region OffColor dependency property

    public static readonly DependencyProperty OffColorProperty = DependencyProperty.Register(
      "OffColor",
      typeof(Brush),
      typeof(DigitSegment),
      new PropertyMetadata(default(Brush)));

    public Brush OffColor { get => (Brush)GetValue(DigitSegment.OffColorProperty); set => SetValue(DigitSegment.OffColorProperty, value); }

    #endregion OffColor dependency property

    #region TiltAngle dependency property

    public static readonly DependencyProperty TiltAngleProperty = DependencyProperty.Register(
      "TiltAngle",
      typeof(double),
      typeof(DigitSegment),
      new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <inheritdoc />
    private Transform geometryTransform;

    public double TiltAngle { get => (double)GetValue(DigitSegment.TiltAngleProperty); set => SetValue(DigitSegment.TiltAngleProperty, value); }

    #endregion TiltAngle dependency property

    protected DigitSegment()
    {
    }

    protected abstract Geometry CreateGeometry();
    protected Rect Bounds { get; set; }

    #region Overrides of Shape
    
    /// <inheritdoc />
    protected override Geometry DefiningGeometry => CreateGeometry();

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      this.Bounds = new Rect(new Point(this.Margin.Left, this.Margin.Top), constraint);
      return base.MeasureOverride(constraint);
    }

    #endregion
  }
}