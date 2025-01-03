namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Collections.Concurrent;
  using System.Threading;
  using System.Threading.Tasks;

  internal class TestEnvironmentSynchronizationContext : SynchronizationContext
  {
    public int ManagedThreadId { get; }
    private bool isShutdown;
    private readonly BlockingCollection<Action> unitOfWorkItems;
    private readonly TaskCompletionSource completionSource;
    private readonly TaskCompletionSource<bool> unitOfWorkExecutedCompletionSource;
    private readonly object syncLock;
    private bool unitOfWorkExecuted;
    private bool canExecuteUnitOfWork;

    public TestEnvironmentSynchronizationContext()
    {
      this.syncLock = new object();
      this.completionSource = new TaskCompletionSource();
      this.unitOfWorkExecutedCompletionSource = new TaskCompletionSource<bool>();
      this.unitOfWorkExecutedCompletionSource.SetResult(true);
      this.unitOfWorkItems = new BlockingCollection<Action>();

      var mainThread = new Thread(OnMessageLoopStarted);
      this.ManagedThreadId = mainThread.ManagedThreadId;
      mainThread.Start();
    }

    public async Task ShutdownAsync()
    {
      this.unitOfWorkItems.CompleteAdding();
      await this.completionSource.Task;
    }

    private void OnMessageLoopStarted(object obj)
    {
      while (!this.unitOfWorkItems.IsCompleted)
      {
        lock (this.syncLock)
        {
          if (this.canExecuteUnitOfWork && this.unitOfWorkItems.TryTake(out Action unitOfWorkItem))
          {
            unitOfWorkItem.Invoke();
            this.unitOfWorkExecuted = true;
          } 
        }
      }
      
      this.isShutdown = true;
      this.unitOfWorkItems.Dispose();
      this.completionSource.SetResult();
    }

    public override SynchronizationContext CreateCopy() => base.CreateCopy();
    public override void OperationCompleted() => base.OperationCompleted();
    public override void OperationStarted() => base.OperationStarted();
    public override void Post(SendOrPostCallback d, object state)
    {
      if (this.isShutdown)
      {
        throw new InvalidOperationException("SynchronizationContext has been shutdown.");
      }

      this.unitOfWorkItems.Add(() => d.Invoke(state));
    }

    public override void Send(SendOrPostCallback d, object state)
    {
      if (this.isShutdown)
      {
        throw new InvalidOperationException("SynchronizationContext has been shutdown.");
      }

      lock (this.syncLock)
      {
        this.canExecuteUnitOfWork = false;
        this.unitOfWorkExecuted = false;
      }

      this.unitOfWorkItems.Add(() => d.Invoke(state));
      lock (this.syncLock)
      {
        this.canExecuteUnitOfWork = true; 
      }

      while (!this.unitOfWorkExecuted) 
      {
        ;
      }
    }

    public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout) => base.Wait(waitHandles, waitAll, millisecondsTimeout);
  }
}
