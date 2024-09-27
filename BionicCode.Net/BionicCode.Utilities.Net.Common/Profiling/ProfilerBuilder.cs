namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Threading;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net.Profiling;

  /// <summary>
  /// Class to configure the <see cref="Profiler"/> when used with annotated profiling. Call <see cref="Profiler.CreateProfilerBuilder{T}"/> to obtain an instance.
  /// </summary>
  public class ProfilerBuilder : IAttributeProfilerConfiguration
  {
    private IEnumerable<TypeData> TypeData { get; }
    private bool IsWarmupEnabled { get; set; }
    private bool IsDefaultLogOutputEnabled { get; set; }
    private int Iterations { get; set; }
    private int WarmupIterations { get; set; }
    private TimeUnit BaseUnit { get; set; }
    //private Dictionary<Type, Assembly> TypeAssemblyTable { get; }
    private Func<ProfilerBatchResult, string, Task> AsyncProfilerLogger { get; set; }
    private Action<ProfilerBatchResult, string> ProfilerLogger { get; set; }
    private Runtime Runtime { get; set; }

    IEnumerable<TypeData> IAttributeProfilerConfiguration.TypeData => this.TypeData;
    //Assembly IAttributeProfilerConfiguration.TypeAssembly => this.TypeAssembly;
    TimeUnit IAttributeProfilerConfiguration.BaseUnit => this.BaseUnit;
    bool IAttributeProfilerConfiguration.IsWarmupEnabled => this.IsWarmupEnabled;
    bool IAttributeProfilerConfiguration.IsDefaultLogOutputEnabled => this.IsDefaultLogOutputEnabled;
    int IAttributeProfilerConfiguration.Iterations => this.Iterations;
    int IAttributeProfilerConfiguration.WarmupIterations => this.WarmupIterations;
    Func<ProfilerBatchResult, string, Task> IAttributeProfilerConfiguration.AsyncProfilerLogger => this.AsyncProfilerLogger;
    Action<ProfilerBatchResult, string> IAttributeProfilerConfiguration.ProfilerLogger => this.ProfilerLogger;
    Runtime IAttributeProfilerConfiguration.Runtime => this.Runtime;

    internal ProfilerBuilder(TypeData targetType) : this(new[] { targetType })
    {
    }

    internal ProfilerBuilder(IEnumerable<TypeData> targetTypes)
    {
      this.TypeData = targetTypes;
      this.IsWarmupEnabled = true;
      this.IsDefaultLogOutputEnabled = true;
      this.WarmupIterations = Profiler.DefaultWarmupCount;
      this.Iterations = Profiler.DefaultIterationsCount;
      this.BaseUnit = TimeUnit.Microseconds;
      this.Runtime = Runtime.Current;
    }

    /// <summary>
    /// Set a log delegate that allows to output the result to a sink, e.g. a file or application logger.
    /// </summary>
    /// <param name="iterations">The number of iterations to perform when executing the target code. The default is <c>1</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetIterations(int iterations)
    {
      if (iterations < 0)
      {
        throw new ArgumentOutOfRangeException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerRunCount(), nameof(iterations));
      }

      this.Iterations = iterations;
      return this;
    }

    /// <summary>
    /// Set the time unit that the results are converted to.
    /// </summary>
    /// <param name="timeUnit">The unit that all result related time ispresented in. The default is <see cref="TimeUnit.Microseconds"/>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetBaseUnit(TimeUnit timeUnit)
    {
      this.BaseUnit = timeUnit;
      return this;
    }

    /// <summary>
    /// Set a log delegate that allows to output the result to a sink, e.g. a file or application logger.
    /// </summary>
    /// <param name="profilerLogger">A delegate that is invoked by profiler to pass in the result.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetLogger(ProfilerLoggerDelegate profilerLogger)
    {
      this.ProfilerLogger = profilerLogger;
      return this;
    }

    /// <summary>
    /// Set a log delegate that allows to asynchronously output the result to a sink, e.g. a file or application logger.
    /// </summary>
    /// <param name="profilerLogger">An asynchronous delegate that is invoked by profiler to pass in the result.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    public ProfilerBuilder SetAsyncLogger(ProfilerLoggerAsyncDelegate profilerLogger)
    {
      this.AsyncProfilerLogger = profilerLogger;
      return this;
    }

    /// <summary>
    /// Enable warm up iterations to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <param name="warmupIterations">The number of iterations to perform before starting the profiling. The default is <c>4</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder EnableWarmup()
    {
      this.IsWarmupEnabled = true;
      return this;
    }

    /// <summary>
    /// Disable warm up iterations. Warm up iterations can be scheduled to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <returns>The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.</returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder DisableWarmup()
    {
      this.IsWarmupEnabled = false;
      return this;
    }

    /// <summary>
    /// Enable default log output. The default is enabled.
    /// </summary>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>Enables to print the profiler results to a HTML document that will be automatically displayed in the default browser. 
    /// <br/>A second output sink is the default Output console window of Visual Studio, but only when in debug mode.
    /// </remarks>
    public ProfilerBuilder EnableDefaultLogOutput()
    {
      this.IsDefaultLogOutputEnabled = true;
      return this;
    }

    /// <summary>
    /// Disable default log output.
    /// </summary>
    /// <returns>The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.</returns>
    public ProfilerBuilder DisableDefaultLogOutput()
    {
      this.IsDefaultLogOutputEnabled = false;
      return this;
    }

    /// <summary>
    /// Enable warm up iterations to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <param name="warmupIterations">The number of iterations to perform before starting the profiling. The default is <c>4</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder SetWarmupIterations(int warmupIterations)
    {
      if (warmupIterations < 0)
      {
        throw new ArgumentOutOfRangeException(ExceptionMessages.GetArgumentExceptionMessage_ProfilerWarmupCount(), nameof(warmupIterations));
      }

      this.WarmupIterations = warmupIterations;
      return this;
    }

    public ProfilerBuilder SetRuntime(Runtime runtime)
    {
      this.Runtime = runtime;
      return this;
    }

    /// <summary>
    /// Execute the profiler using the current configuration.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ProfilerBatchResult"/> items where each <see cref="ProfilerBatchResult"/> holds the result of a particular profiled target to accumulate the individual <see cref="ProfilerResult"/> items for each iteration.
    /// </returns>
    public async Task<ProfiledTypeResultCollection> RunAsync(CancellationToken cancellationToken)
    {
      var attributeProfiler = new AttributeProfiler(this);
      ProfiledTypeResultCollection result = await attributeProfiler.StartAsync(cancellationToken);
      return result;
    }
  }
}