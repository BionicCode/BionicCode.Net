namespace BionicCode.Utilities.Net.Profiling
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Threading.Tasks;

  internal class RuntimeContextProfilerConfiguration : IAttributeProfilerConfiguration
  {
    public RuntimeContextProfilerConfiguration(Runtime runtime, IEnumerable<TypeData> typeData, IAttributeProfilerConfiguration profilerConfigurationToCopy)
    {
      this.Runtime = runtime;
      this.TypeData = typeData;
      this.IsWarmupEnabled = profilerConfigurationToCopy.IsWarmupEnabled;
      this.IsDefaultLogOutputEnabled = profilerConfigurationToCopy.IsDefaultLogOutputEnabled;
      this.Iterations = profilerConfigurationToCopy.Iterations;
      this.WarmupIterations = profilerConfigurationToCopy.WarmupIterations;
      this.BaseUnit = profilerConfigurationToCopy.BaseUnit;
      this.AsyncProfilerLogger = profilerConfigurationToCopy.AsyncProfilerLogger;
      this.ProfilerLogger = profilerConfigurationToCopy.ProfilerLogger;
    }

    public Runtime Runtime { get; }
    public IEnumerable<TypeData> TypeData { get; }
    public bool IsWarmupEnabled { get; }
    public bool IsDefaultLogOutputEnabled { get; }
    public int Iterations { get; }
    public int WarmupIterations { get; }
    public TimeUnit BaseUnit { get; }
    public Func<ProfilerBatchResult, string, Task> AsyncProfilerLogger { get; }
    public Action<ProfilerBatchResult, string> ProfilerLogger { get; }

    public Assembly GetAssembly(Type type) => throw new NotImplementedException();
  }
}