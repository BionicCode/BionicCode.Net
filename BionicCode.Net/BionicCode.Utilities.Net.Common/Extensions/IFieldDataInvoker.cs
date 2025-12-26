namespace BionicCode.Utilities.Net
{
    using System;

    internal interface IFieldDataInvoker
    {
        bool IsInvocable { get; }
        void SetGetterInvoker(Func<object?, object?>? getInvoker);
        void SetSetterInvoker(Action<object?, object?>? setInvoker);
    }
}
