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
            syncLock = new object();
            completionSource = new TaskCompletionSource();
            unitOfWorkExecutedCompletionSource = new TaskCompletionSource<bool>();
            unitOfWorkExecutedCompletionSource.SetResult(true);
            unitOfWorkItems = new BlockingCollection<Action>();

            var mainThread = new Thread(OnMessageLoopStarted);
            ManagedThreadId = mainThread.ManagedThreadId;
            mainThread.Start();
        }

        public async Task ShutdownAsync()
        {
            unitOfWorkItems.CompleteAdding();
            await completionSource.Task;
        }

        private void OnMessageLoopStarted(object obj)
        {
            while (!unitOfWorkItems.IsCompleted)
            {
                lock (syncLock)
                {
                    if (canExecuteUnitOfWork && unitOfWorkItems.TryTake(out Action unitOfWorkItem))
                    {
                        unitOfWorkItem.Invoke();
                        unitOfWorkExecuted = true;
                    }
                }
            }

            isShutdown = true;
            unitOfWorkItems.Dispose();
            completionSource.SetResult();
        }

        public override SynchronizationContext CreateCopy() => base.CreateCopy();
        public override void OperationCompleted() => base.OperationCompleted();
        public override void OperationStarted() => base.OperationStarted();
        public override void Post(SendOrPostCallback d, object state)
        {
            if (isShutdown)
            {
                throw new InvalidOperationException("SynchronizationContext has been shutdown.");
            }

            unitOfWorkItems.Add(() => d.Invoke(state));
        }

        public override void Send(SendOrPostCallback d, object state)
        {
            if (isShutdown)
            {
                throw new InvalidOperationException("SynchronizationContext has been shutdown.");
            }

            lock (syncLock)
            {
                canExecuteUnitOfWork = false;
                unitOfWorkExecuted = false;
            }

            unitOfWorkItems.Add(() => d.Invoke(state));
            lock (syncLock)
            {
                canExecuteUnitOfWork = true;
            }

            while (!unitOfWorkExecuted)
            {
                ;
            }
        }

        public override int Wait(IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout) => base.Wait(waitHandles, waitAll, millisecondsTimeout);
    }
}
