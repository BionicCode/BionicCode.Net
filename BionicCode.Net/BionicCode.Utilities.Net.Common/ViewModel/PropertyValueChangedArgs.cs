#region Info

// //  
// BionicCode.BionicUtilities.Net.Standard

#endregion

namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// Event args fro the <see cref="PropertyValueChangedEventHandler{TValue}"/> event delegate.
  /// </summary>
  /// <typeparam name="TValue"></typeparam>
  public class PropertyValueChangedArgs<TValue>
  {
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="propertyName">The name of the changed property.</param>
    /// <param name="oldValue">The value before the change.</param>
    /// <param name="newValue">The value that caused the change.</param>
    public PropertyValueChangedArgs(string propertyName, TValue oldValue, TValue newValue)
    {
      this.PropertyName = propertyName;
      this.OldValue = oldValue;
      this.NewValue = newValue;
    }

    /// <summary>
    /// Read-only property holding the property's name.
    /// </summary>
    /// <value>The name of the changed property.</value>
    public string PropertyName { get; }
    /// <summary>
    /// Read-only property holding the value before the change.
    /// </summary>
    /// <value>The value before the change.</value>
    public TValue OldValue { get; }
    /// <summary>
    /// Read-only property holding the value after the change.
    /// </summary>
    /// <value>The value after the change.</value>
    public TValue NewValue { get; }
  }
}