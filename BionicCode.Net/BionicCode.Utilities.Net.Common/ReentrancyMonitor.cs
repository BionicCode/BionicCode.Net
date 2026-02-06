namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading;

    internal class ReentrancyMonitorEntry
    {
        public int Count => cancellationTokenSourcesInternal.Count;
        public ReadOnlyCollection<CancellationTokenSource> CancellationTokenSources { get; }
        private readonly List<CancellationTokenSource> cancellationTokenSourcesInternal;

        public ReentrancyMonitorEntry()
        {
            cancellationTokenSourcesInternal = new List<CancellationTokenSource>();
            CancellationTokenSources = new ReadOnlyCollection<CancellationTokenSource>(cancellationTokenSourcesInternal);
        }

        public void Add(CancellationTokenSource cancellationTokenSource)
          => cancellationTokenSourcesInternal.Add(cancellationTokenSource);

        public void Remove(CancellationTokenSource cancellationTokenSource)
          => cancellationTokenSourcesInternal.Remove(cancellationTokenSource);
    }

    internal class ReentrancyMonitor : IDisposable
    {
        public ReentrancyMonitor(object owner, Action enterAction, Action leaveAction)
        {
            owner = owner;
            enterAction = enterAction;
            leaveAction = leaveAction;
            Enter();
        }

        public void Cancel()
          => Cancel(false);

        public void Cancel(bool throwOnFirstException)
          => CancellationTokenSource.Cancel(throwOnFirstException);

        public static void CancelAll(object monitorOwner)
          => CancelAll(monitorOwner, false);

        public static bool CancelAll(object monitorOwner, bool throwOnFirstException)
        {
            bool hasCancelledActions = false;

            if (ReentrancyMonitor.CancellationTokenSourceMap.TryGetValue(monitorOwner, out ReentrancyMonitorEntry reentrancyMonitorEntry))
            {
                foreach (CancellationTokenSource cancellationTokenSource in reentrancyMonitorEntry.CancellationTokenSources)
                {
                    cancellationTokenSource.Cancel(throwOnFirstException);
                    hasCancelledActions = true;
                }
            }

            return hasCancelledActions;
        }

        private void Enter()
        {
            if (!ReentrancyMonitor.CancellationTokenSourceMap.TryGetValue(owner, out ReentrancyMonitorEntry reentrancyMonitorEntry))
            {
                reentrancyMonitorEntry = new ReentrancyMonitorEntry();
                _ = ReentrancyMonitor.CancellationTokenSourceMap.TryAdd(owner, reentrancyMonitorEntry);
            }

            CancellationTokenSource = new CancellationTokenSource();
            reentrancyMonitorEntry.Add(CancellationTokenSource);
            enterAction?.Invoke();
        }

        private void Leave()
        {
            if (ReentrancyMonitor.CancellationTokenSourceMap.TryGetValue(owner, out ReentrancyMonitorEntry reentrancyMonitorEntry))
            {
                reentrancyMonitorEntry.Remove(CancellationTokenSource);
                if (reentrancyMonitorEntry.Count == 0)
                {
                    _ = ReentrancyMonitor.CancellationTokenSourceMap.TryRemove(owner, out _);
                }
            }

            leaveAction?.Invoke();
        }

        private static ConcurrentDictionary<object, ReentrancyMonitorEntry> CancellationTokenSourceMap { get; }
          = new ConcurrentDictionary<object, ReentrancyMonitorEntry>();

        public CancellationTokenSource CancellationTokenSource { get; private set; }

        private readonly object owner;
        private readonly Action enterAction;
        private readonly Action leaveAction;
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Leave();
                    CancellationTokenSource.Dispose();
                    CancellationTokenSource = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~ReentrancyMonitor()
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
