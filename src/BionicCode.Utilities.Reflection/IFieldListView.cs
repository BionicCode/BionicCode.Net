namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IFieldListView : ICollection, IReadOnlyList<IFieldDataView>, IEquatable<IFieldListView>
{
    new int Count { get; }
    ITypeDataView DeclaringType { get; }
    ImmutableList<IFieldDataView> Fields { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }

    bool TryGetFieldByName(string fieldName, out IFieldDataView? fieldData);
    bool ContainsFieldWithName(string fieldName);
}