namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
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
    /// <returns>The data set of x,y points. Can be used to plot the graph.
    /// <br/>The data set always contains the mean µ (0σ), ±1σ, ±2σ, ±3σ.</returns>
    public static IList<CartesianPoint> NormDist(double mean, double standardDeviation, double resolution)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than '0'.", nameof(standardDeviation));
      }

      return EnumerateNormDist(mean, standardDeviation, resolution).ToList();
    }

    /// <summary>
    /// Calculates the normal distribution. The interval used is [µ-5*σ...µ+5*σ], where µ=mean and σ=standard deviation.
    /// </summary>
    /// <param name="mean">The arithmetic mean of the data series.</param>
    /// <param name="standardDeviation">The sigma i.e. standard deviation of the data series.</param>
    /// <param name="resolution">The factor to calculate the increments of the 'x' values used to calculate the normal distribution. 
    /// <br/>The smallest absolute value of the range is multiplied with the <paramref name="resolution"/> value.</param>
    /// <param name="existingXValues">Existing x values that the function should explicitly calculate the result from. Those values are merged nto the normal distributation.</param>
    /// <returns>The data set of x,y points. Can be used to plot the graph. 
    /// <br/>The data set always contains the mean µ (0σ), ±1σ, ±2σ, ±3σ.</returns>
    public static IList<CartesianPoint> NormDist(double mean, double standardDeviation, double resolution, IEnumerable<double> existingXValues)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than '0'.", nameof(standardDeviation));
      }

      return EnumerateNormDistInternal(mean, standardDeviation, resolution, existingXValues ?? Enumerable.Empty<double>()).ToList();
    }

    /// <summary>
    /// Calculates the normal distribution for a given x value.
    /// </summary>
    /// <param name="mean">The arithmetic mean of the data series.</param>
    /// <param name="standardDeviation">The sigma i.e. standard deviation of the data series.</param>
    /// <br/>The smallest absolute value of the range is multiplied with the <paramref name="resolution"/> value.</param>
    /// <returns>The x,y value pair.</returns>
    public static CartesianPoint NormDistOf(double x, double mean, double standardDeviation)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than '0'.", nameof(standardDeviation));
      }

      double y = 1 / (standardDeviation * System.Math.Sqrt(2 * System.Math.PI))
        * System.Math.Exp(-0.5 * System.Math.Pow((x - mean) / standardDeviation, 2));
      var point = new CartesianPoint(x, y);

      return point;
    }

    /// <summary>
    /// Calculates the normal distribution and returns an enumerable collection. The interval used is [µ-5*σ...µ+5*σ], where µ=mean and σ=standard deviation.
    /// </summary>
    /// <param name="mean">The arithmetic mean of the data series.</param>
    /// <param name="standardDeviation">The sigma i.e. standard deviation of the data series.</param>
    /// <param name="resolution">The factor to calculate the increments of the 'x' values used to calculate the normal distribution. 
    /// <br/>The smallest absolute value of the range is multiplied with the <paramref name="resolution"/> value.</param>
    /// <returns>The data set of x,y points. Can be used to plot the graph.
    /// <br/>The data set always contains the mean µ (0σ), ±1σ, ±2σ, ±3σ.</returns>
    /// <remarks><see cref="EnumerateNormDist(double, double, double)"/> and <see cref="NormDist(double, double, double)"/> differ as follows: When you use <see cref="EnumerateNormDist(double, double, double)"/>, you can start enumerating the collection of cartesian points before the whole collection is returned. 
    /// <br/>When you use <see cref="NormDist(double, double, double)"/>, you must wait for the whole array of Cartesian points to be returned before you can access the array. 
    /// <br/>Therefore, when you are expecting a huge spread of values i.e. a big standard deviation value for <paramref name="standardDeviation"/>, <see cref="EnumerateNormDist(double, double, double)"/> can be more efficient.</remarks>
    public static IEnumerable<CartesianPoint> EnumerateNormDist(double mean, double standardDeviation, double resolution = 0.1)
      => EnumerateNormDistInternal(mean, standardDeviation, resolution, Enumerable.Empty<double>());

    internal static IEnumerable<CartesianPoint> EnumerateNormDistInternal(double mean, double standardDeviation, double resolution, IEnumerable<double> existingDataSet)
    {
      if (standardDeviation == 0)
      {
        throw new ArgumentException("Value must be less or greater than 'ß'.", nameof(standardDeviation));
      }

      double sigma0 = mean;
      double sigma1 = mean + standardDeviation;
      double sigma2 = mean + (2 * standardDeviation);
      double sigma3 = mean + (3 * standardDeviation);
      double sigma1Negative = mean - standardDeviation;
      double sigma2Negative = mean - (2 * standardDeviation);
      double sigma3Negative = mean - (3 * standardDeviation);

      var sortedEdgeCases = new List<(bool IsExistingDataSetValue, IndexedNumber IndexedNumber)>(existingDataSet.Select((value, index) => (true, new IndexedNumber(value, index)))) { (false, new IndexedNumber(sigma3Negative)), (false, new IndexedNumber(sigma2Negative)), (false, new IndexedNumber(sigma1Negative)), (false, new IndexedNumber(sigma0)), (false, new IndexedNumber(sigma1)), (false, new IndexedNumber(sigma2)), (false, new IndexedNumber(sigma3)) };
      sortedEdgeCases.Sort(Comparer<(bool IsExistingDataSetValue, IndexedNumber IndexedNumber)>.Create((item1, item2) => item1.IndexedNumber.CompareTo(item2.IndexedNumber)));

      double rangeStart = System.Math.Min(sortedEdgeCases.First().IndexedNumber, mean - (5 * standardDeviation));
      double rangeEnd = System.Math.Max(sortedEdgeCases.Last().IndexedNumber, mean + (5 * standardDeviation));
      double increment = standardDeviation * resolution;
      double oldX = rangeStart;
      for (double x = rangeStart; x <= rangeEnd; x += increment)
      {
        double y = 1 / (standardDeviation * System.Math.Sqrt(2 * System.Math.PI))
          * System.Math.Exp(-0.5 * System.Math.Pow((x - mean) / standardDeviation, 2));
        var point = new CartesianPoint(x, y);

        //for (int index = 0; index < sortedEdgeCases.Count; index++)
        while (sortedEdgeCases.Any())
        {
          (bool IsExistingDataSetValue, IndexedNumber IndexedValue) edgeX = sortedEdgeCases.First();
          if (TryGetEdgePoint(x, edgeX, mean, standardDeviation, out CartesianPoint edgePoint))
          {
            yield return edgePoint;
            oldX = edgeX.IndexedValue;
            sortedEdgeCases.RemoveAt(0);
            continue;
          }

          break;
        }

        // The current x value is the same as the last edge case value. Skip to avoid duplicates.
        if (x == oldX)
        {
          continue;
        }

        oldX = x;
        yield return point;
      }
    }

    private static bool TryGetEdgePoint(double newX, (bool IsExistingDataSetValue, IndexedNumber IndexedNumber) edgeX, double mean, double standardDeviation, out CartesianPoint edgeXPoint)
    {
      edgeXPoint = default;

      if (newX >= edgeX.IndexedNumber)
      {
        CartesianPoint result = NormDistOf(edgeX.IndexedNumber, mean, standardDeviation);
        edgeXPoint = edgeX.IsExistingDataSetValue
          ? new CartesianPoint(result.X, result.Y, edgeX.IsExistingDataSetValue, edgeX.IndexedNumber.Index)
          : result;

        return true;
      }

      return false;
    }
  }

  internal struct IndexedNumber : IComparable<IndexedNumber>, IEquatable<IndexedNumber>
  {
    public IndexedNumber(double value, int index)
    {
      this.Value = value;
      this.Index = index;
    }

    public IndexedNumber(double value)
    {
      this.Value = value;
      this.Index = -1;
    }

    public double Value { get; private set; }
    public int Index { get; private set; }

    public bool Equals(IndexedNumber other) => this.Value.Equals(other.Value) && this.Index.Equals(other.Index);

    public override int GetHashCode()
    {
      int hashCode = 995152453;
      hashCode = (hashCode * -1521134295) + this.Value.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.Index.GetHashCode();
      return hashCode;
    }

    public override bool Equals(object obj) => obj is IndexedNumber indexedNumber && Equals(indexedNumber);
    public override string ToString() => $"Value: {this.Value}; Index: {this.Index}";
    public int CompareTo(IndexedNumber other) => this.Value.CompareTo(other.Value);

    public static bool operator <(IndexedNumber left, IndexedNumber right) => left.CompareTo(right) < 0;
    public static bool operator <=(IndexedNumber left, IndexedNumber right) => left.CompareTo(right) <= 0;
    public static bool operator >(IndexedNumber left, IndexedNumber right) => left.CompareTo(right) > 0;
    public static bool operator >=(IndexedNumber left, IndexedNumber right) => left.CompareTo(right) >= 0;
    public static bool operator ==(IndexedNumber left, IndexedNumber right) => left.Equals(right);
    public static bool operator !=(IndexedNumber left, IndexedNumber right) => !left.Equals(right);
    public static bool operator <(IndexedNumber left, double right) => left.Value.CompareTo(right) < 0;
    public static bool operator <=(IndexedNumber left, double right) => left.Value.CompareTo(right) <= 0;
    public static bool operator >(IndexedNumber left, double right) => left.Value.CompareTo(right) > 0;
    public static bool operator >=(IndexedNumber left, double right) => left.Value.CompareTo(right) >= 0;
    public static bool operator ==(IndexedNumber left, double right) => left.Value.Equals(right);
    public static bool operator !=(IndexedNumber left, double right) => !left.Value.Equals(right);

    public static bool operator <(double left, IndexedNumber right) => left.CompareTo(right.Value) < 0;
    public static bool operator <=(double left, IndexedNumber right) => left.CompareTo(right.Value) <= 0;
    public static bool operator >(double left, IndexedNumber right) => left.CompareTo(right.Value) > 0;
    public static bool operator >=(double left, IndexedNumber right) => left.CompareTo(right.Value) >= 0;
    public static bool operator ==(double left, IndexedNumber right) => left.Equals(right.Value);
    public static bool operator !=(double left, IndexedNumber right) => !left.Equals(right.Value);

    public static implicit operator double(IndexedNumber indexedNumber) => indexedNumber.Value;
  }
}
