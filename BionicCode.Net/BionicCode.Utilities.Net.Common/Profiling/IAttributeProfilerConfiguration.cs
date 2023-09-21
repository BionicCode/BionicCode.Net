namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal interface IAttributeProfilerConfiguration
  {
    Assembly GetAssembly(Type type);
    IEnumerable<Type> Type { get; }
    //Assembly TypeAssembly { get; }
    bool IsWarmupEnabled { get; }
    bool IsDefaultLogOutputEnabled { get; }
    int Iterations { get; }
    int WarmupIterations { get; }
    TimeUnit BaseUnit { get; }
    ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; }
    ProfilerLoggerDelegate ProfilerLogger { get; }
  }
}