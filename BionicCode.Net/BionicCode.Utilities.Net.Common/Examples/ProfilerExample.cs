namespace BionicCode.Utilities.Net.Examples
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Threading.Tasks;

  internal class ProfilerExample
  {
    /* LogTimeScoped */
public static void Main1(string[] args)
{
  ProfilerBatchResult result;
  using (Profiler.LogTimeScoped(LogToFileAsync, out result))
  {
    FibonacciSeries(0, 1, 1, 100);
  }

  // Print a preformatted summary
  Console.Write(result.Summary);

  // Print the average duration
  Console.Write(result.AverageDuration);
}

private static async Task LogToFileAsync(ProfilerBatchResult result)
{
#if NET
      await File.WriteAllTextAsync(result.Summary, @"c:\profiler_log.txt");
#endif
    }

/// <code>
///           ╭───────────────┬────────────────────────────┬────────────────╮
///           | Iteration #   │ Duration [ms]              │ Is cancelled   |
///           ┝━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥
///           │ 1             │                     1.0512 │ False          |
///           │ 2             │                     1.0020 │ False          │
///           │ 3             │                     0.8732 │ False          │
///           │ 4             │                     0.0258 │ True (ignored) │
///           │ 5             │                     0.9943 │ False          │
/// ╭═════════╧═══════════════╪════════════════════════════┼────────────────┤
/// │ Total:    -             │                     3.9207 │                │
/// │ Min:      3             │                     0.8732 │                │
/// │ Max:      1             │                     1.0512 │                │
/// │ Average:  -             │                     0.9802 │                │
/// ╰─────────────────────────┴────────────────────────────┴────────────────╯
/// </code>
private void LogTimeScopedExampleOutput() { }

public static void FibonacciSeries(int firstNumber, int secondNumber, int counter, int number)
{
  Console.Write(firstNumber + " ");
  if (counter < number)
  {
    FibonacciSeries(secondNumber, firstNumber + secondNumber, counter + 1, number);
  }
}
  }
}
