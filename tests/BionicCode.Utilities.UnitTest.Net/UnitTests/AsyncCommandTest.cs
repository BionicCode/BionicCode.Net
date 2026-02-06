// Ignore Spelling: Cancelled Cancellable

namespace BionicCode.Utilities.Net.UnitTest
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using BionicCode.Utilities.Net;
    using BionicCode.Utilities.Net.Profiling;
    using FluentAssertions;
    using Xunit;

    public class AsyncCommandTest : IDisposable
    {
        private static bool IsProfilerLoggingEnabled { get; } = true;
        private TaskCompletionSource executedTaskCompletionSource;
        private TaskCompletionSource executingTaskCompletionSource;
        private readonly TaskCompletionSource cancellationTokenTaskCompletionSource;
        private readonly TaskCompletionSource pendingTaskCompletionSource;
        [Obsolete]
        private readonly EventAggregator eventAggregator;
        private readonly List<string> eventNamesToObserve;

        private IAsyncRelayCommand<string> AsyncTestCommand { get; }
        private IAsyncRelayCommand AsyncTestNoParamCommand { get; }
        private IAsyncRelayCommand<string> AsyncNonValidatingTestCommand { get; }
        private IAsyncRelayCommand<string> AsyncThrowingTestCommand { get; }
        private IAsyncRelayCommand AsyncThrowingTestNoParamCommand { get; }
        private IAsyncRelayCommand<string> AsyncCancellableTestCommand { get; }
        private IAsyncRelayCommand AsyncCancellableTestNoParamCommand { get; }

        private TimeSpan Timeout { get; }
        private TimeSpan AsyncDelay { get; }
        private TimeSpan LongRunningAsyncDelay { get; }
        private string InvalidCommandParameter => "Some invalid command parameter";
        private string ValidCommandParameter => "@Some valid command parameter";
        private long totalCommandsCompletedCount;
        private long commandsCompletedCount;
        private long totalCommandsStartedCount;
        private long commandsStartedCount;
        private long commandsToAwaitCompeletedCount;
        private long commandsToAwaitStartedCount;
        private int pendingCount;
        private long commandsToAwaitPendingCount;

        [Obsolete]
        public AsyncCommandTest()
        {
            Timeout = TimeSpan.FromMilliseconds(10);
            AsyncDelay = TimeSpan.FromMilliseconds(0.1);
            LongRunningAsyncDelay = TimeSpan.FromSeconds(10);
            executingTaskCompletionSource = new TaskCompletionSource();
            executedTaskCompletionSource = new TaskCompletionSource();
            pendingTaskCompletionSource = new TaskCompletionSource();
            cancellationTokenTaskCompletionSource = new TaskCompletionSource();

            eventAggregator = new EventAggregator();
            bool isRegistered = eventAggregator.TryRegisterObserver(nameof(IAsyncRelayCommandCore.Executing), typeof(AsyncRelayCommandCore), OnCommandExecuting);
            Assert.True(isRegistered);
            isRegistered = eventAggregator.TryRegisterObserver(nameof(IAsyncRelayCommandCore.Executed), typeof(AsyncRelayCommandCore), OnCommandExecuted);
            Assert.True(isRegistered);
            isRegistered = eventAggregator.TryRegisterObserver(nameof(INotifyPropertyChanged.PropertyChanged), typeof(AsyncRelayCommandCore), OnCommandPropertyChanged);
            Assert.True(isRegistered);

            eventNamesToObserve = new List<string> { nameof(IAsyncRelayCommandCore.Executing), nameof(IAsyncRelayCommandCore.Executed), nameof(IAsyncRelayCommandCore.PropertyChanged) };

            AsyncTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync, CanExecuteTestCommand);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncTestCommand, eventNamesToObserve);
            Assert.True(isRegistered);

            AsyncTestNoParamCommand = new AsyncRelayCommand(ExecuteTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncTestNoParamCommand, eventNamesToObserve);
            Assert.True(isRegistered);

            AsyncNonValidatingTestCommand = new AsyncRelayCommand<string>(ExecuteTestCommandAsync);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncNonValidatingTestCommand, eventNamesToObserve);
            Assert.True(isRegistered);

            AsyncThrowingTestCommand = new AsyncRelayCommand<string>(ExecuteThrowingTestCommandAsync, CanExecuteTestCommand);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncThrowingTestCommand, eventNamesToObserve);
            Assert.True(isRegistered);

            AsyncThrowingTestNoParamCommand = new AsyncRelayCommand(ExecuteThrowingTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncThrowingTestNoParamCommand, eventNamesToObserve);
            Assert.True(isRegistered);

            AsyncCancellableTestCommand = new AsyncRelayCommand<string>(ExecuteCancellableTestCommandAsync, CanExecuteTestCommand);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncCancellableTestCommand, eventNamesToObserve);
            Assert.True(isRegistered);

            AsyncCancellableTestNoParamCommand = new AsyncRelayCommand(ExecuteCancellableTestNoParamCommandAsync, CanExecuteTestNoParamCommand);
            isRegistered = eventAggregator.TryRegisterObservable(AsyncCancellableTestNoParamCommand, eventNamesToObserve);
            Assert.True(isRegistered);
        }

        [Obsolete]
        public void Dispose()
        {
            _ = AsyncNonValidatingTestCommand.CancelAll();
            _ = AsyncCancellableTestCommand.CancelAll();
            _ = AsyncThrowingTestCommand.CancelAll();
            _ = AsyncTestCommand.CancelAll();
            _ = AsyncCancellableTestNoParamCommand.CancelAll();
            _ = AsyncThrowingTestNoParamCommand.CancelAll();
            _ = AsyncTestNoParamCommand.CancelAll();
            bool isAllUnregistered = eventAggregator.TryRemoveObservable(typeof(IAsyncRelayCommandCore), eventNamesToObserve, removeEventObservers: true);
            Assert.True(isAllUnregistered);
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
            using CancellationTokenRegistration registration = cancellationToken.Register(cancellationTokenTaskCompletionSource.SetResult);
            await cancellationTokenTaskCompletionSource?.Task;
        }

        private async Task WaitForExecutionStartedAsync(int count)
        {
            commandsToAwaitStartedCount = count;
            long startedCount = Interlocked.Read(ref commandsStartedCount);
            if (commandsToAwaitStartedCount == startedCount)
            {
                executingTaskCompletionSource.SetResult();
            }

            await executingTaskCompletionSource?.Task;
        }

        private async Task WaitForExecutionPendingAsync(int count)
        {
            commandsToAwaitPendingCount = count;
            if (pendingCount == count)
            {
                pendingTaskCompletionSource.SetResult();
            }

            await pendingTaskCompletionSource?.Task;
        }

        private async Task WaitForExecutionCompletedAsync(int count = -1)
        {
            commandsToAwaitCompeletedCount = count;
            long completedCount = Interlocked.Read(ref commandsCompletedCount);
            long startedCount = Interlocked.Read(ref commandsStartedCount);
            if (completedCount == commandsToAwaitCompeletedCount
              || (commandsToAwaitCompeletedCount == -1 && completedCount == startedCount))
            {
                executedTaskCompletionSource.SetResult();
            }

            await executedTaskCompletionSource?.Task;
        }

        private void ResetWaitForExecution()
        {
            commandsToAwaitStartedCount = -1;
            commandsToAwaitCompeletedCount = -1;
            totalCommandsCompletedCount = 0;
            totalCommandsStartedCount = 0;
            commandsStartedCount = 0;
            commandsCompletedCount = 0;
            executingTaskCompletionSource = new TaskCompletionSource();
            executedTaskCompletionSource = new TaskCompletionSource();
        }

        private void OnCommandExecuted(object sender, EventArgs e)
        {
            _ = Interlocked.Increment(ref totalCommandsCompletedCount);
            long completedCount = Interlocked.Increment(ref commandsCompletedCount);
            long startedCount = Interlocked.Read(ref commandsStartedCount);
            if (completedCount == commandsToAwaitCompeletedCount
              || (commandsToAwaitCompeletedCount == -1 && completedCount == startedCount))
            {
                _ = Interlocked.Exchange(ref commandsStartedCount, 0);
                _ = Interlocked.Exchange(ref commandsCompletedCount, 0);
                executedTaskCompletionSource.SetResult();
            }
        }

        private void OnCommandExecuting(object sender, EventArgs e)
        {
            _ = Interlocked.Increment(ref totalCommandsStartedCount);
            long startedCount = Interlocked.Increment(ref commandsStartedCount);
            if (startedCount == commandsToAwaitStartedCount)
            {
                executingTaskCompletionSource.SetResult();
            }
        }

        private void OnCommandPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAsyncRelayCommandCore.PendingCount):
                    pendingCount = ((IAsyncRelayCommandCore)sender).PendingCount;
                    if (pendingCount == commandsToAwaitPendingCount)
                    {
                        pendingTaskCompletionSource.SetResult();
                    }

                    break;
            }
        }

        private bool CanExecuteTestCommand(string commandParameter) => commandParameter?.StartsWith("@") ?? false;

        private bool CanExecuteTestNoParamCommand() => true;
        private void ExecuteTestCommand(string commandParameter)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

            Thread.Sleep(AsyncDelay);
        }

        private void ExecuteTestCommandWithExecutionCount(string commandParameter)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

            _ = Interlocked.Increment(ref totalCommandsCompletedCount);
            Thread.Sleep(AsyncDelay);
        }

        private void ExecuteTestNoParamCommand() => Thread.Sleep(AsyncDelay);

        private async Task ExecuteTestCommandAsync(string commandParameter)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

            await Task.Delay(AsyncDelay);
        }

        private async Task ExecuteTestNoParamCommandAsync()
          => await Task.Delay(AsyncDelay);

        private async Task ExecuteCancellableTestCommandAsync(string commandParameter, CancellationToken cancellationToken)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

            await Task.Delay(LongRunningAsyncDelay, cancellationToken);
        }

        private async Task ExecuteCancellableTestNoParamCommandAsync(CancellationToken cancellationToken)
          => await Task.Delay(LongRunningAsyncDelay, cancellationToken);

        private void ExecuteCancellableTestCommand(string commandParameter, CancellationToken cancellationToken)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

            var spinWait = new SpinWait();
            while (!cancellationToken.IsCancellationRequested)
            {
                spinWait.SpinOnce();
            }

            cancellationToken.ThrowIfCancellationRequested();
        }

        private void ExecuteCancellableTestCommandWithoutThrowingCancellationException(string commandParameter, CancellationToken cancellationToken)
        {
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

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
            ArgumentNullExceptionAdvanced.ThrowIfNull(commandParameter, nameof(commandParameter));

            await Task.Delay(AsyncDelay);
            throw new InvalidOperationException("From async test method.");
        }

        private async Task ExecuteThrowingTestNoParamCommandAsync()
        {
            await Task.Delay(AsyncDelay);
            throw new InvalidOperationException("From async test method.");
        }

        #endregion Non test members

        //[Fact]
        //public async Task AwaitSynchronousCommand()
        //{
        //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
        //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(() => TestCommand.ExecuteAsync(ValidCommandParameter), 1, logger);
        //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
        //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(AsyncDelay.TotalMilliseconds);
        //}

        //[Fact]
        //public async Task AwaitSynchronousNoParamCommand()
        //{
        //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
        //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(TestNoParamCommand.ExecuteAsync, 1, logger);
        //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
        //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(AsyncDelay.TotalMilliseconds);
        //}

        //[Fact]
        //public async Task AwaitAsynchronousCommand()
        //{
        //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
        //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(() => AsyncTestCommand.ExecuteAsync(ValidCommandParameter), 1, logger);
        //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
        //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(AsyncDelay.TotalMilliseconds);
        //}

        //[Fact]
        //public async Task AwaitAsynchronousNoParamCommand()
        //{
        //  Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
        //  ProfilerBatchResult profilerResult = await Profiler.LogTimeAsync(AsyncTestNoParamCommand.ExecuteAsync, 1, logger);
        //  TimeSpan exeutionTime = profilerResult.Results.First().ElapsedTime;
        //  _ = exeutionTime.TotalMilliseconds.Should().BeGreaterThanOrEqualTo(AsyncDelay.TotalMilliseconds);
        //}

        [Fact]
        public async Task AwaitAsynchronousCommand_ExecuteAsychronous_ThrowsExceptionInCallerContext()
          => _ = await AsyncThrowingTestCommand.Awaiting(command => command.ExecuteAsync(ValidCommandParameter))
            .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");

        [Fact]
        public async Task AwaitAsynchronousCommand_ExecuteSynchronous_ThrowsExceptionInCallerContext()
          => _ = AsyncThrowingTestCommand.Invoking(command => command.Execute(ValidCommandParameter))
            .Should().ThrowExactly<InvalidOperationException>("exception is propagated outside of async context.");

        [Fact]
        public async Task AwaitAsynchronousNoParamCommandThrowsExceptionInCallerContext()
          => _ = await AsyncThrowingTestNoParamCommand.Awaiting(command => command.ExecuteAsync())
            .Should().ThrowExactlyAsync<InvalidOperationException>("exception is propagated outside of async context.");

        [Fact]
        public void CanExecuteWithInvalidParameterReturnsFalse()
          => _ = AsyncTestCommand.CanExecute(InvalidCommandParameter)
            .Should().BeFalse("command parameter is invalid.");

        [Fact]
        public void ParameterlessCanExecuteReturnsTrueForNonValidatingCommand()
          => _ = AsyncNonValidatingTestCommand.CanExecute(InvalidCommandParameter)
            .Should().BeTrue("command was created without defining a CanExecute delegate. Therefore the default is used which always returns TRUE.");

        [Fact]
        public void CanExecuteWithInvalidParameterReturnsTrueForNonValidatingCommand()
          => _ = AsyncNonValidatingTestCommand.CanExecute(InvalidCommandParameter)
            .Should().BeTrue("command was created without defining a CanExecute delegate. Therefore the default is used which always returns TRUE.");

        [Fact]
        public void ValidCommandParameterReturnsCanExecuteTrueForAsyncCommand()
          => _ = AsyncTestCommand.CanExecute(ValidCommandParameter)
            .Should().BeTrue("command parameter is invalid.");

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20

        [Fact]
        public void InvalidateCommandMustRaiseCanExecuteChangedForSynchronousCommand()
        {
            //using IMonitor<IAsyncRelayCommand<string>> eventMonitor = TestCommand.Monitor();
            //TestCommand.InvalidateCommand();
            //_ = eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
        }

        // TODO::Track events manually as FLuentAssertions feature is not available for .NetStandard 20

        [Fact]
        public void InvalidateCommandMustRaiseCanExecuteChangedForAsynchronousCommand()
        {
            //using IMonitor<IAsyncRelayCommand<string>> eventMonitor = AsyncTestCommand.Monitor();
            //AsyncTestCommand.InvalidateCommand();
            //_ = eventMonitor.Should().Raise(nameof(IAsyncRelayCommand.CanExecuteChanged));
        }

        [Fact]
        public async Task ExecutingAsynchronousCommand_IsExecutingMustBeTrue()
        {
            _ = Task.Run(() => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter));
            await WaitForExecutionStartedAsync(1);

            _ = AsyncCancellableTestCommand.IsExecuting.Should().BeTrue();
        }

        [Fact]
        public async Task CancelledAsynchronousCommand_IsExecutingMustBeFalse()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            try
            {
                await AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
        }

        [Fact]
        public async Task CancelAsynchronousCommand_CancellationToken_IsCancelledMustBeTrue()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            try
            {
                await AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
        }

        [Fact]
        public async Task CancelAsynchronousCommand_Timeout_IsCancelledMustBeTrue()
        {
            try
            {
                await AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, Timeout);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
        }

        [Fact]
        public async Task CancelAsynchronousCommand_CancelMethodCall_IsCancelledMustBeTrue()
        {
            _ = Task.Run(() => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter));
            await WaitForExecutionStartedAsync(1);

            AsyncCancellableTestCommand.Cancel();
            await WaitForExecutionCompletedAsync();

            _ = AsyncCancellableTestCommand.IsCancelled.Should().BeTrue();
        }

        [Fact]
        public async Task ExecutingAsynchronousNonCancellableCommand_WithCancellationTokenAndCancel_IsCancelledMustBeFalse()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            _ = Task.Run(() => AsyncTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource.Token));

            await WaitForCancellationAsync(cancellationTokenSource.Token);
            Assert.True(cancellationTokenSource.IsCancellationRequested);

            _ = AsyncTestCommand.IsCancelled.Should().BeFalse();
        }

        [Fact]
        public void ExecutingAsynchronousCancellableCommand_IsCancelledMustBeFalse()
        {
            _ = Task.Run(() => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, CancellationToken.None));

            _ = AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
        }

        [Fact]
        public async Task CancelledAsynchronousCommand_ExecuteAgain_IsCancelledMustBeFalse()
        {
            using (var cancellationTokenSource1 = new CancellationTokenSource(Timeout))
            {
                try
                {
                    await AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource1.Token);
                }
                catch (OperationCanceledException)
                {
                }

                Assert.True(AsyncCancellableTestCommand.IsCancelled);
            }

            ResetWaitForExecution();
            _ = Task.Run(() => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, CancellationToken.None));
            await WaitForExecutionStartedAsync(1);

            _ = AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
        }

        [Fact]
        public async Task CancelAsynchronousCommand_UsingCommandCancel_IsExecutingMustBeFalse()
        {
            _ = Task.Run(() => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter));
            await WaitForExecutionStartedAsync(1);

            AsyncCancellableTestCommand.Cancel();
            await WaitForExecutionCompletedAsync();

            _ = AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
        }

        [Fact]
        public async Task CancelAsynchronousCommand_UsingCommandCancelAll_IsExecutingMustBeFalse()
        {
            _ = Task.Run(() => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter));
            await WaitForExecutionStartedAsync(1);

            _ = AsyncCancellableTestCommand.CancelAll();
            await WaitForExecutionCompletedAsync();

            _ = AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
        }

        [Fact]
        public async Task CancelAsynchronousCommand_CancelUsingTimeout_IsExecutingMustBeFalse()
        {
            await AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, Timeout);

            _ = AsyncCancellableTestCommand.IsExecuting.Should().BeFalse();
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_MustExecuteBothCommands()
        {
            Task task1 = AsyncTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncTestCommand.ExecuteAsync(ValidCommandParameter);

            await Task.WhenAll(task1, task2);

            _ = task1.Status.Should().Be(TaskStatus.RanToCompletion);
            _ = task2.Status.Should().Be(TaskStatus.RanToCompletion);
            _ = totalCommandsCompletedCount.Should().Be(2);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_CallingCancelAll_MustCancelBoth()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, CancellationToken.None);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, CancellationToken.None);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(2);
                bool hasCancelledActions = AsyncCancellableTestCommand.CancelAll();
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
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, Timeout, CancellationToken.None);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, Timeout, CancellationToken.None);

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
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(0);
            _ = AsyncCancellableTestCommand.IsExecuting.Should().BeTrue();
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_MustQueueSecondCommand_PendingCountMustBeOne()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(1);
                await WaitForExecutionPendingAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.PendingCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_CancelFirst_MustExecuteSecond()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(1);
                AsyncCancellableTestCommand.Cancel();
                await WaitForExecutionCompletedAsync();
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(2);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_CancelFirstWithTimeout_MustExecuteSecond()
        {
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, Timeout);
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(1);
                await WaitForExecutionPendingAsync(1);
                AsyncCancellableTestCommand.Cancel();
                await WaitForExecutionCompletedAsync();
                await WaitForExecutionStartedAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(2);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_CancelSecondPending_MustExecuteOnlyFirst()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionPendingAsync(1);
                _ = AsyncCancellableTestCommand.CancelPending();
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(1);
            _ = task2.Status.Should().Be(TaskStatus.WaitingForActivation);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_CancelSecondPending_CancelPendingMustReturnTrue()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);
            bool hasCancelledPending = false;

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionPendingAsync(1);
                hasCancelledPending = AsyncCancellableTestCommand.CancelPending();
            }
            catch (OperationCanceledException)
            {
            }

            _ = hasCancelledPending.Should().BeTrue();
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_CancelSecondPending_IsCancelledMustBeFalse()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionPendingAsync(1);
                _ = AsyncCancellableTestCommand.CancelPending();
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.IsCancelled.Should().BeFalse();
        }

        [Fact]
        public async Task ExecutingCommandTwoTimesMustEnqueueTheSecond_CancellingTheFirst_HasPendingMustBeFalse()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(1);
                await WaitForExecutionPendingAsync(1);
                AsyncCancellableTestCommand.Cancel();
                await WaitForExecutionCompletedAsync();
                await WaitForExecutionStartedAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.HasPending.Should().BeFalse();
        }

        [Fact]
        public async Task ExecutingCommandTwoTimesMustEnqueueTheSecondCallSoThatPendingCountMustBe_1()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionPendingAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.PendingCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimesMustEnqueueTheSecond_CallCancelOnTheFirst_ThenPendingCountMustBe_0()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionStartedAsync(1);
                await WaitForExecutionPendingAsync(1);
                AsyncCancellableTestCommand.Cancel();
                await WaitForExecutionCompletedAsync();
                await WaitForExecutionStartedAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.PendingCount.Should().Be(0);
        }

        [Fact]
        public async Task ExecutingCommandTwoTimes_HasPendingMustReturnTrue()
        {
            Task task1 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task task2 = AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter);
            Task tasks() => Task.WhenAll(task1, task2);

            try
            {
                _ = Task.Run(tasks);
                await WaitForExecutionPendingAsync(1);
            }
            catch (OperationCanceledException)
            {
            }

            _ = AsyncCancellableTestCommand.HasPending.Should().BeTrue();
        }

        [Fact]
        public async Task ExecutingAsyncCancellationTokenCommand_MustExecuteOnce()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            Task executeTask = AsyncTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource.Token);

            try
            {
                await executeTask;
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecutingAsyncNoParamCancellationTokenCommand_MustExecuteOnce()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            Task executeTask = AsyncTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token);

            try
            {
                await executeTask;
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecutingAsyncCancellationTokeAndTimeoutCommand_MustExecuteOnce()
        {
            Task executeTask = AsyncTestCommand.ExecuteAsync(ValidCommandParameter, Timeout, CancellationToken.None);

            try
            {
                await executeTask;
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecutingAsyncNoParamCancellationTokenAndTimeoutCommand_MustExecuteOnce()
        {
            Task executeTask = AsyncTestNoParamCommand.ExecuteAsync(Timeout, CancellationToken.None);

            try
            {
                await executeTask;
            }
            catch (OperationCanceledException)
            {
            }

            _ = totalCommandsCompletedCount.Should().Be(1);
        }

        [Fact]
        public async Task ExecutingAsyncNoParamCommand_WithCancellationToken_MustBeCancelled()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            Task executeTask = AsyncCancellableTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token);

            try
            {
                await executeTask;
            }
            catch (OperationCanceledException)
            {
            }

            _ = executeTask.Status.Should().Be(TaskStatus.Canceled);
        }

        [Fact]
        public async Task ExecutingAsyncCommand_CancelWithCancellationToken_MustThrow()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            Func<Task> executeTask = () => AsyncCancellableTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource.Token);

            _ = await executeTask.Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task ExecutingAsyncCommand_CastToICommandToExecuteSynchronouslyAndCancelWithCancellationToken_MustThrow()
        {
            using var cancellationTokenSource = new CancellationTokenSource(Timeout);
            Action executeTask = () => ((ICommand)AsyncCancellableTestCommand).Execute(ValidCommandParameter);

            _ = executeTask.Should().Throw<OperationCanceledException>();
        }

        //[Fact]
        //public async Task ExecutingNoParamCommandWithTimeoutMustBeCancelledOnTimeoutExpired()
        //{
        //  using (var cancellationTokenSource = new CancellationTokenSource())
        //  {
        //    Action<ProfilerBatchResult, string> logger = CreateProfilerLogger();
        //    ProfilerBatchResult profilerBatchResult = await Profiler.LogTimeAsync(() => AsyncCancellableTestNoParamCommand.ExecuteAsync(Timeout), 1, logger);
        //    ProfilerResult profilerResult = profilerBatchResult.Results.First();
        //    _ = profilerResult.ProfiledTask.Status.Should().Be(TaskStatus.Canceled);
        //    _ = profilerResult.ElapsedTime.ExecuteDelegate.Should().BeGreaterThanOrEqualTo(Timeout.TotalMilliseconds * System.Math.Pow(10, 3));
        //    _ = profilerResult.ElapsedTime.Should().BeLessThanOrEqualTo(LongRunningAsyncDelay);
        //  }
        //}

        //[Fact]
        //public async Task ExecutingAsyncNoParamCommandWithCancellationTokenMustBeCancelled()
        //{
        //  using var cancellationTokenSource = new CancellationTokenSource();
        //  Task task = CancellableAsyncTestNoParamCommand.ExecuteAsync(cancellationTokenSource.Token)
        //  .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
        //  Task.Run(async () => await task);
        //  await Task.Delay(10);
        //  cancellationTokenSource.Cancel();
        //}

        //[Fact]
        //public async Task ExecutingCommandWithCancellationTokenMustBeCancelled()
        //{
        //  using var cancellationTokenSource = new CancellationTokenSource();
        //  Task task = CancellableTestCommand.ExecuteAsync(cancellationTokenSource.Token)
        //  .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
        //  Task.Run(async () => await task);
        //  await Task.Delay(10);
        //  cancellationTokenSource.Cancel();
        //}

        //[Fact]
        //public async Task ExecutingAsyncCommandWithCancellationTokenMustBeCancelled()
        //{
        //  using var cancellationTokenSource = new CancellationTokenSource();
        //  Task task = CancellableAsyncTestCommand.ExecuteAsync(ValidCommandParameter, cancellationTokenSource.Token)
        //  .ContinueWith(task => task.Status.Should().Be(TaskStatus.Canceled));
        //  Task.Run(async () => await task);
        //  await Task.Delay(10);
        //  cancellationTokenSource.Cancel();
        //}
    }
}
