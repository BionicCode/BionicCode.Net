namespace BionicCode.Utilities.Net
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  /// <summary>
  /// PropertyChanged event handler that supports standard property changed signature events with additional old value and new value parameters.
  /// </summary>
  /// <typeparam name="TValue"></typeparam>
  public delegate void PropertyValueChangedEventHandler<TValue>(
    object sender,
    PropertyValueChangedArgs<TValue> propertyChangedArgs);

  public class ProgressChangedEventArgs : EventArgs
  {
    public ProgressChangedEventArgs() : this(-1, -1, string.Empty)
    {
    }

    public ProgressChangedEventArgs(double oldValue, double newValue) : this(oldValue, newValue, string.Empty)
    {
    }

    public ProgressChangedEventArgs(double oldValue, double newValue, string progressText)
    {
      this.OldValue = oldValue;
      this.NewValue = newValue;
      this.ProgressText = progressText;
    }

    public double OldValue { get; set; }
    public double NewValue { get; set; }
    public string ProgressText { get; set; }
    public bool IsIndeterminate { get; set; }
  }
}
