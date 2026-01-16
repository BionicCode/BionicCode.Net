namespace BionicCode.Utilities.Net
{
    using System;

    internal interface IStrictMethodDataInvoker
    {
        bool IsInvocable(SymbolInfoDataCacheKey symbolKey);
        void SetInvoker(SymbolInfoDataCacheKey symbolKey, Delegate strictlyTypedInvoker);
    }
}
