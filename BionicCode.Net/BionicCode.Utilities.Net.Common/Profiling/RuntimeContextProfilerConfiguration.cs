namespace BionicCode.Utilities.Net.Profiling
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal class RuntimeContextProfilerConfiguration : IAttributeProfilerConfiguration
  {
    public RuntimeContextProfilerConfiguration(Runtime runtime, IEnumerable<Type> types, IAttributeProfilerConfiguration profilerConfigurationToCopy)
    {
      this.Runtime = runtime;
      this.Types = types;
      this.IsWarmupEnabled = profilerConfigurationToCopy.IsWarmupEnabled;
      this.IsDefaultLogOutputEnabled = profilerConfigurationToCopy.IsDefaultLogOutputEnabled;
      this.Iterations = profilerConfigurationToCopy.Iterations;
      this.WarmupIterations = profilerConfigurationToCopy.WarmupIterations;
      this.BaseUnit = profilerConfigurationToCopy.BaseUnit;
      this.AsyncProfilerLogger = profilerConfigurationToCopy.AsyncProfilerLogger;
      this.ProfilerLogger = profilerConfigurationToCopy.ProfilerLogger;
    }

    public Runtime Runtime { get; }
    public IEnumerable<Type> Types { get; }
    public bool IsWarmupEnabled { get; }
    public bool IsDefaultLogOutputEnabled { get; }
    public int Iterations { get; }
    public int WarmupIterations { get; }
    public TimeUnit BaseUnit { get; }
    public ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; }
    public ProfilerLoggerDelegate ProfilerLogger { get; }

    public Assembly GetAssembly(Type type) => throw new NotImplementedException();
  }
}