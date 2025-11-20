namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PendingCommandInfo
    {
        public PendingCommandInfo(TimeSpan pendingTimeout, DateTime timestamp, Action<CancellationToken> executeDelegate, TimeSpan executingTimeout, CancellationToken cancellationToken)
        {
            this.PendingTimeout = pendingTimeout;
            this.Timestamp = timestamp;
            this.ExecuteDelegate = executeDelegate;
            this.ExecutingTimeout = executingTimeout;
            this.CancellationToken = cancellationToken;
            this.AsyncExecuteDelegate = null;
        }

        public PendingCommandInfo(TimeSpan pendingTimeout, DateTime timestamp, Func<CancellationToken, Task> asyncExecuteDelegate, TimeSpan executingTimeout, CancellationToken cancellationToken)
        {
            this.PendingTimeout = pendingTimeout;
            this.Timestamp = timestamp;
            this.AsyncExecuteDelegate = asyncExecuteDelegate;
            this.ExecutingTimeout = executingTimeout;
            this.CancellationToken = cancellationToken;
            this.ExecuteDelegate = null;
        }

        public TimeSpan PendingTimeout { get; }
        public DateTime Timestamp { get; }
        public Action<CancellationToken> ExecuteDelegate { get; }
        public Func<CancellationToken, Task> AsyncExecuteDelegate { get; }
        public TimeSpan ExecutingTimeout { get; }
        public CancellationToken CancellationToken { get; }
    }
}
