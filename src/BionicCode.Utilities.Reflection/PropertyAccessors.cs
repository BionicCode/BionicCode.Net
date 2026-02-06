namespace BionicCode.Utilities.Net.Reflection
{
    [Flags]
    public enum PropertyAccessors
    {
        None = 0,
        Set = 1,
        Get = 2,
        GetAndSet = Get | Set,
    }
}
