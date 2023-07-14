namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Collections.Immutable;
  using System.Threading.Tasks;

  internal class TextContainer
  {
    public TextContainer(TextInfo textInfo, double lineHeight)
    {
      this.Text = textInfo.Text;
      this.TextInfo = textInfo;
      this.LineHeight = lineHeight;
    }

    public async Task<TextLine> GetTextLineAsync(int lineIndex)
    {
      if (this.TextLines is null)
      {
        _ = await GetTextLinesAsync();
      }

      return lineIndex >= 0 && lineIndex < this.LineCount
        ? this.TextLines[lineIndex]
        : throw new IndexOutOfRangeException();
    }

    public async Task<IImmutableList<TextLine>> GetTextLinesAsync()
    {
      if (this.TextLines is not null)
      {
        return this.TextLines;
      }

      string[] textLines = this.Text.Split(Environment.NewLine, StringSplitOptions.None);
      var lines = new List<TextLine>();
      int lineStartIndexInText = 0;
      for (int lineIndex = 0; lineIndex < textLines.Length; lineIndex++)
      {
        string lineText = textLines[lineIndex];
        int lineEndIndexInText = lineStartIndexInText + lineText.Length;
        double verticalOffset = lineIndex * this.LineHeight;
        IEnumerable<TextRange> textRangesInLine = await this.TextInfo.TextRanges.GetTextRangesFromRangeAsync(lineStartIndexInText..lineEndIndexInText);
        var newTextLine = new TextLine(lineIndex, lineStartIndexInText, verticalOffset, lineText, textRangesInLine);
        lines.Add(newTextLine);
        lineStartIndexInText = ++lineEndIndexInText;
      }

      this.TextLines = ImmutableList.CreateRange(lines);
      return this.TextLines;
    }

    public int LineCount => this.TextLines?.Count ?? -1;
    public string Text { get; }
    public double LineHeight { get; }

    public TextInfo TextInfo { get; }
    private IImmutableList<TextLine> TextLines { get; set; }
  }
}
