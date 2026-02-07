namespace BionicCode.Utilities.Net.Profiling
{
    internal class HtmlDocumentBuilderInfo
    {
        public HtmlDocumentBuilderInfo()
        {
            DocumentTitle = string.Empty;
            DocumentTemplate = string.Empty;
            InPageNavigationElements = string.Empty;
            DocumentFooterElements = string.Empty;
            ResultNavigationElements = string.Empty;
            ChartSection = string.Empty;
            FileName = string.Empty;
            MemberName = string.Empty;
            ScriptCode = string.Empty;
            TargetSignature = string.Empty;
            TargetNamespace = string.Empty;
            TargetAssemblyName = string.Empty;
            TargetSourceFileName = string.Empty;
            TargetSourceFileLineNumber = 0;
            EnvironmentInfo = string.Empty;
        }

        public string ScriptCode { get; set; }
        public string DocumentTitle { get; set; }
        public string DocumentTemplate { get; set; }
        public string InPageNavigationElements { get; set; }
        public string DocumentFooterElements { get; set; }
        public string ResultNavigationElements { get; set; }
        public string ChartSection { get; set; }
        public string FileName { get; set; }
        public string MemberName { get; set; }
        public string TargetSignature { get; set; }
        public string TargetNamespace { get; set; }
        public string TargetAssemblyName { get; set; }
        public string TargetSourceFileName { get; set; }
        public int TargetSourceFileLineNumber { get; set; }
        public string EnvironmentInfo { get; internal set; }
    }
}
