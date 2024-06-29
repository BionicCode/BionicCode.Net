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
      new PropertyMetadata(default(bool), DigitSegment.OnIsOnChanged));

    public bool IsOn { get => (bool)GetValue(DigitSegment.IsOnProperty); set => SetValue(DigitSegment.IsOnProperty, value); }

    #endregion IsOn dependency property

    #region OnColor dependency property

    public static readonly DependencyProperty OnColorProperty = DependencyProperty.Register(
      "OnColor",
      typeof(Brush),
      typeof(DigitSegment),
      new PropertyMetadata(default, DigitSegment.OnOnColorChanged));

    public Brush OnColor { get => (Brush)GetValue(DigitSegment.OnColorProperty); set => SetValue(DigitSegment.OnColorProperty, value); }

    #endregion OnColor dependency property

    #region OffColor dependency property

    public static readonly DependencyProperty OffColorProperty = DependencyProperty.Register(
      "OffColor",
      typeof(Brush),
      typeof(DigitSegment),
      new PropertyMetadata(default, DigitSegment.OnOffColorChanged));

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

    protected Rect Bounds { get; set; }

    protected DigitSegment()
    {
    }

    protected abstract Geometry CreateGeometry();

    #region Overrides of Shape

    /// <inheritdoc />
    protected override Geometry DefiningGeometry => CreateGeometry();

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      this.Bounds = new Rect(new Point(this.Margin.Left, this.Margin.Top), constraint);
      return base.MeasureOverride(constraint);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
      LightSegment(this.IsOn);
      return base.ArrangeOverride(finalSize);
    }

    #endregion

    #region OnIsOnChanged dependency property changed handler

    private static void OnIsOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      (d as DigitSegment).OnIsOnChanged((bool)e.OldValue, (bool)e.NewValue);

    protected virtual void OnIsOnChanged(bool oldValue, bool newValue) => LightSegment(newValue);

    #endregion OnIsOnChanged dependency property changed handler

    #region OnOnColorChanged dependency property changed handler

    private static void OnOnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      (d as DigitSegment).OnOnColorChanged((Brush)e.OldValue, (Brush)e.NewValue);

    protected virtual void OnOnColorChanged(Brush oldValue, Brush newValue)
    {
      if (this.IsOn)
      {
        LightSegment(true);
      }
    }

    #endregion OnOnColorChanged dependency property changed handler

    #region OnOffColorChanged dependency property changed handler

    private static void OnOffColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      (d as DigitSegment).OnOffColorChanged((Brush)e.OldValue, (Brush)e.NewValue);

    protected virtual void OnOffColorChanged(Brush oldValue, Brush newValue)
    {
      if (!this.IsOn)
      {
        LightSegment(false);
      }
    }

    #endregion OnOffColorChanged dependency property changed handler

    protected Brush LightSegment(bool newValue) => this.Fill = newValue
        ? this.OnColor
        : this.OffColor;
  }
}