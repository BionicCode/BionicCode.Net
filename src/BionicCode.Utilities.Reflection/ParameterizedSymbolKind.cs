namespace BionicCode.Utilities.Net.Reflection;

public enum ParameterizedSymbolKind
{
    Undefined = 0,
    MemberMethod,
    MemberNormalPropertySet,
    MemberIndexerPropertyGet,
    MemberIndexerPropertySet,
    MemberIndexerPropertyGetOrSet,
    MemberConstructor,
}
