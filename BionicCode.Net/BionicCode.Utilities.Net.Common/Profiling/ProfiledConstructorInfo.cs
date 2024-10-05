namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal class ProfiledConstructorInfo : ProfiledMemberInfo
  {
    public ProfiledConstructorInfo(IList<MethodArgumentInfo> argumentInfo, ConstructorData constructorData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
      this.ConstructorData = constructorData;
      this.ArgumentInfo = argumentInfo;
    }

    public ConstructorData ConstructorData { get; }
    public override MemberInfoData MemberInfoData => this.ConstructorData;
    public IList<MethodArgumentInfo> ArgumentInfo { get; }
  }
}