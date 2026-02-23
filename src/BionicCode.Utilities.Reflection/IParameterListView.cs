namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IParameterListView : ICollection, IReadOnlyList<IParameterDataView>, IEquatable<IParameterListView>
{
    new int Count { get; }
    IParameterizedMemberDataView DeclaringMember { get; }
    ImmutableList<IParameterDataView> Parameters { get; }
    bool TryGetParameterByName(string parameterName, out IParameterDataView? parameterData);
    bool ContainsParameterWithName(string parameterName);
}