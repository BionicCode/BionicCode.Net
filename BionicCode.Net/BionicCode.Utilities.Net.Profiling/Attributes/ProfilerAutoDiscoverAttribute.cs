namespace BionicCode.Utilities.Net.Profiling
{
    using System;

    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ProfilerAutoDiscoverAttribute : Attribute
    {
        public ProfilerAutoDiscoverAttribute() : this(Type.EmptyTypes)
        {
        }

        public ProfilerAutoDiscoverAttribute(params Type[] genericTypeParameters)
          => this.GenericTypeParameters = genericTypeParameters;

        public Type[] GenericTypeParameters { get; }
    }
}
