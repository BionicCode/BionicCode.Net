#region Info

// 2021/02/05  20:40
// BionicCode.Controls.Net.Wpf

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BionicCode.Controls.Net.Wpf
{
  internal class SegmentDisplayDriver
  {
    private ISevenSegmentDisplay Display { get; }

    internal SegmentDisplayDriver(ISevenSegmentDisplay display)
    {
      this.Display = display;
      this.PaddingValue = 0;
      this.SegmentCodeMap = new Dictionary<int, BitArray>()
      {
        {0, new BitArray(new[] {true, true, true, true, true, true, false})},
        {1, new BitArray(new[] {false, true, true, false, false, false, false})},
        {2, new BitArray(new[] {true, true, false, true, true, false, true})},
        {3, new BitArray(new[] {true, true, true, true, false, false, true})},
        {4, new BitArray(new[] {false, true, true, false, false, true, true})},
        {5, new BitArray(new[] {true, false, true, true, false, true, true})},
        {6, new BitArray(new[] {true, false, true, true, true, true, true})},
        {7, new BitArray(new[] {true, true, true, false, false, false, false})},
        {8, new BitArray(new[] {true, true, true, true, true, true, true})},
        {9, new BitArray(new[] {true, true, true, true, false, true, true})},
        {10, new BitArray(new[] {true, true, true, false, true, true, true})},
        {11, new BitArray(new[] {false, false, true, true, true, true, true})},
        {12, new BitArray(new[] {true, false, false, true, true, true, false})},
        {13, new BitArray(new[] {false, true, true, true, true, false, true})},
        {14, new BitArray(new[] {true, false, false, true, true, true, true})},
        {15, new BitArray(new[] {true, false, false, false, true, true, true})},
        {20, new BitArray(new[] {false, false, false, false, false, false, true})}, // '-'
      };
    }

    public void SetValue(int value)
    {
      if (!this.Display.Digits.Any())
      {
        return;
      }

      int displayDigitIndex = 0;
      foreach (BitArray digitWord in ConvertToDisplayDigits(value))
      {
        this.Display.Digits.ElementAt(displayDigitIndex++).ToggleSegments(digitWord);
        if (displayDigitIndex >= this.Display.Digits.Count)
        {
          break;
        }
      }

      if (this.IsPaddingEnabled)
      {
        while (displayDigitIndex < this.Display.Digits.Count)
        {
          this.Display.Digits.ElementAt(displayDigitIndex++).ToggleSegments(this.SegmentCodeMap[0]);
        }
      }
    }

    private IEnumerable<BitArray> ConvertToDisplayDigits(int value)
    {
      if (value == 0)
      {
        yield return this.SegmentCodeMap[0];
      }

      value = Math.Abs(value);
      for (int displayDigitIndex = 0; displayDigitIndex < this.Display.Digits.Count; displayDigitIndex++)
      {
        if (value == 0)
        {
          yield break;
        }

        var decimalDigit = value % 10;
        yield return this.SegmentCodeMap[decimalDigit];
        value /= 10;
      }
    }

    public bool IsPaddingEnabled { get; set; }
    public int PaddingValue { get; set; }
    private Dictionary<int, BitArray> SegmentCodeMap { get; }
  }
}