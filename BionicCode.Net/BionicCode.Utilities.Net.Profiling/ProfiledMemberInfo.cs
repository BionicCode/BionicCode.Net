namespace BionicCode.Utilities.Net.Profiling
{
    internal abstract class ProfiledMemberInfo
    {
        public abstract MemberData MemberInfoData { get; }
        public string AssemblyName { get; }
        public int LineNumber { get; }
        public string SourceFilePath { get; }
        public Runtime TargetFramework { get; }
        public bool IsStatic { get; }
        public string Name => MemberInfoData.Name;
        public string DisplayName => MemberInfoData.DisplayName;
        public string ShortDisplayName => MemberInfoData.ShortDisplayName;
        public string Namespace => MemberInfoData.Namespace;
        public string Signature => MemberInfoData.Signature;
        public string ShortSignature => MemberInfoData.ShortSignature;
        public string ShortCompactSignature => MemberInfoData.ShortCompactSignature;

        protected ProfiledMemberInfo(bool isStatic, string assemblyName, int lineNumber, string sourceFilePath, Runtime targetFramework)
        {
            IsStatic = isStatic;
            AssemblyName = assemblyName;
            LineNumber = lineNumber;
            SourceFilePath = sourceFilePath;
            TargetFramework = targetFramework;
        }
    }
}
