namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// The result of a single benchmark run or iteration.
  /// </summary>
  public struct ProfilerResult : IEquatable<ProfilerResult>, IComparable<ProfilerResult>
  {
    internal static ProfilerResult Empty => new ProfilerResult(-1, TimeSpan.Zero, null);
    internal static ProfilerResult MaxDuration => new ProfilerResult(-1, TimeSpan.MaxValue, null);
    internal static ProfilerResult MinDuration => new ProfilerResult(-1, TimeSpan.MinValue, null);

    internal ProfilerResult(int iteration, TimeSpan elapsedTime, ProfilerBatchResult owner) : this(iteration, null, elapsedTime, TimeSpan.MinValue, owner)
    {
    }

    internal ProfilerResult(int iteration, Task profiledTask, TimeSpan elapsedTime, ProfilerBatchResult owner) : this(iteration, profiledTask, elapsedTime, TimeSpan.MinValue, owner)
    {
    }

    internal ProfilerResult(int iteration, Task profiledTask, TimeSpan elapsedTime, TimeSpan deviation, ProfilerBatchResult owner)
    {
      this.Owner = owner;
      this.Iteration = iteration;
      this.ProfiledTask = profiledTask;
      this.ElapsedTime = elapsedTime;
      this.deviation = deviation;
    }

    internal TimeSpan GetDeviation() => this.ElapsedTime.Subtract(this.Owner?.AverageDuration ?? TimeSpan.Zero);

    /// <summary>
    /// The duration of the benchmark run.
    /// </summary>
    /// <value>The duration.</value>
    public TimeSpan ElapsedTime { get; }

#if NET7_0_OR_GREATER
    internal double ElapsedTimeInMicroseconds => this.ElapsedTime.TotalMicroseconds;
#else
    internal double ElapsedTimeInMicroseconds => this.ElapsedTime.TotalMicroseconds();
#endif

    private TimeSpan deviation;

    /// <summary>
    /// The deviation from the arithmetic mean.
    /// </summary>
    /// <value>A positive or negative value to describe the deviation from the arithmetic mean.</value>
    public TimeSpan Deviation => this.deviation == TimeSpan.MinValue 
      ? this.Owner is null 
        ? throw new InvalidOperationException("The property is unset") 
        : (this.deviation = GetDeviation())
      : this.deviation;

#if NET7_0_OR_GREATER
    internal double DeviationInMicroseconds => this.Deviation.TotalMicroseconds;
#else
    internal double DeviationInMicroseconds => this.Deviation.TotalMicroseconds();
#endif

    /// <summary>
    /// In case the benchmarked operation is an async method, <see cref="IsProfiledTaskCancelled"/> indiocates wwhether the <see langword="async"/>operation was cancelled or not.
    /// </summary>
    /// <value><see langword="true"/> in case the operation wwwas cancelled (and the <see cref="ProfiledTask"/> result is in a canlled state (<see cref="Task.Status"/> returns <see cref="TaskStatus.Canceled"/>).</value>
    public bool IsProfiledTaskCancelled => this.ProfiledTask?.Status is TaskStatus.Canceled || this.ProfiledTask?.Status is TaskStatus.Faulted;

    /// <summary>
    /// In case of a multi-run benchmark run (e.g. by calling <see cref="Profiler.LogTime(Action, int, string, int)"/>), the property <see cref="Iteration"/> returns the run's actual number the <see cref="ProfilerResult"/> is associated with.
    /// </summary>
    /// <value>The number of the run the <see cref="ProfilerResult"/> is associated with. Otherwise <c>-1</c>.</value>
    public int Iteration { get; }

    /// <summary>
    /// The <see cref="Task"/> returned from the async operation that was benchmarked.
    /// </summary>
    /// <value>The <see cref="Task"/> result.</value>
    public Task ProfiledTask { get; }

    internal ProfilerBatchResult Owner { get; set; }

    /// <inheritdoc/>
    public int CompareTo(ProfilerResult other) => this.ElapsedTime.CompareTo(other.ElapsedTime);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ProfilerResult result && Equals(result);

    /// <inheritdoc/>
    public bool Equals(ProfilerResult other) => this.ElapsedTime.Equals(other.ElapsedTime) && this.IsProfiledTaskCancelled == other.IsProfiledTaskCancelled && this.Iteration == other.Iteration && EqualityComparer<Task>.Default.Equals(this.ProfiledTask, other.ProfiledTask);

    /// <inheritdoc/>
#if NET || NETSTANDARD2_1_OR_GREATER
    public override int GetHashCode() => HashCode.Combine(this.ElapsedTime, this.IsProfiledTaskCancelled, this.Iteration, this.ProfiledTask);
#else
    public override int GetHashCode()
    {
      int hashCode = 208172843;
      hashCode = hashCode * -1521134295 + this.ElapsedTime.GetHashCode();
      hashCode = hashCode * -1521134295 + this.IsProfiledTaskCancelled.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Iteration.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<Task>.Default.GetHashCode(this.ProfiledTask);
      return hashCode;
    }
#endif

    public static bool operator <(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) < 0;
    public static bool operator <=(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) <= 0;
    public static bool operator >(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) > 0;
    public static bool operator >=(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) >= 0;
  }
}