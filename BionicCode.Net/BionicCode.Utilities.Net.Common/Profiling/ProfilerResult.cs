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
    internal static ProfilerResult Empty => new ProfilerResult(-1, Microseconds.Zero, null, -1);
    internal static ProfilerResult MaxDuration => new ProfilerResult(-1, Microseconds.MaxValue, null, -1);
    internal static ProfilerResult MinDuration => new ProfilerResult(-1, Microseconds.MinValue, null, -1);

    internal ProfilerResult(int iteration, Microseconds elapsedTime, ProfilerBatchResult owner, int argumentListIndex) : this(iteration, false, elapsedTime, Microseconds.MinValue, owner, argumentListIndex)
    {
    }

    internal ProfilerResult(int iteration, bool isProfiledTaskCancelled, Microseconds elapsedTime, ProfilerBatchResult owner, int argumentListIndex) : this(iteration, isProfiledTaskCancelled, elapsedTime, Microseconds.MinValue, owner, argumentListIndex)
    {
    }

    internal ProfilerResult(int iteration, bool isProfiledTaskCancelled, Microseconds elapsedTime, Microseconds deviation, ProfilerBatchResult owner, int argumentListIndex)
    {
      this.Owner = owner;
      this.Iteration = iteration;
      this.IsProfiledTaskCancelled = isProfiledTaskCancelled;
      this.ElapsedTime = elapsedTime;
      this.deviation = deviation;
      this.ArgumentListIndex = argumentListIndex;
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
    public double ElapsedTimeConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.ElapsedTime, true);

    private Microseconds deviation;

    /// <summary>
    /// The deviation from the arithmetic mean in microseconds.
    /// </summary>
    /// <value>A positive or negative value to describe the deviation from the arithmetic mean in microseconds.</value>
    public Microseconds Deviation => this.deviation == Microseconds.MinValue
      ? (this.deviation = GetDeviation())
      : this.deviation;

    /// <summary>
    /// The deviation from the arithmetic mean converted to the base unit.
    /// </summary>
    /// <value>A positive or negative value to describe the deviation from the arithmetic mean. the value is converted from microseconds to the base unit defined by the <see cref="BaseUnit"/> property.</value>
    public double DeviationConverted => TimeValueConverter.ConvertTo(this.BaseUnit, this.Deviation, true);

    /// <summary>
    /// The base unit used to calculate the values for <see cref="DeviationConverted"/> and <see cref="ElapsedTimeConverted"/>.
    /// </summary>
    public TimeUnit BaseUnit => this.Owner?.BaseUnit ?? TimeUnit.Microseconds;

    /// <summary>
    /// In case the benchmarked operation is an async method, <see cref="IsProfiledTaskCancelled"/> indicates whether the <see langword="async"/>operation was cancelled or not.
    /// </summary>
    /// <value><see langword="true"/> in case the operation was cancelled (and the <see cref="ProfiledTask"/> result is in a cancelled state (<see cref="Task.Status"/> returns <see cref="TaskStatus.Canceled"/>).</value>
    public bool IsProfiledTaskCancelled { get; }

    /// <summary>
    /// In case of a multi-run benchmark run (e.g. by calling <see cref="Profiler.LogTime(Action, int, TimeUnit, string, int)"/>), the property <see cref="Iteration"/> returns the run's actual number the <see cref="ProfilerResult"/> is associated with.
    /// </summary>
    /// <value>The number of the run the <see cref="ProfilerResult"/> is associated with. Otherwise <c>-1</c>.</value>
    /// <remarks>
    /// In case of a attributed benchmark run (e.g. by calling <see cref="Profiler.CreateProfilerBuilder(Type)"/>), the property <see cref="Iteration"/> returns the number of runs for each argument list (defined using the <see cref="ProfilerMethodArgumentAttribute"/> attribute) 
    /// <br/>and the <see cref="ArgumentListIndex"/> property returns the index of the argument list that the <see cref="ProfilerResult"/> is associated with.
    /// </remarks>
    public int Iteration { get; }

    /// <summary>
    /// In case of a attributed benchmark run (e.g. by calling <see cref="Profiler.CreateProfilerBuilder(Type)"/>), the property returns the index of the argument list the <see cref="ProfilerResult"/> is associated with.
    /// </summary>
    /// <value>The index of the argument list the <see cref="ProfilerResult"/> is associated with. Otherwise <c>-1</c>.</value>
    /// <remarks>
    /// In case of a attributed benchmark run (e.g. by calling <see cref="Profiler.CreateProfilerBuilder(Type)"/>), the property <see cref="Iteration"/> returns the number of runs for each argument list (defined using the <see cref="ProfilerMethodArgumentAttribute"/> attribute) 
    /// <br/>and the <see cref="ArgumentListIndex"/> property returns the index of the argument list that the <see cref="ProfilerResult"/> is associated with.
    /// </remarks>
    public int ArgumentListIndex { get; }

    internal ProfilerBatchResult Owner { get; }

    /// <inheritdoc/>
    public int CompareTo(ProfilerResult other) => this.ElapsedTime.CompareTo(other.ElapsedTime);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ProfilerResult result && Equals(result);

    /// <inheritdoc/>
    public bool Equals(ProfilerResult other) => this.ElapsedTime.Equals(other.ElapsedTime) 
      && this.IsProfiledTaskCancelled == other.IsProfiledTaskCancelled 
      && this.Iteration == other.Iteration
      && this.Owner == other.Owner
      && this.ArgumentListIndex == other.ArgumentListIndex
      && this.BaseUnit.Equals(other.BaseUnit)
      && this.Deviation.Equals(other.Deviation);

    /// <inheritdoc/>
#if NET || NETSTANDARD2_1_OR_GREATER
    public override int GetHashCode() => HashCode.Combine(this.ElapsedTime, 
      this.IsProfiledTaskCancelled, 
      this.Iteration, 
      this.Owner, 
      this.ArgumentListIndex, 
      this.BaseUnit, 
      this.Deviation);
#else
    public override int GetHashCode()
    {
      int hashCode = 208172843;
      hashCode = (hashCode * -1521134295) + this.ElapsedTime.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.IsProfiledTaskCancelled.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.Iteration.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.Owner.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.ArgumentListIndex.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.BaseUnit.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.Deviation.GetHashCode();
      return hashCode;
    }
#endif

    public static bool operator <(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) < 0;
    public static bool operator <=(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) <= 0;
    public static bool operator >(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) > 0;
    public static bool operator >=(ProfilerResult left, ProfilerResult right) => left.CompareTo(right) >= 0;
  }
}