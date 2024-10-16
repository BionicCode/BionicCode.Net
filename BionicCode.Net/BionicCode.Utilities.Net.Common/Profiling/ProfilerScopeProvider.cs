namespace BionicCode.Utilities.Net
{
  using System;
  using System.Diagnostics;
  using System.Text;
  using System.Threading.Tasks;

  internal class ProfilerScopeProvider
  {
    public Action<ProfilerBatchResult, string> Logger { get; }
    public Func<ProfilerBatchResult, string, Task> AsyncLogger { get; }
    public ProfilerBatchResult Result { get; }

    internal ProfilerScopeProvider(Action<ProfilerBatchResult, string> logger, ProfilerContext profilerContext)
    {
      this.Logger = logger;
      this.Result = new ProfilerBatchResult(DateTime.Now, profilerContext);
    }

    internal ProfilerScopeProvider(Func<ProfilerBatchResult, string, Task> asyncLogger, ProfilerContext profilerContext)
    {
      this.AsyncLogger = asyncLogger;
      this.Result = new ProfilerBatchResult(DateTime.Now, profilerContext);
    }

    internal IDisposable StartProfiling(out ProfilerBatchResult profilerBatchResult)
    {
      profilerBatchResult = this.Result;
      var scope = new ProfilerScope(this);

      return scope;
    }

    internal IAsyncDisposable StartProfilingAsync(out ProfilerBatchResult profilerBatchResult)
    {
      profilerBatchResult = this.Result;
      var scope = new ProfilerScope(this);

      return scope;
    }

    #region ProfilerScope class

    private class ProfilerScope : IDisposable, IAsyncDisposable
    {
      public bool IsDisposed { get; private set; }
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
        if (!this.IsDisposed)
        {
          if (disposing)
          {
            this.Stopwatch.Stop();

            var iterationResult = new ProfilerResult(1, this.Stopwatch.Elapsed, this.ScopeProvider.Result, -1);

            this.ScopeProvider.Result.AddResult(iterationResult);
            this.ScopeProvider.Result.TotalDuration = this.Stopwatch.Elapsed;
            this.ScopeProvider.Result.AverageDuration = this.Stopwatch.Elapsed;
            this.ScopeProvider.Result.MinResult = iterationResult;
            this.ScopeProvider.Result.MaxResult = iterationResult;

            this.ScopeProvider.Logger?.Invoke(this.ScopeProvider.Result, this.ScopeProvider.Result.Summary);
            if (this.ScopeProvider.AsyncLogger != null)
            {
              await this.ScopeProvider.AsyncLogger.Invoke(this.ScopeProvider.Result, this.ScopeProvider.Result.Summary);
            }
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          this.IsDisposed = true;
        }
      }

      protected virtual async Task DisposeAsync(bool disposing)
      {
        if (!this.IsDisposed)
        {
          if (disposing)
          {
            this.Stopwatch.Stop();

            var iterationResult = new ProfilerResult(1, this.Stopwatch.Elapsed, this.ScopeProvider.Result, -1);

            this.ScopeProvider.Result.AddResult(iterationResult);
            this.ScopeProvider.Result.TotalDuration = this.Stopwatch.Elapsed;
            this.ScopeProvider.Result.AverageDuration = this.Stopwatch.Elapsed;
            this.ScopeProvider.Result.MinResult = iterationResult;
            this.ScopeProvider.Result.MaxResult = iterationResult;

            this.ScopeProvider.Logger?.Invoke(this.ScopeProvider.Result, this.ScopeProvider.Result.Summary);
            if (this.ScopeProvider.AsyncLogger != null)
            {
              await this.ScopeProvider.AsyncLogger.Invoke(this.ScopeProvider.Result, this.ScopeProvider.Result.Summary);
            }
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          this.IsDisposed = true;
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

      public async ValueTask DisposeAsync()
      {
        await DisposeAsync(disposing: true);
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
      }
    }
    #endregion
  }
}
