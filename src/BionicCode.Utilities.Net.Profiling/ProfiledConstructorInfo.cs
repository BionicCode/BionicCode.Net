namespace BionicCode.Utilities.Net.Profiling;

using System.Collections.Generic;

internal class ProfiledConstructorInfo : ProfiledMemberInfo
{
    public ProfiledConstructorInfo(IList<MethodArgumentInfo> argumentInfo, ConstructorData constructorData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
        ConstructorData = constructorData;
        ArgumentInfo = argumentInfo;
    }

    public ConstructorData ConstructorData { get; }
    public override MemberData MemberInfoData => ConstructorData;
    public IList<MethodArgumentInfo> ArgumentInfo { get; }
}
