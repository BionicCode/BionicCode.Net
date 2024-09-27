namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Linq.Expressions;
  using System.Reflection;
  using System.Threading.Tasks;

  internal class InstanceProviderInfo
  {
    public InstanceProviderInfo(MethodData factoryMethod, object[] argumentList = null)
    {
      this.factoryMethodData = factoryMethod;
      this.ArgumentList = argumentList;
      this.IsAwaitable = factoryMethod.IsAwaitable;
    }

    public InstanceProviderInfo(ConstructorData constructor, object[] argumentList = null)
    {
      this.constructorData = constructor;
      this.ArgumentList = argumentList;
      this.IsAwaitable = false;
    }

    public object CreateTargetInstance(object target)
    {
      if (this.IsAwaitable)
      {
        throw new InvalidOperationException($"The factory method awaitable. Check {nameof(this.IsAwaitable)} to ensure that the instance provider is not an awaitable method.")
      }

      if (this.instance is null)
      {
        if (this.factoryMethodData != null)
        {
          this.instance = this.factoryMethodData.Invoke(null);
        } 
        else if (this.constructorData != null)
        {
          this.instance = this.constructorData.Invoke(this.ArgumentList);
        }
        else if (this.fieldData != null)
        {
          this.instance = this.fieldData.GetValue(null);
        }
        else if (this.propertyData != null)
        {
          this.instance = this.propertyData.Get(null, this.ArgumentList);
        }
      }

      return this.instance;
}

    public async ValueTask<object> CreateTargetInstanceAsync()
    {
      if (!this.IsAwaitable)
      {
        throw new InvalidOperationException($"The factory method is not awaitable. Check {nameof(this.IsAwaitable)} to ensure that the instance provider is an awaitable method.")
      }

      if (this.instance is null)
      {
        if (this.factoryMethodData != null)
        {
          if (this.factoryMethodData.IsAwaitableTask)
          {
            this.instance = await this.factoryMethodData.InvokeAwaitableTaskWithResultAsync(null);
          }
          else if (this.factoryMethodData.IsAwaitableGenericValueTask)
          {
            this.instance = await this.factoryMethodData.InvokeAwaitableValueTaskWithResultAsync(null);
          }
        }
      }

      return this.instance;
    }

    public object[] ArgumentList { get; }
    public bool IsAwaitable { get; }

    private readonly MethodData factoryMethodData;
    private readonly ConstructorData constructorData;
    private readonly PropertyData propertyData;
    private readonly FieldData fieldData;
    private object instance;
  }
}