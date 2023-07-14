namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Windows;
  using System.Linq;
  using System.Windows.Media;
    
  public readonly struct CartesianSeriesInfo<TData> : ISeriesInfo<TData, Point>, ICartesianSeriesInfo
  {
    public CartesianSeriesInfo(IEnumerable<TData> series, Pen pen, ICartesianDataConverter<TData> pointConverter)
    {
      this.Series = series;
      this.Pen = pen;
      this.PointConverter = pointConverter;
      this.SeriesBoundaryPoints = new Lazy<(Point MinX, Point MaxX, Point MinY, Point MaxY)>(() => CalculateBoundaryPoints(series, pointConverter));
    }

    public CartesianSeriesInfo(IEnumerable<TData> series, Brush color, double thickness, ICartesianDataConverter<TData> pointConverter) 
      : this(series, new Pen(color, thickness), pointConverter)
    {
    }

    public CartesianSeriesInfo(IEnumerable<TData> series, Pen pen)
    {
      if (typeof(TData) != typeof(Point)
        && typeof(TData) != typeof(ICartesianChartPoint))
      {
        throw new InvalidCastException($"No default conversion available for data type {typeof(TData)}. Please use a different overload and provide a custom ICartesianDataConverter<T> implementation. Alternatively, ensure that the item type is of type {typeof(Point)} or implements {typeof(ICartesianChartPoint)}.");
      }

      this.Series = series;
      this.Pen = pen;
      var cartesianDataConverter = CartesianDataConverter<TData>.Default;
      this.PointConverter = cartesianDataConverter;
      this.SeriesBoundaryPoints = new Lazy<(Point MinX, Point MaxX, Point MinY, Point MaxY)>(() => CalculateBoundaryPoints(series, cartesianDataConverter));
    }

    public CartesianSeriesInfo(IEnumerable<TData> series, Brush color, double thickness)
      : this(series, new Pen(color, thickness))
    {
    }

    private static (Point MinX, Point MaxX, Point MinY, Point MaxY) CalculateBoundaryPoints(IEnumerable<TData> series, ICartesianDataConverter<TData> pointConverter)
    {
      Point maxX;
      Point minX;
      Point maxY;
      Point minY;
      foreach (TData dataItem in series)
      {
        Point cartesianPoint = pointConverter.Convert(dataItem);
        if (cartesianPoint.X > maxX.X)
        {
          maxX = cartesianPoint;
        }
        if (cartesianPoint.X < minX.X)
        {
          minX = cartesianPoint;
        }
        if (cartesianPoint.Y > maxY.Y)
        {
          maxY = cartesianPoint;
        }
        if (cartesianPoint.Y < minY.Y)
        {
          minY = cartesianPoint;
        }
      }

      return (minX, maxX, minY, maxY);
    }

    public IEnumerable<TData> Series { get; }
    public Pen Pen { get; }
    public ICartesianDataConverter<TData> PointConverter { get; }
    IEnumerable ISeriesInfo.Series => this.Series;
    IDataConverter<TData, Point> ISeriesInfo<TData, Point>.PointConverter => this.PointConverter;
    IDataConverter ISeriesInfo.PointConverter => this.PointConverter;
    private Lazy<(Point MinX, Point MaxX, Point MinY, Point MaxY)> SeriesBoundaryPoints { get; }
    public Point MinY => this.SeriesBoundaryPoints.Value.MinY;
    public Point MaxY => this.SeriesBoundaryPoints.Value.MaxY;
    public Point MaxX => this.SeriesBoundaryPoints.Value.MaxX;
    public Point MinX => this.SeriesBoundaryPoints.Value.MinX;
  }
}
