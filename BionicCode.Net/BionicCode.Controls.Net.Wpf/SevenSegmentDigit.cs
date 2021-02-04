#region Info

// 2021/02/04  00:46
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BionicCode.Utilities.Net.Wpf.Extensions;

namespace BionicCode.Controls.Net.Wpf
{
  [TemplatePart(Name = "PART_HostPanel", Type = typeof(Panel))]
  public class SevenSegmentDigit : Control
  {
    #region SegmentTiltAngle dependency property

    public static readonly DependencyProperty SegmentTiltAngleProperty = DependencyProperty.Register(
      "SegmentTiltAngle",
      typeof(double),
      typeof(SevenSegmentDigit),
      new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double SegmentTiltAngle { get => (double)GetValue(SevenSegmentDigit.SegmentTiltAngleProperty); set => SetValue(SevenSegmentDigit.SegmentTiltAngleProperty, value); }

    #endregion SegmentTiltAngle dependency property

    #region SegmentHeight dependency property

    public static readonly DependencyProperty SegmentHeightProperty = DependencyProperty.Register(
      "SegmentHeight",
      typeof(double),
      typeof(SevenSegmentDigit),
      new PropertyMetadata(double.NaN));

    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]
    public double SegmentHeight { get => (double)GetValue(SevenSegmentDigit.SegmentHeightProperty); set => SetValue(SevenSegmentDigit.SegmentHeightProperty, value); }

    #endregion SegmentHeight dependency property

    #region SegmentWidth dependency property
    public static readonly DependencyProperty SegmentWidthProperty = DependencyProperty.Register(
      "SegmentWidth",
      typeof(double),
      typeof(SevenSegmentDigit),
      new PropertyMetadata(100.0));

    [System.ComponentModel.TypeConverter(typeof(System.Windows.LengthConverter))]
    public double SegmentWidth { get => (double)GetValue(SevenSegmentDigit.SegmentWidthProperty); set => SetValue(SevenSegmentDigit.SegmentWidthProperty, value); }

    #endregion SegmentWidth dependency property

    #region SegmentOnColor dependency property

    public static readonly DependencyProperty SegmentOnColorProperty = DependencyProperty.Register(
      "SegmentOnColor",
      typeof(Brush),
      typeof(SevenSegmentDigit),
      new PropertyMetadata(default));

    public Brush SegmentOnColor { get => (Brush)GetValue(SevenSegmentDigit.SegmentOnColorProperty); set => SetValue(SevenSegmentDigit.SegmentOnColorProperty, value); }

    #endregion SegmentOnColor dependency property

    #region SegmentOffColor dependency property

    public static readonly DependencyProperty SegmentOffColorProperty = DependencyProperty.Register(
      "SegmentOffColor",
      typeof(Brush),
      typeof(SevenSegmentDigit),
      new PropertyMetadata(default));

    public Brush SegmentOffColor { get => (Brush)GetValue(SevenSegmentDigit.SegmentOffColorProperty); set => SetValue(SevenSegmentDigit.SegmentOffColorProperty, value); }

    #endregion SegmentOffColor dependency property

    #region Stretch dependency property

    public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
      "Stretch",
      typeof(Stretch),
      typeof(SevenSegmentDigit),
      new PropertyMetadata(System.Windows.Media.Stretch.Uniform));

    public Stretch Stretch { get => (Stretch) GetValue(SevenSegmentDigit.StretchProperty); set => SetValue(SevenSegmentDigit.StretchProperty, value); }

    #endregion Stretch dependency property

    private ScaleTransform StretchTransform { get; }
    private Canvas SegmentPanel { get; }
    private double GapWidth { get; set; }
    private double GapWidthToSegmentWidthRatio => 0.15;
    private double SegmentWidthToSegmentHeightRatio => 0.2;

    static SevenSegmentDigit()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SevenSegmentDigit), new FrameworkPropertyMetadata(typeof(SevenSegmentDigit)));
    }

    public SevenSegmentDigit()
    {
      this.StretchTransform = new ScaleTransform();
      var transformGroup = new TransformGroup();
      transformGroup.Children.Add(this.StretchTransform);
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

    private double GetHorizontalOffset() => Math.Cos((90 - this.SegmentTiltAngle).ToRadians()) * (this.SegmentWidth * 2) + this.SegmentHeight / 2 + this.GapWidth / 2;

    private Size GetNaturalSize()
    {
      double horizontalOffset = GetHorizontalOffset();
      double coercedVerticalSegmentHeight = Math.Cos(this.SegmentTiltAngle.ToRadians()) * this.SegmentWidth;
      var height = 2 * coercedVerticalSegmentHeight + 4 * this.GapWidth + this.SegmentHeight + this.Padding.Top;
      var width = horizontalOffset + 2 * this.GapWidth + this.SegmentWidth +
                  this.SegmentHeight / 2 + this.Padding.Left;
      return new Size(width, height);
    }
    
    private void DrawSegments(Rect bounds)
    {
      this.SegmentPanel.Children.Clear();

      double horizontalOffset = GetHorizontalOffset();
      bounds.Offset(horizontalOffset + this.Padding.Left, this.Padding.Top);
      double coercedVerticalSegmentHeight = Math.Cos(this.SegmentTiltAngle.ToRadians()) * this.SegmentWidth;

      var horizontalTopSegment = new HorizontalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 0,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(horizontalTopSegment);
      Canvas.SetLeft(horizontalTopSegment, bounds.X + this.GapWidth);
      Canvas.SetTop(horizontalTopSegment, bounds.Y);

      var verticalTopLeftSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 5,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(verticalTopLeftSegment);
      Canvas.SetLeft(verticalTopLeftSegment, bounds.X);
      Canvas.SetTop(verticalTopLeftSegment, bounds.Y + this.GapWidth);

      var verticalTopRightSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 1,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(verticalTopRightSegment);
      Canvas.SetLeft(verticalTopRightSegment, bounds.X + this.SegmentWidth + 2 * this.GapWidth);
      Canvas.SetTop(verticalTopRightSegment, bounds.Y + this.GapWidth);

      var verticalBottomRightSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 2,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(verticalBottomRightSegment);
      Canvas.SetLeft(verticalBottomRightSegment, bounds.X + this.SegmentWidth + 2 * this.GapWidth - this.SegmentHeight);
      Canvas.SetTop(verticalBottomRightSegment, bounds.Y + coercedVerticalSegmentHeight + 3 * this.GapWidth);

      var verticalBottomLeftSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 4,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(verticalBottomLeftSegment);
      Canvas.SetLeft(verticalBottomLeftSegment, bounds.X - this.SegmentHeight);
      Canvas.SetTop(verticalBottomLeftSegment, bounds.Y + coercedVerticalSegmentHeight + 3 * this.GapWidth);

      horizontalTopSegment = new HorizontalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 3,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(horizontalTopSegment);
      Canvas.SetLeft(horizontalTopSegment, bounds.X - this.SegmentHeight + this.GapWidth);
      Canvas.SetTop(horizontalTopSegment, bounds.Y + coercedVerticalSegmentHeight + 2 * this.GapWidth);

      horizontalTopSegment = new HorizontalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 6,
        Fill = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      this.SegmentPanel.Children.Add(horizontalTopSegment);
      Canvas.SetLeft(horizontalTopSegment, bounds.X - this.SegmentHeight * 2 + this.GapWidth);
      Canvas.SetTop(horizontalTopSegment, bounds.Y  + 2 * coercedVerticalSegmentHeight + 4 * this.GapWidth);
    }
  }
}