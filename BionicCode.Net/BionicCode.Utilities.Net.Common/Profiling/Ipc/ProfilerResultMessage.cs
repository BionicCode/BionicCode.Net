namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  internal class ProfilerResultMessage
  {
    public ProfiledTypeResultCollection Results { get; }

    public ProfilerResultMessage(ProfiledTypeResultCollection results) => this.Results = results;
  }
}
