using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BionicCode.Utilities.Net.Common.Isolation.Net6")]
namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  internal enum Status
  {
    Undefined = 0,
    RequestHandshake = 1,
    AcceptHandshake = 2,
    WaitToSendData = 4,
    ReadyToReceive = 8,
    WaitToReceiveData = 16,
    ReadyToSend = 32,
    Wait = 64,
    Cancelled = 128,
    Success = 256,
    InvalidCredentials = 512,
    ProtocolViolation = 1024,
    Disconnected = 2048,
    Tranferring = 4096,
    Idle = 8192
  }
}