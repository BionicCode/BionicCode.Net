namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Configures the <see cref="Profiler"/> to use with async API.
  /// </summary>
  public class AsyncProfilerOptions : ProfilerOptions
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    /// <remarks>
    /// Initializes the instance with 100 iterations, and the default number of warmup iteration (defined by <see cref="Profiler.DefaultWarmupCount"/>) and with the default time unit (defined by <see cref="Profiler.DefaultBaseUnit"/>).
    /// </remarks>
    public AsyncProfilerOptions() : this(1000, Profiler.DefaultWarmupCount, null, Profiler.DefaultBaseUnit)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="iterations">The number of iterations to execute the profiled code.</param>
    /// <param name="warmupIterations">The number of warmup iterations.</param>
    /// <param name="logger">A synchronous logging delegate.</param>
    /// <param name="baseUnit">The desired base unit.</param>
    public AsyncProfilerOptions(int iterations, int warmupIterations, ProfilerLogger logger, TimeUnit baseUnit) : base(iterations, warmupIterations, logger, baseUnit)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="iterations">The number of iterations to execute the profiled code.</param>
    /// <param name="warmupIterations">The number of warmup iterations.</param>
    /// <param name="asyncLogger">An asynchronous logging delegate.</param>
    /// <param name="baseUnit">The desired base unit.</param>
    public AsyncProfilerOptions(int iterations, int warmupIterations, ProfilerLoggerAsyncDelegate asyncLogger, TimeUnit baseUnit) : base(iterations, warmupIterations, null, baseUnit) => this.AsyncLogger = asyncLogger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="iterations">The number of iterations to execute the profiled code.</param>
    /// <param name="warmupIterations">The number of warmup iterations.</param>
    /// <param name="logger">A synchronous logging delegate.</param>
    /// <param name="asyncLogger">An asynchronous logging delegate.</param>
    /// <param name="baseUnit">The desired base unit.</param>
    public AsyncProfilerOptions(int iterations, int warmupIterations, ProfilerLogger logger, ProfilerLoggerAsyncDelegate asyncLogger, TimeUnit baseUnit) : base(iterations, warmupIterations, logger, baseUnit) => this.AsyncLogger = asyncLogger;

    /// <summary>
    /// An asynchronous delegate to handle the result.
    /// </summary>
    /// <remarks>
    /// The async profiler API will use either of the defined loggers. When both <see cref="AsyncLogger"/> and <see cref="ProfilerOptions.Logger"/> are defined, both will be executed.
    /// </remarks>
    public ProfilerLoggerAsyncDelegate AsyncLogger { get; set; }
  }
}
