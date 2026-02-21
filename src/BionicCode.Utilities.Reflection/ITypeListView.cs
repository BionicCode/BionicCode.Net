namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface ITypeListView : IReadOnlyList<ITypeDataView>, IEquatable<ITypeListView>
{
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<ITypeDataView> Types { get; }

    bool ContainsTypeWithName(string typeName);
    bool TryGetTypesByName(string typeName, out ITypeListView typeList);
}