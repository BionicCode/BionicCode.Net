namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Media;
  using System.Linq;

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
    using BionicCode.Utilities.Net;
  After:
        using BionicCode.Utilities.Net;
  */
  internal class CartesianPanel : ChartPanel
  {
    public CartesianPanel(CartesianPointInfoGenerator pointInfoGenerator) : base(pointInfoGenerator)
    {
      this.Items = new List<CartesianChartPointInfo>();
      this.OriginalPointInfoLookupTable = new Dictionary<CartesianChartPointInfo, CartesianChartPointInfo>();
      this.TransformedPointInfoLookupTable = new Dictionary<CartesianChartPointInfo, CartesianChartPointInfo>();
    }

    protected override void ApplyDisplayMode()
    {
      base.ApplyDisplayMode();
    }

    public override void InvalidatePlotDataOverride()
    {
      base.InvalidatePlotDataOverride();
      if (!this.PlotInfos.Any())
      {
        return;
      }
      this.OriginalPointInfoLookupTable.Clear();
      this.TransformedPointInfoLookupTable.Clear();
      var cartesianPointInfoGenerator = (CartesianPointInfoGenerator)this.PointInfoGenerator;
      this.VirtualBounds = cartesianPointInfoGenerator.GetValueBounds();
      //this.OriginOffset = new Point(0, Math.Abs(plotInfo.MinY.Y));
      //this.VirtualBounds.Offset(this.OriginOffset.X, this.OriginOffset.Y);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      base.MeasureOverride(availableSize);
      return new Size(0, 0);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      base.ArrangeOverride(finalSize);

      if (this.IsRenderedDataInvalid)
      {
        var pointInfos = this.PlotInfos
          .Cast<CartesianChartPointInfo>();

        this.Items.Clear();

        foreach (var originalPointInfo in pointInfos)
        {
          if (!this.TransformedPointInfoLookupTable.TryGetValue(originalPointInfo, out CartesianChartPointInfo transformedPointInfo))
          {
            transformedPointInfo = TransformPointInfos(originalPointInfo);
            this.OriginalPointInfoLookupTable.Add(transformedPointInfo, originalPointInfo);
            this.TransformedPointInfoLookupTable.Add(originalPointInfo, transformedPointInfo);
          }

          if (this.PlotCanvasBounds.Contains(transformedPointInfo.CartesianChartPoint))
          {
            this.Items.Add(transformedPointInfo);
          }
        }
      }
      return finalSize;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      if (this.HasNoItems || !this.IsRenderedDataInvalid)
      {
        return;
      }

      CartesianChartPointInfo previousPointInfo = this.Items.First();
      foreach (CartesianChartPointInfo pointInfo in this.Items)
      {
        Point previousPoint = previousPointInfo.CartesianChartPoint;
        previousPoint.Offset(-this.HorizontalOffset, -this.VerticalOffset);

        Point nextPoint = pointInfo.CartesianChartPoint;
        nextPoint.Offset(-this.HorizontalOffset, -this.VerticalOffset);

        drawingContext.DrawLine(pointInfo.SeriesInfo.Pen, previousPoint, nextPoint);
        previousPointInfo = pointInfo;
      }

      base.OnRender(drawingContext);
    }

    private CartesianChartPointInfo TransformPointInfos(CartesianChartPointInfo originalPointInfo)
    {
      (double xZoomFactor, double yZoomFactor) = CalculateScaleFactors();
      var yPositivAxisLimit = ((CartesianPointInfoGenerator)this.PointInfoGenerator).MaxY.Y;
      var transformedPoint = originalPointInfo.CartesianChartPoint
        .ToPointOnScreen(yPositivAxisLimit) 
        .Scale(xZoomFactor, yZoomFactor);
      transformedPoint.Offset(-this.HorizontalOffset, -this.VerticalOffset);
      var transformedPointInfo = new CartesianChartPointInfo(transformedPoint, originalPointInfo.SeriesInfo);
      return transformedPointInfo;
    }

    protected override (double XScaleFactor, double YScaleFactor) CalculateScaleFactors()
    {
      (double XScaleFactor, double YScaleFactor) result;

      ZoomFactor xZoomFactor = this.Owner.XZoomFactor;
      result.XScaleFactor = xZoomFactor.Unit switch
      {
        ZoomFactorUnit.Auto => CalculateXAxisAutoScaleFactor(),
        ZoomFactorUnit.Pixel => xZoomFactor.Value,
        _ => 1
      };

      ZoomFactor yZoomFactor = this.Owner.YZoomFactor;
      result.YScaleFactor = yZoomFactor.Unit switch
      {
        ZoomFactorUnit.Auto => CalculateYAxisAutoScaleFactor(),
        ZoomFactorUnit.Pixel => yZoomFactor.Value,
        _ => 1
      };

      return result;
    }

    private double CalculateXAxisAutoScaleFactor()
    {
      if (this.VirtualBounds.Width >= this.PlotCanvasBounds.Width)
      {
        return 1.0;
      }

      double plotAreaWidth = this.PlotCanvasBounds.Width;
      double virtualAreaWidth = this.VirtualBounds.Width;
      double xScaleRatio = plotAreaWidth / virtualAreaWidth is 0.0 ? 1 : virtualAreaWidth;
      return xScaleRatio;
    }

    private double CalculateYAxisAutoScaleFactor()
    {
      (double XScaleFactor, double YScaleFactor) result;

      double plotAreaHeight = this.PlotCanvasBounds.Height;
      double virtualAreaHeight = this.VirtualBounds.Height;
      double yScaleRatio = plotAreaHeight / (virtualAreaHeight is 0.0 ? 1 : virtualAreaHeight);
      return yScaleRatio;
    }

    public void AddPlotInfo(CartesianChartPointInfo plotInfo) => AddPlotInfo(plotInfo);
    public void AddPlotInfoRange(IEnumerable<CartesianChartPointInfo> plotInfo) => AddPlotInfoRange(plotInfo);

    private Point OriginOffset { get; set; }

    private List<CartesianChartPointInfo> Items { get; }
    private Dictionary<CartesianChartPointInfo, CartesianChartPointInfo> OriginalPointInfoLookupTable { get; }
    private Dictionary<CartesianChartPointInfo, CartesianChartPointInfo> TransformedPointInfoLookupTable { get; }
    private bool HasNoItems => !this.Items.Any();
    private bool HasItems => this.Items.Any();

    public override bool CanHorizontallyScroll { get; set; }
    public override bool CanVerticallyScroll { get; set; }
    public override double ExtentHeight => this.VirtualBounds.Height;
    public override double ExtentWidth => this.VirtualBounds.Width;
    public override double VerticalOffset { get; }
    public override double ViewportHeight => this.ViewportBounds.Height;
    public override double ViewportWidth => this.ViewportBounds.Width;

    public override void LineDown() => throw new NotImplementedException();
    public override void LineLeft() => throw new NotImplementedException();
    public override void LineRight() => throw new NotImplementedException();
    public override void LineUp() => throw new NotImplementedException();
    public override Rect MakeVisible(Visual visual, Rect rectangle) => throw new NotImplementedException();
    public override void MouseWheelDown() => throw new NotImplementedException();
    public override void MouseWheelLeft() => throw new NotImplementedException();
    public override void MouseWheelRight() => throw new NotImplementedException();
    public override void MouseWheelUp() => throw new NotImplementedException();
    public override void PageDown() => throw new NotImplementedException();
    public override void PageLeft() => throw new NotImplementedException();
    public override void PageRight() => throw new NotImplementedException();
    public override void PageUp() => throw new NotImplementedException();
    //public override void SetHorizontalOffset(double offset) => throw new NotImplementedException();
    public override void SetVerticalOffset(double offset) => throw new NotImplementedException();
  }
}
