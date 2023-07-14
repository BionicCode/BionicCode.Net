namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows.Media;

  public class NewLine : TextRange
  {
    public NewLine(int begin) : this(begin, Brushes.Transparent, Brushes.Transparent, Brushes.Transparent, 0, string.Empty)
    {
    }

    public NewLine(int begin, Brush background, Brush foreground, string text) : base(begin, background, foreground, text)
    {
    }

    public NewLine(int begin, Brush background, Brush foreground, Brush borderBrush, double borderThickness, string text) : base(begin, background, foreground, borderBrush, borderThickness, text)
    {
    }
  }
}
