namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;

  internal class ProfiledConstructorInfo : ProfiledMemberInfo
  {
    public ProfiledConstructorInfo(IList<IEnumerable<object>> argumentLists, ConstructorData constructorData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(argumentLists, isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework) 
      => this.ConstructorData = constructorData;

    public ConstructorData ConstructorData { get; }
    public override MemberInfoData MemberInfoData => this.ConstructorData;
  }
}