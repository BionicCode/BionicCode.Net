namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;
  using System.Threading.Tasks;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/>. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// </summary>
  public abstract class RelayCommandCommon<TParam> : RelayCommandCore, IRelayCommandCommon<TParam>
  {
    /// <summary>
    /// The registered execute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// A delegate that supports cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
    private readonly Action<TParam, CancellationToken> executeCancellableDelegate;

    /// <summary>
    /// The registered CanExecute delegate that accepts a parameter of <typeparamref name="TParam"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
    private readonly Func<TParam, bool> canExecuteDelegate;

    #region Constructors

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>)
    ///   <br/> and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    protected RelayCommandCommon(Action<TParam> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new synchronous parameterless command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>)
    ///   <br/> and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    protected RelayCommandCommon(Action execute)
    {
      ArgumentNullExceptionEx.ThrowIfNull(execute, nameof(execute));

      this.executeCancellableDelegate = (commandParameter, cancellationToken) => execute.Invoke();
      this.canExecuteDelegate = commandParameter => true;
    }

    /// <summary>
    ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>) 
    ///   <br/>and accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    protected RelayCommandCommon(Action<CancellationToken> execute)
    {
      ArgumentNullExceptionEx.ThrowIfNull(execute, nameof(execute));

      this.executeCancellableDelegate = (commandParameter, cancellationToken) => execute.Invoke(cancellationToken);
      this.canExecuteDelegate = commandParameter => true;
    }

    /// <summary>
    ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected RelayCommandCommon(Action execute, Func<bool> canExecute)
    {
      ArgumentNullExceptionEx.ThrowIfNull(execute, nameof(execute));

      this.executeCancellableDelegate = (commandParameter, cancellationToken) => execute.Invoke();
      this.canExecuteDelegate = commandParameter => canExecute?.Invoke() ?? true;
    }

    /// <summary>
    ///   Creates a new synchronous command that supports cancellation and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected RelayCommandCommon(Action<CancellationToken> execute, Func<bool> canExecute)
    {
      ArgumentNullExceptionEx.ThrowIfNull(execute, nameof(execute));

      this.executeCancellableDelegate = (commandParameter, cancellationToken) => execute.Invoke(cancellationToken);
      this.canExecuteDelegate = commandParameter => canExecute?.Invoke() ?? true;
    }

    /// <summary>
    ///   Creates a new synchronous command that can always execute (<see cref="CanExecute"/> will always return <c>true</c>) 
    ///   <br/>and accepts a command parameter of type <typeparamref name="TParam"/>
    ///   <br/>and supports cancellation.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    protected RelayCommandCommon(Action<TParam, CancellationToken> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected RelayCommandCommon(Action<TParam> execute, Func<TParam, bool> canExecute)
    {
      ArgumentNullExceptionEx.ThrowIfNull(execute, nameof(execute));

      this.executeCancellableDelegate = (commandParameter, cancellationToken) => execute.Invoke(commandParameter);
      this.canExecuteDelegate = canExecute;
    }

    /// <summary>
    ///   Creates a new synchronous command that supports cancellation and accepts a command parameter of type <typeparamref name="TParam"/>.
    /// </summary>
    /// <param name="execute">The execute handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    protected RelayCommandCommon(Action<TParam, CancellationToken> execute, Func<TParam, bool> canExecute)
    {
      ArgumentNullExceptionEx.ThrowIfNull(execute, nameof(execute));

      this.executeCancellableDelegate = execute;
      this.canExecuteDelegate = canExecute;
    }

    #endregion Constructors

    /// <summary>
    ///   Determines whether this AsyncRelayCommandCommon can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute(TParam parameter) => this.canExecuteDelegate?.Invoke(parameter) ?? true;

    /// <inheritdoc />
    public void Execute(TParam parameter) => Execute(parameter, Timeout.InfiniteTimeSpan, CancellationToken.None);

    /// <inheritdoc />
    public void Execute(TParam parameter, TimeSpan timeout) => Execute(parameter, timeout, CancellationToken.None);

    /// <inheritdoc />
    public void Execute(TParam parameter, CancellationToken cancellationToken) => Execute(parameter, Timeout.InfiniteTimeSpan, cancellationToken);

    /// <inheritdoc />
    public  void Execute(TParam parameter, TimeSpan timeout, CancellationToken cancellationToken)
    {
      try
      {
        BeginExecuteCore(timeout, cancellationToken);

        this.CurrentCancellationToken.ThrowIfCancellationRequested();
        this.executeCancellableDelegate?.Invoke(parameter, this.CurrentCancellationToken);
      }
      finally
      {
        EndExecuteCore();
      }
    }

    #region ICommand implementation
#if NET
    /// <inheritdoc />
    bool ICommand.CanExecute(object? parameter) => CanExecute((TParam)parameter);
    /// <inheritdoc />
    async void ICommand.Execute(object? parameter) => Execute((TParam)parameter, CancellationToken.None);
#else
    /// <inheritdoc />
    bool ICommand.CanExecute(object parameter) => CanExecute((TParam)parameter);
    /// <inheritdoc />
    async void ICommand.Execute(object parameter) =>  Execute((TParam)parameter, CancellationToken.None);
#endif

    #endregion ICommand implementation
  }
}