namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections;
  using System.Collections.Concurrent;
  using System.Collections.Generic;
  using System.Collections.ObjectModel;
  using System.Threading;

  public abstract partial class AsyncRelayCommandCommon
  {
    internal class ReentrancyMonitorEntry
    {
      public int Count => this.cancellationTokenSourcesInternal.Count;
      public ReadOnlyCollection<CancellationTokenSource> CancellationTokenSources { get; }
      private readonly List<CancellationTokenSource> cancellationTokenSourcesInternal;

      public ReentrancyMonitorEntry()
      {
        this.cancellationTokenSourcesInternal = new List<CancellationTokenSource>();
        this.CancellationTokenSources = new ReadOnlyCollection<CancellationTokenSource>(this.cancellationTokenSourcesInternal);
      }

      public void Add(CancellationTokenSource cancellationTokenSource)
        => this.cancellationTokenSourcesInternal.Add(cancellationTokenSource);

      public void Remove(CancellationTokenSource cancellationTokenSource)
        => this.cancellationTokenSourcesInternal.Remove(cancellationTokenSource);
    }

    internal class ReentrancyMonitor : IDisposable
    {
      public ReentrancyMonitor(object owner, Action enterAction, Action leaveAction)
      {
        this.owner = owner;
        this.enterAction = enterAction;
        this.leaveAction = leaveAction;
        Enter();
      }

      public void Cancel()
        => Cancel(false);

      public void Cancel(bool throwOnFirstException)
        => this.CancellationTokenSource.Cancel(throwOnFirstException);

      public static void CancelAll(object monitorOwner)
        => CancelAll(monitorOwner, false);

      public static void CancelAll(object monitorOwner, bool throwOnFirstException)
      {
        if (ReentrancyMonitor.CancellationTokenSourceMap.TryGetValue(monitorOwner, out ReentrancyMonitorEntry reentrancyMonitorEntry))
        {
          foreach (CancellationTokenSource cancellationTokenSource in reentrancyMonitorEntry.CancellationTokenSources)
          {
            cancellationTokenSource.Cancel(throwOnFirstException);
          }
        }
      }

      private void Enter()
      {
        if (!ReentrancyMonitor.CancellationTokenSourceMap.TryGetValue(this.owner, out ReentrancyMonitorEntry reentrancyMonitorEntry))
        {
          reentrancyMonitorEntry = new ReentrancyMonitorEntry();
          _ = ReentrancyMonitor.CancellationTokenSourceMap.TryAdd(this.owner, reentrancyMonitorEntry);
        }

        this.CancellationTokenSource = new CancellationTokenSource();
        reentrancyMonitorEntry.Add(this.CancellationTokenSource);
        this.enterAction?.Invoke();
      }

      private void Leave()
      {
        if (ReentrancyMonitor.CancellationTokenSourceMap.TryGetValue(this.owner, out ReentrancyMonitorEntry reentrancyMonitorEntry))
        {
          reentrancyMonitorEntry.Remove(this.CancellationTokenSource);
          if (reentrancyMonitorEntry.Count == 0)
          {
            _ = ReentrancyMonitor.CancellationTokenSourceMap.TryRemove(this.owner, out _);
          }
        }

        this.leaveAction?.Invoke();
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
        if (!this.disposedValue)
        {
          if (disposing)
          {
            Leave();
            this.CancellationTokenSource.Dispose();
            this.CancellationTokenSource = null;
          }

          // TODO: free unmanaged resources (unmanaged objects) and override finalizer
          // TODO: set large fields to null
          this.disposedValue = true;
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
}