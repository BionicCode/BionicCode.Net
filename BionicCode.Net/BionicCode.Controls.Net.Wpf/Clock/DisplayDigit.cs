#region Info

// 2021/02/04  22:33
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BionicCode.Utilities.Net.Wpf.Extensions;

namespace BionicCode.Controls.Net.Wpf
{
  [TemplatePart(Name = "PART_HostPanel", Type = typeof(Panel))]
  public abstract class DisplayDigit : Control
  {
    #region SegmentTiltAngle dependency property

    public static readonly DependencyProperty SegmentTiltAngleProperty = DependencyProperty.Register(
      "SegmentTiltAngle",
      typeof(double),
      typeof(DisplayDigit),
      new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double SegmentTiltAngle { get => (double)GetValue(DisplayDigit.SegmentTiltAngleProperty); set => SetValue(DisplayDigit.SegmentTiltAngleProperty, value); }

    #endregion SegmentTiltAngle dependency property

    #region SegmentHeight dependency property

    public static readonly DependencyProperty SegmentHeightProperty = DependencyProperty.Register(
      "SegmentHeight",
      typeof(double),
      typeof(DisplayDigit),
      new PropertyMetadata(double.NaN));

    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]
    public double SegmentHeight { get => (double)GetValue(DisplayDigit.SegmentHeightProperty); set => SetValue(DisplayDigit.SegmentHeightProperty, value); }

    #endregion SegmentHeight dependency property

    #region SegmentWidth dependency property
    public static readonly DependencyProperty SegmentWidthProperty = DependencyProperty.Register(
      "SegmentWidth",
      typeof(double),
      typeof(DisplayDigit),
      new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsMeasure, DisplayDigit.OnSegmentWidthChanged));

    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]
    public double SegmentWidth { get => (double)GetValue(DisplayDigit.SegmentWidthProperty); set => SetValue(DisplayDigit.SegmentWidthProperty, value); }

    #endregion SegmentWidth dependency property

    #region SegmentOnColor dependency property

    public static readonly DependencyProperty SegmentOnColorProperty = DependencyProperty.Register(
      "SegmentOnColor",
      typeof(Brush),
      typeof(DisplayDigit),
      new PropertyMetadata(default));

    public Brush SegmentOnColor { get => (Brush)GetValue(DisplayDigit.SegmentOnColorProperty); set => SetValue(DisplayDigit.SegmentOnColorProperty, value); }

    #endregion SegmentOnColor dependency property

    #region SegmentOffColor dependency property

    public static readonly DependencyProperty SegmentOffColorProperty = DependencyProperty.Register(
      "SegmentOffColor",
      typeof(Brush),
      typeof(DisplayDigit),
      new PropertyMetadata(default));

    public Brush SegmentOffColor { get => (Brush)GetValue(DisplayDigit.SegmentOffColorProperty); set => SetValue(DisplayDigit.SegmentOffColorProperty, value); }

    #endregion SegmentOffColor dependency property

    #region Stretch dependency property

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
      "Stretch",
      typeof(Stretch),
      typeof(DisplayDigit),
      new PropertyMetadata(System.Windows.Media.Stretch.Uniform));

    public Stretch Stretch { get => (Stretch) GetValue(DisplayDigit.StretchProperty); set => SetValue(DisplayDigit.StretchProperty, value); }

    #endregion Stretch dependency property

    protected Canvas SegmentPanel { get; }
    protected double GapWidth { get; set; }
    protected double GapWidthToSegmentWidthRatio { get; }
    protected double SegmentWidthToSegmentHeightRatio { get; }

    static DisplayDigit()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DisplayDigit), new FrameworkPropertyMetadata(typeof(DisplayDigit)));
    }

    protected DisplayDigit()
    {
      this.GapWidthToSegmentWidthRatio = 0.05;
      this.SegmentWidthToSegmentHeightRatio = 0.2;
      this.SegmentPanel = new Canvas()
      {
        Background = Brushes.Transparent,
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Top
      };
    }

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      var panelHost = GetTemplateChild("PART_HostPanel") as Border;
      panelHost.Child = this.SegmentPanel;
    }

    #endregion

    #region Overrides of Control
    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      if (!double.IsNormal(this.SegmentHeight))
      {
        this.SegmentHeight = this.SegmentWidth * this.SegmentWidthToSegmentHeightRatio;
      }

      this.GapWidth = this.SegmentWidth * this.GapWidthToSegmentWidthRatio;
      Size requiredSize = GetNaturalSize();
      return requiredSize;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size arrangeBounds)
    {
      this.SegmentPanel.Width = arrangeBounds.Width;
      this.SegmentPanel.Height = arrangeBounds.Height;

      DrawSegments(new Rect(arrangeBounds));
      return base.ArrangeOverride(arrangeBounds);
    }
    
    #endregion

    protected abstract Size GetNaturalSize();

    protected abstract void DrawSegments(Rect bounds);

    #region OnSegmentWidthChanged dependency property changed handler

    private static void OnSegmentWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
      (d as DisplayDigit).OnSegmentWidthChanged((double) e.OldValue, (double) e.NewValue);

    protected virtual void OnSegmentWidthChanged(double oldValue, double newValue)
    {
      //this.is = false;
    }

    #endregion OnSegmentWidthChanged dependency property changed handler
  }
}