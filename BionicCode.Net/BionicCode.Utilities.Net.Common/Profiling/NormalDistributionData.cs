namespace BionicCode.Utilities.Net
{
  using System.Collections.Generic;
  using System.Linq;
  using System.Text.Json.Serialization;

  internal class NormalDistributionData<TData>
  {
    public NormalDistributionData(int index, IEnumerable<TData> values, double mean, double standardDeviation)
    {
      this.Index = index;
      this.Values = values;
      this.Mean = mean;
      this.StandardDeviation = standardDeviation;
    }

    public TData this[int index] => this.Values.ElementAt(index);

    [JsonIgnore]
    public int Index { get; }
    [JsonIgnore]
    public int Count => this.Values.Count();

    [JsonPropertyName("values")]
    public IEnumerable<TData> Values { get; }
    [JsonIgnore]
    public double Mean { get; }
    [JsonIgnore]
    public double StandardDeviation { get; }

    [JsonIgnore]
    public string Title { get; set; }
  }
}
