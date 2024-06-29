namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections;
  using System.Collections.Specialized;
  using System.ComponentModel;
  using System.Globalization;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Data;
  using BionicCode.Utilities.Net;

  public abstract class Chart : Control
  {
    #region Depedency properties

    #region Items

    public CollectionView Items
    {
      get => (CollectionView)GetValue(Chart.ItemsProperty);
      private set => SetValue(Chart.ItemsPropertyKey, value);
    }

    public static DependencyProperty ItemsProperty => Chart.ItemsPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(
          "Items",
          typeof(CollectionView),
          typeof(Chart),
          new FrameworkPropertyMetadata(default(CollectionView), OnItemsChanged));

    #endregion Items

    #region ItemsSource
    public IEnumerable ItemsSource
    {
      get => (IEnumerable)GetValue(Chart.ItemsSourceProperty);
      set => SetValue(Chart.ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
          "ItemsSource",
          typeof(IEnumerable),
          typeof(Chart),
          new FrameworkPropertyMetadata(default(IEnumerable), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange, OnItemsSourceChanged));
    #endregion ItemsSource

    #region SeriesSelector

    public SeriesSelector SeriesSelector
    {
      get => (SeriesSelector)GetValue(SeriesSelectorProperty);
      set => SetValue(SeriesSelectorProperty, value);
    }

    public static readonly DependencyProperty SeriesSelectorProperty =
        DependencyProperty.Register("SeriesSelector", typeof(SeriesSelector), typeof(Chart), new PropertyMetadata(default));

    #endregion SeriesSelector

    #region SelectedItem

    public object SelectedItem
    {
      get => GetValue(SelectedItemProperty);
      set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
          "SelectedItem",
          typeof(object),
          typeof(Chart),
          new FrameworkPropertyMetadata(default, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    #endregion SelectedItem

    #region IsAutoScaleEnabled

    /// <summary>
    /// Sets
    /// </summary>
    public static readonly DependencyProperty IsAutoScaleEnabledProperty = DependencyProperty.Register(
      "IsAutoScaleEnabled",
      typeof(bool),
      typeof(Chart),
      new PropertyMetadata(true));

    /// <summary>
    /// When set to <c>true</c> the chart's min and max values of the axis are calculated based on the chart points.
    /// </summary>
    public bool IsAutoScaleEnabled
    {
      get => (bool)GetValue(Chart.IsAutoScaleEnabledProperty);
      set => SetValue(Chart.IsAutoScaleEnabledProperty, value);
    }

    #endregion IsAutoScaleEnabled

    #region XZoomFactor

    public ZoomFactor XZoomFactor
    {
      get => (ZoomFactor)GetValue(XZoomFactorProperty);
      set => SetValue(XZoomFactorProperty, value);
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty XZoomFactorProperty =
        DependencyProperty.Register(
          "XZoomFactor",
          typeof(ZoomFactor),
          typeof(Chart),
          new PropertyMetadata(new ZoomFactor(1, ZoomFactorUnit.Auto)));

    #endregion XZoomFactor

    #region YZoomFactor

    public ZoomFactor YZoomFactor
    {
      get => (ZoomFactor)GetValue(YZoomFactorProperty);
      set => SetValue(YZoomFactorProperty, value);
    }

    // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty YZoomFactorProperty =
        DependencyProperty.Register(
          "YZoomFactor",
          typeof(ZoomFactor),
          typeof(Chart),
          new PropertyMetadata(new ZoomFactor(1, ZoomFactorUnit.Auto)));

    #endregion YZoomFactor

    #region DisplayMode

    public DisplayMode DisplayMode
    {
      get => (DisplayMode)GetValue(DisplayModeProperty);
      set => SetValue(DisplayModeProperty, value);
    }

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(
          "DisplayMode",
          typeof(DisplayMode),
          typeof(Chart),
          new FrameworkPropertyMetadata(DisplayMode.FitVerticalAxisToScreen, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayModeChanged));

    #endregion DisplayMode

    #region ItemsPanel

    public ChartPanel ItemsPanel
    {
      get => (ChartPanel)GetValue(ItemsPanelProperty);
      set => SetValue(ItemsPanelProperty, value);
    }

    public static readonly DependencyProperty ItemsPanelProperty =
        DependencyProperty.Register("ItemsPanel", typeof(ChartPanel), typeof(Chart), new PropertyMetadata(default));

    #endregion ItemsPanel

    #endregion Dependency properties

    static Chart() => FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
        typeof(Chart),
        new FrameworkPropertyMetadata(typeof(Chart)));

    protected Chart() => this.ResourceLocator = new ResourceLocator(this);

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as Chart).OnItemsSourceChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as Chart).OnItemsChanged(e.OldValue as CollectionView, e.NewValue as CollectionView);
    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as Chart).OnSelectedItemChanged(e.OldValue, e.NewValue);
    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as Chart).OnDisplayModeChanged((DisplayMode)e.OldValue, (DisplayMode)e.NewValue);

    public abstract SeriesSelector GetDefaultSeriesSelector();

    protected override void OnInitialized(EventArgs e) => base.OnInitialized(e);

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      if (this.TryFindVisualChildElement(out ScrollViewer scrollViewer))
      {
        if (scrollViewer.Content is ChartPanelPresenter chartPanelPresenter
          || ((scrollViewer.Content as DependencyObject)?.TryFindVisualChildElement(out chartPanelPresenter) ?? false))
        {
          this.PointInfoGenerator = chartPanelPresenter.GetPointInfoGenerator();
        }
      }
      else if (this.TryFindVisualChildElement(out ChartPanelPresenter chartPanelPresenter))
      {
        this.PointInfoGenerator = chartPanelPresenter.PointInfoGenerator;
      }

      this.PointInfoGenerator?.ChangeView(this.ViewId);
      this.PointInfoGenerator.UpdateDisplayMode(this.DisplayMode);
      //this.PointInfoGenerator?.Generate();
    }

    protected virtual void OnSelectedItemChanged(object oldValue, object newValue) => throw new NotImplementedException();
    protected virtual void OnDisplayModeChanged(DisplayMode oldValue, DisplayMode newValue) => this.PointInfoGenerator.UpdateDisplayMode(newValue);

    protected virtual void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
      if (this.Items != null)
      {
        ((INotifyCollectionChanged)this.Items).CollectionChanged -= OnItemsViewChanged;
      }

      if (newValue == null)
      {
        return;
      }
      this.Items = CollectionViewSource.GetDefaultView(newValue) as CollectionView;
      ((INotifyCollectionChanged)this.Items).CollectionChanged += OnItemsViewChanged;
      this.PointInfoGenerator?.OnOwnerItemsChanged();
    }

    protected virtual void OnItemsChanged(CollectionView oldValue, CollectionView newValue)
    {
    }

    /// <inheritdoc />
    protected virtual DependencyObject GetContainerForItemOverride() => new CartesianChartItem();

    /// <inheritdoc />
    protected virtual bool IsItemItsOwnContainerOverride(object item) => item is CartesianChartItem;

    /// <inheritdoc />
    protected virtual void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
      //if (!(element is CartesianChartItem chartPointContainer))
      //{
      //  return;
      //}

      //switch (item)
      //{
      //  case ICartesianChartPoint chartPoint:
      //    if (chartPoint.SeriesId == null || !this.SeriesExtremeValues.TryGetValue(
      //      chartPoint.SeriesId,
      //      out (Point Minima, Point Maxima) extrema))
      //    {
      //      extrema = (
      //        new Point(
      //          this.IsAutoScaleEnabled ? this.MinXValueInternal : this.MinXValue,
      //          this.IsAutoScaleEnabled ? this.MinYValueInternal : this.MinYValue),
      //        new Point(
      //          this.IsAutoScaleEnabled ? this.MaxXValueInternal : this.MaxXValue,
      //          this.IsAutoScaleEnabled ? this.MaxYValueInternal : this.MaxYValue));
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
      //    chartPointContainer.MaxX = this.IsAutoScaleEnabled ? this.MaxXValueInternal : this.MaxXValue;
      //    chartPointContainer.MaxY = this.IsAutoScaleEnabled ? this.MaxYValueInternal : this.MaxYValue;
      //    chartPointContainer.MinX = this.IsAutoScaleEnabled ? this.MinXValueInternal : this.MinXValue;
      //    chartPointContainer.MinY = this.IsAutoScaleEnabled ? this.MinYValueInternal : this.MinYValue;
      //    break;
      //}
    }

    /// <inheritdoc />
    protected virtual void OnItemsViewChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      //switch (e.Action)
      //{
      //  case NotifyCollectionChangedAction.Reset:
      //    if (this.IsAutoScaleEnabled)
      //    {
      //      CalculateMinMaxChartValues(this.Items.SourceCollection);
      //    }

      //    break;
      //  case NotifyCollectionChangedAction.Add:
      //    if (this.IsAutoScaleEnabled)
      //    {
      //      CalculateMinMaxChartValues(e.NewItems);
      //    }

      //    break;
      //  case NotifyCollectionChangedAction.Remove:
      //    {
      //      // Do  nothing if removed items don't affect min max coordinate values
      //      IEnumerable<ICartesianChartPoint> cartesianChartPoints = e.NewItems.OfType<ICartesianChartPoint>();
      //      IEnumerable<Point> points = e.NewItems.OfType<Point>();
      //      if (cartesianChartPoints.All(
      //        chartPoint => chartPoint.X < this.MaxXValue && chartPoint.Y < this.MaxYValue &&
      //                      chartPoint.X > this.MinXValue && chartPoint.Y > this.MinYValue) && points.All(
      //        chartPoint => chartPoint.X < this.MaxXValue && chartPoint.Y < this.MaxYValue &&
      //                      chartPoint.X > this.MinXValue && chartPoint.Y > this.MinYValue))
      //      {
      //        return;
      //      }

      //      if (this.IsAutoScaleEnabled)
      //      {
      //        CalculateMinMaxChartValues(e.NewItems);
      //      }

      //      break;
      //    }
      //}
    }
    internal ResourceLocator ResourceLocator { get; }
    internal PointInfoGenerator PointInfoGenerator { get; set; }
    protected object ViewId { get; set; }
  }
  public enum ZoomFactorUnit
  {
    /// <summary>
    /// The value indicates that content should be calculated without constraints. 
    /// </summary>
    Auto = 0,
    /// <summary>
    /// The value is expressed as a pixel.
    /// </summary>
    Pixel,
    /// <summary>
    /// The value is expressed as a weighted proportion of available space.
    /// </summary>
    Star,
  }

  [TypeConverter(typeof(ZoomFactorConverter))]
  public readonly struct ZoomFactor
  {
    /// <summary>
    /// Constructor, initializes the GridLength and specifies what kind of value 
    /// it will hold.
    /// </summary>
    /// <param name="value">Value to be stored by this GridLength 
    /// instance.</param>
    /// <param name="unit">Types of the value to be stored by this GridLength 
    /// instance.</param>
    /// <remarks> 
    /// If the <c>type</c> parameter is <c>GridUnitType.Auto</c>, 
    /// then passed in value is ignored and replaced with <c>0</c>.
    /// </remarks>
    /// <exception cref="ArgumentException">
    /// If <c>value</c> parameter is <c>double.NaN</c>
    /// or <c>value</c> parameter is <c>double.NegativeInfinity</c>
    /// or <c>value</c> parameter is <c>double.PositiveInfinity</c>.
    /// </exception>
    public ZoomFactor(double value, ZoomFactorUnit unit)
    {
      if (double.IsNaN(value))
      {
        throw new ArgumentException($"Value {nameof(double.NaN)} is not allowed with this constructor.", "value");
      }
      if (double.IsInfinity(value))
      {
        throw new ArgumentException($"Value {nameof(double.PositiveInfinity)} and {nameof(double.NegativeInfinity)} is not allowed with this constructor.", "value");
      }
      this.Value = unit == ZoomFactorUnit.Auto ? 0.0 : value;
      this.Unit = unit;
    }

    public ZoomFactor(ZoomFactorUnit unit) : this(1, unit)
    { }
    public ZoomFactor(double value) : this(value, ZoomFactorUnit.Pixel)
    { }

    public override string ToString() => this.Unit switch
    {
      ZoomFactorUnit.Pixel => this.Value.ToString(),
      ZoomFactorUnit.Auto => this.Unit.ToString(),
      ZoomFactorUnit.Star => $"{this.Value}{this.Unit}",
      _ => base.ToString()
    };

    public ZoomFactorUnit Unit { get; }

    public double Value { get; }
  }

  public class ZoomFactorConverter : TypeConverter
  {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      TypeCode typeCode = Type.GetTypeCode(sourceType);
      switch (typeCode)
      {
        case TypeCode.String:
        case TypeCode.Decimal:
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.Int16:
        case TypeCode.Int32:
        case TypeCode.Int64:
        case TypeCode.UInt16:
        case TypeCode.UInt32:
        case TypeCode.UInt64:
          return true;
        default:
          return false;
      }
    }

    public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType) => destinationType == typeof(string) ? true : base.CanConvertTo(typeDescriptorContext, destinationType);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is string stringValue)
      {
        return ConvertFromtString(stringValue.Trim());
      }

      if (value == null)
      {
        return base.ConvertFrom(context, culture, value);
      }

      double doubleValue = Convert.ToDouble(value, culture);
      ZoomFactorUnit unit = double.IsNaN(doubleValue) ? ZoomFactorUnit.Auto : ZoomFactorUnit.Pixel;
      return new ZoomFactor(unit == ZoomFactorUnit.Auto ? 0.0 : doubleValue, unit);
    }

    private ZoomFactor ConvertFromtString(string zoomFactorUnitString)
    {
      ZoomFactorUnit unit = ZoomFactorUnit.Pixel;

      if (Enum.TryParse(zoomFactorUnitString, out ZoomFactorUnit zoomFactorUnit))
      {
        return new ZoomFactor(zoomFactorUnit);
      }

      int unitIndex = -1;
      foreach (string factorUnit in Enum.GetNames(typeof(ZoomFactorUnit)))
      {
        unitIndex = zoomFactorUnitString.IndexOf(factorUnit, StringComparison.OrdinalIgnoreCase);
        if (unitIndex < 0)
        {
          unit = Enum.Parse<ZoomFactorUnit>(factorUnit, true);
          break;
        }
      }

      if (unitIndex == -1)
      {
        throw new ArgumentException($"Invalid value. Unable to convert from '{zoomFactorUnitString}' to a valid {typeof(ZoomFactorUnit)}.", zoomFactorUnitString);
      }

      string valueString = zoomFactorUnitString[..unitIndex];
      if (!double.TryParse(valueString, out double value))
      {
        throw new ArgumentException($"Invalid value. Unable to convert from '{zoomFactorUnitString}' to a valid {typeof(ZoomFactorUnit)}.", zoomFactorUnitString);
      }

      return new ZoomFactor(value, unit);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) => destinationType == typeof(string) && value is ZoomFactor zoomFactor
      ? zoomFactor.ToString()
      : base.ConvertTo(context, culture, value, destinationType);

    public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
    {
      var unit = (ZoomFactorUnit)propertyValues[nameof(ZoomFactor.Unit)];
      double value = (double)propertyValues[nameof(ZoomFactor.Value)];
      return new ZoomFactor(value, unit);
    }

    public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;
    public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes) => TypeDescriptor.GetProperties(value, attributes);
    public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;
  }
}
