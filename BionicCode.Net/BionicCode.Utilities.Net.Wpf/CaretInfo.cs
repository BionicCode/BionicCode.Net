namespace BionicCode.Utilities.Net
{
#if !NETSTANDARD
  using System.Windows;

  /// <summary>
  /// Struct to store caret information.
  /// </summary>
  public readonly struct CaretInfo
  {
    /// <summary>
    /// Defines the properties of a caret using <see cref="SystemParameters.CaretWidth"/>.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="height"></param>
    /// <param name="offset"></param>
    public CaretInfo(System.Windows.Point position, double height, System.Windows.Point offset) : this(position, SystemParameters.CaretWidth, height, offset)
    {
    }

    public CaretInfo(System.Windows.Point position, double width, double height, System.Windows.Point offset)
    {
      this.Position = position;
      this.Width = width;
      this.Height = height;
      this.Offset = offset;
    }

    public System.Windows.Point Position { get; }
    public double Width { get; }
    public double Height { get; }
    public System.Windows.Point Offset { get; }
  }
#endif
}