#region Info

// 2021/02/04  22:57
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using BionicCode.Utilities.Net;

namespace BionicCode.Controls.Net.Wpf
{
  [TemplatePart(Name = "PART_HostPanel", Type = typeof(Panel))]
  public class SeparatorDisplayDigit : DisplayDigit
  {

    protected override Size GetNaturalSize()
    {
      double height = this.Padding.Top + (this.SegmentHeight / 2) + this.SegmentWidth + (2 * this.GapWidth);
      var width = this.Padding.Left + (Math.Tan(this.SegmentTiltAngle.ToRadians()) * this.SegmentWidth);
      return new Size(width, height);
    }

    protected override void DrawSegments(Rect bounds)
    {
      this.SegmentPanel.Children.Clear();

      var verticalBottomRightSegment = new SeparatorDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 2,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor,
        IsOn = true
      };
      this.SegmentPanel.Children.Add(verticalBottomRightSegment);
      Canvas.SetLeft(verticalBottomRightSegment, bounds.X);
      Canvas.SetTop(verticalBottomRightSegment, bounds.Y + (this.SegmentHeight / 2) + (this.SegmentWidth / 2) + (2 * this.GapWidth));

      var horizontalBottomSegment = new SeparatorDigitSegment()
      {
        TiltAngle = this.SegmentTiltAngle,
        Height = this.SegmentHeight,
        Width = this.SegmentWidth,
        Index = 3,
        OnColor = this.SegmentOnColor,
        OffColor = this.SegmentOffColor,
        IsOn = true
      };
      this.SegmentPanel.Children.Add(horizontalBottomSegment);
      Canvas.SetLeft(horizontalBottomSegment, bounds.X - (Math.Tan(this.SegmentTiltAngle.ToRadians()) * this.SegmentWidth));
      Canvas.SetTop(horizontalBottomSegment, bounds.Y + (this.SegmentHeight / 2) + (this.SegmentWidth / 2) + this.SegmentWidth + (2 * this.GapWidth));
    }
  }
}