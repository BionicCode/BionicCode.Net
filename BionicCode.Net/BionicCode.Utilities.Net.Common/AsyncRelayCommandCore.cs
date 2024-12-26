namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using static BionicCode.Utilities.Net.AsyncRelayCommandCommon;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using System.Collections.Concurrent;

  public abstract class AsyncRelayCommandCore : IAsyncRelayCommandCore
  {
    private readonly object syncLock = new object();
    private readonly ConcurrentQueue<PendingCommandInfo> executeQueue = new ConcurrentQueue<PendingCommandInfo>();
    private CancellationToken currentCancellationToken;
    private bool isCancelled;
    private bool isExecuting;
    private int pendingCount;

    /// <inheritdoc />
    public bool CanBeCanceled => this.CurrentCancellationToken.CanBeCanceled;

    /// <inheritdoc />
    public CancellationToken CurrentCancellationToken
    {
      get => this.currentCancellationToken;
      internal set
      {
        this.currentCancellationToken = value;
        OnPropertyChanged();
      }
    }

    /// <inheritdoc/>
    public bool HasPending => this.PendingCount > 0;
    /// <inheritdoc/>
    public bool IsCancelled
    {
      get => this.isCancelled;
      internal set
      {
        this.isCancelled = value;
        OnPropertyChanged();
      }
    }
    /// <inheritdoc/>
    public bool IsExecuting
    {
      get => this.isExecuting;
      internal set
      {
        this.isExecuting = value;
        OnPropertyChanged();
      }
    }

    private const int MaxDegreeOfParallelism = 1;
    /// <inheritdoc/>
    public int PendingCount
    {
      get => this.pendingCount;
      internal set
      {
        this.pendingCount = value;
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

      lock (this.syncLock)
      {
        if (this.IsExecuting)
        {
          this.executeQueue.Enqueue(pendingInfo);
          IncrementPendingCount();

          return;
        }

        this.IsExecuting = true;
      }

      await ExecuteInternalAsync(pendingInfo);
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

        this.IsExecuting = true;
        this.IsCancelled = false;

        this.CommandCancellationTokenSource = new CancellationTokenSource(pendingCommandInfo.ExecutingTimeout);
        this.MergedCommandCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            pendingCommandInfo.CancellationToken,
            this.CommandCancellationTokenSource.Token);
        this.CurrentCancellationToken = this.MergedCommandCancellationTokenSource.Token;

        this.CurrentCancellationToken.ThrowIfCancellationRequested();

        OnExecuting();
        await pendingCommandInfo.AsyncExecuteDelegate?.Invoke(this.CurrentCancellationToken);
      }
      finally
      {
        await EndExecuteCoreAsync();
      }
    }

    internal async Task EndExecuteCoreAsync()
    {
      this.CommandCancellationTokenSource?.Dispose();
      this.CommandCancellationTokenSource = null;
      this.MergedCommandCancellationTokenSource?.Dispose();
      this.MergedCommandCancellationTokenSource = null;
      OnExecuted();

      PendingCommandInfo pendingInfo;
      lock (this.syncLock)
      {
        if (!this.executeQueue.TryDequeue(out pendingInfo))
        {
          this.IsExecuting = false;
          this.IsCancelled = this.CurrentCancellationToken.IsCancellationRequested;

          return;
        }
      }

      DecrementPendingCount();
      await ExecuteInternalAsync(pendingInfo);
    }

    internal void DecrementPendingCount()
    {
      _ = Interlocked.Decrement(ref this.pendingCount);
      OnPropertyChanged(nameof(this.PendingCount));
      OnPropertyChanged(nameof(this.HasPending));
    }

    internal void IncrementPendingCount()
    {
      _ = Interlocked.Increment(ref this.pendingCount);
      OnPropertyChanged(nameof(this.PendingCount));
      OnPropertyChanged(nameof(this.HasPending));
    }

    /// <inheritdoc />
    public void Cancel()
      => Cancel(throwOnFirstException: false);

    /// <inheritdoc />
    public void Cancel(bool throwOnFirstException)
    {
      if (!this.CanBeCanceled)
      {
        return;
      }

      this.CommandCancellationTokenSource?.Cancel(throwOnFirstException);
      this.IsCancelled = true;
    }

    /// <inheritdoc />
    public bool CancelPending()
    {
      lock (this.syncLock)
      {
        bool hasCancelledPending = this.HasPending;
        while (this.executeQueue.TryDequeue(out _))
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

      if (this.CanBeCanceled && !this.IsCancelled)
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
      => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
    /// </summary>
    protected virtual void OnCanExecuteChanged(object source, EventArgs e)
      => this.CanExecuteChanged?.Invoke(source, e);

    /// <summary>
    /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Raises the <see cref="IAsyncRelayCommandCore.PendingCommandCancelled"/> event.
    /// </summary>
    protected virtual void OnPendingCommandCancelled()
      => this.PendingCommandCancelled?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="IAsyncRelayCommandCore.ExecutingCommandCancelled"/> event.
    /// </summary>
    protected virtual void OnExecutingCommandCancelled()
      => this.ExecutingCommandCancelled?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="IAsyncRelayCommandCore.Executing"/> event.
    /// </summary>
    protected virtual void OnExecuting()
      => this.Executing?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="IAsyncRelayCommandCore.Executed"/> event.
    /// </summary>
    protected virtual void OnExecuted()
      => this.Executed?.Invoke(this, EventArgs.Empty);
  }
}