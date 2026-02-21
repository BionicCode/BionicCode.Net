namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IEventListView : IEquatable<IEventListView>
{
    IEventDataView this[int index] { get; }
    int Count { get; }
    ITypeDataView DeclaringType { get; }
    ImmutableList<IEventDataView> Events { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    IEnumerator<IEventDataView> GetEnumerator();
    bool TryGetEventByName(string eventName, out IEventDataView? eventData);
}