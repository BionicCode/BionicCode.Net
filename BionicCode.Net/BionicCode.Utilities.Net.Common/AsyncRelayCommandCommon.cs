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
    /// The <see cref="AsyncRelayCommandCommon{TParam}"/> accepts asynchronous command handlers and supports data binding to properties like <see cref="IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
    /// <br/>Call and await the <see cref="ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
    ///   <seealso cref="System.Windows.Input.ICommand" />
    /// </summary>
    /// <remarks><c>AsyncRelayCommandCommon</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommandCommon{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="ExecuteAsync()"/> or its overloads instead.</remarks>
    public abstract partial class AsyncRelayCommandCommon : AsyncRelayCommandCore, IAsyncRelayCommandCommon
    {
        /// <summary>
        /// The registered parameterless async execute delegate that supports cancellation.
        /// </summary>
        /// <value>
        /// A delegate that supports cancellation, but takes no command parameter and returns a <see cref="Task"/>.</value>
        private readonly Func<CancellationToken, Task> cancellableAsyncNoParamExecuteDelegate;

        /// <summary>
        /// The registered parameterless CanExecute delegate.
        /// </summary>
        /// <value>
        /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
        private readonly Func<bool> canExecuteNoParamDelegate;

        #region Constructors

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
        ///   Creates a parameterless new asynchronous command.
        /// </summary>
        /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
        /// <param name="canExecuteNoParam">The execution status handler.</param>
        protected AsyncRelayCommandCommon(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam)
        {
            ArgumentNullExceptionEx.ThrowIfNull(executeAsyncNoParam, nameof(executeAsyncNoParam));

            this.cancellableAsyncNoParamExecuteDelegate = cancellationToken => executeAsyncNoParam.Invoke();
            this.canExecuteNoParamDelegate = canExecuteNoParam ?? (() => true);
        }

        /// <summary>
        ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
        /// </summary>
        /// <param name="executeAsync">The awaitable execution handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        protected AsyncRelayCommandCommon(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute)
        {
            this.cancellableAsyncNoParamExecuteDelegate = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            this.canExecuteNoParamDelegate = canExecute ?? (() => true);
        }

        #endregion Constructors

        /// <summary>
        ///   Determines whether this AsyncRelayCommandCommon can execute.
        /// </summary>
        /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
        public bool CanExecute() => this.canExecuteNoParamDelegate?.Invoke() ?? true;

        /// <inheritdoc />
        public async Task ExecuteAsync() => await ExecuteAsync(Timeout.InfiniteTimeSpan, CancellationToken.None).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task ExecuteAsync(CancellationToken cancellationToken) => await ExecuteAsync(Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task ExecuteAsync(TimeSpan timeout) => await ExecuteAsync(timeout, CancellationToken.None).ConfigureAwait(false);

        /// <summary>
        ///   Executes the AsyncRelayCommand on the current command target asynchronously.
        /// </summary>
        /// <param name="timeout">A <seealso cref="TimeSpan"/> to specify the timeout of the operation. 
        /// <br/>A value of <see cref="Timeout.InfiniteTimeSpan"/> (or a <see cref="TimeSpan"/> that represents -1) will specifiy an infinite time out. 
        /// <br/>A value of <see cref="TimeSpan.Zero"/> will cancel the operation immediately.</param>
        /// <param name="cancellationToken">An instance of <seealso cref="CancellationToken"/> to cancel the executing command delegate.</param>
        /// <remarks>If the registered command handler is asynchronous (awaitable), then the execution is asynchronous otherwise the delegate is executed synchronously. 
        /// <br/><br/>Repeated or concurrent calls are synchronized.
        /// </remarks>
        /// <exception cref="OperationCanceledException">If the executing command delegate was cancelled.</exception>
        /// <exception cref="ArgumentOutOfRangeExceptionEx"><paramref name="timeout>"/>.TotalMilliseconds is less than -1 or greater than <see cref="int.MaxValue"/> (or <see cref="uint.MaxValue"/> - 1 on some versions of .NET). Note that this upper bound is more restrictive than <see cref="TimeSpan.MaxValue"/>.</exception>
        public virtual async Task ExecuteAsync(TimeSpan timeout, CancellationToken cancellationToken) => await ExecuteAsync(Timeout.InfiniteTimeSpan, timeout, cancellationToken).ConfigureAwait(false);

        public virtual async Task ExecuteAsync(TimeSpan pendingTimeout, TimeSpan executingTimeout, CancellationToken cancellationToken)
          => await ExecuteCoreAsync(this.cancellableAsyncNoParamExecuteDelegate, pendingTimeout, executingTimeout, cancellationToken).ConfigureAwait(false);

        #region ICommand implementation

#if NET
        /// <inheritdoc />
        bool ICommand.CanExecute(object? parameter) => CanExecute();
        /// <inheritdoc />
        public async void Execute(object? parameter) => await ExecuteAsync().ConfigureAwait(false);
#else
    /// <inheritdoc />
    bool ICommand.CanExecute(object parameter) => CanExecute();
    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync();
#endif

        #endregion ICommand implementation
    }
}
