namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Text.Json.Serialization;

  internal partial class HtmlLogger
  {
    internal class NormalDistributionData<TData>
    {
      public NormalDistributionData(int index, IEnumerable<TData> values)
      {
        this.Index = index;
        this.Values = values;
      }

      [JsonIgnore]
      public int Index { get; }
      public IEnumerable<TData> Values { get; }
    }
  }
}