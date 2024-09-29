namespace BionicCode.Utilities.Net
{
  using System;
  using System.Reflection;
  using System.Runtime.Versioning;
  using System.Threading.Tasks;

  // TODO::Move context related properties from ProfilerBtchResult to Context class and compose
  internal class ProfilerContext
  {
    public ProfilerContext(ProfilerTargetInvokeInfo methodInvokeInfo, string sourceFileName, int lineNumber, int warmupCount, int iterationCount, Runtime runtime, TimeUnit baseUnit, Action<ProfilerBatchResult, string> logger, Func<ProfilerBatchResult, string, Task> asyncLogger)
    {
      this.SourceFileName = sourceFileName;
      this.LineNumber = lineNumber;
      this.RuntimeVersionFactory = new Lazy<string>(() => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
      this.WarmupCount = warmupCount;
      this.IterationCount = iterationCount;
      this.Runtime = runtime;
      this.MethodInvokeInfo = methodInvokeInfo;
      this.BaseUnit = baseUnit;
      this.Logger = logger;
      this.AsyncLogger = asyncLogger;
    }

    public Runtime Runtime { get; }
    public string SourceFileName { get; }
    public int LineNumber { get; }
    public Lazy<string> RuntimeVersionFactory { get; }
    public string RuntimeVersion => this.RuntimeVersionFactory.Value;
    public int WarmupCount { get; }
    public int IterationCount { get; }
    public ProfilerTargetInvokeInfo MethodInvokeInfo { get; }
    public TimeUnit BaseUnit { get; }
    public Action<ProfilerBatchResult, string> Logger { get; }
    public Func<ProfilerBatchResult, string, Task> AsyncLogger { get; }
  }
}