namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;

  internal class TextLine
  {
    public TextLine(int lineIndex, int doucmentIndex, double verticalOffset, string text, IEnumerable<TextRange> textRanges)
    {
      this.Text = text;
      this.VerticalOffset = verticalOffset;
      this.TextRanges = new TextRangeCollection(textRanges);
      this.Index = lineIndex;
      this.DoucmentIndex = doucmentIndex;
      this.RangeInDocument = this.DoucmentIndex..this.Text.Length;
      this.TextInfo = new TextInfo(this.Text, this.TextRanges);
    }

    public void RemoveTextRange(Range range) => this.Text = string.Concat(this.Text[..range.Start.Value], this.Text[range.End.Value..]);

    public string Text { get; private set; }
    public TextRangeCollection TextRanges { get; }
    public int Index { get; }
    public int DoucmentIndex { get; }
    public Range RangeInDocument { get; }
    public double VerticalOffset { get; }
    public TextInfo TextInfo { get; }
  }
}
