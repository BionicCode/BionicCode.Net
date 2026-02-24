namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface ITypeListView : ICollection, IReadOnlyList<ITypeDataView>, IEquatable<ITypeListView>
{
    new int Count { get; }
    bool IsEmpty { get; }
    bool HasItems { get; }
    ImmutableList<ITypeDataView> Types { get; }

    bool ContainsTypeWithName(string typeName);
    bool TryGetTypesByName(string typeName, out ITypeListView typeList);
}