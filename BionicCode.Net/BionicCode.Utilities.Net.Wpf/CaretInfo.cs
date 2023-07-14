namespace BionicCode.Utilities.Net
{
#if NET || NET461_OR_GREATER
  using System.Windows;

  public readonly struct CaretInfo
  {
    /// <summary>
    /// Defines the properties of a caret using <see cref="SystemParameters.CaretWidth"/>.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="height"></param>
    /// <param name="offset"></param>
    public CaretInfo(Point position, double height, Point offset) : this(position, SystemParameters.CaretWidth, height, offset)
    {
    }

    public CaretInfo(Point position, double width, double height, Point offset)
    {
      this.Position = position;
      this.Width = width;
      this.Height = height;
      this.Offset = offset;
    }

    public Point Position { get; }
    public double Width { get; }
    public double Height { get; }
    public Point Offset { get; }
  }
#endif
}