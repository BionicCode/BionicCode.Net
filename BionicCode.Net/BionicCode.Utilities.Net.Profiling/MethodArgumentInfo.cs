namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Immutable;

    internal readonly struct MethodArgumentInfo
    {
        public MethodArgumentInfo(ImmutableList<object?>? arguments, int argumentListIndex)
        {
            this.Arguments = arguments ?? ImmutableList<object?>.Empty;
            this.ArgumentListIndex = argumentListIndex;
        }

        public ImmutableList<object?> Arguments { get; }
        public int ArgumentListIndex { get; }
    }
}
