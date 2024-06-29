namespace BionicCode.Utilities.Net
{
  using System;

  /// <summary>
  /// Event args for the <see cref="ProgressChangedEventHandler"/>.
  /// </summary>
  public class ProgressChangedEventArgs : EventArgs
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    public ProgressChangedEventArgs() : this(-1, -1, string.Empty)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="oldValue">The old progress value before the change.</param>
    /// <param name="newValue">The new progress value after the change.</param>
    public ProgressChangedEventArgs(double oldValue, double newValue) : this(oldValue, newValue, string.Empty)
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="oldValue">The old progress value before the change.</param>
    /// <param name="newValue">The new progress value after the change.</param>
    /// <param name="progressText">A text message to summarize the progress.</param>
    public ProgressChangedEventArgs(double oldValue, double newValue, string progressText)
    {
      this.OldValue = oldValue;
      this.NewValue = newValue;
      this.ProgressText = progressText;
    }

    /// <summary>
    /// The old progress value before the change.
    /// </summary>
    public double OldValue { get; }
    /// <summary>
    /// The new progress value after the change.
    /// </summary>
    public double NewValue { get; }
    /// <summary>
    /// A text message to summarize the progress.
    /// </summary>
    public string ProgressText { get; }
    /// <summary>
    /// Indicates that the progress is indeterminate what would characterize the progress values of <see cref="OldValue"/> and <see cref="NewValue"/> just random progress e.g. bytes transferred instead of an abslote value of a fixed value range.
    /// </summary>
    public bool IsIndeterminate { get; }
  }
}
