namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Diagnostics;
  using System.IO;
  using System.Linq;
  using System.Runtime.CompilerServices;
  using System.Threading;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using FluentAssertions.Events;
  using Xunit;

  public class AsyncCommandTest : IDisposable
  {
    private static bool IsProfilerLoggingEnabled { get; } = true;

    private IAsyncRelayCommand<string> TestCommand { get; }
    private IAsyncRelayCommand TestNoParamCommand { get; }
    private IAsyncRelayCommand<string> AsyncTestCommand { get; }
    private IAsyncRelayCommand AsyncTestNoParamCommand { get; }
    private IAsyncRelayCommand<string> NonValidatingAsyncTestCommand { get; }
    private IAsyncRelayCommand<string> ThrowingAsyncTestCommand { get; }
    private IAsyncRelayCommand ThrowingAsyncTestNoParamCommand { get; }
    private IAsyncRelayCommand<string> CancellableAsyncTestCommand { get; }
    private IAsyncRelayCommand<string> CancellableTestCommand { get; }
    private IAsyncRelayCommand CancellableAsyncTestNoParamCommand { get; }
    private IAsyncRelayCommand CancellableTestNoParamCommand { get; }
    private TimeSpan Timeout { get; }
    private TimeSpan AsyncDelay { get; }
    private TimeSpan LongRunningAsyncDelay { get; }
    private string InvalidCommandParameter => "Some invalid command parameter";
    private string ValidCommandParameter => "@Some valid command parameter";
    private int executionCount;

    public AsyncCommandTest()
    {
      this.Timeout = TimeSpan.FromMilliseconds(10);
      this.AsyncDelay = TimeSpan.FromMilliseconds(15);
      this.LongRunningAsyncDelay = TimeSpan.FromSeconds(15);
      this.TestCommand = new AsyncRelayCommand<string>(ExecuteTestCommand, CanExecuteTestCommand);
      this.TestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommand, CanExecuteTestNoParamCommand);
      this.AsyncTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync, CanExecuteTestCommand);
      this.AsyncTestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.NonValidatingAsyncTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync);
      this.ThrowingAsyncTestCommand = new AsyncRelayCommand<string>(ExecuteThrowingTestCommandAsync, CanExecuteTestCommand);
      this.ThrowingAsyncTestNoParamCommand = new AsyncRelayCommand(ExecuteThrowingTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.CancellableAsyncTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommandAsync, CanExecuteTestCommand);
      this.CancellableAsyncTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
      this.CancellableTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommand, CanExecuteTestCommand);
      this.CancellableTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommand, CanExecuteTestNoParamCommand);
    }

    public void Dispose()
    {
      this.NonValidatingAsyncTestCommand.CancelAll();
      this.CancellableAsyncTestCommand.CancelAll();
      this.ThrowingAsyncTestCommand.CancelAll();
      this.AsyncTestCommand.CancelAll();
      this.TestCommand.CancelAll();
      this.CancellableAsyncTestNoParamCommand.CancelAll();
      this.ThrowingAsyncTestNoParamCommand.CancelAll();
      this.AsyncTestNoParamCommand.CancelAll();
      this.TestNoParamCommand.CancelAll();
      this.CancellableTestCommand.CancelAll();
      this.CancellableTestNoParamCommand.CancelAll();
    }

    private static string CrateProfilerSummary(string summary, string currentMethodName)
      => $"UnitTest profiler summary: {currentMethodName}{Environment.NewLine}{summary}";

    private static ProfilerLoggerDelegate CreateProfilerLogger([CallerMemberName] string currentMethodName = "unknown")
      => UnitTestHelper.IsDebugModeEnabled && IsProfilerLoggingEnabled
      ? (result, summary) =>
        {
          File.WriteAllText("profiler_summary.log", CrateProfilerSummary(summary, currentMethodName));
          Debug.WriteLine(CrateProfilerSummary(summary, currentMethodName), "profiling");
        }
    : (result, summary) => { };

    private bool CanExecuteTestCommand(string commandParameter) => commandParameter?.StartsWith("@") ?? false;

    private bool CanExecuteTestNoParamCommand() => true;
    private void ExecuteTestCommand(string commandParameter)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      _ = Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteTestNoParamCommand()
    {
      _ = Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private async Task ExecuteTestCommandAsync(string commandParameter)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteTestNoParamCommandAsync()
    {
      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteCancellableTestCommandAsync(string commandParameter, CancellationToken cancellationToken)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private async Task ExecuteCancellableTestNoParamCommandAsync(CancellationToken cancellationToken)
    {
      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private void ExecuteCancellableTestCommand(string commandParameter, CancellationToken cancellationToken)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      _ = Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteCancellableTestNoParamCommand(CancellationToken cancellationToken)
    {
      _ = Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private async Task ExecuteThrowingTestCommandAsync(string commandParameter)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    private async Task ExecuteThrowingTestNoParamCommandAsync()
    {
      _ = Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    [Fact]
    public async Task AwaitSynchronousCommand()
    {
      ProfilerLoggerDelegate logger = CreateProfilerLogger();
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(async () => await this.TestCommand.ExecuteAsync(this.ValidCommandParameter), 1, logger);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      _ = exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitSynchronousNoParamCommand()
    {
      ProfilerLoggerDelegate logger = CreateProfilerLogger();
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(this.TestNoParamCommand.ExecuteAsync, 1, logger);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      _ = exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitAsynchronousCommand()
    {
      ProfilerLoggerDelegate logger = CreateProfilerLogger();
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(async () => await this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter), 1, logger);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      _ = exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitAsynchronousNoParamCommand()
    {
      ProfilerLoggerDelegate logger = CreateProfilerLogger();
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(this.AsyncTestNoParamCommand.ExecuteAsync, 1, logger);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      _ = exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitAsynchronousCommandThrowsExceptionInCallerContext()
      => _ = await this.ThrowingAsyncTestCommand.Awaiting(command => command.ExecuteAsync(this.ValidCommandParameter))
        .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");

    [Fact]
    public async Task AwaitAsynchronousNoParamCommandThrowsExceptionInCallerContext()
      => _ = await this.ThrowingAsyncTestNoParamCommand.Awaiting(command => command.ExecuteAsync())
        .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");

    [Fact]
    public void CanExecuteWithInvalidParameterReturnsFalse()
      => _ = this.AsyncTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeFalse("command parameter is invalid.");

    [Fact]
    public void ParameterlessCanExecuteReturnsResultOfRegisteredCanExecuteHandler()
      => _ = this.AsyncTestCommand.CanExecute()
        .Should().Be(CanExecuteTestCommand(null), "command was created with a CanExecute delgate, but is invoked using the parameterles CanEWxecute().");

    [Fact]
    public void ParameterlessCanExecuteReturnsTrueForNonValidatingCommand()
      => _ = this.NonValidatingAsyncTestCommand.CanExecute()
        .Should().BeTrue("command was created withou defining a CanExecute delegate.");

    [Fact]
    public void CanExecuteWithInvalidParameterReturnsTrueForNonValidatingCommand()
      => _ = this.NonValidatingAsyncTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeTrue("command was created withou defining a CanExecute delegate.");

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

    [Fact]
    public void InvalidateCommandMustRaiseCanExecuteChangedForSynchronousCommand()
    {
      using IMonitor<IAsyncRelayCommand<string>> eventMonitor = this.TestCommand.Monitor();
      this.TestCommand.InvalidateCommand();
      _ = eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
    }

    [Fact]
    public void InvalidateCommandMustRaiseCanExecuteChangedForAsynchronousCommand()
    {
      using IMonitor<IAsyncRelayCommand<string>> eventMonitor = this.AsyncTestCommand.Monitor();
      this.AsyncTestCommand.InvalidateCommand();
      _ = eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
    }

    [Fact]
    public async Task IsExecutingMustBeTrueForExecutingAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.CancellableAsyncTestCommand.IsExecuting.Should().BeTrue()));
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.CancellableAsyncTestCommand.IsExecuting.Should().BeFalse()));
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task IsCancelledMustBeTrueForCancelledAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.CancellableAsyncTestCommand.IsCancelled.Should().BeTrue()));
      cancellationTokenSource.Cancel();
      await Task.Delay(10);
    }

    [Fact]
    public void IsCancelledMustBeFalseForNonCancelledAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      _ = Task.Run(async () => await this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.AsyncTestCommand.IsCancelled.Should().BeFalse()));
    }

    [Fact]
    public void IsCancelledMustBeFalseBeforeCancellingAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token));
      _ = this.CancellableAsyncTestCommand.IsCancelled.Should().BeFalse();
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public void IsCancelledMustBeFalseAfterCancelledCommandIsExecutedAgain()
    {
      using var cancellationTokenSource1 = new CancellationTokenSource();
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource1.Token));
      cancellationTokenSource1.Cancel();

      using var cancellationTokenSource2 = new CancellationTokenSource();
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource2.Token));
      _ = this.CancellableAsyncTestCommand.IsCancelled.Should().BeFalse();
      cancellationTokenSource2.Cancel();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommandUsingCommandCancelExecutingMethod()
    {
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter));
      this.CancellableAsyncTestCommand.CancelExecuting();
      _ = this.CancellableAsyncTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommandUsingCommandCancelAllMethod()
    {
      _ = Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter));
      this.CancellableAsyncTestCommand.CancelAll();
      _ = this.CancellableAsyncTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustExecuteBothCommands()
    {
      Task task1 = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      await Task.WhenAll(task1, task2);
      _ = task1.Status.Should().Be(TaskStatus.RanToCompletion);
      _ = task2.Status.Should().Be(TaskStatus.RanToCompletion);
      _ = this.executionCount.Should().Be(2);
      //Task tasks = Task.WhenAll(task1, task2)
      //  .ContinueWith(task =>
      //  {
      //    _ = task1.Status.Should().Be(TaskStatus.RanToCompletion);
      //    _ = task2.Status.Should().Be(TaskStatus.RanToCompletion);
      //    _ = this.executionCount.Should().Be(2);
      //  });
      //_ = Task.Run(async () => await tasks);
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCallingCancelAllMustCancelBoth()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token);
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          _ = task1.Status.Should().Be(TaskStatus.Canceled);
          _ = task2.Status.Should().Be(TaskStatus.Canceled);
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustQueueCommandsAndSecondTaskMustWait()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await tasks);

      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelFirstMustExecuteSecond()
    {
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = task.Status.Should().Be(TaskStatus.Canceled);
          _ = task2.Status.Should().Be(TaskStatus.WaitingForActivation);
          _ = this.CancellableAsyncTestCommand.IsExecuting.Should().BeTrue();
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(2);
        });
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.CancelExecuting();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelSecondPendingMustExecuteOnlyFirst()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = task.Status.Should().Be(TaskStatus.Canceled);
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);

      this.CancellableAsyncTestCommand.CancelPending();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelSecondPendingThenIsPendingCancelledMustBeTrue()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = this.CancellableAsyncTestCommand.IsPendingCancelled.Should().BeTrue();
          this.CancellableAsyncTestCommand.CancelAll();
        });
      var tasks = Task.WhenAll(task1, task2);
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);

      this.CancellableAsyncTestCommand.CancelPending();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondAndCancellingTheFirstThenHasPendingMustBeFalse()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = this.CancellableAsyncTestCommand.HasPending.Should().BeFalse();
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      var tasks = Task.WhenAll(task1, task2);
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.CancelExecuting();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondCallSoThatPendingCountMustBe_1()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      var tasks = Task.WhenAll(task1, task2);
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);
      _ = this.CancellableAsyncTestCommand.PendingCount.Should().Be(1);
      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondAndCancellingTheFirstThenPendingCountMustBe_0()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = this.CancellableAsyncTestCommand.PendingCount.Should().Be(0);
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      var tasks = Task.WhenAll(task1, task2);
      _ = Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.CancelExecuting();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesHasPendingMustReturnTrue()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      var tasks = Task.WhenAll(task1, task2);
      _ = Task.Run(async () => await tasks);
      await Task.Delay(10);

      _ = this.CancellableAsyncTestCommand.HasPending.Should().BeTrue();
      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingNoParamCommandMustExecuteOnce()
    {
      Task task = this.TestNoParamCommand.ExecuteAsync()
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync()
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandMustExecuteOnce()
    {
      Task task = this.TestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingNoParamTimeoutCommandMustExecuteOnce()
    {
      Task task = this.TestNoParamCommand.ExecuteAsync(TimeSpan.Zero)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingTimeoutCommandMustExecuteOnce()
    {
      Task task = this.TestCommand.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncTimeoutCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamTimeoutCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync(TimeSpan.Zero)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingNoParamCancellationTokenCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.TestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingCancellationTokenCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.TestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncCancellationTokenCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingNoParamCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.TestNoParamCommand.ExecuteAsync(TimeSpan.Zero, cancellationTokenSource.Token)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.TestCommand.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero, cancellationTokenSource.Token)
        .ContinueWith(task =>
        {
          _ = this.executionCount.Should().Be(1);
        });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncCancellationTokeAndTimeoutCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero, cancellationTokenSource.Token)
        .ContinueWith(task =>
      {
        _ = this.executionCount.Should().Be(1);
      });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync(TimeSpan.Zero, cancellationTokenSource.Token)
        .ContinueWith(task =>
      {
        _ = this.executionCount.Should().Be(1);
      });
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingNoParamCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableAsyncTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableTestCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      _ = Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingNoParamCommandWithTimeoutMustBeCancelledOnTimeoutExpired()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      ProfilerLoggerDelegate logger = CreateProfilerLogger();
      ProfilerBatchResult profilerBatchResult = await Profiler.LogTimeAsync(() => this.CancellableAsyncTestNoParamCommand.ExecuteAsync(this.Timeout), 1, logger);
      ProfilerResult profilerResult = profilerBatchResult.Results.First();
      _ = profilerResult.ProfiledTask.Status.Should().Be(TaskStatus.Canceled);
      _ = profilerResult.ElapsedTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.Timeout.Milliseconds);
      _ = profilerResult.ElapsedTime.Should().BeLessThan(this.LongRunningAsyncDelay);
    }

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
