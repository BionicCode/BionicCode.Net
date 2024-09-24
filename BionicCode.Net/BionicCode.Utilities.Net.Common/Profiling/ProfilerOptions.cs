namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;

  /// <summary>
  /// Configures the <see cref="Profiler"/>.
  /// </summary>
  public class ProfilerOptions
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// Initializes the instance with 100 iterations, and the default number of warmup iteration (defined by <see cref="Profiler.DefaultWarmupCount"/>) and with the default time unit (defined by <see cref="Profiler.DefaultBaseUnit"/>).
    /// </remarks>
    public ProfilerOptions() : this(1000, Profiler.DefaultWarmupCount, null, Profiler.DefaultBaseUnit)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="iterations">The number of iterations to execute the profiled code.</param>
    /// <param name="warmupIterations">The number of warmup iterations.</param>
    /// <param name="logger">A synchronous logging delegate.</param>
    /// <param name="baseUnit">The desired base unit.</param>
    public ProfilerOptions(int iterations, int warmupIterations, ProfilerLogger logger, TimeUnit baseUnit)
    {
      this.Iterations = iterations;
      this.WarmupIterations = warmupIterations;
      this.Logger = logger;
      this.BaseUnit = baseUnit;
    }

    /// <summary>
    /// The number of iterations that the profiled code is executed.
    /// </summary>
    /// <value>
    /// An integer number greater than <c>0</c> or <c>0</c> to perform no profiling. The <see cref="Profiler"/> will throw an exception if the value is less than <c>0</c>. The default is defined by <see cref="Profiler.DefaultIterationCount"/>.
    /// </value>
    public int Iterations { get; set; }

    /// <summary>
    /// The number of iterations that the profiled code is executed before the actual profiling.
    /// </summary>
    /// <value> 
    /// An integer number greater than <c>0</c> or <c>0</c> to perform no warmup (cold start). The <see cref="Profiler"/> will throw an exception if the value is less than <c>0</c>.  default is defined by <see cref="Profiler.DefaultWarmupCount"/>.
    /// </value>
    public int WarmupIterations { get; set; }

    /// <summary>
    /// A delegate to handle the result.
    /// </summary>
    /// <remarks>
    /// Use the async API of the <see cref="Profiler"/> if you need to define an async delegate.
    /// </remarks>
    public ProfilerLogger Logger { get; set; }

    /// <summary>
    /// The default base unit that all results are transformed to.
    /// </summary>
    /// <remarks>
    /// Internally time is recorded in microseconds and then converted to the desired base unit (e.g., ms). The usual timer resolution on most modern machines is 100 ns (see <see cref="TimeSpan.Ticks"/> to learn more. Call the static <see cref="Stopwatch.IsHighResolution"/> field to know if the executing machine supports that high resolution.)
    /// </remarks>
    /// <value>
    /// The default is defined by <see cref="Profiler.DefaultBaseUnit"/>.
    /// </value>
    public TimeUnit BaseUnit { get; set; }

    /// <summary>
    /// Defines the environment that the profiled code is executed in.
    /// </summary>
    /// <value>The desired runtime. The default is <see cref="Runtime.Default"/> which evaluates to the highest supported .NET version. 
    /// <br/>See <see cref="Runtime"/> to know the currently supported runtimes.</value>
    public Runtime Runtime { get; set; }
  }
}
