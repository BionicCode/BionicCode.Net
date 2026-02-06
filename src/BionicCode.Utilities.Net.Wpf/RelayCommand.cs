namespace BionicCode.Utilities.Net
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// A reusable parameterless command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
    /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
    /// The <see cref="RelayCommand"/> supports data binding to properties like <see cref="IRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
    /// </summary>
    /// <remarks>
    /// For an asynchronous version see <see cref="AsyncRelayCommand"/> and <see cref="AsyncRelayCommand{TParam}"/>.
    /// </remarks>
    public class RelayCommand : RelayCommandCommon, IRelayCommand
    {
#if !NETSTANDARD
        private bool isCommandManagerRequerySuggestedEnabled;
        /// <inheritdoc />
        public bool IsCommandManagerRequerySuggestedEnabled
        {
            get => isCommandManagerRequerySuggestedEnabled;
            set
            {
                if (value == IsCommandManagerRequerySuggestedEnabled)
                {
                    return;
                }

                isCommandManagerRequerySuggestedEnabled = value;
                if (IsCommandManagerRequerySuggestedEnabled)
                {
                    // CommandManager internally uses a WeakEventManager to register the event handlers
                    CommandManager.RequerySuggested += OnCommandManagerRequerySuggested;
                }
                else
                {
                    CommandManager.RequerySuggested -= OnCommandManagerRequerySuggested;
                }

                OnPropertyChanged();
            }
        }
#endif

        #region Constructors

        /// <inheritdoc />
        public RelayCommand(Action executeNoParam) : base(executeNoParam)
        {
#if !NETSTANDARD
            IsCommandManagerRequerySuggestedEnabled = true;
#endif
        }

        /// <inheritdoc />
        public RelayCommand(Action<CancellationToken> executeNoParam) : base(executeNoParam)
        {
#if !NETSTANDARD
            IsCommandManagerRequerySuggestedEnabled = true;
#endif
        }

        /// <inheritdoc />
        public RelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : base(executeNoParam, canExecuteNoParam)
        {
#if !NETSTANDARD
            IsCommandManagerRequerySuggestedEnabled = true;
#endif
        }

        /// <inheritdoc />
        public RelayCommand(Action<CancellationToken> execute, Func<bool> canExecute) : base(execute, canExecute)
        {
#if !NETSTANDARD
            IsCommandManagerRequerySuggestedEnabled = true;
#endif
        }

#if !NETSTANDARD
        /// <summary>
        ///   Creates a new parameterless synchronous command that supports cancellation.
        /// </summary>
        /// <param name="execute">The execute handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="System.Windows.Input.CommandManager.RequerySuggested"/> event. 
        /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IRelayCommandCore.InvalidateCommand"/>.
        /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>
        public RelayCommand(Action<CancellationToken> execute, Func<bool> canExecute, bool isCommandManagerRequerySuggestedEnabled) : base(execute, canExecute) => IsCommandManagerRequerySuggestedEnabled = isCommandManagerRequerySuggestedEnabled;

        /// <summary>
        ///   Creates a new parameterless synchronous command.
        /// </summary>
        /// <param name="executeNoParam">The execute handler.</param>
        /// <param name="canExecuteNoParam">The can execute handler.</param>
        /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="System.Windows.Input.CommandManager.RequerySuggested"/> event. 
        /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IRelayCommandCore.InvalidateCommand"/>.
        /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>
        public RelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam, bool isCommandManagerRequerySuggestedEnabled) : base(executeNoParam, canExecuteNoParam)
          => IsCommandManagerRequerySuggestedEnabled = isCommandManagerRequerySuggestedEnabled;

        /// <summary>
        ///   Creates a new parameterless asynchronous command that supports cancellation and does not take a command parameter.
        /// </summary>
        /// <param name="executeAsync">The awaitable execute handler.</param>
        /// <param name="canExecute">The can execute handler.</param>
        /// <param name="isCommandManagerRequerySuggestedEnabled"><see langword="true"/> to enable the WPF framework to raise the CanExecuteChanged event via the <see cref="System.Windows.Input.CommandManager.RequerySuggested"/> event. 
        /// <br/><see langword="false"/> to only raise the <see cref="ICommand.CanExecuteChanged"/> event manually by calling <see cref="IRelayCommandCore.InvalidateCommand"/>.
        /// <br/>The behavior can be changed anytime by setting the <see cref="IsCommandManagerRequerySuggestedEnabled"/> property.</param>

#endif
        #endregion Constructors

        /// <summary>
        /// Event invocator. Called when <see cref="IsCommandManagerRequerySuggestedEnabled"/> is <see langword="true"/> and the <see cref="System.Windows.Input.CommandManager.RequerySuggested"/> is raised.
        /// </summary>
        /// <param name="sender"><see langword="null"/> because the source event is the static <see cref="System.Windows.Input.CommandManager.RequerySuggested"/> event.</param>
        /// <param name="e">The event args object.</param>
#if NET
        protected virtual void OnCommandManagerRequerySuggested(object? sender, EventArgs e)
#else
    protected virtual void OnCommandManagerRequerySuggested(object sender, EventArgs e)
#endif
          => OnCanExecuteChanged(sender, e);
    }
}
