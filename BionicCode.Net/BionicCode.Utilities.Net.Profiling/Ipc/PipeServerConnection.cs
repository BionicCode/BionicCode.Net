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
      => this.Pipe = new NamedPipeServerStream(this.PipeIdString, PipeDirection.InOut, 1);

    public override void Disconnect() => this.Pipe.Disconnect();
    public override async Task<bool> TryConnectAsync(CancellationToken cancellationToken)
    {
      if (!this.IsConnected)
      {
        Debug.WriteLine("Waiting for client to connect...");
        await this.Pipe.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
        Debug.WriteLine("Client connected.");
        return true;
      }

      return false;
    }

    public override async Task WriteToPipeAsync<TData>(IPipeMessage<TData> message)
    {
      if (!this.IsConnected)
      {
        throw new InvalidOperationException("Not connected to a client.");
      }

      string jsonRequest = JsonSerializer.Serialize(message);
      this.Pipe.WaitForPipeDrain();
      await this.PipeWriter.WriteLineAsync(jsonRequest).ConfigureAwait(false);
    }

    protected override async Task DisposeAsync(bool disposing)
    {
      if (!this.IsDisposed && disposing)
      {
        this.Pipe.Disconnect();
#if NET6_0_OR_GREATER
        await this.Pipe.DisposeAsync().ConfigureAwait(false);
#else
        this.Pipe.Dispose();
#endif
      }

      await base.DisposeAsync(disposing).ConfigureAwait(false);
    }

    private NamedPipeServerStream Pipe { get; }
    protected override PipeStream PipeStream => this.Pipe;
    public override bool IsConnected => this.Pipe.IsConnected;
  }
}