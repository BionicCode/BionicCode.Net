namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;

  /// <summary>
  /// Helper methods to measure code execution time.
  /// </summary>
  public static class Profiler
  {
    /// <summary>
    /// A <see cref="Action{T}"/> delegate that can be used to redirect the log output. By default the log output will be send to the output window.
    /// </summary>
    /// <value>A <see cref="Action{T}"/> delegate which will be invoked to output the elapsed <see cref="TimeSpan"/>. The default delegate will print the output to the output window.</value>
    public static Action<TimeSpan> LogPrinter { get; set; }

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The execution time as a <see cref="TimeSpan"/>.</returns>
    /// <remarks>Specify a <see cref="LogPrinter"/> <see cref="Action"/> to customize the output target and formatting.</remarks>
    public static TimeSpan LogTime(Action action)
    {
      var stopwatch = new Stopwatch();
      stopwatch.Start();
      action.Invoke();
      stopwatch.Stop();
      TimeSpan stopwatchElapsed = stopwatch.Elapsed;
      if (Profiler.LogPrinter == null)
      {
        Profiler.LogPrinter = (elapsedTime) =>
          Console.WriteLine($"Elapsed time: {elapsedTime.TotalMilliseconds} [ms]");
      }
      Profiler.LogPrinter?.Invoke(stopwatchElapsed);

      return stopwatchElapsed;
    }


    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <returns>A list of execution times for all <paramref name="runCount"/> number of iterations <see cref="TimeSpan"/>.</returns>
    /// <remarks>Specify a <see cref="LogPrinter"/> <see cref="Action"/> to customize the output target and formatting.</remarks>
    public static List<TimeSpan> LogTimes(Action action, int runCount)
    {
      if (Profiler.LogPrinter == null)
      {
        Profiler.LogPrinter = (elapsedTime) =>
          Console.WriteLine($"Iteration #{runCount}: Elapsed time: {elapsedTime.TotalMilliseconds} [ms]");
      }
      var stopwatch = new Stopwatch();
      var measuredTimes = new List<TimeSpan>();

      for (; runCount > 0; runCount--)
      {
        stopwatch.Start();
        action.Invoke();
        stopwatch.Stop();
        TimeSpan stopwatchElapsed = stopwatch.Elapsed;
        measuredTimes.Add(stopwatchElapsed);
        Profiler.LogPrinter.Invoke(stopwatchElapsed);
      }

      return measuredTimes;
    }

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <remarks>Specify a <see cref="LogPrinter"/> <see cref="Action"/> to customize the output target and formatting.</remarks>
    public static TimeSpan LogAverageTime(Action action, int runCount)
    {
      var stopwatch = new Stopwatch();
      var measuredTimes = new List<TimeSpan>();

      for (; runCount > 0; runCount--)
      {
        stopwatch.Start();
        action.Invoke();
        stopwatch.Stop();
        TimeSpan stopwatchElapsed = stopwatch.Elapsed;
        measuredTimes.Add(stopwatchElapsed);
      }

      var logAverageTime = new TimeSpan((long) measuredTimes.Average((time) => time.Ticks));
      if (Profiler.LogPrinter == null)
      {
        Profiler.LogPrinter = (elapsedTime) =>
          Console.WriteLine($"Iterations={runCount}; Average elapsed time: {elapsedTime.TotalMilliseconds} [ms]");
      }
      Profiler.LogPrinter.Invoke(logAverageTime);
      return logAverageTime;
    }
  }
}
