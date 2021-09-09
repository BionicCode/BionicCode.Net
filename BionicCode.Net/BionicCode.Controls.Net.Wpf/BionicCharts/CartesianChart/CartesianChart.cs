using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BionicCode.Controls.Net.Wpf.BionicCharts.CartesianChart
{
  public class CartesianChart : Selector
  {
    #region Depedency properties

    public static readonly DependencyProperty IsItemsSourceSortedProperty = DependencyProperty.Register(
      "IsItemsSourceSorted",
      typeof(bool),
      typeof(CartesianChart),
      new PropertyMetadata(default(bool)));

    /// <summary>
    /// To will improve performance, set this property to <c>true</c>, if the item source is presorted by their x-values. 
    /// </summary>
    public bool IsItemsSourceSorted
    {
      get => (bool) GetValue(CartesianChart.IsItemsSourceSortedProperty);
      set => SetValue(CartesianChart.IsItemsSourceSortedProperty, value);
    }

    public static readonly DependencyProperty MaxYValueProperty = DependencyProperty.Register(
      "MaxYValue",
      typeof(double
      ),
      typeof(CartesianChart),
      new PropertyMetadata(double.NaN));

    public double
      MaxYValue
    {
      get => (double
        ) GetValue(CartesianChart.MaxYValueProperty);
      set => SetValue(CartesianChart.MaxYValueProperty, value);
    }

    public static readonly DependencyProperty MaxXValueProperty = DependencyProperty.Register(
      "MaxXValue",
      typeof(double),
      typeof(CartesianChart),
      new PropertyMetadata(double.NaN));

    public double MaxXValue
    {
      get => (double) GetValue(CartesianChart.MaxXValueProperty);
      set => SetValue(CartesianChart.MaxXValueProperty, value);
    }

    public static readonly DependencyProperty MinXValueProperty = DependencyProperty.Register(
      "MinXValue",
      typeof(double),
      typeof(CartesianChart),
      new PropertyMetadata(double.NaN));

    public double MinXValue
    {
      get => (double) GetValue(CartesianChart.MinXValueProperty);
      set => SetValue(CartesianChart.MinXValueProperty, value);
    }

    public static readonly DependencyProperty MinYValueProperty = DependencyProperty.Register(
      "MinYValue",
      typeof(double),
      typeof(CartesianChart),
      new PropertyMetadata(double.NaN));

    public double MinYValue
    {
      get => (double) GetValue(CartesianChart.MinYValueProperty);
      set => SetValue(CartesianChart.MinYValueProperty, value);
    }

    /// <summary>
    /// Sets
    /// </summary>
    public static readonly DependencyProperty IsAutoScaleEnabledProperty = DependencyProperty.Register(
      "IsAutoScaleEnabled",
      typeof(bool),
      typeof(CartesianChart),
      new PropertyMetadata(true));

    /// <summary>
    /// When set to <c>true</c> the chart's min and max values of the axis are calculated based on the chart points.
    /// </summary>
    public bool IsAutoScaleEnabled
    {
      get => (bool) GetValue(CartesianChart.IsAutoScaleEnabledProperty);
      set => SetValue(CartesianChart.IsAutoScaleEnabledProperty, value);
    }

    #endregion Dependency properties

    internal Dictionary<object, (Point Minima, Point Extrema)> SeriesExtremeValues { get; }
    private double MinXValueInternal { get; set; }
    private double MaxXValueInternal { get; set; }
    private double MinYValueInternal { get; set; }
    private double MaxYValueInternal { get; set; }

    static CartesianChart()
    {
      FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(CartesianChart),
        new FrameworkPropertyMetadata(typeof(CartesianChart)));
    }

    public CartesianChart()
    {
      this.SeriesExtremeValues = new Dictionary<object, (Point Minima, Point Extrema)>();
    }

    #region Overrides of ItemsControl

    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new CartesianChartItem();

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is CartesianChartItem;

    #region Overrides of Selector

    /// <inheritdoc />
    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
      base.OnItemsChanged(e);
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Reset:
          if (this.IsAutoScaleEnabled)
          {
            CalculateMinMaxChartValues(Items);
          }

          break;
        case NotifyCollectionChangedAction.Add:
          if (this.IsAutoScaleEnabled)
          {
            CalculateMinMaxChartValues(e.NewItems);
          }

          break;
        case NotifyCollectionChangedAction.Remove:
        {
          // Do  nothing if removed items don't affect min max coordinate values
          IEnumerable<ICartesianChartPoint> cartesianChartPoints = e.NewItems.OfType<ICartesianChartPoint>();
          IEnumerable<Point> points = e.NewItems.OfType<Point>();
          if (cartesianChartPoints.All(
            chartPoint => chartPoint.X < this.MaxXValue && chartPoint.Y < this.MaxYValue &&
                          chartPoint.X > this.MinXValue && chartPoint.Y > this.MinYValue) && points.All(
            chartPoint => chartPoint.X < this.MaxXValue && chartPoint.Y < this.MaxYValue &&
                          chartPoint.X > this.MinXValue && chartPoint.Y > this.MinYValue))
          {
            return;
          }

          if (this.IsAutoScaleEnabled)
          {
            CalculateMinMaxChartValues(e.NewItems);
          }

          break;
        }
      }
    }

    #endregion

    #endregion Overrides of ItemsControl

    #region Overrides of Selector

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      base.PrepareContainerForItemOverride(element, item);
      if (!(element is CartesianChartItem chartPointContainer))
      {
        return;
      }

      switch (item)
      {
        case ICartesianChartPoint chartPoint:
          if (chartPoint.SeriesId == null || !this.SeriesExtremeValues.TryGetValue(
            chartPoint.SeriesId,
            out (Point Minima, Point Maxima) extrema))
          {
            extrema = (
              new Point(
                this.IsAutoScaleEnabled ? this.MinXValueInternal : this.MinXValue,
                this.IsAutoScaleEnabled ? this.MinYValueInternal : this.MinYValue),
              new Point(
                this.IsAutoScaleEnabled ? this.MaxXValueInternal : this.MaxXValue,
                this.IsAutoScaleEnabled ? this.MaxYValueInternal : this.MaxYValue));
          }

          chartPointContainer.X = chartPoint.X;
          chartPointContainer.Y = chartPoint.Y;
          chartPointContainer.MaxX = extrema.Maxima.X;
          chartPointContainer.MaxY = extrema.Maxima.Y;
          chartPointContainer.MinX = extrema.Minima.X;
          chartPointContainer.MinY = extrema.Minima.Y;
          break;
        case Point chartPoint:
          chartPointContainer.X = chartPoint.X;
          chartPointContainer.Y = chartPoint.Y;
          chartPointContainer.MaxX = this.IsAutoScaleEnabled ? this.MaxXValueInternal : this.MaxXValue;
          chartPointContainer.MaxY = this.IsAutoScaleEnabled ? this.MaxYValueInternal : this.MaxYValue;
          chartPointContainer.MinX = this.IsAutoScaleEnabled ? this.MinXValueInternal : this.MinXValue;
          chartPointContainer.MinY = this.IsAutoScaleEnabled ? this.MinYValueInternal : this.MinYValue;
          break;
      }
    }

    #endregion Overrides of Selector

    private void CalculateMinMaxChartValues(IList input)
    {
      IEnumerable<ICartesianChartPoint> cartesianChartPoints = input.OfType<ICartesianChartPoint>();
      IEnumerable<Point> points = input.OfType<Point>();

      foreach (ICartesianChartPoint cartesianChartPoint in cartesianChartPoints)
      {
        if (cartesianChartPoint.SeriesId != null)
        {
          if (!this.SeriesExtremeValues.TryGetValue(
            cartesianChartPoint.SeriesId,
            out (Point Minima, Point Maxima) extremeValues))
          {
            extremeValues = (new Point(0, 0), new Point(0, 0));
            this.SeriesExtremeValues.Add(cartesianChartPoint.SeriesId, extremeValues);
          }

          extremeValues.Maxima.Y = Math.Max(extremeValues.Maxima.Y, cartesianChartPoint.Y);
          extremeValues.Minima.Y = Math.Min(extremeValues.Minima.Y, cartesianChartPoint.Y);
          extremeValues.Maxima.X = Math.Max(extremeValues.Maxima.X, cartesianChartPoint.X);
          extremeValues.Minima.X = Math.Min(extremeValues.Minima.X, cartesianChartPoint.X);

          this.MaxXValueInternal = Math.Max(this.MaxXValueInternal, extremeValues.Maxima.X);
          this.MaxYValueInternal = Math.Max(this.MaxYValueInternal, extremeValues.Maxima.Y);
          this.MinXValueInternal = Math.Min(this.MinXValueInternal, extremeValues.Minima.X);
          this.MinYValueInternal = Math.Min(this.MinYValueInternal, extremeValues.Minima.Y);
        }
        else
        {
          this.MaxXValueInternal = Math.Max(this.MaxXValueInternal, cartesianChartPoint.X);
          this.MaxYValueInternal = Math.Max(this.MaxYValueInternal, cartesianChartPoint.Y);
          this.MinXValueInternal = Math.Min(this.MinXValueInternal, cartesianChartPoint.X);
          this.MinYValueInternal = Math.Min(this.MinYValueInternal, cartesianChartPoint.Y);
        }
      }

      foreach (Point point in points)
      {
        this.MaxXValueInternal = Math.Max(this.MaxXValueInternal, point.X);
        this.MaxYValueInternal = Math.Max(this.MaxYValueInternal, point.Y);
        this.MinXValueInternal = Math.Min(this.MinXValueInternal, point.X);
        this.MinYValueInternal = Math.Min(this.MinYValueInternal, point.Y);
      }
    }

    internal (Point Minima, Point Maxima) GetExtremePointsOf(ICartesianChartPoint cartesianChartPoint)
    {
      if (cartesianChartPoint.SeriesId != null && this.SeriesExtremeValues.TryGetValue(
        cartesianChartPoint.SeriesId,
        out (Point Minima, Point Maxima) extrema))
      {
        return extrema;
      }

      return GetExtremePointsOf(cartesianChartPoint.ToPoint());
    }

    internal (Point Minima, Point Maxima) GetExtremePointsOf(Point chartPoint) =>
    (
      new Point(
        this.IsAutoScaleEnabled ? this.MinXValueInternal : this.MinXValue,
        this.IsAutoScaleEnabled ? this.MinYValueInternal : this.MinYValue),
      new Point(
        this.IsAutoScaleEnabled ? this.MaxXValueInternal : this.MaxXValue,
        this.IsAutoScaleEnabled ? this.MaxYValueInternal : this.MaxYValue));

    internal (Point ChartPoint, Point Minima, Point Maxima) ConvertItemToPointInfo(object item)
    {
      if (item == null)
      {
        throw new NullReferenceException("Conversion source item is not initialized");
      }
      Point chartPoint;
      Point chartPointMaxima;
      Point chartPointMinima;
      if (item is ICartesianChartPoint cartesianChartPoint)
      {
        chartPoint = cartesianChartPoint.ToPoint();
        (Point Minima, Point Maxima) extremePoints = GetExtremePointsOf(cartesianChartPoint);
        chartPointMaxima = extremePoints.Maxima;
        chartPointMinima = extremePoints.Minima;
      }
      else if (item is Point point)
      {
        chartPoint = point;
        (Point Minima, Point Maxima) extremePoints = GetExtremePointsOf(point);
        chartPointMaxima = extremePoints.Maxima;
        chartPointMinima = extremePoints.Minima;
      }
      else if (item is CartesianChartItem itemContainer)
      {
        chartPoint = itemContainer.ToPoint();
        chartPointMaxima = itemContainer.ToExtremeMaxPoint();
        chartPointMinima = itemContainer.ToExtremeMinPoint();
      }
      else
      {
        throw new InvalidCastException("Item type must be of type 'Point', 'CartesianChartItem' or implement ' ICartesianChartPoint.'");
      }

      return (chartPoint, chartPointMinima, chartPointMaxima);
    }
  }
}
