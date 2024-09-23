namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal class ProfiledConstructorInfo : ProfiledMemberInfo
  {
    public ProfiledConstructorInfo(IEnumerable<IEnumerable<object>> argumentLists, ConstructorInfo constructorInfo, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(argumentLists, isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework) => this.ConstructorInfo = constructorInfo;

    public ConstructorInfo ConstructorInfo { get; }
    public string ConstructorName => this.ConstructorInfo.DeclaringType.Name;
    public Action ConstructorDelegate { get; set; }
    public override MemberInfo MemberInfoData => this.ConstructorInfo;
  }
}