namespace BionicCode.Utilities.Net
{
    public enum PropertyAccessor
    {
        Undefined = 0,
        PropertySet,
        PropertyGet,
        PropertyGetAndPropertySet,
        EventAdd,
        EventRemove,
        EventAddAndEventRemove,
        None,
    }
}
