namespace BionicCode.Utilities.Net
{
    using System;
    using static BionicCode.Utilities.Net.MethodData;

    internal interface IStrictMethodDataInvoker
    {
        bool IsInvocable(MethodDataGenericTypeVariantKey methodDataGenericTypeVariantKey);
        void SetInvoker(MethodDataGenericTypeVariantKey methodDataGenericTypeVariantKey, Delegate strictlyTypedInvoker);
    }
}
