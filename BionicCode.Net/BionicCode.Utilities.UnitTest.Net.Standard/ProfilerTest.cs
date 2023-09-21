namespace BionicCode.Utilities.Net.UnitTest
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.Threading.Tasks;
  using BionicCode.Utilities.Net;
  using Xunit;
  using FluentAssertions;
  using FluentAssertions.Events;
  using System.Diagnostics;
  using System.Threading;
  using BionicCode.Utilities.Net.UnitTest.Resources;

  public class ProfilerTest
  {
    private readonly TimeSpan ShortDuration = TimeSpan.FromMilliseconds(1);
    private readonly TimeSpan NoDuration = TimeSpan.Zero;

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

    private ProfilerLoggerDelegate CreateLogger() => LogProfiling;

    private void LogProfiling(ProfilerBatchResult result)
    {
      this.ProfilerRunResult = result;
      this.ProfilerRunSummary = result.Summary;

      Debug.WriteLine(result.Summary);
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

      _ = result.TotalDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
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
      ProfilerLoggerDelegate logger = CreateLogger();
      Func<Task> operationToProfile = CreateAsyncTimeConsumingOperation(this.ShortDuration);
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(logger, out result))
      {
        await operationToProfile.Invoke();
      }

      _ = result.TotalDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    }

    [Fact]
    public void LogTimeScoped_Logger_LoggerResultMustBeEqualToReturnedProfilerResult()
    {
      ProfilerLoggerDelegate logger = CreateLogger();
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
      ProfilerLoggerDelegate logger = CreateLogger();
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
      ProfilerLoggerDelegate logger = CreateLogger();
      Action operationToProfile = CreateNoTimeConsumingOperation();
      ProfilerBatchResult result;

      using (Profiler.LogTimeScoped(logger, out result))
      {
        operationToProfile.Invoke();
      }

      _ = result.Summary.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LogTimeAsync_ExecutionTimeWithLogger_MustBeEqualToMeasuredTime()
    {
      ProfilerLoggerDelegate logger = CreateLogger();
      Func<Task> operationToProfile = CreateAsyncTimeConsumingOperation(this.ShortDuration);

      ProfilerBatchResult result = await Profiler.LogTimeAsync(operationToProfile, 10, logger);

      _ = result.TotalDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    }

    [Fact]
    public void LogTime_ExecutionTimeWithLogger_MustBeEqualToMeasuredTime()
    {
      ProfilerLoggerDelegate logger = CreateLogger();
      Action operationToProfile = CreateTimeConsumingOperation(this.ShortDuration);
      
      ProfilerBatchResult result = Profiler.LogTime(operationToProfile, 1, logger);

      _ = result.TotalDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task LogTimeAsync_IterationsWithLogger_IterationCountMustBeEqualToReference(int iterationCount)
    {
      ProfilerLoggerDelegate logger = CreateLogger();
      Func<Task> operationToProfile = CreateAsyncNoTimeConsumingOperation();

      ProfilerBatchResult result = await Profiler.LogTimeAsync(operationToProfile, iterationCount, logger);

      _ = result.IterationCount.Should().Be(iterationCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void LogTime_IterationsWithLogger_IterationCountMustBeEqualToReference(int iterationCount)
    {
      ProfilerLoggerDelegate logger = CreateLogger();
      Action operationToProfile = CreateNoTimeConsumingOperation();

      ProfilerBatchResult result = Profiler.LogTime(operationToProfile, iterationCount, logger);

      _ = result.IterationCount.Should().Be(iterationCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void LogTime_AverageTimeWithLogger_MustBeEqualToReference(int iterationCount)
    {
      ProfilerLoggerDelegate logger = CreateLogger();
      Action operationToProfile = CreateTimeConsumingOperation(this.ShortDuration);

      ProfilerBatchResult result = Profiler.LogTime(operationToProfile, iterationCount, logger);

      _ = result.AverageDuration.Should().BeGreaterThanOrEqualTo(this.ShortDuration);
    }

    [Fact]
    public async Task LogTime_Atrributes_MustBeEqualToReference()
    {
      ProfilerLoggerDelegate logger = CreateLogger();
      const int iterations = 20;
      var targetTypes = new List<Type>
      {
        typeof(BenchmarkTarget<string>),
        typeof(BenchmarkTargetAlternate<int>)
      };

      ProfilerBuilder profilerBuilder = Profiler.CreateProfilerBuilder(targetTypes)
        .SetIterations(iterations)
        .SetLogger(result => Debug.WriteLine(result.Summary))
        .EnableDefaultLogOutput()
        .SetBaseUnit(TimeUnit.Microseconds);

      ProfiledTypeResultCollection result = await profilerBuilder.RunAsync();

      _ = result.First().First().First().IterationCount.Should().Be(iterations);
    }
  }
}
