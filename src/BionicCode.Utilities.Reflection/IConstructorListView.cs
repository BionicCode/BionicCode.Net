namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IConstructorListView : IReadOnlyList<IConstructorDataView>, IEquatable<IConstructorListView>
{
    ImmutableList<IConstructorDataView> Constructors { get; }
    ITypeDataView DeclaringType { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
}