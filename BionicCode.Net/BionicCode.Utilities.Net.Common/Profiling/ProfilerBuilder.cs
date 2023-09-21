﻿namespace BionicCode.Utilities.Net
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
  /// Class to configure the <see cref="Profiler"/> when used with annotated profiling. Call <see cref="Profiler.CreateProfilerBuilder{T}"/> to obtain an instance.
  /// </summary>
  public class ProfilerBuilder : IAttributeProfilerConfiguration
  {
    private IEnumerable<Type> Type { get; }
    private bool IsWarmupEnabled { get; set; }
    private bool IsDefaultLogOutputEnabled { get; set; }
    private int Iterations { get; set; }
    private int WarmupIterations { get; set; }
    private TimeUnit BaseUnit { get; set; }
    private Dictionary<Type, Assembly> TypeAssemblyTable { get; }
    private ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; set; }
    private ProfilerLoggerDelegate ProfilerLogger { get; set; }

    IEnumerable<Type> IAttributeProfilerConfiguration.Type => this.Type;
    //Assembly IAttributeProfilerConfiguration.TypeAssembly => this.TypeAssembly;
    TimeUnit IAttributeProfilerConfiguration.BaseUnit => this.BaseUnit;
    bool IAttributeProfilerConfiguration.IsWarmupEnabled => this.IsWarmupEnabled;
    bool IAttributeProfilerConfiguration.IsDefaultLogOutputEnabled => this.IsDefaultLogOutputEnabled;
    int IAttributeProfilerConfiguration.Iterations => this.Iterations;
    int IAttributeProfilerConfiguration.WarmupIterations => this.WarmupIterations;
    ProfilerLoggerAsyncDelegate IAttributeProfilerConfiguration.AsyncProfilerLogger => this.AsyncProfilerLogger;
    ProfilerLoggerDelegate IAttributeProfilerConfiguration.ProfilerLogger => this.ProfilerLogger;

    Assembly IAttributeProfilerConfiguration.GetAssembly(Type type)
    {
      if (!this.TypeAssemblyTable.TryGetValue(type, out Assembly assembly))
      { 
        assembly = Assembly.GetAssembly(type);
        this.TypeAssemblyTable.Add(type, assembly);
      }

      return assembly;
    }

    internal ProfilerBuilder(Type targetType) : this(new[] { targetType })
    {
    }

    internal ProfilerBuilder(IEnumerable<Type> targetTypes)
    {
      this.Type = targetTypes;
      this.TypeAssemblyTable = new Dictionary<Type, Assembly>();
      this.IsWarmupEnabled = true;
      this.IsDefaultLogOutputEnabled = true;
      this.WarmupIterations = Profiler.DefaultWarmupCount;
      this.Iterations = 1;
      this.BaseUnit = TimeUnit.Microseconds;
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
    public ProfilerBuilder EnableWarmup(int warmupIterations = 4)
    {
      this.WarmupIterations = warmupIterations;
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
      this.WarmupIterations = warmupIterations;
      return this;
    }

    /// <summary>
    /// Execute the profiler using the current configuration.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="ProfilerBatchResult"/> items where each <see cref="ProfilerBatchResult"/> holds the result of a particular profiled target to accumulate the individual <see cref="ProfilerResult"/> items for each iteration.
    /// </returns>
    public async Task<ProfiledTypeResultCollection> RunAsync()
    {
      var attributeProfiler = new AttributeProfiler(this);
      ProfiledTypeResultCollection result = await attributeProfiler.StartAsync();
      return result;
    }
  }
}