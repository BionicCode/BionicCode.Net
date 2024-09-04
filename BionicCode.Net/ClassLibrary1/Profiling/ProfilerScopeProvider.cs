namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;
  using System.Text;

  internal class ProfilerScopeProvider
  {
    public ProfilerLoggerDelegate Logger { get; }
    public ProfilerLoggerAsyncDelegate AsyncLogger { get; }
    public ProfilerBatchResult Result { get; }

    internal ProfilerScopeProvider(ProfilerLoggerDelegate logger, ProfilerContext profilerContext, TimeUnit baseUnit)
    {
      this.Logger = logger;
      this.Result = new ProfilerBatchResult(1, DateTime.Now) { Context = profilerContext, BaseUnit = baseUnit };
    }

    internal ProfilerScopeProvider(ProfilerLoggerAsyncDelegate logger, ProfilerContext profilerContext, TimeUnit baseUnit)
    {
      this.AsyncLogger = logger;
      this.Result = new ProfilerBatchResult(1, DateTime.Now) { Context = profilerContext, BaseUnit = baseUnit };
    }

    public IDisposable StartProfiling(out ProfilerBatchResult profilerBatchResult)
    {
      profilerBatchResult = this.Result;
      var scope = new ProfilerScope(this);

      return scope;
    }

    #region ProfilerScope class

    private class ProfilerScope : IDisposable
    {
      private bool disposedValue;
      private Stopwatch Stopwatch { get; }
      private ProfilerScopeProvider ScopeProvider { get; }

      internal ProfilerScope(ProfilerScopeProvider scopeProvider)
      {
        this.Stopwatch = new Stopwatch();
        this.ScopeProvider = scopeProvider;
        Start();
      }

      private void Start() => this.Stopwatch.Start();

      protected virtual async void Dispose(bool disposing)
      {
        if (!this.disposedValue)
        {
          if (disposing)
          {
            this.Stopwatch.Stop();

            var iterationResult = new ProfilerResult(1, this.Stopwatch.Elapsed, this.ScopeProvider.Result.BaseUnit, this.ScopeProvider.Result, -1);

            this.ScopeProvider.Result.AddResult(iterationResult);
            this.ScopeProvider.Result.IterationCount = 1;
            this.ScopeProvider.Result.TotalDuration = this.Stopwatch.Elapsed;
            this.ScopeProvider.Result.AverageDuration = this.Stopwatch.Elapsed;
            this.ScopeProvider.Result.MinResult = iterationResult;
            this.ScopeProvider.Result.MaxResult = iterationResult;
            this.ScopeProvider.Result.BaseUnit = Profiler.DefaultBaseUnit;

            ProfilerContext context = this.ScopeProvider.Result.Context;
            var summaryBuilder = new StringBuilder();
            Profiler.BuildSummaryHeader(summaryBuilder, $"Target: scope (context: {context.TargetName})", context.TargetName, context.SourceFileName, context.LineNumber);
            Profiler.BuildSummaryEntry(summaryBuilder, iterationResult);
            Profiler.BuildSummaryFooter(summaryBuilder, this.ScopeProvider.Result);

            this.ScopeProvider.Logger?.Invoke(this.ScopeProvider.Result);
            if (this.ScopeProvider.AsyncLogger != null)
            {
              await this.ScopeProvider.AsyncLogger.Invoke(this.ScopeProvider.Result);
            }
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          this.disposedValue = true;
        }
      }

      // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
      // ~ProfilerScope()
      // {
      //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      //     Dispose(disposing: false);
      // }

      public void Dispose()
      {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
      }
    }
    #endregion
  }
}
