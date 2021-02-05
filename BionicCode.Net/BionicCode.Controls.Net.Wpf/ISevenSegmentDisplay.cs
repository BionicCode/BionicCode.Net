#region Info

// 2021/02/04  12:21
// BionicCode.Controls.Net.Wpf

#endregion

using System.Collections.Generic;

namespace BionicCode.Controls.Net.Wpf
{
  public interface ISevenSegmentDisplay
  {
    SortedSet<ISevenSegmentDigit> Digits { get; }
  }
}