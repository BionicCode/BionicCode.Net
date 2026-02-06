using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BionicCode.Utilities.Net.Common.Isolation.Net6")]
namespace BionicCode.Utilities.Net.Profiling.Ipc
{
    using System;
    using System.Text.Json.Serialization;

    internal readonly struct ConversationId : IEquatable<ConversationId>
    {
        public static ConversationId Empty { get; } = new ConversationId(Guid.Empty, Guid.Empty);

        [JsonConstructor]
        public ConversationId(Guid pipeId, Guid messageId)
        {
            PipeId = pipeId;
            MessageId = messageId;
        }

        public Guid PipeId { get; }
        public Guid MessageId { get; }
        public string Value => ToString();

        public bool Equals(ConversationId other) => Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object obj) => obj is ConversationId conversationId && Equals(conversationId);

        public override int GetHashCode() => HashCode.Combine(PipeId, MessageId, Value);

        public override string ToString() => $"{PipeId}:{MessageId}";

        public static bool operator ==(ConversationId left, ConversationId right) => left.Equals(right);
        public static bool operator !=(ConversationId left, ConversationId right) => !left.Equals(right);
    }
}
