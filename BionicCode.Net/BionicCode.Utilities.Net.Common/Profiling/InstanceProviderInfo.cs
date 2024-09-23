namespace BionicCode.Utilities.Net
{
  using System;
  using System.Reflection;

  internal class InstanceProviderInfo
  {
    private object instance;
    private readonly MethodData factoryMethod;

    public InstanceProviderInfo(object[] argumentList, MethodData factoryMethod)
    {
      this.ArgumentList = argumentList;
      this.factoryMethod = factoryMethod;
    }

    public object GetInstance(object target) => this.instance ?? (this.instance = this.factoryMethod.Invoke(target, this.ArgumentList));

    public object[] ArgumentList { get; }
  }
}