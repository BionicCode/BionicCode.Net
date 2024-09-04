using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("BionicCode.Utilities.Net.Common.Isolation.Net6")]
namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  using System;
  using System.Collections.Generic;
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

    public override int GetHashCode()
    {
      int hashCode = -652492873;
      hashCode = (hashCode * -1521134295) + this.PipeId.GetHashCode();
      hashCode = (hashCode * -1521134295) + this.MessageId.GetHashCode();
      hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(this.Value);
      return hashCode;
    }

    public override string ToString() => $"{this.PipeId}:{this.MessageId}";

    public static bool operator ==(ConversationId left, ConversationId right) => left.Equals(right);
    public static bool operator !=(ConversationId left, ConversationId right) => !left.Equals(right);
  }
}