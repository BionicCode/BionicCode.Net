namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows.Media;

  internal class HighlightBackgroundInfo
  {
    public HighlightBackgroundInfo(Brush borderBrush, Brush backgroundBrush, Geometry backgroundGeometry, double borderThickness)
    {
      this.BorderBrush = borderBrush;
      this.BackgroundBrush = backgroundBrush;
      this.BackgroundGeometry = backgroundGeometry;
      this.BorderThickness = borderThickness;
    }

    public Brush BorderBrush { get; }
    public Brush BackgroundBrush { get; }
    public Geometry BackgroundGeometry { get; }
    public double BorderThickness { get; }
  }
}
