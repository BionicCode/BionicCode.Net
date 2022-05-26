#region Info

// 2020/07/06  13:03
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.TextFormatting;

namespace BionicCode.Controls.Net.Wpf.BionicCharts
{
  public static class ChartExtensionMethods
  {
    /// <summary>
    /// Converts a <see cref="ICartesianChartPoint"/> to a <see cref="Point"/>.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="ICartesianChartPoint"/>.</param>
    /// <returns>An instance of <see cref="Point"/></returns>
    public static Point ToPoint(this ICartesianChartPoint chartPoint) => new Point(chartPoint.X, chartPoint.Y);

    /// <summary>
    /// Converts a <see cref="Point"/> to a <see cref="CartesianChartPoint"/>.
    /// </summary>
    /// <param name="point">An instance of <see cref="Point"/>.</param>
    /// <returns>An instance of <see cref="CartesianChartPoint"/></returns>
    public static CartesianChartPoint ToCartesianChartPoint(this Point point) => new CartesianChartPoint(point.X, point.Y);

    /// <summary>
    ///  Returns the position of an element that was positioned using the attached properties <see cref="Canvas.SetLeft"/>, <see cref="Canvas.SetTop"/>, <see cref="Canvas.SetRight"/> or <see cref="Canvas.SetBottom"/>.
    /// </summary>
    /// <param name="uiElement">The positioned element.</param>
    /// <param name="canvasSize">The size the positions are relative to. Required to calculate positions set using <see cref="Canvas.SetRight"/> and <see cref="Canvas.SetBottom"/>.</param>
    /// <returns>The point that describes the position or <see cref="double.NaN"/> for each unset coordinate.</returns>
    public static Point GetCanvasPosition(this UIElement uiElement, Size canvasSize)
    {
      double x = Canvas.GetLeft(uiElement);
      if (double.IsNaN(x))
      {
        x = canvasSize.Width - uiElement.DesiredSize.Width - Canvas.GetRight(uiElement);
      }
      double y = Canvas.GetTop(uiElement);
      if (double.IsNaN(x))
      {
        y = canvasSize.Height - uiElement.DesiredSize.Height - Canvas.GetBottom(uiElement);
      }
      return new Point(x, y);
    }

    /// <summary>
    /// Deconstructs a <see cref="Point"/> to <paramref name="x"/> and <paramref name="y"/> variables.
    /// </summary>
    /// <param name="point">The point to deconstruct.</param>
    /// <param name="x">The target variable of the <see cref="Point.X"/> value.</param>
    /// <param name="y">The target variable of the <see cref="Point.Y"/> value.</param>
    public static void Deconstruct(this Point point, out double x, out double y)
    {
      x = point.X;
      y = point.Y;
    }

    /// <summary>
    /// Deconstructs a <see cref="ICartesianChartPoint"/> to <paramref name="x"/> and <paramref name="y"/> variables.
    /// </summary>
    /// <param name="chartPoint">The chart point to deconstruct.</param>
    /// <param name="x">The target variable of the <see cref="ICartesianChartPoint.X"/> value.</param>
    /// <param name="y">The target variable of the <see cref="ICartesianChartPoint.Y"/> value.</param>
    public static void Deconstruct(this ICartesianChartPoint chartPoint, out double x, out double y)
    {
      x = chartPoint.X;
      y = chartPoint.Y;
    }

    /// <summary>
    /// Returns a <see cref="Point"/> with zoom applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="Point"/></param>
    /// <param name="xZoomFactor">The zoom factor for the <see cref="Point.X"/> value.</param>
    /// <param name="yZoomFactor">The zoom factor for the <see cref="Point.Y"/> value.</param>
    /// <returns>Returns a <see cref="Point"/> with zoom applied to the coordinate values <see cref="Point.X"/> and <see cref="Point.Y"/>.</returns>
    public static Point Zoom(this Point chartPoint, double xZoomFactor, double yZoomFactor) => new Point(chartPoint.X * xZoomFactor, chartPoint.Y * yZoomFactor);

    /// <summary>
    /// Returns a <see cref="Point"/> with zoom applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="Point"/></param>
    /// <param name="zoomFactor">The zoom factor for both the <see cref="Point.X"/> and <see cref="Point.Y"/> values.</param>
    /// <returns>Returns a <see cref="Point"/> with zoom applied to the coordinate values <see cref="Point.X"/> and <see cref="Point.Y"/>.</returns>
    public static Point Zoom(this Point chartPoint, double zoomFactor) => chartPoint.Zoom(zoomFactor, zoomFactor);

    /// <summary>
    /// Returns a <see cref="Point"/> with zoom applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="ICartesianChartPoint"/></param>
    /// <param name="xZoomFactor">The zoom factor for the <see cref="ICartesianChartPoint.X"/> value.</param>
    /// <param name="yZoomFactor">The zoom factor for the <see cref="ICartesianChartPoint.Y"/> value.</param>
    /// <returns>Returns a <see cref="Point"/> with zoom applied to the coordinate values <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/>.</returns>
    public static Point Zoom(this ICartesianChartPoint chartPoint, double xZoomFactor, double yZoomFactor) => new Point(chartPoint.X * xZoomFactor, chartPoint.Y * yZoomFactor);

    /// <summary>
    /// Returns a <see cref="Point"/> with zoom applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="ICartesianChartPoint"/></param>
    /// <param name="zoomFactor">The zoom factor for both the <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/> values.</param>
    /// <returns>Returns a <see cref="Point"/> with zoom applied to the coordinate values <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/>.</returns>
    public static Point Zoom(this ICartesianChartPoint chartPoint, double zoomFactor) => chartPoint.Zoom(zoomFactor, zoomFactor);

    /// <summary>
    /// Returns a <see cref="Point"/> with ratio applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="Point"/></param>
    /// <param name="xRatio">The ratio for the <see cref="Point.X"/> value.</param>
    /// <param name="yRatio">The ratio for the <see cref="Point.Y"/> value.</param>
    /// <returns>Returns a <see cref="Point"/> with ratio applied to the coordinate values <see cref="Point.X"/> and <see cref="Point.Y"/>.</returns>
    public static Point Scale(this Point chartPoint, double xRatio, double yRatio) => new Point(chartPoint.X * xRatio, chartPoint.Y * yRatio);

    /// <summary>
    /// Returns a <see cref="Point"/> with ratio applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="Point"/></param>
    /// <param name="ratio">The ratio for both the <see cref="Point.X"/> and <see cref="Point.Y"/> values.</param>
    /// <returns>Returns a <see cref="Point"/> with ratio applied to the coordinate values <see cref="Point.X"/> and <see cref="Point.Y"/>.</returns>
    public static Point Scale(this Point chartPoint, double ratio) => chartPoint.Scale(ratio, ratio);

    /// <summary>
    /// Returns a <see cref="Point"/> with ratio applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="ICartesianChartPoint"/></param>
    /// <param name="xRatio">The ratio for the <see cref="ICartesianChartPoint.X"/> value.</param>
    /// <param name="yRatio">The ratio for the <see cref="ICartesianChartPoint.Y"/> value.</param>
    /// <returns>Returns a <see cref="Point"/> with ratio applied to the coordinate values <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/>.</returns>
    public static Point Scale(this ICartesianChartPoint chartPoint, double xRatio, double yRatio) => new Point(chartPoint.X * xRatio, chartPoint.Y * yRatio);

    /// <summary>
    /// Returns a <see cref="Point"/> with ratio applied to the coordinate.
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="ICartesianChartPoint"/>.</param>
    /// <param name="ratio">The ratio for both the <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/> values.</param>
    /// <returns>Returns a <see cref="Point"/> with ratio applied to the coordinate values <see cref="ICartesianChartPoint.X"/> and <see cref="ICartesianChartPoint.Y"/>.</returns>
    public static Point Scale(this ICartesianChartPoint chartPoint, double ratio) => chartPoint.Scale(ratio, ratio);

    /// <summary>
    /// Transforms the point from a cartesian coordinate system to a point in a screen coordinate system (which has its y-axis inverted, starting at the top-left position).
    /// </summary>
    /// <param name="point">An instance of <see cref="Point"/>.</param>
    /// <param name="yAxisPositiveLimit">The maximum value on the cartesian y-axis.</param>
    /// <returns>A point which has its values transformed from a cartesian coordinate system to a point in a screen coordinate system.</returns>
    public static Point ToPointOnScreen(this Point point, double yAxisPositiveLimit) => new Point(point.X, yAxisPositiveLimit - point.Y);

    /// <summary>
    /// Transforms the point from a cartesian coordinate system to a point in a screen coordinate system (which has its y-axis inverted, starting at the top-left position).
    /// </summary>
    /// <param name="chartPoint">An instance of <see cref="ICartesianChartPoint"/>.</param>
    /// <param name="yAxisPositiveLimit">The maximum value on the cartesian y-axis.</param>
    /// <returns>A point which has its values transformed from a cartesian coordinate system to a point in a screen coordinate system.</returns>
    public static Point ToPointOnScreen(this ICartesianChartPoint chartPoint, double yAxisPositiveLimit) => new Point(chartPoint.X, yAxisPositiveLimit - chartPoint.Y);
  }
}