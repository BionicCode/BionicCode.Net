namespace BionicCode.Utilities.Net.Profiling.Ipc
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    internal abstract partial class PipeConnection : IDisposable
    {
        protected PipeConnection(Guid serverClientLinkId) : this(serverClientLinkId, Guid.NewGuid())
        {
        }

        protected PipeConnection(Guid serverClientLinkId, Guid pipeId)
        {
            ServerClientLinkId = serverClientLinkId;
            PipeId = pipeId;
            pipeWriterResource = new Lazy<StreamWriter>(StreamWriterFactory);
            pipeReaderResource = new Lazy<StreamReader>(StreamReaderFactory);
        }

        public abstract void Disconnect();
        public abstract Task<bool> TryConnectAsync(CancellationToken cancellationToken);

        public IPipeMessage<TData> CreateNewConversation<TData>(MessageType messageType, TData data)
        {
            var conversationId = new ConversationId(PipeId, ServerClientLinkId);
            var message = new PipeMessage<TData>(conversationId, data);
            return message;
        }

        public IPipeMessage<TData> CreateMessageForConversation<TData>(ConversationId conversationId, TData data)
        {
            var message = new PipeMessage<TData>(conversationId, data);
            return message;
        }

        public IPipeMessage<TData> CreateEmptyMessage<TData>() => PipeMessage<TData>.Empty;

        public abstract Task WriteToPipeAsync<TData>(IPipeMessage<TData> message);

        public virtual void Close()
        {
            Disconnect();
            Dispose();
        }

#if NET6_0_OR_GREATER
        public virtual async Task CloseAsync() => await DisposeAsync().ConfigureAwait(false);
#endif

        public async Task<IPipeMessage<TData>> ReadFromPipeAsync<TData>()
        {
            //_ = waitHandle.WaitOne();
            PipeStream.WaitForPipeDrain();
            string jsonResponse = await PipeReader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(jsonResponse))
            {
                IPipeMessage<TData> emptyMessage = CreateEmptyMessage<TData>();
                return emptyMessage;
            }

            IPipeMessage<TData> receivedMessage = JsonSerializer.Deserialize<PipeMessage<TData>>(jsonResponse);
            ConversationId conversationId = receivedMessage.Id;
            if (conversationId.PipeId != PipeId)
            {
                receivedMessage.InvalidateMessage(InvalidMessageCondition.Credentials);
            }

            return receivedMessage;
        }

        private StreamWriter StreamWriterFactory()
          => new StreamWriter(PipeStream) { AutoFlush = true };

        private StreamReader StreamReaderFactory()
          => new StreamReader(PipeStream);

        public Guid ServerClientLinkId { get; }
        public Guid PipeId { get; }
        public abstract bool IsConnected { get; }
        public bool IsClosed { get; private set; }
        protected abstract PipeStream PipeStream { get; }
        private readonly Lazy<StreamWriter> pipeWriterResource;
        private readonly Lazy<StreamReader> pipeReaderResource;
        protected StreamWriter PipeWriter => pipeWriterResource.Value;
        protected StreamReader PipeReader => pipeReaderResource.Value;
        public string PipeIdString => PipeId.ToString();
        public bool IsDisposed { get; private set; }

        protected virtual async Task DisposeAsync(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                // TODO: dispose managed state (managed objects)
                PipeReader?.Dispose();
                PipeWriter?.Dispose();

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                IsDisposed = true;
                IsClosed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PipeServerConnection()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }
        public async void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            await DisposeAsync(disposing: true).ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
