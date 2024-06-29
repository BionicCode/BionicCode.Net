namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using System.Threading.Tasks;

  internal class ProfiledMethodInfo : ProfiledMemberInfo
  {
    public ProfiledMethodInfo(IEnumerable<IEnumerable<object>> argumentLists, MethodInfo methodInfo, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
      : base(argumentLists, isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
    {
      this.MethodInfo = methodInfo;
      this.IsAsyncTask = this.MethodInfo.IsAwaitableTask();
      this.IsAsyncValueTask = !this.IsAsyncTask && this.MethodInfo.IsAwaitableValueTask();
      this.IsGeneric = this.MethodInfo.IsGenericMethod;
      this.MethodName = this.MethodInfo.Name;
      this.MethodReturnTypeName = this.MethodInfo.ReturnType.Name;
    }

    public MethodInfo MethodInfo { get; }
    public string MethodName { get; }
    public string MethodReturnTypeName { get; }
    public bool IsAsync => this.IsAsyncTask || this.IsAsyncValueTask;
    public bool IsAsyncTask { get; }
    public bool IsAsyncValueTask { get; }
    public bool IsGeneric { get; }
    public Func<Task> AsyncMethodDelegate { get; set; }
    public Action MethodDelegate { get; set; }
    public override MemberInfo MemberInfo => this.MethodInfo;
  }
}