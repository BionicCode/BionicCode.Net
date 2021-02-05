#region Info

// 2021/02/04  18:02
// BionicCode.Controls.Net.Wpf

#endregion

using System.Collections;

namespace BionicCode.Controls.Net.Wpf
{
  public interface ISevenSegmentDigit
  {
    void ToggleSegments(BitArray word);
    int DisplayIndex { get; }
  }
}