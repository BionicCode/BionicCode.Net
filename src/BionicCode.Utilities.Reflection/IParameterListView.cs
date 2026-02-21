namespace BionicCode.Utilities.Net.Reflection;

using System.Collections.Immutable;

public interface IParameterListView : IReadOnlyList<IParameterDataView>, IEquatable<IParameterListView>
{
    IParameterizedMemberDataView DeclaringMember { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IParameterDataView> Parameters { get; }
    bool TryGetParameterByName(string parameterName, out IParameterDataView? parameterData);
    bool ContainsParameterWithName(string parameterName);
}