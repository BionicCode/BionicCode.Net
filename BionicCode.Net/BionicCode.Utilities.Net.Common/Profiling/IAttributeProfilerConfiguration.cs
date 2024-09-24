namespace BionicCode.Utilities.Net.Profiling
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal interface IAttributeProfilerConfiguration
  {
    Runtime Runtime { get; }
    Assembly GetAssembly(Type type);
    IEnumerable<Type> Types { get; }
    bool IsWarmupEnabled { get; }
    bool IsDefaultLogOutputEnabled { get; }
    int Iterations { get; }
    int WarmupIterations { get; }
    TimeUnit BaseUnit { get; }
    ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; }
    ProfilerLogger ProfilerLogger { get; }
  }
}