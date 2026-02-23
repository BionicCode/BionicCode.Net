namespace BionicCode.Utilities.Net.Reflection;

/// <summary>
/// Specifies the modifier of a parameter.
/// </summary>
public enum ParameterModifier
{
    Undefined = 0,
    Normal,
    In,
    Out,
    Ref,
    RefReadOnly,
}
