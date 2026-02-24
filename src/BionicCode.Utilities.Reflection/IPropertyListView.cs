namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IPropertyListView : ICollection, IReadOnlyList<IPropertyDataView>, IEquatable<IPropertyListView>
{
    new int Count { get; }
    bool IsEmpty { get; }
    bool HasItems { get; }
    ITypeDataView DeclaringType { get; }
    ImmutableList<IPropertyDataView> Properties { get; }
    bool TryGetPropertyByName(string propertyName, out IPropertyDataView? propertyData);
    bool ContainsPropertyWithName(string propertyName);
}
