namespace BionicCode.Utilities.Net.Profiling;

using System.Collections.Generic;
using System.Collections.Immutable;

internal class ProfiledMethodInfo : ProfiledMemberInfo
{
    public ProfiledMethodInfo(IEnumerable<MethodArgumentInfo> argumentInfo, MethodData methodData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
        MethodData = methodData;
        ArgumentInfo = argumentInfo.ToImmutableList();
    }

    public MethodData MethodData { get; }
    public string MethodReturnTypeDisplayName => MethodData.ReturnTypeData.DisplayName;
    public bool IsAwaitable => MethodData.IsAwaitable;

    public bool IsAwaitableTask => MethodData.IsAwaitableTask;

    public bool IsAwaitableValueTask => MethodData.IsAwaitableValueTask;
    public bool IsAwaitableGenericValueTask => MethodData.IsAwaitableGenericValueTask;
    public bool IsAwaitableGenericTask => MethodData.IsAwaitableGenericTask;

    public bool IsGeneric => MethodData.IsGenericMethod;

    public override MemberData MemberInfoData => MethodData;
    public ImmutableList<MethodArgumentInfo> ArgumentInfo { get; }
}
