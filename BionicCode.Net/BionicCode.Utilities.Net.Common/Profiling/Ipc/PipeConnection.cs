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
      this.ServerClientLinkId = serverClientLinkId;
      this.PipeId = pipeId;
      this.pipeWriterResource = new Lazy<StreamWriter>(StreamWriterFactory);
      this.pipeReaderResource = new Lazy<StreamReader>(StreamReaderFactory);
    }

    public abstract void Disconnect();
    public abstract Task<bool> TryConnectAsync(CancellationToken cancellationToken);

    public IPipeMessage<TData> CreateNewConversation<TData>(MessageType messageType, TData data)
    {
      var conversationId = new ConversationId(this.PipeId, this.ServerClientLinkId);
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
      this.PipeStream.WaitForPipeDrain();
      string jsonResponse = await this.PipeReader.ReadLineAsync().ConfigureAwait(false);
      if (string.IsNullOrWhiteSpace(jsonResponse))
      {
        IPipeMessage<TData> emptyMessage = CreateEmptyMessage<TData>();
        return emptyMessage;
      }

      IPipeMessage<TData> receivedMessage = JsonSerializer.Deserialize<PipeMessage<TData>>(jsonResponse);
      ConversationId conversationId = receivedMessage.Id;
      if (conversationId.PipeId != this.PipeId)
      {
        receivedMessage.InvalidateMessage(InvalidMessageCondition.Credentials);
      }

      return receivedMessage;
    }

    private StreamWriter StreamWriterFactory()
      => new StreamWriter(this.PipeStream) { AutoFlush = true };

    private StreamReader StreamReaderFactory()
      => new StreamReader(this.PipeStream);

    public Guid ServerClientLinkId { get; }
    public Guid PipeId { get; }
    public abstract bool IsConnected { get; }
    public bool IsClosed { get; private set; }
    protected abstract PipeStream PipeStream { get; }
    private readonly Lazy<StreamWriter> pipeWriterResource;
    private readonly Lazy<StreamReader> pipeReaderResource;
    protected StreamWriter PipeWriter => this.pipeWriterResource.Value;
    protected StreamReader PipeReader => this.pipeReaderResource.Value;
    public string PipeIdString => this.PipeId.ToString();
    public bool IsDisposed { get; private set; }

    protected virtual async Task DisposeAsync(bool disposing)
    {
      if (!this.IsDisposed && disposing)
      {
        // TODO: dispose managed state (managed objects)
        this.PipeReader?.Dispose();
        this.PipeWriter?.Dispose();

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        this.IsDisposed = true;
        this.IsClosed = true;
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