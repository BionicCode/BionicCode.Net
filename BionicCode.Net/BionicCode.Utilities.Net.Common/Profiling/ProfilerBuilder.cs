namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using System.Runtime.InteropServices;
  using System.Text;
  using System.Threading.Tasks;

  /// <summary>
  /// Class to configure the <see cref="Profiler"/> when used with annotated profiling. Call <see cref="Profiler.LogType{T}"/> to obtain an instance.
  /// </summary>
  public class ProfilerBuilder : IAttributeProfilerConfiguration
  {
    private Type Type { get; }
    private bool IsWarmUpEnabled { get; set; }
    private bool IsDefaultLogOutputEnabled { get; set; }
    private int Iterations { get; set; }
    private int WarmUpIterations { get; set; }
    private Assembly TypeAssembly { get; }
    private ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; set; }
    private ProfilerLoggerDelegate ProfilerLogger { get; set; }

    Type IAttributeProfilerConfiguration.Type => this.Type;
    Assembly IAttributeProfilerConfiguration.TypeAssembly => this.TypeAssembly;
    bool IAttributeProfilerConfiguration.IsWarmupEnabled => this.IsWarmUpEnabled;
    bool IAttributeProfilerConfiguration.IsDefaultLogOutputEnabled => this.IsDefaultLogOutputEnabled;
    int IAttributeProfilerConfiguration.Iterations => this.Iterations;
    int IAttributeProfilerConfiguration.WarmUpIterations => this.WarmUpIterations;
    ProfilerLoggerAsyncDelegate IAttributeProfilerConfiguration.AsyncProfilerLogger => this.AsyncProfilerLogger;
    ProfilerLoggerDelegate IAttributeProfilerConfiguration.ProfilerLogger => this.ProfilerLogger;

    internal ProfilerBuilder(Type targetType)
    {
      this.Type = targetType;
      this.TypeAssembly = Assembly.GetAssembly(targetType);
      this.IsWarmUpEnabled = true;
      this.IsDefaultLogOutputEnabled = true;
      this.WarmUpIterations = Profiler.WarmUpCount;
      this.Iterations = 1;
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
      this.Iterations = iterations;
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
    /// <param name="warmUpIterations">The number of iterations to perform before starting the profiling. The default is <c>4</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder EnableWarmUp(int warmUpIterations = 4)
    {
      this.WarmUpIterations = warmUpIterations;
      this.IsWarmUpEnabled = true;
      return this;
    }

    /// <summary>
    /// Disable warm up iterations. Warm up iterations can be scheduled to trigger the JIT compiler. Warm up is enabled by default.
    /// </summary>
    /// <returns>The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.</returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder DisableWarmUp()
    {
      this.IsWarmUpEnabled = false;
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
    /// <param name="warmUpIterations">The number of iterations to perform before starting the profiling. The default is <c>4</c>.</param>
    /// <returns>
    /// The currently configured <see cref="ProfilerBuilder"/> instance to enable to chain calls.
    /// </returns>
    /// <remarks>When running code the first time there is always the incurrence of the JIT to compile the code. 
    /// <br/>For this reason it is recommended to execute the code at least once in order to avoid the JIT to impact the profiling.
    /// </remarks>
    public ProfilerBuilder SetWarmUpIterations(int warmUpIterations)
    {
      this.WarmUpIterations = warmUpIterations;
      return this;
    }

    /// <summary>
    /// Execute the profiler using the current configuration.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ProfilerBatchResult"/> items where each <see cref="ProfilerBatchResult"/> holds the result of a particular profiled target to accumulate the individual <see cref="ProfilerResult"/> items for each iteration.
    /// </returns>
    public async Task<IEnumerable<ProfilerBatchResult>> RunAsync()
    {
      var attributeProfiler = new AttributeProfiler(this);
      IEnumerable<ProfilerBatchResult> result = await attributeProfiler.StartAsync();
      return result;
    }
  }
}