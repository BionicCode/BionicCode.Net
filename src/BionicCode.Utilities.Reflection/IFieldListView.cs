namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IFieldListView : IReadOnlyList<IFieldDataView>, IEquatable<IFieldListView>
{
    ITypeDataView DeclaringType { get; }
    ImmutableList<IFieldDataView> Fields { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }

    bool TryGetFieldByName(string fieldName, out IFieldDataView? fieldData);
    bool ContainsFieldWithName(string fieldName);
}