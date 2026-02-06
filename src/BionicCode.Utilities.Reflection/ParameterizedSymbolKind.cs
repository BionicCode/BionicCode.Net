namespace BionicCode.Utilities.Net.Reflection
{
    internal enum ParameterizedSymbolKind
    {
        Undefined = 0,
        MemberMethod,
        MemberNormalPropertySet,
        MemberIndexerPropertyGet,
        MemberIndexerPropertySet,
        MemberIndexerPropertyGetOrSet,
        MemberConstructor,
    }
}
