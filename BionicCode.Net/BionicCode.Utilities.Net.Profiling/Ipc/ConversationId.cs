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
            this.PipeId = pipeId;
            this.MessageId = messageId;
        }

        public Guid PipeId { get; }
        public Guid MessageId { get; }
        public string Value => ToString();

        public bool Equals(ConversationId other) => this.Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object obj) => obj is ConversationId conversationId && Equals(conversationId);

        public override int GetHashCode() => HashCode.Combine(this.PipeId, this.MessageId, this.Value);

        public override string ToString() => $"{this.PipeId}:{this.MessageId}";

        public static bool operator ==(ConversationId left, ConversationId right) => left.Equals(right);
        public static bool operator !=(ConversationId left, ConversationId right) => !left.Equals(right);
    }
}
