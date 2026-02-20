namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IParameterListView
{
    IParameterDataView this[int index] { get; }
    int Count { get; }
    IParameterizedMemberDataView DeclaringMemberDataView { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IParameterDataView> Parameters { get; }
    bool Equals(object? obj);
    bool Equals(IParameterListView? other);
    IEnumerator<IParameterDataView> GetEnumerator();
    int GetHashCode();
    bool TryGetParameterByName(string parameterName, out IParameterDataView? parameterData);
}