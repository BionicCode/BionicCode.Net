namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IParameterListView : IEquatable<IParameterListView>
{
    IParameterDataView this[int index] { get; }
    int Count { get; }
    IParameterizedMemberDataView DeclaringMemberDataView { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IParameterDataView> Parameters { get; }
    IEnumerator<IParameterDataView> GetEnumerator();
    bool TryGetParameterByName(string parameterName, out IParameterDataView? parameterData);
}