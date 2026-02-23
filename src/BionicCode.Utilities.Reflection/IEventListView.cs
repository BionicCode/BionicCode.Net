namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IEventListView : ICollection, IReadOnlyList<IEventDataView>, IEquatable<IEventListView>
{
    new int Count { get; }
    ITypeDataView DeclaringType { get; }
    ImmutableList<IEventDataView> Events { get; }
    bool TryGetEventByName(string eventName, out IEventDataView? eventData);
    bool ContainsEventWithName(string eventName);
}