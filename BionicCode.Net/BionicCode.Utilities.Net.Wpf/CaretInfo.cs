namespace BionicCode.Utilities.Net
{
#if NET || NET461_OR_GREATER
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
    public CaretInfo(CartesianPoint position, double height, CartesianPoint offset) : this(position, SystemParameters.CaretWidth, height, offset)
    {
    }

    public CaretInfo(CartesianPoint position, double width, double height, CartesianPoint offset)
    {
      this.Position = position;
      this.Width = width;
      this.Height = height;
      this.Offset = offset;
    }

    public CartesianPoint Position { get; }
    public double Width { get; }
    public double Height { get; }
    public CartesianPoint Offset { get; }
  }
#endif
}