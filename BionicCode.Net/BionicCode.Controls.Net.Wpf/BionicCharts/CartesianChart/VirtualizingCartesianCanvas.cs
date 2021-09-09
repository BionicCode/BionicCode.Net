using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using BionicCode.Utilities.Net.Wpf.Extensions;

namespace BionicCode.Controls.Net.Wpf.BionicCharts.CartesianChart
{
  public class VirtualizingCartesianCanvas : VirtualizingPanel, IScrollInfo
  {
    #region XZoomFactor attached property

    public static readonly DependencyProperty XZoomFactorProperty = DependencyProperty.RegisterAttached(
      "XZoomFactor", typeof(double), typeof(VirtualizingCartesianCanvas), new FrameworkPropertyMetadata(1.0, OnXZoomFactorChanged));

    public static void SetXZoomFactor(DependencyObject attachingElement, double value) => attachingElement.SetValue(XZoomFactorProperty, value);

    public static double GetXZoomFactor(DependencyObject attachingElement) => (double)attachingElement.GetValue(XZoomFactorProperty);

    #endregion
    #region YZoomFactor attached property

    public static readonly DependencyProperty YZoomFactorProperty = DependencyProperty.RegisterAttached(
      "YZoomFactor", typeof(double), typeof(VirtualizingCartesianCanvas), new FrameworkPropertyMetadata(1.0, OnYZoomFactorChanged));

    public static void SetYZoomFactor(DependencyObject attachingElement, double value) => attachingElement.SetValue(YZoomFactorProperty, value);

    public static double GetYZoomFactor(DependencyObject attachingElement) => (double)attachingElement.GetValue(YZoomFactorProperty);

    #endregion

    #region Dependency properties

    #endregion Dependency properties

    public VirtualizingCartesianCanvas()
    {
      _invalidRect = new Rect(double.PositiveInfinity, double.PositiveInfinity, 0, 0);
      AverageItemSize = new Size();
      //this.FirstViewportItem = (0, null);
      //this.LastViewportItem = (0, null);
      InvalidateLayout();
    }

    #region Overrides of VirtualizingPanel

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      Owner = ItemsControl.GetItemsOwner(this) as CartesianChart;
    }

    #endregion

    /// <inheritdoc />
    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
      base.OnItemsChanged(sender, args);

    }

    #endregion Overrides of VirtualizingPanel

    #region Overrides of FrameworkElement

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
      if (IsExtentInvalid || IsLayoutInvalid)
      {
        return finalSize;
      }

      foreach (UIElement internalChild in InternalChildren)
      {
        var absoluteContainerPosition = new Point();

        // Best performance: container is a CartesianChartItem.
        if (internalChild is CartesianChartItem chartPointItem)
        {
          absoluteContainerPosition = CalculateAbsoluteItemPosition(chartPointItem.ToPoint(), chartPointItem.ToExtremeMaxPoint());
        }
        else if (internalChild is FrameworkElement frameworkElement)
        {
          double maxXChartPointValueForNonCartesianChartItems = double.NaN;
          double maxXPointValueForNonCartesianChartItems = double.NaN;
          double maxYChartPointValueForNonCartesianChartItems = double.NaN;
          double maxYPointValueForNonCartesianChartItems = double.NaN;

          if (frameworkElement.DataContext is Point point)
          {
            if (double.IsNaN(maxXPointValueForNonCartesianChartItems))
            {
              maxXPointValueForNonCartesianChartItems = InternalChildren.OfType<FrameworkElement>()
                .Select(element => element.DataContext).OfType<Point>().Max(p => p.X);
            }
            if (double.IsNaN(maxYPointValueForNonCartesianChartItems))
            {
              maxYPointValueForNonCartesianChartItems = InternalChildren.OfType<FrameworkElement>()
                .Select(element => element.DataContext).OfType<Point>().Max(p => p.Y);
            }
            absoluteContainerPosition = CalculateAbsoluteItemPosition(point, new Point(maxXPointValueForNonCartesianChartItems, maxYPointValueForNonCartesianChartItems));
          }
          else if (frameworkElement.DataContext is ICartesianChartPoint chartPoint)
          {
            if (double.IsNaN(maxXChartPointValueForNonCartesianChartItems))
            {
              maxXChartPointValueForNonCartesianChartItems = InternalChildren.OfType<FrameworkElement>()
                .Select(element => element.DataContext).OfType<Point>().Max(p => p.X);
            }
            if (double.IsNaN(maxYChartPointValueForNonCartesianChartItems))
            {
              maxYChartPointValueForNonCartesianChartItems = InternalChildren.OfType<FrameworkElement>()
                .Select(element => element.DataContext).OfType<Point>().Max(p => p.Y);
            }
            absoluteContainerPosition = CalculateAbsoluteItemPosition(chartPoint.ToPoint(), new Point(maxXChartPointValueForNonCartesianChartItems, maxYChartPointValueForNonCartesianChartItems));
          }
        }
        else
        {
          Point canvasPosition = internalChild.GetCanvasPosition(finalSize);

          double x = (double.IsNaN(canvasPosition.X) ? 0 : canvasPosition.X) * GetXZoomFactor(this);
          double y = (double.IsNaN(canvasPosition.Y) ? 0 : canvasPosition.Y) * GetYZoomFactor(this);
          absoluteContainerPosition = new Point(x, y);
        }

        ScrollPositionToOffset(ref absoluteContainerPosition);
        internalChild.Arrange(
          new Rect(absoluteContainerPosition, internalChild.DesiredSize));
      }

      return finalSize;
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      if (
        IsScrolling)
      {
        return constraint;
      }
      EnsureGenerator();
      if (ItemContainerGenerator == null)
      {
        return constraint;
      }
      bool isVirtualizationEnabled = VirtualizingPanel.GetIsVirtualizing(Owner);
      if (!TryUpdateViewport(constraint))
      {
        return _dimension.Size;
      }

      CalculateVirtualizationBounds();

      InvalidateExtent();
      if (InternalChildren.Count > 0)
      {
        CalculateNewExtent(InternalChildren[0] as CartesianChartItem);
      }
      using ((ItemContainerGenerator as ItemContainerGenerator).GenerateBatches())
      {
        (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) startInfo = GetLastGeneratedItem();
        RealizeItems(startInfo.GeneratorStartPosition, GeneratorDirection.Forward);
      }

      //StoreBoundaryItems();
      ScrollOwner.InvalidateScrollInfo();
      return _dimension.Size;
    }

    private (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) GetFirstGeneratedItem()
    {
      UIElement firstItem = InternalChildren.Count == 0 ? null : InternalChildren[0];

      if (firstItem == null)
      {
        ;
      }
      GeneratorPosition generatorPosition = firstItem == null
        ? new GeneratorPosition(-1, 0)
        : new GeneratorPosition(0, 0);
      int index = ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
      return (firstItem, index, generatorPosition);
    }

    private (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) GetLastGeneratedItem()
    {
      UIElement lastItem = InternalChildren.Count == 0 ? null : InternalChildren[^1];

      GeneratorPosition generatorPosition = lastItem == null
        ? new GeneratorPosition(-1, 0)
        : new GeneratorPosition(InternalChildren.Count - 1, 1);
      int index = ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);
      return (lastItem, index, generatorPosition);
    }

    private int GetRealizationStartIndex(GeneratorDirection realizationDirection)
    {
      return realizationDirection == GeneratorDirection.Forward
        ? GetLastGeneratedItem().GeneratorIndex
        : GetFirstGeneratedItem().GeneratorIndex;
      //var cartesianChart = ItemsControl.GetItemsOwner(this.Owner) as CartesianChart;
      //var generator = this.ItemContainerGenerator as ItemContainerGenerator;
      //ReadOnlyCollection<object> generatorItems = generator.Items;

      //(object Item, int GeneratorIndex) startInfo = GetFirstGeneratedItem();

      //(Point ChartPoint, Point Minima, Point Maxima) firstItem = cartesianChart.ConvertItemToPointInfo(startInfo.Item);
      //var startIndex = startInfo.GeneratorIndex;

      //Point absolutePoint = CalculateAbsoluteItemPosition(firstItem.ChartPoint, firstItem.Maxima);
      //if (absolutePoint.X < this._actualVirtualizationBounds.X)
      //{
      //  while (startIndex < generatorItems.Count - 1 && absolutePoint.X < this._actualVirtualizationBounds.X)
      //  {
      //    startIndex++; 
      //    firstItem = cartesianChart.ConvertItemToPointInfo(generatorItems[startIndex]);
      //    absolutePoint = CalculateAbsoluteItemPosition(firstItem.ChartPoint, firstItem.Maxima);
      //  }
      //}
      //else
      //{
      //  while (startIndex > 0 && absolutePoint.X > this._actualVirtualizationBounds.X)
      //  {
      //    startIndex--;
      //  }
      //}
      //return startIndex;
    }

    #endregion Overrides of FrameworkElement

    private void ScrollPositionToOffset(ref Point absoluteContainerPosition)
    {
      absoluteContainerPosition.Offset(-HorizontalOffset, -VerticalOffset);
    }

    private Point CalculateAbsoluteItemPosition(Point relativePosition, Point extremePoint)
    {
      double totalAvailableHeight = ExtentHeight - AverageItemSize.Height;
      double totalAvailableWidth = ExtentWidth - AverageItemSize.Width;

      CoerceCoordinate(ref relativePosition);
      CoerceCoordinate(ref extremePoint);

      // Eliminate negative screen positions by shifting the graph by a 
      // offset that equals the biggest negative number of each axis
      // or '0' if all values are positive.
      relativePosition.Offset(OriginOffset.X, OriginOffset.Y);
      extremePoint.Offset(OriginOffset.X, OriginOffset.Y);

      double xZoomFactor = ExtentWidth <= ViewportWidth ? GetXZoomFactor(this) : 1.0;
      double yZoomFactor = ExtentHeight <= ViewportHeight ? GetYZoomFactor(this) : 1.0;

      // Apply ratio to make biggest x-axis data point
      // fit into the total panel width (ExtentWidth).
      double xPositionRatio = CalculatePositionRatio(totalAvailableWidth, extremePoint.X);

      // Apply ratio to compact y-axis
      // in order to make all data points fit into the viewport height.
      double yPositionRatio = CalculatePositionRatio(totalAvailableHeight, extremePoint.Y);

      Point absolutePosition = relativePosition
        .Scale(xPositionRatio, yPositionRatio)
        .Zoom(xZoomFactor, yZoomFactor)
        .ToPointOnScreen(totalAvailableHeight);

      return absolutePosition;
    }

    private void CoerceCoordinate(ref Point relativePosition)
    {
      if (double.IsNaN(relativePosition.X))
      {
        relativePosition.X = 0.0;
      }
      if (double.IsNaN(relativePosition.Y))
      {
        relativePosition.Y = 0.0;
      }
    }

    /// <summary>
    /// Calculates the ratio for a point or a series of points. The value can be used to get the factor which has to be applied to each point in order to fit them into a limited area.
    /// </summary>
    /// <param name="axisLimit">The maximum possible value.</param>
    /// <param name="maxValue">The maximum occurring value.</param>
    /// <returns>The ratio of the two parameters.</returns>
    protected virtual double CalculatePositionRatio(double axisLimit, double maxValue) =>
      axisLimit /
      (maxValue.Equals(0)
        ? axisLimit
        : maxValue);

    private void InvalidateExtent()
    {
      _dimension = _invalidRect;
    }

    private void InvalidateLayout()
    {
      _viewPort = _invalidRect;
      InvalidateVirtualizationBounds();
      InvalidateExtent();
      InvalidateMeasure();
    }

    private void InvalidateVirtualizationBounds()
    {
      _virtualizationBounds = _invalidRect;
      _virtualizationBounds = _invalidRect;
    }

    private bool TryUpdateViewport(Size newSize)
    {
      bool isChanged = !newSize.Width.Equals(ViewportWidth) || !newSize.Height.Equals(ViewportHeight);
      if (isChanged)
      {
        _viewPort.X = 0 + HorizontalOffset;
        _viewPort.Y = 0 + VerticalOffset;
        _viewPort.Size = newSize;
      }
      return isChanged;
    }

    protected void RealizeItems(GeneratorPosition generatorPosition, GeneratorDirection generatorDirection)
    {
      bool isVirtualizationEnabled = VirtualizingPanel.GetIsVirtualizing(Owner);

      using (ItemContainerGenerator.StartAt(
        generatorPosition,
        generatorDirection,
        true))
      {
        var availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        var totalItemWidth = 0.0;
        var totalItemHeight = 0.0;
        var itemWidth = 0.0;
        var itemHeight = 0.0;
        int itemsGeneratedCount = 0;
        while (itemsGeneratedCount < (ItemContainerGenerator as ItemContainerGenerator).Items.Count - Math.Max(0, generatorPosition.Index))
        {
          var itemContainer = ItemContainerGenerator.GenerateNext(out bool isNew) as UIElement;
          itemsGeneratedCount++;
          if (itemContainer == null)
          {
            continue;
          }
          ItemContainerGenerator.PrepareItemContainer(itemContainer);
          if (isNew || !InternalChildren.Contains(itemContainer))
          {
            if (generatorDirection == GeneratorDirection.Forward)
            {
              AddInternalChild(itemContainer);
            }
            else
            {
              InsertInternalChild(0, itemContainer);
            }
            itemContainer.Measure(availableSize);
          }

          if (itemsGeneratedCount == 1)
          {
            itemHeight = AverageItemSize.Height > 0
              ? AverageItemSize.Height
              : itemContainer.DesiredSize.Height;
            itemWidth = AverageItemSize.Width > 0
              ? AverageItemSize.Width
              : itemContainer.DesiredSize.Width;
          }

          if (IsExtentInvalid)
          {
            CalculateNewExtent(itemContainer as CartesianChartItem);
          }

          totalItemWidth += itemContainer.DesiredSize.Width;
          totalItemHeight += itemContainer.DesiredSize.Height;
          itemWidth = (itemWidth + itemContainer.DesiredSize.Width) / 2;
          itemHeight = (itemHeight + itemContainer.DesiredSize.Height) / 2;
          AverageItemSize = new Size(itemWidth, itemHeight);

          if (!isVirtualizationEnabled)
          {
            continue;
          }
          if (generatorDirection == GeneratorDirection.Forward && CompareItemContainerToVirtualizationBounds(itemContainer) == 1 || generatorDirection == GeneratorDirection.Backward && CompareItemContainerToVirtualizationBounds(itemContainer) == -1)
          {
            break;
          }
        }
      }
    }

    private void CalculateNewOriginOffset(double? minXValue, double? minYValue)
    {
      object firstItem = (ItemContainerGenerator as ItemContainerGenerator).Items[0];

      double smallestXValue = minXValue ??
                              (firstItem is ICartesianChartPoint cartesianChartPoint
                                ? cartesianChartPoint.X
                                : firstItem is Point point
                                  ? point.X
                                  : 0);

      double smallestYValue = minYValue ?? 0;

      OriginOffset = new Point(
        smallestXValue < 0 ? Math.Abs(smallestXValue) : 0,
        smallestYValue < 0 ? Math.Abs(smallestYValue) : 0);
    }

    private void CalculateNewExtent(CartesianChartItem itemContainer)
    {
      double? maxXValue = itemContainer?.MaxX;
      double extentXZoomFactor = Math.Max(1.0, GetXZoomFactor(this));

      CalculateNewOriginOffset(itemContainer?.MinX, itemContainer?.MinY);

      _dimension = new Rect
      {
        Width = Math.Max(
          ViewportWidth + OriginOffset.X,
          ((maxXValue ?? ViewportWidth) + OriginOffset.X) *
          extentXZoomFactor),
        //Height = Math.Max(
        //  this.ViewportHeight,
        //  (this.ViewportHeight + this.OriginOffset.Y) * VirtualizingCartesianCanvas.GetYZoomFactor(this))
        Height = Math.Max(ViewportHeight, ViewportHeight * GetYZoomFactor(this))
      };
    }

    private int CompareItemContainerToVirtualizationBounds(UIElement itemContainer)
    {
      Point itemPosition;
      Point extremePosition;
      if (itemContainer is CartesianChartItem cartesianChartItem)
      {
        itemPosition = cartesianChartItem.ToPoint();
        extremePosition = cartesianChartItem.ToExtremeMaxPoint();
      }
      else
      {
        switch ((itemContainer as FrameworkElement)?.DataContext)
        {
          case ICartesianChartPoint cartesianChartPoint:
            itemPosition = cartesianChartPoint.ToPoint();
            extremePosition = Owner.SeriesExtremeValues.TryGetValue(cartesianChartPoint, out (Point Minima, Point Maxima) chartPointExtrema)
              ? chartPointExtrema.Maxima
              : new Point();
            break;
          case Point chartPoint:
            itemPosition = chartPoint;
            extremePosition = Owner.SeriesExtremeValues.TryGetValue(chartPoint, out (Point Minima, Point Maxima) pointExtrema)
              ? pointExtrema.Maxima
              : new Point();
            break;
          default:
            itemPosition = itemContainer.GetCanvasPosition(new Size(ExtentWidth, ExtentHeight));
            extremePosition = itemPosition;
            break;
        }
      }

      itemPosition = CalculateAbsoluteItemPosition(itemPosition, extremePosition);
      return itemPosition.X < _virtualizationBounds.X ? -1 : itemPosition.X > _virtualizationBounds.X + _virtualizationBounds.Width ? 1 : 0;
    }

    private bool IsPointInsideVirtualizationBounds(Point itemPosition, Point extremePosition)
    {
      itemPosition = CalculateAbsoluteItemPosition(itemPosition, extremePosition);
      return _virtualizationBounds.Contains(itemPosition);
    }

    private void CalculateVirtualizationBounds()
    {
      if (!VirtualizingPanel.GetIsVirtualizing(Owner))
      {
        _virtualizationBounds = new Rect(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return;
      }

      VirtualizationCacheLength cacheLength = VirtualizingPanel.GetCacheLength(Owner);
      switch (VirtualizingPanel.GetCacheLengthUnit(Owner))
      {
        case VirtualizationCacheLengthUnit.Page:
          {
            _virtualizationBounds.Width = ViewportWidth + ViewportWidth * cacheLength.CacheBeforeViewport +
                                                    ViewportWidth * cacheLength.CacheAfterViewport;
            _virtualizationBounds.Height = ViewportHeight + ViewportHeight * cacheLength.CacheBeforeViewport +
                                                     ViewportHeight * cacheLength.CacheAfterViewport;
            break;
          }
        case VirtualizationCacheLengthUnit.Item:
        case VirtualizationCacheLengthUnit.Pixel:
          {
            _virtualizationBounds.Width = ViewportWidth + cacheLength.CacheBeforeViewport + cacheLength.CacheAfterViewport;
            _virtualizationBounds.Height = ViewportHeight + cacheLength.CacheBeforeViewport + cacheLength.CacheAfterViewport;
            break;
          }
      }

      _virtualizationBounds.X = 0;
      _virtualizationBounds.Y = 0;
      ScaleVirtualizationBounds();
    }

    private void ScaleVirtualizationBounds()
    {
      double xZoomFactor = GetXZoomFactor(this);
      double yZoomFactor = GetYZoomFactor(this);
      _virtualizationBounds.Scale(xZoomFactor, yZoomFactor);
    }

    private void ScrollVirtualizationBounds(double horizontalOffset, double verticalOffset)
    {
      _virtualizationBounds.X = horizontalOffset;
      _virtualizationBounds.Y = verticalOffset;
    }

    /// <summary>
    /// The get method of <see cref="Panel.InternalChildren"/> actually creates and connects the <see cref="ItemContainerGenerator"/>. Invoking this method ensures that this will happen. Otherwise the generator would be <c>null</c> until internals access the <see cref="Panel.InternalChildren"/> property. Info was deducted from source code review.
    /// </summary>
    protected void EnsureGenerator() => _ = InternalChildren;


    protected virtual void OnScrollOwnerSizeChanged(object sender, SizeChangedEventArgs e)
    {
      //InvalidateMeasure();
    }

    private static void OnXZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is VirtualizingCartesianCanvas _this)
      {
        _this.InvalidateLayout();
        return;
      }

      if (d is FrameworkElement frameworkElement && !frameworkElement.IsLoaded)
      {
        frameworkElement.Loaded += DelegatePlotXZoomFactor_OnParentLoaded;
        return;
      }

      if (d.TryFindVisualChildElement(out VirtualizingCartesianCanvas panel))
      {
        SetXZoomFactor(panel, (double)e.NewValue);
        panel.InvalidateLayout();
      }
    }

    private static void OnYZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is VirtualizingCartesianCanvas _this)
      {
        _this.InvalidateLayout();
        return;
      }

      if (d is FrameworkElement frameworkElement && !frameworkElement.IsLoaded)
      {
        frameworkElement.Loaded += DelegatePlotYZoomFactor_OnParentLoaded;
        return;
      }

      if (d.TryFindVisualChildElement(out VirtualizingCartesianCanvas panel))
      {
        SetYZoomFactor(panel, (double)e.NewValue);
        panel.InvalidateLayout();
      }
    }

    private static void DelegatePlotXZoomFactor_OnParentLoaded(object sender, RoutedEventArgs e)
    {
      var attachedElement = sender as DependencyObject;
      if (attachedElement.TryFindVisualChildElement(out VirtualizingCartesianCanvas panel))
      {
        var newValue = GetXZoomFactor(attachedElement);
        SetXZoomFactor(panel, newValue);
        panel.InvalidateLayout();
      }
    }

    private static void DelegatePlotYZoomFactor_OnParentLoaded(object sender, RoutedEventArgs e)
    {
      var attachedElement = sender as DependencyObject;
      if (attachedElement.TryFindVisualChildElement(out VirtualizingCartesianCanvas panel))
      {
        var newValue = GetYZoomFactor(attachedElement);
        SetYZoomFactor(panel, newValue);
        panel.InvalidateLayout();
      }
    }

    //private static void DelegatePlotYZoomFactor_OnParentLoaded(object sender, RoutedEventArgs e)
    //{
    //  var frameworkElement = sender as FrameworkElement;
    //  frameworkElement.Loaded -= VirtualizingCartesianCanvas.DelegatePlotYZoomFactor_OnParentLoaded;

    //  if (frameworkElement.TryFindVisualChildElement(out VirtualizingCartesianCanvas panel))
    //  {
    //    double yZoomFactor = VirtualizingCartesianCanvas.GetYZoomFactor(frameworkElement);
    //    VirtualizingCartesianCanvas.SetYZoomFactor(panel, yZoomFactor);
    //    panel.InvalidateLayout();
    //  }
    //}

    //private static void DelegatePlotXZoomFactor_OnParentLoaded(object sender, RoutedEventArgs e)
    //{
    //  var frameworkElement = sender as FrameworkElement;
    //  frameworkElement.Loaded -= VirtualizingCartesianCanvas.DelegatePlotXZoomFactor_OnParentLoaded;

    //  if (frameworkElement.TryFindVisualChildElement(out VirtualizingCartesianCanvas panel))
    //  {
    //    double xZoomFactor = VirtualizingCartesianCanvas.GetXZoomFactor(frameworkElement);
    //    VirtualizingCartesianCanvas.SetXZoomFactor(panel, xZoomFactor);
    //    panel.InvalidateLayout();
    //  }
    //}

    #region Implementation of IScrollInfo

    /// <inheritdoc />
    public void LineUp()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LineDown()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void LineLeft()
    {
      SetHorizontalOffset(HorizontalOffset - 1);
    }

    /// <inheritdoc />
    public void LineRight()
    {
      SetHorizontalOffset(HorizontalOffset + 1);
    }

    /// <inheritdoc />
    public void PageUp()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void PageDown()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void PageLeft()
    {
      SetHorizontalOffset(HorizontalOffset - 10);
    }

    /// <inheritdoc />
    public void PageRight()
    {
      SetHorizontalOffset(HorizontalOffset + 10);
    }

    /// <inheritdoc />
    public void MouseWheelUp()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void MouseWheelDown()
    {
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void MouseWheelLeft()
    {
      SetHorizontalOffset(HorizontalOffset - 10);
    }

    /// <inheritdoc />
    public void MouseWheelRight()
    {
      SetHorizontalOffset(HorizontalOffset + 10);
    }

    /// <inheritdoc />
    public void SetHorizontalOffset(double offset)
    {
      offset = Math.Min(Math.Max(offset, 0), ScrollOwner.ScrollableWidth);
      if (offset.Equals(HorizontalOffset) || InternalChildren.Count == 0)
      {
        return;
      }

      ScrollDirection scrollDirection = offset > HorizontalOffset ? ScrollDirection.Right : ScrollDirection.Left;
      HorizontalOffset = offset;

      if (VirtualizingPanel.GetIsVirtualizing(Owner))
      {
        IsScrolling = true;
        using ((ItemContainerGenerator as ItemContainerGenerator).GenerateBatches())
        {
          switch (scrollDirection)
          {
            case ScrollDirection.Left:
              VirtualizeOnScrollLeft();
              break;
            case ScrollDirection.Right:
              VirtualizeOnScrollRight();
              break;
          }
        }
      }

      InvalidateArrange();
      ScrollOwner.InvalidateScrollInfo();
      IsScrolling = false;
    }

    private void VirtualizeOnScrollLeft()
    {
      VirtualizeOnScroll(GeneratorDirection.Backward);
      GenerateContainersBeforeViewport();
    }

    private void VirtualizeOnScrollRight()
    {
      VirtualizeOnScroll(GeneratorDirection.Forward);
      GenerateContainersAfterViewport();
    }

    private void VirtualizeOnScroll(GeneratorDirection generatorDirection)
    {
      _viewPort.X = HorizontalOffset;
      bool isVirtualizationBoundsScrollable =
        HorizontalOffset >=
        VirtualizingPanel.GetCacheLength(Owner).CacheBeforeViewport * ViewportWidth &&
        HorizontalOffset <= ScrollOwner.ScrollableWidth -
        VirtualizingPanel.GetCacheLength(Owner).CacheAfterViewport * ViewportWidth + ViewportWidth;


      if (isVirtualizationBoundsScrollable)
      {
        ScrollVirtualizationBounds(
          HorizontalOffset - VirtualizingPanel.GetCacheLength(Owner).CacheBeforeViewport * ViewportWidth,
          VerticalOffset);
        //RecycleObsoleteItems(generatorDirection);
      }
    }

    private void GenerateContainersBeforeViewport()
    {
      (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) generatorStartIndex = GetFirstGeneratedItem();
      RealizeItems(generatorStartIndex.GeneratorStartPosition, GeneratorDirection.Backward);
    }

    private void GenerateContainersAfterViewport()
    {
      (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) startInfo = GetLastGeneratedItem();
      RealizeItems(startInfo.GeneratorStartPosition, GeneratorDirection.Forward);
    }

    private int RecycleObsoleteItems(GeneratorDirection generatorDirection)
    {
      int recycleItemsCount = 0;
      UIElementCollection realizedItemContainers = InternalChildren;
      if (generatorDirection == GeneratorDirection.Forward)
      {
        (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) firstGeneratedItemInfo = GetFirstGeneratedItem();
        if (firstGeneratedItemInfo.Item == null)
        {
          return 0;
        }

        bool isRemoveingBefore = true;
        while (firstGeneratedItemInfo.Item != null)
        {
          (Point ChartPoint, Point Minima, Point Maxima) pointInfo = Owner.ConvertItemToPointInfo(firstGeneratedItemInfo.Item);
          if (IsPointInsideVirtualizationBounds(pointInfo.ChartPoint, pointInfo.Maxima))
          {
            //if (isRemoveingBefore)
            //{
            //  isRemoveingBefore = false;
            //  firstGeneratedItemInfo = GetLastGeneratedItem();
            //  continue;
            //}
            break;
          }
          (ItemContainerGenerator as IRecyclingItemContainerGenerator).Recycle(firstGeneratedItemInfo.GeneratorStartPosition, 1);
          RemoveInternalChildRange(0, 1);
          firstGeneratedItemInfo = isRemoveingBefore ? GetFirstGeneratedItem() : GetLastGeneratedItem();
          recycleItemsCount++;
        }
      }
      else
      {
        (UIElement Item, int GeneratorIndex, GeneratorPosition GeneratorStartPosition) lastGeneratedItemInfo = GetLastGeneratedItem();
        if (lastGeneratedItemInfo.Item == null)
        {
          return 0;
        }

        while (lastGeneratedItemInfo.Item != null)
        {
          (Point ChartPoint, Point Minima, Point Maxima) pointInfo = Owner.ConvertItemToPointInfo(lastGeneratedItemInfo.Item);
          if (!IsPointInsideVirtualizationBounds(pointInfo.ChartPoint, pointInfo.Maxima))
          {
            break;
          }

          (ItemContainerGenerator as IRecyclingItemContainerGenerator).Recycle(lastGeneratedItemInfo.GeneratorStartPosition, 1);
          RemoveInternalChildRange(InternalChildren.Count - 1, 1);
          lastGeneratedItemInfo = GetLastGeneratedItem();
          recycleItemsCount++;
        }
      }
      return recycleItemsCount;
    }

    /// <inheritdoc />
    public void SetVerticalOffset(double offset)
    {
      VerticalOffset = offset;
      InvalidateArrange();
    }

    /// <inheritdoc />
    public Rect MakeVisible(Visual visual, Rect rectangle) => throw new NotImplementedException();

    /// <inheritdoc />
    public bool CanVerticallyScroll { get; set; }

    /// <inheritdoc />
    public bool CanHorizontallyScroll { get; set; }

    /// <inheritdoc />
    public double ExtentWidth { get => _dimension.Width; }

    /// <inheritdoc />
    public double ExtentHeight { get => _dimension.Height; }

    /// <inheritdoc />
    public double ViewportWidth { get => _viewPort.Width; }

    /// <inheritdoc />
    public double ViewportHeight { get => _viewPort.Height; }

    /// <inheritdoc />
    public double HorizontalOffset { get; protected set; }

    /// <inheritdoc />
    public double VerticalOffset { get; protected set; }

    private ScrollViewer scrollOwner;
    /// <inheritdoc />
    public ScrollViewer ScrollOwner
    {
      get => scrollOwner;
      set
      {
        if (ScrollOwner != null)
        {
          ScrollOwner.SizeChanged -= OnScrollOwnerSizeChanged;
        }
        scrollOwner = value;
        if (ScrollOwner != null)
        {
          ScrollOwner.SizeChanged += OnScrollOwnerSizeChanged;
        }
      }
    }

    #endregion Implementation of IScrollInfo
    private bool IsScrolling { get; set; }
    private Rect _viewPort;
    private Rect _virtualizationBounds;
    private Rect _dimension;
    private readonly Rect _invalidRect;
    private bool IsExtentInvalid { get => _dimension == _invalidRect; }
    private bool IsLayoutInvalid { get => _viewPort == _invalidRect || IsExtentInvalid || IsVirtualizationBoundsInvalid; }
    private bool IsVirtualizationBoundsInvalid { get => _virtualizationBounds == _invalidRect; }
    private Size AverageItemSize { get; set; }
    private Point OriginOffset { get; set; }
    private CartesianChart Owner { get; set; }
    //private (int Index, UIElement ItemContainer) FirstViewportItem { get; set; } 
    //private (int Index, UIElement ItemContainer) LastViewportItem { get; set; }
    private ILookup<double, UIElement> ItemsLookupTable { get; set; }
    private IOrderedEnumerable<double> SortedItemPool { get; set; }
    private Dictionary<UIElement, UIElement> BeforeViewportDropOuts { get; set; }
    private Dictionary<UIElement, UIElement> AfterViewportDropOuts { get; set; }

  }
}
