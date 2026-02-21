namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IEventListView : IReadOnlyList<IEventDataView>, IEquatable<IEventListView>
{
    ITypeDataView DeclaringType { get; }
    ImmutableList<IEventDataView> Events { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    bool TryGetEventByName(string eventName, out IEventDataView? eventData);
    bool ContainsEventWithName(string eventName);
}