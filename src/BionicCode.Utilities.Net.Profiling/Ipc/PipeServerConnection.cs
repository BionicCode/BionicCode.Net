namespace BionicCode.Utilities.Net.Profiling.Ipc
{
    using System;
    using System.Diagnostics;
    using System.IO.Pipes;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PipeServerConnection : PipeConnection
    {
        public PipeServerConnection() : this(Guid.NewGuid())
        {
        }

        public PipeServerConnection(Guid serverClientLinkId) : base(serverClientLinkId)
          => Pipe = new NamedPipeServerStream(PipeIdString, PipeDirection.InOut, 1);

        public override void Disconnect() => Pipe.Disconnect();
        public override async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
        {
            if (!IsConnected)
            {
                Debug.WriteLine("Waiting for client to connect...");
                await Pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                Debug.WriteLine("Client connected.");
                return true;
            }

            return false;
        }

        public override async Task WriteToPipeAsync<TData>(IPipeMessage<TData> message)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to a client.");
            }

            string jsonRequest = JsonSerializer.Serialize(message);
            Pipe.WaitForPipeDrain();
            await PipeWriter.WriteLineAsync(jsonRequest).ConfigureAwait(false);
        }

        protected override async Task DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                Pipe.Disconnect();
#if NET6_0_OR_GREATER
                await Pipe.DisposeAsync().ConfigureAwait(false);
#else
        Pipe.Dispose();
#endif
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }

        private NamedPipeServerStream Pipe { get; }
        protected override PipeStream PipeStream => Pipe;
        public override bool IsConnected => Pipe.IsConnected;
    }
}
