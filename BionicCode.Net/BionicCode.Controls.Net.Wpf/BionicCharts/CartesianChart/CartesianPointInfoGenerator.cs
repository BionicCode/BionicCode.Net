﻿/* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
Before:
namespace BionicCode.Controls.Net.Wpf
After:
namespace BionicCode.Controls.Net.Wpf
*/

namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Windows;
  using System.Linq;
  using BionicCode.Utilities.Net;

  /* Unmerged change from project 'BionicCode.Controls.Net.Wpf (net6.0-windows)'
  Before:
    using System.Windows.Data;
  After:
    using System.Windows.Data;
      */
  using System.Windows.Data;
  using System.Collections.Generic;
  
  public class CartesianPointInfoGenerator : PointInfoGenerator<ICartesianSeriesInfo>
  {
    public CartesianPointInfoGenerator(CartesianChart owner) : base(owner)
    {
    }

    public override Rect GetValueBounds()
    {
      var valueBounds = new Size(Math.Abs(this.MinX.X) + Math.Abs(this.MaxX.X), Math.Abs(this.MinY.Y) + Math.Abs(this.MaxY.Y));
      return new Rect(valueBounds);
    }

    protected override ChartPanel CreateDefaultPanel() => new CartesianPanel(this);

    protected override Dictionary<IChartPointInfo, object> GeneratePointInfos(ICartesianSeriesInfo seriesInfo)
    {
      var points = new List<Point>();
      var pointToDataItemMap = new Dictionary<Point, object>();
      foreach (object dataItem in seriesInfo.Series)
      {
        var cartesianPoint = (Point)seriesInfo.PointConverter.Convert(dataItem);
        points.Add(cartesianPoint);
        pointToDataItemMap.Add(cartesianPoint, dataItem);
      }

      UpdateValueBoundaries(seriesInfo.MinX, seriesInfo.MaxX, seriesInfo.MinY, seriesInfo.MaxY);

      var drawingInfoToDataMap = new Dictionary<IChartPointInfo, object>();
      foreach (Point point in points)
      {
        var chartPointInfo = new CartesianChartPointInfo(point, seriesInfo);
        var dataItem = pointToDataItemMap[point];
        drawingInfoToDataMap.Add(chartPointInfo, dataItem);
      }

      return drawingInfoToDataMap;
      //this.PanelHost.InvalidateVisual();
    }

    private void UpdateValueBoundaries(Point minX,Point maxX, Point minY, Point maxY)
    {
      if (maxX.X > this.MaxX.X)
      {
        this.MaxX = maxX;
      }
      if (minX.X < this.MinX.X)
      {
        this.MinX = minX;
      }
      if (maxY.Y > this.MaxY.Y)
      {
        this.MaxY = maxY;
      }
      if (minY.Y < this.MinY.Y)
      {
        this.MinY = minY;
      }
    }

    public Point MaxX { get; private set; }
    public Point MinX { get; private set; }
    public Point MaxY { get; private set; }
    public Point MinY { get; private set; }
  }
}
