namespace BionicCode.Utilities.Net.Profiling;

using System;
using System.Diagnostics;
using System.Threading.Tasks;

internal class ProfilerScopeProvider
{
    public Action<ProfilerBatchResult, string>? Logger { get; }
    public Func<ProfilerBatchResult, string, Task>? AsyncLogger { get; }
    public ProfilerBatchResult Result { get; }

    internal ProfilerScopeProvider(Action<ProfilerBatchResult, string>? logger, ProfilerContext profilerContext)
    {
        Logger = logger;
        Result = new ProfilerBatchResult(DateTime.Now, profilerContext);
    }

    internal ProfilerScopeProvider(Func<ProfilerBatchResult, string, Task>? asyncLogger, ProfilerContext profilerContext)
    {
        AsyncLogger = asyncLogger;
        Result = new ProfilerBatchResult(DateTime.Now, profilerContext);
    }

    internal IDisposable StartProfiling(out ProfilerBatchResult profilerBatchResult)
    {
        profilerBatchResult = Result;
        var scope = new ProfilerScope(this);

        return scope;
    }

    internal IAsyncDisposable StartProfilingAsync(out ProfilerBatchResult profilerBatchResult)
    {
        profilerBatchResult = Result;
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
            Stopwatch = new Stopwatch();
            ScopeProvider = scopeProvider;
            Start();
        }

        private void Start() => Stopwatch.Start();

        protected virtual async void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Stopwatch.Stop();

                    var iterationResult = new ProfilerResult(1, Stopwatch.Elapsed, ScopeProvider.Result, -1);

                    ScopeProvider.Result.AddResult(iterationResult);
                    ScopeProvider.Result.TotalDuration = Stopwatch.Elapsed;
                    ScopeProvider.Result.AverageDuration = Stopwatch.Elapsed;
                    ScopeProvider.Result.MinResult = iterationResult;
                    ScopeProvider.Result.MaxResult = iterationResult;

                    ScopeProvider.Logger?.Invoke(ScopeProvider.Result, ScopeProvider.Result.Summary);
                    if (ScopeProvider.AsyncLogger != null)
                    {
                        await ScopeProvider.AsyncLogger.Invoke(ScopeProvider.Result, ScopeProvider.Result.Summary);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                IsDisposed = true;
            }
        }

        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    Stopwatch.Stop();

                    var iterationResult = new ProfilerResult(1, Stopwatch.Elapsed, ScopeProvider.Result, -1);

                    ScopeProvider.Result.AddResult(iterationResult);
                    ScopeProvider.Result.TotalDuration = Stopwatch.Elapsed;
                    ScopeProvider.Result.AverageDuration = Stopwatch.Elapsed;
                    ScopeProvider.Result.MinResult = iterationResult;
                    ScopeProvider.Result.MaxResult = iterationResult;

                    ScopeProvider.Logger?.Invoke(ScopeProvider.Result, ScopeProvider.Result.Summary);
                    if (ScopeProvider.AsyncLogger != null)
                    {
                        await ScopeProvider.AsyncLogger.Invoke(ScopeProvider.Result, ScopeProvider.Result.Summary);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                IsDisposed = true;
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
