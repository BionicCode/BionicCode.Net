namespace BionicCode.Utilities.UnitTest.Net
{
  using BionicCode.Utilities.UnitTest.Net.Resources;
  using System;
  using System.Linq;
  using Xunit;
  using FluentAssertions;
  using System.ComponentModel;
  using BionicCode.Utilities.Net;
  using System.Collections.Generic;
  using System.Collections;
  using FluentAssertions.Events;
  using System.Threading.Tasks;
  using System.Threading;
  using System.IO;

  public class AsyncCommandTest : IDisposable
  {
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

    private bool CanExecuteTestCommand(string commandParameter) => commandParameter?.StartsWith("@") ?? false;

    private bool CanExecuteTestNoParamCommand() => true;
    private void ExecuteTestCommand(string commandParameter)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteTestNoParamCommand()
    {
      Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private async Task ExecuteTestCommandAsync(string commandParameter)
    {
      if (commandParameter == null)
      { 
        throw new ArgumentNullException(nameof(commandParameter)); 
      }

      Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteTestNoParamCommandAsync()
    {
      Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
    }

    private async Task ExecuteCancellableTestCommandAsync(string commandParameter, CancellationToken cancellationToken)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private async Task ExecuteCancellableTestNoParamCommandAsync(CancellationToken cancellationToken)
    {
      Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.LongRunningAsyncDelay, cancellationToken);
    }

    private void ExecuteCancellableTestCommand(string commandParameter, CancellationToken cancellationToken)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private void ExecuteCancellableTestNoParamCommand(CancellationToken cancellationToken)
    {
      Interlocked.Increment(ref this.executionCount);
      Thread.Sleep(this.AsyncDelay);
    }

    private async Task ExecuteThrowingTestCommandAsync(string commandParameter)
    {
      if (commandParameter == null)
      {
        throw new ArgumentNullException(nameof(commandParameter));
      }

      Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    private async Task ExecuteThrowingTestNoParamCommandAsync()
    {
      Interlocked.Increment(ref this.executionCount);
      await Task.Delay(this.AsyncDelay);
      throw new InvalidOperationException("From async test method.");
    }

    [Fact]
    public async Task AwaitSynchronousCommand()
    {
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(() => this.TestCommand.ExecuteAsync(this.ValidCommandParameter), 1);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitSynchronousNoParamCommand()
    {
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(this.TestNoParamCommand.ExecuteAsync, 1);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitAsynchronousCommand()
    {
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(() => this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter), 1);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitAsynchronousNoParamCommand()
    {
      ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(this.AsyncTestNoParamCommand.ExecuteAsync, 1);
      TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
      exeutionTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.AsyncDelay.Milliseconds);
    }

    [Fact]
    public async Task AwaitAsynchronousCommandThrowsExceptionInCallerContext()
    {
      await this.ThrowingAsyncTestCommand.Awaiting(command => command.ExecuteAsync(this.ValidCommandParameter))
        .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");
    }

    [Fact]
    public async Task AwaitAsynchronousNoParamCommandThrowsExceptionInCallerContext()
    {
      await this.ThrowingAsyncTestNoParamCommand.Awaiting(command => command.ExecuteAsync())
        .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");
    }

    [Fact]
    public void CanExecuteWithInvalidParameterReturnsFalse()
    {
      this.AsyncTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeFalse("command parameter is invalid.");
    }

    [Fact]
    public void ParameterlessCanExecuteReturnsResultOfRegisteredCanExecuteHandler()
    {
      this.AsyncTestCommand.CanExecute()
        .Should().Be(CanExecuteTestCommand(null), "command was created with a CanExecute delgate, but is invoked using the parameterles CanEWxecute().");
    }

    [Fact]
    public void ParameterlessCanExecuteReturnsTrueForNonValidatingCommand()
    {
      this.NonValidatingAsyncTestCommand.CanExecute()
        .Should().BeTrue("command was created withou defining a CanExecute delegate.");
    }

    [Fact]
    public void CanExecuteWithInvalidParameterReturnsTrueForNonValidatingCommand()
    {
      this.NonValidatingAsyncTestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeTrue("command was created withou defining a CanExecute delegate.");
    }

    [Fact]
    public void InvalidCommandParameterReturnsCanExecuteFalseForSynchronousCommand()
    {
      this.TestCommand.CanExecute(this.InvalidCommandParameter)
        .Should().BeFalse("command parameter is invalid.");
    }

    [Fact]
    public void ValidCommandParameterReturnsCanExecuteTrueForAsyncCommand()
    {
      this.AsyncTestCommand.CanExecute(this.ValidCommandParameter)
        .Should().BeTrue("command parameter is invalid.");
    }

    [Fact]
    public void ValidCommandParameterReturnsCanExecuteTrueForSynchronousCommand()
    {
      this.TestCommand.CanExecute(this.ValidCommandParameter)
        .Should().BeTrue("command parameter is valid.");
    }

    [Fact]
    public void InvalidateCommandMustRaiseCanExecuteChangedForSynchronousCommand()
    {
      using IMonitor<IAsyncRelayCommand<string>> eventMonitor = this.TestCommand.Monitor();
      this.TestCommand.InvalidateCommand();
      eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
    }

    [Fact]
    public void InvalidateCommandMustRaiseCanExecuteChangedForAsynchronousCommand()
    {
      using IMonitor<IAsyncRelayCommand<string>> eventMonitor = this.AsyncTestCommand.Monitor();
      this.AsyncTestCommand.InvalidateCommand();
      eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
    }

    [Fact]
    public async Task IsExecutingMustBeTrueForExecutingAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.CancellableAsyncTestCommand.IsExecuting.Should().BeTrue()));
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.CancellableAsyncTestCommand.IsExecuting.Should().BeFalse()));
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task IsCancelledMustBeTrueForCancelledAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.CancellableAsyncTestCommand.IsCancelled.Should().BeTrue()));
      cancellationTokenSource.Cancel();
      await Task.Delay(10);
    }

    [Fact]
    public void IsCancelledMustBeFalseForNonCancelledAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task.Run(async () => await this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => this.AsyncTestCommand.IsCancelled.Should().BeFalse()));
    }

    [Fact]
    public void IsCancelledMustBeFalseBeforeCancellingAsynchronousCommand()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token));
      this.CancellableAsyncTestCommand.IsCancelled.Should().BeFalse();
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public void IsCancelledMustBeFalseAfterCancelledCommandIsExecutedAgain()
    {
      using var cancellationTokenSource1 = new CancellationTokenSource();
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource1.Token));
      cancellationTokenSource1.Cancel();

      using var cancellationTokenSource2 = new CancellationTokenSource();
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource2.Token));
      this.CancellableAsyncTestCommand.IsCancelled.Should().BeFalse();
      cancellationTokenSource2.Cancel();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommandUsingCommandCancelExecutingMethod()
    {
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter));
      this.CancellableAsyncTestCommand.CancelExecuting();
      this.CancellableAsyncTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public void IsExecutingMustBeFalseForCancelledAsynchronousCommandUsingCommandCancelAllMethod()
    {
      Task.Run(async () => await this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter));
      this.CancellableAsyncTestCommand.CancelAll();
      this.CancellableAsyncTestCommand.IsExecuting.Should().BeFalse();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustExecuteBothCommands()
    {
      Task task1 = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          task1.Status.Should().Be(TaskStatus.RanToCompletion);
          task2.Status.Should().Be(TaskStatus.RanToCompletion);
          this.executionCount.Should().Be(2);
        });
      Task.Run(async () => await tasks);
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
          task1.Status.Should().Be(TaskStatus.Canceled);
          task2.Status.Should().Be(TaskStatus.Canceled);
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await tasks);

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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await tasks);

      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesAndCancelFirstMustExecuteSecond()
    {
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          task.Status.Should().Be(TaskStatus.Canceled);
          task2.Status.Should().Be(TaskStatus.WaitingForActivation);
          this.CancellableAsyncTestCommand.IsExecuting.Should().BeTrue();
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(2);
        });
      Task.Run(async () => await tasks);

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
          task.Status.Should().Be(TaskStatus.Canceled);
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task tasks = Task.WhenAll(task1, task2)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await tasks);

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
          this.CancellableAsyncTestCommand.IsPendingCancelled.Should().BeTrue();
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task tasks = Task.WhenAll(task1, task2);
      Task.Run(async () => await tasks);

      await Task.Delay(10);

      this.CancellableAsyncTestCommand.CancelPending();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondAndCancellingTheFirstThenHasPendingMustBeFalse()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          this.CancellableAsyncTestCommand.HasPending.Should().BeFalse();
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task tasks = Task.WhenAll(task1, task2);
      Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.CancelExecuting();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondCallSoThatPendingCountMustBe_1()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task tasks = Task.WhenAll(task1, task2);
      Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.PendingCount.Should().Be(1);
      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondAndCancellingTheFirstThenPendingCountMustBe_0()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          this.CancellableAsyncTestCommand.PendingCount.Should().Be(0);
          this.CancellableAsyncTestCommand.CancelAll();
        });
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task tasks = Task.WhenAll(task1, task2);
      Task.Run(async () => await tasks);

      await Task.Delay(10);
      this.CancellableAsyncTestCommand.CancelExecuting();
    }

    [Fact]
    public async Task ExecutingCommandTwoTimesHasPendingMustReturnTrue()
    {
      Task task1 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task task2 = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter);
      Task tasks = Task.WhenAll(task1, task2);
      Task.Run(async () => await tasks);
      await Task.Delay(10);

      this.CancellableAsyncTestCommand.HasPending.Should().BeTrue();
      this.CancellableAsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingNoParamCommandMustExecuteOnce()
    {
      Task task = this.TestNoParamCommand.ExecuteAsync()
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync()
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingCommandMustExecuteOnce()
    {
      Task task = this.TestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingNoParamTimeoutCommandMustExecuteOnce()
    {
      Task task = this.TestNoParamCommand.ExecuteAsync(TimeSpan.Zero)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestNoParamCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingTimeoutCommandMustExecuteOnce()
    {
      Task task = this.TestCommand.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.TestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncTimeoutCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, TimeSpan.Zero)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
      await Task.Delay(10);
      this.AsyncTestCommand.CancelAll();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamTimeoutCommandMustExecuteOnce()
    {
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync(TimeSpan.Zero)
        .ContinueWith(task =>
        {
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
          this.executionCount.Should().Be(1);
        });
      Task.Run(async () => await task);
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
        this.executionCount.Should().Be(1);
      });
      Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCancellationTokenAndTimeoutCommandMustExecuteOnce()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.AsyncTestNoParamCommand.ExecuteAsync(TimeSpan.Zero,  cancellationTokenSource.Token)
        .ContinueWith(task =>
      {
        this.executionCount.Should().Be(1);
      });
      Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingNoParamCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncNoParamCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableAsyncTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableTestCommand.ExecuteAsync(cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingAsyncCommandWithCancellationTokenMustBeCancelled()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      Task task = this.CancellableAsyncTestCommand.ExecuteAsync(this.ValidCommandParameter, cancellationTokenSource.Token)
        .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
      Task.Run(async () => await task);
      await Task.Delay(10);
      cancellationTokenSource.Cancel();
    }

    [Fact]
    public async Task ExecutingNoParamCommandWithTimeoutMustBeCancelledOnTimeoutExpired()
    {
      using var cancellationTokenSource = new CancellationTokenSource();
      ProfilerBatchResult profilerBatchResult = await Profiler.LogTimeAsync(() => this.CancellableAsyncTestNoParamCommand.ExecuteAsync(this.Timeout), 1, (result, summary) =>
      {
        File.WriteAllText("profiler_summary.log", summary);
      });

      ProfilerResult profilerResult = profilerBatchResult.Results.First();
      profilerResult.ProfiledTask.Status.Should().Be(TaskStatus.Canceled);
      profilerResult.ElapsedTime.Milliseconds.Should().BeGreaterThanOrEqualTo(this.Timeout.Milliseconds);
      profilerResult.ElapsedTime.Should().BeLessThan(this.LongRunningAsyncDelay);
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
