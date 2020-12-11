#region Info

// 2020/12/05  20:15
// BionicCode.Controls.Net.Framework.Wpf

#endregion

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BionicCode.Utilities.Net.Framework.Wpf.Extensions;

namespace BionicCode.Controls.Net.Framework.Wpf.BionicCalendar
{
  public class EventResizeAdorner : Adorner
  {
    private List<(Rect HitBox, Cursor Cursor)> HitBoxes { get; }
    private Cursor DefaultCursor { get; }
    private const double RenderRadius = 2.0;
    private const double HitTestPadding = 10.0;

    public event EventHandler Resizing;
    public event EventHandler Resized;
    public bool IsResizing { get; set; }

    // Be sure to call the base class constructor.
    public EventResizeAdorner(UIElement adornedElement)
      : base(adornedElement)
    {
      this.Loaded += Initialize;
      this.HitBoxes = new List<(Rect, Cursor)>();
      this.DefaultCursor = this.Cursor ?? Cursors.Arrow;
      //adornedElement.MouseEnter += (s, e) =>
      //    this.Visibility = Selector.GetIsSelected(adornedElement) ? Visibility.Visible : Visibility.Hidden;
      Selector.AddSelectedHandler(adornedElement, OnAdornedItemSelected);
      Selector.AddUnselectedHandler(adornedElement, OnAdornedItemUnselected);
    }

    private void OnAdornedItemUnselected(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Hidden;
    }

    private void OnAdornedItemSelected(object sender, RoutedEventArgs e)
    {
      this.Visibility = Visibility.Visible;
    }

    private void Initialize(object sender, RoutedEventArgs e)
    {
    }

    #region Overrides of UIElement

    ///// <inheritdoc />
    //protected override GeometryHitTestResult HitTestCore(GeometryHitTestParameters hitTestParameters) => new CombinedGeometry(GeometryCombineMode.Intersect, hitTestParameters.HitGeometry, new RectangleGeometry(new Rect(this.AdornedElement.DesiredSize))).GetOutlinedPathGeometry().IsEmpty() ? new GeometryHitTestResult(this, IntersectionDetail.Empty) : new GeometryHitTestResult(this, IntersectionDetail.Intersects);

    /// <inheritdoc />
    //protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
    //{
    //  var rect = new Rect(this.AdornedElement.DesiredSize);
    //  rect.Inflate(new Size(SimpleCircleAdorner.HitTestPadding, SimpleCircleAdorner.HitTestPadding));
    //  return rect.Contains(hitTestParameters.HitPoint) ? new PointHitTestResult(this, hitTestParameters.HitPoint) : new PointHitTestResult(this, new Point());
    //}

    /// <inheritdoc />
    protected override void OnMouseLeave(MouseEventArgs e)
    {
      base.OnMouseLeave(e);

      if (Mouse.LeftButton == MouseButtonState.Released)
      {
        this.IsResizing = false;
        return;
      }

      if (!this.IsResizing)
      {
        if (Mouse.LeftButton == MouseButtonState.Released && !Selector.GetIsSelected(this.AdornedElement))
        {
          this.Visibility = Visibility.Hidden;
        }

        return;
      }


      Point leavePosition = e.GetPosition(this);

      RaiseEvent(
        new SpanningRequestedRoutedEventArgs(
          Calendar.SpanningRequestedRoutedEvent,
          this,
          leavePosition.X > 0 ? ExpandDirection.Right : ExpandDirection.Left));

      if (this.AdornedElement.TryFindVisualParentElement(out CalendarPanel calendarPanel))
      {
        calendarPanel.SpanEventItemOnSpanningRequested(
          this.AdornedElement,
          new SpanningRequestedRoutedEventArgs(
            Calendar.SpanningRequestedRoutedEvent,
            this.AdornedElement,
            leavePosition.X > (this.AdornedElement as FrameworkElement).ActualWidth
              ? ExpandDirection.Right
              : ExpandDirection.Left));
      }
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonDown(e);
      Point currentMousePosition = e.GetPosition(this);
      //foreach ((Rect HitBox, Cursor Cursor) hitBoxData in this.HitBoxes)
      //{
      //  if (hitBoxData.HitBox.Contains(currentMousePosition))
      //  {
      //    this.Cursor = hitBoxData.Cursor;
      //    this.IsResizing = Mouse.LeftButton == MouseButtonState.Pressed;
      //    OnResizing();
      //  }
      //}
    }

    /// <inheritdoc />
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
      base.OnPreviewMouseLeftButtonUp(e);
      if (this.IsResizing)
      {
        this.IsResizing = false;
        OnResized();
      }
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
      base.OnMouseMove(e);
      if (this.IsResizing)
      {
        return;
      }

      Point currentMousePosition = e.GetPosition(this);
      foreach ((Rect HitBox, Cursor Cursor) hitBoxData in this.HitBoxes)
      {
        if (hitBoxData.HitBox.Contains(currentMousePosition))
        {
          this.Cursor = hitBoxData.Cursor;
          this.IsResizing = Mouse.LeftButton == MouseButtonState.Pressed;
          if (this.IsResizing)
          {
          OnResizing();
          }
          break;
        }
      }

      if (!this.IsResizing
          || !this.AdornedElement.TryFindVisualParentElement(out CalendarPanel calendarPanel))
      {
        this.Cursor = this.DefaultCursor;
        return;
      }

      if (currentMousePosition.X < (this.AdornedElement as FrameworkElement).ActualWidth -
        this.AdornedElement.DesiredSize.Width)
      {
        calendarPanel.SpanEventItemOnSpanningRequested(
          this.AdornedElement,
          new SpanningRequestedRoutedEventArgs(
            Calendar.SpanningRequestedRoutedEvent,
            this.AdornedElement,
            ExpandDirection.Left));
      }
      else if (currentMousePosition.X > (this.AdornedElement as FrameworkElement).ActualWidth + 20)
      {
        calendarPanel.SpanEventItemOnSpanningRequested(
          this.AdornedElement,
          new SpanningRequestedRoutedEventArgs(
            Calendar.SpanningRequestedRoutedEvent,
            this.AdornedElement,
            ExpandDirection.Right));
      }
    }

    #endregion

    //#region Overrides of FrameworkElement

    //#region Overrides of Adorner

    ///// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
      base.MeasureOverride(constraint);
      if (this.IsResizing)
      {
        //(this.AdornedElement as FrameworkElement).Height = constraint.Height;
        //(this.AdornedElement as FrameworkElement).Width = constraint.Width;
      }

      //this.AdornedElement.InvalidateMeasure();

      return constraint.Width == double.PositiveInfinity ? this.DesiredSize : constraint;
    }

    //#endregion

    ///// <inheritdoc />
    //protected override Size ArrangeOverride(Size finalSize)
    //{
    //    base.ArrangeOverride(finalSize);
    //    if (this.IsResizing)
    //    {
    //    }

    //    //this.AdornedElement?.Arrange(this.adornedElementRect);
    //    return finalSize;
    //}

    //#endregion

    protected override void OnRender(DrawingContext drawingContext)
    {
      this.HitBoxes.Clear();

      var transparentRenderBrush = new SolidColorBrush(Colors.Transparent);
      transparentRenderBrush.Freeze();
      var renderBrush = new SolidColorBrush(Colors.DimGray);
      renderBrush.Opacity = 0.5;
      renderBrush.Freeze();
      var transparentRenderPen = new Pen(new SolidColorBrush(Colors.Transparent), 1);
      transparentRenderPen.Freeze();
      var renderPen = new Pen(new SolidColorBrush(Colors.DimGray), 1);
      renderPen.Freeze();
      var dashedRenderPen = new Pen(new SolidColorBrush(Colors.DimGray), 0.5);
      dashedRenderPen.DashStyle = DashStyles.Dot;
      dashedRenderPen.Freeze();

      var adornedElementRect = new Rect(
        0,
        0,
        Math.Max(
          (this.AdornedElement as FrameworkElement).ActualWidth,
          this.AdornedElement.DesiredSize.Width),
        Math.Max(
          (this.AdornedElement as FrameworkElement).ActualHeight,
          this.AdornedElement.DesiredSize.Height));

      var horizontalResizeGripHitBox = new Rect(
        new Size(
          Math.Max(0, adornedElementRect.Width - EventResizeAdorner.HitTestPadding),
          Math.Max(0, adornedElementRect.Height - EventResizeAdorner.HitTestPadding)));
      horizontalResizeGripHitBox.Height = EventResizeAdorner.HitTestPadding;
      horizontalResizeGripHitBox.Offset(
        EventResizeAdorner.HitTestPadding / 2,
        -EventResizeAdorner.HitTestPadding);
      var verticalResizeGripHitBox = new Rect(
        new Size(
          Math.Max(0, adornedElementRect.Width - EventResizeAdorner.HitTestPadding),
          Math.Max(0, adornedElementRect.Height - EventResizeAdorner.HitTestPadding)));
      verticalResizeGripHitBox.Width = EventResizeAdorner.HitTestPadding;
      verticalResizeGripHitBox.Offset(-EventResizeAdorner.HitTestPadding, EventResizeAdorner.HitTestPadding / 2);

      /* Visible resize grip lines.
             Draws a rectangle around the adorned element */
      drawingContext.DrawLine(dashedRenderPen, adornedElementRect.TopLeft, adornedElementRect.TopRight);
      drawingContext.DrawLine(dashedRenderPen, adornedElementRect.TopRight, adornedElementRect.BottomRight);
      drawingContext.DrawLine(dashedRenderPen, adornedElementRect.BottomRight, adornedElementRect.BottomLeft);
      drawingContext.DrawLine(dashedRenderPen, adornedElementRect.BottomLeft, adornedElementRect.TopLeft);

      /* Hidden hit box rectangles, which includes some padding (interaction area).
             Draws a rectangle around the adorned element. */
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, horizontalResizeGripHitBox);
      ////this.HitBoxes.Add((horizontalResizeGripHitBox, Cursors.SizeNS));
      //horizontalResizeGripHitBox.Offset(0, adornedElementRect.Height + EventResizeAdorner.HitTestPadding);
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, horizontalResizeGripHitBox);
      ////this.HitBoxes.Add((horizontalResizeGripHitBox, Cursors.SizeNS));

      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, verticalResizeGripHitBox);
      //this.HitBoxes.Add((verticalResizeGripHitBox, Cursors.SizeWE));
      //verticalResizeGripHitBox.Offset(adornedElementRect.Width + EventResizeAdorner.HitTestPadding, 0);
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, verticalResizeGripHitBox);
      //this.HitBoxes.Add((verticalResizeGripHitBox, Cursors.SizeWE));

      /* Draw a resize grip circle at each corner. Surround it with a hidden hit box rectangle,
             which includes some padding (interaction area) */

      // TopLeft resize grip circle
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopLeft, EventResizeAdorner.RenderRadius, EventResizeAdorner.RenderRadius);

      // TopLeft hit box rectangle
      Point paddingPoint = adornedElementRect.TopLeft;
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      var cornerResizeGripHitBox = new Rect(
        paddingPoint,
        new Size(
          EventResizeAdorner.RenderRadius + EventResizeAdorner.HitTestPadding,
          EventResizeAdorner.RenderRadius + EventResizeAdorner.HitTestPadding));
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      //this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeNWSE));

      // TopCenter resize grip circle
      Point centerPoint = adornedElementRect.TopLeft;
      centerPoint.Offset(adornedElementRect.Width / 2, 0);
      //drawingContext.DrawEllipse(renderBrush, renderPen, centerPoint, EventResizeAdorner.RenderRadius, EventResizeAdorner.RenderRadius);

      // TopCenter hit box rectangle
      paddingPoint = centerPoint;
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      //this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeNS));

      // TopRight resize grip circle
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.TopRight, EventResizeAdorner.RenderRadius, EventResizeAdorner.RenderRadius);
      paddingPoint = adornedElementRect.TopRight;

      // TopRight hit box rectangle
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      //this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeNESW));

      // RightCenter resize grip circle
      centerPoint = adornedElementRect.TopRight;
      centerPoint.Offset(0, adornedElementRect.Height / 2);
      drawingContext.DrawEllipse(
        renderBrush,
        renderPen,
        centerPoint,
        EventResizeAdorner.RenderRadius,
        EventResizeAdorner.RenderRadius);

      // RightCenter hit box rectangle
      paddingPoint = centerPoint;
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeWE));

      // BottomRight resize grip circle
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomRight, EventResizeAdorner.RenderRadius, EventResizeAdorner.RenderRadius);
      paddingPoint = adornedElementRect.BottomRight;

      // BottomRight hit box rectangle
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeNWSE));

      // BottomCenter resize grip circle
      centerPoint = adornedElementRect.BottomRight;
      centerPoint.Offset(-adornedElementRect.Width / 2, 0);
      //drawingContext.DrawEllipse(renderBrush, renderPen, centerPoint, EventResizeAdorner.RenderRadius, EventResizeAdorner.RenderRadius);

      // BottomCenter hit box rectangle
      paddingPoint = centerPoint;
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      //this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeNS));

      // BottomLeft resize grip circle
      //drawingContext.DrawEllipse(renderBrush, renderPen, adornedElementRect.BottomLeft, EventResizeAdorner.RenderRadius, EventResizeAdorner.RenderRadius);

      // BottomLeft hit box rectangle
      paddingPoint = adornedElementRect.BottomLeft;
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      //drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      //this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeNESW));

      // LeftCenter resize grip circle
      centerPoint = adornedElementRect.BottomLeft;
      centerPoint.Offset(0, -adornedElementRect.Height / 2);
      drawingContext.DrawEllipse(
        renderBrush,
        renderPen,
        centerPoint,
        EventResizeAdorner.RenderRadius,
        EventResizeAdorner.RenderRadius);

      // LeftCenter hit box rectangle
      paddingPoint = centerPoint;
      paddingPoint.Offset(
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2,
        -EventResizeAdorner.HitTestPadding / 2 - EventResizeAdorner.RenderRadius / 2);
      cornerResizeGripHitBox.Location = paddingPoint;
      drawingContext.DrawRectangle(transparentRenderBrush, transparentRenderPen, cornerResizeGripHitBox);
      this.HitBoxes.Add((cornerResizeGripHitBox, Cursors.SizeWE));
      this.Visibility = Selector.GetIsSelected(this.AdornedElement) ? Visibility.Visible : Visibility.Hidden;
    }

    protected virtual void OnResizing() => this.Resizing?.Invoke(this, EventArgs.Empty);

    protected virtual void OnResized() => this.Resized?.Invoke(this, EventArgs.Empty);
  }
}