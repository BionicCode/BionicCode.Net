namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Generic;

    internal readonly struct MethodArgumentInfo
    {
        public MethodArgumentInfo(List<object> arguments, int argumentListIndex)
        {
            this.Arguments = arguments;
            this.ArgumentListIndex = argumentListIndex;
        }

        public List<object?> Arguments { get; }
        public int ArgumentListIndex { get; }
    }
}
