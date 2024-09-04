namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  using System.Text.Json.Serialization;
  using System.Threading.Tasks;

  internal abstract partial class PipeConnection
  {
#if NET6_0_OR_GREATER
    public virtual async ValueTask DisposeAsync() => await DisposeAsync(disposing: true).ConfigureAwait(false);
#endif

    public class PipeMessage<TData> : IPipeMessage<TData>
    {
      public static IPipeMessage<TData> Empty { get; }

      static PipeMessage() => PipeMessage<TData>.Empty = new PipeMessage<TData>(ConversationId.Empty, default) { IsValid = false };

      [JsonConstructor]
      public PipeMessage(TData data, Status status, ConversationId id, bool isValid)
      {
        this.Data = data;
        this.Status = status;
        this.Id = id;
        this.IsValid = isValid;
      }

      public PipeMessage(ConversationId conversationId, TData data)
      {
        this.Id = conversationId;
        this.Data = data;
        this.IsValid = true;
      }

      public void InvalidateMessage(InvalidMessageCondition messageCondition)
      {
        this.IsValid = false;

        switch (messageCondition)
        {
          case InvalidMessageCondition.Credentials:
            this.Status = Status.InvalidCredentials; break;
          case InvalidMessageCondition.Undefined:
            break;
          case InvalidMessageCondition.ProtocolViolation:
            this.Status = Status.ProtocolViolation;
            break;
          case InvalidMessageCondition.Disconnected:
            this.Status = Status.Disconnected;
            break;
          default:
            this.Status = Status.Undefined; break;
        }
      }

      public Status Status { get; private set; }
      public TData Data { get; }
      public bool HasData => this.Data != null;
      public ConversationId Id { get; }
      public bool IsValid { get; private set; }
      public bool IsEmpty => this.Id.Equals(ConversationId.Empty);
    }
  }
}