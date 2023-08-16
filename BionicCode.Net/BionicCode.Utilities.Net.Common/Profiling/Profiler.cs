namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using static System.Collections.Specialized.BitVector32;

  /// <summary>
  /// Helper methods to measure code execution time.
  /// </summary>
  public static class Profiler
  {
    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <remarks>Use the <see cref="LogTime(Action, int, ProfilerLoggerDelegate)"/> overload and provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public static ProfilerBatchResult LogTime(Action action, int runCount)
      => LogTime(action, runCount, null);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="action">The code to measure execution time.</param>
    /// <returns>The average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns>
    /// <param name="runCount">Number of iterations the <paramref name="action"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically let the <see cref="LogTime(Action, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
    /// <br/>
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
    /// </code></param>
    /// <remarks>Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static ProfilerBatchResult LogTime(Action action, int runCount, ProfilerLoggerDelegate logger)
    {
      if (runCount < 1)
      {
        throw new ArgumentException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerRunCount(), nameof(runCount));
      }

      var outputBuilder = new StringBuilder();
      BuildSummaryHeader(outputBuilder);

      var stopwatch = new Stopwatch();
      var iterationResults = new List<ProfilerResult>();

      for (int iterationCounter = 1; iterationCounter <= runCount; iterationCounter++)
      {
        stopwatch.Start();
        action.Invoke();
        stopwatch.Stop();
        var iterationResult = new ProfilerResult(iterationCounter, stopwatch.Elapsed);
        iterationResults.Add(iterationResult);

        BuildSummaryEntry(outputBuilder, iterationCounter, iterationResult);
      }

      IEnumerable<ProfilerResult> passedIterations = iterationResults.Where(profilerResult => !profilerResult.IsProfiledTaskCancelled);
      TimeSpan totalDuration = GetTotalElapsedTime(passedIterations);
      TimeSpan averageDuration = GetAverageDuration(passedIterations, runCount);
      ProfilerResult minResult = passedIterations.Min();
      ProfilerResult maxResult = passedIterations.Max();
      BuildSummaryFooter(outputBuilder, totalDuration, averageDuration, minResult, maxResult);
      var result = new ProfilerBatchResult(false, runCount, iterationResults, totalDuration, averageDuration, outputBuilder.ToString(), minResult, maxResult);
      logger?.Invoke(result, result.Summary);

      return result;
    }

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <returns>A <see cref="Task"/> holding the average execution time of all <paramref name="runCount"/> number of iterations as <see cref="TimeSpan"/>.</returns> 
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Use the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> overload and provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <c>logger</c> parameter to control the output target or customize the formatting.</remarks>
    public static Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount)
      => LogTimeAsync(asyncAction, runCount, null);

    /// <summary>
    /// Measures the execution time of a method.
    /// </summary>
    /// <param name="asyncAction">A delegate that executes the asynchronous code to measure execution time.</param>
    /// <param name="runCount">Number of iterations the <paramref name="asyncAction"/> should be executed.</param>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
    /// <br/>
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
    /// </code></param>
    /// <returns>A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains neta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>) of all <paramref name="runCount"/> number of iterations.</returns>
    /// <remarks>
    /// <para>Cancelled tasks are ignored when calculating the result (although the cancelled runs are listed in the <see cref="ProfilerBatchResult.Summary"/>, but marked as cancelled).
    /// <br/>A cancelled task is a <see cref="Task"/> where the <see cref="Task.Status"/> returns either <see cref="TaskStatus.Canceled"/> or <see cref="TaskStatus.Faulted"/> or an <see cref="OperationCanceledException"/> exception was thrown.</para>
    /// Provide an instance of the <see cref="ProfilerLoggerDelegate"/> delegate for the <paramref name="logger"/> parameter to control the output target or customize the formatting.</remarks>
    public static async Task<ProfilerBatchResult> LogTimeAsync(Func<Task> asyncAction, int runCount, ProfilerLoggerDelegate logger)
    {
      var outputBuilder = new StringBuilder();
      BuildSummaryHeader(outputBuilder);

      IEnumerable<ProfilerResult> iterationResults = await InternalLogAverageTimeAsync(asyncAction, runCount, 1, outputBuilder);

      IEnumerable<ProfilerResult> passedIterations = iterationResults.Where(profilerResult => !profilerResult.IsProfiledTaskCancelled);
      TimeSpan totalDuration = GetTotalElapsedTime(passedIterations);
      TimeSpan averageDuration = GetAverageDuration(passedIterations, runCount);
      ProfilerResult minResult = passedIterations.IsEmpty()
        ? ProfilerResult.Empty
        : passedIterations.Min();
      ProfilerResult maxResult = passedIterations.IsEmpty()
        ? ProfilerResult.Empty
        : passedIterations.Max();
      bool hasCancelledTask = iterationResults.Any(profilerResult => profilerResult.IsProfiledTaskCancelled);
      BuildSummaryFooter(outputBuilder, totalDuration, averageDuration, minResult, maxResult);

      var result = new ProfilerBatchResult(hasCancelledTask, runCount, iterationResults, totalDuration, averageDuration, outputBuilder.ToString(), minResult, maxResult);

      logger?.Invoke(result, result.Summary);
      return result;
    }

    /// <summary>
    /// Measures the execution time of using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="logger">A delegate of type <see cref="ProfilerLoggerDelegate"/> which can be used to automatically let the <see cref="LogTimeAsync(Func{Task}, int, ProfilerLoggerDelegate)"/> print the <see cref="ProfilerBatchResult.Summary"/> to a destination (e.g. file) in the following formatting:
    /// 
    /// <br/>
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
    /// </code></param>
    /// <param name="result">A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.</remarks>
    public static IDisposable LogTimeScoped(ProfilerLoggerDelegate logger, out ProfilerBatchResult result)
    {
      result = new ProfilerBatchResult(false,0,Enumerable.Empty<ProfilerResult>(), TimeSpan.Zero, TimeSpan.Zero, string.Empty, default, default);
      var profilerScope = new ProfilerScope(logger);
      profilerScope.StartProfiling(in result);
      return profilerScope;
    }

    /// <summary>
    /// Measures the execution time of a using block i.e. the scope of the <see cref="IDisposable"/>'s lifetime.
    /// </summary>
    /// <param name="result">A <see cref="Task"/> holding the <see cref="ProfilerBatchResult"/> result which contains meta data like average execution time or a formatted report (<see cref="ProfilerBatchResult.Summary"/>).</param>
    /// <returns>An <see cref="IDisposable"/> to control the scope of the profiling.</returns>
    /// <remarks>
    /// Time is measured during the lifetime of the <see cref="IDisposable"/> instance: from time of creation to the final <see cref="IDisposable.Dispose"/> call.
    /// <br/>It's recommended to use the <see cref="IDisposable"/> managed by a using-statement or using-expression.</remarks>
    public static IDisposable LogTimeScoped(out ProfilerBatchResult result)
      => LogTimeScoped(null, out result);

    private static async Task<IEnumerable<ProfilerResult>> InternalLogAverageTimeAsync(Func<Task> asyncAction, int runCount, int iterationCounter, StringBuilder outputBuilder)
    {
      var stopwatch = new Stopwatch();
      var iterationResults = new List<ProfilerResult>();
      Task profiledTask = null;
      try
      {
        for (; iterationCounter <= runCount; iterationCounter++)
        {
          stopwatch.Restart();
          profiledTask = asyncAction.Invoke();
          await profiledTask;
          stopwatch.Stop();
          TimeSpan stopwatchElapsed = stopwatch.Elapsed;
          bool isProfiledTaskCancelled = profiledTask.Status is TaskStatus.Canceled
            || profiledTask.Status is TaskStatus.Faulted;
          var iterationResult = new ProfilerResult(iterationCounter, profiledTask, isProfiledTaskCancelled, stopwatchElapsed);
          iterationResults.Add(iterationResult);

          BuildSummaryEntry(outputBuilder, iterationCounter, iterationResult);
        }
      }
      catch (OperationCanceledException)
      {
        stopwatch.Stop();
        TimeSpan stopwatchElapsed = stopwatch.Elapsed;
        var iterationResult = new ProfilerResult(iterationCounter, profiledTask, true, stopwatchElapsed);
        iterationResults.Add(iterationResult);

        BuildSummaryEntry(outputBuilder, iterationCounter, iterationResult);

        IEnumerable<ProfilerResult> results = await InternalLogAverageTimeAsync(asyncAction, runCount - iterationCounter, iterationCounter + 1, outputBuilder);
        iterationResults.AddRange(results);
      }

      return iterationResults;
    }

    internal static void BuildSummaryHeader(StringBuilder outputBuilder)
    {
      string headerCol1 = "Iteration #";
      string headerCol2 = "Duration [ms]";
      string headerCol3 = "Is cancelled";
      _ = outputBuilder
        .AppendLine($"{"╭",11}───────────────┬────────────────────────────┬────────────────╮")
        .AppendLine($"{"│",11} {headerCol1,-13} │ {headerCol2,-26} │ {headerCol3,-14} │")
        .AppendLine($"{"┝",11}━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━━━━━━━━━━━━━┿━━━━━━━━━━━━━━━━┥");
    }

    internal static void BuildSummaryEntry(StringBuilder outputBuilder, int iterationCounter, ProfilerResult iterationResult) => outputBuilder.AppendLine($"{"│",11} {iterationCounter,-13:N0} │ {iterationResult.ElapsedTime.TotalMilliseconds,26:N6} │ {iterationResult.IsProfiledTaskCancelled + (iterationResult.IsProfiledTaskCancelled ? " (ignored)" : ""),-14} │");

    internal static void BuildSummaryFooter(StringBuilder outputBuilder, TimeSpan totalDuration, TimeSpan averageDuration, ProfilerResult min, ProfilerResult max) => _ = outputBuilder
        .AppendLine($"╭═════════╧═══════════════╪════════════════════════════┼────────────────┤")
        .AppendLine($"{"│ Total:",-11} {"-",-13} │ {totalDuration.TotalMilliseconds,26:N6} │{"│",17}")
        .AppendLine($"{"│ Min:",-11} {(min.Iteration < 0 ? "-" : min.Iteration.ToString("N0")),-13:N0} │ {(min.Iteration < 0 ? "-" : min.ElapsedTime.TotalMilliseconds.ToString("N6")),26:N6} │{"│",17}")
        .AppendLine($"{"│ Max:",-11} {(max.Iteration < 0 ? "-" : max.Iteration.ToString("N0")),-13:N0} │ {(max.Iteration < 0 ? "-" : max.ElapsedTime.TotalMilliseconds.ToString("N6")),26:N6} │{"│",17}")
        .AppendLine($"{"│ Average:",-11} {"-",-13} │ {averageDuration.TotalMilliseconds,26:N6} │{"│",17}")
        .AppendLine($"╰─────────────────────────┴────────────────────────────┴────────────────╯");

    private static TimeSpan GetTotalElapsedTime(IEnumerable<ProfilerResult> entries)
    {
      if (entries.IsEmpty())
      {
        return TimeSpan.Zero;
      }

      long totalElapsedTimeInTicks = entries.Sum(profilerResult => profilerResult.ElapsedTime.Ticks);
      return TimeSpan.FromTicks(totalElapsedTimeInTicks);
    }

    private static TimeSpan GetAverageDuration(IEnumerable<ProfilerResult> entries, int runCount)
    {
      if (entries.IsEmpty())
      {
        return TimeSpan.Zero;
      }

      long totalElapsedTimeInTicks = entries.Sum(profilerResult => profilerResult.ElapsedTime.Ticks);
      long averageTicks = totalElapsedTimeInTicks / runCount;
      return TimeSpan.FromTicks(averageTicks);
    }
  }
}
