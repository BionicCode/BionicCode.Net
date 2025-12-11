namespace BionicCode.Utilities.Net
{
    internal enum ParameterKind
    {
        Undefined = 0,
        In,
        Out,
        Ref,
        RefReadOnly,
        Optional,
        Params,
        This,
    }
}
