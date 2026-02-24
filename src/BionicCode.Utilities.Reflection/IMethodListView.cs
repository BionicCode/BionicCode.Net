namespace BionicCode.Utilities.Net.Reflection;

using System.Collections;
using System.Collections.Immutable;

public interface IMethodListView : ICollection, IReadOnlyList<IMethodDataView>, IEquatable<IMethodListView>
{
    new int Count { get; }
    ITypeDataView DeclaringType { get; }
    bool HasItems { get; }
    bool IsEmpty { get; }
    ImmutableList<IMethodDataView> Methods { get; }

    bool ContainsMethodWithName(string methodName);
    bool Equals(object? obj);
    new IEnumerator<IMethodDataView> GetEnumerator();
    int GetHashCode();
    bool TryGetMethodsByName(string methodName, out IMethodListView methodList);

    /// <summary>
    /// Gets the method data for the method with the specified name and parameter signature.
    /// </summary>
    /// <param name="methodName">The name of the method to retrieve. This value is case-sensitive.</param>
    /// <param name="genericMethodParameters">Optional. A <see cref="TypeList"/> of generic method parameter types that describe the expected generic parameters for the method. Should be <see langword="null"/> or empty if the method is not generic. If the method is a generic method, consider providing the expected generic parameters to disambiguate the method.</param>
    /// <param name="methodParameters">Optional. A <see cref="ParameterDescriptorList"/> of parameter information objects that describe the expected parameter types, positions, and modifiers for the method signature. Should be <see langword="null"/> or empty if method is parameterless. If the method has parameters, consider providing the expected parameters to disambiguate the method.</param>
    /// <param name="methodDataView">When this method returns, contains the method data that matches the specified name and parameter signature, if found; otherwise, null.</param>
    /// <returns>A <see cref="MemberLookupState"/> value indicating the result of the method lookup.</returns>
    MemberLookupState TryGetMethod(string? methodName, ITypeListView? genericMethodParameters, ParameterDescriptorList? methodParameters, out IMethodDataView? methodDataView);
}