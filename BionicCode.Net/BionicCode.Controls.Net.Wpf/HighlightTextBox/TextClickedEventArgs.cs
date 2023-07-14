namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Windows;

  internal class TextClickedEventArgs : EventArgs
  { 
    public Point ClickPosition { get; }
    public TextRenderInfo TextRenderInfo { get; }

    public TextClickedEventArgs(Point clickPosition, TextRenderInfo textRenderInfo)
    {
      this.ClickPosition = clickPosition;
      this.TextRenderInfo = textRenderInfo;
    }
  }
}
