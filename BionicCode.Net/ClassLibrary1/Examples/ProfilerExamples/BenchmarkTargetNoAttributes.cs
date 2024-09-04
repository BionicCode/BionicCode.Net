namespace BionicCode.Utilities.Net.Examples.ProfilerExamples.A
{
  #region CodeWithoutNamespace
  using System.Collections.Generic;
  using System.Linq;

  class BenchmarkTarget
  {
    public IEnumerable<int> CalculateFibonacciSeriesRecursive(int length)
    {
      int firstNumberSeed = 0;
      int secondNumberSeed = 1;
      int counter = 0;
      IEnumerable<int> result = FibonacciSeriesRecursive(firstNumberSeed, secondNumberSeed, ref counter, length);
      return result;
    }

    protected IEnumerable<int> CalculateFibonacciSeriesIterative(int length)
    {
      IEnumerable<int> resultSeries = FibonacciSeriesIterative(length);
      return resultSeries;
    }

    private IEnumerable<int> FibonacciSeriesRecursive(int firstNumber, int secondNumber, ref int counter, int length)
    {
      var series = new List<int>() { firstNumber };
      if (++counter < length)
      {
        IEnumerable<int> restOfSeries = FibonacciSeriesRecursive(secondNumber, firstNumber + secondNumber, ref counter, length);
        series = series
          .Concat(restOfSeries)
          .ToList();
      }

      return series;
    }

    private IEnumerable<int> FibonacciSeriesIterative(int length)
    {
      int count = 0;
      int firstNumber = 0;
      int secondNumber = 1;
      var series = new List<int>() { firstNumber };

      while (++count < length)
      {
        int secondNumberTemp = secondNumber;
        secondNumber = firstNumber + secondNumber;
        firstNumber = secondNumberTemp;
        series.Add(firstNumber);
      }

      return series;
    }
    #endregion
  }
}
