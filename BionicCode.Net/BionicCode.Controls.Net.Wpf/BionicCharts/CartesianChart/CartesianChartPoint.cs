namespace BionicCode.Controls.Net.Wpf
{
  using BionicCode.Utilities.Net;

  public class CartesianChartPoint : ViewModel, ICartesianChartPoint
  {
    /// <summary>
    /// Default constructor. Creates an instance which is initialized with <see cref="X"/> = 0.0 and <see cref="Y"/> = 0.0.
    /// </summary>
    public CartesianChartPoint() : this(0.0, 0.0)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="x">The value for <see cref="X"/>.</param>
    /// <param name="y">The value for <see cref="Y"/>.</param>
    public CartesianChartPoint(double x, double y)
    {
      this.X = x;
      this.Y = y;
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString() => $"({this.X}, {this.Y})";

    #endregion

    #region Implementation of ICartesianChartPoint

    /// <summary>
    /// Returns a shallow copy with zoom applied to the coordinates.
    /// </summary>
    /// <param name="xZoomFactor">The zoom factor for the <see cref="ICartesianChartPoint.X"/> value.</param>
    /// <param name="yZoomFactor">The zoom factor for the <see cref="ICartesianChartPoint.Y"/> value.</param>
    /// <returns>Returns a shallow copy with zoom applied to the coordinates <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/>.</returns>
    /// <remarks>This method will not mutate the original instance, but only the returned shallow copy.</remarks>
    public ICartesianChartPoint Zoom(double xZoomFactor, double yZoomFactor)
    {
      var clone = MemberwiseClone() as ICartesianChartPoint;
      clone.X *= xZoomFactor;
      clone.Y *= yZoomFactor;
      return clone;
    }

    private double x;
    /// <inheritdoc />
    public double X { get => this.x; set => TrySetValue(value, ref this.x); }

    private double y;
    /// <inheritdoc />
    public double Y { get => this.y; set => TrySetValue(value, ref this.y); }

    private string summary;
    /// <inheritdoc />
    public string Summary
    {
      get => this.summary;
      set => TrySetValue(value, ref this.summary);
    }

    private object data;
    /// <inheritdoc />
    public object Data
    {
      get => this.data;
      set => TrySetValue(value, ref this.data);
    }

    private object seriesId;
    /// <inheritdoc />
    public object SeriesId
    {
      get => this.seriesId;
      set => TrySetValue(value, ref this.seriesId);
    }

    #endregion Implementation of ICartesianChartPoint

    #region Equality members

    /// <inheritdoc />
    public int CompareTo(ICartesianChartPoint other) => this.X.CompareTo(other.X);

    /// <inheritdoc />
    public bool Equals(ICartesianChartPoint other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }

      if (ReferenceEquals(this, other))
      {
        return true;
      }

      return this.X.Equals(other.X) && this.Y.Equals(other.Y);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
      {
        return false;
      }

      if (ReferenceEquals(this, obj))
      {
        return true;
      }

      if (obj.GetType() != GetType())
      {
        return false;
      }

      return Equals((ICartesianChartPoint)obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
      unchecked
      {
        return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
      }
    }

    public static bool operator ==(CartesianChartPoint left, CartesianChartPoint right) => Equals(left, right);

    public static bool operator !=(CartesianChartPoint left, CartesianChartPoint right) => !Equals(left, right);

    #endregion
  }
}
