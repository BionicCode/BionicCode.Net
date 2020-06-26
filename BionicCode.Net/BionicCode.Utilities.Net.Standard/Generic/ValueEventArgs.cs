#region Info

// //  
// WpfTestRange.Main

#endregion

using System;

namespace BionicCode.Utilities.Net.Standard.Generic
{
  /// <summary>
  /// Generic EventArgs implementation that supports to store a value.
  /// </summary>
  /// <typeparam name="TValue">The type of the value.</typeparam>
  public class ValueEventArgs<TValue> : EventArgs
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="value">The value to send as event args.</param>
    public ValueEventArgs(TValue value)
    {
      this.Value = value;
    }

    /// <summary>
    /// The value to send as event args.
    /// </summary>
    public TValue Value { get; }
  }
}