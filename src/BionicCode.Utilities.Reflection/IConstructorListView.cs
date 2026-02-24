namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IConstructorListView : ICollection, IReadOnlyList<IConstructorDataView>, IEquatable<IConstructorListView>
{
    new int Count { get; }
    bool IsEmpty { get; }
    bool HasItems { get; }
    ImmutableList<IConstructorDataView> Constructors { get; }
    ITypeDataView DeclaringType { get; }
}