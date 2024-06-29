namespace BionicCode.Controls.Net.Wpf
{
  using System;
  using System.ComponentModel;

  public interface ICartesianChartPoint : INotifyPropertyChanged, IEquatable<ICartesianChartPoint>, IComparable<ICartesianChartPoint>
  {
    /// <summary>
    /// The x value that relates to the x-axis.
    /// </summary>
    double X { get; set; }

    /// <summary>
    /// The y value that relates to the y-axis
    /// </summary>
    double Y { get; set; }

    /// <summary>
    /// A description of the point.
    /// </summary>
    string Summary { get; set; }

    /// <summary>
    /// Property that can be used to store additional data objects with a chart point.
    /// </summary>
    object Data { get; set; }

    /// <summary>
    /// The identifier of a point series.
    /// </summary>
    object SeriesId { get; set; }
  }
}
