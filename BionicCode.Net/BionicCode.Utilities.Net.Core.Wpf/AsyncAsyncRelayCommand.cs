using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BionicCode.Utilities.Net.Core.Wpf
{
  /// <summary>
  /// A  reusable command that encapsulates the implementation of <see cref="ICommand"/> with support for async/await. Enables instant creation of an ICommand without implementing the ICommand interface for each command.
  /// The individual <see cref="Execute()"/>, <see cref="ExecuteAsync()"/> and <see cref="CanExecute()"/> members are supplied via delegates.
  ///   <seealso cref="System.Windows.Input.ICommand" />
  /// </summary>
  /// <remarks><c>AsyncRelayCommand</c> implements <see cref="System.Windows.Input.ICommand" /></remarks>
  public class AsyncRelayCommand : IAsyncRelayCommand
  {
    /// <summary>
    /// The registered parameterless async execute delegate.
    /// </summary>
    /// <value>
    /// A delegate that takes no parameter and returns a <see cref="Task"/>.</value>
    protected readonly Func<Task> ExecuteAsyncNoParam;
    /// <summary>
    /// The registered parameterless synchronous execute delegate.
    /// </summary>
    /// <value>
    /// A delegate that takes no parameter and returns void.</value>
    protected readonly Action ExecuteNoParam;
    /// <summary>
    /// The registered parameterless CanExecute delegate.
    /// </summary>
    /// <value>
    /// <c>true</c> if the command can execute, otherwise <c>false</c>.</value>
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
    ///   Creates a new command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="execute">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action<object> execute)
      : this(execute, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeNoParam">The awaitable execution handler.</param>
    public AsyncRelayCommand(Action executeNoParam)
      : this(executeNoParam, () => true)
    {
    }

    /// <summary>
    ///   Creates a new command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
    /// </summary>
    /// <param name="executeAsync">The awaitable execution handler.</param>
    public AsyncRelayCommand(Func<object, Task> executeAsync)
      : this(executeAsync, param => true)
    {
    }

    /// <summary>
    ///   Creates a new parameterless asynchronous command that can always execute (<see cref="CanExecute()"/> will always return <c>true</c>).
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
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
    public bool CanExecute() => this.CanExecuteNoParam != null && this.CanExecuteNoParam() || this.canExecute != null && this.canExecute(null);
    
    /// <summary>
    /// Executes the <see cref="ICommand"/> on the current command target.
    /// </summary>
    /// <remarks> When this method is called although an asynchronous execute delegate was registered, this asynchronous delegate will be executed asynchronously, but since the <see cref="Execute()"/> does not return a <see cref="Task"/> and is declared as <c>async void</c>, the execution is not awaitable and more important exceptions from an <c>async void</c> method can’t be caught with <c>catch</c>!
    /// <para></para>Async void methods have different error-handling semantics. When an exception is thrown out of an <c>async Task</c> or <c>async Task&lt;T&gt;</c> method, that exception is captured and placed on the <see cref="Task"/> object. With <c>async void</c> methods, there is no Task object, so any exceptions thrown out of an <c>async void</c> method will be raised directly on the SynchronizationContext that was active when the async void method started. Exceptions thrown from <c>async void</c> methods can’t be caught naturally.
    /// <para></para>In such a scenario it is highly recommended to always call <see cref="ExecuteAsync()"/> instead.</remarks>
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
    /// <returns><c>true</c> if this command can be executed, otherwise <c>false</c>.</returns>
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