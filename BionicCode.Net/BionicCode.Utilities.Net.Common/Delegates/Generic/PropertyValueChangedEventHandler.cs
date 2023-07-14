namespace BionicCode.Utilities.Net
{
  /// <summary>
  /// PropertyChanged event handler that supports standard property changed signature events with additional old value and new value parameters.
  /// </summary>
  /// <typeparam name="TValue"></typeparam>
  public delegate void PropertyValueChangedEventHandler<TValue>(
    object sender,
    PropertyValueChangedArgs<TValue> propertyChangedArgs);
}
