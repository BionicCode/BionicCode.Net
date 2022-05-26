namespace BionicCode.Utilities.Net
{
  using System;

  public readonly struct ProgressData
  {
    /// <summary>
    /// Data model to report progress to a implementation of <see cref="IProgressReporter"/>. When using the <see cref="IProgress{T}"/> returned from the <see cref="IProgressReporter.CreateProgressReporterFromCurrentThread"/> method, the <see cref="ProgressData"/> serves as the argument.
    /// </summary>
    /// <param name="message">A progress message.</param>
    /// <param name="progress">The progress value.</param>
    public ProgressData(string message, double progress)
    {
      this.Message = message;
      this.Progress = progress;
    }

    /// <summary>
    /// The progress message text.
    /// </summary>
    public string Message { get; }
    /// <summary>
    /// The progress value.
    /// </summary>
    public double Progress { get; }
  }
}