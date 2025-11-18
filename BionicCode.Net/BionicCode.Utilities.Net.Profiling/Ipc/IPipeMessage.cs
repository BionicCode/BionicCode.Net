namespace BionicCode.Utilities.Net.Profiling.Ipc
{
  internal interface IPipeMessage<TData>
  {
    TData Data { get; }
    bool HasData { get; }
    ConversationId Id { get; }
    bool IsValid { get; }
    bool IsEmpty { get; }
    Status Status { get; }

    void InvalidateMessage(InvalidMessageCondition messageCondition);
  }
}