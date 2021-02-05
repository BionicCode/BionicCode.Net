#region Info

// 2021/02/04  22:22
// BionicCode.Controls.Net.Wpf

#endregion

using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using BionicCode.Utilities.Net.Wpf.Extensions;

namespace BionicCode.Controls.Net.Wpf
{
  public class SeparatorDigitSegment : DigitSegment
  {
    protected override Geometry CreateGeometry()
    {
      return new EllipseGeometry(this.Bounds.Location, this.Bounds.Height / 2, this.Bounds.Height  / 2);
    }
  }
}