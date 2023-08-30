namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommandCommon{TParam}"/> accepts asynchronous command handlers and supports data bindng to properties like <see cref="AsyncRelayCommandCommon.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="ExecuteAsync(TParam)"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommandCommon</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommandCommon{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="AsyncRelayCommandCommon.ExecuteAsync()"/> or its overloads instead.</remarks>
  public abstract class AsyncRelayCommandCommon<TParam> : AsyncRelayCommandCommon, IAsyncRelayCommandCommon, ICommand, IAsyncRelayCommandCommon<TParam>, INotifyPropertyChanged
  {
    private CancellationTokenSource CommandCancellationTokenSource { get; set; }
    private CancellationTokenSource MergedCommandCancellationTokenSource { get; set; }

    /// <summary>
    /// The registered async execute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
    protected Func<TParam, Task> ExecuteAsyncDelegate { get; }

    /// <summary>
    /// The registered async execute delegate that supports cancellation and accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that supports cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
    protected Func<TParam, CancellationToken, Task> ExecuteCancellableAsyncDelegate { get; }

    /// <summary>
    /// The registered execute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that takes no parameter and returns a <see cref="Task"/>.</value>
    protected Action<TParam> ExecuteDelegate { get; }

    /// <summary>
    /// The registered execute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that suppoprts cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
    protected Action<TParam, CancellationToken> ExecuteCancellableDelegate { get; }

    /// <summary>
    /// The registered CanExecute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
    protected Func<TParam, bool> CanExecuteDelegate { get; }

    #region Constructors

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/> and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>) 
    ///   <br/>and accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam, CancellationToken> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action executeNoParam)
      : base(executeNoParam)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Action<CancellationToken> executeNoParam)
      : base(executeNoParam)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>) 
    ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, CancellationToken, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>) 
    ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam)
      : base(executeAsyncNoParam)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsyncNoParam)
      : base(executeAsyncNoParam)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command.
    /// </summary>
    /// <param name="executeNoParam">The execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Action executeNoParam, Func<bool> canExecuteNoParam)
      : base(executeNoParam, canExecuteNoParam)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execution handler.</param>
    /// <param name="canExecute">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam> execute, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
      this.CanExecuteDelegate = param => canExecute?.Invoke(param) ?? true;
    }

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : base(executeAsyncNoParam, canExecuteNoParam)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, Task> executeAsync, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteAsyncDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteDelegate = canExecute?.ToFunc();
    }

    /// <summary>
    ///   Creates a new asynchronous command that supports cancellation and accepts a command parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<TParam, CancellationToken, Task> executeAsync, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteCancellableAsyncDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteDelegate = canExecute?.ToFunc();
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute) : base(executeAsync, canExecute)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that supports cancellation.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Action<CancellationToken> executeAsync, Func<bool> canExecute) : base(executeAsync, canExecute)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that supports cancellation adn accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected AsyncRelayCommandCommon(Action<TParam, CancellationToken> executeAsync, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteCancellableDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteDelegate = canExecute?.ToFunc();
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    protected AsyncRelayCommandCommon()
    {
    }

    #endregion Constructors

    /// <summary>
    ///   Determines whether this AsyncRelayCommandCommon can execute.
    /// </summary>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public new bool CanExecute() => CanExecute(default);

    /// <summary>
    ///   Determines whether this AsyncRelayCommandCommon can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute(TParam parameter) => this.CanExecuteDelegate?.Invoke(parameter)
                                                ?? this.CanExecuteNoParamDelegate?.Invoke()
                                                ?? true;

    /// <inheritdoc />
    public override async Task ExecuteAsync(TimeSpan timeout, CancellationToken cancellationToken) => await ExecuteAsync(default, timeout, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, TimeSpan timeout) => await ExecuteAsync(parameter, timeout, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, TimeSpan timeout, CancellationToken cancellationToken)
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

            if (await TryExecuteNoParamCommand())
            {
              return;
            }
            else if (this.ExecuteAsyncDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              await this.ExecuteAsyncDelegate.Invoke(parameter);
            }
            else if (this.ExecuteCancellableAsyncDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              await this.ExecuteCancellableAsyncDelegate.Invoke(parameter, this.CurrentCancellationToken);
            }
            else if (this.ExecuteCancellableDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              this.ExecuteCancellableDelegate.Invoke(parameter, this.CurrentCancellationToken);
            }
            else
            {
              this.ExecuteDelegate.Invoke(parameter);
              await Task.CompletedTask;
            }
          }
        }
      }
      finally
      {
        this.CommandCancellationTokenSource = null;
        this.MergedCommandCancellationTokenSource = null;
        this.IsExecuting = false;
        this.IsCancelled = this.CurrentCancellationToken.IsCancellationRequested;
        _ = this.SemaphoreSlim.Release();
      }
    }
    #region ICommand implementation
#if NET
    bool ICommand.CanExecute(object? parameter) => parameter == null
        ? CanExecute(default)
        : CanExecute((TParam)parameter);
#else
    bool ICommand.CanExecute(object parameter) => parameter == null
        ? CanExecute(default)
        : CanExecute((TParam)parameter);
#endif
    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);

    #endregion ICommand implementation
  }
}