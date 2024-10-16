namespace BionicCode.Utilities.Net.Profiling
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Reflection;
  using System.Threading.Tasks;

  internal class RuntimeContextProfilerConfiguration : IAttributeProfilerConfiguration
  {
    public RuntimeContextProfilerConfiguration(Runtime runtime, IEnumerable<TypeData> typeData, IAttributeProfilerConfiguration profilerConfigurationToCopy)
    {
      this.Runtime = runtime;
      this.TypeData = new HashSet<TypeData>(typeData);
      this.IsWarmupEnabled = profilerConfigurationToCopy.IsWarmupEnabled;
      this.IsDefaultLogOutputEnabled = profilerConfigurationToCopy.IsDefaultLogOutputEnabled;
      this.Iterations = profilerConfigurationToCopy.Iterations;
      this.WarmupIterations = profilerConfigurationToCopy.WarmupIterations;
      this.BaseUnit = profilerConfigurationToCopy.BaseUnit;
      this.AsyncProfilerLogger = profilerConfigurationToCopy.AsyncProfilerLogger;
      this.ProfilerLogger = profilerConfigurationToCopy.ProfilerLogger;
      this.AutoDiscoverSourceAssemblies = Array.Empty<Assembly>();
      this.IsAutoDiscoverEnabled = false;
    }

    public Runtime Runtime { get; }
    public HashSet<TypeData> TypeData { get; }
    public bool IsWarmupEnabled { get; }
    public bool IsDefaultLogOutputEnabled { get; }
    public int Iterations { get; }
    public int WarmupIterations { get; }
    public TimeUnit BaseUnit { get; }
    public Func<ProfilerBatchResult, string, Task> AsyncProfilerLogger { get; }
    public Action<ProfilerBatchResult, string> ProfilerLogger { get; }
    public bool IsAutoDiscoverEnabled { get; }
    public Assembly[] AutoDiscoverSourceAssemblies { get; }

    public Assembly GetAssembly(Type type) => throw new NotImplementedException();
  }
}