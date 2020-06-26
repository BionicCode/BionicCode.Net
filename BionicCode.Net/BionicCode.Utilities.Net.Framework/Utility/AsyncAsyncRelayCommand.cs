using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Framework.Utility
{
  /// <summary>
  /// A  reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await. Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The individual <see cref="Execute()"/>, <see cref="ExecuteAsync()"/> and <see cref="CanExecute()"/> members are supplied via delegates.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" /></remarks>
  public class AsyncRelayCommand : IAsyncRelayCommand
  {
    protected readonly Func<Task> ExecuteAsyncNoParam;
    protected readonly Action ExecuteNoParam;
    protected readonly Func<bool> CanExecuteNoParam;
    private readonly Func<object, Task> executeAsync;
    private readonly Action<object> execute;
    private readonly Predicate<object> canExecute;

    /// <summary>
    ///   Raised when RaiseCanExecuteChanged is called.
    /// </summary>
    public event EventHandler CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }

    /// <summary>
    ///   Creates a new command that can always execute (<see cref="CanExecute()"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<object> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless command that can always execute (<see cref="CanExecute()"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new command that can always execute (<see cref="CanExecute()"/> always returns <code>true</code>).
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<object, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()")/> always returns <code>true</code>).
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam)
      : this(executeAsyncNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless command.
    /// </summary>
    /// <param name="executeNoParam">The execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Action executeNoParam, Func<bool> canExecuteNoParam)
    {
      this.ExecuteNoParam = executeNoParam ?? throw new ArgumentNullException(nameof(executeNoParam));
      this.CanExecuteNoParam = canExecuteNoParam;
    }

    /// <summary>
    ///   Creates a new command.
    /// </summary>
    /// <param name="execute">The execution handler.</param>
    /// <param name="canExecute">The execution status handler.</param>
    public AsyncRelayCommand(Action<object> execute, Predicate<object> canExecute)
    {
      this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
      this.canExecute = canExecute;
    }

    /// <summary>
    ///   Creates a parameterless new asynchronous command.
    /// </summary>
    /// <param name="executeAsyncNoParam">The awaitable execution handler.</param>
    /// <param name="canExecuteNoParam">The execution status handler.</param>
    public AsyncRelayCommand(Func<Task> executeAsyncNoParam, Func<bool> canExecuteNoParam)
    {
      this.ExecuteAsyncNoParam = executeAsyncNoParam ?? throw new ArgumentNullException(nameof(executeAsyncNoParam));
      this.CanExecuteNoParam = canExecuteNoParam;
    }

    /// <summary>
    ///   Creates a new asynchronous command.
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    /// <param name="canExecute">The can execute handler.</param>
    public AsyncRelayCommand(Func<object, Task> executeAsync, Predicate<object> canExecute)
    {
      this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.canExecute = canExecute;
    }

    /// <summary>
    ///   Determines whether this AsyncRelayCommand can execute.
    /// </summary>
    /// <returns><code>true</code>code> if this command can be executed, otherwise <code>false</code>.</returns>
    public bool CanExecute() => this.CanExecuteNoParam != null && this.CanExecuteNoParam() || this.canExecute != null && this.canExecute(null);

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target. 
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <remarks>If the execute delegate is asynchronous (awaitable) then the execution is asynchronous otherwise synchronous.</remarks>
    public async void Execute()
    {
      if (this.executeAsync != null)
      {
        await this.executeAsync.Invoke(null);
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
      this.execute?.Invoke(null);
    }

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target asynchronously.
    /// </summary>
    /// <remarks>If the execute delegate is asynchronous (awaitable) then the execution is asynchronous otherwise the synchronous execute delegate is wrapped into an asynchronous call. This method is always awaitable and all handlers are always asynchronously executed.</remarks>
    public async Task ExecuteAsync()
    {
      if (this.executeAsync != null)
      {
        await this.executeAsync.Invoke(null);
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
      await Task.Run(() => this.execute?.Invoke(null));
    }

    /// <summary>
    ///   Determines whether this AsyncRelayCommand can execute.
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <returns><code>true</code>code> if this command can be executed, otherwise <code>false</code>.</returns>
    public bool CanExecute(object parameter) => this.canExecute != null && this.canExecute(parameter) || this.CanExecuteNoParam != null && this.CanExecuteNoParam();

    /// <summary>
    ///   Executes the AsyncRelayCommand on the current command target. 
    /// </summary>
    /// <param name="parameter">
    ///   Data used by the command. 
    /// </param>
    /// <remarks>If the execute delegate is asynchronous (awaitable) then the execution is asynchronous otherwise synchronous.</remarks>
    public async void Execute(object parameter)
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
    public async Task ExecuteAsync(object parameter)
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