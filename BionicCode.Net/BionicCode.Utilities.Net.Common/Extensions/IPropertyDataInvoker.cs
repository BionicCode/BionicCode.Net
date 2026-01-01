namespace BionicCode.Utilities.Net
{
    internal interface IPropertyDataInvoker
    {
        bool IsInvocable { get; }
        bool HasGetter { get; }
        bool HasSetter { get; }
        bool IsIndexer { get; }
    }
}
