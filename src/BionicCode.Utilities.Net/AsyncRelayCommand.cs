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
    /// The <see cref="AsyncRelayCommand"/> accepts asynchronous command handlers and supports data binding to properties like <see cref="IAsyncRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
    /// <br/>Call and await the <see cref="IAsyncRelayCommandCommon.ExecuteAsync()"/> method or one of its overloads to execute the command explicitly asynchronously.
    ///   <seealso cref="System.Windows.Input.ICommand" />
    /// </summary>
    /// <remarks>In case the <see cref="AsyncRelayCommand"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable <see cref="AsyncRelayCommandCommon.ExecuteAsync()"/> or its overloads instead.</remarks>
    public partial class AsyncRelayCommand : AsyncRelayCommandCommon, IAsyncRelayCommand
    {
        #region Constructors

        /// <inheritdoc />
        public AsyncRelayCommand(Func<Task> executeAsyncNoParam) : base(executeAsyncNoParam)
        {
        }

        /// <inheritdoc />
        public AsyncRelayCommand(Func<CancellationToken, Task> executeAsyncNoParam) : base(executeAsyncNoParam)
        {
        }

        /// <inheritdoc />
        public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : base(executeAsyncNoParam, canExecuteNoParam)
        {
        }

        /// <inheritdoc />
        public AsyncRelayCommand(Func<CancellationToken, Task> executeAsync, Func<bool> canExecute) : base(executeAsync, canExecute)
        {
        }

        #endregion Constructors
    }
}
