namespace BionicCode.Controls.Net.Wpf
{
  #region Info

  // 2021/02/04  00:47
  // BionicCode.Controls.Net.Wpf

  #endregion

  using System;
  using System.Windows;
  using System.Windows.Media;
  using BionicCode.Utilities.Net;

  public class VerticalDigitSegment : DigitSegment
  {
    public VerticalDigitSegment()
    {
      this.RenderTransformOrigin = new Point(0, 0.5);
      this.TiltTransform = new RotateTransform();
      var transformGroup = new TransformGroup();
      transformGroup.Children.Add(this.TiltTransform);
      this.RenderTransform = transformGroup;
    }

    protected override Geometry CreateGeometry()
    {
      double fullAngle = this.TiltAngle + 90;
      double miter = 90 - (fullAngle / 2);
      double topTipLength = this.Bounds.Height / 2 * Math.Tan(miter.ToRadians());
      double bottomTip2Length = this.Bounds.Height / 2 * Math.Tan((fullAngle - miter).ToRadians());
      var pathSegments = new PathSegmentCollection()
      {
        new LineSegment(new Point(this.Bounds.X + topTipLength, this.Bounds.Y), false),
        new LineSegment(new Point(this.Bounds.X + this.Bounds.Width -bottomTip2Length, this.Bounds.Y), false),
        new LineSegment(new Point(this.Bounds.X + this.Bounds.Width, this.Bounds.Y + (this.Bounds.Height / 2)), false),
        new LineSegment(new Point(this.Bounds.X + this.Bounds.Width - topTipLength, this.Bounds.Y + this.Bounds.Height), false),
        new LineSegment(new Point(this.Bounds.X + bottomTip2Length, this.Bounds.Y + this.Bounds.Height), false),
        new LineSegment(new Point(this.Bounds.X, this.Bounds.Y + (this.Bounds.Height / 2)), false)
      };
      var pathFigure = new PathFigure(new Point(this.Bounds.X, this.Bounds.Y + (this.Bounds.Height / 2)), pathSegments, true);
      return new PathGeometry(new[] { pathFigure });
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
      this.TiltTransform.Angle = 90 + this.TiltAngle;
      return base.ArrangeOverride(finalSize);
    }

    private RotateTransform TiltTransform { get; set; }
  }
}