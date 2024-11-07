// Ignore Spelling: Cancelled Cancellable

namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using BionicCode.Utilities.Net;
  using System.Threading;
  using System.Threading.Tasks;
  using FluentAssertions;
  using Xunit;
  using System.Windows.Input;

  public class AsyncCommandTest : IDisposable
  {
    private static bool IsProfilerLoggingEnabled { get; } = true;
    private TaskCompletionSource executedTaskCompletionSource;
    private TaskCompletionSource executingTaskCompletionSource;
    private readonly TaskCompletionSource cancellationTokenTaskCompletionSource;

    private IAsyncRelayCommand<string> TestCommand { get; }
    private IAsyncRelayCommand TestNoParamCommand { get; }
    private IAsyncRelayCommand<string> AsyncTestCommand { get; }
    private IAsyncRelayCommand AsyncTestNoParamCommand { get; }
    private IAsyncRelayCommand<string> AsyncNonValidatingTestCommand { get; }
    private IAsyncRelayCommand<string> AsyncThrowingTestCommand { get; }
    private IAsyncRelayCommand AsyncThrowingTestNoParamCommand { get; }
    private IAsyncRelayCommand<string> AsyncCancellableTestCommand { get; }
    private IAsyncRelayCommand<string> CancellableTestCommand { get; }
    private IAsyncRelayCommand AsyncCancellableTestNoParamCommand { get; }
    private IAsyncRelayCommand CancellableTestNoParamCommand { get; }

    private TimeSpan Timeout { get; }
    private TimeSpan AsyncDelay { get; }
    private TimeSpan LongRunningAsyncDelay { get; }
    private string InvalidCommandParameter => "Some invalid command parameter";
    private string ValidCommandParameter => "@Some valid command parameter";
    private long executedCount;
    private long commandsStartedCount;
    private long commandsToAwaitCount;
    private long waitedCount;

    public AsyncCommandTest()
    {
      this.Timeout = TimeSpan.FromMilliseconds(10);
      this.AsyncDelay = TimeSpan.FromMilliseconds(0.1);
      this.LongRunningAsyncDelay = TimeSpan.FromSeconds(10);
      this.executingTaskCompletionSource = new TaskCompletionSource();
      this.executedTaskCompletionSource = new TaskCompletionSource();
      this.cancellationTokenTaskCompletionSource = new TaskCompletionSource();

      this.TestCommand = new AsyncRelayCommand<string>(ExecuteTestCommand, CanExecuteTestCommand);
      this.TestCommand.Executing += OnCommandExecuting;
      this.TestCommand.Executed += OnCommandExecuted;

      this.TestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommand, CanExecuteTestNoParamCommand);
      this.TestNoParamCommand.Executing += OnCommandExecuting;
      this.TestNoParamCommand.Executed += OnCommandExecuted;

      this.AsyncTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync, CanExecuteTestCommand);
      this.AsyncTestCommand.Executing += OnCommandExecuting;
      this.AsyncTestCommand.Executed += OnCommandExecuted;

      this.AsyncTestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.AsyncTestNoParamCommand.Executing += OnCommandExecuting;
      this.AsyncTestNoParamCommand.Executed += OnCommandExecuted;

      this.AsyncNonValidatingTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync);
      this.AsyncNonValidatingTestCommand.Executing += OnCommandExecuting;
      this.AsyncNonValidatingTestCommand.Executed += OnCommandExecuted;

      this.AsyncThrowingTestCommand = new AsyncRelayCommand<string>(ExecuteThrowingTestCommandAsync, CanExecuteTestCommand);
      this.AsyncThrowingTestCommand.Executing += OnCommandExecuting;
      this.AsyncThrowingTestCommand.Executed += OnCommandExecuted;

      this.AsyncThrowingTestNoParamCommand = new AsyncRelayCommand(ExecuteThrowingTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.AsyncThrowingTestNoParamCommand.Executing += OnCommandExecuting;
      this.AsyncThrowingTestNoParamCommand.Executed += OnCommandExecuted;

      this.AsyncCancellableTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommandAsync, CanExecuteTestCommand);
      this.AsyncCancellableTestCommand.Executing += OnCommandExecuting;
      this.AsyncCancellableTestCommand.Executed += OnCommandExecuted;

      this.AsyncCancellableTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.AsyncCancellableTestNoParamCommand.Executing += OnCommandExecuting;
      this.AsyncCancellableTestNoParamCommand.Executed += OnCommandExecuted;

      this.CancellableTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommand, CanExecuteTestCommand);
      this.CancellableTestCommand.Executing += OnCommandExecuting;
      this.CancellableTestCommand.Executed += OnCommandExecuted;

      this.CancellableTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommand, CanExecuteTestNoParamCommand);
      this.CancellableTestNoParamCommand.Executing += OnCommandExecuting;
      this.CancellableTestNoParamCommand.Executed += OnCommandExecuted;
    }

    public void Dispose()
    {
      _ = this.AsyncNonValidatingTestCommand.CancelAll();
      _ = this.AsyncCancellableTestCommand.CancelAll();
      _ = this.AsyncThrowingTestCommand.CancelAll();
      _ = this.AsyncTestCommand.CancelAll();
      _ = this.TestCommand.CancelAll();
      _ = this.AsyncCancellableTestNoParamCommand.CancelAll();
      _ = this.AsyncThrowingTestNoParamCommand.CancelAll();
      _ = this.AsyncTestNoParamCommand.CancelAll();
      _ = this.TestNoParamCommand.CancelAll();
      _ = this.CancellableTestCommand.CancelAll();
      _ = this.CancellableTestNoParamCommand.CancelAll();
    }

    #region Non test members
    private static string CrateProfilerSummary(string summary, string currentMethodName)
     => $"UnitTest profiler summary: {currentMethodName}{System.Environment.NewLine}{summary}";

    private static Action<ProfilerBatchResult, string> CreateProfilerLogger([CallerMemberName] string currentMethodName = "unknown")
      => (result, summary) => ProfilerLogger(result, summary, currentMethodName);

    private static void ProfilerLogger(ProfilerBatchResult result, string summary, string currentMethodName)
    {
      File.WriteAllText("profiler_summary.log", CrateProfilerSummary(result.Summary, currentMethodName));
      Debug.WriteLine(CrateProfilerSummary(result.Summary, currentMethodName), "profiling");
    }

    private async Task WaitForCancellationAsync(CancellationToken cancellationToken)
    {
      using (CancellationTokenRegistration registration = cancellationToken.Register(this.cancellationTokenTaskCompletionSource.SetResult))
      {
        await this.cancellationTokenTaskCompletionSource?.Task;
      }
    }

    private Task WaitForExecutionStartedAsync(int count)
    {
      this.commandsToAwaitCount = count;
      return this.executingTaskCompletionSource?.Task;
    }

    private Task WaitForExecutionCompletedAsync()
      => this.executedTaskCompletionSource?.Task;

    private void ResetWaitForExecution()
    {
      this.commandsToAwaitCount = -1;
      this.executedCount = 0;
      this.waitedCount = 0;
      this.commandsStartedCount = 0;
      this.executingTaskCompletionSource = new TaskCompletionSource();
      this.executedTaskCompletionSource = new TaskCompletionSource();
    }

    private void OnCommandExecuted(object sender, EventArgs e)
    {
      _ = Interlocked.Increment(ref this.executedCount);
      _ = Interlocked.Increment(ref this.waitedCount);
      long waitedCount = Interlocked.Read(ref this.executedCount);
      long executedCount = Interlocked.Read(ref this.waitedCount);
      if (waitedCount == this.commandsToAwaitCount || executedCount == this.commandsStartedCount)
      {
        this.executedTaskCompletionSource.SetResult();
      }
    }

    private void OnCommandExecuting(object sender, EventArgs e)
    {
      _ = Interlocked.Increment(ref this.commandsStartedCount);
      long waitCount = Interlocked.Read(ref this.commandsToAwaitCount);
      if (waitCount == this.commandsStartedCount)
      {
        this.executingTaskCompletionSource.SetResult();
      }
    }

    private bool CanExecuteTestCommand(string commandParameter) => commandParameter?.StartsWith("@") ?? false;

    private bool CanExecuteTestNoParamCommand() => true;
    private void ExecuteTestCommand(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteTestCommandWithExecutionCount(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      _ = Interlocked.Increment(ref this.executedCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteTestNoParamCommand() => Thread.Sleep(this.AsyncDelay);

    private void ExecuteTestNoParamCommandWithExecutionCount()
    {
      _ = Interlocked.Increment(ref this.executedCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private async Task ExecuteTestCommandAsync(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteTestNoParamCommandAsync() 
      => await Task.Delay(this.AsyncDelay);

    private async Task ExecuteTestNoParamCommandWithExecutionCountAsync()
    {
      _ = Interlocked.Increment(ref this.executedCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteCancellableTestCommandAsync(string commandParameter, CancellationToken cancellationToken)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private async Task ExecuteCancellableTestNoParamCommandAsync(CancellationToken cancellationToken)
      => await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);

    private void ExecuteCancellableTestCommand(string commandParameter, CancellationToken cancellationToken)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      var spinWait = new SpinWait();
      while (!cancellationToken.IsCancellationRequested)
      {
        spinWait.SpinOnce();
      }

      cancellationToken.ThrowIfCancellationRequested();
    }

    private void ExecuteCancellableTestCommandWithoutThrowingCancellationException(string commandParameter, CancellationToken cancellationToken)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      var spinWait = new SpinWait();
      while (!cancellationToken.IsCancellationRequested)
      {
        spinWait.SpinOnce();
      }

      cancellationToken.ThrowIfCancellationRequested();
    }

    private void ExecuteCancellableTestNoParamCommand(CancellationToken cancellationToken)
    {
      var spinWait = new SpinWait();
      while (!cancellationToken.IsCancellationRequested)
      {
        spinWait.SpinOnce();
      }

      cancellationToken.ThrowIfCancellationRequested();
    }

    private async Task ExecuteThrowingTestCommandAsync(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    private async Task ExecuteThrowingTestNoParamCommandAsync()
    {
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    #endregion Non test members

    //[Fact]
    //public async Task AwaitSynchronousCommand()
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
    //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(() => this.TestCommand.ExecuteAsync(this.ValidCommandParameter), 1, logger);
    //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
    //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.TotalMilliseconds);
    //}

    //[Fact]
    //public async Task AwaitSynchronousNoParamCommand()
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
    //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(this.TestNoParamCommand.ExecuteAsync, 1, logger);
    //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
    //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.TotalMilliseconds);
    //}

    //[Fact]
    //public async Task AwaitAsynchronousCommand()
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
    //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(() => this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter), 1, logger);
    //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
    //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.TotalMilliseconds);
    //}

    //[Fact]
    //public async Task AwaitAsynchronousNoParamCommand()
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
    //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(this.AsyncTestNoParamCommand.ExecuteAsync, 1, logger);
    //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
    //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.TotalMilliseconds);
    //}

    [Fact]
    public async Task AwaitAsynchronousCommandThrowsExceptionInCallerContext()
      => _ = await this.AsyncThrowingTestCommand.Awaiting(command => command.ExecuteAsync(this.ValidCommandParameter))
        .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");

    [Fact]
    public async Task AwaitAsynchronousNoParamCommandThrowsExceptionInCallerContext()
      => _ = await this.AsyncThrowingTestNoParamCommand.Awaiting(command => command.ExecuteAsync())
        .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");

    [Fact]
    public void CanExecuteWithInvalidParameterReturnsFalse()
      => _ = this.AsyncTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeFalse("command parameter is invalid.");

    [Fact]
    public void ParameterlessCanExecuteReturnsTrueForNonValidatingCommand()
      => _ = this.AsyncNonValidatingTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeTrue("command was created without defining a CanExecute delegate. Therefore the default is used which always returns TRUE.");

    [Fact]
    public void CanExecuteWithInvalidParameterReturnsTrueForNonValidatingCommand()
      => _ = this.AsyncNonValidatingTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeTrue("command was created without defining a CanExecute delegate. Therefore the default is used which always returns TRUE.");

    [Fact]
    public void InvalidCommandParameterReturnsCanExecuteFalseForSynchronousCommand()
      => _ = this.TestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeFalse("command parameter is invalid.");

    [Fact]
    public void ValidCommandParameterReturnsCanExecuteTrueForAsyncCommand()
      => _ = this.AsyncTestCommand.CanExecute(this.ValidCommandParameter)
        .Should().BeTrue("command parameter is invalid.");

    [Fact]
    public void ValidCommandParameterReturnsCanExecuteTrueForSynchronousCommand()
      => _ = this.TestCommand.CanExecute(this.ValidCommandParameter)
        .Should().BeTrue("command parameter is valid.");

    // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20

    [Fact]
    public void InvalidateCommandMustRaiseCanExecuteChangedForSynchronousCommand()
    {
      //using IMonitor<IAsyncRelayCommand<string>> eventMonitor = this.TestCommand.Monitor();
      //this.TestCommand.InvalidateCommand();
      //_ = eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
    }

    // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20

    [Fact]
    public void InvalidateCommandMustRaiseCanExecuteChangedForAsynchronousCommand()
    {
      //using IMonitor<IAsyncRelayCommand<string>> eventMonitor = this.AsyncTestCommand.Monitor();
      //this.AsyncTestCommand.InvalidateCommand();
      //_ = eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
    }

    [Fact]
    public async Task ExecutingAsynchronousCommand_IsExecutingMustBeTrue()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter));
      await WaitForExecutionStartedAsync(1);

      _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeTrue();
    }

    [Fact]
    public async Task CancelledAsynchronousCommand_IsExecutingMustBeFalse()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        try
        {
          await this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }

        _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
      }
    }

    [Fact]
    public async Task CancelAsynchronousCommand_CancellationToken_IsCancelledMustBeTrue()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        try
        {
          await this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
        }

        _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
      }
    }

    [Fact]
    public async Task CancelAsynchronousCommand_Timeout_IsCancelledMustBeTrue()
    {
      try
      {
        await this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, this.Timeout);
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task CancelAsynchronousCommand_CancelMethodCall_IsCancelledMustBeTrue()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter));
      await WaitForExecutionStartedAsync(1);

      this.AsyncCancellableTestCommand.Cancel();
      await WaitForExecutionCompletedAsync();

      _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutingAsynchronousNonCancellableCommand_WithCancellationTokenAndCancel_IsCancelledMustBeFalse()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        _ = Task.Run(() => this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token));

        await WaitForCancellationAsync(cancellationTokenSource.Token);
        Assert.True(cancellationTokenSource.IsCancellationRequested);

        _ = this.AsyncTestCommand.IsCancelled.Should().BeFalse();
      }
    }

    [Fact]
    public void ExecutingAsynchronousCancellableCommand_IsCancelledMustBeFalse()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, CancellationToken.None));

      _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public async Task CancelledAsynchronousCommand_ExecuteAgain_IsCancelledMustBeFalse()
    {
      using (var cancellationTokenSource1 = new CancellationTokenSource(this.Timeout))
      {
        try
        {
          await this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource1.Token);
        }
        catch (OperationCanceledException)
        {
        }

        Assert.True(this.AsyncCancellableTestCommand.IsCancelled);
      }

      ResetWaitForExecution();
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, CancellationToken.None));
      await WaitForExecutionStartedAsync();

      _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsynchronousCommand_UsingCommandCancel_IsExecutingMustBeFalse()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter));
      await WaitForExecutionStartedAsync();

      this.AsyncCancellableTestCommand.Cancel();
      await WaitForExecutionCompletedAsync();

      _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsynchronousCommand_UsingCommandCancelAll_IsExecutingMustBeFalse()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter));
      await WaitForExecutionStartedAsync();

      _ = this.AsyncCancellableTestCommand.CancelAll();
      await WaitForExecutionCompletedAsync();

      _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAsynchronousCommand_CancelUsingTimeout_IsExecutingMustBeFalse()
    {
      await this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, this.Timeout);

      _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_MustExecuteBothCommands()
    {
      Task task1 = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);

      await Task.WhenAll(task1, task2);

      _ = task1.Status.Should().Be(TaskStatus.RanToCompletion);
      _ = task2.Status.Should().Be(TaskStatus.RanToCompletion);
      _ = this.executedCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_CallingCancelAll_MustCancelBoth()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, CancellationToken.None);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, CancellationToken.None);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        bool hasCancelledActions = this.AsyncCancellableTestCommandWithExecutionCount.CancelAll();
        await WaitForExecutionCompletedAsync();
        Assert.True(hasCancelledActions);
      }
      catch (OperationCanceledException)
      {
      }

      _ = task1.Status.Should().Be(TaskStatus.Canceled);
      _ = task2.Status.Should().Be(TaskStatus.Canceled);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_CancelAllWithTimeout_MustCancelBoth()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout, CancellationToken.None);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout, CancellationToken.None);
      
      try
      {
        await Task.WhenAll(task1, task2);
      }
      catch (OperationCanceledException)
      {
      }

      _ = task1.Status.Should().Be(TaskStatus.Canceled);
      _ = task2.Status.Should().Be(TaskStatus.Canceled);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_MustQueueSecondCommand_IsExecutingMustBeTrue()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
      _ = this.AsyncCancellableTestCommandWithExecutionCount.IsExecuting.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_MustQueueSecondCommand_PendingCountMustBeOne()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
      _ = this.AsyncCancellableTestCommandWithExecutionCount.PendingCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_CancelFirst_MustExecuteSecond()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        this.AsyncCancellableTestCommandWithExecutionCount.Cancel();
        await WaitForExecutionCompletedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(2);
    } 

  [Fact]
  public async Task ExecutingCommandTwoTimes_CancelFirstWithTimeout_MustExecuteSecond()
  {
    Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout);
    Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
    Func<Task> tasks = () => Task.WhenAll(task1, task2);

    try
    {
      _ = Task.Run(tasks);
      await WaitForExecutionStartedAsync();
      this.AsyncCancellableTestCommandWithExecutionCount.Cancel();
      await WaitForExecutionCompletedAsync();
    }
    catch (OperationCanceledException)
    {
    }

    _ = this.executedCount.Should().Be(2);
  }

  [Fact]
    public async Task ExecutingCommandTwoTimes_CancelSecondPending_MustExecuteOnlyFirst()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        _ = this.AsyncCancellableTestCommandWithExecutionCount.CancelPending();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
      _ = task2.Status.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_CancelSecondPending_CancelPendingMustReturnTrue()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);
      bool hasCancelledPending = false;

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        hasCancelledPending = this.AsyncCancellableTestCommand.CancelPending();
      }
      catch (OperationCanceledException)
      {
      }

      _ = hasCancelledPending.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_CancelSecondPending_IsCancelledMustBeFalse()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        _ = this.AsyncCancellableTestCommand.CancelPending();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondAndCancellingTheFirstThenHasPendingMustBeFalse()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        this.AsyncCancellableTestCommand.Cancel();
        await WaitForExecutionCompletedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.HasPending.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondCallSoThatPendingCountMustBe_1()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.PendingCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecond_CallCancelOnTheFirst_ThenPendingCountMustBe_0()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
        this.AsyncCancellableTestCommand.Cancel();
        await WaitForExecutionCompletedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.PendingCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimes_HasPendingMustReturnTrue()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await WaitForExecutionStartedAsync();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.HasPending.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutingNoParamCancellationTokenCommand_MustExecuteOnce()
    {
      Task executeTask = this.TestNoParamCommandWithExecutionCount.ExecuteAsync(CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingCancellationTokenCommand_MustExecuteOnce()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.TestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);

        try
        {
          await executeTask;
        }
        catch (OperationCanceledException)
        {
        }

        _ = this.executedCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingAsyncCancellationTokenCommand_MustExecuteOnce()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);

        try
        {
          await executeTask;
        }
        catch (OperationCanceledException)
        {
        }

        _ = this.executedCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenCommand_MustExecuteOnce()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.AsyncTestNoParamCommandWithExecutionCount.ExecuteAsync(cancellationTokenSource.Token);

        try
        {
          await executeTask;
        }
        catch (OperationCanceledException)
        {
        }

        _ = this.executedCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingNoParamCancellationTokenAndTimeoutCommand_MustExecuteOnce()
    {
      Task executeTask = this.TestNoParamCommandWithExecutionCount.ExecuteAsync(this.Timeout, CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingCancellationTokenAndTimeoutCommand_MustExecuteOnce()
    {
      Task executeTask = this.TestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout, CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncCancellationTokeAndTimeoutCommand_MustExecuteOnce()
    {
      Task executeTask = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout, CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenAndTimeoutCommand_MustExecuteOnce()
    {
      Task executeTask = this.AsyncTestNoParamCommandWithExecutionCount.ExecuteAsync(this.Timeout, CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executedCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingNoParamCommand_WithCancellationToken_MustBeCancelled()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.CancellableTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token);

        try
        {
          await executeTask;
        }
        catch (OperationCanceledException)
        {
        }

        _ = executeTask.Status.Should().Be(TaskStatus.Canceled);
      }
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCommand_WithCancellationToken_MustBeCancelled()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.AsyncCancellableTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token);

        try
        {
          await executeTask;
        }
        catch (OperationCanceledException)
        {
        }

        _ = executeTask.Status.Should().Be(TaskStatus.Canceled);
      }
    }

    [Fact]
    public async Task ExecutingCommand_CancelWithCancellationToken_MustThrow()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Func<Task> executeTask = () => this.CancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);

        _ = await executeTask.Should().ThrowAsync<OperationCanceledException>();
      }
    }

    [Fact]
    public async Task ExecutingAsyncCommand_CancelWithCancellationToken_MustThrow()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Func<Task> executeTask = () => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);

        _ = await executeTask.Should().ThrowAsync<OperationCanceledException>();
      }
    }

    [Fact]
    public async Task ExecutingAsyncCommand_CastToICommandToExecuteSynchronouslyAndCancelWithCancellationToken_MustThrow()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Action executeTask = () => ((ICommand)this.AsyncCancellableTestCommand).Execute(this.ValidCommandParameter);

        _ = executeTask.Should().Throw<OperationCanceledException>();
      }
    }

    //[Fact]
    //public async Task ExecutingNoParamCommandWithTimeoutMustBeCancelledOnTimeoutExpired()
    //{
    //  using (var cancellationTokenSource = new CancellationTokenSource())
    //  {
    //    Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
    //    ProfilerBatchResult profilerBatchResult = await Profiler.LogTimeAsync(() => this.AsyncCancellableTestNoParamCommand.ExecuteAsync(this.Timeout), 1, logger);
    //    ProfilerResult profilerResult = profilerBatchResult.Results.First();
    //    _ = profilerResult.ProfiledTask.Status.Should().Be(TaskStatus.Canceled);
    //    _ = profilerResult.ElapsedTime.Value.Should().BeGreaterThanOrEqualTo(this.Timeout.TotalMilliseconds * System.Math.Pow(10, 3));
    //    _ = profilerResult.ElapsedTime.Should().BeLessThanOrEqualTo(this.LongRunningAsyncDelay);
    //  }
    //}

    //[Fact]
    //public async Task ExecutingAsyncNoParamCommandWithCancellationTokenMustBeCancelled()
    //{
    //  using var cancellationTokenSource = new CancellationTokenSource();
    //  Task task = this.CancellableAsyncTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
    //  .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
    //  Task.Run(async () => await task);
    //  await Task.Delay(10);
    //  cancellationTokenSource.Cancel();
    //}

    //[Fact]
    //public async Task ExecutingCommandWithCancellationTokenMustBeCancelled()
    //{
    //  using var cancellationTokenSource = new CancellationTokenSource();
    //  Task task = this.CancellableTestCommand.ExecuteAsync(cancellationTokenSource.Token)
    //  .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
    //  Task.Run(async () => await task);
    //  await Task.Delay(10);
    //  cancellationTokenSource.Cancel();
    //}

    //[Fact]
    //public async Task ExecutingAsyncCommandWithCancellationTokenMustBeCancelled()
    //{
    //  using var cancellationTokenSource = new CancellationTokenSource();
    //  Task task = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
    //  .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
    //  Task.Run(async () => await task);
    //  await Task.Delay(10);
    //  cancellationTokenSource.Cancel();
    //}
  }
}
