namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using static BionicCode.Utilities.Net.AsyncRelayCommandCommon;
  using System.Threading.Tasks;
  using System.Windows.Input;

  public abstract class AsyncRelayCommandCore : IAsyncRelayCommandCore, IDisposable
  {
    private SemaphoreSlim ExecuteCommandSemaphore => this.executeCommandSemaphoreFactory.Value;
    private readonly Lazy<SemaphoreSlim> executeCommandSemaphoreFactory;
    private CancellationToken currentCancellationToken;
    private bool isCancelled;
    private bool isExecuting;
    private int pendingCount;
    private bool disposedValue;

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
    /// <inheritdoc/>
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
    public event EventHandler PendingCommandCancelled;
    /// <inheritdoc />
    public event PropertyChangedEventHandler PropertyChanged;

    /// <inheritdoc />
#if NET
    public event EventHandler? CanExecuteChanged;
#else
    public event EventHandler CanExecuteChanged;
#endif

    protected AsyncRelayCommandCore() => this.executeCommandSemaphoreFactory = new Lazy<SemaphoreSlim>(() 
      => new SemaphoreSlim(AsyncRelayCommandCore.MaxDegreeOfParallelism, AsyncRelayCommandCore.MaxDegreeOfParallelism), isThreadSafe: true);

    protected async Task BeginExecuteAsyncCoreAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
      // Monitor pending (waiting) commad executions and make them cancellable.
      // We monotor per reentrant call and not per instance.
      using (var reentrancyMonitor = new ReentrancyMonitor(this, IncrementPendingCount, DecrementPendingCount))
      {
        try
        {
          _ = await this.ExecuteCommandSemaphore.WaitAsync(timeout, reentrancyMonitor.CancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
          OnPendingCommandsCancelled();
        }
      }

      this.IsExecuting = true;

      this.CommandCancellationTokenSource = new CancellationTokenSource(timeout);
      this.MergedCommandCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
          cancellationToken,
          this.CommandCancellationTokenSource.Token);
      this.CurrentCancellationToken = this.MergedCommandCancellationTokenSource.Token;
      this.CurrentCancellationToken.ThrowIfCancellationRequested();
    }

    protected void EndExecuteAyncCore()
    {
      this.CommandCancellationTokenSource.Dispose();
      this.CommandCancellationTokenSource = null;
      this.MergedCommandCancellationTokenSource.Dispose();
      this.MergedCommandCancellationTokenSource = null;
      this.IsExecuting = false;
      this.IsCancelled = this.CurrentCancellationToken.IsCancellationRequested;
      _ = this.ExecuteCommandSemaphore.Release();
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
    public void CancelExecuting()
      => CancelExecuting(throwOnFirstException: false);

    /// <inheritdoc />
    public void CancelExecuting(bool throwOnFirstException)
    {
      this.CommandCancellationTokenSource?.Cancel(throwOnFirstException);
      this.IsCancelled = true;
    }

    /// <inheritdoc />
    public void CancelPending()
      => CancelPending(throwOnFirstException: false);

    /// <inheritdoc />
    public void CancelPending(bool throwOnFirstException)
      => ReentrancyMonitor.CancelAll(this, throwOnFirstException);

    /// <inheritdoc />
    public void CancelAll()
      => CancelAll(throwOnFirstException: false);

    /// <inheritdoc />
    public void CancelAll(bool throwOnFirstException)
    {
      CancelPending(throwOnFirstException);
      CancelExecuting(throwOnFirstException);
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
    /// Raises the <see cref="IAsyncRelayCommandCommon.PendingCommandCancelled"/> event.
    /// </summary>
    protected virtual void OnPendingCommandsCancelled()
      => this.PendingCommandCancelled?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="IAsyncRelayCommandCommon.ExecutingCommandCancelled"/> event.
    /// </summary>
    protected virtual void OnExecutingCommandCancelled()
      => this.ExecutingCommandCancelled?.Invoke(this, EventArgs.Empty);

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          this.ExecuteCommandSemaphore?.Dispose();
          this.CommandCancellationTokenSource?.Dispose();
          this.MergedCommandCancellationTokenSource?.Dispose();
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~AsyncRelayCommandCore()
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