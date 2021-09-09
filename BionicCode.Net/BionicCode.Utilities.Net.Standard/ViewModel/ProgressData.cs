namespace BionicCode.Utilities.Net.Standard.ViewModel
{
  public readonly struct ProgressData
  {
    public ProgressData(string message, int progress)
    {
      Message = message;
      Progress = progress;
    }

    public string Message { get; }
    public int Progress { get; }
  }
}