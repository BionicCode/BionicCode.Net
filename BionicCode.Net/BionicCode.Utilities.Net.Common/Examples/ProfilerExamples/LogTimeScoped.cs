namespace BionicCode.Utilities.Net.Examples.ProfilerExamples
{
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.A;
  using System;
  #region CodeWithoutNamespace
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  internal class LogTimeScopedExample
  {
    public static async Task Main(string[] args)
    {
      const int seriesLength = 10000;
      var benchmarkTarget = new BenchmarkTarget();

#if !NETFRAMEWORK && !NETSTANDARD2_0
      await using (Profiler.LogTimeScopedAsync(LogToFileAsync, out ProfilerBatchResult result))
      {
        // Benchmark the method call
        IEnumerable<int> series = benchmarkTarget.CalculateFibonacciSeriesRecursive(seriesLength);
      }
#else
      using (Profiler.LogTimeScoped(LogToFile, out ProfilerBatchResult result))
      {
        // Benchmark the method call
        IEnumerable<int> series = benchmarkTarget.CalculateFibonacciSeriesRecursive(seriesLength);
      }      
#endif
    }

#if  !NETFRAMEWORK && !NETSTANDARD2_0
    // Custom async result logger callback
    private static async Task LogToFileAsync(ProfilerBatchResult result, string preformattedSummary)
    {
      await using var streamWriter = new StreamWriter(@"c:\profiler_log.txt");
      await streamWriter.WriteAsync(result.Summary);      
    }
#else

    // Custom async result logger callback
    private static void LogToFile(ProfilerBatchResult result, string preformattedSummary)
    {
      using (var streamWriter = new StreamWriter(@"c:\profiler_log.txt"))
      {
        streamWriter.Write(result.Summary);
      }
    }
#endif

    /*
    By default, the 'ProfilerBatchResult.Summary' property will return a pre-formatted result as follows: 

              ╭───────────────┬────────────────────────────┬────────────────╮
              | Iteration #   │ Duration [ms]              │ Is cancelled   |
              ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
              │ 1             │                     1.0512 │ False          |
              │ 2             │                     1.0020 │ False          │
              │ 3             │                     0.8732 │ False          │
              │ 4             │                     0.0258 │ True (ignored) │
              │ 5             │                     0.9943 │ False          │
    ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
    │ Total:    -             │                     3.9207 │                │
    │ Min:      3             │                     0.8732 │                │
    │ Max:      1             │                     1.0512 │                │
    │ Average:  -             │                     0.9802 │                │
    ╰─────────────────────────┴────────────────────────────┴────────────────╯
    */
#endregion
  }
}
