namespace BionicCode.Utilities.Net.Common
{
  using System;
  using System.Collections.Concurrent;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;
  using System.Linq;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommandCommon{TParam}"/> accepts asynchronous command handlers and supports data bindng to properties like <see cref="IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommandCommon</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommandCommon{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="ExecuteAsync()"/> or its overloads instead.</remarks>
  public abstract partial class AsyncRelayCommandCommon : IAsyncRelayCommandCommon, ICommand, INotifyPropertyChanged
  {
    internal bool IsPendingCancelling { get; set; }
    /// <summary>
    /// The number of concurrent threads than can execute a command delegate.
    /// </summary>
    protected const int MaxThreads = 1;

    private bool isExecuting;
    /// <inheritdoc/>
    public bool IsExecuting
    {
      get
      {
        return this.isExecuting;
      }
      internal set
      {
        this.isExecuting = value;
        OnPropertyChanged();
      }
    }

    private bool isPendingCancelled;
    /// <inheritdoc/>
    public bool IsPendingCancelled
    {
      get
      {
        return this.isPendingCancelled;
      }
      internal set
      {
        this.isPendingCancelled = value;
        OnPropertyChanged();
      }
    }

    private bool isCancelled;
    /// <inheritdoc/>
    public bool IsCancelled
    {
      get
      {
        return this.isCancelled;
      }
      internal set
      {
        this.isCancelled = value;
        OnPropertyChanged();
      }
    }

    /// <inheritdoc/>
    public bool HasPending => this.PendingCount > 0;

    private int pendingCount;
    /// <inheritdoc/>
    public int PendingCount
    {
      get
      {
        return this.pendingCount;
      }
      internal set
      {
        this.pendingCount = value;
        OnPropertyChanged();
      }
    }

    /// <inheritdoc />
    public bool CanBeCanceled => this.CurrentCancellationToken.CanBeCanceled;

    private CancellationToken currentCancellationToken;
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
    public event PropertyChangedEventHandler PropertyChanged;

    private CancellationTokenSource CommandCancellationTokenSource { get; set; }
    private CancellationTokenSource MergedCommandCancellationTokenSource { get; set; }
    protected SemaphoreSlim SemaphoreSlim { get; }

    /// <summary>
    /// The registered parameterless async execute delegate.
    /// </summary>
    /// <value>
    /// A delegate that takes no command parameter and returns a <see cref="Task"/>.</value>
    protected Func<Task> ExecuteAsyncNoParamDelegate { get; }

    /// <summary>
    /// The registered parameterless async execute delegate that supports cancellation.
    /// </summary>
    /// <value>
    /// A delegate that supports cancellation, but takes no command parameter and returns a <see cref="Task"/>.</value>
    protected Func<CancellationToken, Task> ExecuteCancellableAsyncNoParamDelegate { get; }

    /// <summary>
    /// The registered parameterless synchronous execute delegate.
    /// </summary>
    /// <value>
    /// A delegate that takes no parameter and returns void.</value>
    protected Action ExecuteNoParamDelegate { get; }

    /// <summary>
    /// The registered parameterless synchronous execute delegate that supports cancellation.
    /// </summary>
    /// <value>
    /// A delegate that suppoprts cancellation, but takes no command parameter and returns void.</value>
    protected Action<CancellationToken> ExecuteCancellableNoParamDelegate { get; }

    /// <summary>
    /// The registered parameterless CanExecute delegate.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
    protected Func<bool> CanExecuteNoParamDelegate { get; }

    /// <inheritdoc />
#if NET
    public event EventHandler? CanExecuteChanged;
#else
    public event EventHandler CanExecuteChanged;
#endif

    #region Constructors

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action<CancellationToken> executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command.
    /// </summary>
    /// <param name="executeNoParam">The execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Action executeNoParam, Func<bool> canExecuteNoParam) : this()
    {
      this.ExecuteNoParamDelegate = executeNoParam ?? throw new ArgumentNullException(nameof(executeNoParam));
      this.CanExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : this()
    {
      this.ExecuteAsyncNoParamDelegate = executeAsyncNoParam ?? throw new ArgumentNullException(nameof(executeAsyncNoParam));
      this.CanExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute) : this()
    {
      this.ExecuteCancellableAsyncNoParamDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteNoParamDelegate = canExecute ?? (() => true);
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that supports cancellation.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Action<CancellationToken> executeAsync, Func<bool> canExecute) : this()
    {
      this.ExecuteCancellableNoParamDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteNoParamDelegate = canExecute ?? (() => true);
    }

    protected AsyncRelayCommandCommon()
    {
      this.SemaphoreSlim = new SemaphoreSlim(MaxThreads, MaxThreads);
    }

    #endregion Constructors

    /// <summary>
    ///   Determines whether this AsyncRelayCommandCommon can execute.
    /// </summary>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute() => this.CanExecuteNoParamDelegate?.Invoke() ?? true;

    /// <inheritdoc />
    public async Task ExecuteAsync() => await ExecuteAsync(Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken) => await ExecuteAsync(Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TimeSpan timeout) => await ExecuteAsync(timeout, CancellationToken.None);

    /// <inheritdoc />
    public virtual async Task ExecuteAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
      if (this.IsPendingCancelling)
      {
        return;
      }

      if (this.IsPendingCancelled)
      {
        this.IsPendingCancelled = false;
      }

      using (var reentrancyMonitor = new ReentrancyMonitor(IncrementPendingCount, DecrementPendingCount))
      {
        await this.SemaphoreSlim.WaitAsync(reentrancyMonitor.CancellationTokenSource.Token);
        
        // In case we left the semaphore before it could throw the OperationCanceledException (race-comdition).
        if (this.IsPendingCancelling || this.IsPendingCancelled)
        {
          throw new OperationCanceledException(reentrancyMonitor.CancellationTokenSource.Token);
        }
      }

      this.IsExecuting = true;

      try
      {
        using (this.CommandCancellationTokenSource = new CancellationTokenSource(timeout))
        {
          using (this.MergedCommandCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            this.CommandCancellationTokenSource.Token))
          {
            this.CurrentCancellationToken = this.MergedCommandCancellationTokenSource.Token;

            this.CurrentCancellationToken.ThrowIfCancellationRequested();

            _ = await TryExecuteNoParamCommand();
          }
        }
      }
      finally
      {
        this.CommandCancellationTokenSource = null;
        this.MergedCommandCancellationTokenSource = null;
        this.IsExecuting = false;
        this.IsCancelled = this.CurrentCancellationToken.IsCancellationRequested;
        this.SemaphoreSlim.Release();
      }
    }

    protected async Task<bool> TryExecuteNoParamCommand()
    {
      bool isSuccessfull = false;
      if (this.ExecuteAsyncNoParamDelegate != null)
      {
        this.CurrentCancellationToken.ThrowIfCancellationRequested();
        await this.ExecuteAsyncNoParamDelegate.Invoke().ConfigureAwait(false);
        isSuccessfull = true;
      }
      else if (this.ExecuteNoParamDelegate != null)
      {
        this.CurrentCancellationToken.ThrowIfCancellationRequested();
        this.ExecuteNoParamDelegate.Invoke();
        isSuccessfull = true;
      }
      else if (this.ExecuteCancellableAsyncNoParamDelegate != null)
      {
        this.CurrentCancellationToken.ThrowIfCancellationRequested();
        await this.ExecuteCancellableAsyncNoParamDelegate.Invoke(this.CurrentCancellationToken);
        isSuccessfull = true;
      }
      else if (this.ExecuteCancellableNoParamDelegate != null)
      {
        this.CurrentCancellationToken.ThrowIfCancellationRequested();
        this.ExecuteCancellableNoParamDelegate.Invoke(this.CurrentCancellationToken);
        isSuccessfull = true;
      }

      return isSuccessfull;
    }

    internal void DecrementPendingCount()
    {
      Interlocked.Decrement(ref this.pendingCount);
      OnPropertyChanged(nameof(this.PendingCount));
      OnPropertyChanged(nameof(this.HasPending));
    }

    internal void IncrementPendingCount()
    {
      Interlocked.Increment(ref this.pendingCount);
      OnPropertyChanged(nameof(this.PendingCount));
      OnPropertyChanged(nameof(this.HasPending));
    }

    /// <inheritdoc />
    public void CancelExecuting()
      => this.CommandCancellationTokenSource?.Cancel();

    /// <inheritdoc />
    public void CancelExecuting(bool throwOnFirstException)
      => this.CommandCancellationTokenSource?.Cancel(throwOnFirstException);

    /// <inheritdoc />
    public void CancelPending()
    {
      this.IsPendingCancelling = true;
      while (ReentrancyMonitor.CancellationTokenSourceMap.Any())
      {
        System.Collections.Generic.KeyValuePair<ReentrancyMonitor, CancellationTokenSource> entry = ReentrancyMonitor.CancellationTokenSourceMap.FirstOrDefault();
        if (entry.Key == null)
        {
          continue;
        }

        if (ReentrancyMonitor.CancellationTokenSourceMap.TryRemove(entry.Key, out CancellationTokenSource cancellationTokenSource))
        {
          cancellationTokenSource?.Cancel();
        }
      }

      this.IsPendingCancelled = true;
      this.IsPendingCancelling = false;
    }

    /// <inheritdoc />
    public void CancelPending(bool throwOnFirstException)
    {
      this.IsPendingCancelling = true;
      while (ReentrancyMonitor.CancellationTokenSourceMap.Any())
      {
        System.Collections.Generic.KeyValuePair<ReentrancyMonitor, CancellationTokenSource> entry = ReentrancyMonitor.CancellationTokenSourceMap.FirstOrDefault();
        if (ReentrancyMonitor.CancellationTokenSourceMap.TryRemove(entry.Key, out CancellationTokenSource cancellationTokenSource))
        {
          cancellationTokenSource?.Cancel();
        }
      }

      this.IsPendingCancelled = true;
      this.IsPendingCancelling = false;
    }

    /// <inheritdoc />
    public void CancelAll()
    {
      CancelPending();
      CancelExecuting();
    }

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
    /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
      => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    #region ICommand implementation
#if NET
    bool ICommand.CanExecute(object? parameter)
#else
    bool ICommand.CanExecute(object parameter)
#endif
    {
      return CanExecute();
    }

    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync();

    #endregion ICommand implementation

  }
}