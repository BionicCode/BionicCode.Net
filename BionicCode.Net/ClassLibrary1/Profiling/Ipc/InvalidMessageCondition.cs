namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  internal enum InvalidMessageCondition
  {
    Undefined = 0,
    Credentials,
    ProtocolViolation,
    Disconnected,
    EmptyMessage
  }
}