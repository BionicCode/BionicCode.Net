namespace BionicCode.Utilities.Net.Profiling.Ipc
{
    using System;
    using System.Diagnostics;
    using System.IO.Pipes;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PipeClientConnection : PipeConnection
    {
        public PipeClientConnection(Guid serverPipeId, Guid serverClientLinkId) : base(serverClientLinkId, serverPipeId)
          => Pipe = new NamedPipeClientStream(".", PipeIdString, PipeDirection.InOut);

        public override void Disconnect() => Dispose();
        public override async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
        {
            if (!IsConnected)
            {
                await Pipe.ConnectAsync();
                return true;
            }

            return false;
        }

        public override async Task WriteToPipeAsync<TData>(IPipeMessage<TData> message)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Not connected to a server.");
            }

            Debug.WriteLine($"Connected");

            string jsonRequest = JsonSerializer.Serialize(message);
            Pipe.WaitForPipeDrain();
            await PipeWriter.WriteLineAsync(jsonRequest).ConfigureAwait(false);
        }

        protected override async Task DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
#if NET6_0_OR_GREATER
                await Pipe.DisposeAsync().ConfigureAwait(false);
#else
        Pipe.Dispose();
#endif
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }

        private NamedPipeClientStream Pipe { get; }
        protected override PipeStream PipeStream => Pipe;
        public override bool IsConnected => Pipe.IsConnected;
    }
}
