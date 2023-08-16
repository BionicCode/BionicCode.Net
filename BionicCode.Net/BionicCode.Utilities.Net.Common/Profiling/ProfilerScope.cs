namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;
  using System.Text;

  internal class ProfilerScope : IDisposable
  {
    private bool disposedValue;

    private ProfilerLoggerDelegate Logger { get; }
    private Stopwatch Stopwatch { get; }

    private ProfilerBatchResult Result { get; set; }

    internal ProfilerScope(ProfilerLoggerDelegate logger)
    {
      this.Logger = logger;
      this.Stopwatch = new Stopwatch();
    }

    internal void StartProfiling(in ProfilerBatchResult result)
    {
      this.Result = result;
      this.Stopwatch.Start();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          this.Stopwatch.Stop();

          var ireationResult = new ProfilerResult(1, this.Stopwatch.Elapsed);
          var summaryBuilder = new StringBuilder();
          Profiler.BuildSummaryHeader(summaryBuilder);
          Profiler.BuildSummaryEntry(summaryBuilder, 1, ireationResult);
          Profiler.BuildSummaryFooter(summaryBuilder, this.Stopwatch.Elapsed, this.Stopwatch.Elapsed, ireationResult, ireationResult);
          
          this.Result.Results = new[] { ireationResult };
          this.Result.HasCancelledProfiledTask = false;
          this.Result.IterationCount = 1;
          this.Result.TotalDuration = this.Stopwatch.Elapsed;
          this.Result.AverageDuration = this.Stopwatch.Elapsed;
          this.Result.Summary = summaryBuilder.ToString();
          this.Result.MinResult = ireationResult;
          this.Result.MaxResult = ireationResult;

          this.Logger?.Invoke(this.Result, summaryBuilder.ToString());
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
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
}
