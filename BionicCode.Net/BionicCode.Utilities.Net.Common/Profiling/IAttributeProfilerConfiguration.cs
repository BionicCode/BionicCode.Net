namespace BionicCode.Utilities.Net
{
  using System;

  internal interface IAttributeProfilerConfiguration
  {
    Type Type { get; }
    bool IsWarmupEnabled { get; }
    bool IsDefaultLogOutputEnabled { get; }
    int Iterations { get; }
    int WarmUpIterations { get; }
    ProfilerLoggerAsyncDelegate AsyncProfilerLogger { get; }
    ProfilerLoggerDelegate ProfilerLogger { get; }
  }
}