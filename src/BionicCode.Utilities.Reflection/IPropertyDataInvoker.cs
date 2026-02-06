namespace BionicCode.Utilities.Net.Reflection
{
    internal interface IPropertyDataInvoker
    {
        bool IsInvocable { get; }
        bool HasGetter { get; }
        bool HasSetter { get; }
        bool IsIndexer { get; }
    }
}
