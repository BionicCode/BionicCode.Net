#region Info

// 2021/02/04  22:22
// BionicCode.Controls.Net.Wpf

#endregion

using System.Windows.Media;

namespace BionicCode.Controls.Net.Wpf
{
  public class SeparatorDigitSegment : DigitSegment
  {
    protected override Geometry CreateGeometry() => new EllipseGeometry(this.Bounds.Location, this.Bounds.Height / 2, this.Bounds.Height / 2);
  }
}