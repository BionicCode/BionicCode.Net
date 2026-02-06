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
                Data = data;
                Status = status;
                Id = id;
                IsValid = isValid;
            }

            public PipeMessage(ConversationId conversationId, TData data)
            {
                Id = conversationId;
                Data = data;
                IsValid = true;
            }

            public void InvalidateMessage(InvalidMessageCondition messageCondition)
            {
                IsValid = false;

                switch (messageCondition)
                {
                    case InvalidMessageCondition.Credentials:
                        Status = Status.InvalidCredentials;
                        break;
                    case InvalidMessageCondition.Undefined:
                        break;
                    case InvalidMessageCondition.ProtocolViolation:
                        Status = Status.ProtocolViolation;
                        break;
                    case InvalidMessageCondition.Disconnected:
                        Status = Status.Disconnected;
                        break;
                    default:
                        Status = Status.Undefined;
                        break;
                }
            }

            public Status Status { get; private set; }
            public TData Data { get; }
            public bool HasData => Data != null;
            public ConversationId Id { get; }
            public bool IsValid { get; private set; }
            public bool IsEmpty => Id.Equals(ConversationId.Empty);
        }
    }
}
