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
  public class AsyncCommandTest : IDisposable
  {
    private static bool IsProfilerLoggingEnabled { get; } = true;

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

    private IAsyncRelayCommand<string> TestCommandWithExecutionCount { get; }
    private IAsyncRelayCommand TestNoParamCommandWithExecutionCount { get; }
    private IAsyncRelayCommand<string> AsyncTestCommandWithExecutionCount { get; }
    private IAsyncRelayCommand AsyncTestNoParamCommandWithExecutionCount { get; }
    private IAsyncRelayCommand<string> AsyncNonValidatingTestCommandWithExecutionCount { get; }
    private IAsyncRelayCommand<string> AsyncThrowingTestCommandWithExecutionCount { get; }
    private IAsyncRelayCommand AsyncThrowingTestNoParamCommandWithExecutionCount { get; }
    private IAsyncRelayCommand<string> AsyncCancellableTestCommandWithExecutionCount { get; }
    private IAsyncRelayCommand<string> CancellableTestCommandWithExecutionCount { get; }
    private IAsyncRelayCommand AsyncCancellableTestNoParamCommandWithExecutionCount { get; }
    private IAsyncRelayCommand CancellableTestNoParamCommandWithExecutionCount { get; }
    private TimeSpan Timeout { get; }
    private TimeSpan AsyncDelay { get; }
    private TimeSpan LongRunningAsyncDelay { get; }
    private string InvalidCommandParameter => "Some invalid command parameter";
    private string ValidCommandParameter => "@Some valid command parameter";
    private int executionCount;

    public AsyncCommandTest()
    {
      this.Timeout = TimeSpan.FromMilliseconds(0.1);
      this.AsyncDelay = TimeSpan.FromMilliseconds(0.1);
      this.LongRunningAsyncDelay = TimeSpan.FromSeconds(10);
      this.TestCommand = new AsyncRelayCommand<string>(ExecuteTestCommand, CanExecuteTestCommand);
      this.TestCommandWithExecutionCount = new AsyncRelayCommand<string>(ExecuteTestCommandWithExecutionCount, CanExecuteTestCommand);
      this.TestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommand, CanExecuteTestNoParamCommand);
      this.TestNoParamCommandWithExecutionCount = new AsyncRelayCommand(ExecuteTestNoParamCommandWithExecutionCount, CanExecuteTestNoParamCommand);
      this.AsyncTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync, CanExecuteTestCommand);
      this.AsyncTestCommandWithExecutionCount = new AsyncRelayCommand<string>(ExecuteTestCommandWithExecutionCountAsync, CanExecuteTestCommand);
      this.AsyncTestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.AsyncTestNoParamCommandWithExecutionCount = new AsyncRelayCommand(ExecuteTestNoParamCommandWithExecutionCountAsync, CanExecuteTestNoParamCommand);
      this.AsyncNonValidatingTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync);
      this.AsyncNonValidatingTestCommandWithExecutionCount = new AsyncRelayCommand<string>(ExecuteTestCommandWithExecutionCountAsync);
      this.AsyncThrowingTestCommand = new AsyncRelayCommand<string>(ExecuteThrowingTestCommandAsync, CanExecuteTestCommand);
      this.AsyncThrowingTestCommandWithExecutionCount = new AsyncRelayCommand<string>(ExecuteThrowingTestCommandWithExecutionCountAsync, CanExecuteTestCommand);
      this.AsyncThrowingTestNoParamCommand = new AsyncRelayCommand(ExecuteThrowingTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.AsyncThrowingTestNoParamCommandWithExecutionCount = new AsyncRelayCommand(ExecuteThrowingTestNoParamCommandWithExecutionCountAsync, CanExecuteTestNoParamCommand);
      this.AsyncCancellableTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommandAsync, CanExecuteTestCommand);
      this.AsyncCancellableTestCommandWithExecutionCount = new AsyncRelayCommand<string>(ExecuteCancellableTestCommandAsync, CanExecuteTestCommand);
      this.AsyncCancellableTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.AsyncCancellableTestNoParamCommandWithExecutionCount = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.CancellableTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommand, CanExecuteTestCommand);
      this.CancellableTestCommandWithExecutionCount = new AsyncRelayCommand<string>(ExecuteCancellableTestCommandWithExecutionCount, CanExecuteTestCommand);
      this.CancellableTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommand, CanExecuteTestNoParamCommand);
      this.CancellableTestNoParamCommandWithExecutionCount = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommandWithExecutionCount, CanExecuteTestNoParamCommand);
    }

    public void Dispose()
    {
      this.AsyncNonValidatingTestCommand.CancelAll();
      this.AsyncCancellableTestCommand.CancelAll();
      this.AsyncThrowingTestCommand.CancelAll();
      this.AsyncTestCommand.CancelAll();
      this.TestCommand.CancelAll();
      this.AsyncCancellableTestNoParamCommand.CancelAll();
      this.AsyncThrowingTestNoParamCommand.CancelAll();
      this.AsyncTestNoParamCommand.CancelAll();
      this.TestNoParamCommand.CancelAll();
      this.CancellableTestCommand.CancelAll();
      this.CancellableTestNoParamCommand.CancelAll();
      this.AsyncNonValidatingTestCommandWithExecutionCount.CancelAll();
      this.AsyncCancellableTestCommandWithExecutionCount.CancelAll();
      this.AsyncThrowingTestCommandWithExecutionCount.CancelAll();
      this.AsyncTestCommandWithExecutionCount.CancelAll();
      this.TestCommandWithExecutionCount.CancelAll();
      this.AsyncCancellableTestNoParamCommandWithExecutionCount.CancelAll();
      this.AsyncThrowingTestNoParamCommandWithExecutionCount.CancelAll();
      this.AsyncTestNoParamCommandWithExecutionCount.CancelAll();
      this.TestNoParamCommandWithExecutionCount.CancelAll();
      this.CancellableTestCommandWithExecutionCount.CancelAll();
      this.CancellableTestNoParamCommandWithExecutionCount.CancelAll();
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

      _ = Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteTestNoParamCommand() => Thread.Sleep(this.AsyncDelay);

    private void ExecuteTestNoParamCommandWithExecutionCount()
    {
      _ = Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private async Task ExecuteTestCommandAsync(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteTestCommandWithExecutionCountAsync(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteTestNoParamCommandAsync() => await Task.Delay(this.AsyncDelay);

    private async Task ExecuteTestNoParamCommandWithExecutionCountAsync()
    {
      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteCancellableTestCommandAsync(string commandParameter, CancellationToken cancellationToken)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private async Task ExecuteCancellableTestCommandWithExecutionCountAsync(string commandParameter, CancellationToken cancellationToken)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private async Task ExecuteCancellableTestNoParamCommandAsync(CancellationToken cancellationToken)
      => await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);

    private async Task ExecuteCancellableTestNoParamCommandWithExecutionCountAsync(CancellationToken cancellationToken)
    {
      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

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

    private void ExecuteCancellableTestCommandWithExecutionCount(string commandParameter, CancellationToken cancellationToken)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      _ = Interlocked.Increment(ref this.executionCount);

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

    private void ExecuteCancellableTestNoParamCommandWithExecutionCount(CancellationToken cancellationToken)
    {
      _ = Interlocked.Increment(ref this.executionCount);

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

    private async Task ExecuteThrowingTestCommandWithExecutionCountAsync(string commandParameter)
    {
      ArgumentNullExceptionEx.ThrowIfNull(commandParameter, nameof(commandParameter));

      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    private async Task ExecuteThrowingTestNoParamCommandAsync()
    {
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    private async Task ExecuteThrowingTestNoParamCommandWithExecutionCountAsync()
    {
      _ = Interlocked.Increment(ref this.executionCount);
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
    public async Task IsExecutingMustBeTrueForExecutingAsynchronousCommand()
    {
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
          .ContinueWith(task => this.AsyncCancellableTestCommand.IsExecuting.Should().BeTrue()));
        await Task.Delay(TimeSpan.FromMilliseconds(0.005));
        cancellationTokenSource.Cancel();
      }
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommand()
    {
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
          .ContinueWith(task => this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse()));
        cancellationTokenSource.Cancel();
      }
    }

    [Fact]
    public async Task IsCancelledMustBeTrueForCancelledAsynchronousCommand()
    {
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
          .ContinueWith(task => this.AsyncCancellableTestCommand.IsCancelled.Should().BeTrue()));
        cancellationTokenSource.Cancel();
        await Task.Delay(TimeSpan.FromMilliseconds(0.005));
      }
    }

    [Fact]
    public void IsCancelledMustBeFalseForNonCancelledAsynchronousCommand()
    {
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
          .ContinueWith(task => this.AsyncTestCommand.IsCancelled.Should().BeFalse()));
      }
    }

    [Fact]
    public void IsCancelledMustBeFalseBeforeCancellingAsynchronousCommand()
    {
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token));
        _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
        cancellationTokenSource.Cancel();
      }
    }

    [Fact]
    public void IsCancelledMustBeFalseAfterCancelledCommandIsExecutedAgain()
    {
      using (var cancellationTokenSource1 = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource1.Token));
        cancellationTokenSource1.Cancel();
      }

      using (var cancellationTokenSource2 = new CancellationTokenSource())
      {
        _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource2.Token));
        _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
        cancellationTokenSource2.Cancel();
      }
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommandUsingCommandCancelExecutingMethod()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter));
      this.AsyncCancellableTestCommand.Cancel();
      _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommandUsingCommandCancelAllMethod()
    {
      _ = Task.Run(() => this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter));
      this.AsyncCancellableTestCommand.CancelAll();
      _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustExecuteBothCommands()
    {
      Task task1 = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      await Task.WhenAll(task1, task2);
      _ = task1.Status.Should().Be(TaskStatus.RanToCompletion);
      _ = task2.Status.Should().Be(TaskStatus.RanToCompletion);
      _ = this.executionCount.Should().Be(2);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCallingCancelAllMustCancelBoth()
    {
      using (var cancellationTokenSource = new CancellationTokenSource())
      {
        Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);
        Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);
        Func<Task> tasks = () => Task.WhenAll(task1, task2)
          .ContinueWith(task =>
          {
            _ = task1.Status.Should().Be(TaskStatus.Canceled);
            _ = task2.Status.Should().Be(TaskStatus.Canceled);
            _ = this.executionCount.Should().Be(1);
          });
        _ = Task.Run(tasks);

        await Task.Delay(TimeSpan.FromMilliseconds(0.005));
        this.AsyncCancellableTestCommandWithExecutionCount.CancelAll();
      }
    }

    [Fact]
    public void ExecutingCommandTwoTimesMustQueueCommandsAndSecondTaskMustWait()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(tasks);

      this.AsyncCancellableTestCommandWithExecutionCount.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelFirstMustExecuteSecond()
    {
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = task.Status.Should().Be(TaskStatus.Canceled);
          _ = task2.Status.Should().Be(TaskStatus.WaitingForActivation);
          _ = this.AsyncCancellableTestCommand.IsExecuting.Should().BeTrue();
          this.AsyncCancellableTestCommandWithExecutionCount.CancelAll();
        });
      Func<Task> tasks = () => Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(2);
        });
      _ = Task.Run(tasks);

      await Task.Delay(TimeSpan.FromMilliseconds(0.005));
      this.AsyncCancellableTestCommandWithExecutionCount.Cancel();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelSecondPendingMustExecuteOnlyFirst()
    {
      Task task1 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        await Task.Delay(TimeSpan.FromMilliseconds(5));
        this.AsyncCancellableTestCommandWithExecutionCount.CancelPending();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
      _ = task2.Status.Should().Be(TaskStatus.WaitingForActivation);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelSecondPendingThenIsCancelledMustBeTrue()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
        this.AsyncCancellableTestCommand.CancelPending();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
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
        this.AsyncCancellableTestCommand.Cancel();
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
        this.AsyncCancellableTestCommand.Cancel();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.AsyncCancellableTestCommand.PendingCount.Should().Be(0);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesHasPendingMustReturnTrue()
    {
      Task task1 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Func<Task> tasks = () => Task.WhenAll(task1, task2);

      try
      {
        _ = Task.Run(tasks);
      }
      catch (OperationCanceledException)
      {
      }
      
      _ = this.AsyncCancellableTestCommand.HasPending.Should().BeTrue();
    }

    [Fact]
    public async Task ExecutingNoParamCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = this.TestNoParamCommandWithExecutionCount.ExecuteAsync;

      try
      {
        _ = Task.Run(executeTask);
        this.TestNoParamCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = this.AsyncTestNoParamCommandWithExecutionCount.ExecuteAsync;

      try
      {
        _ = Task.Run(executeTask);
        this.AsyncTestNoParamCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = () => this.TestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);

      try
      {
        _ = Task.Run(executeTask);
        this.TestCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = () => this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter);

      try
      {
        _ = Task.Run(executeTask);
        this.AsyncTestCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }
    }

    [Fact]
    public async Task ExecutingNoParamTimeoutCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = () => this.TestNoParamCommandWithExecutionCount.ExecuteAsync(TimeSpan.Zero);

      try
      {
        _ = Task.Run(executeTask);
        this.TestNoParamCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingTimeoutCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = () => this.TestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero);

      try
      {
        _ = Task.Run(executeTask);
        this.TestCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }
    }

    [Fact]
    public async Task ExecutingAsyncTimeoutCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = () => this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, System.Threading.Timeout.InfiniteTimeSpan);

      try
      {
        _ = Task.Run(executeTask);
        this.AsyncTestCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }
      
      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncNoParamTimeoutCommand_CallCancel_MustExecuteOnce()
    {
      Func<Task> executeTask = () =>  this.AsyncTestNoParamCommandWithExecutionCount.ExecuteAsync(System.Threading.Timeout.InfiniteTimeSpan);

      try
      { 
      _ = Task.Run(executeTask);
        this.AsyncTestNoParamCommandWithExecutionCount.Cancel();
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingNoParamCancellationTokenCommandMustExecuteOnce()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.TestNoParamCommandWithExecutionCount.ExecuteAsync(cancellationTokenSource.Token);

        try
        {
          await executeTask;
        }
        catch (OperationCanceledException)
        {
        }
        
        _ = this.executionCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingCancellationTokenCommandMustExecuteOnce()
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

        _ = this.executionCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingAsyncCancellationTokenCommandMustExecuteOnce()
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
          
        _ = this.executionCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenCommandMustExecuteOnce()
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

        _ = this.executionCount.Should().Be(1);
      }
    }

    [Fact]
    public async Task ExecutingNoParamCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
       Task executeTask = this.TestNoParamCommandWithExecutionCount.ExecuteAsync(this.Timeout, CancellationToken.None);

      try
      { 
       await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
      Task executeTask = this.TestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout, CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncCancellationTokeAndTimeoutCommandMustExecuteOnce()
    {
      Task executeTask = this.AsyncTestCommandWithExecutionCount.ExecuteAsync(this.ValidCommandParameter, this.Timeout, CancellationToken.None);

      try
      { 
      await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

      _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
      Task executeTask = this.AsyncTestNoParamCommandWithExecutionCount.ExecuteAsync(this.Timeout, CancellationToken.None);

      try
      {
        await executeTask;
      }
      catch (OperationCanceledException)
      {
      }

  _ = this.executionCount.Should().Be(1);
    }

    [Fact]
    public async Task ExecutingNoParamCommandWithCancellationTokenMustBeCancelled()
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
    public async Task ExecutingAsyncNoParamCommandWithCancellationTokenMustBeCancelled()
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
    public async Task ExecutingCommandWithCancellationTokenMustBeCancelled()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Func<Task> executeTask = () => this.CancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);

        _ = await executeTask.Should().ThrowAsync<OperationCanceledException>();
      }
    }

    [Fact]
    public async Task ExecutingAsyncCommandWithCancellationTokenMustBeCancelled()
    {
      using (var cancellationTokenSource = new CancellationTokenSource(this.Timeout))
      {
        Task executeTask = this.AsyncCancellableTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);

        await executeTask;

        _ = executeTask.Status.Should().Be(TaskStatus.Canceled);
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
