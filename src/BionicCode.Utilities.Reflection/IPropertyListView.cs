namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IPropertyListView : IReadOnlyList<IPropertyDataView>, IEquatable<IPropertyListView>
{
    ITypeDataView DeclaringType { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IPropertyDataView> Properties { get; }

    bool TryGetPropertyByName(string propertyName, out IPropertyDataView? propertyData);
    bool ContainsPropertyWithName(string propertyName);
}