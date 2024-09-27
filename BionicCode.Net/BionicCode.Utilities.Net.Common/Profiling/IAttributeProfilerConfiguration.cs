namespace BionicCode.Utilities.Net.Profiling
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Threading.Tasks;

  internal interface IAttributeProfilerConfiguration
  {
    Runtime Runtime { get; }
    IEnumerable<TypeData> TypeData { get; }
    bool IsWarmupEnabled { get; }
    bool IsDefaultLogOutputEnabled { get; }
    int Iterations { get; }
    int WarmupIterations { get; }
    TimeUnit BaseUnit { get; }
    Func<ProfilerBatchResult, string, Task> AsyncProfilerLogger { get; }
    Action<ProfilerBatchResult, string> ProfilerLogger { get; }
  }
}