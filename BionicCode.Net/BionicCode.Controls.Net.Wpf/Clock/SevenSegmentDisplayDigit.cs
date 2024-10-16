#region Info

// 2021/02/04  00:46
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BionicCode.Utilities.Net;
using Math = System.Math;

namespace BionicCode.Controls.Net.Wpf
{
  [TemplatePart(Name = "PART_HostPanel", Type = typeof(Panel))]
  public class SevenSegmentDisplayDigit : DisplayDigit, ISevenSegmentDigit
  {
    public int DisplayIndex { get; }

    public SevenSegmentDisplayDigit(Int16 displayIndex) => this.DisplayIndex = displayIndex;

    public void ToggleSegments(BitArray word)
    {
      DigitSegment[] segments = this.SegmentPanel.Children
        .OfType<DigitSegment>()
        .OrderBy(segment => segment.Index)
        .ToArray();

      for (int segmentIndex = 0; segmentIndex < segments.Length; segmentIndex++)
      {
        segments[segmentIndex].IsOn = word.Get(segmentIndex);
      }
    }

    private double GetHorizontalOffset() => (Math.Cos((90 - this.SegmentTiltAngle).ToRadians()) * (this.SegmentWidth * 2)) + (this.SegmentHeight / 2) + (this.GapWidth / 2);

    protected override Size GetNaturalSize()
    {
      double horizontalOffset = GetHorizontalOffset();
      double coercedVerticalSegmentHeight = Math.Cos(this.SegmentTiltAngle.ToRadians()) * this.SegmentWidth;
      double height = (2 * coercedVerticalSegmentHeight) + (4 * this.GapWidth) + this.SegmentHeight + this.Padding.Top;
      double width = horizontalOffset + (2 * this.GapWidth) + this.SegmentWidth +
                  (this.SegmentHeight / 2) + this.Padding.Left;
      return new Size(width, height);
    }

    protected override void DrawSegments(Rect bounds)
    {
      this.SegmentPanel.Children.Clear();

      double horizontalOffset = GetHorizontalOffset();
      bounds.Offset(horizontalOffset + this.Padding.Left, this.Padding.Top);
      double coercedVerticalSegmentHeight = Math.Cos(this.SegmentTiltAngle.ToRadians()) * this.SegmentWidth;

      var verticalTopLeftSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 5,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(verticalTopLeftSegment);
      Canvas.SetLeft(verticalTopLeftSegment, bounds.X);
      Canvas.SetTop(verticalTopLeftSegment, bounds.Y + this.GapWidth);

      var verticalTopRightSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 1,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(verticalTopRightSegment);
      Canvas.SetLeft(verticalTopRightSegment, bounds.X + this.SegmentWidth + (2 * this.GapWidth));
      Canvas.SetTop(verticalTopRightSegment, bounds.Y + this.GapWidth);

      var verticalBottomRightSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 2,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(verticalBottomRightSegment);
      Canvas.SetLeft(verticalBottomRightSegment, bounds.X + this.SegmentWidth + (2 * this.GapWidth) - this.SegmentHeight);
      Canvas.SetTop(verticalBottomRightSegment, bounds.Y + coercedVerticalSegmentHeight + (3 * this.GapWidth));

      var verticalBottomLeftSegment = new VerticalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 4,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(verticalBottomLeftSegment);
      Canvas.SetLeft(verticalBottomLeftSegment, bounds.X - this.SegmentHeight);
      Canvas.SetTop(verticalBottomLeftSegment, bounds.Y + coercedVerticalSegmentHeight + (3 * this.GapWidth));

      var horizontalTopSegment = new HorizontalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 0,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(horizontalTopSegment);
      Canvas.SetLeft(horizontalTopSegment, bounds.X + this.GapWidth);
      Canvas.SetTop(horizontalTopSegment, bounds.Y);

      var horizontalCenterSegment = new HorizontalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 6,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(horizontalCenterSegment);
      Canvas.SetLeft(horizontalCenterSegment, bounds.X - this.SegmentHeight + this.GapWidth);
      Canvas.SetTop(horizontalCenterSegment, bounds.Y + coercedVerticalSegmentHeight + (2 * this.GapWidth));

      var horizontalBottomSegment = new HorizontalDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 3,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor
      };
      _ = this.SegmentPanel.Children.Add(horizontalBottomSegment);
      Canvas.SetLeft(horizontalBottomSegment, bounds.X + this.GapWidth - horizontalOffset + (this.SegmentHeight / 2));
      Canvas.SetTop(horizontalBottomSegment, bounds.Y + (2 * coercedVerticalSegmentHeight) + (4 * this.GapWidth));
    }
  }
}