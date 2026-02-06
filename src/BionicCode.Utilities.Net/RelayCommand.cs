namespace BionicCode.Utilities.Net
{
    using System;
    using System.ComponentModel;
    using System.Threading;
    using System.Windows.Input;

    /// <summary>
    /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
    /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
    /// The <see cref="RelayCommand"/> supports data binding to properties like <see cref="IRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.  
    /// </summary>
    /// <remarks>See <see cref="RelayCommand{TParam}"/> for a command that accepts a parameter or see <see cref="AsyncRelayCommand"/> and <see cref="AsyncRelayCommand{TParam}"/> for an asynchronous version.</remarks>
    public partial class RelayCommand : RelayCommandCommon, IRelayCommand
    {
        #region Constructors

        /// <inheritdoc />
        public RelayCommand(Action executeNoParam) : base(executeNoParam)
        {
        }

        /// <inheritdoc />
        public RelayCommand(Action<CancellationToken> executeNoParam) : base(executeNoParam)
        {
        }

        /// <inheritdoc />
        public RelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : base(executeNoParam, canExecuteNoParam)
        {
        }

        /// <inheritdoc />
        public RelayCommand(Action<CancellationToken> execute, Func<bool> canExecute) : base(execute, canExecute)
        {
        }

        #endregion Constructors
    }
}
