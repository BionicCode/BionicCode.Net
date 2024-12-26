namespace BionicCode.Utilities.Net
{
  using System;
  using System.Threading;

  internal class PendingCommandInfo
  {
    public PendingCommandInfo(TimeSpan pendingTimeout, DateTime timestamp, Action<CancellationToken> executeDelegate, TimeSpan executingTimeout, CancellationToken cancellationToken)
    {
      this.PendingTimeout = pendingTimeout;
      this.Timestamp = timestamp;
      this.ExecuteDelegate = executeDelegate;
      this.ExecutingTimeout = executingTimeout;
      this.CancellationToken = cancellationToken;
    }

    public TimeSpan PendingTimeout { get; }
    public DateTime Timestamp { get; }
    public Action<CancellationToken> ExecuteDelegate { get; }
    public TimeSpan ExecutingTimeout { get; }
    public CancellationToken CancellationToken { get; }
  }
}