namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Threading;
  using System.Threading.Tasks;

  internal class ProfiledMethodInfo : ProfiledMemberInfo
  {
    public ProfiledMethodInfo(IList<MethodArgumentInfo> argumentInfo, MethodData methodData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
      this.MethodData = methodData;
      this.ArgumentInfo = argumentInfo;
    }

    public MethodData MethodData { get; }
    public string MethodReturnTypeDisplayName => this.MethodData.ReturnTypeData.DisplayName;
    public bool IsAwaitable => this.MethodData.IsAwaitable;

    public bool IsAwaitableTask => this.MethodData.IsAwaitableTask;

    public bool IsAwaitableValueTask => this.MethodData.IsAwaitableValueTask;
    public bool IsAwaitableGenericValueTask => this.MethodData.IsAwaitableGenericValueTask;

    public bool IsGeneric => this.MethodData.IsGenericMethod;

    public override MemberInfoData MemberInfoData => this.MethodData;
    public IList<MethodArgumentInfo> ArgumentInfo { get; }
  }
}