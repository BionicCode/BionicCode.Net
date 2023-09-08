namespace BionicCode.Utilities.Net
{
  using System.Reflection;

  internal class InstanceProviderInfo
  {
    public InstanceProviderInfo(object[] argumentList, MemberInfo memberInfo)
    {
      this.ArgumentList = argumentList;
      this.MemberInfo = memberInfo;
    }

    public object[] ArgumentList { get; }
    public MemberInfo MemberInfo { get; }
  }
}