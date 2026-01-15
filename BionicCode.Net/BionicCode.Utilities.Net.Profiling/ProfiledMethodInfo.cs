namespace BionicCode.Utilities.Net.Profiling
{
    using System.Collections.Generic;
    using System.Collections.Immutable;

    internal class ProfiledMethodInfo : ProfiledMemberInfo
    {
        public ProfiledMethodInfo(IEnumerable<MethodArgumentInfo> argumentInfo, MethodData methodData, string sourceFilePath, int lineNumber, string assemblyName, Runtime targetFramework, bool isStatic)
          : base(isStatic, assemblyName, lineNumber, sourceFilePath, targetFramework)
        {
            this.MethodData = methodData;
            this.ArgumentInfo = argumentInfo.ToImmutableList();
        }

        public MethodData MethodData { get; }
        public string MethodReturnTypeDisplayName => this.MethodData.ReturnTypeData.DisplayName;
        public bool IsAwaitable => this.MethodData.IsAwaitable;

        public bool IsAwaitableTask => this.MethodData.IsAwaitableTask;

        public bool IsAwaitableValueTask => this.MethodData.IsAwaitableValueTask;
        public bool IsAwaitableGenericValueTask => this.MethodData.IsAwaitableGenericValueTask;
        public bool IsAwaitableGenericTask => this.MethodData.IsAwaitableGenericTask;

        public bool IsGeneric => this.MethodData.IsGenericMethod;

        public override MemberData MemberInfoData => this.MethodData;
        public ImmutableList<MethodArgumentInfo> ArgumentInfo { get; }
    }
}
