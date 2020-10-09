using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Framework.Wpf.Generic
{

  /// <summary>
  /// A reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await. Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The individual <see cref="Execute"/>, <see cref="ExecuteAsync"/> and <see cref="CanExecute"/> members are supplied via delegates.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand&lt;T&gt;</c> implements <see cref="System.Windows.Input.ICommand" />.</remarks>
  public class AsyncRelayCommand<TParam> : AsyncRelayCommand, IAsyncRelayCommand<TParam>
  {
    private readonly Func<TParam, Task> executeAsync;
    private readonly Action<TParam> execute;
    private readonly Predicate<TParam> canExecute;


    /// <inheritdoc />
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


    /// <inheritdoc />
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


    /// <inheritdoc />
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam) : base(
      executeNoParam,
      canExecuteNoParam)
    {
    }

    /// <summary>
    ///   Creates a new command.
    /// </summary>
    /// <param name="execute">The execution handler.</param>
    /// <param name="canExecute">The CanExecute handler.</param>
    public AsyncRelayCommand(Action<TParam> execute, Predicate<TParam> canExecute) : base(
      param => execute((TParam)param),
      param => param is TParam predicate && canExecute(predicate))
    {
      this.execute = execute;
      this.canExecute = canExecute;
    }


    /// <inheritdoc />
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


    /// <inheritdoc />
    public bool CanExecute(TParam parameter) => base.CanExecute(parameter);

    /// <summary>
    /// Executes the AsyncRelayCommand on the current command target.
    /// </summary>
    /// <param name="parameter">
    /// The command parameter.
    /// </param>
    /// <remarks> When this method is called although an asynchronous execute delegate was registered, this asynchronous delegate will be executed asynchronously, but since the <see cref="Execute"/> does not return a <see cref="Task"/> and is declared as <c>async void</c>, the execution is not awaitable and more important exceptions from an <c>async void</c> method can’t be caught with <c>catch</c>!
    /// <para></para>Async void methods have different error-handling semantics. When an exception is thrown out of an <c>async Task</c> or <c>async Task&lt;T&gt;</c> method, that exception is captured and placed on the <see cref="Task"/> object. With <c>async void</c> methods, there is no Task object, so any exceptions thrown out of an <c>async void</c> method will be raised directly on the SynchronizationContext that was active when the async void method started. Exceptions thrown from <c>async void</c> methods can’t be caught naturally.
    /// <para></para>In such a scenario it is highly recommended to always call <see cref="ExecuteAsync"/> instead.</remarks>
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