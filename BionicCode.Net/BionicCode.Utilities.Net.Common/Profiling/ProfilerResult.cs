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
    internal static ProfilerResult Empty => new ProfilerResult(-1, Microseconds.Zero, TimeUnit.Microseconds, null);
    internal static ProfilerResult MaxDuration => new ProfilerResult(-1, Microseconds.MaxValue, TimeUnit.Microseconds, null);
    internal static ProfilerResult MinDuration => new ProfilerResult(-1, Microseconds.MinValue, TimeUnit.Microseconds, null);

    internal ProfilerResult(int iteration, Microseconds elapsedTime, TimeUnit baseUnit, ProfilerBatchResult owner) : this(iteration, null, elapsedTime, Microseconds.MinValue, baseUnit, owner)
    {
    }

    internal ProfilerResult(int iteration, Task profiledTask, Microseconds elapsedTime, TimeUnit baseUnit, ProfilerBatchResult owner) : this(iteration, profiledTask, elapsedTime, Microseconds.MinValue, baseUnit, owner)
    {
    }

    internal ProfilerResult(int iteration, Task profiledTask, Microseconds elapsedTime, Microseconds deviation, TimeUnit baseUnit, ProfilerBatchResult owner)
    {
      this.Owner = owner;
      this.Iteration = iteration;
      this.ProfiledTask = profiledTask;
      this.ElapsedTime = elapsedTime;
      this.deviation = deviation;
      this.BaseUnit = baseUnit;
    }

    internal Microseconds GetDeviation() => this.ElapsedTime - (this.Owner?.AverageDuration ?? Microseconds.Zero);

    /// <summary>
    /// The duration of the benchmark run in microseconds.
    /// </summary>
    /// <value>The duration in microseconds.</value>
    public Microseconds ElapsedTime { get; }

    /// <summary>
    /// The duration of the benchmark run converted to the base unit.
    /// </summary>
    /// <value>The duration converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double ElapsedTimeConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.ElapsedTime);

    private Microseconds deviation;

    /// <summary>
    /// The deviation from the arithmetic mean in microseconds.
    /// </summary>
    /// <value>A positive or negative value to describe the deviation from the arithmetic mean in microseconds.</value>
    public Microseconds Deviation => this.deviation == Microseconds.MinValue 
      ? this.Owner is null 
        ? throw new InvalidOperationException("The property is unset") 
        : (this.deviation = GetDeviation())
      : this.deviation;

    /// <summary>
    /// The deviation from the arithmetic mean converted to the base unit.
    /// </summary>
    /// <value>A positive or negative value to describe the deviation from the arithmetic mean. the value is converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double DeviationConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.Deviation);

    /// <summary>
    /// The base unit used to calculate the values for <see cref="DeviationConverted"/> and <see cref="ElapsedTimeConverted"/>.
    /// </summary>
    public TimeUnit BaseUnit { get; internal set; }

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