#region Info

// //  
// BionicCode.BionicUtilities.Net.Standard

#endregion

namespace BionicCode.Utilities.Net.Standard.ViewModel
{
  public class PropertyValueChangedArgs<TValue>
  {
    public PropertyValueChangedArgs(string propertyName, TValue oldValue, TValue newValue)
    {
      this.PropertyName = propertyName;
      this.OldValue = oldValue;
      this.NewValue = newValue;
    }

    public string PropertyName { get; }
    public TValue OldValue { get; }
    public TValue NewValue { get; }
  }
}