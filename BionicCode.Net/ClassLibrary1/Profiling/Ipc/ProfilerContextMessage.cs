namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  internal class ProfilerContextMessage
  {
    public IAttributeProfilerConfiguration ProfilerConfiguration { get; }

    public ProfilerContextMessage(IAttributeProfilerConfiguration profilerConfiguration) => this.ProfilerConfiguration = profilerConfiguration;
  }
}
