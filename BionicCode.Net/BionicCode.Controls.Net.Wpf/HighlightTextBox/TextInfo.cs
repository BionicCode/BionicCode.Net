namespace BionicCode.Controls.Net.Wpf
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Windows;
  using System.Windows.Media;

  internal class TextInfo
  {
    public TextRangeCollection TextRanges { get; }
    public FlowDirection FlowDirection { get; set; }
    public FontFamily FallbackFontFamily { get; set; }
    public double FallbackFontSize { get; set; }
    public string Text { get; }
    public Brush TextBrush { get; set; }
    public double MaxAllowedTextWidth { get; set; }

    public TextInfo() : this(string.Empty, Enumerable.Empty<TextRange>())
    {
    }

    public TextInfo(string text, IEnumerable<TextRange> textRanges)
    {
      this.Text = text;
      this.TextRanges = new TextRangeCollection(textRanges);
      this.TextBrush = Brushes.Black;
      this.MaxAllowedTextWidth = double.MaxValue;
      this.FallbackFontFamily = new FontFamily("Cascadia Mono");
    }
  }
}
