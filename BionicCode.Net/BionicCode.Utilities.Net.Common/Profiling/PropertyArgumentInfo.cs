namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;

  internal readonly struct PropertyArgumentInfo
  {
    public PropertyArgumentInfo(object value, object index, PropertyAccessor accessor, int argumentListIndex)
    {
      this.Value = value;
      this.Index = index;
      this.Accessor = accessor;
      this.ArgumentListIndex = argumentListIndex;
    }

    public object Value { get; }
    public object Index { get; }
    public PropertyAccessor Accessor { get; }
    public int ArgumentListIndex { get; }
  }
}