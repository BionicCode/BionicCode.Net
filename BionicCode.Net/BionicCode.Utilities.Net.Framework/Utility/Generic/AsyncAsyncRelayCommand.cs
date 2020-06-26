using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Framework.Utility.Generic
{

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await. Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The individual <see cref="Execute()"/>, <see cref="ExecuteAsync()"/> and <see cref="CanExecute()"/> members are supplied via delegates.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" /></remarks>
  public class AsyncRelayCommand<TParam> : AsyncRelayCommand, IAsyncRelayCommand<TParam>
  {
    private readonly Func<TParam, Task> executeAsync;
    private readonly Action<TParam> execute;
    private readonly Predicate<TParam> canExecute;

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam) : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command that can always execute (<see cref="CanExecute"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<TParam, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless command that can always execute (<see cref="CanExecute"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new command that can always execute (<see cref="CanExecute"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<TParam> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless command.
    /// </summary>
    /// <param name="executeNoParam"></param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : base(
      executeNoParam,
      canExecuteNoParam)
    {
    }

    /// <summary>
    ///   Creates a new command.
    /// </summary>
    /// <param name="execute">The execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Action<TParam> execute, Predicate<TParam> canExecute) : base(
      param => execute((TParam)param),
      param => param is TParam predicate && canExecute(predicate))
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">Parameterless execute handler.</param>
    /// <param name="canExecuteNoParam">Parameterless can execute handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam) : base(
      executeAsyncNoParam,
      canExecuteNoParam)
    {
    }

    /// <summary>
    ///   Creates a new asynchronous command.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Func<TParam, Task> executeAsync, Predicate<TParam> canExecute) : base(
      param => executeAsync((TParam)param),
      param => param is TParam predicate && canExecute(predicate))
    {
      this.executeAsync = executeAsync;
      this.canExecute = canExecute;
    }

    /// <summary>
    ///   Determines whether this AsyncRelayCommand can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><code>true<</code>code> if this command can be executed, otherwise <code>false</code>.</returns>
    public bool CanExecute(TParam parameter) => base.CanExecute(parameter);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target. 
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <remarks>If the execute delegate is asynchronous (awaitable) then the execution is asynchronous otherwise synchronous.</remarks>
    public async void Execute(TParam parameter)
    {
      if (this.executeAsync != null)
      {
        await this.executeAsync.Invoke(parameter);
        return;
      }
      if (this.ExecuteAsyncNoParam != null)
      {
        await this.ExecuteAsyncNoParam.Invoke();
        return;
      }
      if (this.ExecuteNoParam != null)
      {
        this.ExecuteNoParam.Invoke();
        return;
      }
      this.execute?.Invoke(parameter);
    }

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. If the command does not require data to be passed,
    ///   this object can be set to null.
    /// </param>
    /// <remarks>If the execute delegate is asynchronous (awaitable) then the execution is asynchronous otherwise the synchronous execute delegate is wrapped into an asynchronous call. This method is always awaitable and all handlers are always asynchronously executed.</remarks>
    public async Task ExecuteAsync(TParam parameter)
    {
      if (this.executeAsync != null)
      {
        await this.executeAsync.Invoke(parameter);
        return;
      }
      if (this.ExecuteAsyncNoParam != null)
      {
        await this.ExecuteAsyncNoParam.Invoke();
        return;
      }
      if (this.ExecuteNoParam != null)
      {
        await Task.Run(this.ExecuteNoParam.Invoke);
        return;
      }
      await Task.Run(() => this.execute?.Invoke(parameter));
    }
  }
}