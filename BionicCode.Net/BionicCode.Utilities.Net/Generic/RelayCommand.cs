namespace BionicCode.Utilities.Net
{
  using System;
  using System.ComponentModel;
  using System.Threading;
  using System.Windows.Input;

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await command delegates. 
  /// <br/>Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="RelayCommand{TParam}"/> supports data binding to properties like <see cref="IRelayCommandCore.IsExecuting"/> by implementing <see cref="INotifyPropertyChanged"/>.
  /// </summary>
  /// <remarks>See <see cref="RelayCommand"/> for a parameterless command or see <see cref="AsyncRelayCommand"/> and <see cref="AsyncRelayCommand{TParam}"/> for an asynchronous version.</remarks>
  public class RelayCommand<TParam> : RelayCommandCommon<TParam>, IRelayCommand<TParam>
  {
    #region Constructors

    /// <inheritdoc />
    public RelayCommand(Action<TParam> execute) : base(execute)
    {
    }

    /// <inheritdoc />
    public RelayCommand(Action<TParam, CancellationToken> execute) : base(execute)
    {
    }

    /// <inheritdoc />
    public RelayCommand(Action<TParam> execute, Func<TParam, bool> canExecute) : base(execute, canExecute)
    {
    }

    /// <inheritdoc />
    public RelayCommand(Action<TParam, CancellationToken> execute, Func<TParam, bool> canExecute) : base(execute, canExecute)
    {
    }

    #endregion Constructors
  }
}