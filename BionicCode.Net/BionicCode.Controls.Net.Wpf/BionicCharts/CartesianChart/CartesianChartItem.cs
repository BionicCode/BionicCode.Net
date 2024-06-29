namespace BionicCode.Controls.Net.Wpf
{
  using System.Windows;

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
  using System.Windows.Controls;
  After:
  using System.Windows.Controls;
    */
  using System.Windows.Controls;

  /// <summary>
  /// The item container for the <see cref="CartesianChart"/>. 
  /// </summary>
  /// <remarks>The chart works also with every <see cref="FrameworkElement"/> but the best behavior especially performance wise can be expected using a <see cref="CartesianChartItem"/> or any derived type.</remarks>
  public class CartesianChartItem : ContentControl
  {
    #region Dependency properties

    public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(
      "IsSelected",
      typeof(bool),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(bool)));

    /// <summary>
    /// Returns whether the item is selected or not.
    /// </summary>
    public bool IsSelected { get => (bool)GetValue(IsSelectedProperty); set => SetValue(IsSelectedProperty, value); }

    public static readonly DependencyProperty XProperty = DependencyProperty.Register(
      "X",
      typeof(double),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(double)));

    /// <summary>
    /// The x value of the cartesian coordinate.
    /// </summary>
    public double X { get => (double)GetValue(XProperty); set => SetValue(XProperty, value); }

    public static readonly DependencyProperty YProperty = DependencyProperty.Register(
      "Y",
      typeof(double),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(double)));

    /// <summary>
    /// The y value of the cartesian coordinate.
    /// </summary>
    public double Y { get => (double)GetValue(YProperty); set => SetValue(YProperty, value); }

    public static readonly DependencyProperty MaxXProperty = DependencyProperty.Register(
      "MaxX",
      typeof(double),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(double)));

    /// <summary>
    /// The maximum x value of a cartesian coordinate related to a series of points.
    /// </summary>
    public double MaxX { get => (double)GetValue(MaxXProperty); set => SetValue(MaxXProperty, value); }

    public static readonly DependencyProperty MaxYProperty = DependencyProperty.Register(
      "MaxY",
      typeof(double),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(double)));

    /// <summary>
    /// The maximum y value of a cartesian coordinate related to a series of points.
    /// </summary>
    public double MaxY { get => (double)GetValue(MaxYProperty); set => SetValue(MaxYProperty, value); }

    public static readonly DependencyProperty MinXProperty = DependencyProperty.Register(
      "MinX",
      typeof(double),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(double)));

    /// <summary>
    /// The minimum x value of a cartesian coordinate related to a series of points.
    /// </summary>
    public double MinX { get => (double)GetValue(MinXProperty); set => SetValue(MinXProperty, value); }

    public static readonly DependencyProperty MinYProperty = DependencyProperty.Register(
      "MinY",
      typeof(double),
      typeof(CartesianChartItem),
      new PropertyMetadata(default(double)));

    /// <summary>
    /// The minimum y value of a cartesian coordinate related to a series of points.
    /// </summary>
    public double MinY { get => (double)GetValue(MinYProperty); set => SetValue(MinYProperty, value); }

    #endregion Dependency properties

    static CartesianChartItem() => DefaultStyleKeyProperty.OverrideMetadata(typeof(CartesianChartItem), new FrameworkPropertyMetadata(typeof(CartesianChartItem)));

    /// <inheritdoc />
    public override string ToString() => $"P({this.X}, {this.Y}); Pmax({this.MaxX}, {this.MaxY}); Pmin({this.MinX}, {this.MinY})";

    /// <summary>
    /// Deconstructs the item container.
    /// </summary>
    /// <param name="x">The value of <see cref="X"/></param>
    /// <param name="y">The value of <see cref="Y"/></param>
    /// <param name="xMax">The value of <see cref="MaxX"/></param>
    /// <param name="yMax">The value of <see cref="MaxY"/></param>
    public void Deconstruct(out double x, out double y, out double xMax, out double yMax)
    {
      x = this.X;
      y = this.Y;
      xMax = this.MaxX;
      yMax = this.MaxY;
    }

    /// <summary>
    /// Deconstructs the item container.
    /// </summary>
    /// <param name="x">The value of <see cref="X"/></param>
    /// <param name="y">The value of <see cref="Y"/></param>
    /// <param name="xMax">The value of <see cref="MaxX"/></param>
    /// <param name="yMax">The value of <see cref="MaxY"/></param>
    /// <param name="xMin">The value of <see cref="MinX"/></param>
    /// <param name="yMin">The value of <see cref="MinY"/></param>
    public void Deconstruct(out double x, out double y, out double xMax, out double yMax, out double xMin, out double yMin)
    {
      x = this.X;
      y = this.Y;
      xMax = this.MaxX;
      yMax = this.MaxY;
      xMin = this.MinX;
      yMin = this.MinY;
    }

    /// <summary>
    /// Converts a <see cref="CartesianChartItem"/> to a <see cref="Point"/>.
    /// </summary>
    /// <returns>An instance of <see cref="Point"/> containing <see cref="X"/> and <see cref="Y"/></returns>
    public Point ToPoint() => new Point(this.X, this.Y);

    /// <summary>
    /// Converts a <see cref="CartesianChartItem"/> to a <see cref="Point"/>.
    /// </summary>
    /// <returns>An instance of <see cref="Point"/> containing <see cref="MaxX"/> and <see cref="MaxY"/></returns>
    public Point ToExtremeMaxPoint() => new Point(this.MaxX, this.MaxY);

    /// <summary>
    /// Converts a <see cref="CartesianChartItem"/> to a <see cref="Point"/>.
    /// </summary>
    /// <returns>An instance of <see cref="Point"/> containing <see cref="MinX"/> and <see cref="MinY"/></returns>
    public Point ToExtremeMinPoint() => new Point(this.MinX, this.MinY);
  }
}
