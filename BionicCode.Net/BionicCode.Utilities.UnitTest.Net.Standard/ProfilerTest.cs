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

  public class ProfilerTest
  {
    private readonly TimeSpan ShortDuration = TimeSpan.FromMilliseconds(20);

    private Func<Task> CreateExecution(TimeSpan duration) => async () => await Task.Delay(duration);

    [Fact]
    public async Task LogTimeScoped_ExecutionTime_MustBeEqualToMeasuredTime()
    {
      Func<Task> operationToProfile = CreateExecution(ShortDuration);
      ProfilerBatchResult result;
      using (Profiler.LogTimeScoped(out result))
      {
        await operationToProfile.Invoke();
      }

      _ = result.TotalDuration.Should().Be(ShortDuration);
    }
  }
}
