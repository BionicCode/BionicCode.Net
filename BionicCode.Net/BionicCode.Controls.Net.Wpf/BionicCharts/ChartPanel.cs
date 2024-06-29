namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections.Generic;
  using System.Windows;
  using System.Windows.Controls;
  using System.Windows.Controls.Primitives;
  using System.Windows.Media;

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
    using BionicCode.Utilities.Net;
  After:
        using BionicCode.Utilities.Net;
  */
  public abstract class ChartPanel : FrameworkElement, IScrollInfo
  {
    protected ChartPanel(PointInfoGenerator pointInfoGenerator)
    {
      this.Children = new VisualCollection(this);
      this.PlotInfos = new List<IChartPointInfo>();

      this.CanvasVisual = new DrawingVisual();
      AddChild(this.CanvasVisual);
      this.DrawingContext = this.CanvasVisual.RenderOpen();
      var invalidRect = new Rect(double.PositiveInfinity, double.PositiveInfinity, 0, 0);
      this.PlotCanvasBounds = invalidRect;
      this.ViewportBounds = invalidRect;
      this.VirtualBounds = invalidRect;
      this.PointInfoGenerator = pointInfoGenerator;
      this.Owner = pointInfoGenerator.Owner;
      this.IsPlotDataInvalid = true;
    }

    protected abstract (double XScaleFactor, double YScaleFactor) CalculateScaleFactors();

    public void InvalidatePlotData()
    {
      this.IsPlotDataInvalid = true;
      InvalidateRenderedData();

      this.PointInfoGenerator.Generate();
      InvalidatePlotDataOverride();
      InvalidateVisual();
    }

    public void ClearInvalidatePlotData() => this.IsPlotDataInvalid = false;

    protected void InvalidateRenderedData() => this.IsRenderedDataInvalid = true;
    private void ClearInvalidRenderedData()
    {
      this.IsRenderedDataInvalid = false;
      this.ScrollDelta = new Vector();
    }

    protected void InvalidateScrollInfo() => this.ScrollOwnerInternal?.InvalidateScrollInfo();

    public virtual void InvalidatePlotDataOverride()
    {
    }

    protected override Size MeasureOverride(Size availableSize)
    {
      Size newSize = base.MeasureOverride(availableSize);
      //if (availableSize.Height == double.PositiveInfinity)
      //{
      //  availableSize.Height = this.Owner.DesiredSize.Height;
      //}
      //if (availableSize.Width == double.PositiveInfinity)
      //{
      //  availableSize.Width = this.Owner.DesiredSize.Width;
      //}


      return this.VirtualBounds.Size;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
      if (finalSize != this.ViewportBounds.Size)
      {
        this.ViewportBounds = new Rect(finalSize);
        InvalidateRenderedData();
        ApplyDisplayMode();
        InvalidateScrollInfo();
      }

      if (this.IsPlotDataInvalid)
      {
        (double xZoomFactor, double yZoomFactor) = CalculateScaleFactors();
        this.VirtualBounds.Scale(xZoomFactor, yZoomFactor);
        ClearInvalidatePlotData();
      }
      this.PlotCanvasBounds = new Rect(finalSize);
      return base.ArrangeOverride(finalSize);
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);
      ClearInvalidRenderedData();
    }

    protected void ScrollViewport(Vector offset) => ScrollViewport(offset.X, offset.Y);

    private void ScrollViewport(double horizontalOffset, double verticalOffset)
    {
      Rect viewport = this.ViewportBounds;
      viewport.Offset(horizontalOffset, verticalOffset);
      this.ViewportBounds = viewport;

      Rect plotCanvas = this.PlotCanvasBounds;
      plotCanvas.Offset(horizontalOffset, verticalOffset);
      this.PlotCanvasBounds = viewport;

      InvalidateRenderedData();
    }

    //protected virtual void OnPanelScrolled(object sender, ScrollChangedEventArgs e) => InvalidatePlot();

    protected virtual void ApplyDisplayMode()
    {
      //double horizontalScalFactor = 1;
      //double verticalScalFactor = 1;
      switch (this.DisplayMode)
      {
        case DisplayMode.FitVerticalAxisToScreen:
          if (this.ScrollOwnerInternal == null)
          {
            break;
          }
          //verticalScalFactor = this.ViewportBounds.Height / this.VirtualBounds.Height;
          this.ScrollOwnerInternal.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
          this.ScrollOwnerInternal.HorizontalScrollBarVisibility = this.VirtualBounds.Width < this.ViewportBounds.Width
            ? ScrollBarVisibility.Disabled
            : ScrollBarVisibility.Visible;

          //ScrollViewer.SetHorizontalScrollBarVisibility(this.Owner, ScrollBarVisibility.Visible);
          //this.ScrollOwnerInternal.InvalidateScrollInfo();
          //this.ScrollOwnerInternal.UpdateLayout();
          //InvalidateMeasure();
          break;
      }

      //InvalidateScrollInfo();
      //this.VirtualBounds.Scale(horizontalScalFactor, verticalScalFactor);
    }

    public void AddPlotInfo(IChartPointInfo plotInfo)
    {
      this.PlotInfos.Add(plotInfo);
      InvalidatePlotData();
    }

    public void AddPlotInfoRange(IEnumerable<IChartPointInfo> plotInfo)
    {
      this.PlotInfos.AddRange(plotInfo);
      if (!this.IsPlotDataInvalid)
      {
        InvalidatePlotData();
      }
    }

    public void RemovedPlotInfo(IChartPointInfo plotInfo) => this.PlotInfos.Remove(plotInfo);
    public void ClearPlotInfos() => this.PlotInfos.Clear();
    public void AddChild(Visual visual) => this.Children.Add(visual);
    public void RemovedChild(Visual visual) => this.Children.Remove(visual);
    public void ClearChildren() => this.Children.Clear();

    protected DrawingVisual CanvasVisual { get; }
    protected DrawingContext DrawingContext { get; }
    protected PointInfoGenerator PointInfoGenerator { get; set; }
    protected Rect PlotCanvasBounds { get; set; }
    public Size PlotArea => this.PlotCanvasBounds.Size;
    protected Rect ViewportBounds { get; set; }
    protected Rect VirtualBounds { get; set; }
    public Size VirtualArea => this.VirtualBounds.Size;

    protected Vector ScrollDelta { get; private set; }
    public DisplayMode DisplayMode { get; set; }
    protected VisualCollection Children { get; set; }
    protected List<IChartPointInfo> PlotInfos { get; set; }
    protected Chart Owner { get; set; }
    private bool IsPlotDataInvalid { get; set; }
    protected bool IsRenderedDataInvalid { get; set; }
    protected bool IsScrollInfoInvalid { get; set; }
    private ScrollViewer ScrollOwnerInternal { get; set; }

    #region implementation of IScrollInfo

    private ScrollViewer scrollOwner;
    public ScrollViewer ScrollOwner
    {
      get => this.scrollOwner;
      set
      {
        if (this.ScrollOwner != null)
        {
          //this.ScrollOwner.ScrollChanged -= OnPanelScrolled;
        }

        this.ScrollOwnerInternal = value;
        ApplyDisplayMode();
        //InvalidateMeasure();

        if (!value.CanContentScroll)
        {
          return;
        }
        this.scrollOwner = value;
        //this.ScrollOwner.ScrollChanged += OnPanelScrolled;
      }
    }

    public abstract bool CanHorizontallyScroll { get; set; }
    public abstract bool CanVerticallyScroll { get; set; }
    public abstract double ExtentHeight { get; }
    public abstract double ExtentWidth { get; }
    public double HorizontalOffset { get; private set; }
    public abstract double VerticalOffset { get; }
    public abstract double ViewportHeight { get; }
    public abstract double ViewportWidth { get; }

    public abstract void LineDown();
    public abstract void LineLeft();
    public abstract void LineRight();
    public abstract void LineUp();
    public abstract Rect MakeVisible(Visual visual, Rect rectangle);
    public abstract void MouseWheelDown();
    public abstract void MouseWheelLeft();
    public abstract void MouseWheelRight();
    public abstract void MouseWheelUp();
    public abstract void PageDown();
    public abstract void PageLeft();
    public abstract void PageRight();
    public abstract void PageUp();

    public virtual void SetHorizontalOffset(double offset)
    {
      double scrollableWidth = this.ExtentWidth - this.ViewportWidth;
      double horizontalOffset = Math.Min(Math.Max(0, offset), scrollableWidth);
      this.ScrollDelta = new Vector(horizontalOffset - this.HorizontalOffset, 0);
      if (this.ScrollDelta.X == 0)
      {
        return;
      }

      this.HorizontalOffset = horizontalOffset;
      InvalidateScrollInfo();
      ScrollViewport(this.ScrollDelta);
      InvalidateVisual();
    }

    public abstract void SetVerticalOffset(double offset);
    #endregion
  }
}
