namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Immutable;

    internal readonly struct MethodArgumentInfo
    {
        public MethodArgumentInfo(ImmutableArray<object?>? arguments, int argumentListIndex)
        {
            Arguments = arguments ?? ImmutableArray<object?>.Empty;
            ArgumentListIndex = argumentListIndex;
        }

        public ImmutableArray<object?> Arguments { get; }
        public int ArgumentListIndex { get; }
    }
}
