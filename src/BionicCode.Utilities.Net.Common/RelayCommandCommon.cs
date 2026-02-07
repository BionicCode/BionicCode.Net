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
    public abstract class RelayCommandCommon : RelayCommandCore, IRelayCommandCommon
    {
        /// <summary>
        /// The registered execute delegate that accepts a parameter of <typeparamref name="TParam"/>.
        /// </summary>
        /// <value>
        /// A delegate that supports cancellation and takes a command parameter of <typeparamref name="TParam"/> and returns a <see cref="Task"/>.</value>
        private readonly Action<CancellationToken> cancellableExecuteDelegate;

        /// <summary>
        /// The registered CanExecute delegate that accepts a parameter of <typeparamref name="TParam"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the command can execute, otherwise <see langword="false"/>.</value>
        private readonly Func<bool> canExecuteDelegate;

        #region Constructors

        /// <summary>
        ///   Creates a new synchronous parameterless command that can always execute (<see cref="CanExecute"/> will always return <see langword="true"/>)
        ///   <br/> and accepts a command parameter of type <typeparamref name="TParam"/>.
        /// </summary>
        /// <param name="execute">The execute handler.</param>
        protected RelayCommandCommon(Action execute)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(execute, nameof(execute));

            cancellableExecuteDelegate = cancellationToken => execute.Invoke();
            canExecuteDelegate = () => true;
        }

        /// <summary>
        ///   Creates a new parameterless synchronous command that can always execute (<see cref="CanExecute"/> will always return <see langword="true"/>) 
        ///   <br/>and accepts a command parameter of type <typeparamref name="TParam"/>
        ///   <br/>and supports cancellation.
        /// </summary>
        /// <param name="execute">The execute handler.</param>
        protected RelayCommandCommon(Action<CancellationToken> execute)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(execute, nameof(execute));

            cancellableExecuteDelegate = execute;
            canExecuteDelegate = () => true;
        }

        /// <summary>
        ///   Creates a new synchronous command that accepts a command parameter of type <typeparamref name="TParam"/>.
        /// </summary>
        /// <param name="execute">The execute handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        protected RelayCommandCommon(Action execute, Func<bool> canExecute)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(execute, nameof(execute));

            cancellableExecuteDelegate = cancellationToken => execute.Invoke();
            canExecuteDelegate = canExecute is null ? () => true : canExecute;
        }

        /// <summary>
        ///   Creates a new synchronous command that supports cancellation and accepts a command parameter of type <typeparamref name="TParam"/>.
        /// </summary>
        /// <param name="execute">The execute handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        protected RelayCommandCommon(Action<CancellationToken> execute, Func<bool> canExecute)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(execute, nameof(execute));

            cancellableExecuteDelegate = execute;
            canExecuteDelegate = canExecute is null ? () => true : canExecute;
        }

        #endregion Constructors

        /// <summary>
        ///   Determines whether this AsyncRelayCommandCommon can execute.
        /// </summary>
        /// <returns><see langword="true"/> if this command can be executed, otherwise <see langword="false"/>.</returns>
        public bool CanExecute() => canExecuteDelegate.Invoke();

        /// <inheritdoc />
        public void Execute() => Execute(Timeout.InfiniteTimeSpan, CancellationToken.None);

        /// <inheritdoc />    
        public void Execute(TimeSpan timeout) => Execute(timeout, CancellationToken.None);

        /// <inheritdoc />
        public void Execute(CancellationToken cancellationToken) => Execute(Timeout.InfiniteTimeSpan, cancellationToken);

        /// <inheritdoc />
        public void Execute(TimeSpan timeout, CancellationToken cancellationToken) => Execute(Timeout.InfiniteTimeSpan, timeout, cancellationToken);

        /// <inheritdoc />
        public void Execute(TimeSpan pendingTimeout, TimeSpan executingTimeout, CancellationToken cancellationToken)
          => ExecuteCore(cancellableExecuteDelegate, pendingTimeout, executingTimeout, cancellationToken);

        #region ICommand implementation
#if NET
        /// <inheritdoc />
        bool ICommand.CanExecute(object? parameter) => CanExecute();
        /// <inheritdoc />
        async void ICommand.Execute(object? parameter) => Execute();
#else
    /// <inheritdoc />
    bool ICommand.CanExecute(object parameter) => CanExecute();
    /// <inheritdoc />
    async void ICommand.Execute(object parameter) => Execute();
#endif

        #endregion ICommand implementation
    }
}
