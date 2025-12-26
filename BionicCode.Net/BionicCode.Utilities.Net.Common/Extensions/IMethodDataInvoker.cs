namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading.Tasks;

    internal interface IMethodDataInvoker
    {
        bool IsInvocable { get; }
        void SetInvoker(Func<object?, object?[]?, object?>? invocator);
        void SetInvoker(Func<object?, object?[]?, Task>? asyncTaskInvocator);
        void SetInvoker(Func<object?, object?[]?, Task<object?>>? asyncGenericTaskInvocator);
        void SetInvoker(Func<object?, object?[]?, ValueTask>? asyncValueTaskInvocator);
        void SetInvoker(Func<object?, object?[]?, ValueTask<object?>>? asyncGenericValueTaskInvocator);
    }
}
