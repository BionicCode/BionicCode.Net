namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;

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
    public static IList<CartesianPoint> NormDist(double mean, double standardDeviation, double resolution)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than '0'.", nameof(standardDeviation));
      }

      return EnumerateNormDist(mean, standardDeviation, resolution).ToList();
    }

    /// <summary>
    /// Calculates the normal distribution for a given x value.
    /// </summary>
    /// <param name="mean">The arithmetic mean of the data series.</param>
    /// <param name="standardDeviation">The sigma i.e. standard deviation of the data series.</param>
    /// <br/>The smallest absolute value of the range is multiplied with the <paramref name="resolution"/> value.</param>
    /// <returns>The data set of x/y points. Can be used to plot the graph.</returns>
    public static CartesianPoint NormDistOf(double x, double mean, double standardDeviation)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than '0'.", nameof(standardDeviation));
      }

      double y = (1 / (standardDeviation * System.Math.Sqrt(2 * System.Math.PI)))
        * System.Math.Exp(-0.5 * System.Math.Pow((x - mean) / standardDeviation, 2));
      var point = new CartesianPoint(x, y);

      return point;
    }

    /// <summary>
    /// Calculates the normal distribution and retuns an enumerable collection. The interval used is [µ-5*σ...µ+5*σ], where µ=mean and σ=standard deviation.
    /// </summary>
    /// <param name="mean">The arithmetic mean of the data series.</param>
    /// <param name="standardDeviation">The sigma i.e. standard deviation of the data series.</param>
    /// <param name="resolution">The factor to calculate the increments of the 'x' values used to calculate the normal distribution. 
    /// <br/>The smallest absolute value of the range is multiplied with the <paramref name="resolution"/> value.</param>
    /// <returns>The data set of x/y points. Can be used to plot the graph.</returns>
    /// <remarks><see cref="EnumerateNormDist(double, double, double)"/> and <see cref="NormDist(double, double, double)"/> differ as follows: When you use <see cref="EnumerateNormDist(double, double, double)"/>, you can start enumerating the collection of cartesian points before the whole collection is returned. 
    /// <br/>When you use <see cref="NormDist(double, double, double)"/>, you must wait for the whole array of cartesian points to be returned before you can access the array. 
    /// <br/>Therefore, when you are expecting a huge spread of values i.e. a big standard deviation value for <paramref name="standardDeviation"/>, <see cref="EnumerateNormDist(double, double, double)"/> can be more efficient.</remarks>
    public static IEnumerable<CartesianPoint> EnumerateNormDist(double mean, double standardDeviation, double resolution = 0.1)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than 'ß'.", nameof(standardDeviation));
      }

      double sigma0 = mean;
      double sigma1 = mean + standardDeviation;
      double sigma2 = mean + 2 * standardDeviation;
      double sigma3 = mean + 3 * standardDeviation;
      double sigma1Negative = mean - standardDeviation;
      double sigma2Negative = mean - 2 * standardDeviation;
      double sigma3Negative = mean - 3 * standardDeviation;

      var edgeCases = new List<double> { sigma3Negative, sigma2Negative, sigma1Negative, sigma0, sigma1, sigma2, sigma3 };

      double rangeStart = mean - 5 * standardDeviation;
      double rangeEnd = mean + 5 * standardDeviation;
      double increment = standardDeviation * resolution;
      double oldX = rangeStart;
      for (double x = rangeStart; x <= rangeEnd; x += increment)
      {
        double y = (1 / (standardDeviation * System.Math.Sqrt(2 * System.Math.PI)))
          * System.Math.Exp(-0.5 * System.Math.Pow((x - mean) / standardDeviation, 2));
        var point = new CartesianPoint(x, y);

        for (int index = 0; index < edgeCases.Count; index++)
        {
          double edgeX = edgeCases[index];
          if (TryGetEdgePoint(x, oldX, edgeX, mean, standardDeviation, out CartesianPoint edgePoint))
          {
            yield return edgePoint;
            oldX = edgeX;
            edgeCases.RemoveAt(index);
            break;
          }
        }

        oldX = x;
        yield return point;
      }
    }

    private static bool TryGetEdgePoint(double newX, double oldX, double edgeX, double mean, double standardDeviation, out CartesianPoint edgeXPoint)
    {
      edgeXPoint = default;

      if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);

        return true;
      }
      else if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);
        return true;
      }
      else if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);

        return true;
      }
      else if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);

        return true;
      }
      else if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);

        return true;
      }
      else if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);

        return true;
      }
      else if (newX > edgeX && oldX < edgeX)
      {
        edgeXPoint = NormDistOf(edgeX, mean, standardDeviation);

        return true;
      }

      return false;
    }
  }
}
