namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;

  /// <summary>
  /// Additional mth functions
  /// </summary>
  public static class Math
  {
    /// <summary>
    /// Calculates the normal distribution. The interval used is [µ-5*σ...µ+5*σ], where µ=mean and σ=standard deviation.
    /// </summary>
    /// <param name="mean">The arithmetic mean of the data series.</param>
    /// <param name="standardDeviation">The sigma i.e. standard deviation of the data series.</param>
    /// <param name="resolution">The factor to calculate the increments of the 'x' values used to calculate the normal distribution. 
    /// <br/>The smallest absolute value of the range is multiplied with the <paramref name="resolution"/> value.</param>
    /// <returns>The data set of x/y points. Can be used to plot the graph.</returns>
    public static IEnumerable<CartesianPoint> NormDist(double mean, double standardDeviation, double resolution)
    {
      double rangeStart = mean - 5 * standardDeviation;
      double rangeEnd = mean + 5 * standardDeviation;
      double increment = System.Math.Min(System.Math.Abs(rangeStart), System.Math.Abs(rangeEnd)) * resolution;
      var results = new List<CartesianPoint>();
      for (double x = rangeStart; x <= rangeEnd; x += increment)
      {
        double y = (1 / (standardDeviation * System.Math.Sqrt(2 * System.Math.PI)))
          * System.Math.Exp(-0.5 * System.Math.Pow((x - mean) / standardDeviation, 2));
        var point = new CartesianPoint(x, y);
        results.Add(point);
      }

      return results;
    }
  }
}
