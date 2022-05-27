namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommand{TParam}"/> accepts asynchronous command handlers and supports data bindng to properties like <see cref="IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// <br/>Call and await the <see cref="ExecuteAsync"/> method or one of its overloads to execute the command explicitly asynchronously.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommand{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="ExecuteAsync()"/> or its overloads instead.</remarks>
  public class AsyncRelayCommand<TParam> : IAsyncRelayCommand, ICommand, IAsyncRelayCommand<TParam>, INotifyPropertyChanged
  {
    private const int MaxThreads = 1;

    private bool isExecuting;
    /// <inheritdoc/>
    public bool IsExecuting
    {
      get
      {
        return this.isExecuting;
      }
      set
      {
        this.isExecuting = value;
        OnPropertyChnaged();
      }
    }

    /// <inheritdoc />
    public bool CanBeCanceled => this.CurrentCancellationToken.CanBeCanceled;

    private CancellationToken currentCancellationToken;
    /// <inheritdoc />
    public CancellationToken CurrentCancellationToken
    {
      get => this.currentCancellationToken;
      private set
      {
        this.currentCancellationToken = value;
        OnPropertyChnaged();
      }
    }

    public event PropertyChangedEventHandler PropertyChanged;

#if !NETSTANDARD
    private bool isCommandManagerRequerySuggestedEnabled;

    /// <inheritdoc />
    public bool IsCommandManagerRequerySuggestedEnabled
    {
      get => this.isCommandManagerRequerySuggestedEnabled;
      set
      {
        if (value == this.IsCommandManagerRequerySuggestedEnabled)
        {
          return;
        }

        this.isCommandManagerRequerySuggestedEnabled = value;
        if (this.IsCommandManagerRequerySuggestedEnabled)
        {
          foreach (Delegate canExecuteChangedDelegate in this.canExecuteChangedDelegate.GetInvocationList())
          {
            CommandManager.RequerySuggested += (EventHandler)canExecuteChangedDelegate;
          }
        }
        else
        {
          foreach (Delegate canExecuteChangedDelegate in this.canExecuteChangedDelegate.GetInvocationList())
          {
            CommandManager.RequerySuggested -= (EventHandler)canExecuteChangedDelegate;
          }
        }
      }
    }
#endif
    private CancellationTokenSource CommandCancellationTokenSource { get; set; }
    private CancellationTokenSource SynchronizationCancellationTokenSource { get; set; }
    private CancellationTokenSource MergedCommandCancellationTokenSource { get; set; }
    private SemaphoreSlim SemaphoreSlim { get; }

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

    private EventHandler canExecuteChangedDelegate;

    /// <inheritdoc />
#if NET
    public event EventHandler? CanExecuteChanged
#else
public event EventHandler CanExecuteChanged
#endif
    {
      add
      {
#if !NETSTANDARD
        if (this.IsCommandManagerRequerySuggestedEnabled)
        {
          CommandManager.RequerySuggested += value;
        }
#endif
        this.canExecuteChangedDelegate += value;
      }
      remove
      {
#if !NETSTANDARD
        CommandManager.RequerySuggested -= value;
#endif
        this.canExecuteChangedDelegate -= value;
      }
    }

    #region Constructors

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/> and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<TParam> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>) 
    ///   <br/>and accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<TParam, CancellationToken> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<CancellationToken> executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>) 
    ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<TParam, CancellationToken, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>) 
    ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<TParam, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>)
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command.
    /// </summary>
    /// <param name="executeNoParam">The execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : this()
    {
      this.ExecuteNoParamDelegate = executeNoParam ?? throw new ArgumentNullException(nameof(executeNoParam));
      this.CanExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
    }

    ///// <summary>
    /////   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    ///// </summary>
    ///// <param name="execute">The execution handler.</param>
    ///// <param name="canExecute">The execution status handler.</param>
    //public AsyncRelayCommand(Action<TParam> execute, Func<TParam, bool> canExecute) : this()
    //{
    //  this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
    //  this.CanExecuteDelegate = canExecute ?? (param => true);
    //}

    /// <summary>
    ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execution handler.</param>
    /// <param name="canExecute">The execution status handler.</param>
    public AsyncRelayCommand(Action<TParam> execute, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteDelegate = execute ?? throw new ArgumentNullException(nameof(execute));
      this.CanExecuteDelegate = param => canExecute?.Invoke(param) ?? true;
    }

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : this()
    {
      this.ExecuteAsyncNoParamDelegate = executeAsyncNoParam ?? throw new ArgumentNullException(nameof(executeAsyncNoParam));
      this.CanExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a new asynchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Func<TParam, Task> executeAsync, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteAsyncDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteDelegate = canExecute?.ToFunc();
    }

    ///// <summary>
    /////   Creates a new asynchronous command tha accepts a command parameter of type <typeparamref name="TParam"/>.
    ///// </summary>
    ///// <param name="executeAsync">The awaitable execution handler.</param>
    ///// <param name="canExecute">The can execute handler.</param>
    //public AsyncRelayCommand(Func<TParam, Task> executeAsync, Func<TParam, bool> canExecute) : this()
    //{
    //  this.ExecuteAsyncDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
    //  this.CanExecuteDelegate = canExecute ?? (param => true);
    //}

    /// <summary>
    ///   Creates a new asynchronous command that supports cancellation and accepts a command parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Func<TParam, CancellationToken, Task> executeAsync, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteCancellableAsyncDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteDelegate = canExecute?.ToFunc();
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute) : this()
    {
      this.ExecuteCancellableAsyncNoParamDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteNoParamDelegate = canExecute ?? (() => true);
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that supports cancellation.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Action<CancellationToken> executeAsync, Func<bool> canExecute) : this()
    {
      this.ExecuteCancellableNoParamDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteNoParamDelegate = canExecute ?? (() => true);
    }

    /// <summary>
    ///   Creates a new synchronous command that supports cancellation adn accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Action<TParam, CancellationToken> executeAsync, Predicate<TParam> canExecute) : this()
    {
      this.ExecuteCancellableDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.CanExecuteDelegate = canExecute?.ToFunc();
    }

    private AsyncRelayCommand()
    {
      this.SemaphoreSlim = new SemaphoreSlim(MaxThreads, MaxThreads);
    }

    #endregion Constructors

    /// <summary>
    ///   Determines whether this AsyncRelayCommand can execute.
    /// </summary>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute() => CanExecute(default);

    /// <summary>
    ///   Determines whether this AsyncRelayCommand can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute(TParam parameter) => this.CanExecuteDelegate?.Invoke(parameter)
                                                ?? this.CanExecuteNoParamDelegate?.Invoke()
                                                ?? true;

    /// <inheritdoc />
    public async Task ExecuteAsync() => await ExecuteAsync(default, Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(CancellationToken cancellationToken) => await ExecuteAsync(default, Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TimeSpan timeout) => await ExecuteAsync(default, timeout, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, TimeSpan timeout) => await ExecuteAsync(parameter, timeout, CancellationToken.None);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public async Task ExecuteAsync(TParam parameter, TimeSpan timeout, CancellationToken cancellationToken)
    {
      try
      {
        using (this.SynchronizationCancellationTokenSource = new CancellationTokenSource())
        {
          await this.SemaphoreSlim.WaitAsync(this.SynchronizationCancellationTokenSource.Token);
        }
      }
      catch (OperationCanceledException)
      { }
      finally
      {
        this.SynchronizationCancellationTokenSource = null;
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

            if (this.ExecuteAsyncDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              await this.ExecuteAsyncDelegate.Invoke(parameter).ConfigureAwait(false);
            }
            else if (this.ExecuteAsyncNoParamDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              await this.ExecuteAsyncNoParamDelegate.Invoke().ConfigureAwait(false);
            }
            else if (this.ExecuteNoParamDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              this.ExecuteNoParamDelegate.Invoke();
            }
            else if (this.ExecuteCancellableAsyncNoParamDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              await this.ExecuteCancellableAsyncNoParamDelegate.Invoke(this.CurrentCancellationToken);
            }
            else if (this.ExecuteCancellableNoParamDelegate != null)
            {
              this.CurrentCancellationToken.ThrowIfCancellationRequested();
              this.ExecuteCancellableNoParamDelegate.Invoke(this.CurrentCancellationToken);
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
            }
          }
        }
      }
      finally
      {
        this.CommandCancellationTokenSource = null;
        this.MergedCommandCancellationTokenSource = null;
        this.IsExecuting = false;
        this.SemaphoreSlim.Release();
      }
    }

    /// <inheritdoc />
    public void CancelExecuting()
      => this.CommandCancellationTokenSource?.Cancel();

    /// <inheritdoc />
    public void CancelExecuting(bool throwOnFirstException)
      => this.CommandCancellationTokenSource?.Cancel(throwOnFirstException);

    /// <inheritdoc />
    public void CancelPending()
      => this.SynchronizationCancellationTokenSource?.Cancel();

    /// <inheritdoc />
    public void CancelPending(bool throwOnFirstException)
      => this.SynchronizationCancellationTokenSource?.Cancel(throwOnFirstException);

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

    /// <inheritdoc cref="IAsyncRelayCommand{TParam}"/>
    public void InvalidateCommand()
      => OnCanExecuteChanged();

    /// <summary>
    /// Raises the <see cref="ICommand.CanExecuteChanged"/> event.
    /// </summary>
    protected virtual void OnCanExecuteChanged()
      => this.canExecuteChangedDelegate?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
    /// </summary>
    protected virtual void OnPropertyChnaged([CallerMemberName] string propertyName = "")
      => this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    
    #region ICommand implementation
#if NET
    bool ICommand.CanExecute(object? parameter)
#else
  bool ICommand.CanExecute(object parameter)
#endif
    {
      return parameter == null
        ? CanExecute(default)
        : CanExecute((TParam)parameter);
    }

    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);

    #endregion ICommand implementation

    #region Explicit IAsyncRelayCommand implementation
    /// <inheritdoc />
    async Task IAsyncRelayCommand.ExecuteAsync(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);

    /// <inheritdoc />
    async Task IAsyncRelayCommand.ExecuteAsync(object parameter, CancellationToken cancellationToken) => await ExecuteAsync((TParam)parameter, cancellationToken);

    /// <inheritdoc />
    Task IAsyncRelayCommand.ExecuteAsync(object parameter, TimeSpan timeout) => throw new NotImplementedException();

    /// <inheritdoc />
    Task IAsyncRelayCommand.ExecuteAsync(object parameter, TimeSpan timeout, CancellationToken cancellationToken) => throw new NotImplementedException();
    
    #endregion Explicit IAsyncRelayCommand implementation
  }
}