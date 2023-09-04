namespace BionicCode.Utilities.Net
{
  using System.Text.Json.Serialization;

  internal class LegendOptions
  {
    [JsonIgnore]
    internal const string PositionLeft = "left";
    [JsonIgnore]
    internal const string PositionTop = "top";
    [JsonIgnore]
    internal const string PositionRight = "right";
    [JsonIgnore]
    internal const string PositionBottom = "bottom";

    [JsonPropertyName("position")]
    public string Position { get; set; }
  }
}