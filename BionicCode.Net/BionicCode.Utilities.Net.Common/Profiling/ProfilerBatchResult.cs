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
      this.TotalDuration = Microseconds.Zero;
      this.AverageDuration = Microseconds.Zero;
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
      this.TotalDuration = Microseconds.Zero;
      this.AverageDuration = Microseconds.Zero;
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

      this.TotalDuration = this.TotalDuration + result.ElapsedTime;
      this.AverageDuration = this.TotalDuration / (double)this.ResultsInternal.Count;

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
      this.StandardDeviation = System.Math.Round(System.Math.Sqrt(this.Variance), 1);
    }

    private double CalculateVariance()
    {
      double totalDeviationMicroseconds = 0;
      foreach (ProfilerResult result in this.Results)
      {
        totalDeviationMicroseconds += System.Math.Pow(result.Deviation, 2);
      }

      return totalDeviationMicroseconds / this.IterationCount;
    }

    private bool HasCancelledTask() => this.Results.Any(result => result.IsProfiledTaskCancelled);

    /// <summary>
    /// The index of the current result batch.
    /// </summary>
    public int Index { get; internal set; }

    private Microseconds standardDeviation;
    /// <summary>
    /// The stamdard deviation over all the <see cref="ProfilerResult"/> items in microseconds.
    /// </summary>
    /// <value>Stamdard deviation in microseconds.</value>
    public Microseconds StandardDeviation 
    {
      get
      { 
        if (!this.IsDataCalculated)
        {
          CalculateData();
        }

        return  this.standardDeviation;
      } 
      private set => this.standardDeviation = value;
    }

    /// <summary>
    /// The stamdard deviation over all the <see cref="ProfilerResult"/> items converted to the base unit.
    /// </summary>
    /// <value>Stamdard deviation converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double StandardDeviationConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.StandardDeviation, true);

    private double variance;
    /// <summary>
    /// The variance over all the <see cref="ProfilerResult"/> items.
    /// </summary>
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
    /// <value><see langword="true"/> if the benchmark series contains a cancelled run (where the <see cref="Task"/> is cancelled).</value>
    public bool HasCancelledProfiledTask => this.HasCancelledProfiledTaskFactory.Value;

    /// <summary>
    /// The number of iterations the <see cref="Profiler"/> has run the specified operation.
    /// </summary>
    /// <value>The number of iterations (which is equivalent to the number of results) per argument list.</value>
    /// <remarks>Using the profiler with annotations (attributes) (see <see cref="ProfileAttribute"/>) allows to specify argument lists (see <see cref="ProfilerArgumentAttribute"/>) to simulate real-world behavior where usually the values of the arguments that are provided to the member change.
    /// <br/>A member is executed and profiled with each argument list for the number of the <see cref="IterationCount"/> times.
    /// <br/>For example if a method is profiled with three argument lists and an iteration count of 10, the <see cref="IterationCount"/> will return <c>10</c> and the total iterations run for the profiled member is <c>30</c>, the product of <see cref="IterationCount"/> and <see cref="ArgumentListCount"/> (see <see cref="TotalIterationCount"/>).</remarks>
    public int IterationCount { get; internal set; }

    /// <summary>
    /// The total number of iterations to conduct the profiling.
    /// </summary>
    /// <value>The value is based on the number of iterations and the number of argument lists. If <see cref="IterationCount"/> returns <c>10</c> and <see cref="ArgumentListCount"/> returns <c>3</c> then <see cref="TotalIterationCount"/> will return <c>30</c> because each argument list is executed 10 times.</value>
    public int TotalIterationCount => System.Math.Max(this.ArgumentListCount, 1) * this.IterationCount;

    /// <summary>
    /// The number of iterations the <see cref="Profiler"/> has run the specified operation.
    /// </summary>
    /// <value>The number of iterations (which is equivalent to the number of results) per argument list.</value>
    /// <remarks>Using the profiler with annotations (attributes) allows to specify argument lists to simulate real-world behavior where usually the values of the arguments that are provided to the member change.
    /// <br/>A member is executed and profiled with each argument list for the number of the <see cref="IterationCount"/> times.
    /// <br/>For example if a method is profiled with three argument lists and an iteration count of 10, the <see cref="IterationCount"/> will return <c>10</c> and the total iterations run for the profiled member is the product of <see cref="IterationCount"/> and <see cref="ArgumentListCount"/>.</remarks>
    public int ArgumentListCount { get; internal set; }

    /// <summary>
    /// The logged results of each iteration.
    /// </summary>
    /// <value>The read-only results of all iterations.</value>
    public IReadOnlyCollection<ProfilerResult> Results { get; }

    /// <summary>
    /// The total duration of all logged iterations in microseconds.
    /// </summary>
    /// <value>The sum of each duration per iteration in microseconds.</value>
    public Microseconds TotalDuration { get; internal set; }

    /// <summary>
    /// The total duration of all logged iterations converted to the base unit.
    /// </summary>
    /// <value>The sum of each duration per iteration converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double TotalDurationConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.TotalDuration, true);

    /// <summary>
    /// The average duration of all logged iterations in microseconds.
    /// </summary>
    /// <value>The average duration of all iterations in microseconds.</value>
    public Microseconds AverageDuration { get; internal set; }

    /// <summary>
    /// The average duration of all logged iterations converted to the base unit.
    /// </summary>
    /// <value>The average duration of all iterations converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double AverageDurationConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.AverageDuration, true);

    /// <summary>
    /// The range of all logged iterations in microseconds.
    /// </summary>
    /// <value>The range of all iterations in microseconds.</value>
    public Microseconds Range => this.MaxResult.ElapsedTime - this.MinResult.ElapsedTime;

    /// <summary>
    /// The range of all logged iterations converted to the base unit.
    /// </summary>
    /// <value>The range of all iterations converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double RangeConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.Range, true);

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

    /// <summary>
    /// The base unit used to calculate the values for <see cref="TotalDurationConverted"/>, <see cref="StandardDeviationConverted"/> and <see cref="AverageDurationConverted"/>.
    /// </summary>
    public TimeUnit BaseUnit { get; internal set; }

    internal ProfilerBatchResult ProfilerReferenceResult { get; set; }
  }
}