using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Uwp.Generic
{
  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await. Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommand"/> accepts asynchronous command handlers.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" />. In case <see cref="ICommand.Execute"/> is invoked with a registered asynchronous command handler, the handler is executed asynchronously. In case the <see cref="AsyncRelayCommand"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="ExecuteAsync()"/> or its overloads instead!</remarks>
  public class AsyncRelayCommand<TParam> : IAsyncRelayCommand<TParam>, IAsyncRelayCommand
  {
    /// <inheritdoc cref="IAsyncRelayCommand{TParam}"/>
    public bool IsExecuting => this.executionCount > 0;

    /// <summary>
    /// The registered parameterless async execute delegate.
    /// </summary>
    /// <value>
    /// A delegate that takes no parameter and returns a <see cref="Task"/>.</value>
    protected readonly Func<Task> ExecuteAsyncNoParam;
    /// <summary>
    /// The registered parameterless synchronous execute delegate.
    /// </summary>
    /// <value>
    /// A delegate that takes no parameter and returns void.</value>
    protected readonly Action ExecuteNoParam;

    /// <summary>
    /// The registered parameterless CanExecute delegate.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
    protected readonly Func<bool> CanExecuteNoParam;

    private readonly Func<TParam, Task> executeAsync;
    private readonly Action<TParam> execute;
    private readonly Predicate<TParam> canExecute;
    private int executionCount;

    /// <inheritdoc />
    public event EventHandler CanExecuteChanged;

    #region Constructors

    /// <summary>
    ///   Creates a new command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<TParam> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
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
    ///   Creates a new parameterless command.
    /// </summary>
    /// <param name="executeNoParam">The execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam)
    {
      this.ExecuteNoParam = executeNoParam ?? throw new ArgumentNullException(nameof(executeNoParam));
      this.CanExecuteNoParam = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a new command.
    /// </summary>
    /// <param name="execute">The execution handler.</param>
    /// <param name="canExecute">The execution status handler.</param>
    public AsyncRelayCommand(Action<TParam> execute, Predicate<TParam> canExecute)
    {
      this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
      this.canExecute = canExecute ?? (param => true); ;
    }

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam)
    {
      this.ExecuteAsyncNoParam = executeAsyncNoParam ?? throw new ArgumentNullException(nameof(executeAsyncNoParam));
      this.CanExecuteNoParam = canExecuteNoParam ?? (() => true);
    }

    /// <summary>
    ///   Creates a new asynchronous command.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Func<TParam, Task> executeAsync, Predicate<TParam> canExecute)
    {
      this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.canExecute = canExecute ?? (param => true); ;
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
    public bool CanExecute(TParam parameter) => this.canExecute?.Invoke(parameter)
                                                ?? this.CanExecuteNoParam?.Invoke()
                                                ?? true;
    /// <summary>
    ///  Explicit <see cref="ICommand"/> implementation. Determines whether this AsyncRelayCommand can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    bool ICommand.CanExecute(object parameter) => CanExecute((TParam)parameter);

    /// <summary>
    /// Explicit <see cref="ICommand"/> implementation. Executes the AsyncRelayCommand on the current command target. 
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <remarks>For asynchronous command handlers always prefer to call <see cref="ExecuteAsync()"/> instead!
    /// <para>If the command handler is asynchronous (awaitable) then the execution is asynchronous otherwise the delegate is executed synchronously.</para></remarks>
    async void ICommand.Execute(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);

    /// <inheritdoc />
    async Task IAsyncRelayCommand.ExecuteAsync(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);

    /// <inheritdoc />
    async Task IAsyncRelayCommand.ExecuteAsync(object parameter, CancellationToken cancellationToken) => await ExecuteAsync((TParam)parameter, cancellationToken);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.</remarks>
    public async Task ExecuteAsync() => await ExecuteAsync(default, CancellationToken.None);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.</remarks>
    public async Task ExecuteAsync(CancellationToken cancellationToken) => await ExecuteAsync(default, cancellationToken);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.</remarks>
    public async Task ExecuteAsync(TParam parameter) => await ExecuteAsync(parameter, CancellationToken.None);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/>.</param>
    /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously.</remarks>
    public async Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken)
    {
      try
      {
        Interlocked.Increment(ref this.executionCount);
        cancellationToken.ThrowIfCancellationRequested();

        if (this.executeAsync != null)
        {
          await this.executeAsync.Invoke(parameter).ConfigureAwait(false);
          return;
        }
        if (this.ExecuteAsyncNoParam != null)
        {
          await this.ExecuteAsyncNoParam.Invoke().ConfigureAwait(false);
          return;
        }
        if (this.ExecuteNoParam != null)
        {
          this.ExecuteNoParam.Invoke();
          return;
        }

        this.execute?.Invoke(parameter);
      }
      finally
      {
        Interlocked.Decrement(ref this.executionCount);
      }
    }

    /// <inheritdoc cref="IAsyncRelayCommand{TParam}"/>
    public void InvalidateCommand() => OnCanExecuteChanged();

    /// <summary>
    /// Raises the <see cref="CanExecuteChanged"/> event.
    /// </summary>
    protected virtual void OnCanExecuteChanged()
    {
      this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
  }
}