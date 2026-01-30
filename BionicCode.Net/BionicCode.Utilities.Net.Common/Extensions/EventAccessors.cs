namespace BionicCode.Utilities.Net
{
    [Flags]
    public enum EventAccessors
    {
        None = 0,
        Add = 1,
        Remove = 2,
        AddAndRemove = Add | Remove,
    }
}
