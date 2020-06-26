#region Info

// //  
// WpfTestRange.Main

#endregion

using System;

namespace BionicCode.Utilities.Net.Standard.Generic
{
  /// <summary>
  /// Generic EventArgs implementation that supports value changed information
  /// by holding the old and the new value.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class ValueChangedEventArgs<TValue> : EventArgs
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="newValue">The new value that replaces the old value.</param>
    /// <param name="oldValue">The old value that was replaced by the new value.</param>
    public ValueChangedEventArgs(TValue newValue, TValue oldValue)
    {
      this.NewValue = newValue;
      this.OldValue = oldValue;
    }

    /// <summary>
    /// The new value after the change.
    /// </summary>
    public TValue NewValue { get; }
    /// <summary>
    /// The old value before the change.
    /// </summary>
    public TValue OldValue { get; }
  }
}