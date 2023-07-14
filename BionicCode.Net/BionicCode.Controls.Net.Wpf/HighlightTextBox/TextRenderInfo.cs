namespace BionicCode.Controls.Net.Wpf
{
  using System.Collections.Generic;
  using System.Windows.Media;

  internal class TextRenderInfo
  {
    public TextRenderInfo(IEnumerable<HighlightBackgroundInfo> highlightBackgroundInfos, FormattedText formattedText)
    {
      this.HighlightBackgroundInfos = highlightBackgroundInfos;
      this.FormattedText = formattedText;
    }

    public IEnumerable<HighlightBackgroundInfo> HighlightBackgroundInfos { get; }
    public FormattedText FormattedText { get; }
  }
}
