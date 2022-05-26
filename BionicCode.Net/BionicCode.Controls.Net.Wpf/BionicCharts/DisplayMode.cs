/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
namespace BionicCode.Controls.Net.Wpf.BionicCharts
After:
namespace BionicCode.Controls.Net.Wpf.BionicCharts
*/

namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  [System.Flags]
  public enum DisplayMode
  {
    Default = 0,
    FitVerticalAxisToScreen = 1,
    FitHorizontalAxisToScreen = 2,
    LeftAligned = 4,
    RightAligned = 8,
    TopAligned = 16,
    BottomAligned = 32,
    LeftTopAligned = LeftAligned | TopAligned,
    RightTopAligned = RightAligned | TopAligned,
    LeftBottomAligned = LeftAligned | BottomAligned,
    RightBottomAligned = RightAligned | BottomAligned,
    FitToScreen = FitHorizontalAxisToScreen | FitVerticalAxisToScreen,
    Centered = 64
  }
}
