namespace BionicCode.Utilities.Net.Profiling
{
    using System;
    using System.Threading.Tasks;

    internal class InstanceProviderInfo
    {
        public InstanceProviderInfo(MethodData factoryMethod, object[]? argumentList = null)
        {
            factoryMethodData = factoryMethod;
            ArgumentList = argumentList;
            IsAwaitable = factoryMethod.IsAwaitable;
        }

        public InstanceProviderInfo(ConstructorData constructor, object[]? argumentList = null)
        {
            constructorData = constructor;
            ArgumentList = argumentList;
            IsAwaitable = false;
        }

        public InstanceProviderInfo(FieldData field, object[]? argumentList = null)
        {
            fieldData = field;
            ArgumentList = argumentList;
            IsAwaitable = false;
        }

        public InstanceProviderInfo(PropertyData property, object[]? argumentList = null)
        {
            propertyData = property;
            ArgumentList = argumentList;
            IsAwaitable = false;
        }

        public object CreateTargetInstance(object target)
        {
            if (IsAwaitable)
            {
                throw new InvalidOperationException($"The factory method is awaitable. Check {nameof(IsAwaitable)} to ensure that the instance provider is not an awaitable method.");
            }

            if (instance is null)
            {
                if (factoryMethodData != null)
                {
                    instance = factoryMethodData.Invoke(target, ReadOnlySpan<object>.Empty);
                }
                else if (constructorData != null)
                {
                    instance = constructorData.Invoke(ArgumentList);
                }
                else if (fieldData != null)
                {
                    instance = fieldData.GetValue(target);
                }
                else if (propertyData != null)
                {
                    instance = propertyData.IsIndexer
                        ? propertyData.GetIndexerValue(target, ArgumentList)
                        : propertyData.GetValue(target);
                }
            }

            return instance ?? throw new InvalidOperationException(InstanceProviderInfo.UnableToCreateInstanceMessage);
        }

        public async ValueTask<object> CreateTargetInstanceAsync(object target)
        {
            if (!IsAwaitable)
            {
                throw new InvalidOperationException($"The factory method is not awaitable. Check {nameof(IsAwaitable)} to ensure that the instance provider is an awaitable method.");
            }

            if (instance is null)
            {
                if (factoryMethodData != null)
                {
                    if (factoryMethodData.IsAwaitableTask)
                    {
                        instance = await factoryMethodData.InvokeAwaitableTaskWithResultAsync(target, ArgumentList);
                    }
                    else if (factoryMethodData.IsAwaitableGenericValueTask)
                    {
                        instance = await factoryMethodData.InvokeAwaitableValueTaskWithResultAsync(target, ArgumentList);
                    }
                }
            }

            return instance ?? throw new InvalidOperationException(InstanceProviderInfo.UnableToCreateInstanceMessage);
        }

        public object[] ArgumentList { get; }
        public bool IsAwaitable { get; }

        private readonly MethodData factoryMethodData;
        private readonly ConstructorData constructorData;
        private readonly PropertyData propertyData;
        private readonly FieldData fieldData;
        private object? instance;
        private const string UnableToCreateInstanceMessage = "Unable to create an instance of the profiled type";
    }
}
