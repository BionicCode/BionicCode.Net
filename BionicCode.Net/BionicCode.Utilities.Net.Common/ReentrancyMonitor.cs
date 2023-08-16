namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Concurrent;
  using System.Threading;

  public abstract partial class AsyncRelayCommandCommon
  {
    internal class ReentrancyMonitor : IDisposable
    {
      public static ConcurrentDictionary<ReentrancyMonitor, CancellationTokenSource> CancellationTokenSourceMap { get; } = new ConcurrentDictionary<ReentrancyMonitor, CancellationTokenSource>();

      public ReentrancyMonitor(Action enterAction, Action leaveAction)
      {
        this.EnterAction = enterAction;
        this.LeaveAction = leaveAction;
        this.CancellationTokenSource = new CancellationTokenSource();
        Enter();
      }

      private void Enter()
      {
        _ = ReentrancyMonitor.CancellationTokenSourceMap.TryAdd(this, this.CancellationTokenSource);
        this.EnterAction?.Invoke();
      }

      private void Leave()
      {
        _ = ReentrancyMonitor.CancellationTokenSourceMap.TryRemove(this, out _);
        this.LeaveAction?.Invoke();
      }

      public CancellationTokenSource CancellationTokenSource { get; }
      private Action EnterAction { get; }
      private Action LeaveAction { get; }

      private bool disposedValue;
      protected virtual void Dispose(bool disposing)
      {
        if (!this.disposedValue)
        {
          if (disposing)
          {
            Leave();
            this.CancellationTokenSource.Dispose();
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