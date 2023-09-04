namespace BionicCode.Utilities.Net
{
  using System.Text.Json.Serialization;

  internal class ChartAxis
  {
    [JsonPropertyName("title")]
    public string Title { get; set; }

    [JsonIgnore]
    public int AxisIndex { get; set; }
  }
}