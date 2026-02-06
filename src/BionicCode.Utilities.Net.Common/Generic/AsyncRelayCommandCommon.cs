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
    /// The <see cref="AsyncRelayCommandCommon{TParam}"/> accepts asynchronous command handlers and supports data binding to properties like <see cref="AsyncRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
    /// <br/>Call and await the <see cref="ExecuteAsync(TParam)"/> method or one of its overloads to execute the command explicitly asynchronously.
    ///   <seealso cref="System.Windows.Input.ICommand" />
    /// </summary>
    /// <remarks><c>AsyncRelayCommandCommon</c> implements <see cref="System.Windows.Input.ICommand" />. In case the <see cref="AsyncRelayCommandCommon{TParam}"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="AsyncRelayCommandCommon.ExecuteAsync()"/> or its overloads instead.</remarks>
    public abstract class AsyncRelayCommandCommon<TParam> : AsyncRelayCommandCore, IAsyncRelayCommandCommon<TParam>
    {
        /// <summary>
        /// The registered async execute delegate that supports cancellation and accepts a parameter of <typeparamref name="TParam"/>.
        /// </summary>
        /// <value>
        /// A delegate that supports cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
        private readonly Func<TParam, CancellationToken, Task> cancellableAsyncExecuteDelegate;

        /// <summary>
        /// The registered CanExecute delegate that accepts a parameter of <typeparamref name="TParam"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the command can execute, otherwise <see langword="false"/>.</value>
        private readonly Func<TParam, bool> canExecuteDelegate;

        #region Constructors

        /// <summary>
        ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute"/> will always return <see langword="true"/>) 
        ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>
        ///   <br/>and supports cancellation.
        /// </summary>
        /// <param name="executeAsync">The awaitable execute handler.</param>
        protected AsyncRelayCommandCommon(Func<TParam, CancellationToken, Task> executeAsync)
          : this(executeAsync, param => true)
        {
        }

        /// <summary>
        ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute"/> will always return <see langword="true"/>) 
        ///   <br/>and that accepts a command parameter of type <typeparamref name="TParam"/>.
        /// </summary>
        /// <param name="executeAsync">The awaitable execute handler.</param>
        protected AsyncRelayCommandCommon(Func<TParam, Task> executeAsync)
          : this(executeAsync, param => true)
        {
        }

        /// <summary>
        ///   Creates a new asynchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
        /// </summary>
        /// <param name="executeAsync">The awaitable execute handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        protected AsyncRelayCommandCommon(Func<TParam, Task> executeAsync, Func<TParam, bool> canExecute)
        {
            if (executeAsync is null)
            {
                throw new ArgumentNullException(nameof(executeAsync));
            }

            cancellableAsyncExecuteDelegate = (commandParameter, cancellationToken) => executeAsync.Invoke(commandParameter);
            canExecuteDelegate = canExecute;
        }

        /// <summary>
        ///   Creates a new asynchronous command that supports cancellation and accepts a command parameter of <typeparamref name="TParam"/>.
        /// </summary>
        /// <param name="executeAsync">The awaitable execute handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        protected AsyncRelayCommandCommon(Func<TParam, CancellationToken, Task> executeAsync, Func<TParam, bool> canExecute)
        {
            if (executeAsync is null)
            {
                throw new ArgumentNullException(nameof(executeAsync));
            }

            cancellableAsyncExecuteDelegate = executeAsync;
            canExecuteDelegate = canExecute;
        }

        #endregion Constructors

        /// <summary>
        ///   Determines whether this AsyncRelayCommandCommon can execute.
        /// </summary>
        /// <param name="parameter">
        ///   Data used by the command. 
        /// </param>
        /// <returns><see langword="true"/> if this command can be executed, otherwise <see langword="false"/>.</returns>
        public bool CanExecute(TParam parameter) => canExecuteDelegate?.Invoke(parameter) ?? true;

        /// <inheritdoc />
        public async Task ExecuteAsync(TParam parameter) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, CancellationToken.None).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task ExecuteAsync(TParam parameter, TimeSpan timeout) => await ExecuteAsync(parameter, timeout, CancellationToken.None).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task ExecuteAsync(TParam parameter, CancellationToken cancellationToken) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, cancellationToken).ConfigureAwait(false);

        /// <inheritdoc />
        public async Task ExecuteAsync(TParam parameter, TimeSpan timeout, CancellationToken cancellationToken) => await ExecuteAsync(parameter, Timeout.InfiniteTimeSpan, timeout, cancellationToken).ConfigureAwait(false);

        public async Task ExecuteAsync(TParam parameter, TimeSpan pendingTimeout, TimeSpan executingTimeout, CancellationToken cancellationToken)
          => await ExecuteCoreAsync(ct => cancellableAsyncExecuteDelegate(parameter, ct), pendingTimeout, executingTimeout, cancellationToken).ConfigureAwait(false);

        #region ICommand implementation
#if NET
        /// <inheritdoc />
        bool ICommand.CanExecute(object? parameter) => CanExecute((TParam)parameter);
        /// <inheritdoc />
        public async void Execute(object? parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None).ConfigureAwait(false);
#else
    /// <inheritdoc />
    bool ICommand.CanExecute(object parameter) => CanExecute((TParam)parameter);
    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => await ExecuteAsync((TParam)parameter, CancellationToken.None);
#endif

        #endregion ICommand implementation
    }
}
