namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Windows;

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
    using System.Windows.Controls.Primitives;
  After:
    using System.Windows.Controls.Primitives;
    using BionicCode;
    using BionicCode.Controls;
    using BionicCode.Controls.Net;
    using BionicCode.Controls.Net.Wpf;
          */

  public class CartesianChart : Chart
  {
    #region Depedency properties

    #region DataConverter

    public static readonly DependencyProperty DataConverterProperty = DependencyProperty.Register(
      "DataConverter",
      typeof(ICartesianDataConverter),
      typeof(CartesianChart),
      new PropertyMetadata(default(ICartesianDataConverter)));

    /// <summary>
    /// To will improve performance, set this property to <c>true</c>, if the item source is presorted by their x-values. 
    /// </summary>
    public ICartesianDataConverter DataConverter
    {
      get => (ICartesianDataConverter)GetValue(DataConverterProperty);
      set => SetValue(DataConverterProperty, value);
    }

    #endregion DataConverter

    #region SelectedView

    public CartesianChartViewId SelectedView
    {

/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
      get { return (CartesianChartViewId)GetValue(SelectedViewProperty); }
After:
      get => (CartesianChartViewId)GetValue(SelectedViewProperty); }
*/
      get => (CartesianChartViewId)GetValue(SelectedViewProperty);
      set => SetValue(SelectedViewProperty, value);
    }

    public static readonly DependencyProperty SelectedViewProperty =
        DependencyProperty.Register(
          "SelectedView",
          typeof(CartesianChartViewId),
          typeof(CartesianChart),
          new FrameworkPropertyMetadata(CartesianChartViewId.Line, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedViewChanged));


/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
    #endregion SelectedView

    

    /// <summary>
After:
    #endregion SelectedView



    /// <summary>
*/
    #endregion SelectedView



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
      get => (bool)GetValue(IsAutoScaleEnabledProperty);
      set => SetValue(IsAutoScaleEnabledProperty, value);
    }

    #endregion Dependency properties

    static CartesianChart()
    {
      //DefaultStyleKeyProperty.OverrideMetadata(
      //  typeof(CartesianChart),
      //  new FrameworkPropertyMetadata(typeof(CartesianChart)));
    }

    public CartesianChart() => this.DefaultSeriesSelector = new SeriesSelector();

    private static void OnSelectedViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as CartesianChart).OnSelectedViewChanged((CartesianChartViewId)e.OldValue, (CartesianChartViewId)e.NewValue);

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);
      this.ViewId = this.SelectedView;
    }

    #region Overrides of Chart

    public override SeriesSelector GetDefaultSeriesSelector() => this.DefaultSeriesSelector;
    /// <inheritdoc />
    protected override DependencyObject GetContainerForItemOverride() => new CartesianChartItem();

    /// <inheritdoc />
    protected override bool IsItemItsOwnContainerOverride(object item) => item is CartesianChartItem;

    /// <inheritdoc />
    //protected override void OnItemsViewChanged(object sender, NotifyCollectionChangedEventArgs e)
    //{
    //  base.OnItemsViewChanged(sender, e);
    //  switch (e.Action)
    //  {
    //    case NotifyCollectionChangedAction.Reset:
    //      if (IsAutoScaleEnabled)
    //      {
    //        CalculateMinMaxChartValues(this.Items.SourceCollection);
    //      }

    //      break;
    //    case NotifyCollectionChangedAction.Add:
    //      if (IsAutoScaleEnabled)
    //      {
    //        CalculateMinMaxChartValues(e.NewItems);
    //      }

    //      break;
    //    case NotifyCollectionChangedAction.Remove:
    //      {
    //        // Do  nothing if removed items don't affect min max coordinate values
    //        IEnumerable<ICartesianChartPoint> cartesianChartPoints = e.NewItems.OfType<ICartesianChartPoint>();
    //        IEnumerable<Point> points = e.NewItems.OfType<Point>();
    //        if (cartesianChartPoints.All(
    //          chartPoint => chartPoint.X < MaxXValue && chartPoint.Y < MaxYValue &&
    //                        chartPoint.X > MinXValue && chartPoint.Y > MinYValue) && points.All(
    //          chartPoint => chartPoint.X < MaxXValue && chartPoint.Y < MaxYValue &&
    //                        chartPoint.X > MinXValue && chartPoint.Y > MinYValue))
    //        {
    //          return;
    //        }

    //        if (IsAutoScaleEnabled)
    //        {
    //          CalculateMinMaxChartValues(e.NewItems);
    //        }

    //        break;
    //      }
    //  }
    //}

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      base.PrepareContainerForItemOverride(element, item);
      if (!(element is CartesianChartItem chartPointContainer))
      {
        return;
      }

      //switch (item)
      //{
      //  case ICartesianChartPoint chartPoint:
      //    if (chartPoint.SeriesId == null || !SeriesExtremeValues.TryGetValue(
      //      chartPoint.SeriesId,
      //      out (Point Minima, Point Maxima) extrema))
      //    {
      //      extrema = (
      //        new Point(
      //          IsAutoScaleEnabled ? MinXValueInternal : MinXValue,
      //          IsAutoScaleEnabled ? MinYValueInternal : MinYValue),
      //        new Point(
      //          IsAutoScaleEnabled ? MaxXValueInternal : MaxXValue,
      //          IsAutoScaleEnabled ? MaxYValueInternal : MaxYValue));
      //    }

      //    chartPointContainer.X = chartPoint.X;
      //    chartPointContainer.Y = chartPoint.Y;
      //    chartPointContainer.MaxX = extrema.Maxima.X;
      //    chartPointContainer.MaxY = extrema.Maxima.Y;
      //    chartPointContainer.MinX = extrema.Minima.X;
      //    chartPointContainer.MinY = extrema.Minima.Y;
      //    break;
      //  case Point chartPoint:
      //    chartPointContainer.X = chartPoint.X;
      //    chartPointContainer.Y = chartPoint.Y;
      //    chartPointContainer.MaxX = IsAutoScaleEnabled ? MaxXValueInternal : MaxXValue;
      //    chartPointContainer.MaxY = IsAutoScaleEnabled ? MaxYValueInternal : MaxYValue;
      //    chartPointContainer.MinX = IsAutoScaleEnabled ? MinXValueInternal : MinXValue;
      //    chartPointContainer.MinY = IsAutoScaleEnabled ? MinYValueInternal : MinYValue;
      //    break;
      //}
    }

    #endregion Overrides of Chart

    protected virtual void OnSelectedViewChanged(CartesianChartViewId oldValue, CartesianChartViewId newValue)
    {
      this.ViewId = newValue;
      this.PointInfoGenerator.ChangeView(newValue);
    }

    //private void CalculateMinMaxChartValues(IEnumerable input)
    //{
    //  IEnumerable<ICartesianChartPoint> cartesianChartPoints = input.OfType<ICartesianChartPoint>();
    //  IEnumerable<Point> points = input.OfType<Point>();

    //  foreach (ICartesianChartPoint cartesianChartPoint in cartesianChartPoints)
    //  {
    //    if (cartesianChartPoint.SeriesId != null)
    //    {
    //      if (!SeriesExtremeValues.TryGetValue(
    //        cartesianChartPoint.SeriesId,
    //        out (Point Minima, Point Maxima) extremeValues))
    //      {
    //        extremeValues = (new Point(0, 0), new Point(0, 0));
    //        SeriesExtremeValues.Add(cartesianChartPoint.SeriesId, extremeValues);
    //      }

    //      extremeValues.Maxima.Y = Math.Max(extremeValues.Maxima.Y, cartesianChartPoint.Y);
    //      extremeValues.Minima.Y = Math.Min(extremeValues.Minima.Y, cartesianChartPoint.Y);
    //      extremeValues.Maxima.X = Math.Max(extremeValues.Maxima.X, cartesianChartPoint.X);
    //      extremeValues.Minima.X = Math.Min(extremeValues.Minima.X, cartesianChartPoint.X);

    //      MaxXValueInternal = Math.Max(MaxXValueInternal, extremeValues.Maxima.X);
    //      MaxYValueInternal = Math.Max(MaxYValueInternal, extremeValues.Maxima.Y);
    //      MinXValueInternal = Math.Min(MinXValueInternal, extremeValues.Minima.X);
    //      MinYValueInternal = Math.Min(MinYValueInternal, extremeValues.Minima.Y);
    //    }
    //    else
    //    {
    //      MaxXValueInternal = Math.Max(MaxXValueInternal, cartesianChartPoint.X);
    //      MaxYValueInternal = Math.Max(MaxYValueInternal, cartesianChartPoint.Y);
    //      MinXValueInternal = Math.Min(MinXValueInternal, cartesianChartPoint.X);
    //      MinYValueInternal = Math.Min(MinYValueInternal, cartesianChartPoint.Y);
    //    }
    //  }

    //  foreach (Point point in points)
    //  {
    //    MaxXValueInternal = Math.Max(MaxXValueInternal, point.X);
    //    MaxYValueInternal = Math.Max(MaxYValueInternal, point.Y);
    //    MinXValueInternal = Math.Min(MinXValueInternal, point.X);
    //    MinYValueInternal = Math.Min(MinYValueInternal, point.Y);
    //  }
    //}

    //internal (Point Minima, Point Maxima) GetExtremePointsOf(ICartesianChartPoint cartesianChartPoint)
    //{
    //  if (cartesianChartPoint.SeriesId != null && SeriesExtremeValues.TryGetValue(
    //    cartesianChartPoint.SeriesId,
    //    out (Point Minima, Point Maxima) extrema))
    //  {
    //    return extrema;
    //  }

    //  return GetExtremePointsOf(cartesianChartPoint.ToPoint());
    //}

    //internal (Point Minima, Point Maxima) GetExtremePointsOf(Point chartPoint) =>
    //  (
    //    new Point(
    //      IsAutoScaleEnabled ? MinXValueInternal : MinXValue,
    //      IsAutoScaleEnabled ? MinYValueInternal : MinYValue),
    //    new Point(
    //      IsAutoScaleEnabled ? MaxXValueInternal : MaxXValue,
    //      IsAutoScaleEnabled ? MaxYValueInternal : MaxYValue));

    private SeriesSelector DefaultSeriesSelector { get; }
  }
}
