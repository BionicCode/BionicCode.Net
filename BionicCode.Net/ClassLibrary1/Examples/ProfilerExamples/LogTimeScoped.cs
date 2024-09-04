namespace BionicCode.Utilities.Net.Examples.ProfilerExamples
{
  using BionicCode.Utilities.Net.Examples.ProfilerExamples.A;
  #region CodeWithoutNamespace
  using System.Collections.Generic;
  using System.IO;
  using System.Threading.Tasks;

  internal class LogTimeScopedExample
  {
    public static void Main(string[] args)
    {
      const int seriesLength = 10000;
      var benchmarkTarget = new BenchmarkTarget();

      ProfilerBatchResult result;
      using (Profiler.LogTimeScoped(LogToFileAsync, out result))
      {
        // Benchmark the method call
        IEnumerable<int> series = benchmarkTarget.CalculateFibonacciSeriesRecursive(seriesLength);
      }
    }

    // Custom async result logger callback
    private static async Task LogToFileAsync(ProfilerBatchResult result)
    {
      using (var streamWriter = new StreamWriter(@"c:\profiler_log.txt"))
      {
        await streamWriter.WriteAsync(result.Summary);
      }
    }

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
