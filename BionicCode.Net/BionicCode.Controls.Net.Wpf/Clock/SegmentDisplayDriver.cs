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
      this.SegmentCodeMap = new Dictionary<int, int>()
      {
        {0, 0x3F},
        {1, 0x06},
        {2, 0x5B},
        {3, 0x4F},
        {4, 0x66},
        {5, 0x6D},
        {6, 0x7D},
        {7, 0x07},
        {8, 0x7F},
        {9, 0x6F},
        {10, 0x77},
        {11, 0x7C},
        {12, 0x39},
        {13, 0x5E},
        {14, 0x79},
        {15, 0x71},
        {20, 0x40}, // '-'
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
          byte[] bytes = BitConverter.GetBytes(this.SegmentCodeMap[0]);
          var zeroDigitWord = new BitArray(bytes);
          this.Display.Digits.ElementAt(displayDigitIndex++).ToggleSegments(zeroDigitWord);
        }
      }
    }

    private IEnumerable<BitArray> ConvertToDisplayDigits(int value)
    {
      value = Math.Abs(value);
      if (value == 0)
      {
        byte[] bytes = BitConverter.GetBytes(this.SegmentCodeMap[0]);
        yield return new BitArray(bytes);
      }

      for (int displayDigitIndex = 0; displayDigitIndex < this.Display.Digits.Count; displayDigitIndex++)
      {
        if (value == 0)
        {
          yield break;
        }

        int decimalDigit = value % 10;
        byte[] bytes = BitConverter.GetBytes(this.SegmentCodeMap[decimalDigit]);

        yield return new BitArray(bytes);

        value /= 10;
      }
    }

    public bool IsPaddingEnabled { get; set; }
    public int PaddingValue { get; set; }
    private Dictionary<int, int> SegmentCodeMap { get; }
  }
}