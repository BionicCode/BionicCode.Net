#region Info

// 2021/02/04  00:47
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Windows;
using System.Windows.Media;
using BionicCode.Utilities.Net;

namespace BionicCode.Controls.Net.Wpf
{
  public class HorizontalDigitSegment : DigitSegment
  {
    protected override Geometry CreateGeometry()
    {
      double fullAngle = 90 + this.TiltAngle;
      double miter = 90 - (fullAngle / 2);
      double tipLength = this.Bounds.Height / 2 * Math.Tan(miter.ToRadians());
      var pathSegments = new PathSegmentCollection()
      {
        new LineSegment(new Point(this.Bounds.X + tipLength, this.Bounds.Y), false),
        new LineSegment(new Point(this.Bounds.X + this.Bounds.Width - tipLength, this.Bounds.Y), false),
        new LineSegment(new Point(this.Bounds.X + this.Bounds.Width, this.Bounds.Y + (this.Bounds.Height / 2)), false),
        new LineSegment(new Point(this.Bounds.X + this.Bounds.Width - tipLength, this.Bounds.Y + this.Bounds.Height), false),
        new LineSegment(new Point(this.Bounds.X + tipLength, this.Bounds.Y + this.Bounds.Height), false),
        new LineSegment(new Point(this.Bounds.X, this.Bounds.Y + (this.Bounds.Height / 2)), false)
      };
      var pathFigure = new PathFigure(new Point(this.Bounds.X, this.Bounds.Y + (this.Bounds.Height / 2)), pathSegments, true);
      return new PathGeometry(new[] { pathFigure });
    }
  }
}