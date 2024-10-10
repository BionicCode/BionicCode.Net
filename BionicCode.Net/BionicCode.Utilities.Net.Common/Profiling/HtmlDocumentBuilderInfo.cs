namespace BionicCode.Utilities.Net
{
  internal class HtmlDocumentBuilderInfo
  {
    public HtmlDocumentBuilderInfo()
    {
      this.DocumentTitle = string.Empty;
      this.DocumentTemplate = string.Empty;
      this.InPageNavigationElements = string.Empty;
      this.DocumentFooterElements = string.Empty;
      this.ResultNavigationElements = string.Empty;
      this.ChartSection = string.Empty;
      this.FileName = string.Empty;
      this.MemberName = string.Empty;
      this.ScriptCode = string.Empty;
      this.TargetSignature = string.Empty;
      this.TargetNamespace = string.Empty;
      this.TargetAssemblyName = string.Empty;
      this.TargetSourceFileName = string.Empty;
      this.TargetSourceFileLineNumber = 0;
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
  }
}