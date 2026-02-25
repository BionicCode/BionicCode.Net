namespace BionicCode.Utilities.Net.Reflection;

using System.Reflection;

public readonly struct MemberEnumerationRule : IEquatable<MemberEnumerationRule>
{
    public static MemberEnumerationRule FullTypeHierarchyAndImplicitInterfaceImplementationsOnly { get; } = new MemberEnumerationRule(ReflectionConstants.AllMembersFullHierarchyFlags, InterfaceImplementationScope.ImplicitInterfaceImplementationsOnly);
    public static MemberEnumerationRule FullTypeHierarchyAndExplicitInterfaceImplementationsOnly { get; } = new MemberEnumerationRule(ReflectionConstants.AllMembersFullHierarchyFlags, InterfaceImplementationScope.ExplicitInterfaceImplementationsOnly);
    public static MemberEnumerationRule FullTypeHierarchyAndImplicitAndExplicitInterfaceImplementations { get; } = new MemberEnumerationRule(ReflectionConstants.AllMembersFullHierarchyFlags, InterfaceImplementationScope.AllImplementations);
    public static MemberEnumerationRule DeclaringTypeAndImplicitInterfaceImplementationsOnly { get; } = new MemberEnumerationRule(ReflectionConstants.AllDeclaredMembersFlags, InterfaceImplementationScope.ImplicitInterfaceImplementationsOnly);
    public static MemberEnumerationRule DeclaringTypeAndExplicitInterfaceImplementationsOnly { get; } = new MemberEnumerationRule(ReflectionConstants.AllDeclaredMembersFlags, InterfaceImplementationScope.ExplicitInterfaceImplementationsOnly);
    public static MemberEnumerationRule DeclaringTypeAndImplicitAndExplicitInterfaceImplementations { get; } = new MemberEnumerationRule(ReflectionConstants.AllDeclaredMembersFlags, InterfaceImplementationScope.AllImplementations);

    public MemberEnumerationRule(BindingFlags bindingFlags, InterfaceImplementationScope strictness)
    {
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<BindingFlags>(bindingFlags);
        ArgumentExceptionAdvanced.ThrowIfEnumIsNotDefined<InterfaceImplementationScope>(strictness);

        BindingFlags = bindingFlags;
        Strictness = strictness;
    }

    public BindingFlags BindingFlags { get; }
    public InterfaceImplementationScope Strictness { get; }

    public override bool Equals(object? obj) => obj is MemberEnumerationRule other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(BindingFlags, Strictness);

    public bool Equals(MemberEnumerationRule other) => BindingFlags == other.BindingFlags
        && Strictness == other.Strictness;

    public static bool operator ==(MemberEnumerationRule left, MemberEnumerationRule right) => left.Equals(right);

    public static bool operator !=(MemberEnumerationRule left, MemberEnumerationRule right) => !(left == right);
}
