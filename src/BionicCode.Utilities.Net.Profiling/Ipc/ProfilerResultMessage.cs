namespace BionicCode.Utilities.Net.Profiling.Ipc
{
    internal class ProfilerResultMessage
    {
        public ProfiledTypeResultCollection Results { get; }

        public ProfilerResultMessage(ProfiledTypeResultCollection results) => Results = results;
    }
}
