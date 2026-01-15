namespace BionicCode.Utilities.Net
{
    using System;

    internal interface IStrictMethodDataInvoker
    {
        bool IsInvocable(Type returnType, Type targetType);
        void SetInvoker(Type returnType, Type targetType, Delegate strictlyTypedInvoker);
    }
}
