namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PendingCommandInfo
    {
        public PendingCommandInfo(TimeSpan pendingTimeout, DateTime timestamp, Action<CancellationToken> executeDelegate, TimeSpan executingTimeout, CancellationToken cancellationToken)
        {
            PendingTimeout = pendingTimeout;
            Timestamp = timestamp;
            ExecuteDelegate = executeDelegate;
            ExecutingTimeout = executingTimeout;
            CancellationToken = cancellationToken;
            AsyncExecuteDelegate = null;
        }

        public PendingCommandInfo(TimeSpan pendingTimeout, DateTime timestamp, Func<CancellationToken, Task> asyncExecuteDelegate, TimeSpan executingTimeout, CancellationToken cancellationToken)
        {
            PendingTimeout = pendingTimeout;
            Timestamp = timestamp;
            AsyncExecuteDelegate = asyncExecuteDelegate;
            ExecutingTimeout = executingTimeout;
            CancellationToken = cancellationToken;
            ExecuteDelegate = null;
        }

        public TimeSpan PendingTimeout { get; }
        public DateTime Timestamp { get; }
        public Action<CancellationToken> ExecuteDelegate { get; }
        public Func<CancellationToken, Task> AsyncExecuteDelegate { get; }
        public TimeSpan ExecutingTimeout { get; }
        public CancellationToken CancellationToken { get; }
    }
}
