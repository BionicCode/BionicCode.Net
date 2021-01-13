using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BionicCode.Utilities.Net.Wpf.Generic;

namespace BionicCode.Utilities.Net.Wpf
{
  /// <summary>
  /// A  reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await. Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The <see cref="AsyncRelayCommand"/> accepts asynchronous command handlers.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" />. In case <see cref="ICommand.Execute"/> is invoked with a registered asynchronous command handler (e.g., by an implementation of <see cref="ICommandSource"/>), the handler is executed asynchronously. In case the <see cref="AsyncRelayCommand"/> is executed explicitly, especially with an asynchronous command handler registered, it is highly recommended to invoke the awaitable ExecuteAsync(object) or one of its overloads instead!</remarks>
  public class AsyncRelayCommand : AsyncRelayCommand<object>
  {
    /// <inheritdoc />
    public AsyncRelayCommand(Action<object> execute) : base(execute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action executeNoParam) : base(executeNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<object, Task> executeAsync) : base(executeAsync)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam) : base(executeAsyncNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : base(executeNoParam, canExecuteNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Action<object> execute, Predicate<object> canExecute) : base(execute, canExecute)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : base(executeAsyncNoParam, canExecuteNoParam)
    {
    }

    /// <inheritdoc />
    public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute) : base(executeAsync, canExecute)
    {
    }
  }
}