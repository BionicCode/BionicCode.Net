namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IParameterListView
{
    IParameterDataView this[int index] { get; }

    static abstract IParameterListView Empty { get; }
    int Count { get; }
    SymbolReflectionInfoCacheKey DeclaringMemberCacheKey { get; }
    IParameterizedMemberDataView DeclaringMemberData { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IParameterDataView> Parameters { get; }

    bool Equals(object? obj);
    bool Equals(IParameterListView? other);
    IEnumerator<IParameterDataView> GetEnumerator();
    int GetHashCode();
    bool TryGetParameterByName(string parameterName, out IParameterDataView? parameterData);
}