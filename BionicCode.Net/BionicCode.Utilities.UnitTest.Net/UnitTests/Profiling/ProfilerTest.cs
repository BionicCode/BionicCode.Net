﻿namespace BionicCode.Utilities.Net.UnitTest.ProfilerTests
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using System.Reflection;
  using System.Threading;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using BionicCode.Utilities.Net.UnitTest.Resources;
  using FluentAssertions;
  using Xunit;

  public class ProfilerTest
  {
    private readonly Milliseconds ShortDuration = TimeSpan.FromMilliseconds(1);
    private readonly Milliseconds NoDuration = TimeSpan.Zero;

    private ProfilerBatchResult ProfilerRunResult { get; set; }
    private string ProfilerRunSummary { get; set; }

    private Func<Task> CreateAsyncTimeConsumingOperation(TimeSpan duration) => async () => await Task.Delay(duration).ConfigureAwait(false);
    private Func<Task> CreateAsyncNoTimeConsumingOperation() => async () => await Task.Delay(this.NoDuration).ConfigureAwait(false);
    private Action CreateTimeConsumingOperation(TimeSpan duration) => () =>
    {
      //Thread.Sleep(duration);
      while (SpinWait.SpinUntil(() => false, duration))
      {
      }
    };

    private Action CreateNoTimeConsumingOperation() => () => { };

    private Action<ProfilerBatchResult, string> CreateLogger() => LogProfiling;
    private Func<ProfilerBatchResult, string, Task> CreateAsyncLogger() => LogProfilingAsync;

    private void LogProfiling(ProfilerBatchResult result, string summary)
    {
      this.ProfilerRunResult = result;
      this.ProfilerRunSummary = result.Summary;

      Debug.WriteLine(result.Summary);
    }

    private Task LogProfilingAsync(ProfilerBatchResult result, string summary)
    {
      this.ProfilerRunResult = result;
      this.ProfilerRunSummary = result.Summary;

      Debug.WriteLine(result.Summary);

      return Task.CompletedTask;
    }

    [Fact]
    public async Task LogTimeScoped_ExecutionTime_MustBeEqualToMeasuredTime()
    {
      Func<Task> operationToProfile = CreateAsyncTimeConsumingOperation(this.ShortDuration);
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(out result))
      {
        await operationToProfile.Invoke();
      }

      _ = result.TotalDuration.Value.Should().BeGreaterThanOrEqualTo(this.ShortDuration.ToMicroseconds());
    }

    [Fact]
    public void LogTimeScoped_NoLogger_LoggerResultSummaryMustNotBeEmpty()
    {
      Action operationToProfile = CreateNoTimeConsumingOperation();
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(out result))
      {
        operationToProfile.Invoke();
      }

      _ = result.Summary.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LogTimeScoped_ExecutionTimeWithLogger_MustBeEqualToMeasuredTime()
    {
      Action<ProfilerBatchResult, string> logger = CreateLogger();
      Func<Task> operationToProfile = CreateAsyncTimeConsumingOperation(this.ShortDuration);
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(logger, out result))
      {
        await operationToProfile.Invoke();
      }

      _ = result.TotalDuration.Value.Should().BeGreaterThanOrEqualTo(this.ShortDuration.ToMicroseconds());
    }

    [Fact]
    public void LogTimeScoped_Logger_LoggerResultMustBeEqualToReturnedProfilerResult()
    {
      Action<ProfilerBatchResult, string> logger = CreateLogger();
      Action operationToProfile = CreateNoTimeConsumingOperation();
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(logger, out result))
      {
        operationToProfile.Invoke();
      }

      _ = result.Should().BeSameAs(this.ProfilerRunResult);
    }

    [Fact]
    public void LogTimeScoped_Logger_LoggerResultSummaryMustBeEqualToReturnedProfilerResultSummary()
    {
      Action<ProfilerBatchResult, string> logger = CreateLogger();
      Action operationToProfile = CreateNoTimeConsumingOperation();
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(logger, out result))
      {
        operationToProfile.Invoke();
      }

      _ = result.Summary.Should().BeEquivalentTo(this.ProfilerRunResult.Summary);
    }

    [Fact]
    public void LogTimeScoped_Logger_LoggerResultSummaryMustNotBeEmpty()
    {
      Action<ProfilerBatchResult, string> logger = CreateLogger();
      Action operationToProfile = CreateNoTimeConsumingOperation();
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(logger, out result))
      {
        operationToProfile.Invoke();
      }

      _ = result.Summary.Should().NotBeEmpty();
    }

    //[Fact]
    //public async Task LogTimeAsync_ExecutionTimeWithLogger_MustBeEqualToMeasuredTime()
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateLogger();
    //  Func<Task> operationToProfile = CreateAsyncTimeConsumingOperation(this.ShortDuration);

    //  ProfilerBatchResult result = await Profiler.LogTimeAsync(operationToProfile, 10, logger);

    //  _ = result.TotalDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    //}

    //[Fact]
    //public void LogTime_ExecutionTimeWithLogger_MustBeEqualToMeasuredTime()
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateLogger();
    //  Action operationToProfile = CreateTimeConsumingOperation(this.ShortDuration);

    //  ProfilerBatchResult result = Profiler.LogTime(operationToProfile, 1, logger);

    //  _ = result.TotalDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    //}

    //[Theory]
    //[InlineData(1)]
    //[InlineData(5)]
    //public async Task LogTimeAsync_IterationsWithLogger_IterationCountMustBeEqualToReference(int iterationCount)
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateLogger();
    //  Func<Task> operationToProfile = CreateAsyncNoTimeConsumingOperation();

    //  ProfilerBatchResult result = await Profiler.LogTimeAsync(operationToProfile, iterationCount, logger);

    //  _ = result.IterationCount.Should().Be(iterationCount);
    //}

    //[Theory]
    //[InlineData(1)]
    //[InlineData(5)]
    //public void LogTime_IterationsWithLogger_IterationCountMustBeEqualToReference(int iterationCount)
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateLogger();
    //  Action operationToProfile = CreateNoTimeConsumingOperation();

    //  ProfilerBatchResult result = Profiler.LogTime(operationToProfile, iterationCount, logger);

    //  _ = result.IterationCount.Should().Be(iterationCount);
    //}

    //[Theory]
    //[InlineData(1)]
    //[InlineData(5)]
    //public void LogTime_AverageTimeWithLogger_MustBeEqualToReference(int iterationCount)
    //{
    //  Action<ProfilerBatchResult, string> logger = CreateLogger();
    //  Action operationToProfile = CreateTimeConsumingOperation(this.ShortDuration);

    //  ProfilerBatchResult result = Profiler.LogTime(operationToProfile, iterationCount, logger);

    //  _ = result.AverageDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    //}

    [Fact]
    public async Task LogTime_Attributes_MustBeEqualToReference()
    {
      Action<ProfilerBatchResult, string> logger = CreateLogger();
      const int iterations = 20;
      var targetTypes = new List<Type>
      {
        typeof(BenchmarkTarget<string>),
        //typeof(BenchmarkTargetAlternate<int>)
      };

      //ProfilerBuilder profilerBuilder = Profiler.CreateProfilerBuilder(targetTypes)
      ProfilerBuilder profilerBuilder = Profiler.CreateProfilerBuilder()
        //.AddAutoDiscoverAssembly(Assembly.GetExecutingAssembly())
        .SetIterations(iterations)
        //.SetRuntime(Runtime.Net8_0)
        .SetLogger((result, summary) => Debug.WriteLine(result.Summary))
        .EnableDefaultLogOutput()
        .SetBaseUnit(TimeUnit.Auto);

      ProfiledTypeResultCollection profilerResult = await profilerBuilder.RunAsync(CancellationToken.None);

      _ = profilerResult.First().First().First().IterationCount.Should().Be(iterations);
    }
  }
}