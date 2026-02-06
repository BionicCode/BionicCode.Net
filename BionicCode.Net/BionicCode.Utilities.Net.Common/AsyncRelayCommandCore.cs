namespace BionicCode.Utilities.Net
{
    using System;
    using System.Collections.Concurrent;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public abstract class AsyncRelayCommandCore : IAsyncRelayCommandCore
    {
        private readonly object syncLock = new object();
        private readonly ConcurrentQueue<PendingCommandInfo> executeQueue = new ConcurrentQueue<PendingCommandInfo>();
        private CancellationToken currentCancellationToken;
        private bool isCancelled;
        private bool isExecuting;
        private int pendingCount;

        /// <inheritdoc />
        public bool CanBeCanceled => CurrentCancellationToken.CanBeCanceled;

        /// <inheritdoc />
        public CancellationToken CurrentCancellationToken
        {
            get => currentCancellationToken;
            internal set
            {
                currentCancellationToken = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public bool HasPending => PendingCount > 0;
        /// <inheritdoc/>
        public bool IsCancelled
        {
            get => isCancelled;
            internal set
            {
                isCancelled = value;
                OnPropertyChanged();
            }
        }
        /// <inheritdoc/>
        public bool IsExecuting
        {
            get => isExecuting;
            internal set
            {
                isExecuting = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public int PendingCount
        {
            get => pendingCount;
            internal set
            {
                pendingCount = value;
                OnPropertyChanged();
            }
        }

        protected CancellationTokenSource CommandCancellationTokenSource { get; private set; }
        protected CancellationTokenSource MergedCommandCancellationTokenSource { get; private set; }

        /// <inheritdoc />
        public event EventHandler ExecutingCommandCancelled;
        /// <inheritdoc />
        public event EventHandler Executing;
        /// <inheritdoc />
        public event EventHandler Executed;
        /// <inheritdoc />
        public event EventHandler PendingCommandCancelled;
        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc />
#if NET
        public event EventHandler? CanExecuteChanged;
#else
    public event EventHandler CanExecuteChanged;
#endif

        protected async Task ExecuteCoreAsync(Func<CancellationToken, Task> asyncExecuteDelegate, TimeSpan pendingTimeout, TimeSpan executingTimeout, CancellationToken cancellationToken)
        {
            var pendingInfo = new PendingCommandInfo(pendingTimeout, DateTime.Now, asyncExecuteDelegate, executingTimeout, cancellationToken);

            lock (syncLock)
            {
                if (IsExecuting)
                {
                    executeQueue.Enqueue(pendingInfo);
                    IncrementPendingCount();

                    return;
                }

                IsExecuting = true;
            }

            await ExecuteInternalAsync(pendingInfo).ConfigureAwait(false);
        }

        private async Task ExecuteInternalAsync(PendingCommandInfo pendingCommandInfo)
        {
            try
            {
                DateTime timestamp = DateTime.Now;
                if (pendingCommandInfo.CancellationToken.IsCancellationRequested)
                {
                    OnPendingCommandCancelled();
                    return;
                }

                TimeSpan elapsedPendingTime = timestamp.Subtract(pendingCommandInfo.Timestamp);
                if (pendingCommandInfo.PendingTimeout > Timeout.InfiniteTimeSpan && elapsedPendingTime > pendingCommandInfo.PendingTimeout)
                {
                    OnPendingCommandCancelled();
                    return;
                }

                IsExecuting = true;
                IsCancelled = false;

                CommandCancellationTokenSource = new CancellationTokenSource(pendingCommandInfo.ExecutingTimeout);
                MergedCommandCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                    pendingCommandInfo.CancellationToken,
                    CommandCancellationTokenSource.Token);
                CurrentCancellationToken = MergedCommandCancellationTokenSource.Token;

                CurrentCancellationToken.ThrowIfCancellationRequested();

                OnExecuting();
                await (pendingCommandInfo.AsyncExecuteDelegate?.Invoke(CurrentCancellationToken)).ConfigureAwait(false);
            }
            finally
            {
                await EndExecuteCoreAsync().ConfigureAwait(false);
            }
        }

        internal async Task EndExecuteCoreAsync()
        {
            CommandCancellationTokenSource?.Dispose();
            CommandCancellationTokenSource = null;
            MergedCommandCancellationTokenSource?.Dispose();
            MergedCommandCancellationTokenSource = null;
            OnExecuted();

            PendingCommandInfo pendingInfo;
            lock (syncLock)
            {
                if (!executeQueue.TryDequeue(out pendingInfo))
                {
                    IsExecuting = false;
                    IsCancelled = CurrentCancellationToken.IsCancellationRequested;

                    return;
                }
            }

            DecrementPendingCount();
            await ExecuteInternalAsync(pendingInfo).ConfigureAwait(false);
        }

        internal void DecrementPendingCount()
        {
            _ = Interlocked.Decrement(ref pendingCount);
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(HasPending));
        }

        internal void IncrementPendingCount()
        {
            _ = Interlocked.Increment(ref pendingCount);
            OnPropertyChanged(nameof(PendingCount));
            OnPropertyChanged(nameof(HasPending));
        }

        /// <inheritdoc />
        public void Cancel()
          => Cancel(throwOnFirstException: false);

        /// <inheritdoc />
        public void Cancel(bool throwOnFirstException)
        {
            if (!CanBeCanceled)
            {
                return;
            }

            CommandCancellationTokenSource?.Cancel(throwOnFirstException);
            IsCancelled = true;
        }

        /// <inheritdoc />
        public bool CancelPending()
        {
            lock (syncLock)
            {
                bool hasCancelledPending = HasPending;
                while (executeQueue.TryDequeue(out _))
                {
                    DecrementPendingCount();
                    OnPendingCommandCancelled();
                }

                return hasCancelledPending;
            }
        }

        /// <inheritdoc />
        public bool CancelAll()
          => CancelAll(throwOnFirstException: false);

        /// <inheritdoc />
        public bool CancelAll(bool throwOnFirstException)
        {
            bool hasCancelledActions = CancelPending();

            if (CanBeCanceled && !IsCancelled)
            {
                hasCancelledActions = true;
                Cancel(throwOnFirstException);
            }

            return hasCancelledActions;
        }

        /// <inheritdoc/>
        public void InvalidateCommand()
          => OnCanExecuteChanged();

        /// <summary>
        /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
          => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
        /// </summary>
        protected virtual void OnCanExecuteChanged(object source, EventArgs e)
          => CanExecuteChanged?.Invoke(source, e);

        /// <summary>
        /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Raises the <see cref="IAsyncRelayCommandCore.PendingCommandCancelled"/> event.
        /// </summary>
        protected virtual void OnPendingCommandCancelled()
          => PendingCommandCancelled?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="IAsyncRelayCommandCore.ExecutingCommandCancelled"/> event.
        /// </summary>
        protected virtual void OnExecutingCommandCancelled()
          => ExecutingCommandCancelled?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="IAsyncRelayCommandCore.Executing"/> event.
        /// </summary>
        protected virtual void OnExecuting()
          => Executing?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Raises the <see cref="IAsyncRelayCommandCore.Executed"/> event.
        /// </summary>
        protected virtual void OnExecuted()
          => Executed?.Invoke(this, EventArgs.Empty);
    }
}
