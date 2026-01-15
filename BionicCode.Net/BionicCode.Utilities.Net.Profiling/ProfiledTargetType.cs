[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("BionicCode.Utilities.Net.UnitTest")]
namespace BionicCode.Utilities.Net.Profiling
{
    internal enum ProfiledTargetType
    {
        Undefined = 0,
        Method,
        Constructor,
        Delegate,
        Event,
        Scope,
        PropertyGet,
        PropertySet,
        IndexerGet,
        IndexerSet,
    }
}
