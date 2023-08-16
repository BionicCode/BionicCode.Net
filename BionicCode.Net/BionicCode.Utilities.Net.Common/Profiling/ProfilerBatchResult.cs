namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// The result of a multi iteration benchmark session.
  /// <br/>The <see cref="ProfilerBatchResult"/> consolidates all <see cref="ProfilerResult"/> result items of a single benchmark run
  /// <br/>and provides meta data on hte collected data.
  /// </summary>
  /// <remarks>For example, the <see cref="ProfilerBatchResult"/> is returned by multi-run benchmark methods like <see cref="Profiler.LogTimeAsync(System.Func{System.Threading.Tasks.Task}, int)"/>.</remarks>
  public class ProfilerBatchResult
  {
    internal ProfilerBatchResult(bool hasCancelledProfiledTask, int iterationCount, IEnumerable<ProfilerResult> results, TimeSpan totalDuration, TimeSpan averageDuration, string summary, ProfilerResult minResult, ProfilerResult maxResult)
    {
      this.HasCancelledProfiledTask = hasCancelledProfiledTask;
      this.IterationCount = iterationCount;
      this.Results = results;
      this.TotalDuration = totalDuration;
      this.AverageDuration = averageDuration;
      this.Summary = summary;
      this.MinResult = minResult;
      this.MaxResult = maxResult;
    }

    /// <summary>
    /// In case ansync operation was benchmarked, this property returns whether the <see cref="Task"/> was cancelled or not.
    /// </summary>
    /// <value><see langword="true"/> if the benchmark series contains a cancelled run (where the <see cref="Task"/> is cancelled).</value>
    public bool HasCancelledProfiledTask { get; internal set; }
    
    /// <summary>
    /// The number of iterations the <see cref="Profiler"/> has run the specified operation.
    /// </summary>
    /// <value>The number of iterations (which is equivalent to the number of results).</value>
    public int IterationCount { get; internal set; }

    /// <summary>
    /// The logged results of each iteration.
    /// </summary>
    /// <value>The results of each iteration.</value>
    public IEnumerable<ProfilerResult> Results { get; internal set; }

    /// <summary>
    /// The total duration of all logged iterations.
    /// </summary>
    /// <value>The sum of each duration per iteration.</value>
    public TimeSpan TotalDuration { get; internal set; }

    /// <summary>
    /// The average duration of all logged iterations.
    /// </summary>
    /// <value>The average duration of all iterations.</value>
    public TimeSpan AverageDuration { get; internal set; }

    /// <summary>
    /// A report of the benchmarking ready for output, formatted as follows:
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
    /// </code>
    /// </summary>
    /// <value>A formatted string that presents the results of the benchmarking in an Unicode formatted table.</value>
    public string Summary { get; internal set; }

    /// <summary>
    /// The smallest <see cref="ProfilerResult"/> result of the batch run (which is the fastest).
    /// </summary>
    public ProfilerResult MinResult { get; internal set; }

    /// <summary>
    /// The greatest <see cref="ProfilerResult"/> result of the batch run (which is the slowest).
    /// </summary>
    public ProfilerResult MaxResult { get; internal set; }
  }
}