namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal class ProfiledPropertyInfo : ProfiledMemberInfo
  {
    public ProfiledPropertyInfo(IEnumerable<IEnumerable<object>> argumentLists, PropertyInfo methodInfo, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isIndexer, bool isStatic)
      : base(argumentLists, isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
      this.PropertyInfo = methodInfo;
      this.IsIndexer = isIndexer;
    }

    public PropertyInfo PropertyInfo { get; }
    public string PropertyName => this.PropertyInfo.Name;
    public bool IsIndexer { get; }
    public Action SetDelegate { get; set; }
    public Action GetDelegate { get; set; }
    public override MemberInfo MemberInfoData => this.PropertyInfo;
  }
}