namespace BionicCode.Utilities.Net
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
