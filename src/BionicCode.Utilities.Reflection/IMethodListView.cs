namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IMethodListView : ICollection, IReadOnlyList<IMethodDataView>, IEquatable<IMethodListView>
{
    new int Count { get; }
    IMethodDataView this[string? methodName, ParameterDescriptorList? methodParameters] { get; }
    ITypeDataView DeclaringType { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IMethodDataView> Methods { get; }

    bool ContainsMethodWithName(string methodName);
    bool Equals(object? obj);
    new IEnumerator<IMethodDataView> GetEnumerator();
    int GetHashCode();
    bool TryGetMethodsByName(string methodName, out IMethodListView methodList);
}