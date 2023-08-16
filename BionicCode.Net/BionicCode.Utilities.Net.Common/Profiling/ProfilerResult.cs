namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Threading.Tasks;

  /// <summary>
  /// The result of a single benchmark run or iteration.
  /// </summary>
  public readonly struct ProfilerResult : IEquatable<ProfilerResult>, IComparable<ProfilerResult>
  {
    internal static ProfilerResult Empty => new ProfilerResult(-1, TimeSpan.Zero);

    internal ProfilerResult(int iteration, TimeSpan elapsedTime)
    {
      this.Iteration = iteration;
      this.ProfiledTask = Task.CompletedTask;
      this.IsProfiledTaskCancelled = false;
      this.ElapsedTime = elapsedTime;
    }

    internal ProfilerResult(int iteration, Task profiledTask, bool isProfiledTaskCancelled, TimeSpan elapsedTime)
    {
      this.Iteration = iteration;
      this.ProfiledTask = profiledTask;
      this.IsProfiledTaskCancelled = isProfiledTaskCancelled;
      this.ElapsedTime = elapsedTime;
    }

    /// <summary>
    /// The duration of the benchmark run.
    /// </summary>
    /// <value>The duration.</value>
    public TimeSpan ElapsedTime { get; }

    /// <summary>
    /// In case the benchmarked operation is an async method, <see cref="IsProfiledTaskCancelled"/> indiocates wwhether the <see langword="async"/>operation was cancelled or not.
    /// </summary>
    /// <value><see langword="true"/> in case the operation wwwas cancelled (and the <see cref="ProfiledTask"/> result is in a canlled state (<see cref="Task.Status"/> returns <see cref="TaskStatus.Canceled"/>).</value>
    public bool IsProfiledTaskCancelled { get; }

    /// <summary>
    /// In case of a multi-run benchmark run (e.g. by calling <see cref="Profiler.LogTime(Action, int)"/>), the property <see cref="Iteration"/> returns the run's actual number the <see cref="ProfilerResult"/> is associated with.
    /// </summary>
    /// <value>The number of the run the <see cref="ProfilerResult"/> is associated with. Otherwise <c>-1</c>.</value>
    public int Iteration { get; }

    /// <summary>
    /// The <see cref="Task"/> returned from the async operation that was benchmarked.
    /// </summary>
    /// <value>The <see cref="Task"/> result.</value>
    public Task ProfiledTask { get; }

    /// <inheritdoc/>
    public int CompareTo(ProfilerResult other) => other == default 
      ? 1 
      : this.ElapsedTime.CompareTo(other.ElapsedTime);

    /// <inheritdoc/>
    public override bool Equals(object obj) => obj is ProfilerResult result && Equals(result);

    /// <inheritdoc/>
    public bool Equals(ProfilerResult other) => this.ElapsedTime.Equals(other.ElapsedTime) && this.IsProfiledTaskCancelled == other.IsProfiledTaskCancelled && this.Iteration == other.Iteration && EqualityComparer<Task>.Default.Equals(this.ProfiledTask, other.ProfiledTask);

    /// <inheritdoc/>
#if NET || NETSTANDARD2_1_OR_GREATER
    public override int GetHashCode() => HashCode.Combine(this.ElapsedTime, this.IsProfiledTaskCancelled, this.Iteration, this.ProfiledTask);
#else
    public override int GetHashCode()
#pragma warning restore IDE0070 // Use 'System.HashCode'
    {
      int hashCode = 208172843;
      hashCode = hashCode * -1521134295 + this.ElapsedTime.GetHashCode();
      hashCode = hashCode * -1521134295 + this.IsProfiledTaskCancelled.GetHashCode();
      hashCode = hashCode * -1521134295 + this.Iteration.GetHashCode();
      hashCode = hashCode * -1521134295 + EqualityComparer<Task>.Default.GetHashCode(this.ProfiledTask);
      return hashCode;
    }
#endif

    public static bool operator <(ProfilerResult left, ProfilerResult right) => left.ElapsedTime < right.ElapsedTime;
    public static bool operator >(ProfilerResult left, ProfilerResult right) => left.ElapsedTime > right.ElapsedTime;
    public static bool operator >=(ProfilerResult left, ProfilerResult right) => left.ElapsedTime >= right.ElapsedTime;
    public static bool operator <=(ProfilerResult left, ProfilerResult right) => left.ElapsedTime <= right.ElapsedTime;
    public static bool operator ==(ProfilerResult left, ProfilerResult right) => left.Equals(right);
    public static bool operator !=(ProfilerResult left, ProfilerResult right) => !(left == right);
  }
}