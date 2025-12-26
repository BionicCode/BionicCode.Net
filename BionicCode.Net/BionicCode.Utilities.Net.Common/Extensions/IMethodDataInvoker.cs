namespace BionicCode.Utilities.Net
{
    using System;
    using System.Threading.Tasks;

    internal interface IMethodDataInvoker
    {
        void SetInvocator(Func<object?, object?[]?, object?>? invocator);
        void SetInvocator(Func<object?, object?[]?, Task>? asyncTaskInvocator);
        void SetInvocator(Func<object?, object?[]?, Task<object?>>? asyncGenericTaskInvocator);
        void SetInvocator(Func<object?, object?[]?, ValueTask>? asyncValueTaskInvocator);
        void SetInvocator(Func<object?, object?[]?, ValueTask<object?>>? asyncGenericValueTaskInvocator);
    }
}
