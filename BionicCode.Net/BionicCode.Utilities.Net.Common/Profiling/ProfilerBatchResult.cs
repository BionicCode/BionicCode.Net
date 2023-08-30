namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading.Tasks;

  /// <summary>
  /// The result of a multi iteration benchmark session.
  /// <br/>The <see cref="ProfilerBatchResult"/> consolidates all <see cref="ProfilerResult"/> result items of a single benchmark run
  /// <br/>and provides meta data on hte collected data.
  /// </summary>
  /// <remarks>For example, the <see cref="ProfilerBatchResult"/> is returned by multi-run benchmark methods like <see cref="Profiler.LogTime(System.Action, int, string, string, int)"/>.</remarks>
  public class ProfilerBatchResult
  {
    internal static ProfilerBatchResult Empty { get; } = new ProfilerBatchResult();

    internal ProfilerBatchResult(int iterationCount, DateTime timeStamp)
    {
      if (iterationCount < 1)
      {
        throw new ArgumentOutOfRangeException(nameof(iterationCount), "A valid result must have at least a single iteration.");
      }

      this.IterationCount = iterationCount;
      this.TimeStamp = timeStamp;
      this.ResultsInternal = new List<ProfilerResult>();
      this.Results = new ReadOnlyCollection<ProfilerResult>(this.ResultsInternal);
      this.TotalDuration = TimeSpan.Zero;
      this.AverageDuration = TimeSpan.Zero;
      this.MinResult = ProfilerResult.MaxDuration;
      this.MaxResult = ProfilerResult.MinDuration;
      this.Index = 0;

      this.HasCancelledProfiledTaskFactory = new Lazy<bool>(HasCancelledTask);
      this.SummaryFactory = new Lazy<string>(BuildSummary);
    }

    private ProfilerBatchResult()
    {
      this.IterationCount = 0;
      this.TimeStamp = DateTime.Now;
      this.ResultsInternal = new List<ProfilerResult>();
      this.Results = new ReadOnlyCollection<ProfilerResult>(this.ResultsInternal);
      this.TotalDuration = TimeSpan.Zero;
      this.AverageDuration = TimeSpan.Zero;
      this.MinResult = ProfilerResult.Empty;
      this.MaxResult = ProfilerResult.Empty;
      this.Index = 0;

      this.HasCancelledProfiledTaskFactory = new Lazy<bool>(HasCancelledTask);
      this.SummaryFactory = new Lazy<string>(BuildSummary);
    }

    private string BuildSummary()
    {
      if (ReferenceEquals(this, ProfilerBatchResult.Empty))
      {
        return string.Empty;
      }

      var summaryBuilder = new StringBuilder();
      string title = $"Profile target: {this.Context.TargetName}";
      Profiler.BuildSummaryHeader(summaryBuilder, title, this.Context.TargetName, this.Context.SourceFileName, this.Context.LineNumber);

      foreach (ProfilerResult result in this.Results)
      {
        Profiler.BuildSummaryEntry(summaryBuilder, result);
      }

      Profiler.BuildSummaryFooter(summaryBuilder, this);

      return summaryBuilder.ToString();
    }

    private bool HasCancelledTask() => this.Results.Any(result => result.IsProfiledTaskCancelled);

    internal void Combine(ProfilerBatchResult batchResultToAppend)
      => AddResultRange(batchResultToAppend.Results);

    internal void AddResultRange(IEnumerable<ProfilerResult> results)
    {
      if (ReferenceEquals(this, ProfilerBatchResult.Empty))
      {
        throw new InvalidOperationException("Can't modify an Empty instance because it is a constant immutable value. Call a constructor to create a mutable instance.");
      }

      foreach (ProfilerResult result in results)
      {
        AddResult(result);
      }
    }

    internal void AddResult(ProfilerResult result)
    {
      if (ReferenceEquals(this, ProfilerBatchResult.Empty))
      {
        throw new InvalidOperationException("Can't modify an Empty instance because it is a constant immutable value. Call a constructor to create a mutable instance.");
      }

      this.ResultsInternal.Add(result);

      this.TotalDuration = this.TotalDuration.Add(result.ElapsedTime);
      this.AverageDuration = TimeSpan.FromTicks(this.TotalDuration.Ticks / this.IterationCount);

      if (result < this.MinResult)
      {
        this.MinResult = result;
      }

      if (result > this.MaxResult)
      {
        this.MaxResult = result;
      }
    }

    private void CalculateData()
    {
      this.IsDataCalculated = true;
      this.Variance = CalculateVariance();
      this.StandardDeviationInMicroseconds = System.Math.Sqrt(this.Variance);
    }

    private double CalculateVariance()
    {
      double totalDeviationMicroseconds = 0;
      foreach (ProfilerResult result in this.Results)
      {
        totalDeviationMicroseconds += System.Math.Pow(result.DeviationInMicroseconds, 2);
      }

      return totalDeviationMicroseconds / this.IterationCount;
    }

    public int Index { get; internal set; }

    private double standardDeviationInMicroseconds;
    public double StandardDeviationInMicroseconds 
    {
      get
      { 
        if (!this.IsDataCalculated)
        {
          CalculateData();
        }

        return  this.standardDeviationInMicroseconds;
      } 
      private set => this.standardDeviationInMicroseconds = value; 
    }

    private double variance;
    public double Variance
    {
      get
      {
        if (!this.IsDataCalculated)
        {
          CalculateData();
        }

        return this.variance;
      }
      private set => this.variance = value; 
    }

    /// <summary>
    /// In case ansync operation was benchmarked, this property returns whether the <see cref="Task"/> was cancelled or not.
    /// </summary>
    /// <standardDeviationInMicroseconds><see langword="true"/> if the benchmark series contains a cancelled run (where the <see cref="Task"/> is cancelled).</standardDeviationInMicroseconds>
    public bool HasCancelledProfiledTask => this.HasCancelledProfiledTaskFactory.Value;

    /// <summary>
    /// The number of iterations the <see cref="Profiler"/> has run the specified operation.
    /// </summary>
    /// <standardDeviationInMicroseconds>The number of iterations (which is equivalent to the number of results).</standardDeviationInMicroseconds>
    public int IterationCount { get; internal set; }

    /// <summary>
    /// The logged results of each iteration.
    /// </summary>
    /// <standardDeviationInMicroseconds>The results of each iteration.</standardDeviationInMicroseconds>
    public IReadOnlyCollection<ProfilerResult> Results { get; }

    /// <summary>
    /// The total duration of all logged iterations.
    /// </summary>
    /// <standardDeviationInMicroseconds>The sum of each duration per iteration.</standardDeviationInMicroseconds>
    public TimeSpan TotalDuration { get; internal set; }
#if NET7_0_OR_GREATER
    internal double TotalDurationInMicroseconds => this.TotalDuration.TotalMicroseconds;
#else
    internal double TotalDurationInMicroseconds => this.TotalDuration.TotalMicroseconds();
#endif

    /// <summary>
    /// The average duration of all logged iterations.
    /// </summary>
    /// <standardDeviationInMicroseconds>The average duration of all iterations.</standardDeviationInMicroseconds>
    public TimeSpan AverageDuration { get; internal set; }

#if NET7_0_OR_GREATER
    internal double AverageDurationInMicroseconds => this.AverageDuration.TotalMicroseconds;
#else
    internal double AverageDurationInMicroseconds => this.AverageDuration.TotalMicroseconds();
#endif

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
    /// <standardDeviationInMicroseconds>A formatted string that presents the results of the benchmarking in an Unicode formatted table.</standardDeviationInMicroseconds>
    public string Summary => this.SummaryFactory.Value;
    private Lazy<string> SummaryFactory { get; }

    /// <summary>
    /// The smallest <see cref="ProfilerResult"/> result of the batch run (which is the fastest).
    /// </summary>
    public ProfilerResult MinResult { get; internal set; }

    /// <summary>
    /// The greatest <see cref="ProfilerResult"/> result of the batch run (which is the slowest).
    /// </summary>
    public ProfilerResult MaxResult { get; internal set; }

    /// <summary>
    /// The timestamp that describes the moment in time the profiling session associated with this current result was executed.
    /// </summary>
    public DateTime TimeStamp { get; }

    private Lazy<bool> HasCancelledProfiledTaskFactory { get; set; }

    private IList<ProfilerResult> ResultsInternal { get; set; }
    private bool IsDataCalculated { get; set; }

    /// <summary>
    /// Describes the context that the profiler was executed.
    /// </summary>
    /// <remarks>The context describes machine atributes like core clock and core count to help to compare results from sessions on different machines.</remarks>
    public ProfilerContext Context { get; internal set; }
  }
}