namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading.Tasks;

    internal interface IMethodDataInvoker
    {
        bool IsInvocable { get; }
        void SetInvoker(Func<object?, object?[]?, object?>? invoker);
        void SetInvoker(Func<object?, object?[]?, Task>? asyncTaskInvoker);
        void SetInvoker(Func<object?, object?[]?, Task<object?>>? asyncGenericTaskInvoker);
        void SetInvoker(Func<object?, object?[]?, ValueTask>? asyncValueTaskInvoker);
        void SetInvoker(Func<object?, object?[]?, ValueTask<object?>>? asyncGenericValueTaskInvoker);
    }
}
